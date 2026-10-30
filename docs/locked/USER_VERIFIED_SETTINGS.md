# 사용자 검증 완료 설정 — 명시적 요청 없이는 변경 금지

## PASS4F.1 — USER VERIFIED / MVP 임시 수용 (2026-10-31)

사용자가 직접 플레이하여 다음만 **MVP 수준으로 임시 수용**했다.

- AI 모드 선택 및 기본 동작.
- 공격형 AI와 방어형 AI의 현재 기본 구조.
- 움직임의 어색함은 추후 개선하고 현재 구현을 유지한 채 다음 작업으로 넘어가는 결정.

이는 AI 완성도의 최종 승인이 아니다. 자연스러운 이동, 공격·방어 행동 전환, 대전 난이도·재미, 패링 성공률/반응 정확도의 최종 밸런스, 종합 모드 공격 적극성, 캐릭터별 성향, 장시간 안정성·전체 경계 조건은 승인 범위에서 제외한다. 자동 검사만 통과한 세부 조건을 USER VERIFIED로 확대하지 않는다.

기본 AI OFF와 모드 선택/실제 ON·OFF 분리를 유지한다. 선택만으로 움직이지 않는 상황을 활성화된 AI의 고장으로 혼동하지 않도록 검수 시 두 상태를 구분한다. 기존 PASS4F.0 공격형 AI와 PASS4C~4E·고죠/VFX·주력 아우라·카메라·전투값 잠금은 계속 유지한다. 이번 저장에서는 게임플레이 설정값을 변경하지 않는다.

검증 근거는 PASS4F.1 최신 고유39 PASS/0 FAIL/0 SKIP(6회 실행), PASS4F.0 최신 고유34 PASS(7회 실행)이다. 단일 전체 성공 실행으로 표현하지 않는다. 최초 실패·수정 이력, 이전 도구/QA1928건 분류, PASS4A 미해결18건은 보존한다. 후속 AI 개선 목록은 CURRENT_HANDOFF/CURRENT_TASK/SHIBUYA_PASS4F1_REVIEW에 기록했으며 이번에는 구현하지 않는다. 아래는 과거 승인·구현 시점의 기록이다.

## PASS4F.0 공격형 훈련 AI 사용자 승인 — 2026-10-30

사용자가 직접 플레이하여 플레이어 접근, 방향 정렬, 기존 Fighter의 1~4타 공격, 공격 후 휴식·재접근, 기본 조작감을 승인했다. 이 범위만 USER VERIFIED다. 장애물 우회, 장시간 안정성, 모든 경계 조건 및 최종 AI 난이도는 승인하지 않았다.

기존 PASS4C~4E 승인과 Gojo/VFX·아우라·카메라 보호 규칙을 유지한다. 자동 검증 최신 고유34 PASS는7회 실행 집계이며 사용자 수동 검증 범위를 확대하지 않는다. PASS4A 미해결18건과 최초 실패 원본은 보존한다. PASS4F.1 방어형 AI는 별도 구현·사용자 검수 대상이며 이번 승인에 포함하지 않는다.

## PASS4D.2 + PASS4E 체크포인트 사용자 승인 — 2026-10-30

이번 체크포인트 요청에서 사용자가 실제 Play Mode 검증과 Git 저장을 승인했다. 아래 범위만 **USER VERIFIED**로 기록한다.

- **PASS4D.2 잡기:** 잡기 입력·조작감, 성공·실패의 기본 동작, 기존 전투 시스템과의 기능적 연계.
- **PASS4E 긴급 탈출:** 다운 후 방향 기상의 기본 기능, 방향 입력 및 기상 조작.

잡기 피해량·밀려남·캐릭터별 모션·후속 타격·카메라 연출은 최종 확정하지 않았다. 현재 **15 HP / 1.4m는 공통 MVP 초기값**이며 모든 캐릭터의 고정 규칙이나 새 잠금 밸런스가 아니다. 향후 캐릭터별 성공 연출, 후속 타격 횟수/피해, 밀쳐내기·던지기, 시네마틱 여부, 짧은 즉시 실행형 잡기는 별도 설계 대상이다. 이를 이미 구현한 기능으로 취급하지 않는다.

긴급 탈출의 모든 극단적 충돌·저프레임·무적 경계까지 사용자가 직접 검증한 것은 아니다. 최종 캐릭터별 기상 애니메이션과 VFX도 후속 작업이다. 자동 검증 범위는 CODEX VALIDATED로 구분한다.

기존 PASS4C.2 방향 대시·조작감, PASS4D.1 패링 구조·조작감의 USER VERIFIED 범위를 유지한다. PASS4D.0 원통형 가드 방어막은 임시 MVP 표현 수용이며 최종 VFX 승인이 아니다. 가드 체력 HUD 비공개, 기존 F 주력 아우라·카메라·고죠 VFX 및 기존 잠금 수치를 보존한다.

PASS4D.2 최신 고유41 PASS는4회 실행, PASS4E 최신 고유37 PASS는7회 실행의 케이스별 집계다. 단일 전체 회귀 성공 실행으로 표현하지 않는다. 최초 실패·컴파일 수정·플랫폼 중단 수정 이력, 초기 쿨타임0 관측의 근본 원인 미확정, 이후 계측 통과를 함께 보존한다. PASS4A 과거 미해결18건은 해결로 변경하지 않는다.

아래 검수 대기/LOCAL UNCOMMITTED 표시는 각 PASS 작성 당시 이력이다. 최신 승인 범위는 이 절을 따른다. 다음 후보는 **훈련 AI 설계**이며 행동 규칙 합의와 별도 PASS 승인 전에는 구현하지 않는다.

## PASS4C~4D.1 체크포인트 사용자 승인 — 2026-10-30

사용자가 이번 Git 체크포인트 요청에서 다음 승인 범위를 명시했다.
- PASS4C.2 카메라 기준 방향 대시·조작감: **USER VERIFIED**.
- PASS4D.1 패링 구조·조작감: **USER VERIFIED**.
- PASS4D.0 원통형 방어막: **MVP 임시 표현 수용**, 최종 VFX 승인 아님. 가드 전체 기능의 수동 승인을 확대하지 않는다.
- 패링 최종 VFX·캐릭터별 모션은 후속 작업이다. 자동 검사만으로 시각 최종 승인을 선언하지 않는다.

가드 체력은 내부 수치로 관리하고 게임 HUD에 공개하지 않으며, 방어 이펙트 상태로 위험을 표현한다. PASS4A의 과거 회귀 실패18건은 별도 미해결 이력으로 보존한다. 아래 검수 대기/LOCAL UNCOMMITTED 표시는 각 기록 작성 당시 상태이며 이번 승인 범위는 이 절을 따른다. 기존 잠금 수치·Gojo/아우라 자산은 변경하지 않았다.

## PASS4D.1 요청에서 확인한 승인 범위 — 2026-10-30

PASS4C.2 카메라 기준 대시 방향·조작감은 기존 USER VERIFIED다. PASS4D.0 가드 전체는 CODEX VALIDATED이며 원통형 외형은 **임시 MVP 표현 수용**이다. 이를 최종 방어 연출이나 가드 전체의 USER VERIFIED로 확대하지 않는다. 새 PASS4D.1 패링·쳐내기 모션·섬광은 사용자 직접 검수 대기다. 아래 기존 잠금값과 승인 기능을 변경하지 않는다.

## 시부야 PASS4C.2 사용자 직접 승인 — 2026-10-30

사용자가 PASS4D.0 요청에서 PASS4C.2의 **카메라 기준 대시 방향과 속도**를 직접 확인하고 승인했다고 명시했다. 락온·비락온의 동일 카메라 기준 WASD+C와 시작 경로 유지 동작을 보존한다. 현재 로컬 설정은 거리5.25/3.60/3.30m, 지속시간0.22/0.18/0.20s, 쿨타임0.45s, 기존 측면·후방0.04~0.14s 무적이다. 수치·검사 모든 경계 조건을 사용자 수동 검증으로 확대하지 않는다.
이 승인은 LOCAL UNCOMMITTED PASS4C.2 기능 범위이며 GitHub 기준 체크포인트는 여전히 `1202e940176daec9fbb0b8027702f600b2f677d4`다. 새 PASS4D.0 가드와 방어막은 CODEX VALIDATED/사용자 직접 검수 대기이며 USER VERIFIED가 아니다. 아래 기존 고죠/시부야 잠금값은 변경하지 않는다.

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

## Inspector 한글화 — LOCAL USER VERIFIED

2026-09-13 Codex 검증:
- compile error 0
- localization focused tests 24/24
- 13개 Data Asset hash 불변
- runtime / balance / reference 변경 없음

그리고 사용자가 실제 Unity Inspector에서 한글화가 정상 적용된 것을 직접 확인함.

따라서 Inspector localization은 **LOCAL USER VERIFIED**.
이 상태를 다시 `PENDING USER VISUAL REVIEW`로 되돌리지 않는다.

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

## 시부야 PASS 4B.4a 체크포인트 — 사용자 확인 2026-10-25

아래는 사용자의 직접 확인과 이번 Git 체크포인트 승인을 기록한다. 기존 고죠 보호값을 교체하거나 자동 검사 범위를 사용자 검증으로 확대하지 않는다.

USER VERIFIED:
- 전신 혼령 주력 아우라의 시각적 방향.
- QA 메뉴 없이 일반 F 홀드에서 최신 승인 아우라가 출력되는 동작.
- 실제 F 홀드 주력 충전 동작.
- 기존 시부야 공통 전투의 앞선 직접 승인 범위 유지. PASS3D는 무피해 접촉·자연 낙하/공중3클릭/시네마틱 발동·카메라 복귀의 기능 범위이며 세부 모션·샷 품질은 제외한다.
- PASS4A.1 락온 공격 방향 전환의 속도·자연스러움에 대한 기존 사용자 승인 유지.

현재 저장 기준:
- 일반 지상 피해20/20/20/25, Dummy HP1000, Player HP100.
- 최대 주력량 단일 원천은 CursedEnergyProfile.MaxEnergy. 시부야 저장 최대100, 시작 비율1, 초당 충전12. 현재 주력은 아우라 크기에 영향을 주지 않음.
- 아우라 크기 곡선은 **100→1.0 / 150→1.5 / 200→2.0**, 저장 최소0.5/최대3.0.
- **4.0배는 실험 가능 범위이며 최종 기본값이 아니다.** 2.7/3.0/3.5/4.0 중 최종 크기 정책은 별도 사용자 결정 대기다.
- 승인 시각 구성: Details OFF, 국소 형태 보존 ON, 추가 확장0.65, 먹물 경계1.30/내부 비중0.80, 분리 화염 ON, 상하 연결1/1. Interior0.65/밀도1.0 유지. 청록·핑크·초록의 색상 보존.

보류·검증 경계:
- **신체 강화 패시브는 사용자 결정으로 보류**. 준비 임팩트/해제 버프/강화 지속 아우라/공격력·이동속도 강화/W+F 대시는 구현되지 않았다.
- 자동 검사만 통과한 극단적 충돌·무적·저프레임·복수 소유권·카메라·리소스 경계는 CODEX VALIDATED다. PASS4A 과거 회귀18건도 해결로 소급 변경하지 않는다.
- 이번 체크포인트가 모든 QA 원본/로컬 모델·음성·레퍼런스를 GitHub에 백업한다는 뜻은 아니다.

상세: `docs/CURRENT_HANDOFF.md`, `docs/SHIBUYA_PASS4B4_REVIEW.md`, `docs/SHIBUYA_PASS4B4a_REVIEW.md`.
