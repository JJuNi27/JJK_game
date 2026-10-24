# PASS 4B.3a — 크기 차이와 내부 상승 흐름

2026-10-24 KST. **LOCAL / CODEX VALIDATED(targeted 기술 범위) / USER VERIFIED 대기**.
브랜치 `feat/shibuya-combat-mvp`, HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`. 기존 PASS 3D~4B.3 미커밋 작업 위에서 진행했다. 원격 반영·최종 시각 승인 상태가 아니다.

사용자는 이번 요청에서 PASS4B.2~4B.3의 전신 청록·먹물 혼령 불꽃 구조와 전체 품질에 만족한다고 명시했다. 전면 재제작 대신 크기 곡선과 기존 필드 내부의 가시성만 보완했다.

## 1. 조사 및 레퍼런스

현재 Git·handoff/current task·locked settings·4B.2/4B.3 보고서와 실제 Profile/Controller/Visual/Field/Geometry/Shader/QA를 확인했다. handoff/current task는 10-22 PASS3D 시점이므로 최신 로컬 구현 및 이번 요청과 혼동하지 않았다. 시작 시 Unity는 stopped / 씬 clean이었다.

기존 4B.3 비교 이미지와 로컬 `주력이펙트.mp4`, `다양한 주력이펙트.mp4`, `주력이펙트 전투.mp4`를 직접 추출해 확인했다. 각 영상20/50/80% 시점9장과 메타데이터를 새 QA 경로에 보존했다. 내부 청록과 짙은 경계가 함께 움직이는 시각 언어를 참고했으며, 다양한 예시 중 평면 panel 표현을 우리 geometry로 복제하지 않았다. 이 성긴 표본으로 원본의 정확한 모션 주기를 확정하지 않는다. 기존 혼령 불꽃 움직임은 유지했다.

## 2. 곡선 변경과 200 후보

| 최대 주력량 | 기존 4B.3 | 4B.3a 기본 | 추가 비교 |
|---:|---:|---:|---:|
| 50 | 0.85 | 0.85 | — |
| 100 | 1.00 | 1.00 | — |
| 150 | 1.25 | 1.50 | — |
| 200 | 1.50 | 2.00 | **2.50** |
| 250 | 1.65 | 2.20 | 2.70 |
| 300 | 1.75 | 2.30 | 2.80 |

최대 배율 제한은2→3으로 높였다. 기준 최대치100, 기준 외부 확장1.25, 최소 배율0.5, 보간 응답0.2초는 유지했다. 곡선 X는 최대 주력량/기준 최대치, Y는 크기 배율이다. 기존 CursedEnergyProfile.MaxEnergy 참조와 현재 잔량 독립성도 그대로다.

**현재 기본200은2.00이며 최종 사용자 선택 전의 기본값이다.** 2.50은 실제 QA 복제 프로필로 별도 실행·캡처했다. 2.50 쪽은200 이후 key도0.5씩 올려 증가 방향을 유지했다. 비교 결과2.50이 더 넓고 풍성하지만 일부 각도에서 꼬리·먹물 면이 크게 읽히므로 기본은2.00으로 두었다. 2.50을 폐기하거나 사용자의 최종 선택을 대신한 것은 아니다.

## 3. 확대 방법과 실측

Transform 전체 확대를 추가하지 않았다. 몸 밖 필드 여유 거리·보조 불꽃의 root 상대 좌표를 확대한다. 몸·발밑·root·CharacterController는 그대로다.

큰 배율에서 상향 성장만 과해지지 않도록 새 `verticalSizeResponse=0.5`를 적용했다. 수평 외부 성장 계수는 기존처럼 `1.25 × 크기 배율`, 상향 계수는 `1.25 × 배율^0.5`다. 이 지수는 Inspector에서 조절 가능하다. 따라서100의 좌표는 이전과 같고, 큰 배율의 위쪽 성장은 수평보다 완만하다. 머리/팔/다리별 emitter를 늘리거나 필드를 다시 분리하지 않았다.

같은 정지 캐릭터의 CPU 메인 carrier 실측:

| 최대치/배율 | 폭 | 최하단Y | 최상단Y |
|---|---:|---:|---:|
| 100 / 1.00 | 1.48085 | 0.05378 | 2.22406 |
| 150 / 1.50 | 1.62429 | 0.05378 | 2.26842 |
| 200 / 2.00 | 1.76495 | 0.05378 | 2.30582 |
| 200 / 2.50 | 1.90770 | 0.05378 | 2.33877 |

100은 변경 전 측정과0.0001 이내에서 같았다. 2.00→2.50의 carrier 폭은 약8.1% 증가했다. 이 배율은 몸을 포함한 모든 좌표의 배율이 아니라 외부 성장분의 배율이다. 보조 꼬리와 GPU 변위가 추가되므로 위 표는 최종 픽셀 실루엣 크기와 다르다. 동일 카메라 비교 이미지를 함께 봐야 한다.

## 4. 희미한 내부 흐름의 실제 원인

독립적인 내부 파티클/메시/pass가 있는 구조가 아니었다. `CursedEnergyAuraVisual`의 **MainAura → Cursed Energy Volume shader의 field 분기**에서 외곽과 정면 영역이 함께 처리된다. InkRuptures는 공유 먹물 hull, EnergyWisps는 표면 근처 보조 불꽃, BodyRim은 몸 표면의 윤곽광이다.

기존 Frag는 `flow.y=-time*3.3`으로 상승하는 비등방성 noise와 window/streams를 계산하지만, 정면 영역의 alpha 가중치를 `.012 + .025*streams`, 외곽은 `.98`로 두었다. 여기에 coverage·cut·opacity를 곱한 뒤 `.012` clip을 하므로 내부는 외곽보다 약26~82배 낮고 상당 부분 제거됐다. 과거 껍데기 인상을 줄이기 위한 억제가 현재 빈 내부로 읽히는 원인이었다.

## 5. 내부 보완과 독립 조절

기존 n/window를 재사용해 내부 상승 채널의 추가 alpha를 계산했다. 별도 noise texture, particle, renderer, mesh, shader pass는 추가하지 않았다. 공간 전체를 고르게 불투명하게 만들지 않고 `(1-contour)` 영역의 불균일한 상승 채널에만 추가 가시성을 준다.

추가 흐름은 같은 메인 색상에서 만든 청록/핑크/초록을 사용하며 외곽의 강한 white crest보다 낮은 색 기여로 합성한다. 현재 최대 주력량이 커졌다고 추가 발광 강도를 올리는 경로는 없다. 현재 남은 주력량은 읽지 않는다.

한국어 Inspector 신규 항목:

- **내부 상승 에너지 가시성** (`innerFlowVisibility`, 기본0.65)
- **내부 상승 흐름 밀도** (`innerFlowDensity`, 기본1.0)
- **주력량 증가 시 상향 성장 비중** (`verticalSizeResponse`, 기본0.5)

기존 크기 곡선/최대 배율도 계속 한글 Inspector에서 편집한다. SerializedProperty·Undo·기존 편집 기능을 유지했다. 내부 불투명도/발광 강도처럼 같은 역할의 중복 필드를 추가하지 않았다.

## 6. 메인·먹물 보존 근거

기존 외곽 alpha 가중치·color/먹물 계산과 Ink pass는 유지했다. 추가 내부 alpha는 에너지 pass의 정면 영역에만 들어간다. 같은 MainAura mesh 안에서 내부와 외곽을 구분하므로 분리된 먹물 판을 만들지 않는다. Blend SrcAlpha/OneMinusSrcAlpha, ZWrite Off, Cull, 렌더 순서도 변경하지 않았다.

같은 프레임·카메라·phase에서 내부 가시성0/0.65를 바꾼 **외곽+먹물 분리 캡처는 decoded RGB의 변경 픽셀0 / 최대 채널 차이0**이었다. 단, 이는 외곽 분리 모드의 검사이며 전체 합성 화면의 지각적 인상까지 불변이라는 뜻은 아니다.

내부만 분리한 body ROI(120×250)에서는2/255 초과 변화 픽셀14,703개, 평균 green 증가0.04710549를 관측했다. 밀도0.5→1.5의 live SerializedProperty 변경도 material에 전달됐고, ROI9,744픽셀 변화/green 증가0.01712248을 확인했다. 이는 화면 색 성분 측정이며 물리적 에너지/발광량의 측정이 아니다.

`_FieldViewMode`는 소유 Material에서만 쓰는 숨김 QA 분리 모드다. 외곽/내부 분리 캡처를 만들기 위한 것으로 별도 실제 렌더 레이어가 생겼다는 뜻이 아니다. 저장 자산에 debug 모드를 적용하지 않았다.

## 7. 시각 QA 위치

공통 QA 경로:
`unity/Logs/ShibuyaPass4B3aQA/20261024-size-interior/`

`media/`의 주요 자료:

- `four-size-front.png`, `four-size-side.png`, `four-size-diagonal.png`, `four-size-back.png`, `four-size-high.png`: 동일 캐릭터/자세/카메라/main·detail shader phase2, 왼쪽부터100×1 / 150×1.5 / 200×2 / 200×2.5.
- `four-size-*-detail.png`: 동일 원본 픽셀 크기의 crop 비교.
- `previous-new-diagonal.png`, `previous-new-side.png`: 기존4B.3과 새 곡선의 실제 변경 전후.
- `actual-code-before-after.png`: 런타임 변경 전에 캡처한100과 개선 후100.
- `interior-before-after.png`, `interior-isolated.png`: 기존 가시성 항과 강화된 내부 흐름.
- `layer-isolation.png`: 외곽+먹물 / 같은 필드의 내부 / 표면 보조 / 전체 합성.
- `100-x1-orbit-original-speed.mp4`, `150-x1.5-orbit-original-speed.mp4`, `200-x2-orbit-original-speed.mp4`, `200-x2.5-orbit-original-speed.mp4`: 각각 약4초 원속도 회전.
- `200-x2.5-moving-original-speed.mp4`: 약3초, 큰 비교 후보의 이동/회전 중 추종.
- `interior-close-original-speed.mp4`: 약2.53초, 내부 상승을 볼 수 있는 근접 원속도.
- `*-contact.jpg`: 실제 timestamp를 붙인 연속 구간 표본.

영상은 원본 timestamp 간격으로 인코딩하고 30fps 출력에서 필요한 프레임만 반복했다. 속도를 높이거나 보간 프레임을 생성하지 않았다. Orbit은111~112원본 프레임이며 간격 중앙값 약34.3~34.5ms, 최초 렌더 부근 최대171~218ms 간격이 있다. 이 간격도 보존했다. 이동은83프레임/중앙36.42ms/최대38.30ms, 근접은73프레임/중앙34.51ms/최대44.40ms다. 작은 입자의 무작위 위치나 서로 다른 영상의 모든 위상을 픽셀 동기화한 것은 아니다.

원본은 각 실행의 `legacy/ShibuyaPass4B3QA/`에 있다. 기존 PASS4B.3 이미지·영상·실패 XML은 그대로 보존했다.

## 8. targeted 결과

- `before/results.xml`: 기존 구현의 비교 증거1 PASS. 기술 검사18개에는 포함하지 않는다.
- `visual-targeted/results.xml`: **단일 실행6 PASS / 0 FAIL / 0 SKIP**.
- `related-regression/results.xml`: **별도 단일 실행12 PASS / 0 FAIL / 0 SKIP**.
- 두 기술 실행의 서로 다른 케이스 합계: **18 고유 PASS / 0 FAIL / 0 SKIP**. 단일18개 실행이 아니다.

신규 검사7개와 기존 관련11개다. 범위는 새 곡선과2.5 허용/상한,100 크기 보존, root/하단/Controller 크기 불변, 내부 실제 픽셀 증가/외곽 분리 보존, live Inspector/밀도/Undo, 현재 잔량 독립성, F 반복/해제/피격 억제/사망 정리, 색상 프리셋, 필드 연결/표면 분포, 실제 지상85피해·어퍼컷→추격→공중5타, 이동·점프, shader/한글 매핑/자산 참조다.

기존 ShibuyaPass4B3Tests의 곡선 기대값도 사용자가 요청한150=1.5/200=2/250=2.2/300=2.3 및 상한3으로 갱신했다. 새 VFX 사양에 따른 변경이며 공격/피격 기대값이나 판정 기준을 완화한 것이 아니다. 전체190개 등의 대규모 회귀는 돌리지 않았다.

## 9. 리소스·성능 범위

소유 Material4 / Mesh3 / 총7,138 vertices / 메인2,009 vertices / 보조 root24개로 그대로다. 신규12회 F 반복 및 기존30회 반복+12초 hold 검사에서 자원 증가가 없었다. Play Mode 종료 후 소유 Material/Mesh0/0, dirty Data Asset0을 확인했다.

2.5 이동 중 CPU refit83표본의 중앙1.7268ms / 최대2.3459ms. Editor의 해당 CPU 계산 구간만 측정한 값이며 실기 프레임 예산/GPU 비용 보장은 아니다.

같은 사선 카메라에서 CPU carrier의 투영 triangle 면적 합은2.00에서50,550, 2.50에서56,910 pixel²로 약12.6% 늘었다. **이는 컬링 전 CPU 메인 carrier의 잠재 화면 점유 proxy이며 실제 overdraw/GPU 시간 측정이 아니다.** GPU 변형·보조 불꽃·opaque 가림·clip·blend를 포함하지 않는다. draw/pass 수는 늘리지 않았지만 큰 아우라의 면적, 내부에서 실제 blend되는 픽셀, 추가 fragment 계산 때문에 비용이 증가할 수 있다. 정확한 transparent overdraw/GPU profiler 결과는 미측정으로 남긴다.

## 10. 컴파일·Console·Editor

최종 Editor ready / 컴파일 완료·실패false / Play stopped / ShibuyaCombatMVP scene clean. shader 지원·ShaderUtil 오류 검사 통과. 현재 Console ground truth 오류0 / 경고1(기존 invalid font reference)이다. Pipeline buffer는 오류5 / 경고74를 보관한다. 오류5개는 이번 시작 전 UnityConnect1, CrashReporter3, 이전4B.3에서 수정한 QA CS1061 1개이며 새 오류로 취급하거나 삭제하지 않았다. 이번 패스의 컴파일/테스트 실패는 없었다.

QA active=false, targetFrameRate=-1, vSync0, Time.timeScale1, AutoTick ON/16ms를 확인했다. 이 설정과 Play Unfocused를 이번에 변경하지 않았다. Windows 화면/마우스 직접 조작도 사용하지 않았다. 상태와 이력은 `final-editor-state.json`에 보존했다.

## 11. 변경 파일과 Git

기존 로컬 파일9개 수정:

- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraProfile.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraField.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraVisual.cs`
- `unity/Assets/Shibuya/Editor/CursedEnergyAuraInspector.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4B3Tests.cs`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/CursedEnergyAuraVolume.shader`
- `unity/Assets/Shibuya/Resources/Cursed Energy Aura.asset`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/Presets/Pink Aura.asset`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/Presets/Green Aura.asset`

신규3개:

- `unity/Assets/Shibuya/Editor/ShibuyaPass4B3aTests.cs` 및 `.meta`
- 이 보고서 `docs/SHIBUYA_PASS4B3a_REVIEW.md`

시작1,169개 파일 SHA-256과 비교해 위9개 외1,160개 동일/누락0. 세 색상 자산은 곡선·최대 배율·새 필드3개를 원래 내용으로 재구성하면 시작 바이트와 정확히 일치했다. 원래 색·강도·fade·개별 레이어 설정을 보존했다.

Controller/F 입력/최대치 참조, 전투 알고리즘·데이터, HP1000/100, 공중·락온·카메라, Geometry/Surface, Gojo/VFX, 기존 GUID·Scene/Prefab/Material·Packages/ProjectSettings·USER VERIFIED 문서·기존 QA는 수정하지 않았다.

HEAD/브랜치 유지. 시작 staged0 / tracked unstaged29 / untracked102, 종료 staged0 / tracked unstaged29 / untracked105. 이번 수정 대상이 기존 untracked 파일들이므로 tracked Git diff만으로 범위를 판단하지 않았다. staging/commit/push/merge/reset/clean/checkout/worktree 생성은 하지 않았다.

## 12. 사용자 직접 검수와 한계

ShibuyaCombatMVP에서 F를 유지하며 정면/측면/후면/사선/하이앵글과 WASD·점프·전투를 확인한다. 특히 내부가 비어 보이지 않는지, 반대로 몸을 덮는 코팅/면처럼 보이지 않는지,200의2.00과2.50 중 어떤 체급이 맞는지를 판단한다.

현재 실제 캐릭터 최대 주력량은100으로 보존했다. Project 원본 ScriptableObject는 Play Mode에서 수정해도 저장값에 남을 수 있으므로 비교는 이 캡처 또는 복제한 QA 데이터로 한다. 사용자 선택 후 곡선 X2(최대치200)의 Y를2.50으로 바꿀 수 있다. 이번2.50 후보와 같게 하려면 이후 X2.5/Y2.7, X3/Y2.8도 맞추고 최대 배율3 이상을 유지한다. 색상과 최대치의 연결은 독립적이다.

2.50은 체급이 더 잘 읽히지만 일부 원근/위상에서는 넓은 꼬리·검은 면이 강조된다. 내부는 실제 raymarch volume이 아니라 기존 전신 3D carrier의 정면에 흐르는 표현이므로 근접·특정 각도에서는 여전히 얇은 막으로 읽힐 가능성이 있다. 실제 스킨 캐릭터·다인전·GPU 비용·최종 사용감은 이번 블록 캐릭터 QA로 승인하지 않는다.

기술 상태까지만 CODEX VALIDATED로 기록한다. 주력 게이지/충전/강화 지속/대시/전투·카메라 개편은 구현하지 않았다. 추가 자동 polish 없이 사용자 시각 검수를 기다린다.
