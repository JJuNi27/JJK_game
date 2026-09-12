# JJK_game — Codex 프로젝트 가이드

이 파일은 전체 명세서가 아니라 **짧은 프로젝트 지도**다.
현재 작업과 관련된 문서만 필요한 범위에서 읽는다.

## 기본 작업 원칙
- 수정 전에 실제 working tree를 먼저 확인한다.
- 광범위한 재작성보다 가장 작은 안전한 수정안을 우선한다.
- 관련 없는 로컬 변경사항을 보존한다.
- 사용자가 직접 만든 Scene / Animator / FBX 작업을 reset, checkout, clean 또는 덮어쓰기 하지 않는다.
- 사용자가 명시적으로 승인하지 않는 한 commit / push / merge / master 변경을 하지 않는다.
- 진행을 막지 않는 사소한 확인 때문에 작업을 멈추지 않는다. 파괴적 작업이거나 실제 설계 결정을 사용자가 해야 할 때만 질문한다.
- `USER VERIFIED` 값은 사용자가 직접 변경을 요청하지 않는 한 잠금값으로 취급한다.
- 작업 중에는 targeted validation을 우선하고, 전체 regression은 구현이 안정된 뒤 마지막에 한 번 수행한다.
- 주관적인 시각 품질을 절대 `USER VERIFIED`라고 표현하지 않는다. Unity Play Mode의 최종 시각 승인은 사용자만 할 수 있다.
- REMOTE / LOCAL / CODEX VALIDATED / USER VERIFIED를 명확히 구분한다.

## 문서 우선순위
현재 상태 복구 시 다음 순서로 읽는다.

1. `docs/CURRENT_HANDOFF.md`
2. `docs/active/CURRENT_TASK.md`
3. `docs/locked/USER_VERIFIED_SETTINGS.md`
4. 관련 architecture/design 문서
5. 오래된 Gate / roadmap 문서

오래된 문서와 최신 handoff가 충돌하면 최신 handoff / current task / locked settings가 우선한다.

## 게임 방향
읽기: `docs/design/GAME_VISION.md`

핵심:
- 1:1 중심 3D 액션 격투
- 넓은 JJK 도시형 전투 맵
- Gojo는 첫 캐릭터일 뿐 주인공 고정이 아님
- 팬텀 퍼레이드/애니/만화 고증을 최대한 살린 기술
- 원작 규칙을 Trait / Passive / Domain rule로 시스템화
- Data-driven / Inspector 튜닝 우선

## VFX 품질 방향
읽기: `docs/design/VFX_DIRECTION.md`

핵심:
- 원작 정체성 + Strikeborn급 이상의 연출 밀도/타격감
- 기술을 단순히 발사→폭발로 끝내지 않음
- impact / environment reaction / aftermath / camera/screen feedback까지 고려
- Strikeborn/JJS의 asset을 복사하는 것이 아니라 presentation philosophy만 참고

## 비주얼 방향
이 프로젝트는 게임이며 **강한 가독성, 화려함, 타격감**이 핵심 목표다.
효과 하나만 떼어봤을 때 "너무 화려한 것 같다"는 이유로 자동으로 약하게 만들지 않는다.
실제 플레이에서는 VFX가 예상보다 작고 약하게 느껴지는 경우가 많다.
안전성과 가독성이 유지된다면, 두 선택지 중에서는 더 강한 임팩트 / 아우라 / 타격감 / 깊이 / 움직임이 느껴지는 쪽을 우선한다.

## 프로젝트 우선순위
1. 시각적 임팩트 / 아우라
2. 타격감
3. 사운드
4. 프리미엄 VFX
5. 캐릭터별 원작 정체성
6. 유지보수성 및 Inspector/Data-driven 튜닝

## Combat Data / Inspector 튜닝
읽기: `docs/architecture/COMBAT_DATA_DRIVEN.md`

핵심 원칙:
**코드는 HOW, Data Asset은 WHAT.**

- 캐릭터/기술/밸런스 값을 바꾸기 위해 C#을 열어야 한다면 Data ownership을 먼저 의심한다.
- Character name / GameObject name string으로 gameplay rule을 결정하지 않는다.
- 사용자가 자주 조절할 gameplay 값은 Profile / Definition / Inspector에서 조절 가능하게 한다.
- 알고리즘 내부 epsilon / clamp / state-machine mechanics는 코드에 남길 수 있다.

### 사용자-facing 한글화 — 영구 규칙
매우 중요:
- C# class / field / enum identifier는 영어 유지.
- **사용자가 직접 만지는 Unity Inspector / Data Asset / 설정 UI는 한국어 표시가 기본.**
- 새 Profile / ScriptableObject를 만들 때 생성 시점부터 한글 Inspector를 같이 구현한다.
- Header만 한국어이고 실제 field label이 영어라면 미완성으로 본다.
- 일반 serialized field label을 `InspectorNameAttribute`만으로 해결하려 하지 않는다.
- 기존 `SerializedProperty` + scoped `CustomEditor` / `PropertyDrawer` + Korean `GUIContent` 흐름을 우선한다.
- Undo / prefab override / array / foldout / object picker / enum / Range / Min 등 Unity 기본 편집 기능을 깨뜨리지 않는다.

## Hardcoding 분류
하드코딩 audit 시 모든 발견 항목을 다음 중 하나로 분류한다.

A. `MIGRATE NOW`
- gameplay / balance / character-specific tuning

B. `ASTRA / PRESENTATION 이후 MIGRATE`
- VFX / camera / hit-stop / flash / focus / presentation tuning

C. `KEEP IN CODE`
- 실제 알고리즘 내부 상수와 mechanics

"이번에 migration하지 않음"과 "존재를 놓침"을 구분한다.

## VFXLab
VFXLab은 **Gojo 전용 scene이 아니다.**
현재는 첫 캐릭터가 Gojo일 뿐이며, 장기적으로 Character Select 기반 공용 presentation/test scene으로 확장한다.

## 보호된 고죠 기준값
읽기: `docs/locked/USER_VERIFIED_SETTINGS.md`

## 무량공처 / Domain 시스템을 수정할 때
읽기: `docs/architecture/DOMAIN_SYSTEM.md`

Gameplay Capture Radius / Visual Barrier Radius(or Diameter) / Domain Interior Space를 하나의 값으로 합치지 않는다.

## 카메라 / 시네마틱 소유권을 수정할 때
읽기: `docs/architecture/CAMERA_PRESENTATION.md`

## 고죠 Blue / Red / Purple을 수정할 때
읽기: `docs/architecture/GOJO_VFX.md`

## 사운드 / 음성 / 타격 beat 동기화를 수정할 때
읽기: `docs/architecture/AUDIO_PRESENTATION.md`

## Unity 검증 전에
읽기: `docs/workflows/UNITY_VALIDATION.md`

자동 테스트 / static validation은 사용자 Play Mode 시각 검증을 대체하지 않는다.

## 현재 작업
읽기: `docs/active/CURRENT_TASK.md`

## 비주얼 레퍼런스
텍스트만으로 설명하기 어려운 시각 방향을 전달하기 위한 이미지/영상은:
`docs/references/`

사용 전에 `docs/references/README.md`를 읽는다.

CURRENT_TASK에서 특정 레퍼런스를 지정했다면:
- 관련 VFX 구현 전에 그 파일을 직접 확인한다.
- 형태, 구도, 색, 움직임 언어, 깊이감, 분위기를 참고한다.
- 관련 없는 전경 캐릭터나 피사체를 무작정 복제하지 않는다.
- 명시적 요청이 없으면 레퍼런스 파일을 Unity 런타임 Asset으로 옮기지 않는다.

레퍼런스 미디어는 로컬 전용이며 Git에서 의도적으로 무시될 수 있다.

## 중요 Asset / Git 안전수칙
사용자의 명시적 요청이 없으면 LocalModels, LocalAudio, import된 FBX, texture, audio, `.blend` 파일을 추가하거나 다시 쓰지 않는다.
로컬 `VFXLab.unity` 변경을 버리지 않는다.

다음 명령/행동은 특별한 승인 없이 금지:
- `git reset --hard`
- `git clean`
- 로컬 작업을 덮는 checkout/restore
- dirty tree에서 `git add .`

구조 checkpoint가 필요할 때는 selective staging 후 `git diff --cached --name-status`를 검토한다.
