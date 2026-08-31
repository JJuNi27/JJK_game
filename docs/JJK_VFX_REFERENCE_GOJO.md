# JJK VFX Reference Sheet — Gojo Pass 8R-1

작성 기준일: 2026-09-01

목적:
Gate 5B Pass 8의 사용자 시각 검수 실패를 바탕으로, 현대 고죠의 핵심 술식 VFX를
`reference-first` 방식으로 재작업하기 위한 구현 기준을 고정한다.

이 문서는 원작/애니/공식 게임을 1:1 복제하기 위한 문서가 아니다.
기술의 시각 정체성, 모션 언어, 레이어 구조, 전장 가독성을 추출해
현재 3D arena prototype에 맞게 재해석하기 위한 기준이다.

## Source priority

1. TV 애니 / 공식 Jujutsu Kaisen 영상
2. Jujutsu Kaisen Phantom Parade 공식 캐릭터/필살기 영상
3. Jujutsu Kaisen Cursed Clash 공식 3D 전투 영상
4. fan game / community material은 3D 가독성 보조 참고만 사용

공식 reference examples:
- Phantom Parade Official: "Hollow Purple" Satoru Gojo
- Phantom Parade Official: "The Inside of Limitless" Satoru Gojo
- Phantom Parade official anniversary gallery: Gojo special-move videos
- Cursed Clash official site / overview: anime-like dynamic cursed techniques and Domains that alter the battlefield

---

# 1. Cursed Technique Lapse: Blue / 순전 「창」

## 반드시 남아야 하는 silhouette

Blue는 파란 spark burst가 아니다.

화면에서 가장 먼저 읽혀야 하는 것은:

```text
작고 강하게 읽히는 BLUE ORB / CORE
+
그 orb를 중심으로 주변이 끌려 들어가는 수렴장
```

기존 Pass 8에서 blue ring/orb 존재감이 사라지고
cyan streak가 사방으로 튀어 사용자에게 전기/얼음 burst처럼 보인 것은 실패였다.

## motion language

```text
outside
  ↓
  ↓
[ BLUE CORE ]
  ↑
  ↑
outside
```

- particle velocity의 주 방향은 center-in
- core 주변에 짧은 orbit / compression arc 허용
- 바깥쪽으로 터지는 radial spark는 최소
- field 전체가 사라지지 않고 blue core / circular boundary가 읽혀야 함
- target pull gameplay와 시각 방향이 일치해야 함

## visual layering

1. Dense blue orb
2. cyan rim/glow
3. inward motes/streaks
4. subtle circular convergence boundary
5. optional short distortion-like arcs (실제 screen distortion shader는 이번 pass에서 필수 아님)

---

# 2. Cursed Technique Reversal: Red / 반전 「혁」

## 반드시 남아야 하는 silhouette

Red 역시 spark cloud가 아니라

```text
RED ORB / CORE
→
강한 outward repulsion
```

이 먼저 읽혀야 한다.

기존 Pass 8처럼 작은 빨간 점 + 몇 개의 짝대기로는
척력 signature가 보이지 않는다.

## motion language

Blue와 정반대여야 한다.

```text
      ↑
   ↗     ↖
← [ RED CORE ] →
   ↘     ↙
      ↓
```

- compact red sphere/orb를 유지
- release 순간 얇은 circular shock front
- outward streak는 중심에서 멀어지는 방향
- projectile 이동 중 orb silhouette가 사라지면 안 됨
- impact는 fire explosion보다 repulsive shockwave가 우선

## visual layering

1. red orb
2. bright crimson rim
3. outward radial wave
4. short trailing fragments
5. impact shock ring / expanding distortion cue

---

# 3. Hollow Purple / 허식 「자」

## 핵심

Purple은 단순 보라색 projectile가 아니라
Blue와 Red가 결합해 생기는 상위 signature 기술이다.

현재 검증된 gameplay/presentation timing:

```text
MergeDuration = 0.24s
LaunchDuration = 0.78s
TravelDistance = 18m
```

이 값은 변경하지 않는다.

## 반드시 보이는 sequence

```text
BLUE ORB        RED ORB
    \          /
     \        /
      \      /
       PURPLE FORMATION
             ↓
      DENSE PURPLE BODY
             ↓
           TRAVEL
```

- merge 전에는 blue와 red가 각각 독립된 구체로 읽혀야 함
- 합쳐지는 순간 violet ignition / compression
- 형성 후 purple 본체는 sphere/energy mass silhouette 유지
- 주변 particle는 본체를 보강해야지 본체를 대신하면 안 됨
- 기존 prototype beam/line이 보라색 본체보다 더 눈에 띄면 실패

## visual layering

Merge:
- blue orb + inward blue motes
- red orb + outward/unstable red motes

Formation:
- compact violet core
- magenta/deep purple shell
- short ignition flash

Travel:
- clear purple body
- short residual motes
- restrained trail
- no long laser beam silhouette

---

# 4. Unlimited Void / 무량공처

## 핵심

Unlimited Void는 particle burst가 아니라
`match-space / environment presentation` 이다.

기존 `UnlimitedVoidPrototypeVisual`의
큰 cyan/purple rings + 약간의 particles 방식은 사용자 검수에서 실패했다.

영역 발동 시 플레이어가 느껴야 하는 것은:

```text
"아레나 위에 이펙트가 생겼다"
가 아니라
"전장 자체가 무량공처 내부로 바뀌었다"
```

## presentation layers

1. Arena background suppression
   - 기존 arena geometry는 완전히 삭제하지 않아도 되지만
     어둡게 가라앉히거나 시각 우선순위를 크게 낮춘다.

2. Void backdrop
   - near-black / deep navy void
   - 충분한 depth가 느껴지는 large enclosure / backdrop

3. star / information field
   - 작은 luminous point
   - 크기/깊이/속도가 서로 다름
   - blue/cyan/pale-violet
   - 폭발 debris처럼 radial하게 튀지 않음

4. central infinite-space focal element
   - 단순 3개 ring 대신 깊이감을 주는 중심 시각 요소
   - 거대한 암흑 중심 / distant luminous field / layered celestial cue 중
     현재 primitive/material 범위에서 가장 안정적인 구현 선택

5. restrained domain pulse
   - 영역 시작 순간만 짧은 expansion
   - 3초 Active 동안 계속 회전하는 debug rings로 화면을 채우지 않음

## gameplay visibility

- player / opponent silhouette는 계속 식별 가능
- HUD는 항상 읽힘
- target lock도 보임
- opaque fog로 전장을 지우지 않음

---

# 5. Implementation rule

이번 재작업에서 signature 기술은
범용 `ProductionParticleVfxInstance`의 색/particle-count 분기만으로 해결하지 않는다.

권장:

```text
Presentation contract
  ├─ Generic / Basic particle effects
  │    └─ ProductionParticleVfxInstance
  │
  ├─ GojoBluePresentation
  ├─ GojoRedPresentation
  ├─ HollowPurplePresentation
  └─ UnlimitedVoidEnvironmentPresentation
```

공통 runtime contract는 유지하되
signature visual은 전용 presenter / instance를 허용한다.

## 절대 변경 금지

- damage
- CE
- cooldown
- hitbox
- pull/push
- attack timing
- Purple timing
- Domain duration/rule/stun
- Target Lock
- camera gameplay
- input
- HUD
- CharacterSelect

---

# 6. Pass 8R-1 user acceptance

Blue:
- 파란 orb가 확실히 보인다.
- 주위가 그 orb로 빨려 들어간다.
- 전기 burst처럼 보이지 않는다.

Red:
- 빨간 orb가 확실히 보인다.
- release가 척력/충격파로 읽힌다.
- 작은 빨간 spark projectile처럼 보이지 않는다.

Purple:
- Blue + Red가 실제 구체로 보인 뒤 Purple로 합쳐진다.
- Purple body가 확실하다.
- 기존보다 변화가 눈에 띈다.

Unlimited Void:
- 기존 경기장이 아닌 별도의 영역 내부처럼 보인다.
- 큰 debug ring 몇 개가 주연이 아니다.
- Domain activation이 signature event로 느껴진다.
