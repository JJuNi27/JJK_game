# PASS 3C.1 — 공중 연계 피격자 높이 유지

2026-10-24 / `feat/shibuya-combat-mvp` / HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`.

**LOCAL UNCOMMITTED / targeted 범위 CODEX VALIDATED / PENDING USER PLAY REVIEW.**
이번 변경은 사용자가 요청한 공중 연계 피격자 높이 유지에 한정한다. 과거 PASS 4A 미해결 18건을 해결로 처리하지 않는다. 전체 회귀 190개 실행, staging/commit/push 없음.

## 실제 원인과 재현

기존 `ShibuyaAerialHoverRuntime.IsHeldByAerialHit`은 `IsStunned`를 요구했다. 마지막 타격 경직이 0.16초로 끝나면 공격자의 회복/후속 클릭 유예가 남아 있어도 피격자 중력이 재개됐다. `FinishAirAttack`도 `CancelCombo → CancelAerialHover → ReleaseAerialVictimHover`를 호출한 뒤 공격자만 `airGrace`로 되돌렸다. 유예 후 새 클릭은 `BeginCommonAirStep(false)`에서 다시 유지 관계를 해제했다.

수정 전 실제 W 어퍼컷→Space 추격→공중 LMB 경로에서 다음을 재현했다.

| 최초 클릭 종료 후 구간 | 공격자 Y | 피격자 Y |
|---|---:|---:|
| 수정 전 약 61.3ms | 4.596280 → 4.596280m | 4.786147 → 4.649133m |
| 수정 후 약 60.4ms | 4.607648 → 4.607648m | 4.838796 → 4.838796m |

기존에는 피격자만 약 **0.137m** 내려갔고 속도도 -1.446→-2.978m/s로 누적됐다. 수정 후 실제 두 CharacterController 이동 결과와 수직 속도 모두 유지됐다. 테스트는 화면상의 외형이나 내부 bool만으로 통과시키지 않았다.

## 최소 구조 변경

- 유지 관계는 기존 `heldAirVictim ↔ airHoldSource`를 재사용한다. 실제 Applied 공중 타격 후에만 성립하며, 기존 `sequenceToken`과 첫 공격의 절대 `airHoverDeadline` 사본을 피격자에게 연결했다.
- 개별 짧은 경직 종료를 높이 유지의 종료 조건에서 제외했다. 같은 유효 공격 세션의 준비·활성·회복·짧은 후속 입력 유예 동안 관계를 유지한다.
- 정상 공격 종료→입력 유예, 입력 유예→다음 클릭 전환에서만 관계를 보존한다. 일반 취소·피격·사망 등의 `CancelCombo()`는 기본적으로 계속 해제한다. 서로 다른 시점의 Transform 대입으로 높이를 보정하지 않는다.
- 기존 수직 이동 경로에서 `verticalSpeed=0`을 적용한다. 이동 소유자는 기존 ShibuyaFighter/CharacterController 하나다. 수평 넉백·충돌·판정은 그대로다.
- 공중 유지 관계가 유효한 동안만 행동 제한을 적용한다. `IsStunned`나 지상 경직 시간을 늘리지 않았다. 어퍼컷 피격자는 기존에도 airborne 상태로 제한됐으며, 일반 점프 피격자도 유효 관계 동안만 제한한다.
- 총 예산은 첫 공중 공격의 **unscaled 1.2초**다. 후속 클릭/타격/히트스톱으로 갱신하지 않는다. 후속 입력 유예 0.12초, 추격 후 최초 입력 창 0.55초는 변경하지 않았다.
- 거리·높이·벽·무적·착지·다운·비활성·사망·공격자 중단·예산/입력 유예 만료를 계속 검사한다. 다른 Applied 피해나 강제 피격은 기존 `OnDamageResolved`/`ApplyHitReaction`에서 즉시 이전 소유권을 해제한다.
- 락온 조작만으로 소유권을 옮기는 경로를 추가하지 않았다. 새 무브셋, 카메라 보정, 공격, 연출 시스템도 추가하지 않았다.

## 타격·데이터 보존

공통 2이벤트 공중 단계를 추가 유효 클릭으로 잇는 구조를 유지한다. 별도 3단계 무브셋으로 변경하지 않았다. 실제 PlayerInput의 새 클릭 3회로 **수락 3단계 / 이벤트 5개 / Applied 5개 / 피해 62**를 확인했다. 한 클릭은 그대로 최대 12+13 피해다.

연속 연계의 실제 높이를 관측한 428프레임에서 공격자·피격자 각각의 최대 Y 범위는 **0m**였다. 두 번째 단계 종료 후 35ms의 유효 입력 대기를 포함해 세 번째 클릭으로 연결했다. 절대 종료 시각은 갱신되지 않았고 상한 종료 후 양쪽의 정상 낙하를 검사했다.

Data Asset 변경 **0**. 기존 피해·이벤트·Collider·히트스톱·어퍼컷 높이·추격/접촉·지상 콤보·락온/카메라 설정 및 GUID를 보존했다. 기본 지상 4타 피해 85 검사도 통과했다. Dummy HP 1000 / Player HP 100 유지.

새 튜닝 필드를 추가하지 않았다. 기존 `holdVictimOnAirHit`의 한국어 표시만 **“유효 공중 연계 중 피격자 높이 유지”**로 바꿨다. 기존 공중 설정 자산의 스위치·1.2초·0.12초를 계속 사용하며 SerializedProperty/Undo 구조는 그대로다.

## 변경 파일 — 시작 시점 로컬 대비

기존 파일 6개:

- `unity/Assets/Shibuya/Scripts/ShibuyaAerialHoverRuntime.cs`: 세션/절대 상한 확인, 짧은 경직과 분리, 관계 유지/해제.
- `unity/Assets/Shibuya/Scripts/ShibuyaAerialCombatRuntime.cs`: 유효 후속 입력 유예로 넘어갈 때 관계 보존.
- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs`: 콤보 정리의 좁은 관계 보존 옵션. 기본 취소 정책 유지.
- `unity/Assets/Shibuya/Scripts/ShibuyaHitStateRuntime.cs`: 유효 공중 유지 관계 동안만 국소 행동 제한.
- `unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs`: 기존 필드의 한국어 설명 갱신.
- `unity/Assets/Shibuya/Editor/ShibuyaPass3CHoverTests.cs`: 요청으로 명시 변경된 경직/입력 유예 정책 검사 2개 갱신.

신규 3개: `unity/Assets/Shibuya/Editor/ShibuyaPass3C1Tests.cs`, 필요한 `.meta`, 이 보고서.

시작 스냅샷 180개 중 변경은 위 6개, 나머지 174개 SHA-256 동일, 누락 0개다. 기존 PASS 3D/4A/4A.1/4A.2.1 미커밋 작업, Scene/Prefab/Data Asset/기존 .meta/Packages/ProjectSettings/잠금 문서/고죠 VFX 변경 없음. 기존 전체 로컬 변경과 이번 추가 변경을 혼동하지 않는다.

## 집중 검증과 실패 이력

QA 루트: `unity/Logs/ShibuyaPass3C1QA/20261024-024800/`.

| 별도 실행 | PASS / FAIL / SKIP |
|---|---:|
| `before-fix/results.xml` — 기존 실제 하강 재현 | 0 / 1 / 0 |
| `after-fix-pilot/results.xml` — 높이 및 3클릭 5타 | 2 / 0 / 0 |
| `targeted/results.xml` — 신규 안전성 7 + 기존 영향 9 | 15 / 1 / 0 |
| `policy-and-landing-check/results.xml` — 착지 재검증 + 정책 2 + 지상 1 | 4 / 0 / 0 |

**수정 후 22회 실행, 최신 고유 21케이스 = 21 PASS / 0 FAIL / 0 SKIP.** 신규 9개 + 기존/갱신 12개이며 단일 전체 실행이 아니다. `validation-summary.json`이 각 원본 XML과 케이스를 연결한다.

최초 착지 검사 실패는 관측 시점 오류다. 첫 적중 직후 피격자를 바닥 위 0.03m로 옮긴 fixture에서, 원래 35ms 국소 히트스톱이 아직 진행 중인 두 프레임 뒤에 grounded를 요구했다. 실제 유지 관계는 이미 정상 해제됐지만 물리는 아직 정지 중이었다. 런타임/히트스톱/판정 기대값을 바꾸지 않고 정상 물리 재개와 실제 바닥 접촉까지 관측한 재검증이 통과했다. 실패 XML과 당시 테스트 소스 `targeted/test-source-before-landing-observation-fix.txt` 보존.

이전 `SuccessfulAttackGraceIsShortAndDoesNotHoldRecoveredDefender`, `StunExpiryReleasesVictimWhileAttackerRecoveryStillHovers`는 이번 요청과 반대인 “경직 종료 즉시 피격자 낙하” 정책을 요구했다. 원본 소스는 `before/`에 보존하고 새 명세에 맞게 이름과 검사를 갱신했다. 새 검사에서는 경직 종료 뒤에도 실제 Y 불변, 유예/상한 종료 후 실제 낙하를 함께 요구한다. 기존 실패를 억지로 통과시키기 위한 허용 오차 완화나 SKIP이 아니다.

확인한 안전 범위:

- 입력 미연결 시 짧은 유예 종료·정상 낙하, 반복 공격 요청으로 절대 상한 연장 불가.
- 거리/높이 이탈·실제 착지·중간 벽 추가 시 관계 해제와 벽 뒤 추가 피해 차단.
- 짧은 경직 대조에서도 높이 유지, 공격자 피격/비활성·피격자 무적/사망·전투 종료 시 해제.
- 제3자 출처의 Applied DamageContext 주입 시 이전 쌍의 참조 즉시 해제. 완전한 3인 무브셋 대전 검사는 아니다.
- 국소 히트스톱과 진단용 180ms 단일 메인 스레드 정체를 겹쳐 상한을 넘겨도 소유권 누수 없음. 실제 지속 저프레임 환경 전체를 보증하지 않는다.
- 일반 점프의 동일 공중 데이터/2이벤트/체공 유지, 허공 공격 후 추가 연계 없음.
- 시네마틱 후 체공 상한 종료, 다른 명시적 락온보다 기존 공중 접촉 우선, 최고 Pitch 화면 밖 복구 회귀 검사 통과.

## Compile / Console / 환경

- 마지막 compile: error 0 / warning 0. 새 검사와 기존 소스 모두 컴파일됨.
- 종료 후 큰 상태 조회 1회가 Pipeline 메인 스레드 5초 timeout으로 실패했다. 원본 `final-editor.json` 보존. 작은 읽기 조회는 정상 복구됐으며 추가 테스트 재실행은 하지 않았다.
- 최종 Console: **error 1 / warning 1**. 오류는 위 Pipeline 조회 timeout, 경고는 기존 `Deleting invalid font reference.`. 누적 버퍼 error 1 / warning 23이며 지우지 않았다. 따라서 `console error 0`으로 보고하지 않는다.
- 최종 Pipeline 정상, Play Mode 종료, active QA false, 미저장 씬 없음. 공중/기본 전투/Player/Dummy/Lock On 자산 5개의 dirty=false 확인.
- 이번에 AutoTick·프레임률·vSync·Play Unfocused 설정을 변경하지 않았다. 최종 fps -1 / vSync 0 / Time.timeScale 1 / AutoTick SessionState 저장값 없음(-1). Play Unfocused(2) 유지. 저프레임 진단은 테스트 안의 유한 정체이며 설정을 바꾸지 않았다.
- Computer Use, 패키지 변경, 포커스 강제 전환 없음. `final-editor-recheck.json`, `final-data-dirty.json`, `final-console.json` 참조.
- 최종 Git은 같은 브랜치/HEAD에서 staged 0 / tracked unstaged 28 / untracked 51. 기존 여러 PASS의 미커밋 변경을 포함한 전체 수다. `git diff --check` 오류 없음(기존 LF→CRLF 안내만 출력). 사용자 승인 없는 Git 변경 명령 없음.

## 사용자 Play Mode 확인

1. `ShibuyaCombatMVP`에서 W+4타 어퍼컷→새 Space 추격→도착 후 LMB 한 번을 사용한다.
2. 최초 2타 직후 다음 클릭 전 짧은 구간에 상대만 아래로 미끄러지지 않는지 본다. 정상 연타로 3클릭 총 5타를 확인한다.
3. 다음 클릭을 놓거나 늦게 입력하여 두 캐릭터가 정상 낙하하는지 확인한다. 총 1.2초는 유지되므로 클릭 사이 오래 기다리면 5타를 모두 끝내기 전에 예산이 만료될 수 있다.
4. 일반 점프 공중 LMB도 같은 높이 유지/무브셋을 사용하는지, 지상 기본·W 업·S 다운·락온 회전·최고 하이 앵글 복구가 유지되는지 본다.

사람이 느끼는 입력 여유/공중 손맛은 아직 USER VERIFIED가 아니다. 다인전의 복잡한 소유권 경쟁, 여러 높이의 움직이는 지형, 지속 저프레임/다양한 수동 클릭 간격은 이번 집중 검사의 보증 범위 밖이다. 과거 QA와 미해결 18건은 그대로 남긴다.
