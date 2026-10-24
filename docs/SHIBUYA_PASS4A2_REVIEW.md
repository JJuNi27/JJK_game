# PASS 4A.2 — 느슨한 락온 전투 카메라

2026-10-23 / `feat/shibuya-combat-mvp` / HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`.

**LOCAL UNCOMMITTED / targeted 범위 CODEX VALIDATED / PENDING USER PLAY REVIEW.**
카메라 품질은 USER VERIFIED가 아니다. 이번 사용자 요청에서 명시한 PASS 4A.1의 빠른 락온 공격 회전 승인을 보호 기준으로 삼았으며 그 구현은 바꾸지 않았다. 과거 PASS 4A 전체 회귀 미해결 18건은 별도 검증 보류 상태로 유지한다.

## 원인과 변경 책임

기존 `AssistLockOnCamera`는 0.35초 수동 조작 유예 뒤 매 프레임 상대 방향의 절대 yaw/pitch로 수렴했다. 상대가 이미 잘 보여도 보정했고 화면 여유 영역은 없었다. `focus`는 플레이어 위치+고정 높이, 거리도 기본 6.5m여서 카메라→플레이어→상대가 일직선이 되기 쉬웠다. 몸을 월드 좌표로 정렬하는 코드가 원인은 아니었다. 높이는 pitch에 반영됐지만 상대를 중앙에 두는 방식이었고 focus·거리·겹침 보정은 없었다.

기존 소유권은 정상적으로 시네마틱/복귀가 먼저 반환하는 구조였다. 현재 `ShibuyaHitFeedback`은 타격음과 로컬 충돌 구체를 생성하며 카메라 Shake/FOV를 소유하지 않는다. 새 보정은 FOV와 전투 상태를 쓰지 않으며, 기존 시네마틱의 FOV 쓰기/복구 경로를 유지한다. Gojo/VFXLab/Domain 카메라는 변경하지 않았다.

## 동작

- 수동 조작 후 0.8초, MMB 재선택 중에는 자동 yaw/pitch·어깨·높이·거리 보정을 멈춘다. 기존 일반 추종은 유지한다.
- 상대 조준점이 가로 22~78%, 세로 28~72% 영역 안에 보이면 중앙 정렬하지 않는다. 영역 밖이면 가장 가까운 경계까지만 제한 속도로 추적한다. 몸에 가려지는 경우에는 영역 안에서도 겹침 완화를 수행한다.
- 두 CharacterController bounds의 8개 꼭짓점을 화면에 투영해 겹침 면적/작은 쪽 면적을 계산한다. 근거리 겹침에 한해 카메라 로컬 어깨 이동과 제한된 측면 orbit을 함께 사용한다. 월드 X축 오프셋이나 고정 40/60 배치가 아니다.
- 어깨 방향은 같은 선택 대상 동안 유지한다. 좌우 움직임마다 부호를 뒤집지 않는다. 방향/위치 보정은 속도 상한을 적용해 이동한다.
- 상대 높이 차이는 제한된 focus 높이 이동과 별도의 pitch 추적으로 처리한다. 근거리 겹침에 따라 기본 거리에서 최대 1m만 추가하며, 장거리 대상 때문에 계속 줌아웃하지 않는다. 50/60m 선택·유지 거리는 불변이다.
- 시선 중심→어깨 오프셋, 어깨 중심→카메라 arm, 코너의 대각 경로를 SphereCast로 제한한다. 충돌 시 즉시 안쪽으로 당기고, 공간이 확보되면 제한 속도로 arm 거리를 복구한다. 화면 투영에도 충돌로 짧아진 arm을 반영한다.
- 자동 보정의 한 프레임 시간 적분은 최대 0.05초다. 기본 100도/초에서 자동 회전은 한 프레임 최대 5도이며 거리/높이/어깨도 제한한다. 실제 저프레임 전투 전체의 안정성을 새로 보증하는 검사는 아니다.

## 시네마틱 소유권

우선순위는 시네마틱 → 복귀 블렌드 → 일반 전투 보조 그대로다. 시네마틱 중 새 보정 상태를 진행시키거나 Transform/FOV를 덮지 않는다. 복귀 목표에는 정지해 둔 기존 어깨·높이·거리 구도를 포함하여, 복귀 마지막 프레임과 일반 카메라 첫 프레임의 서로 다른 목표로 인한 점프를 막았다. 복귀 뒤 0.5초 자동 보정 대기를 추가했다. 카메라 소유자 변경/비활성화에서는 임시 구도 상태를 초기화한다.

## 파일

기존 로컬 파일 중 이번에 추가 수정한 6개:

- `unity/Assets/Shibuya/Scripts/ShibuyaOrbitCamera.cs`: 기존 일반 카메라 위치 결정을 충돌/구도 helper로 연결.
- `unity/Assets/Shibuya/Scripts/ShibuyaLockOnCameraRuntime.cs`: 기존 강제 중앙 추적을 제거하고 새 보조 연결. **Fighter partial은 시작 시점과 완전 동일**.
- `unity/Assets/Shibuya/Scripts/ShibuyaCinematicCameraRuntime.cs`: 복귀 목표 구도와 복귀 후 대기, 소유자 변경 시 상태 정리.
- `unity/Assets/Shibuya/Scripts/ShibuyaLockOnProfile.cs`: 기존 카메라 설정 재사용, 필요한 구도 설정 추가.
- `unity/Assets/Shibuya/Editor/ShibuyaLockOnInspector.cs`: 새 필드 한국어 SerializedProperty 편집.
- `unity/Assets/Shibuya/Resources/Shibuya Lock On.asset`: 카메라 기본값 추가 및 수동 유예 0.35→0.8초. 공격/탐지/거리/시야 상실 값 불변.

신규 5개:

- `unity/Assets/Shibuya/Scripts/ShibuyaLockOnFramingRuntime.cs` 및 `.meta`: 기존 OrbitCamera의 작은 보조 partial.
- `unity/Assets/Shibuya/Editor/ShibuyaPass4A2Tests.cs` 및 `.meta`: 실제 Play Mode 카메라 검사.
- 이 보고서.

새 카메라 GameObject/범용 카메라 시스템/Package/Scene/Prefab은 만들지 않았다. 기존 테스트와 보고서는 수정하지 않았다.

## 실제 한국어 Inspector와 기본값

`Assets/Shibuya/Resources/Shibuya Lock On.asset`에서 조절한다. 기존 SerializedProperty, Undo 및 다중 편집 유지.

| 표시 | 이번 값 | 이전 |
|---|---:|---:|
| 느슨한 락온 카메라 사용 | ON | 신규 |
| 마우스 조작 후 보조 대기 시간 (초) | 0.8 | 0.35 |
| 자동 추적 여유 영역 (가로·세로 반폭) | 0.28 / 0.22 | 신규 |
| 카메라 보조 강도 / 수렴 속도 / 초당 최대 회전 각도 | 0.7 / 5 / 100 | 동일 |
| 자동 회전 가속·감속 (도/초²) | 180 | 신규 |
| 겹침 완화 최대 어깨 이동 (m) | 2.2 | 신규 |
| 화면 겹침 완화 강도 | 1 | 신규 |
| 겹침 완화 최대 측면 구도 (도) | 32 | 신규 |
| 어깨·높이·거리 보정 속도 (m/초) | 3 | 신규 |
| 근거리 구도 보정 범위 (m) | 12 | 신규 |
| 추가 카메라 거리 상한 (m) | 1 | 신규 |
| 상대 높이의 시선 중심 반영 비율 | 0.2 | 신규 |
| 시선 중심 높이 보정 상한 (m) | 1 | 신규 |
| 시네마틱 복귀 후 보정 대기 (초) | 0.5 | 신규 |

카메라 기본 6.5m, 높이 1.25m, pitch 안전 범위 -25~70도, FOV 60은 변경하지 않았다. 2.2m는 실제 고정 이동량이 아니라 강도·거리·겹침에 따라 감소하는 상한이다.

## 검증과 실패 이력

QA 루트: `unity/Logs/ShibuyaPass4A2QA/20261023-172846/`.

| 별도 실행 | PASS / FAIL / SKIP |
|---|---:|
| `pilot/results.xml` | 1 / 0 / 0 |
| `targeted/results.xml` | 8 / 2 / 0 |
| `preconditions-recheck/results.xml` | 2 / 0 / 0 |
| `core-regression/results.xml` | 8 / 0 / 0 |
| `collision-projection-check/results.xml` | 3 / 0 / 0 |

**24회 실행, 최신 고유 19케이스 = 19 PASS / 0 FAIL / 0 SKIP.** 신규 카메라 11개 + 기존 8개다. 단일 전체 회귀 실행이 아니다. XML, discovery, execution 및 `validation-summary.json`에 실행별 결과를 보존한다.

최초 새 테스트 2개 실패:

- `ManualLookHasGraceAndSearchDoesNotRecenter`: 근거리 1.4m 상대를 둔 orbit 회전 뒤에는 상대가 여전히 보일 수 있는데, 유예 종료 후 반드시 자동 회전할 것을 기대했다. 새 lazy 정책을 검사할 화면 밖 전제가 없었다. 대상 15m로 조건을 명시하고 실제 뒤쪽 화면 좌표 z=-5.89를 확인한 뒤, 유예 유지→점진 추적과 재선택 수동 우선을 검증했다.
- `AirHeightTracksWithinPitchAndFocusLimits`: 공중 상대가 근거리 지상으로 돌아오면 여유 영역에 남을 수 있는데, 반드시 다시 pitch를 움직일 것을 기대했다. 새 검사에서는 아래쪽 상대의 화면 y=-0.059가 여유 영역 밖임을 먼저 확인하고 하향 추적을 검증했다. 이 두 실패를 해결하기 위해 런타임/승인값을 변경하지 않았으며 원본 XML을 보존했다. 환경 지연 탓이라고 분류하지 않는다.

마지막 코드 보완은 벽으로 짧아진 카메라 arm을 화면 투영에도 반영한 것이다. 관련 겹침·dead zone·벽 검사 3개를 별도 재검증했다.

확인한 수치/범위:

- 근거리 bounds 화면 겹침 0.8766→0.2373, yaw 약 -30.91도, 실제 어깨 보정 약 0.42m, 추가 거리 약 0.19m. 실제 캡처에서도 더미 얼굴/몸통이 드러났다. 완전 가림 제거를 모든 상황에서 보장하지 않는다.
- 화면 밖 추적의 관측 최대 프레임 yaw 변화 약 0.136도. 새 코드의 큰 프레임 적분 상한은 별도 적용한다.
- 공중 대상 검사 pitch 16→약 -0.09도, 아래쪽 목표로 약 2.23도까지 복귀. 기존 pitch 안전 범위 유지.
- 비중앙 dead zone 유지, 좌우 반복 이동 중 어깨 부호 유지, 장거리 줌아웃 상한, 벽/기둥/천장 충돌, 벽 제거 후 점진 복귀, 해제 후 자유 카메라 복귀 검사 통과.
- 시네마틱 복귀 마지막 프레임→일반 카메라 첫 프레임 위치 차이 0m / 회전 차이 0도, FOV 복구 확인.
- 기존 검사 8개: 락온 몸/카메라 90·180도 공격, 일반/W/S 4타 피해 85, 추격 대상 우선권, 추격 후 3클릭 5타, 시네마틱 우선/복귀 통과.

이전 `CameraAssistsYawPitchSmoothlyAndRespectsManualSearchAndClamp`는 상대를 여유 영역 안에서도 중앙으로 끌어오는 옛 정책의 기대를 포함한다. 해당 파일/기대값을 덮어쓰거나 이번 통과 수에 넣지 않았다. 새 정책은 새 검사로 기록했다. 과거 PASS 4A 전체 미해결 18건도 해결로 표시하지 않는다. 전체 190개 재실행 없음.

## 보호 및 최종 환경

보호 파일 48개, 기존 QA 472개, 최초 전체 실행 221개, 진단 14개, 기존 재검증 9개 해시 일치. 기존 PASS 4A.1 QA 90개도 보존 확인. 시작 스냅샷 112개 중 위 기존 6개만 바뀌었다. `preservation-audit.json` 참조.

공격 회전 2400도/초·180도·준비 상한 0.12초, 피해 20/20/20/25, Dummy HP 1000 / Player HP 100, 공중 이동/체공/접촉과 타깃 우선권, 탐지 50/60m·시야 유예 1.5초 불변. 기존 GUID/Scene/Prefab/Packages/ProjectSettings/Gojo/VFX/locked 문서 보존.

컴파일 성공. 최종 현재 Console error 0 / warning 1(기존 font reference 경고). 누적 오류 1건은 작업 전 07:23의 Play Mode 실행 전제 예외 기록이며 삭제하지 않았다. 전체 과거 이력이 오류 0이라는 뜻은 아니다.

AutoTick ON/16ms 및 저장 override 없음(-1), targetFrameRate -1, vSync 0, Time.timeScale 1 유지. 임시 환경 설정 변경 없음. Play Unfocused 유지, Computer Use/포커스 강제 호출 없음. 종료 시 Play Mode/활성 QA/미저장 씬/dirty Data Asset 없음을 확인한다. staged 0, 브랜치/HEAD 불변, commit/push 없음.

## 사용자 직접 Play Mode 검수

1. `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 열고 Play. 더미 근처에서 MMB를 눌렀다 놓아 락온한다.
2. 마우스로 옆/뒤를 본 뒤 손을 뗀다. 상대가 계속 보이면 중앙으로 돌아오지 않아야 한다. 상대가 시야 밖이면 0.8초 유예 뒤 부드럽게 따라와야 한다.
3. 근접 정면에서 플레이어 뒤 더미의 얼굴/몸통이 드러나는지, 좌우 이동 시 카메라가 좌우로 튀거나 어깨를 계속 바꾸지 않는지 본다.
4. W+4타 어퍼컷 후 위를 보는 느낌, Space 추격 후 가까운 공중 구도를 확인한다. 일반/W/S와 공중 3클릭 손맛이 유지되는지도 본다.
5. 건물/기둥/벽에 붙어 카메라가 안으로 당겨지고, 벗어날 때 점진적으로 돌아오는지 확인한다. MMB 짧은 클릭으로 해제한 자유 시점도 비교한다.
6. 카메라를 90·180도 돌린 직후 LMB: 승인된 빠른 락온 공격 방향이 유지돼야 한다.
7. 시네마틱은 Play Mode의 `JJK Game → 시부야 PASS3D → 시네마틱 예제 ON (Play Mode 전용)` 후 일반 4타 적중. 복귀가 튀지 않는지 확인. 같은 메뉴의 OFF로 해제한다.

남은 한계: bounds는 실제 모델/옷 전체 실루엣이 아닌 현재 전투 충돌체 기준이다. 근거리 가림은 감소시켰으나 복잡한 지형/다양한 캐릭터 체형/빠른 실제 공중전 전체 구도까지 시각 승인한 것은 아니다. 충돌로 매우 좁아진 공간에서는 완벽한 두 캐릭터 분리보다 카메라 관통 방지가 우선이다. 자연스러움·회전 속도·멀미·화면 구도는 사용자 검수 대상이다.

비교 이미지: `collision-projection-check/legacy/ShibuyaPass4A2QA/overlap-before.png`, `overlap-after.png` (위 QA 루트 기준).
