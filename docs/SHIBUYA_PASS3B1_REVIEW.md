# Shibuya PASS 3B.1 — 근접 추적 / 공중 궤도 / 바닥 균열

기준: `7e0e4146de9ee1720f435bc84c2af7751e01876b`, `feat/shibuya-combat-mvp`.
기존 PASS 3B 미커밋 작업을 보존한 후 추가했다. 기존 22개 변경 파일 사본과 시작 시점 tracked 파일 1,024개의 SHA-256은 `unity/Logs/ShibuyaPass3B1QA/pre-work/`, `before.json`에 있다.

상태: **USER VERIFIED — 아래 직접 승인 범위 한정 / 자동 검증 경계 조건은 CODEX VALIDATED**.

## 사용자 최종 승인 — 2026-10-19

사용자가 Unity Play Mode에서 PASS 3B와 PASS 3B.1을 직접 플레이하고 다음 항목이 원하는 방향대로 동작한다고 승인했다.

- 기존 일반 4타 체술
- W+4타 어퍼컷
- S+4타 다운 콤보
- 개선된 어퍼컷 높이와 착지 흐름
- 평타 대상 추적 및 유도
- 다운 공격의 일시적 바닥 균열
- 전체적인 조작감과 전투 흐름

위 항목을 **USER VERIFIED** 체크포인트로 기록한다. 사용자가 직접 검증하지 않은 극단적 충돌·밀집·경계·무적·저프레임 등의 세부 조건은 포함하지 않는다. 해당 자동 검증 결과는 확인한 범위에서만 **CODEX VALIDATED**이며 모든 경우를 보장하지 않는다.

이번 체크포인트 검토에서는 테스트를 재실행하지 않고 아래 저장된 discovery/execution/XML 및 케이스별 최신 결과를 교차 확인했다. 일반 피해 20/20/20/25, Dummy HP 1000, Player HP 100을 확인했고, Gojo/VFX·Scene·Prefab·Packages·ProjectSettings·기존 GUID와 기존 잠금 문서는 변경하지 않았다.

## 원인과 변경

기존 보정은 직전 적중 대상에만 적용되고, 한 타격 시작 시 정한 전방·짧은 준비 창·단계별 대상 참조에 의존했다. 첫 타격 후보 선정이 없었으며 명시적 무브셋은 별도 ApproachLastHit 이동만 사용했다. 따라서 근접 추적과 authored 접근의 적용 범위가 달랐다.

- `ShibuyaTargetingRuntime`을 모든 타격 이벤트의 준비 구간에 적용한다. 공격자의 정면 각도, 거리, 장애물, 높이와 보정 후 실제 공격 구의 도달 가능성으로 첫 후보를 고른다. 회전과 접근은 독립 예산으로 제한하고 CC 단일 이동 경로로 처리한다. 활성 구간 이후에는 새 방향으로 회전하지 않는다.
- 콤보의 최초 유효 적중 대상(`comboTarget`)과 실제 직전 적중 대상(`lastHitTarget`)을 분리했다. 일반 추적은 전자를 유지하며, 다른 적이 가까워져도 자동 전환하지 않는다. PASS 3A의 명시적인 `ApproachLastHit`는 기존 후자 의미를 유지한다. 해당 이동이 필수인 이벤트에는 자동 접근 예산을 중복 적용하지 않는다. 물리 범위 안의 실제 다중 대상 적중 자체를 금지하지 않는다.
- 이벤트 직전까지 준비 구간에서 현재 위치와 제한된 속도 예측을 다시 계산한다. 범위·각도·높이·벽·무적 이탈 시 해당 이벤트 보정을 종료하고 재탐색하지 않는다. 사망/종료 대상은 즉시 유효 대상에서 제외한다. 후속 입력 예약/확정 연계 시간 상한은 바꾸지 않았다.
- 개별 `strike.overrideTargeting`을 켜면 해당 타격의 추적 설정을 사용한다. 꺼져 있으면 Combat Profile의 공통 설정을 사용하며 개별 설정은 Inspector에서 감춘다. 기본 4타와 명시적 이벤트가 같은 로직을 사용한다.

공통 설정 변경(승인된 이동·회전 범위): 유지 거리 **2.7m 유지**, 유지 각도 **35→70도**, 누적 회전 한도 **18→45도**, 회전 속도 **180→540도/초**, 추가 접근 **0.16→0.30m**, 접근 속도 **3→4m/s**, 준비 중 허용 창 **0.07→0.16초**(실제 준비 시간까지만). 첫 선정 정면각 **50도**, 마지막 적중 후 유지 **1.2초**, 높이 차 **1.4m**, 예측 **0.05초**. 명시적 방향 입력 우선 각도 **60도 유지**. 원래 타격 전진 거리와 속도, 피해, 타이밍 및 넉백은 그대로다.

## 어퍼컷

- 기존 속도 방식도 유지한다. 기본 어퍼컷만 목표 높이 모드를 사용해 중력으로 초기 속도를 계산한다. 목표 높이 **5.5m**, 중력 **25**, 정점 체공 **0.06→0.12초**. 기존 속도 11 방식의 측정 높이는 약 2.40m였다.
- 상승 중 수평 거리 **2.4m / 0.55초**, 낙하 수평 속도 **1.2m/s**, 착지 캡슐 분리 속도 **8m/s**를 독립 설정한다. 기존 단순 감쇠 넉백 대신 발사 상태의 수평 이동이 소유한다.
- 발사 방향은 실제 타격 방향을 직접 전달한다. 일반 감쇠 넉백 속도를 0으로 하거나 피격자의 시선이 옆을 향해도 발사 방향이 바뀌지 않는다. 띄우기에서는 사용하지 않는 감쇠 넉백 속도를 Inspector에서 감춘다. 명시적으로 작성한 거리 넉백은 별도 추가 예산임을 Inspector에서 안내한다.
- 하강 시 실제 캡슐 간격을 검사해 옆으로 제한된 물리 이동을 요청한다. 머리 위 접촉을 지면 착지로 처리하지 않는다. 유효한 지면 법선/바닥 접촉을 확인해야 공중 상태가 끝난다. CC.Move, 벽·천장 충돌은 유지한다. 위치 대입이나 충돌 무시는 런타임에 사용하지 않는다.
- 공중 추격 입력·공중 콤보는 구현하지 않았다. 블록 캐릭터 모션/카메라도 추가 제작하지 않았다.

## 다운 균열

`ShibuyaGroundImpact`는 피격자당 메시/재질 한 세트를 재사용하는 임시 바닥 표시다. S의 유효 적중 후 **실제 지면 접촉**에서만 생성한다. 바닥 위에 방사형 균열 메시를 얹고 유지 후 페이드한다. 지형·콜라이더·Renderer Feature는 수정하지 않는다.

- 반경 **1.8m**, 유지 **0.65초**, 페이드 **0.65초**, 진하기 **0.85**. 해당 다운 이벤트에서 조절한다.
- 벽, 다른 캐릭터, 바닥 없는 공간은 생성 대상이 아니다. 균열 선분의 지면 적합성을 검사해 바닥 밖으로 이어 붙이지 않는다.
- 사망/전투 종료/비활성화 시 표시를 끄고, 소유자 파괴/씬 종료 시 생성 메시·재질을 해제한다. 한 피격자의 짧은 반복 충돌은 기존 표시를 재사용한다.

## 공개 참고 근거와 제한

- [Sparking! ZERO 공식 가이드](https://en.bandainamcoent.eu/dragon-ball/news/dragon-ball-sparking-zero-combos-and-features): Step-in Rush와 날아간 상대를 쫓는 Dragon Homing을 구분한다. 이번에는 근접 준비 구간의 유한한 접근만 다루며 장거리 공중 추격은 보류했다.
- [Epic Motion Warping 문서](https://dev.epicgames.com/documentation/unreal-engine/motion-warping-in-unreal-engine?lang=ko): 정해진 구간에서 타깃에 맞춰 이동/방향을 정렬하는 공개 원리만 참고했다. Unreal 기능을 설치하지 않았고 상용 게임의 내부 구현을 확인했다고 주장하지 않는다.
- 지정 [Storm 4 영상](https://www.youtube.com/watch?v=6IELTcZ1x0k&t=669s)은 이번 웹 접근에서 오류로 내용을 확인하지 못했다. 해당 영상의 정확한 높이·속도·내부 추적 알고리즘을 측정하거나 복제했다는 주장은 하지 않는다. 높이/궤도는 사용자 피드백을 반영한 검수용 초기값이다.

## 검증 기록

- 신규 focused: 18 PASS / 0 FAIL. `targeted-initial-18/`.
- 보강 focused: 실제 입력 전진·낙하 겹침·바닥 부재/벽의 3 PASS / 0 FAIL. `targeted-extra-3/`. 이 중 1개는 기존 신규 사례 재실행이므로 신규 고유 케이스는 총 20개다.
- 전체 회귀: 기존 62 + 신규 20 = **82개 discovery / 78 PASS / 4 FAIL / 0 SKIP**. `regression-initial-82/`에 실패 원문을 보존했다.
- 4건은 새 첫 타격 회전/추가 접근과 이전 검사의 고정축·단독 이동 예산 전제가 충돌했다. 피격자 정렬 3건은 runtime clone에서 추적을 꺼 기존 고정축 검증을 격리했다. 실제 회전된 타격의 넉백 X 성분까지 정렬 오류로 취급하지 않도록 한 것이다. 작성 이동 예산 검사는 해당 이벤트의 자동 접근/회전을 0으로 두고 기존 이동·벽 충돌·넉백 수치 assertion을 유지했다. 정렬/충돌 알고리즘을 이 실패를 숨기기 위해 변경하지 않았다.
- 재검증: 실패 4건 + 관련 검사 + 새 추적/다중 공격자 결합 사례 = **10 PASS / 0 FAIL**. `recheck-final-10/`. 추적을 켠 결합 상황도 따로 실행했다. 결합 사례의 두 번째 적중 시 이전 정렬은 이미 종료된 상태였으며, 실제 활성 소유권 충돌의 취소는 분리된 기존 다중 공격자 검사에서 확인했다.
- 마지막 발사 방향 독립성 수정 후 관련 공중·다운·사망·균열 검사 **7 PASS / 0 FAIL**. `air-final-7/`.
- **고유 83개 케이스의 최신 결과가 모두 PASS / 0 FAIL**이다. **단일 83개 실행은 아니다.** `latest-per-case-83.json`이 실행별 결과를 연결한다. 기존 62개 이름 누락은 0개다. focused 실행 수를 더해 고유 개수를 부풀리지 않았다.
- `discovery.json`, `execution.jsonl`, `results.xml`이 실제 발견/실행/결과의 근거다. focused 재실행을 더해 고유 테스트 수를 부풀리지 않는다.
- 기존 검사 두 곳의 의미를 갱신했다. PASS 2.1의 첫 타격 전 자동 선정 금지 검사는 이번 승인에 맞춰 제한된 선정/회전 상한을 확인한다. PASS 3B의 사용자 지정 상승 속도 7 검사는 목표 높이 모드를 끄고 기존 속도 모드를 명시한다. 검사를 삭제하거나 SKIP 처리하지 않았다.
- 기존 작은 QA 결과가 덮어써지기 전에 `pre-regression-evidence/`로 보존했다.

최종 관측:
- 첫 타격 0/30도는 적중, 60/90도는 합법적인 빗나감. 30도 사례의 회전 약 33도/추가 접근 0.30m. 카메라 회전 후 첫 대상 유지, 다른 가까운 적으로 자동 전환 없음.
- 일반 발사 최고 높이 약 **5.43~5.47m**, 천장 제한 약 **1.04m**. 수평 발사와 낙하 드리프트를 모두 0으로 만든 강제 겹침에서도 약 **0.69m**의 캡슐 분리 요청 후 지면 Y≈0.03m로 착지했다. 단순히 기본 수평 발사가 멀리 보내서 성공한 검사로 끝내지 않았다.
- 실제 입력으로 전진한 공격자는 약 6.20m 이동했으며 피격자는 지면에 착지했다. 균열은 실제 바닥에서만 생성/페이드/메시 재사용되고, 벽·바닥 부재·빗나감·무적에서는 생성되지 않았다.
- `final-audit.json`: Play Mode=false, sceneDirty=false, dirtyAssets=[], 누락 스크립트 0, Time.timeScale=1, 추가 균열 Mesh/Material **0/0**. URP Unlit shader 해석/지원 정상, shader error 0.
- `final-console.json`: compilationFailed=false, compiling=false, Console error **0**, warning **2**. 경고는 기존 `Deleting invalid font reference.`이며 세션 전체 반복 기록은 보존했다. Console을 비워 오류 0으로 만들지 않았다.
- 시작 시 tracked 1,024개 대조에서 Shibuya 범위 밖 변경/누락/기존 `.meta` 변경은 0. Scene/Prefab/Gojo/VFX/Packages/ProjectSettings/잠금 문서 및 HP 자산은 보존했다. 기본 피해 20/20/20/25, 준비·활성·회복 시간, 넉백 6/4/4/11, 강타 히트스톱 0.065초, Dummy 1000/Player 100을 확인했다.

## 사용자 Play Mode 재현 방법

기존 `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 그대로 Play한다.

1. 정면 또는 약간 비스듬한 거리에서 W 없이 4타. 첫 적중 후 카메라만 돌리거나 작은 위치 편차가 생겨도 후속 타격을 확인한다. 크게 옆/뒤로 빠지거나 장애물이 있으면 빗나가야 한다.
2. W+4타: 높아진 발사 후 그대로 서 있기/앞으로 이동하기. 상대가 머리에 남거나 바닥을 관통하지 않는지 본다.
3. S+4타: 실제 바닥 접촉에 균열이 생겼다가 사라지는지 본다. 다시 사용해도 중복 오브젝트가 쌓이지 않아야 한다.
4. Combat Profile의 공통 추적, 타격별 개별 추적, Ground Finishers의 공중/다운 설정에서 한국어 Inspector로 조절한다. 진단 Gizmos는 Combat Profile에서 기본 OFF이며 필요할 때 켠다.

## 한계

자동 검사 통과 자체는 USER VERIFIED가 아니며, 이번 승인은 위 사용자의 직접 플레이 결과에 근거한다. 서로 여러 겹으로 막힌 극단적 밀집 상황까지 위치 분리를 보장하는 군중 물리 시스템은 만들지 않았다. 다운 외형과 충돌 캡슐은 기존처럼 분리되어 있다. Player 빌드/셰이더 스트리핑/네트워크 검증은 이번 범위 밖이다.

구현 당시 staging/commit/push는 하지 않았다. 이번 별도 승인에서는 PASS 3B/3B.1의 관련 34개 파일만 선택적으로 저장하고 `origin/feat/shibuya-combat-mvp`에 일반 push한다. QA Logs·임시 영상·캐시는 포함하지 않는다. PASS 3C는 시작하지 않는다.

## PASS 3B.1 file scope

Existing files changed (14):

- `unity/Assets/Shibuya/Data/Shibuya Combat Profile.asset`
- `unity/Assets/Shibuya/Data/Shibuya Ground Finishers.asset`
- `unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaMovesetInspector.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass21Tests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass22Tests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3ATests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3BTests.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaFinisherSet.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaHitStateRuntime.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaMovesetDefinition.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaMovesetRuntime.cs`

New files (9):

- `docs/SHIBUYA_PASS3B1_REVIEW.md`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3B1TestRunner.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3B1TestRunner.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3B1Tests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3B1Tests.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaGroundImpact.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaGroundImpact.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaTargetingRuntime.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaTargetingRuntime.cs.meta`
