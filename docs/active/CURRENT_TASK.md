# 현재 작업 — Combat Data-driven 구조 checkpoint 마무리

상태: **Combat Data-driven migration LOCAL USER VERIFIED / Inspector localization LOCAL USER VERIFIED / 구조 checkpoint push 전**

작성 기준: 2026-09-13

## 현재 즉시 목표

이번 단계의 목표는 새 기능 추가가 아니다.
이미 완료된 Data-driven migration + Inspector 한글화를 안전하게 구조 checkpoint로 묶는 것이다.

## 완료된 LOCAL 구조 작업

### Combat Data-driven migration — USER VERIFIED

사용자가 Unity에서 직접 확인:
- Data Asset damage 변경 → 실제 runtime damage 반영
- 이동 정상
- basic attack 정상
- Blue / Red / Purple 정상
- Domain 정상

주요 Profile / Definition:
- CharacterStatsProfile
- CursedEnergyProfile
- BasicAttackProfile + variable BasicAttackStep[]
- GojoTechniqueGameplayProfile
- DomainGameplayProfile
- BurnoutPolicyProfile
- TargetingProfile
- TrainingBotProfile
- CharacterCombatDefinition
- CharacterCombatCatalog
- Trait / Passive seam

제거/일반화한 주요 hardcoding:
- 3타 고정 basic chain 전제
- `ATTACK CHAIN n / 3` 고정 표시
- 반복 Six Eyes profile 강제 적용
- `name.Contains("_B")` gameplay ability convention
- `BONUS +12` 중복 숫자
- Red spawn gameplay constant 직접 의존

검증:
- Unity compile errors 0
- data smoke 3/3
- full EditMode regression 50/50
- 사용자 실제 gameplay 확인 완료

### Inspector localization — USER VERIFIED

목표:
C# identifier는 영어로 유지하면서 사용자가 만지는 Inspector/Data label을 한국어로 표시.

구현/검증:
- 10개 신규 Data Profile scoped CustomEditor
- BasicAttackStep / Blue / Red / Purple / Blue→Red PropertyDrawer
- localization focused tests 24/24
- 실제 13개 asset Editor/SerializedObject 검증
- 13개 `.asset` SHA-256 전후 불일치 0
- balance / reference / runtime / Scene / VFX / Audio 변경 없음
- compile errors 0
- 사용자가 실제 Unity Inspector에서 한글 표시 정상 확인

따라서 Inspector localization도 **LOCAL USER VERIFIED**.

## 영구 Inspector 규칙

앞으로 새 Profile / ScriptableObject 작성 시:
- 내부 C# identifier = 영어
- 사용자-facing Inspector / Data UI = 한국어
- Header만 한글이고 field label이 영어면 미완성
- 생성 시점부터 localization 포함
- 기존 SerializedProperty + scoped CustomEditor/PropertyDrawer 방식 재사용
- Undo / array / foldout / object reference / enum / Range/Min 보존

## 보호 대상 — 이번 checkpoint에서도 변경 금지

### Movement USER VERIFIED
- Walk 3
- Run 14
- Idle 0 @1.00
- Walk 3 @1.15
- Run 14 @1.00
- Space Dodge

### P0 Domain physical gameplay USER VERIFIED
- ACTIVE 내부 basic attack / hit / damage / dodge
- Domain ACTIVE 유지
- technique lock 유지
- natural end 뒤 basic/dodge 복구

### Presentation baseline
- Blue 1차 baseline + 4-hit
- Unlimited Void blue-black nebula
- Purple formation/fusion
- current Domain capture/barrier/interior/restoration architecture

## 지금 해야 할 일

1. `git status -sb`

2. Data migration + localization 관련 파일만 selective staging
   - `git add .` 금지
   - Scene/VFX/Audio/Model/user-local unrelated work 제외

3. `git diff --cached --name-status` 검토

4. 구조 checkpoint commit / push

5. 그 뒤 Astra visual track 시작

## 다음 Astra visual track

### Blue
1차 baseline 보호 + 2차 enhancement:
- environment inward reaction
- 4-hit presentation beat
- stronger final collapse
- restrained camera/screen FX
- residual spatial shimmer / dust pull

### Red
repulsion identity:
compressed anticipation → violent release → pressure travel → impact → outward debris/dust → gray-white pressure vapor → aftermath

### Purple
- actual launch-axis bug fix: 약간 아래로 발사되는 원인 수정
- formation/fusion 보호
- release compression / distortion / branching lightning / scar / aftermath

### Cosmic Eye
- whole-eye heartbeat/breathing pulse 제거
- pupil/silhouette 안정
- cloud/rim/right-tail subtle flow 유지

### Domain release
- ceiling/dome/shell artifact 제거
- caster-centered world-space release 유지
- nebula / participant restore / camera safety 보호

### White Blood
- 3 spatial groups
- `퉁 → pause → 투둥 → pause → 퉁`
- timing은 Data/Inspector 조절 가능

## Hardcoding debt 분류

### Astra / Presentation 이후
- camera shake / FOV / hit-stop / flash
- technique feedback tuning
- Infinity ripple presentation
- Target Lock indicator presentation
- visual presentation colors/multipliers

### 별도 architecture 후보
- `worldDeathY` → ArenaRules
- roster/team sorting `_B` convention
- prototype component enable/disable switch
- legacy non-Gojo CE fallback

### 코드에 유지 가능
- epsilon
- clamp
- state-machine mechanics
- collection/event plumbing

## Git 안전

현재 LOCAL working tree에는 기존 사용자 작업이 섞여 있을 수 있다.
절대:
- reset
- clean
- checkout으로 덮기
- `git add .`
- 임의 commit/push/merge
하지 않는다.

`docs/CURRENT_HANDOFF.md`의 REMOTE / LOCAL 구분을 항상 따른다.
