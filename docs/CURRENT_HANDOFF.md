# Current Project Handoff

작성 기준: **2026-09-13**

> 이 문서의 최신 섹션이 현재 상태 판단의 최우선 기준이다.
> 2026-09-06 시점의 상세 historical handoff 원문은
> `docs/archive/CURRENT_HANDOFF_2026-09-06.md`에 보존한다.

## 0. 현재 Branch / Remote / Local 구분

현재 작업 branch:

`feat/gojo-blue-screen-distortion`

2026-09-13 인수인계 동기화 직전 최신 **production-code checkpoint**:

`142606ea3fe7a7298bb2ea1c54bb80efcaf170c6`
`feat: finalize Gojo P0-P6 presentation checkpoint`

매우 중요:
- 위 checkpoint 이후 **Combat Data-driven migration + Inspector 한글화 작업은 사용자 PC LOCAL working tree에서 진행됨**.
- 해당 구조 작업은 현재 사용자 수동 검증 및 Codex 검증까지 끝났지만, 이 handoff 갱신 시점에는 아직 별도 commit/push 전이다.
- 따라서 새 채팅/에이전트는 GitHub에 Data Profile 구현이 이미 올라왔다고 가정하지 말고 먼저 `git status -sb`와 실제 diff를 확인한다.
- REMOTE / LOCAL / USER VERIFIED / CODEX VALIDATED를 반드시 구분한다.

## 1. 게임 한 문장 방향

**팬텀 퍼레이드의 기술 연출과 주술회전 원작의 술식/영역/패시브 규칙을 Unity 6 URP 3D로 최대한 고증하고, Strikeborn급 이상의 타격감과 연출 밀도를 지향하는 넓은 도시형 1:1 캐릭터 액션 격투게임.**

중요:
- Gojo는 첫 production-quality 캐릭터일 뿐 고정 주인공이 아니다.
- VFXLab도 Gojo 전용 scene이 아니라 장기적으로 공용 Character Presentation Lab이다.
- 공통 시스템은 Character name hardcoding보다 Trait / Passive / Domain rule / Data Profile을 우선한다.

상세:
- `docs/design/GAME_VISION.md`
- `docs/design/VFX_DIRECTION.md`
- `docs/architecture/COMBAT_DATA_DRIVEN.md`

## 2. 2026-09-13 현재 공식 상태

### USER VERIFIED — 보호 대상

- Gojo movement
  - Walk Speed **3**
  - Run Speed **14**
  - Blend Tree: Idle 0 @1.00 / Walk 3 @1.15 / Run 14 @1.00
  - 일반 이동 Walk / Shift Run / Space Dodge
- P0 Unlimited Void physical gameplay fix
  - Domain ACTIVE 내부 이동 정상
  - basic attack 1 / 2 / finisher 정상
  - 실제 hit / damage 정상
  - Space dodge 정상
  - Domain은 ACTIVE 상태 유지
  - Blue / Red / Purple / Domain technique는 ACTIVE 동안 lock 유지
  - 자연 종료 뒤 basic attack / dodge 정상 복구
- Gojo Blue 1차 baseline
  - 4-hit gameplay 포함
  - singularity / attraction / core / debris 기본 방향 보호
- Unlimited Void blue-black nebula background 1차 완성
- Hollow Purple formation / fusion baseline
- Gojo model/animation baseline
  - `Gojo_Blender_MasterAvatar` 기반 Humanoid pipeline
  - Idle / Walk / Run 사용자 검증 완료

### LOCAL USER VERIFIED — Combat Data-driven migration

사용자가 Unity에서 직접 확인:
- Data Asset의 damage 값 변경이 실제 runtime damage에 반영됨
- 이동 정상
- basic attack 정상
- Blue / Red / Purple 정상
- Domain 정상

따라서 **Combat Data-driven migration은 USER VERIFIED**로 본다.

현재 LOCAL에서 구성된 핵심 Profile / Definition:
- `CharacterStatsProfile`
- `CursedEnergyProfile`
- `BasicAttackProfile` + variable `BasicAttackStep[]`
- `GojoTechniqueGameplayProfile`
- `DomainGameplayProfile`
- `BurnoutPolicyProfile`
- `TargetingProfile`
- `TrainingBotProfile`
- `CharacterCombatDefinition`
- `CharacterCombatCatalog`
- Trait / Passive 확장 seam

Gojo Data Asset 위치:
`unity/Assets/Characters/Gojo/Data/`

주요 asset:
- `Gojo Character Stats.asset`
- `Gojo Cursed Energy Profile.asset`
- `Gojo Basic Attack Profile.asset`
- `Gojo Technique Gameplay Profile.asset`
- `Gojo Unlimited Void Gameplay.asset`
- `Gojo Burnout Policy.asset`
- `Gojo Targeting Profile.asset`
- `Gojo Combat Definition.asset`

공용 LOCAL asset:
- `Assets/Characters/Common/Data/Training Bot Normal Profile.asset`
- `Assets/Characters/Common/Data/Training Bot Domain Amplification Profile.asset`
- `Assets/Resources/CombatData/Character Combat Catalog.asset`

Runtime 연결 방향:
- `Health` → Character Stats
- `BasicAttack` → BasicAttackProfile / AttackStep[]
- `CursedEnergyController` → CursedEnergyProfile
- `GojoTechniqueController` → Blue / Red gameplay data
- `GojoTechniqueChainController` → Purple / Blue→Red synergy
- `GojoDomainController` → Domain gameplay data
- `TechniqueBurnoutController` → Burnout policy
- `TargetLockController` → Targeting profile
- `CurseBotController` → TrainingBotProfile
- `PrototypeCharacterController` → Catalog → CharacterCombatDefinition → individual profiles

LOCAL migration 검증:
- Unity compile error: **0**
- data smoke test: **3/3**
- Korean Inspector label focused test 당시: **1/1**
- full Unity EditMode regression: **50/50**
- `git diff --check`: migration 변경 파일 이상 없음

### CODEX VALIDATED / USER VISUAL CHECK PENDING — Inspector 완전 한글화

Data migration 후 남아 있던 영어 field label을 기존 scoped Editor/Drawer 방식으로 한글화함.

Codex 보고:
- 10개 신규 Data Profile용 scoped `CustomEditor`
- `BasicAttackStep`, Blue/Red/Purple, Blue→Red synergy `PropertyDrawer`
- `cooldown`은 평타 문맥에서 `입력 간격`
- Pull/Push는 실제 velocity 의미에 맞춰 `끌어당김 속도` / `밀어내기 속도`
- C# identifier / serialization key / asset filename은 영어 유지
- localization focused tests: **24/24**
- 실제 13개 asset `Editor.CreateEditor` + `SerializedObject` 검증
- 대상 13개 `.asset` SHA-256 전후 불일치: **0**
- runtime / Scene / VFX / Audio 변경 없음
- compile error: **0**

아직 사용자가 한글화 완료 Inspector를 직접 최종 확인했다는 판정은 기록되지 않았으므로
**LOCAL CODEX VALIDATED / USER VISUAL CHECK PENDING**으로 둔다.

## 3. 이번 구조화로 제거한 주요 Hardcoding

LOCAL migration에서 제거/일반화한 항목:
- `chainIndex >= 2` 기반 basic attack 3타 고정 전제
- `ATTACK CHAIN n / 3` 고정 표시
- Gojo controller들의 반복적인 Six Eyes CE profile 강제 적용
- `CurseBotController`의 `name.Contains("_B")` gameplay ability 결정
- HUD `BONUS +12` 중복 숫자
- Red spawn gameplay 값의 production constant 직접 의존

핵심 원칙:
**코드는 HOW / Data Asset은 WHAT.**

밸런스나 캐릭터별 설정을 바꾸기 위해 C#을 열어야 한다면 먼저 Data ownership을 의심한다.

## 4. 영구 Inspector / Data 작성 규칙

매우 중요. 이후 새 Profile / ScriptableObject / 사용자 설정 UI를 만들 때 처음부터 적용한다.

- C# class / field / enum identifier: **영어 유지**
- Unity Inspector / Data Asset / 사용자가 직접 만지는 설정 UI: **한국어 표시가 기본**
- Header만 한국어이고 실제 field label이 영어라면 **미완성**으로 본다.
- 새 Data Profile을 만들 때 한글 Inspector를 "나중에" 붙이지 말고 생성 시점부터 함께 만든다.
- 일반 serialized field label을 `InspectorNameAttribute`만으로 해결하려 하지 않는다.
- 기존 `SerializedProperty` + scoped `CustomEditor` / `PropertyDrawer` + Korean `GUIContent` 흐름을 우선한다.
- Undo / prefab override / object picker / enum / array / foldout / Range / Min 등 Unity 기본 편집 기능을 보존한다.
- 사용자가 자주 조절할 값은 Data/Inspector로, 알고리즘 내부 epsilon/수학 상수는 코드에 둘 수 있다.

## 5. Hardcoding 분류 규칙

모든 발견 항목은 이유 없이 방치하지 않고 다음 중 하나로 분류한다.

A. `MIGRATE NOW`
- 캐릭터/기술/봇/밸런스 등 사용자가 조절할 gameplay data

B. `ASTRA / PRESENTATION 이후 MIGRATE`
- camera shake / FOV / hit-stop / flash / technique presentation color
- Infinity ripple presentation
- Target Lock indicator presentation
- VFX presentation tuning

C. `KEEP IN CODE`
- epsilon
- clamp
- state transition mechanics
- collection/event plumbing
- 실제 알고리즘 내부 상수

LOCAL audit 문서가 존재할 수 있음:
`docs/COMBAT_HARDCODING_AUDIT.md`
이 파일도 아직 remote에 있다고 가정하지 않는다.

남은 대표 debt:
- `worldDeathY` → 향후 `ArenaRules`
- Presentation feedback → Astra polish 후 `TechniquePresentationProfile` / `CombatCameraProfile`
- 비이관 Sukuna/Megumi CE legacy fallback
- prototype 캐릭터별 component enable/disable switch
- gameplay가 아닌 roster/team sorting의 `_B` convention

## 6. Domain 아키텍처 — 절대 합치지 않음

세 개를 독립적으로 유지:
1. **Gameplay Capture Radius** — 누가 포획되는가
2. **Visual Barrier Radius / Diameter** — 원래 세계의 검은 구형 결계 크기
3. **Domain Interior Space** — 별도 pocket-dimension 내부 공간 크기

Barrier 크기를 바꿔도 capture 판정이나 interior 크기가 자동으로 바뀌면 안 된다.

의도 흐름:
`Original World → cast/capture → black barrier → transition/cinematic → purple tunnel → isolated Unlimited Void interior → gameplay → safe return`

현재 barrier HP / destruction / outside rescue / simultaneous inside-outside combat은 범위 밖.

상세:
- `docs/architecture/DOMAIN_SYSTEM.md`
- `docs/architecture/CAMERA_PRESENTATION.md`

## 7. 현재 VFX / Presentation 방향

품질 기준:
**원작 정체성 + Strikeborn급 이상의 impact density / environment reaction / aftermath.**

기술을 `spawn → travel → boom → gone`으로 끝내지 않는다.

현재 다음 Astra visual track 후보:

### Blue
1차 baseline은 보호하면서 2차 enhancement 허용:
- environment inward reaction
- 4-hit별 더 읽히는 beat
- final collapse peak
- restrained distortion/camera
- residual spatial shimmer / dust pull

### Red
Repulsion identity 유지:
- compressed anticipation
- violent release
- pressure-reactive travel
- impact flash
- irregular repulsive shockwave
- outward debris/dust
- gray-white pressure vapor
- aftermath

불꽃 기술처럼 만들지 않는다.

### Hollow Purple
보호:
- formation / fusion
- 현재 hold baseline

남은 실제 문제:
- 발사 시 약간 아래 방향으로 나가는 launch-axis 문제
- 실제 anchor/release vector 원인을 수정해야 하며 눈속임 lateral offset으로 덮지 않는다.

2차 enhancement:
- release compression / flash
- travel distortion
- branching violet lightning
- environment reaction
- scar / residual electricity / spatial shimmer

### Unlimited Void
보호:
- blue-black nebula background
- capture/barrier/interior/restoration architecture

남은 visual:
- Cosmic Eye 전체 scale/intensity heartbeat/breathing pulse 제거
- cloud/rim/tail subtle flow는 유지
- release 중 ceiling/dome/shell처럼 읽히는 artifact 실제 원인 제거
- caster-centered world-space release 유지
- White Blood는 `퉁 → pause → 투둥 → pause → 퉁` 3-group beat로 확장하며 timing은 Data/Inspector 조절 가능하게 유지

상세:
`docs/design/VFX_DIRECTION.md`

## 8. Gojo 모델 / Animation 지속 규칙

현재 기준 rig/model pipeline:
- actual model: `Gojo_Blender_Master`
- Avatar: `Gojo_Blender_MasterAvatar`
- Animator: `Gojo_Animator`
- Apply Root Motion: OFF
- 동일 Blender Armature에서 export한 animation FBX는
  `Humanoid → Copy From Other Avatar → Gojo_Blender_MasterAvatar`
- 기존 `Gojo_Blindfold_RiggedAvatar`와 Blender-export animation FBX를 섞지 않는다.

Local/copyright-sensitive asset 정책:
- `unity/Assets/LocalModels/`
- `unity/Assets/Resources/LocalAudio/`
- root `사운드 모음/`
- `.blend`, local FBX/texture/audio

위 자산은 명시적 승인 없이 공개 Git에 올리거나 덮어쓰지 않는다.

## 9. 다음 안전한 작업 순서

현재 LOCAL Data migration은 이미 사용자 gameplay 검증 완료.
Inspector localization은 Codex 24/24 + asset hash 불변 확인 완료이며 사용자 최종 시각 확인만 남아 있음.

권장 다음 순서:
1. 사용자가 새 Data Asset Inspector 한글 표기를 직접 확인
2. `git status -sb`로 LOCAL dirty tree 재확인
3. Data migration + localization 관련 파일만 **selective staging**
4. `git diff --cached --name-status` 검토
5. 구조 checkpoint commit / push
6. 그 뒤 Astra visual track 시작

절대 `git add .`로 현재 LOCAL Scene/VFX/Audio/Model 작업을 섞지 않는다.
reset / clean / checkout으로 사용자 로컬 작업을 제거하지 않는다.

## 10. 문서 우선순위

새 채팅은 다음 순서로 읽는다.

1. `AGENTS.md`
2. `docs/design/GAME_VISION.md`
3. `docs/CURRENT_HANDOFF.md`
4. `docs/active/CURRENT_TASK.md`
5. `docs/locked/USER_VERIFIED_SETTINGS.md`
6. 작업별 architecture 문서

`docs/DEVELOPMENT_ROADMAP.md`와 오래된 Gate/Pass 문서는 historical context로 참고할 수 있으나,
현재 상태가 충돌하면 위 최신 문서가 우선한다.
