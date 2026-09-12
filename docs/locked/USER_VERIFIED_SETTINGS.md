# 사용자 검증 완료 설정 — 명시적 요청 없이는 변경 금지

작성 기준: 2026-09-13

이 문서는 사용자가 Unity / Blender에서 직접 확인한 값과 동작을 기록한다.
자동 테스트만 통과한 결과보다 우선한다.

## 고죠 이동 — USER VERIFIED

### Movement Profile
- Walk Speed: **3**
- Run Speed: **14**

### Animator Blend Tree
| Motion | Threshold | Time Scale |
|---|---:|---:|
| Gojo_Idle | 0 | 1.00 |
| Gojo_Walk | 3 | 1.15 |
| Gojo_Run | 14 | 1.00 |

### 입력
- 일반 이동: Walk
- Shift + 이동: Run
- Space: Dodge / Evade

### 사용자 검증 완료 전환 흐름
`Idle → Walk → Run → Walk → Idle`

## 보호된 애니메이션 Asset
- Gojo_Idle
- Gojo_Walk
- Gojo_Run
- Gojo_Walk_Base (backup)

관련 없는 VFX/Data 작업 때문에 locomotion이나 위 애니메이션 관계를 변경하지 않는다.

## P0 Unlimited Void physical gameplay — USER VERIFIED

이전 문서의 "Domain 종료 후 basic melee 미해결" 상태는 종료됐다.
사용자가 실제 CombatMVP에서 직접 확인함.

Domain ACTIVE 내부:
- 이동 정상
- Basic Attack 1 / 2 / Finisher 정상
- 실제 hit / damage 정상
- Space Dodge 정상
- Domain 상태는 Active 유지
- Blue / Red / Purple / Domain technique는 잠금 유지

자연 종료 후:
- Normal 상태 복구
- Basic Attack 정상
- Space Dodge 정상
- Technique burnout은 의도대로 유지 가능

따라서 P0는 **USER VERIFIED / CLOSED**.

CombatActionGate / Domain presentation/session policy를 관련 없는 작업에서 재설계하지 않는다.

## Gojo Blue 1차 baseline — USER VERIFIED

사용자 판정:
현재 Blue는 1차 production baseline으로 보호한다.

보존:
- 4-hit gameplay
- singularity / attraction
- core
- debris 기본 언어
- 현재 사용자가 승인한 기본 silhouette

2차 enhancement는 가능하지만 baseline을 갈아엎지 않는다.

## Unlimited Void 푸른 성운 배경 — USER VERIFIED

현재 blue-black nebula / cosmic background 방향은 사용자 만족 상태.

보존:
- 푸른 성운 / 우주 깊이감
- 전체 색 방향
- invisible gameplay floor
- 360° / above-below orientation ambiguity

White Blood / Cosmic Eye / release를 수정해도 이 배경을 훼손하지 않는다.

## Hollow Purple formation / fusion — USER VERIFIED 기준

보호:
- 현재 formation / fusion 기본 구조
- 현재 fusion 후 hold baseline

알려진 별도 문제:
- 발사 시 약간 아래 방향으로 나가는 launch-axis 문제는 아직 별도 수정 대상
- formation/fusion을 갈아엎어서 해결하지 않는다.

## Domain architecture 보호

현재 구조:
- Gameplay Capture Radius
- Visual Barrier Radius / Diameter
- separate Domain Interior Space
- participant state restoration
- camera restoration / cleanup

세 공간/반경 개념을 하나로 합치지 않는다.

## Combat Data-driven migration — LOCAL USER VERIFIED

2026-09-13 사용자 직접 확인:
- Data Asset damage 수치 변경이 실제 runtime damage에 반영됨
- 이동 정상
- Basic Attack 정상
- Blue / Red / Purple 정상
- Domain 정상

따라서 Data-driven migration 자체는 **LOCAL USER VERIFIED**.

핵심 의미:
- gameplay/balance source of truth를 C# 숫자보다 Data Asset으로 이동
- BasicAttack은 variable AttackStep[] 기반
- HP / CE / basic / Blue / Red / Purple / synergy / Domain / Burnout / Targeting / Training Bot을 Data Asset에서 조절 가능

단, 이 문서 갱신 시점에 해당 LOCAL 구조 작업이 GitHub remote에 code commit/push 되었는지는 별도 확인 필요.

## Inspector 한글화 상태

2026-09-13 Codex 검증:
- compile error 0
- localization focused tests 24/24
- 13개 Data Asset hash 불변
- runtime / balance / reference 변경 없음

하지만 사용자가 최종 한글 Inspector 화면을 직접 확인했다는 판정은 아직 기록 전이므로
`CODEX VALIDATED / USER VISUAL CHECK PENDING`.

USER VERIFIED로 올릴 때는 실제 Unity Inspector 확인 후 갱신한다.

## 영구 사용자-facing UI 규칙

다음은 향후 새 Profile/ScriptableObject에도 적용:
- C# identifier는 영어 유지
- 사용자가 직접 만지는 Inspector / Data Asset / 설정 UI는 한국어 표시
- Header만 한국어이고 field label이 영어면 미완성
- 새 Profile 생성 시 localization을 동시에 구현

## 상태 용어

항상 아래 용어를 같은 의미로 사용한다.

- **USER VERIFIED**: 사용자가 Unity / Blender에서 직접 확인
- **LOCAL USER VERIFIED**: 사용자가 직접 확인했지만 구현/asset이 아직 local-only일 수 있음
- **CODEX VALIDATED**: compile/test/static/editor automation은 통과했지만 사용자 직접 판정은 아직 없음
- **PENDING USER VISUAL REVIEW**: 기술적으로 구현됐지만 사용자의 Play Mode/Inspector 시각 판단 필요

자동 테스트 결과로 USER VERIFIED 상태를 새로 만들지 않는다.
반대로 사용자가 이미 검증한 항목을 다시 pending으로 되돌리지 않는다.
