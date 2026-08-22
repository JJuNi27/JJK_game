# Roadmap Scope Refinement — Prototype Proof → First Production Slice

작성 기준일: 2026-08-23

## 목적

`docs/PROJECT_DIRECTION.md`의 장기 게임 방향과 Risk-Driven Vertical Slice 원칙은 유지한다.

다만 최초 Gate 3 설명과 실제 자산 상황 사이에 생긴 차이를 숨기지 않고 명시적으로 기록한다.

---

# 최초 계획과 실제 Gate 3의 차이

초기 `PROJECT_DIRECTION.md`의 Gate 3 Beauty Corner 예시는 다음을 포함했다.

```text
대표 전투장 한 구역
실제 고죠 모델
실제 스쿠나 모델
기본 이동/공격/회피 애니메이션
허식 자 / 푸가 / 무량공처 / 복마어주자
대표 피격/카메라/사운드/보이스
```

하지만 Gate 3 진행 시점의 저장소에는 실제 rigged Gojo/Sukuna model과 production Animator/Animation Clip이 없었다.

없는 자산을 임시 구조로 최종 자산처럼 굳히지 않기 위해 Gate 3의 검증 범위를 다음으로 좁혔다.

```text
Gate 3 Beauty Corner Prototype Proof
- Combat Feel
- Prototype Character Presentation
- Signature Technique Presentation
- Arena Mood
- HUD Information Hierarchy
```

따라서 Gate 3은 `실제 모델/애니메이션/최종 HUD/production audio까지 완료`한 것이 아니다.

이것은 게임 방향 변경이 아니라 **자산 의존 작업의 실행 시점을 뒤로 미룬 scope refinement**다.

---

# 유지되는 핵심 방향

다음은 바뀌지 않는다.

```text
캐릭터 중심 3D 아레나 액션 격투
Character Authenticity
Asymmetric Combat
Domain as Match State
Spectacle + Destruction
Standard Input + optional Hand Sign / Voice immersion
1v1 core + 2~3인 Active/Reserve Team Battle
후속 Incident Battle
```

개발 방법도 그대로 유지한다.

```text
Risk-Driven Vertical Slice Production
→ 위험한 전투/팀 구조를 먼저 증명
→ 작은 대표 구간의 표현을 증명
→ 반복된 제작 규격만 추출
→ 다른 구조의 캐릭터로 stress test
→ production-quality slice
→ 본격 production
```

---

# 현재 Gate 순서 — 실행 기준

## Gate 4 — Production Pipeline Extraction

Gate 3 prototype에서 실제 반복된 경계만 추출한다.

```text
Character Presentation Contract
HUD Data Binding
Technique Presentation Request
VFX Lifecycle
Animation Contract
Audio Event Contract
```

실제 asset이 없는데 Animator/VFX Graph/최종 UI 규격을 상상으로 과설계하지 않는다.

## Gate 5A — Third Character Stress Test

이 단계는 **초기 Gate 5의 원래 목적 그대로**다.

```text
고죠/스쿠나와 구조가 다른 세 번째 캐릭터 추가
→ Gate 4 contract가 실제로 재사용되는지 검증
→ 복붙/특수예외가 많이 필요하면 Gate 4 경계 보강
```

후보:

```text
메구미 — 식신/소환 구조
유타 — 리카 동반체/상태 변화
```

## Gate 5B — First Production-Quality Vertical Slice

이 이름은 2026-08-23에 **명시적으로 추가한 세부 단계**다.

목적은 최초 Gate 3 목표 중 자산 부재로 미뤄 둔 production-quality 부분을, Gate 4/5A 구조 검증 뒤 한 판에 실제로 연결하는 것이다.

대표 범위 예:

```text
고죠 vs 스쿠나 한 매치
실제 rigged character model
Animator + 실제 animation set
production-grade technique VFX
Canvas/TMP final-style HUD
production SFX / Voice / Music
대표 맵 art / material / lighting / post process
```

완료 기준은 단순히 기능이 동작하는 것이 아니라:

```text
"이제 실제 주술회전 게임처럼 보이고 느껴진다"
```

라는 첫 production-quality vertical slice를 확보하는 것이다.

Gate 5B는 새 게임 방향이 아니라 **초기 Gate 3에서 미뤄진 자산 품질 목표를 다시 회수하는 단계**다.

## Gate 6 — Production

Gate 5A에서 구조를 검증하고 Gate 5B에서 실제 production 품질을 검증한 뒤, 같은 파이프라인으로 본격 확장한다.

```text
캐릭터 로스터
맵
추가 모드
온라인
Incident Battle
PvE / Story
```

---

# 문서 우선순위

장기 게임 방향은 계속:

```text
docs/PROJECT_DIRECTION.md
```

을 따른다.

단, **Gate 3의 실제 완료 범위와 Gate 5B의 production-quality 자산 시점**에 대해서는 이 문서가 초기 `PROJECT_DIRECTION.md`의 Gate 3 예시보다 최신 실행 기준이다.

즉 문서를 조용히 덮어쓰지 않고, 왜 순서가 세분화됐는지 기록을 남긴다.
