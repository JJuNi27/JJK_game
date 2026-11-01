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

## PASS4H — 공통 흑섬 로컬 영상 (2026-11-01)

PASS4H.0 구현 단계에서 저장소 공통 `.gitignore`에 `_local_refs/`를 추가했다. 아래 최초 분석 당시의 로컬 exclude 설명은 당시 상태 기록이며, 현재는 공유 규칙도 존재한다. 영상 원본은 계속 로컬 전용이고 Git/Unity Assets에 넣지 않는다. 구현·검증은 `../SHIBUYA_PASS4H0_REVIEW.md`를 따른다.

실제 확인한 위치는 이 폴더 안이 아니라 **프로젝트 루트의 `_local_refs/videos/blackflash_refs/`**다. 절대 경로는 `D:\JJK_game\_local_refs\videos\blackflash_refs\`다. 아래 경로는 프로젝트 상대 경로다.

| 파일 | 파일별 용도 |
|---|---|
| `_local_refs/videos/blackflash_refs/로블_흑섬.mp4` |20.467초/30fps. 3D 전장 내 Manji 흑섬·피니셔의 검은 질량/붉은 방전/짧은 화면 변화/잔류. 주황 counter·청록 선행 기술·상태 연출은 보조 참고로 분리.|
| `_local_refs/videos/blackflash_refs/주팬퍼_흑섬.mp4` |113.733초/30fps. 유지·나나미·고죠 및 말미 장발 캐릭터의 검붉은 잉크 family와 타격별 방향·규모·컷인 차이. 긴 대사 암전을 일반 흑섬 pulse로 복제하지 않음.|

두 영상을 실제 디코드해 개요 및 인접 프레임으로 분석했다. 기록된 시간은 녹화 PTS이며 원 게임 배속/hit-stop 시간을 확정한 값이 아니다. 오디오 존재와 청취 분석은 구분한다. 관찰된 사실·캐릭터 식별 추론·사용자 확정 규칙·새 구현 제안은 [흑섬 VFX 설계](../design/BLACK_FLASH_VFX.md)에 구분했다.

원본은 읽기 전용으로 보존하고 Unity Assets로 이동/복제하거나 Git에 추가하지 않는다. 현재 로컬 `.git/info/exclude`의 `_local_refs/` 규칙이 적용되며 추적 파일0개다. 해당 규칙은 다른 clone에 공유되지 않으므로 사용 전 `git check-ignore -v -- <원본 경로>`와 `git ls-files -- _local_refs/videos/blackflash_refs/`로 다시 확인한다. 이번 문서 작업에서 공유 `.gitignore`나 로컬 exclude 설정을 수정하지 않았다.

파생 분석용 이미지·ffprobe·프레임 구간·원본 해시 기록은 `unity/Logs/ShibuyaPass4HReferenceReview/20261101/`에만 둔다. 원본 변환/덮어쓰기/삭제를 하지 않으며 분석용 시트도 커밋하지 않는다. 레퍼런스의 asset/code를 추출하지 않고 연출 타이밍·색·형태·깊이의 관계만 참고한다.

확정 방향은 강한 충격광, 검은 주력핵, 굵은 붉은 분기, 공간 압력/왜곡, 검은 연기·붉은 잔광이다. 일반 유효 흑섬마다 짧은 화면 변화, 특별 흑섬은 더 강한 Presentation 등급, 연속 발생은 월드 impact를 유지하며 화면 중첩을 제한하는 방향이다. 최종 수치·Zone 외형·기술 선택과 실제 VFX 구현은 별도 PASS 승인 대상이다.
