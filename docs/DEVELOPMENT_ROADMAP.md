# 개발 로드맵과 우선순위 판단 기준

작성 기준일: 2026-08-24

상세 게임 방향은 `docs/PROJECT_DIRECTION.md`를 기준으로 한다.
Gate 3 자산 범위와 Gate 5 세부 순서의 최신 실행 기준은 `docs/ROADMAP_SCOPE_REFINEMENT.md`를 함께 따른다.
캐릭터/팀 선택 front-end 실행 계획은 `docs/CHARACTER_TEAM_SELECT_PLAN.md`를 따른다.

## 판단 기준

```text
NOW
FOUNDATION_ONLY
LATER
REJECT
```

새 작업은 전투 루프 정확성, 재사용성, 향후 재작성 비용, 자산 의존성, 원작/게임오리지널 구분, 최종 3D 아레나 팀전 방향을 기준으로 판단한다.

---

# 현재 프로젝트 단계

```text
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED
Gate 5A Third Character Stress Test: USER VERIFIED / CLOSED
Gate 5B First Production-Quality Vertical Slice: STARTED
- Pass 1 Character / Team Select Data Boundary: USER VERIFIED
- Pass 2 Battle Runtime consumes MatchTeamSelection: USER VERIFIED
- Pass 3A Functional Character / Team Select front-end: PARTIAL USER VERIFIED
- Pass 3B Character Select visual identity shell: USER VERIFIED
- Pass 3C Character Select return-state continuity: USER VERIFIED
- Pass 4A Production-facing Combat HUD Canvas shell: USER VERIFIED
- Pass 4B Combat HUD Consolidation & State Readability: USER VERIFIED
- Pass 4C Production Match Overlay & Domain Input Readability: USER VERIFIED
- Pass 5 Production Input Boundary & Developer Harness Isolation: USER VERIFIED
- Pass 6 Arena Lighting & Post-Process Readability: USER VERIFIED
- Pass 7 Combat Camera & Impact Feedback Polish: USER VERIFIED
- Pass 8 JJK-Referenced Production Particle VFX Runtime: USER VISUAL REVIEW FAILED / REWORK REQUIRED
- TMP/final asset binding: ASSET-DEPENDENT / PENDING
Gate 6 Production: PENDING
```

---

# 게임 방향

```text
캐릭터 중심 3D 아레나 액션 격투

Core A: 1v1 Solo Battle
Core B: 2~3인 Team Battle
- 전장 Active 1명
- Reserve 1 / Reserve 2 교대
- 캐릭터별 HP/주력 독립 보존

LATER:
- 4~8인 Incident Battle
- 환경 파괴 강화
- PvPvE/사건전
```

초기 메인으로 하지 않음:

```text
20~50인 상시 FFA
오픈월드 MMO
지원 공격/합동기 선행 확장
온라인 선행 구현
```

---

# Gate 1 — Core Combat Proof · USER VERIFIED

현대 고죠와 시부야 스쿠나의 핵심 전투 규칙, 방어/영역/주력 상호작용 검증 완료.

# Gate 2 — Match Architecture Proof · USER VERIFIED

```text
Player Active / Reserve
T prototype Tag
HP / CE 독립 보존
KO Auto Tag
마지막 Player KO에만 DEFEAT

Opponent Active / Reserve
첫 KO → Reserve Entry
Target Lock 자동 승계
마지막 상대 KO에만 VICTORY

Training / Team Battle 분리
```

# Gate 3 — Beauty Corner Prototype Proof · USER VERIFIED

```text
Combat Feel
Prototype Character Presentation
Signature Technique Presentation
Arena Mood
HUD / Skill Deck
```

초기 Gate 3 예시에 있던 실제 rigged model / production animation / final HUD / production audio는 자산 부재로 미뤄졌고 Gate 5B에서 회수한다.

# Gate 4 — Production Pipeline Extraction · USER VERIFIED / CLOSED

확정한 production-candidate 경계:

```text
Character Presentation Profile
HUD read-only Snapshot
Technique Presentation Request
VFX Lifecycle
Animation State / Cue
Audio Event
```

세부:

```text
docs/GATE4_PRODUCTION_PIPELINE_PLAN.md
docs/GATE4_PRODUCTION_BOUNDARIES.md
docs/GATE4_FINAL_REGRESSION.md
```

Gate 4에서는 실제 반복된 것만 공통화했고 고유 Gameplay를 거대한 범용 Ability System으로 합치지 않았다.

---

# Gate 5A — Third Character Stress Test · USER VERIFIED / CLOSED

스트레스 캐릭터:

```text
후시구로 메구미
```

검증 완료:

```text
새 Character ID/Profile을 기존 HUD가 수용
HP / CE / Team state 보존
Animation State/Cue 재사용
Q 옥견 별도 combat actor 소환
Technique Presentation Request 재사용
VFX Lifecycle 재사용
Audio Event 재사용
기존 고죠/스쿠나 Presentation/HUD 대량 복붙 없음
```

옥견은 실제로 소환·추적·근접 공격·Damage가 정상 작동했고 final cleanup regression도 사용자 확인 완료.

F3/T/Alpha 키는 production UX가 아니라 developer harness다.

세부:

```text
docs/GATE5A_THIRD_CHARACTER_STRESS_TEST.md
```

---

# Gate 5B — First Production-Quality Vertical Slice · STARTED

Gate 3에서 자산 부재로 미룬 production-quality 부분을 대표 한 판에 실제 연결한다.

```text
고죠 vs 스쿠나 대표 매치
실제 rigged model
Animator + 실제 animation set
production-grade technique VFX
Canvas/TMP final-style HUD
production SFX / Voice / Music
대표 맵 art / lighting / material / post process
```

이 단계에서 첫 정식 pre-match Character / Team Select front-end도 만든다.

최종 Team Select 방향:

```text
1명: MAIN
2명: MAIN + RESERVE 1
3명: MAIN + RESERVE 1 + RESERVE 2

1 → Active ↔ Reserve 1
2 → Active ↔ Reserve 2
```

Pass 1 · USER VERIFIED:

```text
MatchTeamSelection
- Team Size 1~3
- Main / Reserve1 / Reserve2 slot data

MatchTeamSelectionStore
- Character Select와 Battle Scene 사이의 임시 selection holder
- prototype default GOJO + SUKUNA
```

Pass 2 · USER VERIFIED:

```text
PrototypePlayerTeamController가 MatchTeamSelectionStore를 소비
Runtime Team Size 1~3
members[0] Active / [1] R1 / [2] R2
1 = Active ↔ R1
2 = Active ↔ R2
HP / CE / KO state slot swap 보존
첫/두 번째 KO 시 살아있는 Reserve Auto Tag
PlayerCombatHudSnapshot에 TeamSize / R2 / per-slot tag state 확장
```

Developer regression harness:

```text
F3: G/S ↔ G/M Duo
F4: Duo G/S → Trio G/S/M → Solo G → Duo G/S
T: R1 교대 compatibility
```

F3/F4/T는 최종 사용자 입력이 아니다.

Pass 2 Unity 회귀는 사용자 검증 완료.

Pass 3A:
- CharacterSelect 전용 pre-match scene
- SampleScene은 developer compatibility host
- Canvas runtime front-end
- Gojo / Sukuna / Megumi roster
- 1~3명 선택
- 선택 순서 = MAIN / R1 / R2
- MatchTeamSelectionStore 작성 후 CombatMVP 진입
- 현재 typography는 임시 Unity UI Text/runtime font
- UI boot/visibility 사용자 확인
- Solo/Duo/Trio handoff 전체 회귀 대기
- 결과 화면 ESC → CharacterSelect return flow 추가

Pass 3B visual identity shell은 사용자 검증 완료.

Pass 3C CharacterSelect return-state continuity는 사용자 검증 완료.

Pass 4A에서는 Gate 4 HUD snapshot 경계를 그대로 사용해 CombatMVP의 첫 Canvas 전투 HUD shell을 연결했고 사용자 검증 완료.
HP/CE bar visual fill 및 current TargetLock 기반 opponent HUD도 정상 확인했다.
기존 prototype IMGUI HUD는 fallback으로 보존하되 Canvas 활성 시 중복 표시만 숨긴다.

Pass 4B에서는 남아 있던 캐릭터별 prototype 전투 HUD를 production Canvas 상태 표시와 통합했고 사용자 검증 완료.
Target Lock chip / Reserve tag state / Q·E·R·V READY·LOCKED·BURNOUT / Domain INPUT·ACTIVE /
Opponent attack telegraph를 Canvas로 표시하고, F1 Help와 Match Result overlay는 기존 경로를 유지한다.

Pass 4C에서는 Gojo domain input/release timing, F1 Control Help, Victory/Defeat Result를 production Canvas overlay로 통합했다.
domain timing은 `PlayerCombatHudSnapshot`에 추가한 최소 read-only presentation state만 소비하며,
승패 및 rematch/character select 입력 소유권은 `MatchController`에 유지한다.
Production Canvas 활성 중 production-facing 중복 IMGUI만 숨기고 비활성 시 기존 fallback을 보존한다.
사용자 Unity Play Mode 회귀까지 정상 확인했다.

Pass 5에서는 gameplay component의 직접 keyboard/mouse polling을 `ProductionCombatInput`과
`ProductionMatchInput` command 경계로 모으고, F2/F3/F4/T를 `PrototypeDeveloperInput`으로 격리했다.
Editor / Development Build의 regression harness 동작은 유지하며 일반 production build에서는
developer command가 inactive가 된다. Input Actions asset이나 최종 gamepad mapping은 확정하지 않았다.
사용자 Unity Editor 회귀까지 정상 확인했다.

Pass 6에서는 `PrototypeBeautyArenaPresentation`을 procedural geometry 책임으로 좁히고,
scene-owned `ProductionArenaMoodController`가 RenderSettings / URP runtime Global Volume /
main key + cool fill + warm rim lighting을 소유하도록 분리했다. ACES, 제한된 Bloom,
Color Adjustments, White Balance, 약한 Vignette와 linear fog로 야간 전투 가독성 기준을 만든다.
이는 final environment art 완료가 아닌 production-facing lighting/post-process readability proof이며,
사용자 시각 확인에서 초기 버전보다 가독성이 개선됨을 확인했고, final map art/lighting 재튜닝은 실제 맵 자산 단계로 이관한다.

Pass 7에서는 기존 `TechniquePresentationRequest`와 기본 공격 판정 timing을 유지하면서,
CombatMVP scene-owned `ProductionCombatFeedbackDirector`가 camera / flash / hit-stop tuning을 소유한다.
`SimpleCameraFollow`는 연속 noise 기반 damped shake, 중첩 가능한 bounded FOV impulse,
거리 제한과 smooth recovery를 갖는 world focus를 제공한다. Flash는 production HUD보다 낮은
Canvas sorting order에서 표시되며, scene unload 중 hit stop은 캡처한 time scale로 즉시 복원된다.
Gameplay damage / cooldown / knockback / hit stun / target lock / input은 변경하지 않는다.

Pass 8에서는 Gate 4 `PresentationVfxRuntime` 경계에 renderer-facing style metadata를 추가하고,
CombatMVP scene-owned `ProductionParticleVfxRuntime`이 ParticleSystem / URP runtime material /
effect lifetime을 소유한다. Blue의 inward compression, Red의 outward repulsion, Purple merge/travel,
Fuga charge/projectile/fire burst, Shrine의 field slash, Void의 depth mote, Divine Dog의 shadow rise,
Nue electric streak, basic contact spark를 서로 다른 motion language로 구성한다.
이는 final anime-quality VFX가 아니라 line/ring prototype을 replaceable ParticleSystem runtime으로
전환하고 JJK 기술 정체성의 가독성을 검증하는 단계다.

전용 font / portrait / model asset이 들어오면 TMP + final asset binding으로 교체한다.

세부:

```text
docs/CHARACTER_TEAM_SELECT_PLAN.md
docs/GATE5B_FIRST_PRODUCTION_SLICE.md
```

목표 체감:

```text
"이제 실제 주술회전 게임 같다"
```

---

# Gate 6 — Production

Gate 5A에서 pipeline 구조를 검증하고 Gate 5B에서 실제 production 품질을 검증한 뒤 본격 확장한다.

```text
캐릭터 로스터
맵
추가 모드
온라인
Incident Battle
PvE / Story
```

---

# Codex / Agent / Context7 사용 원칙

Gate 번호 자체가 도구 사용 여부를 결정하지 않는다.

```text
여러 파일 구조 변경 + agent 실제 사용 가능
→ Codex/agent 우선 검토
→ diff review
→ 사용자 Unity test

작은 변경 또는 현재 agent 실행 인터페이스 없음
→ 직접 GitHub 수정
→ 동일하게 사용자 Unity test
```

실제로 사용하지 않은 도구를 사용했다고 표현하지 않는다.

Context7은 VS Code Codex에 MCP로 연결 완료됐다.
Unity/package API의 현재 문서가 구현 정확도에 중요한 경우 선택적으로 참고한다.
프로젝트 자체 코드와 실제 설치 버전은 항상 저장소를 우선한다.

---

## 캐릭터 설계 원칙

- 모든 캐릭터를 같은 강도로 평준화하지 않는다.
- 원작상 강한 캐릭터는 실제로 강한 플레이 경험을 준다.
- 경쟁 팀전은 Character Cost/편성 제한을 검토한다.
- 범용 방어보다 캐릭터별 고유 방어 규칙을 우선한다.
- 영역전개는 단순 대미지 궁극기가 아니라 Match State로 취급한다.
