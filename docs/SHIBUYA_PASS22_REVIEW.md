# 시부야 PASS 2.2 검증 결과

> 최신 상태 (2026-10-18): 사용자가 PASS 2~2.3 전체를 Play Mode에서 검수하고 현재 체술에 만족한다고 명시하여 **USER VERIFIED 체크포인트**로 승인했다. 아래는 구현 당시의 검증 기록이다. 최신 동작·기본값·승인 범위는 [통합 체크포인트](SHIBUYA_COMBAT_PASS2_23_CHECKPOINT.md)를 따른다.

CODEX VALIDATED / PENDING USER PLAY MODE REVIEW / NOT USER VERIFIED

Branch: feat/shibuya-combat-mvp. HEAD: 0f416389c602ed601aa6447238a520f007550ef0 유지.
staging / commit / push / Computer Use / 전역 Time.timeScale 변경 없음. PASS 3 미착수.

## 변경

- 1~3타의 기본 넉백 방향을 공격자 전방으로 변경. 피해 20/20/20/25, 넉백 속도 6/4/4/11, 경직 .25/.25/.25/.45초 및 기존 공격 타이밍/판정 크기는 유지.
- 유효 적중 뒤에만 타격 순간의 공격 축을 기준으로 피격자를 측면 정렬. 허용 측면 오차 .65m, 최대 이동 .20m, 속도 2m/s, 최대 .10초. 공격자 전방 넉백 정책에서만 적용하며 4타는 기본 비활성.
- 피격자의 CharacterController 소유자가 넉백과 정렬을 함께 처리. 피격자 시선은 바꾸지 않으며 벽/다른 전투원 충돌을 존중하고 막힌 이동을 축적하지 않음.
- 경직 종료, 무적, 거리 이탈, 공격자 중단/비활성/사망, 공격 교체 시 정렬 종료. 서로 다른 공격자가 적중하면 정렬 소유권을 합산하지 않고 짧은 충돌 억제 구간 동안 정렬을 중단.
- 준비/활성 중 좌우·후방 일반 이동 입력 영향도를 기본 0으로 설정. 공격 고유 전진은 유지하고 명시적 반대 입력은 추적 보정만 중단. 회복 구간에서 일반 이동을 점진적으로 복귀. 원본 이동 입력은 별도로 유지하며 W/S 분기는 구현하지 않음.
- 타격 강조 구체의 Collider를 생성 즉시 비활성화. 기존 Destroy 호출만으로는 프레임 끝까지 남아 CharacterController를 밀 수 있었음. 외형/사운드는 유지.

## 이번 PASS에서 실제 수정한 기존 파일

- unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs
- unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs
- unity/Assets/Shibuya/Scripts/ShibuyaPlayerInput.cs
- unity/Assets/Shibuya/Scripts/ShibuyaHitFeedback.cs
- unity/Assets/Shibuya/Data/Shibuya Combat Profile.asset
- unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs
- unity/Assets/Shibuya/Editor/ShibuyaPass21Tests.cs
- unity/Assets/Shibuya/Editor/ShibuyaPass2Tests.cs

기존 PASS 2.1 방향 입력 테스트는 새 요구사항(추적 중단/고유 전진 유지)에 맞췄다. 기존 PASS 2 독립 설정 테스트는 선택 가능한 radial 넉백 정책을 런타임 복제 데이터에서 명시적으로 계속 검사한다.

신규: unity/Assets/Shibuya/Editor/ShibuyaPass22Tests.cs, ShibuyaPass22TestRunner.cs 및 각각의 .meta.
그 외 Git dirty 항목은 작업 시작 전부터 있던 PASS 2/2.1 변경이다.

## Inspector

Shibuya Combat Profile의 한국어 SerializedProperty 편집 경로를 유지했다.

- 준비·활성 중 좌우 입력 영향도 / 후방 입력 영향도
- 피격자 측면 정렬 사용 / 허용 측면 오차 / 한 타격 최대 정렬 거리 / 정렬 속도 / 최대 시간
- 각 타격의 적중 후 피격자 측면 정렬 허용

기존 추적의 거리/각도/속도/시간 상한은 그대로이며 새 중복 조향 설정을 만들지 않았다. Undo 및 직렬화 테스트 통과. CharacterStatsProfile 공격·방어 배율은 이전과 같이 피해 계산에 사용하지 않는다.

## 최종 자동 검증

단일 최종 Unity Test Framework 실행: **21 PASS / 0 FAIL / 0 SKIP**.

- PASS 1: 2 PASS. 이동/달리기/점프/카메라/벽/피격/승패/재도전 및 씬 연결.
- PASS 2: 5 PASS. 개별 타격 데이터·타이밍·중복 방지, 입력 유지/연타/버퍼/초기화, 피격·사망·종료 취소, W+4타, Inspector/Undo.
- PASS 2.1: 7 PASS. 제한 추적/이탈/벽/반대 입력, 히트스톱 버퍼, 4타 정지와 넉백 복귀, 제3자 진행, 빗나감·무적, 반복 1타 행동 구간.
- PASS 2.2: 7 PASS. 정면/좌우 오차/좌우·후방 교대 입력의 4타, 정렬 상한/시선 유지, 반대·측면 입력 및 이동 복귀, 이탈·무적·공격자 중단, 벽/제3자 충돌, 다중 공격자, 신규 Inspector/사용자 값.

실제 discovery 21개와 execution 21개 및 results.xml을 확인했다. process exit 0만으로 PASS 처리하지 않았다.

관측:

- 정면, 좌우 .45m, 교대 입력 .30m 사례 모두 4회 적중/85 피해. 공격 사이 캐릭터 위치 재배치 없이 연결.
- 측면 정렬 사례: 약 .497m → .303m, 실제 약 .193m 보정. 프레임별 이동은 설정 속도×delta 상한 이내. 피격자 시선 유지.
- 오른쪽을 보며 반대/측면 입력: 준비·활성 중 측면 이동 0m, 회전 0도. 공격 고유 전진 약 .70m. 공격 후 일반 이동 복귀.
- 벽 사례 실제 측면 이동 약 .069m, 다른 전투원 사례 약 .044m. 요청한 정렬 이동을 충돌 때문에 다 쓰지 못해도 이후 튀어나오는 보정 없음.
- 4타 로컬 히트스톱: 설정 65ms / 측정 약 65.6ms. 제3자는 약 .315m 이동. Time.timeScale=1.
- 경직을 연장하지 않았다. 첫 타 사례에서 피격자가 행동 가능해진 뒤 공격자가 회복을 마치기까지 약 49ms가 남았고, 피격자의 실제 공격 진입을 확인했다. 위치 연결 안정성을 모든 타격이 회피 불가능한 확정 콤보라는 뜻으로 해석하면 안 된다.

최종 C# compile 실패 0, Console 오류 0. 현재 Console 경고 2개는 기존 `Deleting invalid font reference.`이다. Pipeline 세션 누적 경고는 46개이며 현재 Console 개수와 구분한다.

종료 시 Play Mode OFF / ShibuyaCombatMVP 씬 미저장 변경 없음 / Profile·Stats 미저장 변경 없음 / Dummy Stats 최대 체력 1000 / QA Profile 잔존 0 / 현재 씬 missing script 0.

작업 전 기존 996개 파일의 해시와 비교: 허용한 8개 기존 파일만 변경, 누락 0, 범위 밖 변경 0. 기존 더미 체력 파일, Gojo/VFX, USER VERIFIED, 씬·프리팹·Packages·ProjectSettings 변경 없음. 기존 사용자 dirty 변경은 보존했다.

## 증거와 초기 실패

- results.xml / discovery.json / execution.jsonl: 최종 21개 실행 결과
- validation-summary.json / preservation-check.json: 기계 판독 요약/파일 보존
- metrics.jsonl: 최종 위치·이동 측정
- final-editor-state.json / final-console.json: 종료 상태
- initial-results.xml / initial-execution.jsonl: 초기 4 PASS / 3 FAIL 보존
- targeted-results.xml: Collider 수정 후 신규 7 PASS / 0 FAIL

초기 실패 뒤 VFX Collider 문제를 수정했다. 신규 다중 공격자 테스트는 짧은 정렬 도중 프리팹을 import/생성하는 지연을 제거하기 위해 미리 생성한 뒤 실제 공격을 연계하도록 정리했다. 입력 테스트는 실제 입력 경계에서 수신된 것을 확인하며 다음 단계로 진행한다. 초기 실패를 최종 PASS로 덮어 숨기지 않는다.

## 사용자 Play Mode 검수 / 남은 한계

ShibuyaCombatMVP에서 W 없이 정면/조금 비스듬히 4타, 오른쪽을 본 상태에서 반대·좌우 입력 공격, W+연타, 4타 멈춤→큰 넉백, 회복 후 자유 이동을 확인한다. 조작감과 자연스러움은 사용자 검수 대기다.

현재는 고정 더미와 자동 재현 범위다. 실제 회피는 아직 없으므로 테스트용 대상 이동/Health 무적으로 이탈을 검사했으며, 실제 다인 PvP·모든 각도/거리·네트워크 동기화를 검증한 것은 아니다. 언급된 두 사용자 플레이 영상은 접근 가능한 파일로 식별하지 못해 분석 근거로 쓰지 않았다.

고급 무브셋, 순간이동, W/S 4타 분기, 회피/방어, 술식 연계는 구현하지 않았다.
