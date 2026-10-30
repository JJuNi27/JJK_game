# PASS 4D.0 — 공통 가드·가드 체력·별도 방어막 MVP

후속 PASS4D.1: 사용자는 원통형 방어막을 임시 MVP 표현으로 수용했으며 가드 전체 기능의 USER VERIFIED 승인은 아니다. 우클릭 타이밍 패링 추가와 최신 검증은 `docs/SHIBUYA_PASS4D1_REVIEW.md`를 참조한다. 아래는4D.0 당시 기록이다.

2026-10-30 KST · `feat/shibuya-combat-mvp` · HEAD `1202e940176daec9fbb0b8027702f600b2f677d4`.
**LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**.
사용자의 PASS4C.2 카메라 기준 대시 방향·속도 승인은 별도 기록하며 새 가드의 조작감/시각 승인을 대신하지 않는다.

## 1. 입력·이동·피해 소유권

`ShibuyaInputFrame.guardHeld` → PlayerInput → Fighter.SetGuardHeld 경로로 우클릭 홀드를 전달한다. 새 누름 한 번만 진입을 시도하고, 실패/피격/파괴/포커스 상실 후 계속 누르는 입력은 소비된 상태로 둔다. 우클릭을 실제로 놓은 뒤 다시 눌러야 재진입한다. 포커스 상실·Escape·비활성·사망·전투 종료도 해제한다. 패링/잡기/더블클릭 반전술식은 구현하지 않았다. 향후 raw 입력 조합 판단을 현재 가드 진입 앞에 추가할 수 있으며 공격·카메라 입력을 재매핑하지 않았다.

지상 정상 상태만 진입 가능하다. 공격/대시/피격/히트스톱/다운·기상/공중·추격/시네마틱/가드 파괴 잠금은 거부한다. 가드 중 이동은 **걷기 속도×0.4**이며 Shift로 빨라지지 않는다. 실측0.606004m/0.315627s = **1.920000m/s** (기본 걷기4.8). 점프·공격·C 대시·F 충전을 막는다. 선행 F 홀드 중 새 가드는 거부하고, 동시 새 가드+F는 가드가 먼저 진입해 F를 차단한다. 기존 이동 방향/몸 회전과 자유 카메라 정책을 유지하며 타깃 정면으로 강제 회전시키지 않는다. 실제 이동은 계속 Fighter의 기존 CharacterController motor 하나다.

공유 `Health`, `DamageContext`, `DamageResolution`, `IDamageGuard` 소스는 변경하지 않았다. 기존 Fighter의 IDamageGuard 안에서 **시네마틱 보호 → 대시 회피 → 일반 가드** 순으로 처리한다. 새 `ReceiveStrike(context,strike)`는 한 번의 동기 Health 호출 동안만 현재 타격의 가드 메타데이터를 제공하고 finally에서 해제한다. 출처의 실제 Transform 위치를 XZ로 투영해 몸 전방과 비교한다. 카메라/락온 타깃은 판정에 사용하지 않는다. 방향을 알 수 없는 출처 없는 피해도 임의로 방어하지 않는다.

성공은 기존 `DamageResolution.Guarded`로 반환하여 ResolveHits의 Applied 뒤 피격·넉백·다운·적중 수·히트스톱·시네마틱 경로에 진입하지 않는다. 기존 대상별 중복 적중 집합도 유지한다. 파괴는 가드만 종료하고 기존 Health가 원래 HP 피해를 **한 번** 적용하도록 NoDecision을 반환한다. GuardBroken은 별도 상태 이벤트이며 새 공격/추가 피해를 생성하지 않는다.

일반 체술만 방어한다. 환경·술식·기존 Unblockable은 통과한다. 새 `guardable=false`는 일반 가드만 우회하는 메타데이터이며 기존 DamageTraits를 바꾸지 않는다. 이 설정을 기존 대시 무적까지 무효화하는 새 예외로 확장하지 않았다. 명시적인 기존 Unblockable의 대시 정책은 보존했다.

## 2. 데이터·회복·파괴

새 `Assets/Shibuya/Data/Shibuya Guard.asset`을 기존 Combat Profile.guard에 연결했다. 실제 사용자-facing 항목은 SerializedProperty 기반 한국어 CustomEditor/기존 StrikeDrawer로 편집하며 Undo/배열/기존 편집 기능을 유지한다.

| 설정 | 초기값 |
|---|---:|
| 최대 가드 체력 | 100 |
| 정면 방어 반각 | 60도 (총120도) |
| 이동 배율 | 걷기×0.4 |
| 종료 후 회복 대기 | 1.2초 |
| 초당 회복 | 20 |
| 파괴 경직·재가드 잠금 | 0.8초 |
| 기본1/2/3/4타 가드 피해 | 16 / 16 / 18 / 35 |
| 타격 메타데이터 없는 일반 체술 가드 피해 | 16 |

HP/주력과 독립적인 현재 가드 체력을 Fighter마다 관리하고 0~최대치로 제한한다. 프로필 최대치 축소도 clamp한다. 방어 중 회복하지 않으며 종료 후 대기한다. 타격별 `guardDamage`, `guardable`은 실제 타격 데이터에 있다. 기본4타에 위 수치를 명시했고 기존 별도 무브셋 에셋을 일괄 재저장하지 않았다. 별도 타격의 새 필드는 기본 가드 피해16/방어 가능으로 시작하며 개별 Inspector에서 조절할 수 있다.

잔여 체력과 같거나 큰 가드 피해로0이 되면 파괴한다. 기본 파괴 잠금/경직0.8초는 Fighter의 기존 국소 전투 시계를 사용한다. 적법한 기존 히트스톱 중에는 타이머가 앞서 진행하지 않는다. 잠금 종료 후에도 체력이0이면 가드할 수 없고, 회복 후 새 누름이 필요하다. 파괴 성공 시험은 HP100→75, Applied1회, GuardBreak1회이며 추가 HP 피해는 없었다. GuardChanged/GuardBlocked/GuardBroken 이벤트를 제공한다. 임시 프로필 해제 시 가드·잠금을 안전하게 해제하고 자세 코드는 null 프로필을 읽지 않는다.

## 3. 비공개 체력과 별도 시각 표현

게임 HUD에 가드 체력 막대/숫자/퍼센트/정확한 잔량 UI를 추가하지 않았다. 내부 값은 개발용 Fighter Inspector의 읽기 전용 한국어 필드와 QA 로그에서 확인한다. 검수 도구 창에도 가드 잔량을 표시하지 않는다.

전용 `ShibuyaGuardVisual`/`ShibuyaGuard.shader`/Guard Profile을 사용한다. 승인된 F 아우라 Shader/Geometry/Profile/색/크기/주력 HUD는 변경하지 않았다. F 아우라의 자식 Renderer 수집에 방어막이 섞이지 않도록 방어막은 **별도 scene root 렌더 객체**이며 소유자의 위치·회전·크기를 따라간다. 충돌체가 없고 Fighter 루트/CharacterController를 이동하지 않는다. 아우라 수집/외곽선에서 배제됨과 소유자 파괴 후 리소스 정리를 확인했다.

- 70% 초과: 안정적인 청록 반투명 곡면.
- 40% 초과~70%: 충돌 파동·흔들림 증가.
- 0% 초과~40%: 보라빛 위험 상태, 꺾인 균열·불규칙 깜빡임·흔들림.
- 0%/파괴: 곡면이 분절·확산·소멸. 일정 시간이 지나면 숨기고 회복/재가드 시 현재 비율로 다시 계산한다.
- 가드 팔 자세와 파괴 기울기를 기존 BlockVisual에 추가했으며 기존 공격/피격/다운 포즈를 교체하지 않았다.

비율 경계·상태별 색/투명도·충돌 강도/시간·균열/불안정 강도·파괴 확산/시간·방어막 크기/위치·팔/파괴 자세를 Guard Profile의 한국어 Inspector에서 조절한다. Mesh/Material/MaterialPropertyBlock을 한 번 생성해 재사용하며 매 프레임 오브젝트를 생성하지 않는다. 8회 반복 시 동일 Mesh/Material ID, 명시적 파괴 후 잔류 없음. 실제 GPU/GC/다인전 성능 프로파일링은 하지 않았다.

## 4. 자동 검증과 실패 이력

QA 루트: `unity/Logs/ShibuyaPass4DQA/20261030-guard/`.

| 실행 | PASS | FAIL | SKIP |
|---|---:|---:|---:|
| initial | 9 | 1 | 0 |
| rearm-diagnostic | 0 | 1 | 0 |
| final-targeted (기존 대시20 + 가드10) | 29 | 1 | 0 |
| guard-final | 10 | 1 | 0 |
| isolation-capacity-final | 1 | 1 | 0 |
| lifecycle-final | 4 | 0 | 0 |
| damage-priority-final | 3 | 0 | 0 |

**최신 고유33 PASS / 0 FAIL / 0 SKIP = 기존 대시 관련20 + 가드 관련13**. 복수 실행의 최신 케이스별 결과이며 단일33개 전체 성공 실행이 아니다. 모든 원본 XML/실패/실행 로그를 보존했다. 과거 PASS4A 미해결18건을 해결로 소급 판정하지 않는다.

실패 구분:

1. initial/rearm-diagnostic의 재가드 대기: 테스트가 한 coroutine 프레임만 우클릭 Up을 보내 입력 Update가 놓쳤다. 실제 Ready/ground=true, 체력7.35, clock1.953>stunnedUntil1.196, broken=false인데 guardInputHeld=true인 원본 관측을 기록했다. 테스트가 Up 수신을 확인한 뒤 새 Down을 보내도록 동기화했다. 런타임의 홀드 소비 규칙은 완화하지 않았다.
2. final-targeted의 마지막 가드 해제 관측: 동일하게 한 프레임 뒤 즉시 false를 읽던 검사였다. 실제 Up 처리 완료를 기다리며 종료 조건/판정 기대값은 유지했다.
3. guard-final의 아우라 분리 검사 NullReference: F를 활성화하기 전에 지연 생성되는 aura.Visual을 조회한 테스트 준비 오류였다. 실제 F를 먼저 켜고 검사하여 통과했다.
4. isolation-capacity-final의 실제 BlockVisual NullReference: 임시 프로필 해제 순서에서 가드 자세가 null 프로필을 읽었다. 새 코드의 생명주기 결함으로 분류하고 프로필 제거 시 해제/시각 null 방어를 수정했다. 프로필 제거·재연결/비활성화/사망/반복/F 복귀 재검증4개 통과.
5. 초기 테스트 작성의 enum 이름2개 컴파일 오류는 실제 프로젝트의 Knockdown/Normal로 수정했다. 이후 C# 컴파일 성공.

가드 정상 시작/유지/해제, 전방0/59도 방어 및61/180도 실패, HP 보존·가드 소모/회복, 파괴 원래 피해1회, 홀드 재가드 금지, Unblockable/환경 통과, 다중 Collider 중복 방어·피해 방지, 이동1.92m/s, 행동/상태/포커스 제한, 위험 구간/회복 표현, F 분리/리소스 폐기, 메타데이터와 대시 무적 독립성을 확인했다. 기존20개에서 방향·거리·쿨타임·충돌·무적·공중 추격·시네마틱·F 등의 관련 회귀를 확인했다. 전체171개 이상 전투 회귀는 반복하지 않았다.

damage-priority-final 실행 요청의 MCP 응답은5초 timeout이었으나 discovery/실행이 이미 시작됐음을 파일로 확인하여 중복 시작하지 않았다. 해당 실행은3 PASS로 종료했다.

## 5. 자료·실제 Play 검수

`unity/Logs/ShibuyaPass4DQA/20261030-guard/media/guard-states-comparison.jpg`, `guard-states-original-speed.mp4`, 안정/중간/위험/파괴/숨김/회복 PNG를 제공한다. 영상은960×720 H.264 VFR, 약2.37초/55표본, 실제 게임 캡처 시간 간격을 사용하며 가속/슬로모션/보간 없음. 상태 촬영은 실제 Health/ReceiveStrike 경로로 피해를 전달하는 기술 시연이며 빨간 더미의 공격 모션까지 촬영한 전투 영상은 아니다.

직접 검수:

1. `ShibuyaCombatMVP` Play. 우클릭 홀드로 팔 자세·방어막을 확인한다. 이동/Shift는 느린 걷기이며 LMB/C/Space/F는 차단된다. 해제 후 정상 복귀한다.
2. 메뉴 **`JJK Game → 시부야 PASS4D → 가드 피격 검수 도구`**. 개발 전용/비저장 기능이다. 전방·측면·후방·파괴·방어 불가 버튼을 누르면2초 뒤 더미가 실제 TryAttack을 실행한다. 그 사이 Game View를 클릭해 입력을 다시 잡고 우클릭을 유지한다. 창을 닫으면 미실행 예약은 취소한다.
3. 정면은 HP가 줄지 않고 방어 반응, 측/후방 또는 우클릭 해제는 HP 피해가 나야 한다. 파괴 강타는 HP25이며 파괴/경직/재가드 제한을 확인한다. 반복 정면 방어로 위험 단계를 확인하고, 해제·회복·재가드로 균열이 없어지는지 본다. 죽으면 기존 결과 화면 재도전으로 새 전투를 시작한다.
4. 정확한 가드 잔량은 게임 HUD가 아닌 Player의 Fighter Inspector 개발용 읽기 전용 필드에서만 확인한다. 조절은 Guard Asset, 공격별 값은 Combat Profile의 타격 배열/별도 무브셋 타격 이벤트에서 한다.
5. 카메라 자유 회전/락온/카메라 기준 대시/F 충전·승인 아우라/공중 추격/기존 콤보를 직접 확인한다. 가드 방향은 몸 전방이며 카메라/락온만 돌린다고 방어 방향을 강제로 바꾸지 않는다.

시각 한계: 안정/중간은 정지 화면 차이가 작고 중간은 충돌 순간 반응이 주된 구분이다. 위험 균열은 보이지만 파괴는 단순 곡면 분절/소멸 MVP이며 최종 캐릭터용 파편·사운드 연출은 아니다. 원속도 위험 인지성과 조작감은 사용자 검수 대기다.

## 6. 환경·보호·파일

최종 C# 컴파일 성공, 전용 Shader 오류 없음. 현재 Console error0/warning1(font). 보존 Pipeline 버퍼에는 총 error6가 남았다: 이전 timeout1, 이번 초기 테스트 컴파일 오류2, 수정한 BlockVisual 예외2, 마지막 실행 요청 timeout1. 기존 font 경고도 누적 보존했다. 전체 세션을 오류0이라고 주장하지 않는다. 원문은 `console-status-final.json`, `console-error-history.json`에 있다.
Play=false/씬 dirty=false/QA=false, 프레임률 원래-1 복구, Time.timeScale1. AutoTick/Play Unfocused/Windows 포커스 변경 없음. 종료 후 가드 Mesh/Material/렌더 root 잔류0 확인. Computer Use/패키지 수정 없음.

시작1206개 파일의 해시/원본 소스 사본을 보존했다. 기존 Combat Asset은 guard 참조/타격별 guardDamage·guardable만 추가했고 나머지는 byte 동일하다. 대시 런타임은 가드 중 시작 거부만 추가하며 승인된 방향 계산/수치/Curve는 유지했다. Gojo/VFX/아우라/카메라/공중 런타임/공유 Health/Scene/Prefab/Packages/ProjectSettings/기존meta/HP1000·100/피해20·20·20·25는 보존했다.

기존 변경: Fighter, PlayerInput, CombatProfile, GroundDashRuntime(진입 조건), CinematicFighterRuntime(방어 우선순위/가드 해제), BlockVisual, EnergyCharge(가드 중 충전 거부), ShibuyaInspectors, Pass3CTestRunner(QA 경로), Combat Profile.asset.
신규: GuardProfile/GuardRuntime/GuardVisual.cs, GuardInspector/GuardTestWindow/Pass4DTests.cs, Guard.shader, Guard.asset 및 각각meta (모두 `unity/Assets/Shibuya/` 아래).
문서: 이 보고서, CURRENT_HANDOFF/CURRENT_TASK, locked의 사용자 직접4C.2 승인 기록,4C.2 보고서 후속 주석. 새 가드를 USER VERIFIED로 기록하지 않았다. staging/commit/push/reset/clean/merge/rebase/worktree/기존 QA 삭제 없음.
