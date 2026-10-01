# Purple Ingredient FlowFirst exploration

2026-09-26 — CODEX VALIDATED (targeted technical/regression scope)

USER VISUAL REVIEW PENDING / NOT USER VERIFIED. Production integration 없음. 저장 기본 OFF. Commit/push/staging 없음.

## Change Contract

- Target gap: Blue/Red의 번개·고체 조각 중심 표현을 흡인/척력의 짧고 거친 곡선 흐름으로 바꾼 별도 후보.
- Allowed scope: 새 FlowFirst profile/runtime/shader/Inspector/QA, 기존 ingredient와 sequence의 opt-in 연결만.
- Must preserve: Production, Final2 및 기존 Outer/Hybrid/Expanded/ElectricArc 후보, core mass/크기/궤적/타이밍, gameplay/camera, Purple body/Birth/Travel/terminal.
- Expected visible change: Blue는 안으로 가속하는 청록 흐름, Red는 밖으로 확장하는 진홍 압력 곡선. 작은 orb/짧은 flare, 융합 시 재료 산란, 완성 hold 이전 Purple spike/tear.
- Failure condition: 긴 리본/고체 shard/단순 공전으로 읽힘, 보호 구간 변화, 자원 누적. 자동 테스트가 시각 실패 여부를 대신 판정하지 않음.
- Required evidence: 단계별 실제 Play Mode 캡처, Final2 vs FlowFirst Side/Caster A/B, 위치/크기 및 2초 이후 RGB 비교, 실제 실행된 targeted tests와 cleanup.

## 구현

- Blue: 무작위 3D 평면의 짧은 곡선 brush가 바깥에서 안으로 가속하며 소멸. 작은 cyan orb는 짧게 머물다가 흡수. 약한 지면 흡인 결, 간헐적 cross flare.
- Red: 표면에서 바깥으로 확장하는 짧은 bowed pressure front. Blue와 다른 속도/위상/폭 변화. 작은 red orb와 flare, 약한 바깥 방향 공간 흐름.
- Fusion: 기존 첫 접촉 계산을 사용해 Blue/Red orb 위치에서 일부를 산란. 색 잔광과 flare가 잠깐 남음. 기존 collision accent/timing은 그대로.
- Purple: 기존 성장 크기를 읽어 표면 바깥에 비균일 spike/짧은 tear 추가. 기존 본체나 Birth를 바꾸지 않고 2.0초 completed hold 전에 모두 종료. 별도 black debris는 추가하지 않음.
- 새 layer는 LineRenderer와 수동 sampled ParticleSystem을 사용. Hybrid/Expanded/ElectricArc layer를 재사용하지 않음. 새 custom mesh 생성 없음.
- `PurpleIngredientFlowFirstProfile.asset`의 `candidateEnabled: 0`. QA에서만 임시 활성화 후 원래 profile 선택 상태 복원. Inspector는 scoped 한국어 CustomEditor.

## 실제 변경 파일

기존 파일의 최소 연결 변경:

- `unity/Assets/Scripts/Player/PurpleFusionIngredient.cs`
- `unity/Assets/Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs`

신규 파일 (각 Unity 파일의 `.meta` 포함):

- `unity/Assets/Scripts/Player/PurpleIngredientFlowFirstProfile.cs`
- `unity/Assets/Scripts/Player/PurpleIngredientFlowFirstVolume.cs`
- `unity/Assets/Scripts/Player/PurpleIngredientFlowFirstFusion.cs`
- `unity/Assets/Resources/VFX/PurpleIngredientFlowFirstProfile.asset`
- `unity/Assets/Resources/VFX/PurpleIngredientFlowBrush.shader`
- `unity/Assets/Resources/VFX/PurpleIngredientFlowSprite.shader`
- `unity/Assets/Editor/PurpleIngredientFlowFirstProfileEditor.cs`
- `unity/Assets/Editor/PurpleIngredientFlowFirstTests.cs`
- 이 보고서.

## Technical / Regression

- 최종 compile: completed, compilationFailed=false, C# errors 0.
- 실제 NUnit 실행: **2 PASS / 0 FAIL / 0 SKIP** (`targeted_final.json`).
  - `PurpleIngredientFlowFirstTests.CandidateIsSeparateDefaultOffAndReleasesMaterials`
  - `PurpleIngredientFlowFirstTests.OppositeFlowDirectionsFusionScatterAndRepeatedCleanup`
- 새 brush/sprite 및 사용한 filament shader의 error 0을 테스트에서 확인.
- 최초 실행은 1 PASS / 1 FAIL: Edit Mode의 정지 ParticleSystem에서 SetParticles 후 particleCount=0. 단독 재현으로 확인. 해당 검사는 실제 Play Mode 캡처로 이동했으며 VFX 튜닝으로 우회하지 않음.
- 실제 Play Mode에서 1.6초 fusion particle 20개 확인, Side/Caster 각각 assertion 통과 (`*.particles.json`).
- 반복 생성/종료 3회 material 수 baseline 복귀. 최종 Play Mode 캡처 종료 뒤 새 Volume 0, Fusion 0, 소유 material 0 (`playmode_cleanup.json`). 새 런타임은 Mesh를 직접 생성하지 않음.
- Side/Caster 각각 97개 프레임의 Blue/Red/Purple position, scale, active 기록 동일.
- Side/Caster 각각 **2.0–3.2초 37프레임 RGB changed channels=0**. 이 구간은 완성 hold/초기 release를 포함하며 전체 비행/terminal 전 구간 테스트는 아님.
- 시작 시 hash 358개 비교: 허용된 공유 연결 파일 2개만 변경. 기존 후보/production 자산, standalone Blue/Red, Scene/Packages/ProjectSettings/TimeManager의 기존 로컬 내용 보존 (`protection_hash_audit.json`).
- Final2 OFF 경로 선택 및 소유 material 회귀 검사 PASS. 전체 gameplay/terminal regression은 실행하지 않음.
- Console을 무조건 0이라고 보고하지 않음: 최종 렌더 중 Pipeline `/api/exec`의 5000ms 응답 시간 초과 **1건**. 실제 캡처 4개 raw, pose/particle/format 결과는 완결됐으며 97프레임 길이와 영상 인코딩 검증 완료. VFX 런타임 예외는 관찰되지 않음. 과거 수정 중 compile 오류도 console history에 남아 있음 (`console_final.json`).
- 원래 VFXLab Scene을 저장 없이 다시 열었고 isDirty=false 확인. 기존 dirty working tree/index 보존.

## 비교 자료

LEFT = BASELINE (Final2), RIGHT = CANDIDATE (FlowFirst). 화면 직접 라벨 표시.

동일 카메라/seed/30fps, 1920×540, 97프레임, 3.233초, 원속도.

- `D:\JJK_game\unity\Logs\PurpleIngredientFlowFirstQA\AB\20260926_104453_929\Side_AB_Full.mp4`
- `D:\JJK_game\unity\Logs\PurpleIngredientFlowFirstQA\AB\20260926_104453_929\Caster_AB_Full.mp4`
- 동일 폴더 `regression.json`, `*.poses.txt`, `*.particles.json`, `Side_AB_Label_Check.png`.
- 단계별 캡처: `unity/Logs/PurpleIngredientFlowFirstQA/PrototypeStep1` ~ `PrototypeStep5`.
- 최종 Step5: `PrototypeStep5/20260926_103908_161` (1.58/1.70/1.82/1.94초).

## Visual QA 및 한계

Technical: PASS (Pipeline 응답 제한은 위에 별도 기록). Regression: PASS (위 한정 범위).

Visual similarity: **MIXED**. Art direction: **MIXED**. 더 많은 요소가 보인다는 사실을 성공 판정으로 쓰지 않음.

- Frame evidence: Side 0.767초/1.433초에서 기존 두꺼운 밝은 번개형 조각 대신 청록/진홍의 짧은 곡선 결과 작은 orb가 보임. Confidence medium. Counter-evidence: 일부 brush는 여전히 독립된 잎/띠처럼 정돈되어 보여, 연속적인 공간 흐름으로 느껴지는지는 불확실.
- Frame evidence: 최종 Step5 1.70초에서 Blue/Red 잔광이 Purple 주변에 남고, 1.82초에는 방사형 spike/tear가 더해짐. Confidence high (존재/색/위치). Counter-evidence: 기존 Purple의 강한 밝기와 외곽이 새 accent를 일부 묻으며 추가분의 체감 폭주감은 제한적.
- Direction evidence: 샘플 좌표 검사에서 Blue stroke head는 안으로, Red pressure front는 밖으로 이동. 이는 기술 근거이며 원속도에서 사용자가 즉시 흡인/척력으로 인식한다는 증거는 아님.
- Real-time perception: 원속도 MP4를 생성했으나 이 실행 환경의 직접 관찰은 단계별/추출 프레임 중심. 원속도 지각 성공을 확정하지 않으며 사용자 검수 필요.
- 보호된 dark core는 reference보다 어둡고 경계가 뚜렷함. 이 pass에서는 잠긴 본체를 바꾸지 않음.
- 지면/주변 공간 gust는 약함. 검수 배경이 어둡기 때문에 실제 밝은 전투 배경의 가독성은 아직 미확인.
- Purple 추가 layer는 보호 조건상 2초 이전에만 존재. 발사 이후에도 reference처럼 새로운 spike가 지속되는 결과를 주장하지 않음.

A/B 생성 후 추가 visual polish 중단. ASTRA ESCALATION 없음. 최종 시각 승인 대기.
