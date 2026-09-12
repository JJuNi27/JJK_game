# New Chat Bootstrap — JJK_game

새 ChatGPT 채팅에서 이 프로젝트를 바로 이어가기 위한 시작 지시문.

아래 블록을 새 채팅 첫 메시지로 붙여넣는다.

---

## Copy/Paste Prompt

나는 GitHub 저장소 `JJuNi27/JJK_game`에서 Unity 6 URP 기반 주술회전 3D 캐릭터 액션 격투게임을 개발 중이야.

이전 채팅이 길어져 새 채팅으로 넘어왔다.
내가 이전 내용을 다시 설명하게 하지 말고, 먼저 GitHub connector를 사용해서 아래 문서를 직접 읽고 현재 상태를 복구해줘.

반드시 읽을 문서 순서:
1. `AGENTS.md`
2. `docs/design/GAME_VISION.md`
3. `docs/CURRENT_HANDOFF.md`
4. `docs/active/CURRENT_TASK.md`
5. `docs/locked/USER_VERIFIED_SETTINGS.md`
6. 현재 작업과 관련된 `docs/architecture/*`

VFX 작업이면 추가로:
- `docs/design/VFX_DIRECTION.md`
- `docs/architecture/GOJO_VFX.md`

Domain이면:
- `docs/architecture/DOMAIN_SYSTEM.md`
- `docs/architecture/CAMERA_PRESENTATION.md`

Data/Inspector면:
- `docs/architecture/COMBAT_DATA_DRIVEN.md`

현재 작업 branch는 우선:
`feat/gojo-blue-screen-distortion`

master만 보고 현재 상태를 추정하지 말 것.
`docs/CURRENT_HANDOFF.md`의 최신 날짜 섹션을 가장 우선한다.
오래된 `DEVELOPMENT_ROADMAP.md` / Gate 문서와 충돌하면 최신 handoff / current task / locked settings가 우선한다.

## 프로젝트 핵심 방향

- 1:1 중심 3D 액션 격투
- 넓은 JJK 도시형 전투 맵
- Gojo는 첫 production-quality 캐릭터일 뿐 고정 주인공이 아님
- 팬텀 퍼레이드 + 애니/만화 원작 고증
- Strikeborn급 이상의 타격감 / presentation density / aftermath
- Trait / Passive / Domain rule 기반 재사용 가능한 시스템
- Data-driven / Inspector tuning 우선

## 영구 개발 규칙

- 하드코딩은 최소화한다.
- 코드는 HOW, Data Asset은 WHAT을 담당한다.
- 캐릭터/기술/밸런스 값을 바꾸기 위해 C#을 열어야 하면 Data ownership을 먼저 의심한다.
- Character name / GameObject name string으로 gameplay rule을 몰래 결정하지 않는다.
- C# identifier는 영어 유지.
- **사용자가 직접 만지는 Unity Inspector / Data Asset / 설정 UI는 한국어 표시가 기본.**
- 새 Profile / ScriptableObject는 생성 시점부터 한글 Inspector까지 같이 만든다.
- Header만 한국어이고 field label이 영어면 미완성으로 본다.
- 사용자 검증된 값은 명시적 요청 없이 변경하지 않는다.

## 현재 보호된 USER VERIFIED 기준

- Walk 3 / Run 14
- Idle 0 @1.00 / Walk 3 @1.15 / Run 14 @1.00
- Space Dodge
- P0 Unlimited Void ACTIVE 내부 basic attack / actual hit / damage / dodge
- Domain 자연 종료 후 basic attack / dodge 복구
- Domain ACTIVE 동안 Blue/Red/Purple/Domain technique lock
- Gojo Blue 1차 baseline + 4-hit gameplay
- Unlimited Void blue-black nebula background
- Hollow Purple formation / fusion baseline
- `Gojo_Blender_MasterAvatar` animation pipeline

## 2026-09-13 LOCAL 최신 상태 — 매우 중요

최신 remote production-code checkpoint는:
`142606ea3fe7a7298bb2ea1c54bb80efcaf170c6`
`feat: finalize Gojo P0-P6 presentation checkpoint`

그 이후 사용자 PC LOCAL working tree에서:

1. Combat Data-driven migration 완료
2. Data Asset tuning이 실제 runtime damage에 반영되는 것을 사용자가 직접 확인
3. 이동 / 평타 / Blue / Red / Purple / Domain 정상 확인
4. 따라서 Data-driven migration은 LOCAL USER VERIFIED
5. 새 Profile/Data Asset의 영어 Inspector label을 scoped CustomEditor/PropertyDrawer로 한글화
6. localization compile 0 / focused test 24/24 / 13 asset hash 불변 확인

단, 위 Data migration / localization은 이 bootstrap 갱신 시점에 아직 별도 code commit/push 전일 수 있다.
GitHub에 이미 구현됐다고 단정하지 말고 같은 PC라면 먼저:

```powershell
git status -sb
git log -1 --oneline
```

을 확인한다.

LOCAL Data 구조 후보:
- CharacterStatsProfile
- CursedEnergyProfile
- BasicAttackProfile + variable AttackStep[]
- GojoTechniqueGameplayProfile
- DomainGameplayProfile
- BurnoutPolicyProfile
- TargetingProfile
- TrainingBotProfile
- CharacterCombatDefinition
- CharacterCombatCatalog
- Trait / Passive seam

## 현재 다음 작업 순서

1. 사용자가 새 Data Asset Inspector 한글 표기를 실제로 확인
2. local dirty tree 재확인
3. Data migration + localization 관련 파일만 selective staging
4. `git diff --cached --name-status` 검토
5. 구조 checkpoint commit / push
6. 이후 Astra visual track

절대 `git add .`로 unrelated Scene/VFX/Audio/Model 작업을 섞지 않는다.
reset / checkout / clean 금지.
사용자 승인 전 commit / push / merge를 임의로 하지 않는다.

## Astra visual track — 다음 큰 시각 작업

- Blue: 1차 baseline 보호 + environment reaction / final collapse / aftermath 강화
- Red: repulsion identity 기반 2차 enhancement
- Purple: 아래로 살짝 나가는 실제 launch-axis 원인 수정 + 2차 enhancement
- Cosmic Eye: 전체 heartbeat/breathing pulse 제거, cloud/rim/tail subtle flow 유지
- Domain release: ceiling/dome/shell artifact 실제 원인 제거
- White Blood: `퉁 → pause → 투둥 → pause → 퉁` 3-group beat, timing Data/Inspector 조절

VFX는 원작 정체성을 우선하고 `기술 생성 → 이동 → 펑 → 끝`으로 완료 처리하지 않는다.
Environment Reaction / Aftermath / Camera / Audio까지 포함해 평가한다.

## Domain 핵심

다음 3개는 절대 하나로 합치지 않는다.
1. Gameplay Capture Radius
2. Visual Barrier Radius / Diameter
3. Domain Interior Space

현재 same-scene isolated interior + participant safe restoration 구조를 유지한다.
Barrier HP/destruction/outside rescue는 현재 범위 밖.

## Local asset 안전

다음은 local-only일 수 있음:
- `unity/Assets/LocalModels/`
- `unity/Assets/Resources/LocalAudio/`
- `.blend`
- imported FBX / texture / audio
- `VFXLab.unity` 사용자 로컬 변경

명시적 승인 없이 추가/덮어쓰기/reset/clean 하지 않는다.

## 새 채팅의 첫 응답 요구

문서를 읽은 뒤:
1. 현재 REMOTE와 LOCAL-ONLY 상태를 구분해 5~10줄로 복구 요약
2. USER VERIFIED 보호값을 다시 pending으로 되돌리지 않기
3. 현재 immediate next action 하나만 명확히 제시
4. 자동 테스트를 사용자 Play Mode 검증과 동일시하지 않기
5. 전문 용어는 필요하면 짧게 뜻도 설명하기

참고:
무량공처 장인 인식(MediaPipe/RandomForest) 별도 MVP 문서는 다른 실험 흐름이다.
명시적으로 연결하라고 하지 않는 한 현재 Unity `JJK_game` 전투/VFX/animation 작업과 섞지 않는다.

---

## Handoff policy

- 새 중요 결정이 생기면 `docs/CURRENT_HANDOFF.md` 갱신
- 현재 실행 작업은 `docs/active/CURRENT_TASK.md` 갱신
- 사용자 직접 검증 항목은 `docs/locked/USER_VERIFIED_SETTINGS.md` 반영
- 장기 철학은 `GAME_VISION.md` / architecture 문서에 남김
- REMOTE / LOCAL / CODEX VALIDATED / USER VERIFIED를 구분
- 다음 새 채팅에서도 이 bootstrap + 최신 canonical docs만으로 재개 가능해야 함
