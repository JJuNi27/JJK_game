# 시부야 PASS 2.3 — 방향별 피격 리액션

> 최신 상태 (2026-10-18): 사용자가 PASS 2~2.3 전체를 Play Mode에서 검수하고 현재 체술에 만족한다고 명시하여 **USER VERIFIED 체크포인트**로 승인했다. 아래는 구현 당시의 검증 기록이다. 최신 동작·기본값·승인 범위는 [통합 체크포인트](SHIBUYA_COMBAT_PASS2_23_CHECKPOINT.md)를 따른다.

상태: CODEX VALIDATED / PENDING USER PLAY MODE REVIEW / NOT USER VERIFIED.

## 구현

실제 적중이 확인된 뒤 ShibuyaFighter가 ShibuyaHitReaction 이벤트로 공격자 위치, 타격 진행 방향, 피격자 로컬 방향, 일반/4타 강타 여부, 국소 히트스톱 여부를 전달한다. 타격 진행 방향이 우선이며 방향이 없는 경우 공격자 위치를 기준으로 보완한다. 기존 IHitReactable 호출 형태는 유지했다. 충격·경직이 모두 0인 취소 호출은 새 피격 이벤트를 발행하지 않는다.

- 정면: 상체·머리가 뒤로 젖혀짐.
- 왼쪽: 상체·어깨가 오른쪽으로 비틀리고 기울어짐.
- 오른쪽: 반대로 비틀리고 기울어짐.
- 후방: 상체·머리가 앞으로 쏠림.
- 대각선: 방향 성분을 연속적으로 혼합. 네 방향 중 하나로 강제 스냅하지 않음.

ShibuyaBlockVisual은 기존 블록 rig의 몸통·머리·팔을 다리 연결 높이를 기준으로 변형한다. 새 Scene/Prefab/rig 계층은 만들지 않았다. CharacterController와 캐릭터의 실제 시선·위치는 이 모션 때문에 회전/이동하지 않는다. 기존 넉백·측면 정렬은 ShibuyaFighter가 계속 소유한다.

매 프레임 캐시한 기본 자세에서 계산하므로 회전/위치가 누적되지 않는다. 연속 피격은 현재 표시 자세에서 새 목표 자세로 전환한다. 피격 경직 안에서 복귀하며, 비활성화·사망·전투 종료·재시작 시 피격 상태를 해제한다. 기존 사망 시 쓰러지는 외형 표현은 유지한다.

4타는 동일 방향 계산에 더 큰 시각 배율을 적용한다. 기존 국소 히트스톱이 있는 타격은 강한 피격 자세를 정지 시작부터 표시하고, 피격자 로컬 CombatTime으로 고정·회복한다. 전역 Time.timeScale은 수정하지 않았다.

## 한글 Inspector

Assets/Shibuya/Data/Shibuya Combat Profile.asset:

| 설정 | 초기값 |
|---|---:|
| 방향별 피격 모션 사용 | ON |
| 정면 / 왼쪽 / 오른쪽 / 후방 반응 강도 | 각각 1 |
| 상체 기울기 / 상체·어깨 비틀림 | 18도 / 20도 |
| 피격 자세 진입 / 회복 시간 | 35ms / 180ms |
| 일반 / 4타 강타 반응 배율 | 1 / 1.8 |

히트스톱이 있는 타격은 진입 지연 없이 강한 자세부터 고정한다. 회복 시간은 남은 피격 경직 안으로 제한된다. 기존 SerializedProperty 편집과 Undo를 유지했고 한글 매핑/직렬화/Undo를 검사했다. 기존 VFX 연출 강도와 새 몸 자세 배율을 중복 곱하지 않는다.

## 실제 변경 파일

- unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs
- unity/Assets/Shibuya/Scripts/ShibuyaBlockVisual.cs
- unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs
- unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs
- unity/Assets/Shibuya/Data/Shibuya Combat Profile.asset
- 신규 unity/Assets/Shibuya/Editor/ShibuyaPass23Tests.cs 및 .meta
- 신규 unity/Assets/Shibuya/Editor/ShibuyaPass23TestRunner.cs 및 .meta

기존 PASS 1~2.2 테스트 소스는 변경하지 않았다. QA 생성 결과는 이 폴더에 기록했다.

## 최종 검증

단일 최종 Unity Test Framework 실행: **25 PASS / 0 FAIL / 0 SKIP**.

- 신규 4개: 방향별 실제 적중/대각선/충돌체 시선 유지/넉백 0일 때도 모션, 4타 반복 갱신 및 정지 중 자세 고정, 연속 피격·설정 OFF·비활성화·사망·종료·재시작 초기화, 한글 설정·Undo·기존 전투 수치 보존.
- 기존 21개: PASS 1 이동/점프/카메라/피격/승패/재도전, PASS 2 콤보/개별 수치/버퍼/초기화/다중 Collider 중복 방지, PASS 2.1 추적/히트스톱/버퍼/회피 상황/벽/행동 가능 구간, PASS 2.2 정렬/반대 이동/4타 연결/다중 캐릭터 충돌 회귀.

discovery 25개 / execution 25개 / 실제 XML 결과를 확인했다. process exit 0 또는 내부 flag만으로 PASS 처리하지 않았다.

측정:

- 정면/후방 상체 기울기 약 -17.8 / +17.8도. 머리가 각각 뒤/앞으로 약 27.5cm 이동.
- 좌우 어깨 비틀림 약 +19.8 / -19.8도, 측면 기울기는 반대 부호. 머리 좌우 이동 약 26cm.
- 대각선에서는 앞뒤/좌우 기울기가 함께 적용됨.
- 캐릭터 실제 방향 변화 0도. CharacterController 크기 유지.
- 일반 피격 약 17.9도, 4타 약 32.4도. 실제 4타 85 피해 유지.
- 국소 히트스톱 측정 약 65.8ms. 그동안 머리 위치/회전과 로컬 시계 및 해당 피격자의 실제 이동이 고정되고, 종료 후 넉백·자세 회복이 재개됨. Time.timeScale=1 유지.

컴파일 실패 0 / 현재 Console 오류 0 / 현재 경고 2. 경고는 기존 `Deleting invalid font reference.`이다. Pipeline 세션 누적 경고 77개와 현재 Console 개수를 구분한다.

종료 상태: Play Mode OFF, ShibuyaCombatMVP 씬 미저장 변경 없음, Profile/Stats 미저장 변경 없음, Dummy Stats 최대 체력 1000, QA Profile 잔존 0, 현재 씬 missing script 0.

작업 전 기존 1000개 파일의 해시와 비교해 허용한 기존 5개 파일만 변경됐고 누락/범위 밖 변경은 0이다. Dummy Stats 1000, 기존 1~4타 공격/이동/추적/정렬/경직/넉백/히트스톱 값, 씬·프리팹, Gojo/VFX 및 USER VERIFIED 파일을 보존했다. 기존 dirty 작업도 보존했다.

Branch feat/shibuya-combat-mvp / HEAD 0f416389c602ed601aa6447238a520f007550ef0 유지. Staged 0. commit/push/Computer Use/PASS 3 구현 없음.

## 증거와 한계

- results.xml / discovery.json / execution.jsonl: 최종 25개 결과
- metrics.jsonl: 실제 자세·위치·히트스톱 측정
- validation-summary.json / preservation-check.json: 최종 상태와 파일 보존
- final-editor-state.json / final-console.json / final-warnings.json: Editor 종료 증거
- initial-results.xml: 최초 3 PASS / 1 FAIL. 효과 없는 취소 호출도 새 피격 이벤트로 세던 문제를 수정했다.
- direction-results.xml: 해당 검사만 재실행해 1 PASS / 0 FAIL

절차적 블록 캐릭터 피격 표현이며, 전용 캐릭터 피격 애니메이션을 제작한 것은 아니다. 자동 검사는 실제 Update/LateUpdate와 변형된 상체·머리 Transform을 확인했지만, 자연스러움·손맛에 대한 사용자 승인을 대신하지 않는다.

사용자 확인: ShibuyaCombatMVP에서 더미의 정면/좌우/후방으로 돌아가 공격하고 대각선 반응, 연타 중 자세 갱신, 4타의 강한 자세 고정 후 넉백·복구를 확인한다. Profile의 강도·각도·진입/회복 시간과 모션 OFF도 직접 조절할 수 있다.
