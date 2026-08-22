# Gate 5A — Third Character Stress Test

작성 기준일: 2026-08-23

## 상태

```text
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED
Gate 5A Third Character Stress Test: IN PROGRESS

Pass 1 · Third Character Identity / Roster / HUD Plumbing: USER VERIFIED
Pass 2 · First Summon Gameplay / Divine Dog Contract Reuse:
REMOTE FIX APPLIED / USER RETEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

---

# 선택 캐릭터

```text
후시구로 메구미
```

선택 이유:

```text
고죠/스쿠나 = 사용자 본체가 직접 기술을 발동하는 구조
메구미 = 식신이라는 별도 전투 주체를 소환/관리하는 구조
```

따라서 Gate 4의 공통 경계가 단순 세 번째 색놀이가 아니라 구조가 다른 Fighter에서도 버티는지 확인하기 좋다.

---

# Gate 5A 성공 기준

```text
1. 새 Character ID/Profile을 추가해 기존 HUD가 수정 없이 identity를 표시한다.
2. Team state/HP/CE 보존 구조가 세 번째 Character ID에서도 동작한다.
3. Animation State/Cue contract가 세 번째 Fighter를 받아들인다.
4. 식신 술식이 Technique Presentation Request를 재사용한다.
5. 식신 VFX가 VFX Lifecycle을 재사용한다.
6. 메구미 SFX가 CombatAudioEvent를 재사용한다.
7. 고죠/스쿠나 Presentation/HUD 코드를 대량 복붙하지 않는다.
```

고유 식신 AI/소환 규칙 자체가 별도 Gameplay 코드인 것은 정상이다.

---

# Pass 1 — Identity / Roster / HUD Plumbing · USER VERIFIED

## 새 Character ID / Profile

```text
PrototypeCharacterId.MegumiStudent

FUSHIGURO MEGUMI · 도쿄고 학생
HUD: MEGUMI
Short: 메구미

Q 옥견
E 누에
R 만상
V 감합암예정
```

## Prototype Visual

```text
MegumiPrototypeAvatar
→ PrototypeMegumiAvatar
```

Gate 5A 구조 테스트용 procedural placeholder이며 실제 메구미 모델이 아니다.

`PrototypeFighterPresentationController`는 Gate 4E의 `FighterAnimationStateSnapshot.CharacterId`를 통해 메구미 visual root를 선택한다.

## Stress Roster

현재 F3는 최종 캐릭터 선택 UX가 아니라 개발/스트레스 테스트용 harness다.

```text
기본:
GOJO + SUKUNA

F3:
GOJO + MEGUMI

다시 F3:
GOJO + SUKUNA
```

기존 2인 Active/Reserve 규칙을 유지하면서 roster 데이터만 바꿔 세 번째 Character ID가 공통 경계를 통과하는지 빠르게 검증한다.

최종 Character / Team Select UX는 별도 계획 문서 `docs/CHARACTER_TEAM_SELECT_PLAN.md`를 따른다.

최종 방향:

```text
Team Size: 1~3명
MAIN + RESERVE 1 + RESERVE 2
전투 중 교대: 1 / 2로 Active ↔ 해당 Reserve Slot 교환
```

현재 T/F3/Alpha 숫자키는 prototype/developer 입력이며 최종 production 입력으로 고정하지 않는다.

## 사용자 검증

2026-08-23 사용자 확인:

```text
F3 stress roster 정상
메구미 HUD / Skill Deck 정상
T Tag 정상
메구미 이동 / 기본공격 / Dodge 정상
HP / CE 왕복 보존 정상
기존 고죠/스쿠나 회귀 없음
```

사용자 결과:

```text
정상
```

따라서:

```text
Gate 5A Pass 1: USER VERIFIED
```

---

# Pass 2 — First Summon Gameplay / Divine Dog Contract Reuse

Pass 1에서 identity/HUD/roster 경계가 통과했으므로 첫 구조 차이인 `별도 전투 주체`를 실제로 넣는다.

## Gameplay

새 파일:

```text
unity/Assets/Scripts/Player/MegumiTechniqueController.cs
unity/Assets/Scripts/Player/MegumiDivineDogSummon.cs
```

현재 prototype 규칙:

```text
Q 옥견
CE 20
Cooldown 6.5s
Summon lifetime 약 6s
Damage 16

Target Lock 대상이 있으면 우선 추적
없으면 가장 가까운 living target 탐색
접근 후 반복 공격
```

옥견 자체에는 `Health`를 붙이지 않았다.

목적:

```text
플레이어/상대 Fighter처럼 Target Lock 후보가 되거나
Opponent Team 생존 수/Victory 판정을 오염시키지 않으면서
별도 gameplay actor ownership을 검증
```

현재 Pass 2에서는 새 옥견을 다시 부르면 이전 옥견을 종료해 `1 active summon`을 유지한다.
메구미에서 Tag Out 하면 옥견을 종료한다. 이 persistence 규칙은 Gate 5A prototype 결정이며 최종 원작 충실 규칙으로 고정한 것은 아니다.

## Gate 4 Contract 재사용

### Technique Presentation Request

```text
TechniquePresentationId.DivineDog
Release
Impact
End
```

옥견 Gameplay는 camera 수치나 renderer를 직접 모른다.

### VFX Lifecycle

`PrototypeSignatureSpatialVfxController`가 `DivineDog` semantic request를 받아:

```text
소환 위치 teal burst
공격 impact teal burst
```

를 `PresentationVfxRuntime`으로 생성한다.

### Camera / HitStop

`PrototypeTechniquePresentationDirector`가 동일 request를 소비해 소환/타격의 가벼운 focus, shake, FOV, impact hit-stop을 담당한다.

### Audio Event

```text
CombatAudioEventId.DivineDog
variant 1 = summon
variant 2 = hit
```

현재 prototype audio bridge는 기존 fallback sound를 재사용한다. 실제 메구미 SFX는 Gate 5B production audio에서 교체 가능하다.

### Animation Contract

Gate 4E의 `FighterAnimationStateSource`가 Technique Presentation Request를 이미 animation-facing cue로 연결하므로 옥견 추가를 위해 별도 고죠/스쿠나 animation plumbing을 복사하지 않는다.

---

# Pass 2 첫 테스트 이슈 / 수정

2026-08-23 사용자 첫 테스트:

```text
Q 입력 시 옥견이 소환되지 않는 것으로 보임
```

코드 검토에서 `MegumiTechniqueController` runtime bootstrap이 `PrototypeCharacterController`가 이미 존재하는 시점에 의존하고 있어, runtime bootstrap 순서에 따라 fighter shell에 technique component attach가 누락될 수 있는 경로를 확인했다.

수정:

```text
MegumiTechniqueController.BootstrapAfterSceneLoad

기존:
PrototypeCharacterController를 찾아 attach

수정:
BasicAttack이 붙은 player fighter shell을 찾아 attach
```

`BasicAttack`은 현재 player fighter shell의 안정적인 marker이며, `MegumiTechniqueController`는 이후 Update에서 `PrototypeCharacterController` identity를 다시 resolve한다.

이 수정은 옥견 damage/cooldown/CE/VFX/audio 규칙을 바꾸지 않고 component attach 경로만 안정화한다.

현재 상태:

```text
REMOTE FIX APPLIED / USER RETEST PENDING
```

---

# Pass 2에서 의도적으로 하지 않은 것

```text
E 누에 Gameplay
R 만상 Gameplay
V 감합암예정 Gameplay
범용 Summon Framework 선행 구축
식신별 HP/피격 시스템
production model / animation / SFX
최종 캐릭터 선택 화면
3인 Team Runtime
```

첫 옥견 경로에서 실제 중복이 드러난 뒤에만 공통 Summon contract가 필요한지 판단한다.

---

# Pass 2 사용자 재테스트

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

Unity compile 완료 후 `CombatMVP` Play.

우선 가장 먼저:

```text
F3 → GOJO + MEGUMI
T → 메구미
Q → 옥견 등장 + CE 약 20 감소
```

이 세 단계가 통과하는지 확인한다.

그 다음:

```text
[ ] 메구미 근처에 어두운 색 prototype 옥견 등장
[ ] 소환 시 teal VFX / camera / sound 정상
[ ] TAB Target Lock 대상이 있으면 옥견이 해당 적을 우선 추적
[ ] Target Lock이 없어도 가까운 적을 찾아 이동
[ ] 적 근접 후 물기 공격으로 HP 감소
[ ] 공격 시 impact VFX / camera / sound 정상
[ ] 약 6초 후 옥견 자연 종료
[ ] cooldown 중 Q 연타로 무한 생성되지 않음
[ ] 다시 소환해도 active 옥견은 1마리만 유지
[ ] 옥견 활성 중 T Tag Out → 옥견 종료
[ ] 옥견에는 Target Lock 링이 잡히지 않음
[ ] 옥견이 상대 HUD/Victory 생존 판정을 방해하지 않음
[ ] 고죠/스쿠나 기존 전투 회귀 없음
[ ] Console 빨간 오류 없음
```

주의:

```text
E 누에 / R 만상 / V 감합암예정은 아직 미구현이 정상이다.
```

통과하면:

```text
Gate 5A Pass 2: USER VERIFIED
```

다음은 옥견 결과를 보고 두 번째 소환 구조를 추가할지, 먼저 드러난 contract gap을 보강할지 결정한다.
