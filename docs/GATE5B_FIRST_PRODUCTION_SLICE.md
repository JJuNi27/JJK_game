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
REMOTE IMPLEMENTED / USER TEST PENDING
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
REMOTE IMPLEMENTED / USER TEST PENDING
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
