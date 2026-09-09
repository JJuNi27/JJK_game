# 비주얼 레퍼런스 작업공간

이 폴더는 **시각 방향을 Codex에게 전달하기 위한 로컬 참고자료**용이다.

예:
- 애니메이션 스크린샷
- 게임 스크린샷
- 사용자가 만든 컨셉 이미지
- 생성형 AI 컨셉 이미지
- 짧은 참고 영상

## 중요
이 파일들은 레퍼런스이며 Unity 런타임 Asset이 아니다.
명시적 요청이 없으면 `Assets/`로 복사하거나 runtime dependency를 만들지 않는다.

대부분의 binary reference media는 Git에서 의도적으로 무시한다.

즉:
- 폴더 구조와 설명 문서는 GitHub에 남음
- 사용자는 `git pull` 뒤 로컬 폴더에 참고 이미지를 넣을 수 있음
- Codex는 로컬 파일을 직접 확인할 수 있음
- 임시 screenshot/video가 GitHub에 계속 쌓이지 않음

Git에 공유해야 하는 reference가 생기면 그때 명시적으로 결정한다.

## 파일명 규칙
권장:
`<subject>_<purpose>_ref_<nn>.<ext>`

예:
- `cosmic_eye_ref_01.png`
- `interior_anime_ref_01.png`
- `red_impact_ref_01.mp4`
- `gojo_melee_motion_ref_02.mp4`

## Workflow
1. 사용자가 비주얼 피드백을 전달
2. ChatGPT가 승인된 내용을 `docs/active/CURRENT_TASK.md`로 정리
3. 텍스트만으로 부족하면 CURRENT_TASK에 reference path 지정
4. 사용자가 pull 후 로컬 reference media 배치
5. Codex가 구현 전에 해당 reference 직접 확인
6. 관련 없는 전경 피사체를 복제하지 않고 시각 언어만 참고
