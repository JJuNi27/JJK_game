# PASS 4B.3 — 최대 주력량 기반 아우라 외부 크기

2026-10-24 KST. **LOCAL / CODEX VALIDATED(targeted 기술 범위) / USER VERIFIED 대기**.
브랜치 `feat/shibuya-combat-mvp`, HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`. 기존 미커밋 PASS 3D~4B.2 위에서 진행했다. 원격 반영·Production 대체 완료가 아니다.

사용자는 이번 요청에서 PASS 4B.2의 전신 청록·먹물 혼령 불꽃 표현에 만족했다고 명시했다. 이번에는 그 표현을 재제작하지 않고 외부 성장 거리와 최대 주력량 연결만 추가했다. 기존 PASS 4B.2 보고서와 QA 원본은 보존했다.

## 1. 기존 구조와 새 100 기준

기존 메인은 6개 몸 Renderer의 현재 포즈를 높이별 단면으로 연결한 전신 필드였다. `fieldThickness`, width/height 및 셰이더 국소 변위로 몸 바깥 영역을 만들고, 실제 삼각형 표면 샘플 24개는 작은 보조 불꽃에 사용했다. 캐릭터 주력량과의 연결은 없었다.

새 기본값 `baselineExpansion=1.25`는 **몸 바깥 거리와 불꽃 성장 범위**를 확대한다. 주력량100의 곡선 배율은 정확히1.0이며, 최종 외부 성장 계수는 `기준 확장1.25 × 곡선 배율`이다.

- 메인: 몸 단면 자체의 반경을 통째로 확대하지 않고 추가 외곽 여유 거리, 머리 위 성장 범위, GPU crest/변위에만 계수를 적용한다. 하단 y는 유지한다.
- 보조: 몸 표면 root를 이동시키지 않고 root에서 꼬리로 향하는 상대 좌표만 확대한다.
- 먹물: 같은 형상과 변위를 사용한다. 메인의 얇은 먹물 hull 두께는 계수의 제곱근으로 보정해 확대 시 절대 두께가 약해지지 않게 한다.
- 캐릭터 Transform, CharacterController, 공격 판정, BodyRim, 입자 수·밝기·색상은 크기 계수로 바꾸지 않는다. 아우라 전체 Transform도 이 기능 때문에 확대하지 않는다.

## 2. 최대 주력량의 원본과 연결

기존 `CharacterStatsProfile`에는 HP/공격·방어 배율만 있지만, 공통 **`CursedEnergyProfile.MaxEnergy`가 이미 존재**한다. 새 최대치 필드를 중복 생성하거나 기존 고죠 능력치를 바꾸지 않았다.

`ShibuyaCombatProfile.cursedEnergy` 참조를 추가했다. 기본 전투 자산은 새 `Data/Shibuya Cursed Energy.asset`(최대100)을 참조한다. 향후 캐릭터별 전투 프로필에서 각자의 주력 능력치 자산을 연결한다. 이름 문자열로 캐릭터를 구분하지 않는다. 색상 프로필과 최대치 자산은 독립적이다.

`CursedEnergyAuraController`는 `owner.Profile.cursedEnergy.MaxEnergy`만 읽는다. 참조가 없으면 아우라 프로필의 기준 최대치(기본100)로 안전하게 표시한다. 기존 `CursedEnergyProfile`의 시작량·회복·소모 필드는 이번 시부야 연결에서 사용하지 않는다. 실제 씬에 CursedEnergyController나 주력 게이지/충전·강화 시스템을 새로 붙이지 않았다.

현재 잔량 독립성 검증에서만 QA 플레이어에 기존 CursedEnergyController를 잠시 붙이고 자동 회복을 끈 뒤 잔량을 바꿨다. 최대200 / 현재200·10·0·180 모두 배율1.50, 같은 필드 폭1.62221을 유지했다. QA 종료 시 이 임시 컴포넌트는 씬과 함께 제거됐다.

## 3. 곡선과 보간

`CursedEnergyAuraProfile.SizeMultiplier(maximumEnergy)`는 순수한 공용 계산 경로다. 향후 F 충전 표시와 강화 지속 표시 모두 이 함수를 재사용할 수 있다. 이번에는 강화 지속 기능을 구현하지 않았다.

곡선의 X는 **최대 주력량 / 기준 최대 주력량**, Y는 외부 성장 배율이다.

| 최대 주력량 | X | 기본 Y |
|---:|---:|---:|
| 50 | 0.5 | 0.85 |
| 100 | 1.0 | 1.00 |
| 150 | 1.5 | 1.25 |
| 200 | 2.0 | 1.50 |
| 250 | 2.5 | 1.65 |
| 300 | 3.0 | 1.75 |

200 이후 tangent를 낮춰 증가가 완만해지고 기본 곡선은 마지막 key 밖에서 유지된다. 기본 최소/최대는0.5/2.0이다. 곡선이 없거나 비정상적인 출력이면 기준값으로 처리하고, 출력은 허용 범위로 제한한다. 0 이하/NaN/Infinity 직접 입력 및 비정상 설정의 안전한 처리를 검사했다. 기존 CursedEnergyProfile getter 자체도 저장 최대치에 최소1을 적용한다. 크기 clamp에는 비정상 설정을 위한 내부 절대 안전 상한8도 있다.

표시 중 최대치가 바뀌면 기본0.2초의 응답 시간으로 지수 보간한다. 약0.001 이내에서 정확한 목표값으로 정착한다. 이는0.2초에 즉시 완료되는 선형 이동 시간이 아니다. 꺼져 있는 동안은 목표치를 미리 맞추고 다음 F 활성 페이드와 함께 표시한다. 현재 잔량·비율·소모량은 계산에 전달하지 않는다.

## 4. 한국어 Inspector

공통 아우라 프로필의 신규 항목:

- 기준 최대 주력량100
- 기준 외곽·화염 확장 배율1.25
- 최대 주력량별 크기 곡선(X: 기준 대비 주력량 / Y: 배율)
- 아우라 크기 최소 배율0.5 / 최대 배율2
- 크기 변화 보간 시간0.2초

기존 fieldThickness는 메인 외곽의 모양, detailScale은 메인 대비 보조의 비중이다. 신규 기준 확장은 이 기존 구성을 유지하며 외부 성장 범위를 함께 바꾸는 계수다. 캐릭터의 실제 최대 주력량을 여기에 중복 저장하지 않는다.

시부야 전투 프로필에는 **캐릭터 주력 능력치** object picker가 추가됐다. 기존 CursedEnergyProfile의 한글 Inspector를 재사용한다. 신규 항목도 SerializedProperty로 편집하며 곡선 기본 편집기·Undo·다중 편집·기존 override 흐름을 유지한다. 자동 검사에서 신규 라벨 매핑과 SerializedProperty Undo를 확인했다.

## 5. 실제 크기 및 보존 계측

동일한 임시 캐릭터의 정지 상태에서 측정한 CPU carrier 값이다. 셰이더 변위 이후 픽셀 경계나 실제 공격 범위가 아니다.

| 최대치 | 곡선 배율 | 외부 성장 계수 | 메인 폭 | 최하단Y | 최상단Y |
|---:|---:|---:|---:|---:|---:|
| 100 | 1.00 | 1.2500 | 1.48085 | 0.05378 | 2.22406 |
| 150 | 1.25 | 1.5625 | 1.55257 | 0.05378 | 2.27340 |
| 200 | 1.50 | 1.8750 | 1.62221 | 0.05378 | 2.32275 |

따라서 배율1.50은 몸을 포함한 전체 폭이1.50배라는 뜻이 아니다. 기존 신체는 그대로 두고 외부 성장분에 배율을 적용한다. root 위치 오차0.001m 이내, 하단 높이 오차0.0001m 이내, CharacterController 높이/반경과 캐릭터 scale 불변을 검사했다. 메인2009 vertex, 보조24개, 전체7138 vertices 및 동일 Mesh 인스턴스를 유지했다.

기존 전신 연결 topology, 먹물/에너지 동일 vertex, 기본4타 실제 피해85, 어퍼컷→추격→공중5타, F hold·해제·피격 억제·사망 해제·락온 및 최고70도 관련 targeted 회귀도 통과했다. 이 결과를 전체 기존 전투 회귀 통과로 확대하지 않는다.

## 6. 검증 실행과 실패 이력

QA prefix: `unity/Logs/ShibuyaPass4B3QA/20261024-reserve-scale/`.

1. `baseline/`: 기존 PASS4B.2 코드로 비교 캡처 검사1 PASS. 크기 기능의 신규 고유 검사 수에는 포함하지 않는다.
2. `capacity-targeted/`: 신규8개 최초 실행 **5 PASS / 3 FAIL / 0 SKIP**.
3. `settled-and-aura-regression/`: 실패3개 재검증 + 기존 관련 아우라14개, **단일 실행17 PASS / 0 FAIL / 0 SKIP**.

기능 검증의 **최신 고유 케이스별 결과는22 PASS / 0 FAIL / 0 SKIP(복수 실행 집계)**다. 단일22개 성공 실행이 아니다. 최초 XML/JSONL 및 당시 QA 코드 사본을 보존했다. 전체190개 등 대규모 suite는 재실행하지 않았다.

최초 실패 내용:

- 색상 검사: 보간 도중1.49090159를 정확한1.5와 비교했다.
- 150 시각 검사:0.7초 후1.24250698을 목표1.25±0.001과 비교했다.
- 200 시각/이동 검사: QA 캡처 coroutine에서 NullReferenceException이 발생했다. 스택은 QA 메서드였으며 런타임 전투 코드의 예외는 아니었다. 단일 기록만으로 내부 순서를 확정하지 않는다.

QA의 초기 정착 대기를1.6초로 맞추고 이동 캡처 전 Visual 존재를 명시적으로 확인했다. 이후 세 검사 모두 완료됐고 200의 정지/회전/이동 원본도 생성됐다. 곡선, 기대 배율, 런타임 보간 시간 또는 게임플레이 값은 재검증을 위해 변경하지 않았다.

초기 새 QA fixture에서 기존 API명을 잘못 사용한 CS1061 컴파일 오류도 있었으며 `TestInput` delegate로 수정했다. 기록은 `implementation-history.json`과 Pipeline 오류 이력에 남겼다.

## 7. 시각 자료

QA prefix 아래 `media/`:

- `four-way-front.png`, `four-way-side.png`, `four-way-diagonal.png`, `four-way-back.png`, `four-way-high.png`: 왼쪽부터 PASS4B.2 / 새100 / 150 / 200. 동일 캐릭터·카메라·정지 자세, main/detail shader phase2.
- `four-way-*-detail.png`: 위 이미지의 같은 영역을 원래 픽셀 배율로 crop한 자료.
- `100-150-200.png`: 세 최대치를 같은 화면에서 비교.
- `baseline-orbit-original-speed.mp4`, `max-100-orbit-original-speed.mp4`, `max-150-orbit-original-speed.mp4`, `max-200-orbit-original-speed.mp4`: 각각 약4초, 960×720/30fps 인코딩, 원속도 회전.
- `max-200-moving-original-speed.mp4`: 약3초 이동/회전 중 추종.
- `*-contact.jpg`: 실제 timestamp가 붙은 시퀀스 표본.

원본PNG·timestamp·측정JSON은 각 실행의 `legacy/ShibuyaPass4B3QA/`에 남겼다. 원속도 영상은 원본 timestamp duration을 사용하고 필요한 프레임을 반복하며 보간 프레임은 만들지 않았다. Orbit 원본은 각각111프레임, 간격 중앙값 약34.2~34.5ms이나 최초 렌더 부근 최대202~212ms 간격이 있다. 이를 잘라내거나 빨리 재생해 숨기지 않았다. 이동85프레임은 중앙35.09ms/최대41.40ms다. 별도 실행이므로 작은 입자의 무작위 위치와 영상의 전체 위상까지 픽셀 동기화한 비교는 아니다.

관측:100은 기존보다 외곽과 꼬리가 넓어졌다. 150/200 차이는 측면과 하이앵글에서 특히 잘 읽힌다. 정면 원거리에서는 차이가 상대적으로 작다. 200에서는 머리 주변 꼬리/국소 먹물 면이 커지고 기존 block silhouette와 겹쳐 보일 수 있다. “과도함/충분함”은 사용자 Play Mode 검수 대상으로 남긴다.

## 8. 리소스·성능·Console

기존30회 F 반복+12초 hold 및 신규15회 용량 변경/F 반복에서 Material4 / Mesh3 유지. 종료·파괴 후 소유 Material/Mesh0/0, 입자0을 확인했다. 입자 수를 늘리거나 새 레이어를 추가하지 않았다.

최대200 이동 중 서로 다른85회 CPU refit 표본: 중앙1.7418ms / 최대2.2209ms. 기존과 동일한2009-vertex 메인 topology이며 크기 갱신은 보간 중 refit을 추가로 유발한다. 정착 후 정지 cache는 유지된다. 이는 Editor CPU 특정 계산 구간이며 GPU fillrate/실기 프레임 예산/다인전 성능 검증이 아니다. 확대에 따른 transparent overdraw는 별도 측정하지 않았다.

최종 Editor ready, Play stopped, ShibuyaCombatMVP scene clean, dirty Data Asset0. 컴파일 완료/실패false, 관련 셰이더 지원·오류 검사 통과. 현재 Console ground truth 오류0/경고1(기존 invalid font reference). Pipeline 보관 buffer에는 오류5개가 남는다: 작업 시작 전10:13의 UnityConnect token 교환1 + CrashReporter main-thread assert3, 이번10:29의 수정 완료된 QA CS1061 1개다. Console을 지우지 않았으며 전체 이력이 오류0이라고 보고하지 않는다.

QA active=false, targetFrameRate=-1, vSync0, Time.timeScale1. 이번에 AutoTick/프레임률/Play Unfocused를 변경하지 않았다. 최종 AutoTick 실제값은 ON/16ms(명시적 SessionState interval 없음), Play Unfocused=2다. 초기 GetInt로 조회한 Enabled=-1은 bool인 Enabled 상태 자체를 뜻하지 않으므로 ON/OFF 판정 근거로 사용하지 않았다. 실제 Windows 화면 직접 조작도 사용하지 않았다.

## 9. 변경 파일과 보존

기존 파일12개 수정:

- `unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs`
- `unity/Assets/Shibuya/Data/Shibuya Combat Profile.asset` — 새 주력 자산 참조1개만 추가
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraProfile.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraController.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraVisual.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraField.cs`
- `unity/Assets/Shibuya/Editor/CursedEnergyAuraInspector.cs`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/CursedEnergyAuraVolume.shader`
- `unity/Assets/Shibuya/Resources/Cursed Energy Aura.asset`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/Presets/Pink Aura.asset`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/Presets/Green Aura.asset`

신규5개:

- `unity/Assets/Shibuya/Data/Shibuya Cursed Energy.asset` 및 `.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4B3Tests.cs` 및 `.meta`
- 이 보고서

시작1164개 tracked+비무시 untracked SHA-256 비교에서 위12개 외1152개 동일, 누락0. 세 색상 프로필에서 신규 크기 설정 블록만 제거하면 시작 바이트 해시가 일치한다. 셰이더 fragment는 시작 코드와 동일하다. 기존 입력/카메라/전투 알고리즘·PASS4B.2 Geometry/Surface·Gojo/VFX·USER VERIFIED 문서·Scene/Prefab/Material·기존 GUID·Packages/ProjectSettings·HP1000/100은 이번에 변경하지 않았다. 전투 자산의 임시 serialization으로 생긴 null defaultFinisherCinematic 줄은 이번에 추가한 줄만 제거해 최종 diff를 새 주력 참조1개로 한정했다.

Git 시작 staged0 / tracked unstaged28 / untracked97. 종료 staged0 / tracked unstaged29 / untracked102. HEAD/브랜치 유지. staging/commit/push/merge/reset/clean/checkout/worktree는 실행하지 않았다. 기존 QA를 수정·삭제하지 않고 새 PASS4B3 경로에만 기록했다.

## 10. 사용자 직접 검수

1. ShibuyaCombatMVP Play, F hold/release 및 마우스 회전으로 전신 외곽·발밑 부착·먹물·머리 꼬리를 확인한다.
2. 기본 실제 데이터는 최대100으로 남겼다. 150/200 시험은 Project 원본 자산을 바로 바꾸기보다 Play Mode의 복제 CursedEnergyProfile/Combat Profile을 연결하거나 이 QA 캡처를 먼저 사용한다. ScriptableObject 원본을 Play Mode 중 수정해도 저장값에 남을 수 있음에 주의한다.
3. 설정 위치: Shibuya Combat Profile → 캐릭터 주력 능력치 → 최대 주력. 공통 VFX 곡선과 기본 외곽 크기는 Resources/Cursed Energy Aura에서 조절한다. Pink/Green도 같은 계산을 사용한다.
4. WASD/Shift/Space, F 중 지상4타·W/S마무리·공중추격·공중3클릭·MMB 락온의 익숙한 동작을 확인한다. 원거리 정면의 차이가 충분한지,200에서 꼬리가 과하거나 뿔처럼 읽히는지를 판단한다.

현재 임시 캐릭터에서만 기술/시각 자료를 확인했다. 실제 고죠 스킨·캐릭터별 포즈·다인전·실기 성능 및 최종 시각 품질은 미검증이다. 이번 결과를 PASS4B.2 대체 승인으로 선언하지 않고 사용자 검수를 기다린다. 주력 게이지/충전/강화 지속/대시는 구현하지 않았다.
