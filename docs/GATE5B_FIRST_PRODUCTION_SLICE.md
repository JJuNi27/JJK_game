# Gate 5B — First Production-Quality Vertical Slice

작성 기준일: 2026-08-24

## 상태

```text
Gate 5A Third Character Stress Test: USER VERIFIED / CLOSED
Gate 5B First Production-Quality Vertical Slice: STARTED

Pass 1 · Character / Team Select Data Boundary: USER VERIFIED
Pass 2 · Battle Runtime consumes MatchTeamSelection:
USER VERIFIED

Pass 3A · Functional Character / Team Select front-end:
PARTIAL USER VERIFIED

Pass 3B · Character Select visual identity shell:
USER VERIFIED

Pass 3C · Character Select return-state continuity:
USER VERIFIED

Pass 4A · Production-facing Combat HUD Canvas shell:
USER VERIFIED

Pass 4B · Combat HUD Consolidation & State Readability:
USER VERIFIED

Pass 4C · Production Match Overlay & Domain Input Readability:
USER VERIFIED

Pass 5 · Production Input Boundary & Developer Harness Isolation:
USER VERIFIED

Pass 6 · Arena Lighting & Post-Process Readability:
USER VERIFIED

Pass 7 · Combat Camera & Impact Feedback Polish:
USER VERIFIED

Pass 8 · JJK-Referenced Production Particle VFX Runtime:
USER VISUAL REVIEW FAILED / REWORK REQUIRED

Pass 8R-1 · Gojo Signature VFX Reference-First Rework:
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

---

# Gate 5B 목표

Gate 3에서 자산 부재로 미뤘던 production-quality 부분을 대표 한 판에 실제 연결한다.

```text
고죠 vs 스쿠나 대표 매치
실제 rigged model
Animator + 실제 animation set
production-grade technique VFX
Canvas/TMP final-style HUD
production SFX / Voice / Music
대표 맵 art / lighting / material / post process
정식 Character / Team Select front-end
```

목표 체감:

```text
"이제 실제 주술회전 게임 같다"
```

---

# 최종 Character / Team Select 방향

```text
Team Size: 1~3명

1명:
MAIN

2명:
MAIN + RESERVE 1

3명:
MAIN + RESERVE 1 + RESERVE 2
```

전투 시작 시 MAIN이 Active Fighter다.

최종 PC 교대 기본안:

```text
1 → Active ↔ Reserve 1
2 → Active ↔ Reserve 2
```

현재 `F3`, `F4`, `T`는 prototype/developer harness이며 최종 사용자 캐릭터 선택/교대 UX로 사용하지 않는다.

세부 UX 계획:

```text
docs/CHARACTER_TEAM_SELECT_PLAN.md
```

---

# Pass 1 — Character / Team Select Data Boundary · USER VERIFIED

파일:

```text
unity/Assets/Scripts/Player/MatchTeamSelection.cs
```

추가한 production-candidate 데이터:

```text
MatchTeamSlot
- Main
- Reserve1
- Reserve2

MatchTeamSelection
- Solo(main)
- Duo(main, reserve1)
- Trio(main, reserve1, reserve2)
- TeamSize
- slot 조회

MatchTeamSelectionStore
- Character Select와 Battle Scene 사이의 임시 cross-scene selection holder
- prototype default GOJO + SUKUNA
```

이 데이터는 캐릭터 선택/팀 슬롯만 표현하며 Battle GameObject, HP/CE, KO, HUD를 직접 소유하지 않는다.

2026-08-23 사용자 확인:

```text
추가 후 Unity compile / 기존 CombatMVP 시작 정상
```

따라서 Pass 1 데이터 경계는 USER VERIFIED 처리한다.

---

# Pass 2 — Battle Runtime consumes MatchTeamSelection

## 구현 상태

```text
USER VERIFIED
```

2026-08-27 사용자 회귀:
- Duo / Trio / Solo 전환 정상
- 1 / 2 Reserve slot 교대 정상
- 3인 KO 연쇄 / 마지막 KO defeat 정상
- F4 scene reload 후 전투 사운드 누락을 수정했고 재검증 정상

추가 combat timing cleanup:
- Hollow Purple lethal damage가 시전 즉시 승리를 띄우던 문제 수정
- 현재 prototype orb merge / travel timing에 맞춰 hit/damage/death/victory가 발생하도록 동기화
- 사용자 확인 정상

이번 묶음은 Pass 1의 선택 데이터를 실제 Player Team Runtime이 소비하게 연결한다.

변경 파일:

```text
unity/Assets/Scripts/Player/PrototypePlayerTeamController.cs
unity/Assets/Scripts/Core/PlayerCombatHudDataSource.cs
unity/Assets/Scripts/Core/CombatInputBindings.cs
```

## Runtime Team Model

기존 2인 고정 배열 / `activeIndex` 구조를 다음 slot 모델로 확장했다.

```text
members[0] = ACTIVE
members[1] = RESERVE 1
members[2] = RESERVE 2
```

`MatchTeamSelectionStore.PlayerTeam`을 읽어 실제 runtime roster를 1~3명으로 구성한다.

교대 시 캐릭터 ID를 새로 계산하는 대신 slot의 `TeamMemberState` 자체를 교환한다.

예:

```text
시작
A  GOJO
R1 SUKUNA
R2 MEGUMI

1 입력
A  SUKUNA
R1 GOJO
R2 MEGUMI

1 다시 입력
A  GOJO
R1 SUKUNA
R2 MEGUMI

2 입력
A  MEGUMI
R1 SUKUNA
R2 GOJO
```

각 TeamMemberState에 저장된 HP / CE / KO 상태도 slot swap과 함께 보존된다.

## 교대 입력

production-facing prototype command:

```text
1 → Reserve 1과 교대
2 → Reserve 2와 교대
```

기존 `T`는 현재 regression 편의를 위해 developer compatibility로 R1 교대에 남겨두었다.

교대 조건은 기존 규칙을 유지한다.

```text
CombatActionState.Normal
manual tag cooldown 완료
대상 Reserve 생존
```

## KO Auto Tag

Active가 KO되면:

```text
R1 생존 → R1 자동 교대
R1 KO + R2 생존 → R2 자동 교대
모든 Reserve KO → 교대 없음 → 기존 final defeat 경로
```

3인 확장에서 이 부분은 반드시 사용자 회귀로 확인한다.

## HUD Snapshot

기존 Gate 4B read-only 경계를 깨지 않고 확장했다.

추가:

```text
TeamSize
Reserve2Character
Reserve2Member
Reserve1TagState
Reserve2TagState
```

호환 유지:

```text
ReserveCharacter = R1
ReserveMember = R1
TagState = R1 state
```

Prototype Team HUD도 A / R1 / R2를 표시하도록 확장했다.

## Developer Harness

정식 Character Select UI가 아직 없으므로 regression용 임시 harness를 둔다.

```text
F3
GOJO + SUKUNA ↔ GOJO + MEGUMI

F4
DUO GOJO/SUKUNA
→ TRIO GOJO/SUKUNA/MEGUMI
→ SOLO GOJO
→ DUO GOJO/SUKUNA
```

`F3/F4/T`는 production UX가 아니다.

---

# Pass 2 사용자 회귀

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

Unity compile 완료 후 `CombatMVP` Play.

## 1. 기본 DUO

```text
[ ] 시작 GOJO + SUKUNA 정상
[ ] 1 → SUKUNA Active / GOJO R1
[ ] 1 다시 → GOJO Active
[ ] 2 → 아무 변화 없음
[ ] HP / CE가 왕복 교대 후 보존
[ ] T도 developer compatibility로 R1 교대 가능
```

## 2. TRIO

기본 DUO에서 `F4`.

```text
[ ] reload 후 TEAM · 3
[ ] A GOJO / R1 SUKUNA / R2 MEGUMI 표시
[ ] 1 → SUKUNA Active / GOJO R1
[ ] 1 다시 → GOJO Active
[ ] 2 → MEGUMI Active / 이전 Active가 R2
[ ] 여러 번 1/2 교대해도 각 캐릭터 HP / CE 보존
[ ] 메구미 Active에서 Q 옥견 정상
[ ] 술식/회피 등 Action 중 1/2 교대 차단
```

KO 회귀:

```text
[ ] 첫 Active KO → 살아있는 R1 자동 교대, DEFEAT 아님
[ ] 다음 Active KO → 남은 R2 자동 교대, DEFEAT 아님
[ ] 마지막 Fighter KO에만 DEFEAT
```

## 3. SOLO

TRIO에서 `F4`.

```text
[ ] reload 후 GOJO Solo
[ ] R1/R2 Team HUD 없음
[ ] 1 / 2 입력으로 교대되지 않음
[ ] 이동 / 기본공격 / Q/E/R/V 정상
```

다시 `F4` → 기본 DUO 복귀.

## 4. 기존 회귀

```text
[ ] F3 G/S ↔ G/M developer stress roster 정상
[ ] F2 Opponent Team 첫 KO reserve entry / target handoff / final victory 정상
[ ] VFX / Camera / Audio 중복 이상 없음
[ ] Console 빨간 오류 없음
```

---

# Pass 2 통과 후

```text
Gate 5B Pass 2: USER VERIFIED
```

그 다음 구현은 정식 `Character / Team Select` Canvas/TMP front-end가 `MatchTeamSelectionStore`를 작성하고 Battle Scene으로 넘기는 흐름이다.

3인 Runtime이 먼저 검증된 뒤 UI를 붙여 UI 문제와 Team Gameplay 문제를 섞지 않는다.


---

# Pass 3A — Functional Character / Team Select Front-End

상태:

```text
USER VERIFIED
```

전용 `CharacterSelect` 씬을 추가했다. `SampleScene`은 developer compatibility host로만 남긴다.
실제 Battle GameObject를 직접 조작하지 않고 다음 경계를 검증한다.

```text
Character Select UI
→ MatchTeamSelection
→ MatchTeamSelectionStore
→ CombatMVP
→ PrototypePlayerTeamController
```

현재 구현:

```text
Roster: GOJO / SUKUNA / MEGUMI
1~3명 선택
선택 순서 = MAIN / R1 / R2
중복 선택 차단
slot 클릭 제거
UNDO / CLEAR
BATTLE 확정
키보드 1/2/3, Backspace, C, Enter
캐릭터 variant + Q/E/R/V preview
Canvas 기반 1920x1080 scale UI
```

주의:
- 현재 typography는 Unity UI Text + runtime system font fallback이다.
- Gate 5B final-style 단계에서 dedicated font asset과 TMP binding으로 교체한다.
- 실제 portrait / 3D preview 자산 hook도 production asset 투입 시 연결한다.

사용자 테스트:

```text
1. CharacterSelect Play
2. GOJO만 선택 → BATTLE → CombatMVP Solo
3. CharacterSelect 재시작 → GOJO + SUKUNA → Duo
4. CharacterSelect 재시작 → GOJO + SUKUNA + MEGUMI → Trio
5. 순서를 바꿔 MAIN이 선택 순서 첫 캐릭터인지 확인
6. slot 제거 / Undo / Clear 확인
7. Battle 진입 후 1/2 교대와 HP/CE 보존 확인
8. Console red error 없음
```


추가 flow cleanup:
- `CharacterSelect.unity` 전용 pre-match 씬 추가
- Build Settings 첫 씬을 CharacterSelect로 변경
- CombatMVP 결과 화면에서 `ENTER = REMATCH`, `ESC = TEAM SELECT`
- SampleScene은 기존 테스트 호환을 위해 selection host로도 계속 허용

현재 사용자 확인:
- Character Select UI가 실제로 표시되는 것까지 확인
- Solo/Duo/Trio handoff 전체 회귀는 다음 테스트에서 확정


---

# Pass 3B — Character Select Visual Identity Shell

상태:

```text
USER VERIFIED
```

사용자 확인:
- CharacterSelect UI 정상 표시
- clean UI-only CharacterSelect scene 전환 후 missing-script 경고 재발 없음
- Battle 진입 / 결과 후 ESC → CharacterSelect 복귀 정상
- Console 오류 없음

Pass 3A에서 확인된 전용 CharacterSelect → Battle → ESC return 흐름 위에
첫 production-facing visual hierarchy를 추가했다.

추가:
- dark arena-fighter visual shell
- character accent glow / animated background pulse
- roster hover preview
- selected fighter MAIN / R1 / R2 badge
- Fighter Profile hierarchy
- variant / role / playstyle description
- technique loadout
- team ready counter
- filled team slot accent
- Korean interaction/status copy
- production asset 전 단계용 monogram preview

새 presentation-only data:
```text
CharacterSelectPresentationData
CharacterSelectPresentationProfiles
```

이 데이터는 UI copy만 소유하고 combat balance / HP / CE / damage / cooldown을 소유하지 않는다.

현재 제한:
- 실제 portrait / 3D model preview asset 없음
- dedicated production font asset 없음
- TMP final binding은 해당 asset import 후 진행
- 따라서 이번 pass는 final art가 아니라 visual identity / hierarchy proof다.


---

# Pass 3C — Character Select Return-State Continuity

상태:

```text
USER VERIFIED
```

사용자 확인:
- Battle 결과 후 ESC → CharacterSelect 재진입 정상
- sceneLoaded bootstrap 수정 후 빈 CharacterSelect 화면 재발 없음
- Console 오류 없음

전투에서 `ESC → CharacterSelect`으로 돌아왔을 때 방금 확정했던 팀 편성을 다시 보여준다.

구조:
- `MatchTeamSelectionStore`가 current runtime roster와 last confirmed Character Select roster를 분리
- UI의 `SetPlayerTeam`만 last confirmed selection을 갱신
- F3/F4 developer harness는 `SetPrototypePlayerTeam`을 사용하여 사용자의 마지막 선택을 덮어쓰지 않음
- CharacterSelect 진입 시 last confirmed team을 MAIN / R1 / R2 순서로 복원
- 첫 실행처럼 아직 사용자가 팀을 확정한 적이 없으면 기존처럼 빈 선택 상태 유지

테스트:
```text
1. CharacterSelect에서 메구미 → 스쿠나 → 고죠 선택
2. BATTLE 진입
3. 승/패 후 ESC
4. CharacterSelect에 MAIN 메구미 / R1 스쿠나 / R2 고죠가 이미 채워져 있는지
5. 슬롯 하나 제거 후 다른 캐릭터로 재편성 가능
6. F3/F4 developer harness 사용 후 TEAM SELECT로 돌아가도 마지막 정식 선택이 유지되는지
7. Console red error 없음
```


Pass 3C return regression 발견 및 원격 수정:
- 증상: Battle 결과 후 ESC로 CharacterSelect scene은 로드되지만 UI runner가 다시 생성되지 않아 빈 화면만 표시
- 원인: CharacterTeamSelectFrontEnd bootstrap이 RuntimeInitializeOnLoadMethod(AfterSceneLoad)에만 의존하여 최초 플레이 진입에서만 설치되고 이후 scene reload/return에서는 다시 설치되지 않음
- 수정: SceneManager.sceneLoaded를 idempotent 구독하고 CharacterSelect/SampleScene이 로드될 때 InstallForCurrentScene() 재실행
- 현재 상태: REMOTE FIXED / USER RETEST PENDING


---

# Pass 4A — Production-facing Combat HUD Canvas Shell

상태:

```text
USER VERIFIED
```

Gate 4에서 검증한 read-only HUD snapshot을 실제 Canvas HUD가 소비하도록 연결했다.

추가 파일:
```text
unity/Assets/Scripts/Core/ProductionCombatHudCanvas.cs
```

표시:
- 좌측 상단 Player 이름 / Variant / HP / CE / Dodge
- 우측 상단 Active Opponent / HP / Encounter mode
- 좌측 하단 Active / R1 / R2 Team slots
- 우측 하단 Q / E / R / V Technique deck
- 중앙 Combo / Chain state
- 캐릭터별 HUD accent / skill accent
- Team KO / reserve 상태 반영

구조:
```text
PlayerCombatHudDataSource
OpponentCombatHudDataSource
        ↓ read-only snapshots
ProductionCombatHudCanvas
```

HUD는 gameplay command / damage / CE / KO를 소유하지 않는다.

기존 prototype IMGUI는 삭제하지 않고 regression fallback으로 보존한다.
Production Canvas가 활성화된 동안:
- MatchController compact HUD
- PrototypePlayerTeamController team HUD
- PrototypeOpponentTeamController opponent HUD
- PrototypeSkillDeckHud

는 중복 표시를 막기 위해 숨긴다.

단:
- F1 control help
- Match result overlay

는 현재 기존 MatchController IMGUI를 그대로 유지한다.

현재 font는 runtime system font fallback이며 final TMP/font asset binding은 asset-dependent stage에서 교체한다.

사용자 테스트:
```text
1. CharacterSelect → 3인 팀 → BATTLE
2. 예전 IMGUI HUD가 겹쳐 보이지 않는지
3. 좌상단 HP/CE가 공격/술식 사용에 따라 갱신
4. 1/2 교대 시 이름/색/기술덱/팀 슬롯 갱신
5. R1/R2 KO 시 팀 슬롯 KO 표시
6. 상대 HP 감소 정상
7. F2 Team Battle에서 상대 active/reserve 상태 HUD 반영
8. F1 도움말 정상
9. 승/패 Result overlay 정상
10. Console red error 없음
```


Pass 4A follow-up fixes verified:
- HP/CE bar fill now shrinks visually with value instead of number-only update
- Opponent HUD follows current TargetLock target instead of staying on Curse A
- switching lock A → B updates displayed opponent HP/name
- user regression: normal


---

# Pass 4B — Combat HUD Consolidation & State Readability

상태:

```text
USER VERIFIED
```

구현:
- production Canvas 활성 중 Gojo technique/chain/Infinity, Sukuna technique, burnout, character switch, Target Lock prototype IMGUI 중복 표시 차단
- 기존 prototype IMGUI 구현은 삭제하지 않고 fallback/debug 경로로 보존
- `TargetLockController.CurrentTarget` read-only 기반 Target Lock chip
- R1/R2 slot에 `Reserve1TagState` / `Reserve2TagState` / `TagCooldownRemaining` 기반 READY / COOLDOWN / LOCKED / KO
- Q/E/R/V 카드에 snapshot 범위의 READY / LOCKED / BURNOUT, Domain INPUT / ACTIVE
- `OpponentCombatHudSnapshot.AttackTelegraphCount` / `AttackTelegraphProgress` 기반 DANGER warning + progress bar
- F1 Help 활성 중 technique deck만 숨김
- Victory / Defeat 시 production Canvas를 숨겨 기존 Match Result overlay 가독성 보존

경계:
- damage / HP / CE / cooldown / KO / tag / target handoff / AI gameplay logic 변경 없음
- character-specific controller를 새 HUD 상태 계산용으로 직접 결합하지 않음
- 기존 Gate 4 read-only HUD snapshot 경계 유지

코드 리뷰:
- Codex local implementation → dedicated branch → master rebase → GitHub diff review → PR merge 완료
- C# 정적 build 결과는 Codex 보고 기준 warning 0 / error 0
- 실제 Unity Play Mode 사용자 검증 완료

사용자 회귀:
```text
1. CharacterSelect → Solo/Duo/Trio Battle
2. prototype combat HUD 중복 표시 없음
3. Gojo/Sukuna/Megumi 기존 gameplay 정상
4. 1/2 tag + HP/CE 보존
5. R1/R2 READY / COOLDOWN / LOCKED / KO 표시
6. TAB Target Lock / A↔B / unlock chip + opponent HP/name 동기화
7. Q/E/R/V READY / LOCKED / BURNOUT, Domain INPUT / ACTIVE 표시
8. 상대 attack windup 동안만 DANGER warning + progress
9. F2 Team Battle / reserve entry / Target Lock handoff
10. F1 Help 열기/닫기 시 technique deck 처리
11. Victory/Defeat result overlay 가림 없음
12. ENTER Rematch / ESC CharacterSelect
13. Console red error 없음
```

---

# Pass 4C — Production Match Overlay & Domain Input Readability

상태:

```text
USER VERIFIED
```

사용자 확인:
- Gojo domain input/release Canvas 정상
- F1 production help 정상
- Victory/Defeat Canvas result overlay 정상
- ENTER Rematch / ESC CharacterSelect 정상
- 기존 전투 HUD/Target Lock/Team flow 정상
- Console red error 없음

구현:
- Gojo 영역 입력 중에만 Canvas domain panel 표시
- controller가 계산한 release progress / green window / cursor를 `PlayerCombatHudSnapshot`으로 read-only 전달
- F1 Control Help를 Canvas overlay로 이동하고 실제 production 입력(WASD / SPACE / TAB / LMB / Q·E·R·V / X / 1·2)에 맞춰 정리
- Solo에서는 교대 도움말을 숨기고 Duo/Trio에서만 R1/R2 교대 표시
- 캐릭터별 기술명은 `CharacterPresentationProfile` 기반으로 표시
- Victory / Defeat 및 결과 설명, ENTER Rematch, ESC Team Select 안내를 Canvas result overlay로 이동
- 결과 중 combat HUD / Target Lock / warning / technique deck을 숨겨 결과 계층 우선
- Production Canvas 활성 중 F1 / Result / Domain IMGUI 중복만 숨기고, Canvas 비활성 시 기존 fallback 유지

경계:
- domain release 성공/실패 판정과 입력 처리는 기존 `GojoDomainController`가 계속 소유
- 승패 판정과 ENTER / ESC 입력 처리는 기존 `MatchController`가 계속 소유
- damage / HP / CE / cooldown / KO / tag / target / AI gameplay 변경 없음
- 외부 image / font / TMP asset 추가 없음

사용자 시각 검수 결과:
- 구조적 ParticleSystem 전환은 동작했지만 기술별 원작 시각 정체성이 충분히 재현되지 않음
- Blue/Red의 핵심 orb가 사라져 원작 인상이 약해짐
- Unlimited Void는 기존 line/ring 인상에서 크게 벗어나지 못함
- Hollow Purple은 기존 대비 개선 폭이 작음
- Sukuna slash 계열은 상대적으로 개선됐으나 전체 품질은 production 기준 미달
- 현재 Pass 8은 USER VERIFIED가 아니며 reference-first 재작업 필요

재작업 전 기준:

```text
1. Gojo V → RMB 유지 → LMB → 초록 구간 RMB 해제
2. 영역 입력 전/후 Canvas domain panel 숨김 확인
3. F1 Help Solo에서 교대 안내 없음
4. F1 Help Duo/Trio에서 1=R1, 2=R2 표시
5. Gojo/Sukuna/Megumi 기술명이 현재 캐릭터에 맞게 표시
6. Victory/Defeat 시 combat HUD가 사라지고 Canvas result overlay만 표시
7. ENTER Rematch / ESC CharacterSelect
8. Canvas component 비활성 시 기존 IMGUI fallback 표시
9. 작은 Game View에서 help/result/domain text 잘림·겹침 확인
10. 기존 전투/팀/Target Lock/F2 회귀 및 Console red error 없음
```

---

# Pass 5 — Production Input Boundary & Developer Harness Isolation

상태:

```text
USER VERIFIED
```

사용자 확인:
- CombatMVP에서 production 입력 회귀 정상
- F2/F3/F4/T developer harness 정상
- F1 / ENTER / ESC 정상
- Gojo domain 입력 sequence 정상
- opponent HUD scene-reload 재바인딩 수정 후 정상
- Console red error 없음

입력 조사 결과:
- production gameplay: WASD / LMB / SPACE / TAB / Q / E / R / V / X / 1 / 2
- production match/UI: F1 / ENTER / ESC
- developer harness: F2 opponent mode, F3 stress roster, F4 team-size cycle, T legacy R1 tag
- CharacterSelect의 1 / 2 / 3 / Backspace / C / Enter는 전투 입력과 별개의 front-end UI 입력으로 유지
- 비활성 team controller에서 사용하던 구형 1 / 2 / 3 직접 캐릭터 reload도 prototype-only로 분류

구조:
- `CombatInputBindings`: 현재 keyboard label과 KeyCode 정의 유지
- `ProductionCombatInput`: Move / BasicAttack / Dodge / TargetLock / Skill1·2 / Ultimate / Domain / Cancel / Reserve1·2 및 Gojo domain modifier gesture 제공
- `ProductionMatchInput`: Control Help / Rematch / Character Select command 제공
- `PrototypeDeveloperInput`: Editor 또는 Development Build에서만 developer shortcut을 활성화
- gameplay component는 command 의미를 읽으며 실제 keyboard/mouse polling은 입력 경계에 집중

developer harness 격리:
- F2 / F3 / F4 / T는 `UNITY_EDITOR || DEVELOPMENT_BUILD`에서 기존과 동일하게 동작
- 일반 production build에서는 해당 command read가 항상 inactive
- 기존 component별 `enableDeveloperHarness`는 Editor/Development Build 내부의 추가 opt-out으로 유지
- production Canvas F1 Help에는 F2 / F3 / F4 / T를 표시하지 않음
- fallback IMGUI의 developer key 문구도 developer harness가 허용된 build에서만 표시

보존:
- 기존 Legacy Input Manager axis와 keyboard/mouse key semantics 유지
- Gojo RMB hold → LMB press → RMB release sequence 및 timing 판정 변경 없음
- F2/F3/F4의 mode/roster/team-size 동작과 T legacy R1 tag 동작 변경 없음
- combat rule / HP / CE / cooldown / KO / tag state / victory 판정 변경 없음
- Input Actions asset, final gamepad mapping, HUD 디자인, asset/font 변경 없음

사용자 테스트 대기:

```text
1. CharacterSelect → Solo/Duo/Trio Battle
2. WASD / LMB / SPACE / TAB / Q / E / R / V / X
3. Gojo V → RMB 유지 → LMB → 초록 구간 RMB 해제
4. 1 / 2 tag 및 HP/CE 보존
5. Editor에서 F2 opponent mode 전환
6. Editor에서 F3 stress roster 전환
7. Editor에서 F4 Duo → Trio → Solo → Duo cycle
8. Editor에서 T legacy R1 tag
9. F1 Help에 F2/F3/F4/T가 없는지 확인
10. Victory/Defeat → ENTER Rematch / ESC CharacterSelect
11. Development Build에서 harness 사용 가능 여부 확인
12. non-Development production build에서 F2/F3/F4/T 비활성 확인
13. Console red error 없음
```

---

# Pass 6 — Arena Lighting & Post-Process Readability

상태:

```text
USER VERIFIED
```

사용자 확인:
- 초기 lighting pass보다 fighter / arena 가독성 개선 확인
- 야간 cool mood 방향 수용
- HUD 가독성 정상
- final map art / lighting의 큰 재튜닝은 실제 맵 자산 단계로 이관

이 pass는 final environment art 완료가 아니라, 실제 environment asset을 넣기 전의
production-facing lighting / post-process readability proof다.

ownership:
- `PrototypeBeautyArenaPresentation`: floor rings / lanes / skyline / neon procedural geometry와 runtime material cleanup
- `ProductionArenaMoodController`: CombatMVP RenderSettings, URP Global Volume, Main Camera post-process, main directional/key light, cool fill, warm rim
- prototype skyline geometry는 directional shadow를 투사하지 않아 fight area shadow cost와 시각 혼잡을 제한
- gameplay VFX가 소유한 일시적 Light와 timing은 변경하지 않음

URP runtime Global Volume:
- scene-owned `Volume`과 runtime-only `VolumeProfile`을 사용해 외부 profile asset 의존성 없음
- ACES Tonemapping
- restrained Bloom: threshold 0.95 / intensity 0.24 / scatter 0.52 / clamp 6
- Color Adjustments: post exposure -0.10 / contrast +11 / saturation -2 / mild cool filter
- White Balance: temperature -4 / tint +1
- subtle Vignette: intensity 0.13 / smoothness 0.55
- Depth of Field / Motion Blur / Film Grain / Chromatic Aberration 미사용

lighting / fog:
- 기존 scene Directional Light를 cool key로 조정하고 단일 soft shadow source로 제한
- non-shadow cool fill 1개와 warm rim 1개만 생성
- linear fog 26–80m로 arena 바깥 silhouette를 정리하면서 30m Target Lock 범위의 과도한 감쇠 방지
- Trilight ambient로 cool night sky / equator / ground 분리
- Main Camera는 dark solid background와 post-processing을 사용
- Screen Space Overlay production HUD는 post-process 대상이 아님

lifecycle / cleanup:
- controller, Global Volume, mood lights는 CombatMVP scene-owned이며 `DontDestroyOnLoad` 미사용
- F2/F3/F4/ENTER reload마다 기존 scene object가 파기되고 새 scene에서 idempotent bootstrap
- runtime VolumeProfile 및 각 VolumeComponent를 `OnDestroy`에서 명시적으로 파기
- controller disable/destroy 시 RenderSettings, camera, directional light 원래 값 복원
- CharacterSelect에는 CombatMVP Volume / fog / mood light가 유지되지 않음
- 외부 asset / font / TMP / texture / Volume Profile asset 추가 없음

사용자 테스트 대기:

```text
1. CharacterSelect → CombatMVP에서 cool urban-night mood 확인
2. Player / Opponent가 배경과 분리되어 보이는지 확인
3. Gojo Blue / Red / Hollow Purple / Unlimited Void 색 분리
4. Sukuna Dismantle / Cleave / Fuga / Malevolent Shrine 색 분리
5. Megumi Divine Dog silhouette 가독성
6. normal / Domain Amplification attack telegraph 가독성
7. Bloom이 네온/VFX를 살리되 화면을 뿌옇게 만들지 않는지 확인
8. 30m Target Lock과 arena 외곽 fog 가독성 확인
9. F1 Help / Domain timing / Result overlay 색과 밝기 정상
10. F2/F3/F4 reload 및 ENTER Rematch 반복 후 Volume/Light 중복 없음
11. ESC CharacterSelect에서 CombatMVP fog/post-process 잔존 없음
12. 다시 CombatMVP 진입 시 mood 정상 복원
13. 기존 gameplay/HUD 회귀 및 Console red error 없음
```

---

# Pass 7 — Combat Camera & Impact Feedback Polish

상태:

```text
USER VERIFIED
```

사용자 확인:
- basic hit / technique camera feedback 변화 확인
- 기존 gameplay와 HUD 정상
- 현재 placeholder SFX / procedural avatar / line-circle VFX 한계 때문에 체감 타격감 상승 폭은 제한적
- 추가 camera 과튜닝은 보류하고 production audio / VFX / animation 자산 단계에서 최종 조정

ownership:
- `TechniquePresentationRequest`는 technique id / phase / world point / direction만 전달하는 production-facing semantic contract로 유지
- `BasicHitPresentationRequest`는 적중한 basic chain step만 전달하며 damage나 판정을 소유하지 않음
- `ProductionCombatFeedbackDirector`가 CombatMVP에서 basic/technique camera, flash, hit-stop tuning을 단독 소유
- 기존 `PrototypeTechniquePresentationDirector` runtime consumer는 production director로 교체되어 동시 request 소비 경로 제거
- `FighterAnimationContract`와 animation cue 소비 경로는 변경하지 않음

camera shake:
- 프레임별 `Random.insideUnitCircle` 대신 unscaled time 기반 연속 Perlin noise 사용
- 각 shake request에 빠른 attack과 제곱 감쇠 envelope 적용
- 겹친 request는 에너지를 합산하되 기존 maximum amplitude 0.75 safety cap 유지
- basic 1타 0.075 < 2타 0.110 < 3타 0.220 < Divine Dog impact 0.260 < signature/domain 계층 유지
- Hollow Purple / Fuga / Domain은 signature 체감을 유지하면서 과도한 amplitude와 duration을 제한

FOV / world focus:
- 각 FOV request를 독립 impulse로 유지해 빠른 phase 연속 입력이 이전 kick을 덮어쓰지 않음
- sine envelope 합산 후 inward -5.5 / outward +10 범위에서 제한하고 각 impulse 종료 시 base FOV로 자연 복귀
- anticipation pull-in, release outward punch, impact/signature punch 방향 유지
- world focus point의 NaN/Infinity를 거부하고 player look point 기준 22m로 제한
- focus request 전환 시 point를 보간하고 unscaled smooth recovery로 player 중심에 복귀
- 기본 follow offset / target과 `TargetLockController` gameplay는 변경하지 않음

basic hit / flash:
- 기본 공격의 damage / cooldown / attack range / knockback / hit stun / impact VFX 호출 위치를 변경하지 않음
- 기존 적중 직후 timing과 shake / flash / hit-stop 수치를 production director로 그대로 이전
- 3타 finisher의 warm flash와 상대적 강도는 유지하되 camera safety cap 적용
- IMGUI full-screen flash를 sorting order 200의 scene-owned Canvas overlay로 교체
- sorting order 250의 `ProductionCombatHudCanvas`가 flash 위에 렌더되어 강한 technique 중 HUD 가독성 유지

lifecycle:
- feedback director는 CombatMVP scene-owned이며 `DontDestroyOnLoad` 미사용
- bootstrap과 active-instance guard로 F2/F3/F4/ENTER reload 시 단일 request consumer 유지
- camera disable/destroy 시 shake/FOV/focus/flash 상태와 base FOV 복원
- persistent hit-stop controller는 scene unload 즉시 캡처한 time scale을 복원
- ESC CharacterSelect 및 reload가 hit stop 도중 발생해도 낮은 time scale이 다음 scene에 남지 않음

사용자 테스트 대기:

```text
1. CharacterSelect → CombatMVP에서 기본 follow offset과 이동 조작감이 기존과 같은지 확인
2. 기본 공격 1타 < 2타 < 3타 finisher의 shake / flash / hit stop 계층 확인
3. 3타 finisher 중 공격 판정 / damage / knockback / hit stun timing 회귀 확인
4. Hollow Purple release/culmination의 shake, FOV, focus와 player 중심 복귀 확인
5. Fuga anticipation → release → impact의 pull-in / outward kick stacking과 focus 확인
6. Unlimited Void / Malevolent Shrine activation이 화면을 잃지 않으면서 가장 강하게 느껴지는지 확인
7. Divine Dog release/impact focus와 basic finisher보다 강한 impact 확인
8. 강한 flash 중 HP/CE/skill/Target Lock HUD가 위에서 읽히는지 확인
9. F2/F3/F4 reload 반복 후 feedback director / flash Canvas 중복 없음
10. hit stop 순간 F2/F3/F4 또는 ENTER Rematch 후 Time.timeScale과 FOV 정상 확인
11. hit stop 순간 ESC CharacterSelect 후 정상 속도, flash/focus/shake 잔류 없음
12. 다시 CombatMVP 진입 후 feedback 정상 복원
13. 기존 Target Lock / Dodge / Tag / Domain / Victory / Defeat 회귀 확인
14. Console red error 없음
```

---

# Pass 8 — JJK-Referenced Production Particle VFX Runtime

상태:

```text
USER VISUAL REVIEW FAILED / REWORK REQUIRED
```

사용자 시각 검수:
- 구조적 ParticleSystem 전환은 동작했지만 기술별 원작 시각 정체성이 충분히 재현되지 않음
- Blue/Red의 핵심 orb가 사라져 원작 인상이 약해짐
- Unlimited Void는 기존 line/ring 인상에서 크게 벗어나지 못함
- Hollow Purple은 기존 대비 개선 폭이 작음
- Sukuna slash 계열은 상대적으로 개선됐으나 전체 품질은 production 기준 미달
- reference-first 방식으로 재작업 후 다시 사용자 검수 필요

이 단계는 final anime-quality VFX가 아니다. 외부 texture / flipbook / mesh / VFX Graph /
custom shader 없이 기존 line/ring prototype을 replaceable ParticleSystem runtime으로 전환하고,
공식 애니와 공식 게임에서 확인되는 기술별 motion / color / timing hierarchy를 우리 arena에 맞게
재해석한 JJK visual identity proof다. 타 작품의 asset이나 디자인을 복제하지 않는다.

contract / ownership:
- Gate 4 `TechniquePresentationRequest → presentation consumer → PresentationVfxSpawnRequest → PresentationVfxRuntime` 경계 유지
- `PresentationVfxStyleId`는 renderer-facing metadata만 가지며 damage / cooldown / hit timing 규칙 없음
- `Generic` default로 기존 `AtWorld` / `Follow` 호출 호환 유지
- `ProductionParticleVfxRuntime`과 모든 effect는 CombatMVP scene-owned
- `PrototypePresentationVfxRuntime` 자동 bootstrap / persistent registration 제거, manual fallback/reference로만 유지
- `ProductionSignatureVfxDirector`가 semantic technique phase를 style spawn request로 변환
- Pass 7 `ProductionCombatFeedbackDirector`의 shake / FOV / focus / flash / hit stop 소유권 변경 없음

production Particle runtime:
- style당 Core / Streak / Mote 또는 Burst / Residual 등 2~3개의 작은 ParticleSystem 조합
- collision off, shadow casting/receive off, bounded maxParticles, 짧은 lifetime
- particle Light module과 particle별 Point Light 미사용
- URP `Particles/Unlit` 우선, Standard Particle/Sprite safe fallback
- runtime material은 effect instance `OnDestroy`에서 명시적으로 파기
- Natural lifetime / FadeOut / Immediate semantics 유지
- follow target 파기 시 마지막 world position에서 자연 종료하며 target access exception 방지

visual language:
- Gojo Blue: deep-blue dense core + cyan inward streak/mote, outward explosion보다 compression 우선
- Gojo Red: crimson compact core + 빠른 outward repulsion streak + warm red edge
- Hollow Purple: blue/red merge mote + purple ignition + violet forward fragment/residual, 기존 orb 이동과 병행
- Dismantle/Cleave: thin short stretched cut와 제한된 white/red highlight, beam/fire 형태 금지
- Fuga: inward ember charge → directional projectile → red/orange radial burst + dark residual ember
- Unlimited Void: 느린 ordered anticipation mote와 서로 다른 크기/속도의 blue/cyan/pale-violet depth points
- Malevolent Shrine: deep-crimson anticipation rise와 arena를 가리지 않는 다방향 thin field slash
- Divine Dog: low dark shadow rise + teal accent, impact는 3방향 claw/bite streak
- Nue: 짧은 blue-white diagonal electric streak/flicker style 준비, gameplay 추가 없음
- Basic hit: 1타 6 spark / 2타 9 spark / finisher 14 spark + warm secondary burst

Hollow Purple timing:
- canonical orb의 `MergeDuration 0.24s`, `LaunchDuration 0.78s`, `TravelDistance 18m` 변경 없음
- legacy gameplay-owned root 활성 감지를 그대로 사용
- Release particle lifetime을 merge+launch 합계 1.02s에 맞추고 orb transform movement는 수정하지 않음
- Culmination은 기존 +0.09s semantic request 위치에 짧은 purple formation ignition만 추가

lifecycle / cleanup:
- CharacterSelect에는 production runtime registration / active particle / runtime material이 남지 않음
- F2/F3/F4/ENTER reload마다 scene root와 child effect가 함께 파기되고 runtime 단일 재등록
- prototype과 production runtime 동시 자동 registration 없음
- Blue/Red/Fuga follow effect는 gameplay object `OnDestroy`에서 Immediate cleanup
- canonical Purple sequence runner도 CombatMVP scene-owned로 변경

사용자 테스트 대기:

```text
1. CharacterSelect → CombatMVP에서 ProductionParticleVfxRuntime 1개 확인
2. Gojo Blue가 바깥에서 중심으로 압축되고 Red는 바깥으로 밀려나는지 비교
3. Hollow Purple blue/red merge → ignition → 18m travel 동안 particle가 orb timing과 맞는지 확인
4. Sukuna Dismantle/Cleave가 얇은 절단으로 읽히며 laser/fire처럼 보이지 않는지 확인
5. Fuga anticipation → projectile → impact, 일반/Domain amplified 밀도와 dark residual 차이 확인
6. Unlimited Void가 폭발보다 depth/space 변화로 보이고 기존 Domain root와 경쟁하지 않는지 확인
7. Malevolent Shrine이 화염이 아니라 다방향 절단으로 보이며 바닥/전투원을 가리지 않는지 확인
8. Divine Dog shadow summon과 bite/claw impact 확인, Nue는 기존 request 경로만 회귀 확인
9. 기본 공격 1타 < 2타 < 3타 contact spark 계층과 기존 hit timing 확인
10. Bloom에서 Blue/Red/Purple/Teal 색이 white blob으로 소실되지 않는지 확인
11. F2/F3/F4/ENTER 반복 후 runtime/effect/material 중복 또는 MissingReference 없음
12. effect 재생 중 ESC CharacterSelect 후 particle/runtime 잔류 없음
13. 다시 CombatMVP 진입 후 Natural/FadeOut/Immediate 및 follow target cleanup 확인
14. damage / CE / cooldown / range / hitbox / knockback / hit stun / camera / HUD 회귀 확인
15. Console red error 없음
```

---

# Pass 8R-1 — Gojo Signature VFX Reference-First Rework

상태:

```text
USER VISUAL PARTIAL / DETAIL REWORK REQUIRED
```

사용자 시각 검수:
- Pass 8 대비 Blue/Red/Purple/Unlimited Void의 방향성은 확실히 개선됨
- 그러나 세부 레이어, 재질감, 형태 디테일, 애니메이션다운 밀도가 여전히 부족함
- 현재 구현은 reference-first 구조 전환의 개선된 baseline이며 final-quality VFX로 승인되지 않음
- 다음 rework에서 detail layer / material treatment / silhouette richness를 집중 보강할 것

Pass 8의 `USER VISUAL REVIEW FAILED / REWORK REQUIRED` 상태는 유지한다. 이번 rework는
`docs/JJK_VFX_REFERENCE_GOJO.md`를 시각 기준으로 사용하며, 스쿠나 계열은 변경하지 않았다.

signature presentation 경계:
- `PresentationVfxRuntime` 등록은 기존 CombatMVP scene-owned runtime 하나만 유지
- `ProductionParticleVfxRuntime.Spawn`이 Gojo Blue/Red를 전용 orb-first instance로 dispatch
- generic/basic hit/Fuga/Sukuna/Megumi particle path는 기존 `ProductionParticleVfxInstance` 유지
- Hollow Purple은 gameplay-owned marker root의 activation을 소비하는 canonical sequence가 전용 구체와 travel을 소유
- Unlimited Void는 `PresentationVfxStyleId.UnlimitedVoidActive` particle burst를 production path에서 사용하지 않고 domain visual root 내부 environment가 소유

visual rework:
- Blue: deep-blue dense sphere + electric-blue shell + 부분 circular boundary + outer-to-core particle motion
- Red: 이동 중 유지되는 crimson sphere + 짧은 residual trail + 반복을 제한한 outward shell/shock front + impact repulsion
- Hollow Purple: 독립 Blue/Red core/rim orb가 0.24초 동안 접근한 뒤 dense violet body로 점화되어 0.78초/18m 이동
- 기존 Hollow Purple long beam 생성 제거; gameplay damage sync와 marker lifetime은 유지
- Unlimited Void: inward-visible near-black enclosure, collider 없는 dark floor overlay, Near/Far/Dust depth stars, dark central body와 제한된 celestial arc로 환경 전환
- 기존 GroundRing/OrbitRingA/OrbitRingB 중심 prototype은 production component에서 제거

lifecycle / ownership:
- runtime 생성 sphere의 collider 제거, renderer shadow/receive shadow 비활성
- Blue/Red handle의 Immediate/FadeOut와 follow target 유실 시 마지막 위치 유지
- Purple sequence root는 CombatMVP runner child이며 완료/scene unload 때 runtime material과 함께 파기
- Void environment는 domain root 비활성화 시 star field를 stop-and-clear하고 재활성화 시 fresh play
- Void runtime material은 domain visual 파기 시 명시적으로 파기
- `ProductionArenaMoodController`/`RenderSettings`를 수정하지 않음
- `ProductionCombatFeedbackDirector`의 shake/FOV/focus/flash/hit-stop 소유권 변경 없음
- damage, CE, cooldown, hitbox, pull/push, stun, Purple timing/damage sync, Domain state/duration/radius/rule/input 변경 없음

Unity 사용자 시각 검수 필수:

```text
1. Blue field에서 파란 orb, 원형 범위, outer → core 수렴이 한 화면에 읽히는지
2. Blue impact cue가 outward electric/ice burst로 보이지 않는지
3. Red release/flight에서 crimson orb가 소실되지 않고 core → outer shock front가 읽히는지
4. Red impact가 fire explosion보다 공간을 미는 shell/ring으로 보이는지
5. Blue와 Red를 연속 사용해 색 없이도 motion 방향이 반대로 읽히는지
6. Purple 발동에서 실제 Blue/Red orb → 중앙 merge → dense Purple body가 순서대로 보이는지
7. Purple이 18m travel 동안 짧은 trail만 남기고 long beam처럼 보이지 않는지
8. Unlimited Void에서 arena가 near-black enclosure/depth stars/focal body 내부 공간으로 바뀌는지
9. Void floor가 기존 floor를 억제하되 fighter/Target Lock/HUD를 가리지 않는지
10. Void 종료, F2/F3/F4, ENTER rematch, ESC CharacterSelect 후 backdrop/floor/stars/light 잔류가 없는지
11. Bloom에서 Blue/Red/Purple 중심색이 white blob으로 소실되지 않는지
12. 기존 damage/CE/cooldown/pull/push/stun/Purple damage sync/Domain duration 회귀가 없는지
13. Console red error와 MissingReference가 없는지
```
