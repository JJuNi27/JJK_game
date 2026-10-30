# PASS 4D.2 — 공통 근접 잡기 MVP

## 체크포인트 승인 범위 — 2026-10-30

사용자가 잡기 입력·조작감, 성공·실패 기본 동작, 기존 전투 연계를 직접 확인해 **USER VERIFIED**로 승인했다. 피해15 HP/밀려남1.4m는 공통 MVP 초기값이며 캐릭터 전체의 고정 규칙이 아니다. 캐릭터별 모션·후속 피해·던지기·카메라/시네마틱 연출은 후속 설계 대상이다. 아래 USER VERIFIED 대기/LOCAL UNCOMMITTED는 구현 당시 기록이며 최초 실패·테스트 컴파일 수정과4회 실행 고유41 PASS 집계를 그대로 보존했다. 이번 저장을 위한 테스트 재실행은 하지 않았다.

2026-10-30 KST. 로컬 `feat/shibuya-combat-mvp`, 기준 `bd5acdcb22c35002317a438a2b0fd28986b53607`.
LOCAL UNCOMMITTED / **CODEX VALIDATED(targeted) / USER VERIFIED 대기**. staging·commit·push 없음.

## 입력과 상태

`ShibuyaPlayerInput.Update`는 기존 `SetGuardHeld` 이후 **실제로 IsGuarding인 경우의 새 LMB**만 `TryGrab`에 전달한다. 그 클릭은 평타에 전달하지 않는다. 같은 프레임 RMB+LMB도 정상 가드 진입이 성립하면 잡기 우선이다. 공격 중 가드 진입이 거절된 경우에는 기존 평타 버퍼 경로를 유지한다. 패링 성공으로 가드가 끝난 뒤 계속 누른 RMB는 잡기 조건이 아니므로 기존 LMB 반격이 가능하다.

잡기는 `Startup → Active → Contact → SuccessRecovery` 또는 `FailureRecovery → Idle`이다. `IsGrabBusy`는 공격자 단계와 피격자의 짧은 접촉 소유권을 포함한다. 기존 행동 제한에 합류하여 공격/버퍼/점프/대시/가드/패링/F를 막지만 전역 시간·다른 참가자 진행을 멈추지 않는다. 입력 포커스 상실·ESC·피격은 짧은 접촉 관계를 해제하며, 사망/비활성화/전투 종료/시네마틱 진입은 소유권을 정리한다. 잡기 종료 후 RMB 홀드로 자동 가드하지 않으며 Up 후 새 Down이 필요하다.

## 판정과 방어 상성

`ShibuyaGrabRuntime`은 CharacterController **bounds 중심 간 3D 거리**, 몸 전방의 수평 반각, 실제 Collider OverlapSphere, 기존 `WorldBlocks`를 사용한다. 카메라 방향이나 강제 회전은 쓰지 않는다. 유효한 명시적 락온을 우선하고, 아니면 가장 가까운 후보를 선택한다. 동거리에서는 런타임 InstanceID로 안정적으로 한 명을 고른다(네트워크 결정성 정책 아님).

공중/다운/기상/피격 경직/시네마틱/무적 등 부적합 대상은 제외한다. 정상 전방 대시는 대상이 될 수 있지만 측면·후방 대시의 기존 회피 무적 구간은 제외한다. 일반 가드와 열린 일반 패링 창은 잡기로 해제된다. 가드 잔량/파괴/패링 성공은 발생하지 않는다. 붙잡은 뒤의 비정상 이동·비활성화·사망·새 무적·장애물·시네마틱 등도 확인해 피해 없이 해제한다.

Core `Health`, `DamageContext`, `IDamageGuard`를 변경하지 않았다. 실제 피해는 **PhysicalStrike + DamageTraits.None**으로 Health를 한 번 호출한다. `receivingGrabFrom`을 try/finally로 한정하여 시부야 일반 가드·패링 평가만 생략한다. 대시 회피와 다른 `IDamageGuard`는 그대로 유지한다. `Unblockable`, `IgnoresInfinity`, 전역 방어 우회 예외를 사용하지 않는다. 포획 전에 다른 활성 방어도 확인하며, 이 확인은 방어 구현 자체의 기존 피드백을 발생시킬 수 있다.

Health.Applied 후에만 `GrabSucceeded`/전용 카운트를 올리고 기존 피격 반응·거리 넉백을 연결한다. 일반 `HitConfirmed`, 콤보 카운트, 히트스톱, 마무리 시네마틱을 발동하지 않는다. Health 사망/승패 이벤트는 그대로 사용한다. 치명 잡기는 즉시 기존 전투 종료 규칙을 따르며 연출을 위해 생명을 연장하지 않는다.

## 설정과 이동 소유권

`Data/Shibuya Grab.asset`을 Combat Profile의 `grab`이 참조한다. 별도 한국어 SerializedProperty Inspector는 Undo·다중 편집·객체 참조와 Min/Range 편집을 유지한다.

| 항목 | 초기 실험값 |
|---|---:|
| 최대 사거리 / 정면 반각 | 1.55m / 50도 |
| 준비 / 유효 시간 | 0.18 / 0.08초 |
| 접촉 표현 / 피해 | 0.10초 / 15 HP |
| 대상 밀려남 / 경직 | 1.4m / 0.40초 |
| 성공 후 회복 / 실패 후 회복 | 0.25 / 0.50초 |
| 블록 모션 강도 | 1.0 |
| 주력 / 가드 소모 | 없음 |

빠른 적중은 약0.18+0.10+0.25=0.53초, 일반 빗나감은 약0.18+0.08+0.50=0.76초다. 실제 관측은 프레임 경계를 포함한다. 큰 Editor 프레임이 짧은 활성 구간을 통째로 건너뛰어도 첫 판정 진입은 한 번 수행한다. 소스가 피격되어 중단되면 그때부터 실패 회복을 적용하며 반복 피격으로 같은 실패 타이머를 재시작하지 않는다. 무적·슈퍼아머는 없다.

캐릭터 강제 위치 맞춤/순간이동/별도 CharacterController.Move는 없다. Contact 동안 정상 입력만 제한한다. 밀려남은 기존 `distanceKnockbackRemaining/Speed/Direction`, `ConstrainFighterMovement`, Fighter 단일 motor에서 소비한다. 막힌 거리도 소비해 벽 제거 후 누적 이동이 튀어나오지 않는다. 기존 시부야/고죠 카메라, 대시 프로필, 가드/패링 프로필, 아우라 셰이더·Geometry·프로필은 변경하지 않았다.

## targeted QA와 최초 이력

원본: `unity/Logs/ShibuyaPass4D2QA/20261030-grab/`. 각 실행의 discovery/execution/results.xml은 별도 폴더에 보존한다. 과거 PASS4A 미해결18건을 해결로 소급하지 않는다. 전체 구형 회귀는 실행하지 않았다.

| 실행 | PASS | FAIL | SKIP |
|---|---:|---:|---:|
| initial | 13 | 0 | 0 |
| edges-media | 7 | 1 | 0 |
| focused-final | 18 | 0 | 0 |
| lifecycle-branches-media | 5 | 0 | 0 |

**최신 고유41 PASS / 0 FAIL / 0 SKIP**: 신규 잡기23개 + 선택한 기존 회귀18개. 위 4회 실행의 최신 케이스별 집계이며 단일41개 전체 실행이 아니다. 중복 재검증은 한 번만 센다. `test-summary-final.json`에 전체 케이스명/실행별 수치를 기록했다. 기존 테스트 기대값·승인된 게임플레이 설정을 변경하지 않았다.

최초 캡처 실패는 `OriginalSpeedNormalGuardedAndMissGrabEvidence`의 Newtonsoft Vector3.normalized 순환 직렬화다. QA 로그를 xyz 배열로 바꿨으며 게임플레이/기대값은 변경하지 않았다. 원본 실패 XML·기존 캡처를 보존하고 focused-final에서 통과했다. 뒤쪽 추가 캡처는 사거리 밖 더미까지 보이도록 QA 카메라 구도만 보완했다.

작성 중 테스트 컴파일 오류2건(StunRemaining 미존재, private IsStunned 접근)을 기존 경직 필드 계측/공개 State 관측으로 수정했다. `compile-initial.json`, `compile-edges.json`에 원본을 남겼다. 런타임 수치 수정이나 테스트 기준 완화는 없었다.

실제 성공 계측: HP 피해15, 피해 후 밀려남 **1.4000001m**, 초기 경직 잔여 **0.400000036초**, GrabSucceeded1, 일반 HitConfirmed0, AttackStarted0. 첫 결과는 `initial/legacy/ShibuyaPass4B3QA/4D2-success.json`이다.

검사 내용: 입력 동시 Down/홀드 소비/재가드/패링 후 평타/공격 중 버퍼, 가드·패링 우회, 실제 회피 입력, 무적/특수 방어, 위치·각도·벽, 단일 대상/다중 Collider/락온 우선, 접촉 후 대상 이탈·무적, 피격/사망/종료, 실제 검수 도구, 기존 가드 회복·파괴/패링 취소·쿨타임/대시 방향·거리/지상·공중 콤보/시네마틱/F·본인 HUD. Infinity는 실제 `GojoInfinityDefense.EvaluateDamage`의 결정을 비활성 테스트 오브젝트에서 검증했으며, 레거시 캐릭터 bootstrap/모델은 실행하지 않았다.

## 시각 자료·리소스·한계

새 VFX 오브젝트/메시/머티리얼을 만들지 않는다. 기존 BlockVisual에 팔 내밀기·짧은 움찔·허공 실패 자세만 추가했다. 손과 상대 몸의 접촉점을 맞추는 리깅, 장거리 끌어오기, 최종 캐릭터 모션/사운드/VFX/카메라는 구현하지 않았다. 짧은 Contact는 실제 강제 손잡이 구속이 아니라 충돌을 유지한 행동 제한이다.

최종 `media-final/normal.mp4`, `guarded.mp4`, `miss.mp4` 및 `grab-comparison.jpg`. 960×720 원속도 H.264 VFR, 실제 게임 캡처 간격 사용, 슬로모션/가속/보간 없음. 각각46/46/47개 표본, 마지막 표본 시간1.441/1.416/1.437초다. 테스트 입력으로 실제 입력→잡기→Health 경로를 촬영했다. QA 고정 카메라이며 사용자 자유 카메라의 수동 품질 승인을 대체하지 않는다. 원본 프레임과 timestamps, 이전 `media/` 구도도 보존했다. 정상·가드 적중에서는 짧은 움찔과 밀려남, 실패에서는 앞으로 기운 빈틈을 읽을 수 있으나 팔/손의 실제 접촉 맞춤은 임시 수준이다.

최종 컴파일 오류0, `scriptCompilationFailed=false`. 현재 Editor Console **error0/warning1**(Deleting invalid font reference)이며, Pipeline 누적 버퍼에는 위에서 수정한 테스트 컴파일 오류2건과 경고77건이 남아 있다. 이를 전체 이력 error0으로 표현하지 않는다. Console을 수동으로 지우지 않았다. 신규 셰이더 변경 없음. Play stopped/원래 Shibuya 씬 clean/fps -1/Time.timeScale1/AutoTick true·16ms/PlayUnfocused 유지. AutoTick·프레임률 임시 영구 설정 변경 없음. Play 종료 후 가드·패링 runtime Mesh/Material **0/0**, 잡기·가드·패링 관련 씬 잔존 객체0. 잡기 자체에는 새 Mesh/Material 생성이 없다. 실제 GPU/다인전 부하와 네트워크 동기화는 이번 검증 범위가 아니다.

## 사용자 직접 검수

1. `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 열고 Play.
2. `JJK Game → 시부야 PASS4D → 잡기 검수 도구`를 연다.
3. 일반/가드/패링/사거리 밖/측면·후방 회피/준비 중 반격 중 하나를 선택한다.
4. Game View로 돌아가 클릭하여 입력을 잡는다. 2초 후 더미가 몸 전방에 배치된다. **직접 우클릭 유지→새 좌클릭**.
5. 성공 피해·밀려남, 실패 빈틈, 가드 홀드 자동 재시작 없음, 새 RMB 후 재가드, 패링 성공 후 RMB를 유지한 LMB 평타 반격을 비교한다.

도구는 Play Mode 한정 배치/실제 GuardHeld·TryGroundDash·TryAttack만 사용한다. 성공 카운터/HP를 조작하지 않으며 창 닫기/중단/전투 종료 시 예약과 더미 가드 입력을 해제한다. 정상 전투에 자동 AI나 고정 더미 규칙을 추가하지 않았다. 몸 전방과 거리 조건이 있으므로 준비 후 캐릭터를 돌리거나 멀어지면 실패할 수 있다. 접촉 모션·실패 자세·상성의 실제 조작감은 사용자 검수 대기다.

## 변경 범위

신규: GrabProfile/GrabRuntime/GrabInspector/GrabTestWindow/Pass4D2Tests, Grab Data Asset과 각각의 meta, 본 보고서.
수정: Fighter/PlayerInput/CombatProfile/GuardRuntime/HitStateRuntime/CinematicFighterRuntime/EnergyCharge/BlockVisual/공통 Inspectors, Combat Data Asset의 grab 참조, CURRENT_HANDOFF/CURRENT_TASK.
기존 USER_VERIFIED_SETTINGS, Gojo/VFX, 아우라, Scene/Prefab, Packages/ProjectSettings, 기존 meta GUID, 기본 피해20/20/20/25·HP100/1000은 보존한다. 새 입력 기능·반전술식·자동 시네마틱·공중 잡기·밸런스 재설계는 포함하지 않는다.
