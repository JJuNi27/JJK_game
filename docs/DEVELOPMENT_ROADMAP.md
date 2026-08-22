# 개발 로드맵과 우선순위 판단 기준

작성 기준일: 2026-08-23

상세 게임 방향은 `docs/PROJECT_DIRECTION.md`를 기준으로 한다.

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
Gate 4 Production Pipeline Extraction: STARTING
Gate 5 Third Character Stress Test: PENDING
Gate 6 Production: PENDING
```

현재는 `First Playable + Beauty Corner prototype proof`를 통과하고, 반복 가능한 Production 구조를 추출하는 구간으로 넘어간다.

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

완료:

```text
3A Combat Feel
- Shake / Hit Stop / Flash / Impact VFX

3B Prototype Character Presentation
- Gojo/Sukuna pose / locomotion / dodge / tag entry

3C Signature Technique Presentation
- Flash / Shake / FOV / Camera Focus / Spatial VFX
- 허식 자 Beam → 청/적 합류 → 큰 보라 구체
- 푸가 / 무량공처 / 복마어주자 대표 Presentation

3D Arena + HUD
- 야간 도시 Mood scaffold
- Team HUD
- Contextual Skill Deck
- 최종 Match regression
```

Gate 3 완료는 최종 아트 완료를 뜻하지 않는다.

남은 자산 의존:

```text
실제 rigged model
Animator animation set
final environment art
Canvas/TMP final HUD
production Voice/SFX/Music
```

---

# Gate 4 — Production Pipeline Extraction · STARTING

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

순서:

```text
4A Character Presentation Contract
- Character ID
- Display / Short / Era label
- HUD Accent
- Skill Labels
- Portrait/Icon/Prefab/Animator hook

4B HUD Data Binding
- Active Fighter / HP / CE / Reserve / Skill availability

4C Technique Presentation Request
- Anticipation / Release / Impact / Active / End
- Camera / Hit Stop / VFX 요청 분리

4D VFX Lifecycle
- Spawn / Follow / Stop / Fade / Destroy

4E Animation Contract
- locomotion / dodge / basic / technique / domain / tag / hit / KO

4F Audio Event Contract
- Voice / Skill / Hit / Domain / Result
```

원칙:

```text
실제로 반복된 것만 공통화
캐릭터 고유 Gameplay Rule은 억지로 범용화하지 않음
한 번에 전체 재작성하지 않음
각 migration 뒤 Gojo/Sukuna regression
```

Gate 4부터는 여러 파일 중복 탐색과 Migration이 많아 Codex류 agent 사용 효율이 높은 구간이다.

```text
agent multi-file edit
→ Git diff review
→ 작은 migration 단위
→ 사용자 Unity test
```

---

# Gate 5 — Third Character Stress Test

후보:

```text
메구미 — 식신/소환 구조
유타 — 리카 동반체/상태 변화
```

Gate 4 contract가 구조가 다른 Fighter에서 깨지는 지점을 찾아 수정한다.

# Gate 6 — Production

그 뒤 캐릭터, 맵, 온라인, Incident Battle, PvE를 본격 확장한다.

## 캐릭터 설계 원칙

- 모든 캐릭터를 같은 강도로 평준화하지 않는다.
- 원작상 강한 캐릭터는 실제로 강한 플레이 경험을 준다.
- 경쟁 팀전은 Character Cost/편성 제한을 검토한다.
- 범용 방어보다 캐릭터별 고유 방어 규칙을 우선한다.
- 영역전개는 단순 대미지 궁극기가 아니라 Match State로 취급한다.
