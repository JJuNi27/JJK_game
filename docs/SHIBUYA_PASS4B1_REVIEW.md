# PASS 4B.1 — 전신 메시 표면 주력 방출

2026-10-24 KST. LOCAL / CODEX VALIDATED(기술) / PENDING USER VISUAL REVIEW.
브랜치 `feat/shibuya-combat-mvp`, HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`. 기존 여러 PASS의 미커밋 작업 위에서 진행했다. USER VERIFIED 또는 GitHub 반영 상태가 아니다.

1. **실제 캐릭터 구성**

   실제 Play Mode의 Renderer/메시를 읽었다. Torso, Head, FaceDirection, 양팔 Block, 양다리 Block의 7개 MeshRenderer/MeshFilter다. 각 메시가 읽기 가능한 Cube(24 vertices/12 triangles)이며 SkinnedMeshRenderer/본은 없다. 팔·다리의 pivot Transform과 BlockVisual의 포즈가 움직이고, 아우라는 포즈 적용 이후 표면 부착점만 따라간다. 얇은 FaceDirection 보조 표시는 자동 선별에서 제외해 실제 방출 원본은 6개다. 근거: QA `live-hierarchy-before.json`.

2. **이전 8개 앵커와의 차이**

   전체 bounds의 고정 비율 8개를 Renderer 박스 안으로 옮기는 방식을 제거했다. 실제 메시 삼각형 안의 barycentric 좌표와 법선으로 방출점을 만든다. 머리/몸통/팔/다리 분류에 bounds를 쓰지만, 그 분류는 밀도와 성장 길이를 조절할 뿐 방출점을 생성하는 대표 좌표가 아니다.

3. **표면 샘플링 방식과 이유**

   초기화 또는 분포 설정 변경 때만 삼각형을 읽는다. 변환된 실제 삼각형 면적에 따른 층화 표본 128개/Renderer를 만든 뒤, 최소 배분·면적 가중치·기존 점과의 거리로 최종 점을 선택한다. 재현 가능한 해시를 사용하며 Unity Random 상태를 바꾸지 않는다. 선택된 점의 Source Renderer/로컬 좌표/법선/triangle ID/barycentric 좌표를 보존한다. 이후 전체 삼각형 탐색이나 CPU 메시 재생성은 없다.

   읽을 수 없는 메시와 SkinnedMeshRenderer에는 해당 Renderer의 bounds **표면** fallback을 제공한다. 매 프레임 BakeMesh를 하지 않으며, 이 fallback을 실제 skin 변형 추종이라고 설명하지 않는다. `CursedEnergyAuraSurface`와 `CursedAuraSurfacePoint`가 향후 스킨/본 공급 방식의 분리 지점이다. 유효 Renderer가 없으면 전신 캡슐로 가짜 신체를 만들지 않는다.

4. **배분 정책**

   기본 메인8/보조24. 메인은 각 유효 Renderer에 최소1개, 보조는 최소2개를 먼저 배분하고 나머지는 면적과 하체 가중치1.5로 나눈다. 첫 다리 메인은 실제 표면 후보 중 발 쪽에서 선택한다. 머리의 정면 법선은 피하고, 아래로 향하는 면은 제외해 얼굴과 지면을 뚫는 방출을 줄인다. 기본 수치는 현재 6개 신체 메시에서 전신 배분을 만족하며 모든 체형/장비에 대한 의미 분류를 보장하는 것은 아니다.

5. **실제 방출점 수와 분포**

   | 실제 원본 | 메인 | 보조 | 합계 |
   |---|---:|---:|---:|
   | Head | 1 | 2 | 3 |
   | Torso | 2 | 6 | 8 |
   | LeftArm/Block | 1 | 2 | 3 |
   | RightArm/Block | 1 | 2 | 3 |
   | LeftLeg/Block | 2 | 6 | 8 |
   | RightLeg/Block | 1 | 6 | 7 |
   | 합계 | 8 | 24 | 32 |

   표면 높이 범위는 플레이어 루트 기준 약0.053671~1.851377m다. 실제 삼각형 재구성 오차0.00001m 미만, 최종 뿌리는 표면에서 법선 방향0.012m(검사 오차0.001m 이내)다. 앞/뒤/좌/우 법선의 존재도 검사했다. 좌우 점 수 차이는 면적 배분의 반올림/안정된 순서로 발생하며 좌우를 같은 불꽃 복제로 만들지 않았다. 근거: `final-targeted/legacy/ShibuyaPass4B0QA/surface-distribution.json`.

6. **전신 입체감과 움직임**

   표면 법선의 바깥 성분과 상승 성분을 합쳐 각 불꽃의 성장 축을 만든다. 뿌리는 해당 신체 표면에 남고 꼬리만 굽힘/상승/침식된다. 부위별 길이와 폭이 다르며 머리의 성장은 짧게 제한한다. 메인 흐름은 기반을 유지하고 보조 흐름은 서로 다른 위상으로 생성/소멸한다. 타원형 껍데기·새 벽·VFX Graph를 추가하지 않았다. 아우라 전체의 scale/width를 변경해도 부착점은 몸에 남는다.

7. **기존 혼령 불꽃·먹물 보존**

   fragment 함수는 수정 전과 동일하다. 청록/푸른 내부, 침식·먹물 대비/색/alpha 계산을 바꾸지 않았다. vertex 쪽의 표면 좌표/법선 방향 연결과 배열 용량만 확장했다. 청록과 먹물은 계속 같은 geometry/phase/erosion을 공유한다. 3개 프로필은 원본 파일 내용 전체를 보존하고 신규 필드만 뒤에 추가했음을 SHA-256으로 확인했다. 근거: `art-and-profile-preservation.json`.

8. **변경 파일**

   `unity/Assets/Shibuya/` 기준:
   - 신규 `Scripts/CursedEnergyAuraSurface.cs` 및 필요한 `.meta`.
   - 수정 `Scripts/CursedEnergyAuraVisual.cs`, `CursedEnergyAuraGeometry.cs`, `CursedEnergyAuraController.cs`, `CursedEnergyAuraProfile.cs`.
   - 수정 `Editor/CursedEnergyAuraInspector.cs`, `Editor/ShibuyaPass4B0Tests.cs`.
   - 수정 `VFX/CursedEnergyAura/CursedEnergyAuraVolume.shader`.
   - 신규 설정 필드만 추가: `Resources/Cursed Energy Aura.asset`, `VFX/CursedEnergyAura/Presets/Pink Aura.asset`, `Green Aura.asset`.
   - 이 보고서 신규 작성. Material/Prefab/Scene/기존 GUID/Packages/ProjectSettings/전투 코드/Gojo VFX는 수정하지 않았다. Controller 변경은 신체 Renderer 전달과 Scene 전용 디버그이며 F 입력/억제/페이드 규칙은 그대로다.

9. **한국어 Inspector**

   프로필: 전신 메인 흐름 수8, 전신 보조 흐름 수24, 신체 표면 방출 간격0.012m, 하체 표면 배분 가중치1.5, 표면 바깥 방향 흐름0.45, 위로 솟는 흐름0.85. 기존 SerializedProperty/Undo/필드 편집 유지.

   캐릭터의 CursedEnergyAuraController: `방출할 신체 Renderer (비우면 자동 선별)`, `선택 시 Scene에 방출점 표시`. 자동 모드는 아우라·카메라·HUD·EditorOnly·숨김 디버그 및 얇은 보조 표시를 제외한다. 장비가 있는 실제 모델은 신체 Renderer를 명시적으로 지정해야 안전하다. 임의의 두꺼운 장비를 자동으로 의미 판별한다고 주장하지 않는다. 디버그는 선택된 Scene View에서만 노란색 메인/청록 보조 점으로 표시하며 게임 화면이나 Player 빌드에 그리지 않는다.

10. **targeted 자동 검증**

   QA 루트: `unity/Logs/ShibuyaPass4B1QA/20261024-full-body/`.

   | 별도 실행 | PASS | FAIL | SKIP |
   |---|---:|---:|---:|
   | surface-and-visual | 4 | 0 | 0 |
   | final-targeted | 12 | 0 | 0 |
   | lock-and-highest-angle | 1 | 0 | 0 |

   최종 12개는 단일 실행이며, F+MMB/최고 앵글1개는 별도 보완 실행이다. **최신 고유13개 PASS / 0 FAIL / 0 SKIP**이며 단일13개 전체 성공 실행으로 표현하지 않는다. 초기4개 시각 캡처에서 짧은 조각 인상이 보여 성장 길이/방향만 보완한 후 최종12개를 수행했다. 이후 추가 시각 polish는 없다. 기존 bounds 내부8개 검사는 새 surface-offset 계약의 삼각형/부착/전신 분포 검사로 교체했으며 과거 원본은 보존했다.

   표면 정확성/방향/분포/재구축 결정성, 부위0.2m 이동 시 뿌리0.200000018m 추종, scale1.5/width1.4 적용 후 뿌리 오차 약0.00000003m, F 전환·피격 억제·사망, 지상 피해85, 업→추격→공중5타 피해62, F+MMB 락온·반대 시점 공격 피해20, 색상 분리/한국어/Undo/셰이더, 반복/파괴 cleanup 및 실제 렌더를 검사했다. 기존 전체190개를 재실행하지 않았고 PASS4A 과거 미해결18건을 해결했다고 취급하지 않는다.

11. **이미지**

   QA 루트의 `full-body-before-after.jpg`: 이전 대표 앵커/현재 표면 방출의 정면·측면·후면·상부. 동일 카메라 offset/해상도이며 별도 실행의 다른 시각이라 픽셀 일치 검사는 아니다.

   `final-targeted/legacy/ShibuyaPass4B0QA/`: OFF, 활성화, 정면, 측면, 후면, 상부, 좌측,45도 사선, 근접, 핑크,초록,해제 PNG. 기존 `05-high-angle`은 약54도이며, 실제 최고70도는 `lock-and-highest-angle/legacy/ShibuyaPass4B0QA/09-highest-70deg.png`다. `final-orbit-sheet.jpg`와 실제 샘플 기반 `surface-point-distribution.png`도 제공한다.

12. **원속도 영상**

   `full-body-original-speed.mp4`: 이전과 같은 거리/회전 방식, 고정 측면 약2초+약4초360도. 실제174프레임, 약5.995초, 최대 샘플 간격77.7ms.
   `full-body-moving-original-speed.mp4`: 이동·방향 전환 약3초, 실제86프레임, 최대 간격58.9ms. 실제 타임스탬프 기준30fps 인코딩이라 일부 출력 프레임은 복제되며 모든 순간을 네이티브30fps로 측정한 것은 아니다. 원본 PNG/타임스탬프/ffconcat을 보존했다.

13. **성능·자원**

   30회 토글+12초 홀드: Transform26 / 소유 Material4 / Mesh3 유지, 최종8,272 vertices(이전4,390). 측정 미세 입자6개, 상한64는 유지. 렌더러 수와 Material 수를 늘리지 않았지만 geometry는 늘었다. 삼각형 샘플링과 메시 작성은 초기/분포 설정 변경 때만 수행하고 LateUpdate에는32개 부착 좌표·법선만 갱신한다. 종료 후 소유 Material/Mesh0/0. 이는 GPU overdraw/다인전 성능 벤치마크가 아니며 그 비용은 미검증이다.

14. **컴파일·Console·환경**

   C# error/warning0/0, 셰이더 오류 검사PASS. 최종 현재 Console error0/warning0. 누적 보존 버퍼 error25/warning87에는 이전 오류와 invalid-font 경고가 남아 있으며 삭제하지 않았다. Pipeline ready, Play stopped, Shibuya scene dirty=false, dirty ScriptableObject 없음. fps -1/vSync0/scale1/AutoTick SessionState -1/Play Unfocused2 유지. Computer Use·포커스 강제 전환 없음.

15. **Git·보호 상태**

   동일 브랜치/HEAD, staging/commit/push/merge/reset/clean/checkout/worktree 없음. 최종 파일별 보존 검사는 `preservation-check.json`, 작업 상태는 `final-git-status.txt`, 집계는 `validation-summary.json`에 저장한다. 기존 PASS3D/4A/공중전·Gojo·USER VERIFIED 파일과 QA를 보존했다. 이번 생성 결과는 모두 새 QA 루트로 분리했다.

16. **사용자 Play Mode 확인**

   `ShibuyaCombatMVP`에서 F를 유지하며 정면/후면/좌우/사선/최고 하이 앵글과 근접을 확인한다. 머리·양팔·양다리·발의 주력 존재감, 몸과 뿌리의 거리, 긴 꼬리와 검은 경계, 몸/공격 동작 가독성을 검수한다. F를 유지한 WASD/점프/지상 콤보/W업→Space→공중 LMB/MMB 락온 후 F 해제를 확인한다. 필요하면 Player의 Controller에서 Scene 전용 방출점 표시를 켜 점 분포를 확인한다. Play 중 데이터값을 바꾸면 종료 전에 보존 여부를 직접 판단한다.

17. **남은 한계**

   하체가 비던 문제는 줄었고, 실제 표면에서 머리~발끝으로 발생하는 기반은 확인했다. 다만 시점/프레임에 따라 흐름 사이 빈 공간, 넓은 불꽃 면과 독립된 작은 조각 인상이 남는다. 하나의 연속 유체 볼륨을 완성했다고 선언하지 않는다. 실제 스킨 모델의 변형 표면 추종, 장비 포함/제외 정책의 캐릭터별 지정, 다인전 GPU 비용은 별도 작업이다. 시각 품질은 사용자 검수 대기이며 이번 결과를 USER VERIFIED로 승격하지 않는다.
