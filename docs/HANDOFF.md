# JJK Game 프로젝트 인수인계서

작성 기준일: 2026-08-23

> 새 ChatGPT 대화에서는 이 문서와 `docs/PROJECT_DIRECTION.md`, `docs/ORIGINAL_FIDELITY_POLICY.md`, `docs/CANON_SKILL_RULES.md`, `docs/DEVELOPMENT_ROADMAP.md`, `docs/GATE3_FINAL_REGRESSION.md`, `docs/GATE4_PRODUCTION_PIPELINE_PLAN.md`, 최신 코드를 먼저 읽는다.

---

# 1. 환경 / 작업 방식

```text
프로젝트: 주술사 되어보기
저장소: JJuNi27/JJK_game
브랜치: master
로컬: D:\GitHub\JJK_game
Unity: D:\GitHub\JJK_game\unity
Unity 6.3 LTS 6000.3.20f1 / URP
```

사용자 pull:

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

작업 원칙:

```text
조사/설계
→ 작은 변경은 master 직접 수정
→ 여러 파일 구조 변경은 agent/Codex 활용 가능
→ Git diff 검토
→ 사용자 pull
→ 사용자 Unity 실제 테스트
→ USER VERIFIED만 완료 처리
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

속도 원칙:

```text
관련 작업은 2~5개 묶어서 검증
단, 핵심 Gameplay/컴파일 위험 migration은 작은 단위로 검증
```

---

# 2. 게임 방향

```text
Character-Authentic 3D Arena Action Fighter
```

```text
기본: 1v1
핵심 확장: 2~3인 Active/Reserve Team Battle
후속: 4~8인 Incident Battle
```

원작 우선. 지원 공격/합동기/온라인/완전 동적 파괴는 Beauty Corner 이전에 확장하지 않았으며 Gate 4에서도 먼저 Production contract를 정리한다.

---

# 3. Gate 상태

```text
Gate 0 Vision Lock: 충분히 진행됨
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: STARTING
Gate 5 Third Character Stress Test: PENDING
Gate 6 Production: PENDING
```

Gate 3 최종 회귀:

```text
docs/GATE3_FINAL_REGRESSION.md
```

Gate 4 계획:

```text
docs/GATE4_PRODUCTION_PIPELINE_PLAN.md
```

---

# 4. Gate 1 — Core Combat · USER VERIFIED

현대 고죠:

```text
이동 / 기본 3타 / 회피
Q 창
E 혁
R 허식 자
V 무량공처
무하한
영역전연 우회
DomainSureHit
술식 번아웃
```

시부야 스쿠나:

```text
Q 해
E 팔
R 푸가
V 복마어주자
영역 밖 푸가
영역 안 광역 푸가
```

---

# 5. Gate 2 — Match Architecture · USER VERIFIED

Player Team:

```text
Gojo ↔ Sukuna T Tag
HP / CE 독립 보존
Reserve freeze
Action Lock Tag 차단
첫 KO Auto Tag
마지막 Player KO에만 DEFEAT
Tag 시 콤보 초기화
카메라/Target Lock 유지
```

Opponent Team:

```text
F2 Training / Team Battle
Active + Reserve
첫 KO → Reserve Entry
첫 KO에는 VICTORY 없음
Reserve 등장 시 Target Lock 자동 승계
마지막 상대 KO에만 VICTORY
```

---

# 6. Gate 3 — Beauty Corner Prototype Proof · USER VERIFIED

2026-08-23 최종 한 세션 회귀 후 사용자 판정:

```text
다 정상!
```

## 3A Combat Feel

```text
Shake
Hit Stop
Hit Flash
Impact Burst
1 < 2 < 3 FINISH 강조
```

## 3B Character Presentation

```text
Gojo/Sukuna prototype pose
locomotion lean
dodge presentation
tag entry
```

실제 rigged model/Animator는 아직 없음.

## 3C Signature Presentation

```text
Flash / Shake / FOV
Camera World Focus
Spatial VFX
푸가 / 무량공처 / 복마어주자 대표 연출
```

허식 「자」:

```text
기존 Beam 제거
청색 + 적색 구체 수렴
→ 큰 보라 구체 전방 이동
```

Prototype 부채:

```text
Visual = 이동 구체
Damage = 즉시 Capsule
```

## 3D Arena + HUD

```text
야간 도심 Mood scaffold
비충돌 건물 실루엣
Arena ring / Neon / Fog
Player/Opponent Team HUD
Contextual Skill Deck
```

Skill Deck:

```text
Gojo: Q 창 / E 혁 / R 허식 자 / V 무량공처
Sukuna: Q 해 / E 팔 / R 푸가 / V 복마어주자
READY / DODGE / CASTING / DOMAIN INPUT / DOMAIN ACTIVE / BURNOUT / DISABLED
```

최종 회귀에서 Tag, HP/CE, Purple, Fuga, Domain, Arena, HUD, Opponent Team, Victory/Defeat까지 한 번에 정상 확인.

---

# 7. Gate 3 완료가 의미하지 않는 것

아직 Production 자산 의존:

```text
실제 rigged Gojo/Sukuna model
Animator animation set
final environment art
Canvas/TMP final HUD
production Voice/SFX/Music
```

Gate 3은 이 자산을 연결할 전투 감각, Presentation timing, 카메라/VFX 규칙, HUD 정보 구조가 성립함을 증명했다.

---

# 8. 현재 바로 할 일 — Gate 4A

```text
Gate 4 Production Pipeline Extraction: STARTING
NEXT: 4A Character Presentation Contract
```

먼저 여러 파일에 중복된 다음 정보를 하나의 Presentation data contract로 추출한다.

```text
Character ID
Display Name
Short Name
Era / Variant Label
HUD Accent
Q/E/R/V Skill Labels
Portrait/Icon hook
Presentation Prefab / Animator hook
```

목표 후보:

```text
CharacterPresentationProfile
```

주의:

```text
HP / CE
Damage
Domain Rule
고유 술식 Gameplay
```

은 이 Profile에 넣지 않는다.

첫 migration 뒤 반드시 확인:

```text
Gojo/Sukuna 이름/색상/Skill Deck 동일
T Tag identity 전환 정상
HP/CE/기술/Domain Gameplay 변화 없음
```

---

# 9. Gate 4 이후

```text
4A Character Presentation Contract
4B HUD Data Binding
4C Technique Presentation Request
4D VFX Lifecycle
4E Animation Contract
4F Audio Event Contract
```

Gate 4는 다중 파일 중복 탐색과 migration이 많아 Codex류 agent 활용 가치가 높은 구간이다.

```text
agent multi-file work
→ Git diff review
→ 작은 migration
→ 사용자 Unity regression
```

이 대화에 별도 로컬 Codex/Unity Editor 제어 도구가 없다면 실행했다고 가장하지 않는다.

Gate 5에서는 메구미 또는 유타처럼 구조가 다른 세 번째 Fighter로 Gate 4 contract를 스트레스 테스트한다.
