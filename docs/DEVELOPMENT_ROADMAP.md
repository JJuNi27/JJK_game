# 개발 로드맵과 우선순위 판단 기준

작성 기준일: 2026-08-23

상세 게임 방향은 `docs/PROJECT_DIRECTION.md`를 기준으로 한다.

Gate 3 자산 범위와 Gate 5 세부 순서에 대한 최신 실행 기준은:

```text
docs/ROADMAP_SCOPE_REFINEMENT.md
```

를 함께 따른다.

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
Gate 4 Production Pipeline Extraction: IN PROGRESS
Gate 5A Third Character Stress Test: PENDING
Gate 5B First Production-Quality Vertical Slice: PENDING
Gate 6 Production: PENDING
```

현재는 First Playable + Beauty Corner prototype proof를 통과하고 반복 가능한 Production 구조를 추출하는 Gate 4 후반부다.

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

---

# Gate 3 — Beauty Corner Prototype Proof · USER VERIFIED

최종 회귀:

```text
docs/GATE3_FINAL_REGRESSION.md
```

2026-08-23 사용자 Unity 최종 판정:

```text
다 정상!
```

검증 완료:

```text
3A Combat Feel
- Shake / Hit Stop / Flash / Impact VFX

3B Prototype Character Presentation
- Gojo/Sukuna pose / locomotion / dodge / tag entry

3C Signature Technique Presentation
- Flash / Shake / FOV / Camera Focus / Spatial VFX
- 허식 자 청/적 합류 → 큰 보라 구체
- 푸가 / 무량공처 / 복마어주자 대표 Presentation

3D Arena + HUD
- 야간 도시 Mood scaffold
- Team HUD
- Contextual Skill Deck
- 최종 Match regression
```

중요한 scope refinement:

초기 `PROJECT_DIRECTION.md`의 Gate 3 예시에는 실제 rigged model, 실제 animation, 대표 voice까지 포함되어 있었지만 당시 저장소에 해당 production asset이 없었다.

없는 자산을 가짜 최종 구조로 굳히지 않고 Gate 3을 `Prototype Proof`로 닫았으며, 미뤄진 production-quality 자산 연결은 Gate 5B에서 명시적으로 회수한다.

남은 자산 의존:

```text
실제 rigged model
Animator animation set
final environment art
Canvas/TMP final HUD
production Voice/SFX/Music
```

상세:

```text
docs/ROADMAP_SCOPE_REFINEMENT.md
```

---

# Gate 4 — Production Pipeline Extraction · IN PROGRESS

계획:

```text
docs/GATE4_PRODUCTION_PIPELINE_PLAN.md
```

목표:

```text
세 번째 Fighter를 추가할 때
고죠/스쿠나 Prototype 코드를 복사하지 않고
공통 제작 절차를 반복 가능하게 만들기
```

현재 상태:

```text
4A Character Presentation Contract: USER VERIFIED
4B HUD Data Binding Extraction: USER VERIFIED
4C Technique Presentation Request: USER VERIFIED
4D VFX Lifecycle Contract: USER VERIFIED
4E Animation Contract: USER VERIFIED
4F Audio Event Contract
- Pass 1: USER VERIFIED
- Pass 2: REMOTE IMPLEMENTED / USER TEST PENDING
```

Gate 4 원칙:

```text
실제로 반복된 것만 공통화
캐릭터 고유 Gameplay Rule은 억지로 범용화하지 않음
한 번에 전체 재작성하지 않음
각 migration 뒤 Gojo/Sukuna regression
실제 asset이 없는 부분은 가짜 production asset 구조로 굳히지 않음
```

Codex류 agent는 Gate 번호 때문에 의무적으로 쓰는 것이 아니라 **여러 파일에 걸친 탐색/마이그레이션 규모일 때 사용하는 가속 도구**다.

```text
Codex/agent 사용 가능 + multi-file 구조 작업
→ agent edit
→ Git diff review
→ 사용자 Unity test

작은 변경 또는 agent 미사용 환경
→ 직접 수정
→ 동일하게 diff/Unity regression
```

---

# Gate 5A — Third Character Stress Test

이 단계는 최초 Gate 5의 목적을 그대로 유지한다.

후보:

```text
메구미 — 식신/소환 구조
유타 — 리카 동반체/상태 변화
```

Gate 4 contract가 구조가 다른 Fighter에서 깨지는 지점을 찾아 수정한다.

성공 기준:

```text
새 캐릭터 Gameplay는 고유하게 작성
HUD / Camera / VFX / Animation / Audio 경계는 최대한 재사용
고죠/스쿠나 코드를 대량 복붙하지 않음
```

---

# Gate 5B — First Production-Quality Vertical Slice

2026-08-23 실행 계획을 명확하게 하기 위해 추가한 세부 단계다.

Gate 5A 구조 검증 뒤, 최초 Gate 3에서 자산 부재로 미룬 production-quality 부분을 대표 한 판에 실제 연결한다.

대표 목표:

```text
고죠 vs 스쿠나 한 매치
실제 rigged model
Animator + 실제 animation set
production-grade 술식 VFX
Canvas/TMP final-style HUD
production SFX / Voice / Music
대표 맵 art / lighting / material / post process
```

목표 체감:

```text
"이제 실제 주술회전 게임 같다"
```

이 단계는 방향 변경이 아니라 초기 Beauty Corner의 미뤄진 자산 품질 목표를 회수하는 단계다.

---

# Gate 6 — Production

Gate 5A에서 pipeline 구조를 검증하고 Gate 5B에서 실제 production 품질을 검증한 뒤 본격 확장한다.

```text
캐릭터 로스터 확장
맵 확장
게임 모드 확장
온라인 구조
Incident Battle
PvE / Story
```

---

## 캐릭터 설계 원칙

- 모든 캐릭터를 같은 강도로 평준화하지 않는다.
- 원작상 강한 캐릭터는 실제로 강한 플레이 경험을 준다.
- 경쟁 팀전은 Character Cost/편성 제한을 검토한다.
- 범용 방어보다 캐릭터별 고유 방어 규칙을 우선한다.
- 영역전개는 단순 대미지 궁극기가 아니라 Match State로 취급한다.
