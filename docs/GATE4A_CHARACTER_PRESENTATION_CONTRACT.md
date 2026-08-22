# Gate 4A — Character Presentation Contract

작성 기준일: 2026-08-23

## 상태

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: STARTED
Gate 4A Pass 1 · Character Presentation Profile: USER VERIFIED
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 왜 먼저 이걸 하는가

Gate 3에서 고죠/스쿠나를 빠르게 검증하면서 다음 정보가 여러 파일에 반복됐다.

```text
캐릭터 이름
짧은 이름
시대/Variant 라벨
HUD 색상
CE 보조 색상
Q/E/R/V 기술명
기술별 표현 색상
```

세 번째 캐릭터를 추가할 때 이 문자열/색상을 HUD마다 다시 복사하면 캐릭터 추가 비용과 오류가 계속 늘어난다.

Gate 4A는 Gameplay를 범용화하는 작업이 아니라 `누가 전투 중인지 화면에 표현하는 데이터`만 한 곳으로 모은다.

---

# 새 Contract

파일:

```text
unity/Assets/Scripts/Player/CharacterPresentationProfile.cs
```

타입:

```text
CharacterPresentationProfile
CharacterSkillPresentation
CharacterPresentationSkillSlot
CharacterPresentationProfiles
```

현재 Profile:

```text
GojoModern
- GOJO SATORU · 현대 · 교사
- 고죠
- 현대 · 교사 / 현대
- blue HUD / CE accent
- Q 창
- E 혁
- R 허식 자
- V 무량공처

SukunaShibuyaYujiBody
- RYOMEN SUKUNA · 시부야 사변
- 스쿠나
- 시부야 사변 / 시부야
- red HUD / CE accent
- Q 해
- E 팔
- R 푸가
- V 복마어주자
```

## 의도적으로 넣지 않은 것

```text
HP
CE max/start/regen
Damage
Cooldown
Cast Time
Fuga condition
Infinity rule
Domain rule
Sure Hit
Tag rule
```

이런 값은 Presentation identity가 아니라 Gameplay Rule이므로 기존 시스템 소유권을 유지한다.

---

# Migration

## PrototypeCharacterController

```text
PresentationProfile
→ DisplayName
→ HUD Accent / Energy Accent
→ Q/E/R/V label
```

## PrototypePlayerTeamController

기존 로컬 identity helper를 줄이고 `CharacterPresentationProfiles.Get(characterId)`에서 읽도록 이관했다.

Tag/HP/CE/KO 로직은 수정하지 않았다.

## PrototypeSkillDeckHud

기존 Gojo/Sukuna 분기형 기술명/색상 하드코딩을 제거하고 Active CharacterPresentationProfile에서 Q/E/R/V label + accent를 읽도록 바꿨다.

---

# Asset Hook 정책

현재 실제 rigged Gojo/Sukuna model, portrait, Animator Controller asset이 저장소에 없다.

그래서 존재하지 않는 asset reference를 Profile에 가짜로 설계하지 않았다.

실제 asset이 들어오면 다음을 검토한다.

```text
ScriptableObject-backed CharacterPresentationProfile
Portrait/Icon reference
Presentation Prefab reference
RuntimeAnimatorController reference
Technique icon set
```

---

# 사용자 검증

2026-08-23 사용자 Unity 실제 확인:

```text
- Unity/CombatMVP 정상 실행
- 고죠 HUD / Skill Deck 정상
- T Tag 후 스쿠나 identity/HUD 정상
- Skill pulse/state 표시 정상
- HP/CE 보존 정상
- 대표 기술 회귀 없음
```

사용자 판정:

```text
정상이다 다음!
```

따라서:

```text
Gate 4A Pass 1 · Character Presentation Profile: USER VERIFIED
```

로 닫는다.

다음은 `Gate 4B — HUD Data Binding Extraction`이다.
