# 개발 로드맵과 우선순위 판단 기준

작성 기준일: 2026-08-23

상세 게임 방향은 `docs/PROJECT_DIRECTION.md`를 기준으로 한다.

## 판단 질문

새 제안이 들어오면 다음 순서로 평가한다.

1. 현재 전투 루프의 정확성이나 테스트 품질을 높이는가?
2. 다음 캐릭터에도 재사용될 실제 공통 규칙인가?
3. 지금 하지 않으면 이후 코드를 크게 다시 작성해야 하는가?
4. 반대로 지금 하면 모델·연출·콘텐츠 변경 때문에 반복 작업이 커지는가?
5. 원작 조건인지, 공식 게임의 각색인지, 우리 게임용 편의인지 구분됐는가?
6. 최종 목표인 캐릭터 중심 3D 아레나/팀 전투 구조에 맞는가?

판정:

```text
NOW
FOUNDATION_ONLY
LATER
REJECT
```

---

# 현재 프로젝트 단계

```text
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner: FINAL REGRESSION
Gate 4 Production Pipeline Extraction: PREPARED / PENDING
Gate 5 Third Character Stress Test: PENDING
Gate 6 Production: PENDING
```

대형 스튜디오식 표현으로는 `First Playable을 넘어서 Vertical Slice prototype을 닫는 구간`에 가깝다.

---

# 잠정 게임 방향

```text
캐릭터 중심 3D 아레나 액션 격투

Core A: 1v1 Solo Battle
Core B: 2~3인 Team Battle
- 전장에는 기본적으로 한 명 활성
- 예비 캐릭터와 교대
- 캐릭터 HP/주력 독립 보존

LATER:
- 4~8인 Incident Battle
- 환경 파괴 강화
- PvPvE/사건전
```

20~50인 상시 FFA나 오픈월드 MMO는 메인 방향으로 잡지 않는다.

---

# 개발 방법 — Risk-Driven Vertical Slice

## Gate 1 — Core Combat Proof · USER VERIFIED

현대 고죠와 시부야 스쿠나의 핵심 전투 규칙, 방어/영역/주력 상호작용을 사용자 Unity에서 검증했다.

## Gate 2 — Match Architecture Proof · USER VERIFIED

```text
Player Team
- Active + Reserve
- T 수동 Tag
- HP / CE 독립 상태
- KO Auto Tag
- 마지막 팀원 KO에만 DEFEAT

Opponent Team
- Active + Reserve
- 첫 KO 뒤 Reserve 자동 입장
- Target Lock 자동 승계
- 마지막 팀원 KO에만 VICTORY

Encounter Mode
- Training · Multi Curse
- Team Battle
```

현재 T/F2는 개발용 `GAME_ORIGINAL` 임시 입력이다.

지원 공격, 합동기, 3인 팀 완성형 UI, Character Cost, 온라인은 아직 만들지 않는다.

상세:

```text
docs/TEAM_MATCH_GATE2.md
docs/GATE2B_OPPONENT_TEAM.md
docs/GATE2B_ENEMY_HUD_CLEANUP.md
docs/GATE2B_TARGET_HANDOFF.md
```

---

# Gate 3 — Beauty Corner · FINAL REGRESSION

상세:

```text
docs/GATE3_BEAUTY_CORNER.md
docs/GATE3_FINAL_REGRESSION.md
```

현재 상태:

```text
3A Combat Feel: USER VERIFIED
- 기본 공격 Shake
- Hit Stop
- Flash
- Impact VFX

3B Character Presentation: USER VERIFIED
- Gojo/Sukuna prototype pose
- locomotion lean
- dodge presentation
- tag entry

3C Signature Technique Presentation: USER VERIFIED
- Flash / Shake / FOV
- Camera World Focus
- Spatial VFX
- Purple / Fuga / Domain presentation
- 허식 자: Beam → 청/적 합류 → 큰 보라 구체

3D Arena + HUD Polish
- Arena Mood: USER VERIFIED
- Team HUD Visual Integration: USER VERIFIED
- Contextual Skill Deck: visual feedback confirmed / final regression pending
```

Gate 3 종료는 최종 아트 완성을 의미하지 않는다.

아직 자산 의존:

```text
실제 rigged Gojo/Sukuna model
Animator animation set
final environment art
Canvas/TMP final HUD
production Voice/SFX/Music
```

Gate 3은 이 자산을 연결할 전투 감각/표현 타이밍/UI 정보 구조를 증명한다.

최종 한 세션 회귀가 통과하면:

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
```

로 닫는다.

---

# Gate 4 — Production Pipeline Extraction

계획:

```text
docs/GATE4_PRODUCTION_PIPELINE_PLAN.md
```

Beauty Corner에서 실제 반복된 것만 공통화한다.

우선순위:

```text
4A Character Presentation Contract
- Character ID / Display / Accent / Skill Labels / asset hooks

4B HUD Data Binding
- Fighter identity / HP / CE / Reserve / Skill availability

4C Technique Presentation Request
- Anticipation / Release / Impact / Active / End
- Camera / Hit Stop / VFX 요청 분리

4D VFX Lifecycle
- Spawn / Follow / Stop / Fade / Destroy

4E Animation Contract
- locomotion / dodge / basic / technique / domain / tag / hit / KO

4F Audio Event Contract
- Voice / Technique / Hit / Domain / Result
```

거대한 범용 Ability System을 미리 만들지 않는다.

Gate 4부터는 여러 파일에 걸친 중복 탐색과 Migration이 많기 때문에 Codex류 agent 사용 효율이 높다.

```text
agent multi-file edit
→ Git diff review
→ 작은 migration 단위 적용
→ 사용자 Unity regression
```

---

# Gate 5 — Third Character Stress Test

Gate 4 파이프라인을 구조가 다른 세 번째 캐릭터로 검증한다.

후보:

```text
메구미 — 식신/소환 구조
유타 — 리카 동반체/상태 변화
```

세 번째 Fighter가 공통 파이프라인을 깨뜨리는 지점이 나오면 Gate 4 계약을 수정한다.

---

# Gate 6 — Production

그 뒤 캐릭터, 맵, 온라인, 사건전, PvE를 본격 확장한다.

## 캐릭터 설계 원칙

- 모든 캐릭터를 같은 강도로 평준화하지 않는다.
- 원작상 강한 캐릭터는 실제로 강한 플레이 경험을 준다.
- 경쟁 팀전은 Character Cost/편성 제한 방식으로 균형을 잡는 것을 검토한다.
- 범용 방어는 최소화하고 캐릭터별 고유 방어 규칙을 적극 사용한다.
- 영역전개는 단순 큰 피해 궁극기가 아니라 전투 규칙을 바꾸는 Match State로 취급한다.
