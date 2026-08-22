# Gate 5A — Third Character Stress Test

작성 기준일: 2026-08-23

## 상태

```text
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED
Gate 5A Third Character Stress Test: USER VERIFIED / CLOSED

Pass 1 · Third Character Identity / Roster / HUD Plumbing: USER VERIFIED
Pass 2 · First Summon Gameplay / Divine Dog Contract Reuse: USER VERIFIED
Final Cleanup · Remove temporary summon diagnostics/bootstrap dependency: USER VERIFIED

NEXT: Gate 5B First Production-Quality Vertical Slice
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

---

# 선택 캐릭터

```text
후시구로 메구미
```

메구미 자체를 버릴 테스트 더미로 취급한다는 뜻은 아니다.
Gate 5A에서 메구미를 먼저 사용한 목적이 `세 번째 캐릭터 + 소환체 구조 스트레스 테스트`였다는 뜻이다.

고죠/스쿠나는 본체가 직접 기술을 발동하는 구조지만 메구미는 식신이 별도 combat actor가 되므로 Gate 4 contract의 범용성을 검증하기 적합했다.

---

# Gate 5A 성공 기준 결과

```text
[PASS] 새 Character ID/Profile을 기존 HUD가 수용
[PASS] Team state / HP / CE 보존
[PASS] Animation State/Cue가 세 번째 Fighter를 수용
[PASS] 식신이 Technique Presentation Request 재사용
[PASS] 식신 VFX가 VFX Lifecycle 재사용
[PASS] 메구미 SFX가 CombatAudioEvent 재사용
[PASS] 고죠/스쿠나 Presentation/HUD 대량 복붙 없이 확장
```

고유 식신 AI/소환 규칙 자체가 별도 Gameplay 코드인 것은 정상이다.

---

# Pass 1 — Identity / Roster / HUD Plumbing · USER VERIFIED

추가:

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

Gate 5A 구조 테스트용 procedural `MegumiPrototypeAvatar`를 추가했다.

Stress roster:

```text
기본: GOJO + SUKUNA
F3:  GOJO + MEGUMI
```

F3는 최종 Character Select가 아니라 개발용 stress-test harness다.

사용자 검증:

```text
F3 roster 정상
메구미 HUD / Skill Deck 정상
T Tag 정상
이동 / 기본공격 / Dodge 정상
HP / CE 왕복 보존 정상
고죠/스쿠나 회귀 없음
```

---

# Pass 2 — Divine Dog · USER VERIFIED

Prototype 규칙:

```text
Q 옥견
CE 20
Cooldown 6.5s
Lifetime 약 6s
Damage 16
Target Lock 우선 추적
없으면 가까운 living target 탐색
접근 후 반복 공격
```

옥견 자체에는 `Health`를 붙이지 않아 Target Lock 후보나 상대 팀 생존/Victory 판정을 오염시키지 않는다.

재사용 경계:

```text
TechniquePresentationId.DivineDog
→ Release / Impact / End

PresentationVfxRuntime
Technique Presentation Director
CombatAudioEvent.DivineDog
FighterAnimationStateSource
```

첫 테스트에서는 Q 입력 시 CE도 줄지 않고 소환되지 않았다.
원인은 메구미 활성화 시 기술 컴포넌트 존재가 확실히 보장되지 않던 hookup 경로였다.

최종 수정:

```text
PrototypeCharacterController.ApplyMegumi()
→ MegumiTechniqueController.GetOrCreate(gameObject)
→ enabled = true

ApplyGojo() / ApplySukuna()
→ MegumiTechniqueController disabled
```

진단용 임시 IMGUI/bootstrap은 원인 확인 후 제거했다.

사용자 검증 결과:

```text
"잘 나와서 겁나게 물어뜯는다"
```

확인:

```text
Q 입력 정상
CE 소비 정상
옥견 생성 정상
추적 정상
근접 공격 / Damage 정상
```

---

# Final Cleanup · USER VERIFIED

정리 버전에서 다시 확인:

```text
F3 → GOJO + MEGUMI
T → 메구미
Q → CE 감소 + 옥견 등장
옥견 추적/공격 정상
자연 종료 정상
Tag Out 시 종료 정상
기존 전투 회귀 없음
```

사용자 최종 결과:

```text
정상
```

따라서 Gate 5A를 닫는다.

---

# Gate 5A에서 의도적으로 하지 않은 것

```text
E 누에 Gameplay
R 만상 Gameplay
V 감합암예정 Gameplay
범용 Summon Framework 선행 구축
식신별 HP/피격 시스템
production model / animation / SFX
최종 Character Select 화면
3인 Team Runtime
```

옥견 한 경로만으로 `새 캐릭터 + 구조가 다른 별도 combat actor`가 Gate 4 경계를 통과한다는 핵심 리스크가 검증됐으므로 Gate 5A를 더 늘리지 않는다.

---

# 다음

```text
Gate 5B First Production-Quality Vertical Slice
```

첫 경계:

```text
Character / Team Select
→ 1~3명 선택
→ MAIN + RESERVE 1 + RESERVE 2
→ Match Setup 데이터
→ Battle Runtime
```

세부:

```text
docs/CHARACTER_TEAM_SELECT_PLAN.md
docs/GATE5B_FIRST_PRODUCTION_SLICE.md
```
