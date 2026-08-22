# 개발 로드맵과 우선순위 판단 기준

작성 기준일: 2026-08-23

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
Gate 5A Third Character Stress Test: IN PROGRESS
- Pass 1 Identity / Roster / HUD Plumbing: USER VERIFIED
- Pass 2 Divine Dog Summon Contract Reuse: REMOTE IMPLEMENTED / USER TEST PENDING
Gate 5B First Production-Quality Vertical Slice: PENDING
Gate 6 Production: PENDING
```

---

# 게임 방향

```text
캐릭터 중심 3D 아레나 액션 격투

Core A: 1v1 Solo Battle
Core B: 2~3인 Team Battle
- 전장 Active 1명
- Reserve 교대
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
T Tag
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

# Gate 5A — Third Character Stress Test · IN PROGRESS

1차 스트레스 캐릭터:

```text
후시구로 메구미
```

선택 이유:

```text
옥견 / 누에 / 만상처럼 식신이 별도 전투 주체가 되므로
고죠/스쿠나의 직접 술식 구조와 다르다.
```

검증 질문:

```text
새 Character ID/Profile만 추가해 HUD가 따라오는가?
Animation State/Cue를 새 Fighter가 재사용하는가?
소환 술식이 Technique Presentation/VFX Lifecycle/Audio Event를 재사용하는가?
고죠/스쿠나 Presentation/HUD 코드를 복붙하지 않아도 되는가?
```

Pass 1 · USER VERIFIED:

```text
Identity / Profile
2인 팀 구조를 유지한 Stress Roster
F3: GOJO + SUKUNA ↔ GOJO + MEGUMI
Megumi prototype visual
HUD / Skill Deck / Animation State plumbing
HP / CE state preservation
```

F3는 production character selection이 아니라 Gate 5A 개발용 stress-test shortcut이다.

Pass 2 · REMOTE IMPLEMENTED / USER TEST PENDING:

```text
Q 옥견 첫 소환 Gameplay
별도 summon actor lifetime / targeting / damage ownership
TechniquePresentationRequest.DivineDog
PresentationVfxRuntime 재사용
CombatAudioEvent.DivineDog
기존 Animation cue 경계 재사용
```

첫 옥견 경로의 실제 결과를 보기 전에는 범용 Summon Framework를 선행 구축하지 않는다.

고유 식신 AI/소환 규칙이 별도 코드인 것은 정상이다.
공통 HUD/Presentation을 복붙해야 한다면 Gate 4 contract를 보강한다.

---

# Gate 5B — First Production-Quality Vertical Slice

Gate 5A 구조 검증 뒤, 최초 Gate 3에서 자산 부재로 미룬 production-quality 부분을 대표 한 판에 실제 연결한다.

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

```text
Canvas + TMP
Character portrait/card grid
Character + Variant 표시
Solo / Active + Reserve team slot 선택
Match Setup 확정 후 Battle Scene 진입
```

현재 Alpha/F3 keyboard roster switching은 developer-only 경로로 남기거나 제거하며 최종 사용자 캐릭터 선택 방식으로 사용하지 않는다.

세부:

```text
docs/CHARACTER_TEAM_SELECT_PLAN.md
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

# Codex / Agent 사용 원칙

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

---

## 캐릭터 설계 원칙

- 모든 캐릭터를 같은 강도로 평준화하지 않는다.
- 원작상 강한 캐릭터는 실제로 강한 플레이 경험을 준다.
- 경쟁 팀전은 Character Cost/편성 제한을 검토한다.
- 범용 방어보다 캐릭터별 고유 방어 규칙을 우선한다.
- 영역전개는 단순 대미지 궁극기가 아니라 Match State로 취급한다.
