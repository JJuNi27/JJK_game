# PASS 4D.1 — 공통 타이밍 패링 MVP

## 체크포인트 승인 반영 — 2026-10-30

사용자가 이번 PASS4C~4D.1 Git 체크포인트 요청에서 **패링 구조·조작감 USER VERIFIED**를 명시했다. 최종 패링 VFX·캐릭터별 모션은 후속 작업이다. 가드 원통은 MVP 임시 수용이며 가드 전체의 사용자 수동 검증으로 확대하지 않는다. 아래 CODEX VALIDATED/검수 대기/LOCAL UNCOMMITTED 표시는 구현 당시 기록이다. 원본 검증 수치·실패 이력은 유지했고 체크포인트를 위한 테스트 재실행은 하지 않았다.

2026-10-30 KST · `feat/shibuya-combat-mvp` · HEAD `1202e940176daec9fbb0b8027702f600b2f677d4`.
**LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**.
PASS4C.2 방향·조작감은 USER VERIFIED. PASS4D.0 가드 전체는 CODEX VALIDATED이며 원통형 외형만 사용자가 임시 MVP 표현으로 수용했다. 새 패링과 최종 방어 연출의 사용자 승인을 뜻하지 않는다.

## 1. 실제 구현·입력 정책

기존 우클릭 입력/SetGuardHeld 경로를 사용한다. 정상적인 지상 가드 진입에 성공한 **새 Down**에서만 패링 시도를 연다. 창이 끝나거나 재시도 제한에 걸려도 홀드 일반 가드는 유지한다. Up/포커스 상실/가드 중단은 창을 닫고, 실패한 누름이나 홀드가 나중에 자동으로 패링을 시작하지 않는다. 시작 사이 간격은 game time 기준이며 국소 히트스톱으로 창을 늘리지 않는다. 설정한 재시도 간격이 창보다 짧아도 진행 중 창을 반복 연장하지 않는다.

성공 시 가드를 종료하고0.10초의 쳐내기 동작·행동 제한 후 공격/이동/대시 등을 직접 선택한다. 자동 반격은 없다. 계속 누른 우클릭은 소비된 상태로 두므로 가드를 다시 시작하려면 놓았다가 새로 누른다. 성공 후 별도 무적을 추가하지 않았다. 공격/피격/다운·기상/공중·추격/대시/히트스톱/시네마틱/가드 파괴/F 충전 등 기존 가드 불가 상태에서는 시도도 시작하지 않는다.

방향은 몸 전방과 실제 공격자 위치의 수평 차이로 계산한다. 카메라/락온으로 성공시키지 않는다. 기존 Guard Profile.halfAngle은 홀드 가드, 새 Parry Profile.halfAngle은 타이밍 패링에만 사용한다. 둘 다 초기 반각60도이며 독립 튜닝이다.

## 2. 피해·공격 취소·이동 연결

기존 `Health/DamageContext/DamageResolution/IDamageGuard`는 수정하지 않았다. Fighter의 기존 우선순위는 시네마틱 보호 → 대시 회피 → (유효 패링 → 일반 가드)다. 패링은 기존 `DamageResolution.Guarded`를 반환하고 **ParrySucceeded/ParrySuccessCount**로 구분한다. GuardBlocked/가드 체력 피해/일반 적중 이벤트는 중복 발생하지 않는다.

새 `strike.parryable`과 기존 `guardable`은 별개다. 기본4타에는 parryable=true를 명시했다. parryable=false/guardable=true면 일반 가드, parryable=true/guardable=false면 유효 창에서 패링 가능하다. 환경·술식·영역 필중·기존 Unblockable은 제외한다. Source가 정상적인 ShibuyaFighter가 아니면 패링 반응을 임의로 적용하지 않고 기존 방어 경로로 넘긴다. 새 규칙을 다른 Core 피해 시스템에 전역 적용하지 않았다.

성공은 방어자의 HP/가드/주력을 소모하지 않는다. 공격자에게 가짜 Health 피해를 보내지 않고 기존 ApplyHitReaction/CancelCombo를 사용해 현재 공격·예약 입력·확정 연계를 취소한다. 밀려남은 기존 distanceKnockback과 Fighter 단일 CharacterController motor에서 처리한다. 막힌 이동은 즉시 소비하며 순간이동하지 않는다. 기본 거리1.2m를 경직0.35초 동안 진행한다.

방어 판정 중 공격자를 동기적으로 취소하므로 ResolveHits는 Health 반환 직후 취소 여부를 재확인해 Collider/후속 이벤트/저장된 시네마틱으로 진행하지 않는다. 같은 프레임 이동도 변경된 행동 제한을 다시 확인하여 취소 전 전진/입력이 남지 않게 했다. 일반 피해 계산·공격 타이밍·기존 넉백 수치는 바꾸지 않았다.

## 3. 데이터·한국어 Inspector

새 `Data/Shibuya Parry.asset`을 기존 `Shibuya Guard.asset.parry`가 참조한다. 실제 사용자 필드는 새 ParryInspector/기존 GuardInspector/StrikeDrawer의 한국어 SerializedProperty 편집으로 제공한다. Undo·배열·객체 선택·기존 편집 구조를 유지한다.

| 항목 | 초기값 |
|---|---:|
| 패링 창 | 0.12초 |
| 시작 사이 최소 간격 | 0.50초 |
| 정면 반각 | 60도 (총120도) |
| 공격자 경직·밀려남 시간 | 0.35초 |
| 밀려남 목표 거리 | 1.2m |
| 방어자 성공 동작·행동 제한 | 0.10초 |
| 국소 히트스톱 | 0초 (선택적 사용 가능) |
| 효과 크기 / 지속시간 / 강도 | 0.9 / 0.22초 / 2.8 |
| 블록 쳐내기 강도 | 1.0 |

성공 섬광의 Shader/색상과 위 값을 독립 프로필에서 조절한다. 기존 Guard Asset의 가드 체력/회복/파괴/색/형상 값은 유지하고 참조만 추가했다. 기존 Combat Asset도 기본4타 parryable=true만 추가했다. 현재 주력·가드 잔량 HUD를 추가하지 않았다. 패링 회복 보너스/잡기/더블클릭 반전술식/신체 강화는 구현하지 않았다.

## 4. 시각 표현·리소스

기존 방어막은 그대로 유지한다. 패링 성공에서만 짧은 팔 쳐내기/상체 회전과 실제 타격점 부근 섬광·확산 링을 보인다. 일반 GuardBlocked가 발생하지 않아 방어막 충돌 반응과 겹치지 않는다. 공격자의 흰색 반응은 기존 피격 프레젠테이션 재사용이며 HP 피해는 없다.

`ShibuyaParryVisual`은 별도 scene root의 전용 Mesh/Material/Shader/PropertyBlock을 한 번 생성해 재사용한다. 가드 원통 크기를 바꾸지 않고 F 아우라의 몸 Renderer 수집에서도 배제한다. 수명 종료 시 숨기며 소유자 정리 시 Mesh/Material/root를 파괴한다. F 아우라 Shader/Geometry/Profile/색/크기/HUD와 카메라는 수정하지 않았다. 실제 GPU/다인전 부하 측정은 하지 않았다.

블록 모션·단순 링/섬광은 MVP다. 현재 타격 정의의 몸통 접촉점과 임시 팔 모션을 사용하며 캐릭터별 손 맞춤/최종 애니메이션·사운드 품질은 후속 검수 대상이다.

## 5. targeted 결과·원본 이력

QA: `unity/Logs/ShibuyaPass4D1QA/20261030-parry/`.

| 실행 | PASS | FAIL | SKIP |
|---|---:|---:|---:|
| initial (신규) | 10 | 1 | 0 |
| focused-regression | 19 | 0 | 0 |
| practice-final | 1 | 0 | 0 |
| impact-proof | 2 | 0 | 0 |

**최신 고유30 PASS / 0 FAIL / 0 SKIP** (신규12 + 기존 가드13 + 선택한 대시/공중/시네마틱/F 관련5). 복수 실행의 최신 케이스별 결과이며 단일30개 전체 실행이 아니다. 원본 XML/execution/discovery를 보존했고 기존 전체 전투 검사를 반복하지 않았다. PASS4A 과거 미해결18건도 해결로 소급 표시하지 않는다.

최초 실패는 밀려남 기대1.2m에 순변위0.68m를 비교한 측정 기준 문제였다. 준비 중 기존 추적이0.52m 전진했으며 이 수치를 바꾸지 않았다. ParrySucceeded 순간의 실제 위치부터 측정해 **1.20000017m**, 경직 초기 잔여 **0.350000024초**, HP100/1000 불변, 가드100, 패링1, 소스 적중0, 타격 이벤트1, 예약 후속 실행 없음으로 확인했다. 기대 거리/오차를 완화하지 않았고 첫 실패 XML도 남겼다.

창 열림/만료/홀드 가드/연타 제한, 전방0·59도 성공과61·180도 실패, 가드·패링 플래그 독립, 특수 피해 제외, 입력 불가 상태·포커스 상실, 실제 다중 Collider·다중 이벤트·예약 취소, 직접 후속 행동 가능/자동 반격 없음, 벽 차단/막힌 거리 소비, 기본0 및 선택적 국소 히트스톱에서 게임 시간 진행, F 복귀/아우라 배제/리소스 폐기, 연습 도구의 실제 공격/종료 예약 정리를 확인했다. 명중 히트스톱0.067초와 시네마틱이 설정된 실제 마무리도 패링 시 둘 다 발동하지 않았다.

## 6. 비교 자료와 직접 연습

`media/parry-success.mp4`, `early-guard.mp4`, `late-hit.mp4`, `parry-guard-hit-comparison.jpg`, 각 impact PNG와 timestamps를 제공한다. 같은 구도의 실제 TryAttack/Health 경로와 테스트 입력을 사용한960×720 H.264 VFR 원속도 자료다. 약1.58~1.60초/59~61표본, 가속·슬로모션·프레임 보간 없음. 성공은 HP/가드 불변·패링1/가드0, 이른 홀드는 HP불변·일반 가드 체력16 소모, 늦은 입력은 HP20 피해를 확인했다. 결과 카운터는 동일 촬영 테스트 내 누적값이며 게임 HUD에 표시하지 않았다.

직접 검수:

1. ShibuyaCombatMVP Play → **JJK Game → 시부야 PASS4D → 가드 피격 검수 도구**.
2. **패링 반복 연습 시작 (3초 예고)** 클릭 → Game View를 클릭해 조작을 잡는다. 우클릭을 놓은 상태에서 예고/공격을 보고 적중 직전에 새로 누른다. 일찍 누르고 유지하면 일반 가드, 늦으면 피격된다.
3. 개발 전용 Game View 예고와 결과 문구는 실제 이벤트를 읽을 뿐 입력·성공 판정을 조작하지 않는다. 더미는 기존 실제 TryAttack을 실행한다. 반복 예제만 임시 startup0.3초를 사용하며 기본 공격 데이터는 변경하지 않는다. 연습 중단/창 닫기/사망·종료·Actor 제거 시 예약을 정리한다.
4. 성공 후0.1초 뒤 직접 LMB/이동/C를 선택한다. 계속 누른 우클릭으로 가드가 자동 복귀하지 않으므로 다시 가드하려면 새로 누른다. 전/측/후방·방어 불가 단일 버튼도 재사용할 수 있다.
5. 반각/창/경직/밀려남/프레젠테이션은 Parry Asset, 일반 가드와 잔량 위험 표시는 Guard Asset, 개별 타격 가능 여부는 Strike Inspector에서 조절한다. 가드 수치 비공개 정책을 유지한다.

반복 도구는 Editor 전용 opt-in이며 기존 일반 전투/씬·프리팹에 상시 AI를 추가하지 않는다. 우클릭의 향후 더블클릭/조합 판단은 현재 raw 입력 경계 앞에 확장할 수 있다. 이번에는 해당 기능을 발동시키지 않는다.

## 7. 컴파일·보호·파일

Unity CLI로 기존 Pipeline7800/Unity6000.3.20f1에 연결했다. 샌드박스 프로세스 조회 제한으로 최초 일반 status가0개를 반환했지만 읽기 권한 범위를 확장한 조회로 정상 Editor를 확인했다. 설치/업데이트/MCP 재설정/Computer Use/포커스 변경은 하지 않았다. 초기 인라인 eval의 문자열 전달 실패는 코드 파일 평가로 해결한 CLI 진단이며 프로젝트 소스 컴파일 실패가 아니다.

최종 C# 컴파일 성공/Parry Shader 오류 없음. 현재 Console error0/warning1(font), 이 Editor 세션 보존 버퍼 error0/warning31. 이전 PASS4D의 오류/실패 기록은 기존 경로에 보존했고 로그를 지우지 않았다. 최종 Play=false/씬 dirty=false/QA=false, targetFrameRate=-1 복구, Time.timeScale1, 패링 Mesh/Material/root 잔류0. AutoTick/Play Unfocused 변경 없음.

시작1223개 파일 해시와 변경 전 Shibuya 소스/에셋을 저장했다. 기존 Data Asset의 숫자/공격·이동/가드·대시 값은 byte 비교로 보존했다. 기존 Core Health/카메라/락온/아우라/Gojo/Scene/Prefab/Packages/ProjectSettings/기존meta/HP1000·100/피해20·20·20·25/모든 이전 QA 원본은 유지한다. 일반 가드 검사는 새 패링 창이 끝난 홀드를 기다리도록 시험 준비만 조정했다.

기존 변경: GuardProfile/GuardRuntime, CombatProfile/Combat Asset/Guard Asset, Fighter/HitStateRuntime/PlayerInput/EnergyCharge/BlockVisual, GuardInspector/GuardTestWindow/ShibuyaInspectors/기존4D 테스트. 신규: ParryProfile/ParryRuntime/ParryVisual/Editor 전용 PracticeCue, ParryInspector/4D1Tests, Parry.shader, Parry.asset 및 meta. 문서는 이 보고서·handoff/current task·기존 가드 보고서 후속 링크·승인 범위 설명만 갱신했다. 정확한 파일 목록은 `final-audit.json`에 기록한다.

staging/commit/push/reset/clean/merge/rebase/worktree/기존 QA 삭제 없음. 실제 사람의 타이밍 난이도·원속도 모션/섬광 직관성·컨트롤러 감각은 USER VERIFIED 대기다.
