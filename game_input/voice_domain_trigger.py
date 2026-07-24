from __future__ import annotations

import json
import queue
import threading
import time
from pathlib import Path

from game.domain_controller import DomainController


DEFAULT_MODEL_PATH = Path("models/vosk-model-small-ko-0.22")


class VoiceDomainTrigger:
    """마이크에서 영역전개 선언을 감지해 Controller에 전달한다.

    음성인식 의존성이나 한국어 모델이 없어도 프로그램 전체가 실패하지 않도록
    선택 기능으로 동작한다. 음성 스레드는 명령을 큐에 넣고, Pygame 메인
    스레드가 update()에서 이를 소비한다.
    """

    def __init__(self, model_path: Path | str = DEFAULT_MODEL_PATH) -> None:
        self.model_path = Path(model_path)
        self.status_message = "음성인식: 준비 확인 중"
        self.is_available = False

        self._audio_queue: queue.Queue[bytes] = queue.Queue()
        self._command_queue: queue.Queue[str] = queue.Queue()
        self._stop_event = threading.Event()
        self._thread: threading.Thread | None = None

        self._accepted_phrases = {
            "영역전개",
            "료이키텐카이",
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
                "음성인식: 라이브러리 없음 (V 키는 정상 사용 가능)"
            )
            return

        if not self.model_path.is_dir():
            self.status_message = (
                "음성인식: 한국어 모델 없음 (setup_voice_model.py 실행 필요)"
            )
            return

        self._stop_event.clear()
        self.status_message = "음성인식: 마이크 시작 중"
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

    def _contains_activation_phrase(self, text: str) -> bool:
        normalized = self._normalize(text)
        return any(phrase in normalized for phrase in self._accepted_phrases)

    def _listen_loop(self) -> None:
        """백그라운드에서 마이크 스트림을 Vosk로 처리한다."""
        try:
            import sounddevice as sd
            from vosk import KaldiRecognizer, Model, SetLogLevel

            SetLogLevel(-1)
            model = Model(str(self.model_path))

            input_device = sd.query_devices(kind="input")
            sample_rate = int(input_device["default_samplerate"])

            # 고정 문구 중심으로 판정한다. 띄어쓰기 유무를 모두 포함한다.
            grammar = json.dumps(
                [
                    "영역 전개",
                    "영역전개",
                    "료이키 텐카이",
                    "료이키텐카이",
                    "[unk]",
                ],
                ensure_ascii=False,
            )
            recognizer = KaldiRecognizer(model, sample_rate, grammar)

            def audio_callback(indata, frames, time_info, status) -> None:  # type: ignore[no-untyped-def]
                del frames, time_info
                if status:
                    # 순간적인 오디오 경고로 게임을 종료하지는 않는다.
                    pass
                self._audio_queue.put(bytes(indata))

            last_triggered_at = 0.0
            self.is_available = True
            self.status_message = '음성인식: 듣는 중 ("영역전개")'

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

                    if recognizer.AcceptWaveform(data):
                        result = json.loads(recognizer.Result()).get("text", "")
                    else:
                        result = json.loads(recognizer.PartialResult()).get(
                            "partial", ""
                        )

                    now = time.monotonic()
                    if (
                        self._contains_activation_phrase(result)
                        and now - last_triggered_at >= 1.5
                    ):
                        self._command_queue.put("request_domain")
                        last_triggered_at = now
                        recognizer.Reset()

        except Exception as exc:  # 음성 기능 실패가 게임 전체 실패로 번지지 않게 한다.
            self.status_message = f"음성인식 사용 불가: {type(exc).__name__}"
        finally:
            self.is_available = False
