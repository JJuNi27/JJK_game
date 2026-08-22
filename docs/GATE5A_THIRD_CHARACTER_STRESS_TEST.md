# Gate 5A — Third Character Stress Test

작성 기준일: 2026-08-23

## 상태

```text
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED
Gate 5A Third Character Stress Test: FINAL CLEANUP / USER REGRESSION PENDING

Pass 1 · Third Character Identity / Roster / HUD Plumbing: USER VERIFIED
Pass 2 · First Summon Gameplay / Divine Dog Contract Reuse: USER VERIFIED
Final Cleanup · Remove temporary summon diagnostics/bootstrap dependency:
REMOTE IMPLEMENTED / USER TEST PENDING
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

메구미 자체를 버릴 테스트 더미로 취급한다는 뜻은 아니다.
Gate 5A에서 메구미를 먼저 사용하는 목적이 `세 번째 캐릭터 + 소환체 구조 스트레스 테스트`라는 뜻이다.

---

# Gate 5A 성공 기준

```text
1. 새 Character ID/Profile을 추가해 기존 HUD가 identity를 표시한다.
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

# Pass 2 — First Summon Gameplay / Divine Dog Contract Reuse · USER VERIFIED

Pass 1에서 identity/HUD/roster 경계가 통과했으므로 첫 구조 차이인 `별도 전투 주체`를 실제로 넣었다.

## Gameplay

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

### VFX Lifecycle

동일 semantic request를 기존 `PresentationVfxRuntime`이 소비한다.

### Camera / HitStop

동일 request를 기존 technique presentation director가 소비한다.

### Audio Event

```text
CombatAudioEventId.DivineDog
variant 1 = summon
variant 2 = hit
```

### Animation Contract

기존 `FighterAnimationStateSource`가 Technique Presentation Request를 animation-facing cue로 연결한다.

따라서 옥견 추가를 위해 고죠/스쿠나 전용 Presentation/HUD plumbing을 복사하지 않았다.

---

# Pass 2 첫 테스트 이슈와 수정

첫 테스트:

```text
Q 입력 시 옥견 소환 안 됨
CE도 감소하지 않음
```

첫 수정은 runtime bootstrap attach 경로를 바꿨지만 해결되지 않았다.

두 번째 진단에서 메구미가 실제 활성화되는 `PrototypeCharacterController.ApplyMegumi()`가 기술 컴포넌트 존재를 보장하지 않는 경로를 제거했다.

최종 수정:

```text
ApplyMegumi()
→ MegumiTechniqueController.GetOrCreate(gameObject)
→ enabled = true

ApplyGojo() / ApplySukuna()
→ MegumiTechniqueController disabled
```

임시 Q diagnostic HUD를 추가해 입력/Action Gate/CE 단계도 확인 가능하게 했다.

## 사용자 검증

2026-08-23 사용자 결과:

```text
"잘 나와서 겁나게 물어뜯는다"
```

확인된 핵심:

```text
Q 입력 경로 정상
CE 소비 경로 정상
옥견 생성 정상
옥견 추적/근접 공격 정상
Damage 적용 정상
```

따라서:

```text
Gate 5A Pass 2: USER VERIFIED
```

---

# Final Cleanup — REMOTE IMPLEMENTED / USER TEST PENDING

Pass 2 원인이 확인됐으므로 진단용 임시 코드를 제거했다.

제거:

```text
Health 전체에 MegumiTechniqueController를 붙이던 runtime bootstrap
Q RECEIVED / BLOCKED / SUMMONED 임시 IMGUI diagnostic overlay
```

유지하는 안정적인 production-candidate hookup:

```text
PrototypeCharacterController.ApplyMegumi()
→ MegumiTechniqueController.GetOrCreate()
→ enabled = true
```

이제 적/훈련 저주령의 Health 오브젝트에 불필요한 메구미 기술 컴포넌트를 붙이지 않는다.

---

# Gate 5A에서 의도적으로 하지 않은 것

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

Gate 5A의 목적은 메구미를 완성하는 것이 아니라 `새 캐릭터 + 구조가 다른 별도 combat actor`가 Gate 4 경계를 통과하는지 검증하는 것이다.
옥견 한 경로에서 그 핵심 리스크가 실제로 검증됐으므로 누에/만상까지 prototype으로 미리 만들어 Gate 5A를 늘리지 않는다.

---

# Final Cleanup 사용자 회귀

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

빠른 확인:

```text
[ ] F3 → GOJO + MEGUMI
[ ] T → 메구미
[ ] Q → CE 약 20 감소 + 옥견 정상 등장
[ ] 옥견이 적을 추적하고 물어뜯어 HP 감소
[ ] 약 6초 후 자연 종료
[ ] 옥견 활성 중 T → 옥견 종료
[ ] 고죠/스쿠나 기존 전투 회귀 없음
[ ] Console 빨간 오류 없음
```

통과하면:

```text
Gate 5A Third Character Stress Test: USER VERIFIED / CLOSED
NEXT: Gate 5B First Production-Quality Vertical Slice
```

Gate 5B 첫 작업은 `Character / Team Select + Match Setup 데이터 경계`부터 시작하고,
그 뒤 실제 모델/Animator/VFX/UI/Audio asset 교체가 가능한 production slice로 이어간다.
