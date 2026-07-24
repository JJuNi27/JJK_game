from __future__ import annotations

import json
import queue
import threading
import time
from pathlib import Path

from game.domain_controller import DomainController


DEFAULT_MODEL_PATH = Path("models/vosk-model-small-ja-0.22")
MIN_COMMAND_CONFIDENCE = 0.88


class VoiceDomainTrigger:
    """마이크에서 일본어 영역전개 선언을 감지한다.

    프로젝트 분위기에 맞춰 일본어 「領域展開」(료이키 텐카이)를 주 명령으로
    사용한다. 확정된 인식 결과가 허용 문구와 정확히 일치하고 신뢰도 기준까지
    통과했을 때만 Controller에 영역 준비 요청을 전달한다.

    음성인식 의존성, 일본어 모델 또는 마이크가 없어도 프로그램 전체는 계속
    실행되며 V 키 대체 입력을 사용할 수 있다.
    """

    def __init__(
        self,
        model_path: Path | str = DEFAULT_MODEL_PATH,
        min_confidence: float = MIN_COMMAND_CONFIDENCE,
    ) -> None:
        self.model_path = Path(model_path)
        self.min_confidence = min_confidence
        self.status_message = "일본어 음성인식: 준비 확인 중"
        self.is_available = False
        self.last_recognized_text = ""
        self.last_confidence = 0.0

        self._audio_queue: queue.Queue[bytes] = queue.Queue()
        self._command_queue: queue.Queue[str] = queue.Queue()
        self._stop_event = threading.Event()
        self._thread: threading.Thread | None = None

        # 일본어 모델이 출력할 수 있는 표기 차이를 모두 정규화해 허용한다.
        self._accepted_phrases = {
            "領域展開",
            "りょういきてんかい",
            "リョウイキテンカイ",
        }

    def start(self) -> None:
        """필요 파일을 확인하고 음성인식 스레드를 시작한다."""
        if self._thread is not None and self._thread.is_alive():
            return

        try:
            import sounddevice  # noqa: F401
            import vosk  # noqa: F401
        except ImportError:
            self.status_message = (
                "일본어 음성인식: 라이브러리 없음 (V 키는 정상 사용 가능)"
            )
            return

        if not self.model_path.is_dir():
            self.status_message = (
                "일본어 음성인식: 모델 없음 (setup_voice_model.py 실행 필요)"
            )
            return

        self._stop_event.clear()
        self.status_message = "일본어 음성인식: 마이크 시작 중"
        self._thread = threading.Thread(
            target=self._listen_loop,
            name="voice-domain-trigger",
            daemon=True,
        )
        self._thread.start()

    def update(self, controller: DomainController) -> None:
        """메인 스레드에서 인식된 명령을 Controller에 전달한다."""
        while True:
            try:
                command = self._command_queue.get_nowait()
            except queue.Empty:
                break

            if command == "request_domain":
                controller.request_domain()

    def stop(self) -> None:
        """음성인식 스레드 종료를 요청한다."""
        self._stop_event.set()
        if self._thread is not None and self._thread.is_alive():
            self._thread.join(timeout=1.0)

    @staticmethod
    def _normalize(text: str) -> str:
        return "".join(text.lower().split())

    def _is_exact_activation_phrase(self, text: str) -> bool:
        return self._normalize(text) in self._accepted_phrases

    @staticmethod
    def _extract_confidence(payload: dict[str, object]) -> float:
        """Vosk의 단어별 confidence 평균을 0~1 값으로 반환한다."""
        words = payload.get("result")
        if not isinstance(words, list) or not words:
            return 0.0

        confidences: list[float] = []
        for word in words:
            if not isinstance(word, dict):
                continue
            value = word.get("conf")
            if isinstance(value, (int, float)):
                confidences.append(float(value))

        if not confidences:
            return 0.0
        return sum(confidences) / len(confidences)

    def _handle_final_result(
        self,
        payload: dict[str, object],
        last_triggered_at: float,
    ) -> float:
        """확정 결과를 검사하고, 발동했다면 최신 발동 시각을 반환한다."""
        raw_text = payload.get("text", "")
        text = raw_text.strip() if isinstance(raw_text, str) else ""
        if not text:
            return last_triggered_at

        confidence = self._extract_confidence(payload)
        self.last_recognized_text = text
        self.last_confidence = confidence

        now = time.monotonic()
        is_exact_phrase = self._is_exact_activation_phrase(text)
        is_confident = confidence >= self.min_confidence
        cooldown_finished = now - last_triggered_at >= 1.5

        if is_exact_phrase and is_confident and cooldown_finished:
            self._command_queue.put("request_domain")
            self.status_message = (
                f'음성 명령 인식: "{text}" · 신뢰도 {confidence:.0%}'
            )
            return now

        self.status_message = (
            f'명령 아님: "{text}" · 신뢰도 {confidence:.0%}'
        )
        return last_triggered_at

    def _listen_loop(self) -> None:
        """백그라운드에서 마이크 스트림을 일본어 Vosk로 처리한다."""
        try:
            import sounddevice as sd
            from vosk import KaldiRecognizer, Model, SetLogLevel

            SetLogLevel(-1)
            model = Model(str(self.model_path))

            input_device = sd.query_devices(kind="input")
            sample_rate = int(input_device["default_samplerate"])

            # 일본어 표기의 후보를 제한해 짧은 게임 명령의 인식률을 높인다.
            # 실제 발동은 확정 결과의 정확한 일치와 신뢰도 기준으로 다시 검사한다.
            grammar = json.dumps(
                [
                    "領域 展開",
                    "りょういき てんかい",
                    "リョウイキ テンカイ",
                    "[unk]",
                ],
                ensure_ascii=False,
            )
            recognizer = KaldiRecognizer(model, sample_rate, grammar)
            recognizer.SetWords(True)

            def audio_callback(indata, frames, time_info, status) -> None:  # type: ignore[no-untyped-def]
                del frames, time_info
                if status:
                    # 순간적인 오디오 경고로 게임을 종료하지는 않는다.
                    pass
                self._audio_queue.put(bytes(indata))

            last_triggered_at = 0.0
            self.is_available = True
            self.status_message = (
                f'일본어 음성인식: 듣는 중 ("료이키 텐카이", 최소 {self.min_confidence:.0%})'
            )

            with sd.RawInputStream(
                samplerate=sample_rate,
                blocksize=8000,
                device=None,
                dtype="int16",
                channels=1,
                callback=audio_callback,
            ):
                while not self._stop_event.is_set():
                    try:
                        data = self._audio_queue.get(timeout=0.2)
                    except queue.Empty:
                        continue

                    # 말이 끝난 뒤 만들어지는 확정 결과만 판정한다.
                    if not recognizer.AcceptWaveform(data):
                        continue

                    payload = json.loads(recognizer.Result())
                    if not isinstance(payload, dict):
                        continue

                    last_triggered_at = self._handle_final_result(
                        payload,
                        last_triggered_at,
                    )

        except Exception as exc:  # 음성 기능 실패가 게임 전체 실패로 번지지 않게 한다.
            self.status_message = f"일본어 음성인식 사용 불가: {type(exc).__name__}"
        finally:
            self.is_available = False
