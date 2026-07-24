from __future__ import annotations

import shutil
import sys
import urllib.request
import zipfile
from pathlib import Path


MODEL_NAME = "vosk-model-small-ko-0.22"
MODEL_URL = f"https://alphacephei.com/vosk/models/{MODEL_NAME}.zip"
MODELS_DIR = Path("models")
TARGET_DIR = MODELS_DIR / MODEL_NAME
ARCHIVE_PATH = MODELS_DIR / f"{MODEL_NAME}.zip"


def report_progress(block_count: int, block_size: int, total_size: int) -> None:
    if total_size <= 0:
        return

    downloaded = min(block_count * block_size, total_size)
    percent = downloaded / total_size * 100
    print(
        f"\r다운로드 중: {percent:5.1f}% "
        f"({downloaded / 1024 / 1024:.1f}MB / {total_size / 1024 / 1024:.1f}MB)",
        end="",
        flush=True,
    )


def main() -> int:
    if TARGET_DIR.is_dir():
        print(f"이미 설치되어 있습니다: {TARGET_DIR}")
        return 0

    MODELS_DIR.mkdir(parents=True, exist_ok=True)

    try:
        print("Vosk 한국어 소형 모델을 내려받습니다.")
        print(f"출처: {MODEL_URL}")
        urllib.request.urlretrieve(
            MODEL_URL,
            ARCHIVE_PATH,
            reporthook=report_progress,
        )
        print("\n압축을 해제합니다...")

        with zipfile.ZipFile(ARCHIVE_PATH) as archive:
            archive.extractall(MODELS_DIR)

        if not TARGET_DIR.is_dir():
            print("오류: 압축 해제 후 모델 폴더를 찾지 못했습니다.")
            return 1

        print(f"설치 완료: {TARGET_DIR}")
        return 0

    except KeyboardInterrupt:
        print("\n사용자가 다운로드를 취소했습니다.")
        return 1
    except (OSError, urllib.error.URLError, zipfile.BadZipFile) as exc:
        print(f"\n모델 설치 실패: {exc}")
        return 1
    finally:
        if ARCHIVE_PATH.exists():
            try:
                ARCHIVE_PATH.unlink()
            except OSError:
                # 잠긴 파일이면 다음 실행에서 덮어쓸 수 있도록 그대로 둔다.
                pass

        # 실패 중 일부만 생성된 폴더는 다음 실행을 위해 제거한다.
        if TARGET_DIR.exists() and not TARGET_DIR.is_dir():
            try:
                if TARGET_DIR.is_file():
                    TARGET_DIR.unlink()
                else:
                    shutil.rmtree(TARGET_DIR)
            except OSError:
                pass


if __name__ == "__main__":
    sys.exit(main())
