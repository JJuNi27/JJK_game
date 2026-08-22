# Character / Team Select Front-End Plan

작성 기준일: 2026-08-23

## 결론

최종 게임의 캐릭터/팀 구성은 전투 중 개발용 키를 눌러 roster를 바꾸는 방식으로 제공하지 않는다.

현재의:

```text
Alpha1 / Alpha2 / Alpha3
F3 Stress Roster Toggle
```

는 prototype / developer test harness다.

최종 사용자 흐름은 대전 액션 게임의 일반적인 pre-match Character / Team Select 화면을 둔다.

```text
Mode Select
→ Character / Team Select
→ Variant / Team Slot 확인
→ Match Setup 확정
→ Battle Scene
```

---

# 이 계획이 생긴 경위

프로젝트의 기존 장기 방향에는 이미 다음이 있었다.

```text
1v1 Solo Battle
2~3인 Active / Reserve Team Battle
캐릭터별 고유성
팀 편성 전략
Naruto Storm / Sparking 계열을 포함한 3D arena fighter 참고
```

따라서 플레이어가 전투 전에 캐릭터와 팀을 구성하는 UX 자체는 장기 방향상 필요했다.

다만 2026-08-23 이전 문서에는 다음과 같은 **구체적인 프런트엔드 구현 형태**까지는 확정되어 있지 않았다.

```text
portrait/card grid
character select cursor
Active / Reserve slot assignment
variant panel
confirm flow
```

즉 `캐릭터 선택 UX가 필요하다`는 방향은 기존 구조에서 자연스럽게 전제됐지만, Naruto Storm/Sparking처럼 보이는 선택 grid를 언제 어떤 UI로 만들지까지는 명시되지 않았다.

이 문서에서 처음 명시적으로 고정한다.

---

# 최종 UX 원칙

참고 감각:

```text
Naruto Ultimate Ninja Storm 계열
Dragon Ball Sparking! ZERO 계열
```

단, 해당 게임 UI를 그대로 복제하지 않는다.

가져올 것은:

```text
한눈에 보이는 roster grid
빠른 캐릭터 탐색
선택한 Fighter의 큰 preview/info
팀 슬롯을 명확히 보여주는 flow
variant를 헷갈리지 않게 표시
대전 시작 전 최종 조합 확인
```

우리 게임의 Character Authenticity / Active-Reserve Team 구조에 맞춰 독자적인 화면으로 만든다.

---

# Gate별 처리

## Gate 5A — 지금

Character Select 화면을 먼저 만들지 않는다.

Gate 5A의 목적은:

```text
세 번째 캐릭터를 넣었을 때
Gate 4 production contract가 실제로 버티는가?
```

를 검증하는 것이다.

따라서 F3는 빠른 stress-test harness로 유지한다.

```text
GOJO + SUKUNA
↕ F3
GOJO + MEGUMI
```

이 UI를 production feature로 승격하지 않는다.

## Gate 5B — First Production-Quality Vertical Slice

여기서 첫 정식 Character / Team Select front-end를 만든다.

기술 방향:

```text
Canvas + TMP 기반
```

첫 production slice가 사용할 최소 roster:

```text
Gojo
Sukuna
Megumi
```

실제 slice의 주력 매치가 Gojo vs Sukuna여도, Gate 5A에서 검증한 Megumi 데이터를 선택 시스템이 읽을 수 있게 구성한다.

---

# Gate 5B 최소 화면 구성

## 1. Character Grid

각 카드/portrait가 다음 데이터를 사용한다.

```text
Character ID
Portrait
Display Name
Variant Label
HUD Accent
```

현재 `CharacterPresentationProfile`의 identity data를 출발점으로 삼고, 실제 portrait/model asset hook은 production asset이 들어올 때 연결한다.

## 2. Selected Fighter Panel

선택 캐릭터의:

```text
큰 portrait 또는 3D preview
이름
variant / 시대
Q / E / R / V 대표 기술
짧은 playstyle 설명
```

을 보여준다.

## 3. Match Formation

Solo:

```text
P1 Fighter
vs
Opponent Fighter
```

Team:

```text
ACTIVE
RESERVE 1
RESERVE 2 (2~3인 팀 기능이 실제 production 범위에 들어올 때)
```

Gate 5B 첫 slice에서는 현재 검증된 Active + Reserve 구조부터 UI에 연결한다.

## 4. Match Setup Data

선택 UI가 Gameplay object를 직접 켜고 끄는 식으로 만들지 않는다.

목표 경계:

```text
Character Select UI
→ Match Setup / Team Selection Data
→ Battle Scene bootstrap
→ Fighter/Team 생성 및 초기화
```

최종 production에서는 `F3 → scene reload → static stress roster bool` 같은 개발용 경로를 제거하거나 developer-only로 격리한다.

---

# Variant 지원

동일 인물의 시대/형태 차이는 별도 고려 대상이다.

예:

```text
고죠 · 현대
고죠 · 회옥·옥절 시기
스쿠나 · 시부야 유지 몸
향후 다른 스쿠나 incarnation
```

Character Select에서는 `인물 이름`만 보여주고 내부에서 몰래 변형하지 않는다.

플레이어가 현재 어떤 variant를 고르는지 명확히 알 수 있게 한다.

---

# 향후 Team Rule

현재는 먼저 재미와 구조를 검증한다.

LATER 후보:

```text
Character Cost
편성 제한
같은 인물 variant 중복 제한
Competitive preset
Story-specific roster restriction
```

이 규칙들은 Character Select UI에 하드코딩하지 않고 Match/Team rule data에서 판단하도록 한다.

---

# 현재 결론

```text
F3 = 개발용 stress-test shortcut
Character Select Grid = Gate 5B production front-end
```

Gate 5A에서는 메구미/식신으로 전투 골격을 먼저 검증한다.
Gate 5B에서는 이 검증된 roster data를 실제 캐릭터 선택 화면과 production-quality 전투로 연결한다.
