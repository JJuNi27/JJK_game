# PASS 4B.2 — 전신 주력장과 표면 디테일 분리

2026-10-24 KST. **LOCAL / CODEX VALIDATED(이번 targeted 기술 검증만) / PENDING USER VISUAL REVIEW**.
브랜치 `feat/shibuya-combat-mvp`, HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`. 기존 PASS 3D·4A·4B 미커밋 작업 위에서 진행했다. USER VERIFIED 또는 원격 반영 상태가 아니다.

## 1. 문제 원인과 선택한 구조

PASS 4B.1은 실제 메시 삼각형의 표면 부착점을 사용했지만 메인 8개와 보조 24개 모두 각자의 뿌리·성장 축을 가진 불꽃이었다. 표면 추종은 정확해도 큰 불꽃이 부위별로 솟아 뿔처럼 읽힐 수 있었다. 방출점 증가는 이 문제의 해결책이 아니므로 메인 방출점 8개를 제거했다.

새 메인은 **현재 포즈의 높이별 전신 외곽 단면을 연결한 하나의 필드 carrier**다. 임시 캐릭터의 실제 6개 몸 Renderer(머리·몸통·양팔·양다리, FaceDirection 제외)의 로컬 bounds를 현재 Transform으로 변환하고 높이별 수평 교차점을 구한다. 각 단면의 convex hull에서 둘레 반경을 얻고 둥글게 완화해 높이 41행 × 둘레 48분할로 연결한다. 에너지/먹물이 동일한 2,009-vertex 형상을 공유한다.

단일 전신 타원체, 동서남북 flame card, 부위별 큰 emitter는 사용하지 않는다. 현재 캐릭터는 실제 블록 메시와 bounds가 잘 일치하므로 이 단면 방식이 적합하다. 다만 정확한 메시 Boolean union이나 실제 volumetric raymarch가 아니다. 팔다리 사이의 빈 공간을 연결하는 보수적인 외곽이며, 복잡한 스킨 캐릭터에 대한 정확한 피부 추종까지 검증한 것은 아니다.

## 2. 메인 흐름과 보조 불꽃

- 메인: 개별 birth/root 없이 이어진 필드 위에서 높이·둘레에 연속된 위상으로 상향 흐름, 국소 crest, 침식과 가시 범위가 이동한다. 양쪽으로 넓은 판을 세우지 않는다. 정면 면적을 덜 드러내고 외곽에서 청록/먹물이 읽히도록 조정했다.
- 보조: 기존 실제 삼각형/barycentric 표면 샘플링을 그대로 이용하되 메인이 아닌 24개 작은 불꽃으로 한정했다. 상대 크기 기본값은 0.55다. 기본 분포는 머리2 / 몸통6 / 양팔 각2 / 양다리 각6이다. 하체 가중치1.5와 표면 오프셋0.012m는 유지했다.
- 메인 필드 표면 높이 관측: 약 **0.05095~2.06960m**. CPU carrier 범위이며 셰이더 변위 뒤의 최종 픽셀 높이 측정은 아니다. 2,009개 vertex가 triangle adjacency상 하나로 연결됨을 검사했다.
- 포즈가 바뀌면 단면과 normal을 재계산하고 두 Mesh에 갱신한다. 정지 상태는 재계산하지 않는다. 기존 디테일의 표면 추종과 소유권은 유지했다.

## 3. 검은 외곽

청록과 먹물은 같은 필드 위치·위상·침식·density를 사용한다. 먹물은 공유 형상의 얇은 normal 방향 확장으로 표현하며 별도의 독립 판을 배치하지 않는다. 외곽 facing 범위를 좁혀 검정과 청록이 각각 읽히게 하고 필드 먹물 대비 계수1.10을 적용했다. 기존 `inkColor`, ink opacity/intensity 및 색상 프로필 값은 변경하지 않았다. 작은 디테일의 기존 fragment 먹물 표현도 유지했다.

검은색이 항상 더 강하게 보인다는 정량 판정은 하지 않는다. 비교 캡처에서는 몸 옆의 검은 경계가 남지만, 면의 가시성이 낮아지는 각도에서는 기존 큰 불꽃보다 검은 면적이 작게 보일 수 있다. 사용자의 직접 판단이 필요하다.

## 4. Inspector / 데이터

`Cursed Energy Aura` 및 Pink/Green 프리셋에 동일한 구조 설정을 반영했다.

| 한국어 표시 | 필드 | 기본값 |
|---|---|---:|
| 메인 주력장 두께 (m) | fieldThickness | 0.14 |
| 메인 주력장 밀도 | fieldDensity | 0.9 |
| 보조 불꽃 상대 크기 | detailScale | 0.55 |
| 전신 보조 흐름 수 | detailFlowCount | 24 (기존 값) |

기존 `mainFlowCount: 8`은 더 이상 사용하지 않고 위 세 필드로 교체했다. 보조 바깥/상향 흐름은 보조 전용임을 라벨에 명시했다. SerializedProperty 기반 한글 CustomEditor, Undo 및 기존 편집 흐름을 유지했다. 메인/보조 레이어 ON/OFF는 기존 레이어 설정을 이용한다. 디테일만 켠 경우 숨은 메인 먹물 hull이 남지 않도록 했다.

세 프로필에서 새 세 필드를 옛 `mainFlowCount: 8` 한 줄로 되돌린 바이트의 SHA-256이 시작 시점과 일치했다. 즉 색·강도·입력 fade 등 다른 기존 프로필 내용은 동일하다. 두께는 캐릭터 스케일과 기존 width 설정의 영향을 받는 기준 두께이며 최종 화면에서 균일한 물리 두께를 의미하지 않는다.

## 5. 실제 변경 파일

신규:

- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraField.cs` 및 `.meta`
- 이 보고서 `docs/SHIBUYA_PASS4B2_REVIEW.md`

기존 로컬 파일 중 이번에 수정한 9개:

- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraVisual.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraSurface.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraProfile.cs`
- `unity/Assets/Shibuya/Editor/CursedEnergyAuraInspector.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4B0Tests.cs`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/CursedEnergyAuraVolume.shader`
- `unity/Assets/Shibuya/Resources/Cursed Energy Aura.asset`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/Presets/Pink Aura.asset`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/Presets/Green Aura.asset`

Controller/Input、Combat、Camera、기존 Geometry、Scene、Prefab、Material、Packages、ProjectSettings 및 기존 `.meta`는 이번 패스에서 수정하지 않았다. 이전 handoff/current task의 미커밋 내용도 보존했다.

## 6. Targeted 검증

최종 단일 실행: **14 PASS / 0 FAIL / 0 SKIP**.
경로: `unity/Logs/ShibuyaPass4B2QA/20261024-aura-field/final-targeted/`.
`results.xml`, `execution.jsonl`, `discovery.json`을 보존했다.

검사 범위:

- 필드 단일 연결 topology·전신 높이·idle cache·레이어 분리 캡처
- 표면 샘플 원본/분포/결정성 및 실제 포즈·scale 추종
- F 활성·해제, 피격 억제/복귀, 사망·비활성 해제
- 이동·점프·회전, 기본4타 피해85, 어퍼컷→추격→공중5타의 F hold 조건
- 락온 선택 및 최고70도 캡처
- 청록/핑크/초록, 재질 분리, 해제/파괴 cleanup
- shader 지원/ShaderUtil 오류, 한글 라벨 매핑, SerializedProperty Undo, Asset 참조

중간 시각 시안 `field-pilot` / `field-reveal` / `field-contour` / `field-fringe`는 각각 3 PASS였다. 최종 14개와 중복되는 검사이므로 **26개 고유 PASS로 합산하지 않는다**. 첫 시안이 코팅/껍데기로 보인 점을 확인하고 coverage와 외곽을 수정한 기록도 그대로 남겼다.

30회 ON/OFF 및 12초 hold 후 단일 캐릭터 소유 자원: Material4 / Mesh3 / vertices7,138, 검사 시 입자4. 자원 누적 검사는 통과했다. 최종 Play Mode 종료 후 소유 Material/Mesh는 0/0이다. 이번 요청은 전체 회귀가 아니며 과거 PASS4A 미해결 18건을 해결했다고 주장하지 않는다.

## 7. 캡처와 원속도 영상

아래 경로의 공통 prefix:
`unity/Logs/ShibuyaPass4B2QA/20261024-aura-field/`

- `orbit-original-speed.mp4`: 약6.01초. 측면 유지 후 둘레 회전. 원본173프레임.
- `movement-original-speed.mp4`: 약3.02초. 이동+회전 중 몸 추종. 원본82프레임.
- `compare-02-front-loop.png`, `compare-03-side.png`, `compare-04-back.png`, `compare-05-high-angle.png`, `compare-05c-diagonal.png`, `compare-08-released.png`: LEFT BASELINE=로컬PASS4B.1 / RIGHT CANDIDATE=PASS4B.2 라벨.
- `layer-isolation.png`: combined / main-field-only / surface-detail-only.
- `orbit-contact-sheet.jpg`, `movement-contact-sheet.jpg`: 실제 timestamp를 붙인 연속 구간 표본.
- `final-targeted/legacy/ShibuyaPass4B0QA/`: 원본 PNG, motion/moving 프레임·timestamp, 최고70도, 색상과 분리 캡처, resource/surface/field JSON.

영상은 실제 캡처 timestamp 간격을 ffconcat duration으로 반영한 뒤 30fps로 인코딩했다. 프레임을 균일 간격으로 간주해 재생 속도를 바꾸지 않았다. 필요 프레임 반복만 사용하며 motion interpolation은 없다. 원본 캡처 간격 중앙값은 회전34.52ms / 이동36.08ms, 최대52.88ms / 64.66ms다. 별도 실행의 before/after이므로 불꽃 위상까지 일치하는 픽셀 A/B는 아니다.

## 8. Console / Editor / 성능 범위

최종 Editor는 ready / 컴파일 종료 / Play stopped / ShibuyaCombatMVP scene clean이다. 마지막 recompile 상태 completed, failed=false. 이번 targeted shader 검사에서도 오류가 없었다.

현재 Console ground truth 오류0 / 경고1, Pipeline 보관 buffer 오류0 / 경고26이다. 경고는 `Deleting invalid font reference.`이며 원본을 지우지 않았다. 이 순간의 Console 상태를 과거 전체 QA 이력의 오류0으로 확대하지 않는다. 최종 조회는 `final-editor-state.json`에 기록했다.

QA active=false, Time.timeScale=1, targetFrameRate=-1, vSync=0, AutoTick SessionState 미설정(-1), Play Unfocused=2로 시작 기준과 같다. 이번에 임시 프레임률/AutoTick 변경은 하지 않았다. Windows 화면/마우스 직접 조작은 사용하지 않았다.

새 필드는 포즈가 변할 때 CPU refit 비용이 생긴다. cold fit 관측4.2641ms, 이동 캡처에서 관측한 서로 다른80회 fit의 중앙값2.0554ms / 최대4.7512ms였다. 이는 Editor CPU 해당 계산 범위의 표본이며 전체 프레임 비용/GPU 비용/실기 성능 보장이 아니다. 다인전 확장 전에 refit 비용과 transparent overdraw 최적화 검토가 필요하다. 정지 상태 fitCount가 늘지 않는 것은 검증했다.

## 9. 보존 / Git

시작 시점 tracked+비무시 untracked 파일1,161개를 SHA-256으로 비교했다. 이번 승인 범위의9개만 변경, 나머지1,152개 동일, 누락0. 신규는 Field.cs/.meta와 이 보고서다. 근거: `before-hashes.json`, `preservation-check.json`.

브랜치/HEAD 유지. 시작 staged0 / tracked unstaged28 / untracked94, 종료 staged0 / tracked unstaged28 / untracked97. 다수 aura 파일이 원래 untracked였으므로 Git의 tracked diff만으로 이번 수정 범위를 판단하지 않았다. 기존 Scene/Prefab/GUID·전투 데이터·locked 문서 등 비교 대상은 동일하다. 기존 QA는 삭제하거나 덮지 않고 새 PASS4B2 경로에만 결과를 생성했다. staging/commit/push/merge/reset/clean은 하지 않았다.

## 10. 화면 판독과 남은 한계

확인한 개선: 부위별로 길게 솟던 큰 뿔이 줄고, 발목부터 어깨까지 몸 외곽을 따라 흐름이 이어진다. 메인만 켠 캡처에서도 둘레 흐름이 남으며 작은 표면 불꽃이 주역인 구조는 아니다. 측면/후면/하이앵글에서 네 장의 고정 벽으로 전환되는 모습은 비교 표본에서 줄었다.

**시각 목표 완전 달성으로 판정하지 않는다.** 일부 정면·측면에서는 얇은 주력 코팅/윤곽선이 먼저 읽힌다. 몸 전체에서 불꽃이 풍성하게 피어오르는 레퍼런스의 밀도는 아직 부족할 수 있다. 머리 위 국소 봉우리도 완전히 없어지지는 않았다. 블록 캐릭터의 딱딱한 형상이 필드에 일부 남고, 카메라 facing mask의 특성상 각도별 청록/먹물 면적 차이가 있다. 몸과 분리된 벽을 줄이는 대신 외부 불꽃 공간 점유가 기존보다 줄어든 표본도 있다. 이 변화를 자동으로 성공 또는 강도 유지라고 판정하지 않는다.

## 11. 사용자 직접 확인

`ShibuyaCombatMVP`를 Play하고 **F를 누른 채** 몸 둘레를 마우스로 회전한다. 정면/양측면/후면/45도/최고하이앵글에서 발·다리·허리·어깨가 하나의 흐름으로 연결되는지, 코팅 또는 껍데기처럼 보이지 않는지 확인한다. WASD/Shift 이동과 점프 중 필드가 몸을 따라오는지, F 해제 후 사라지는지 확인한다. 기존 LMB·W/S마무리·공중추격·락온도 평소대로 조작한다.

기본 프로필 `Resources/Cursed Energy Aura`에서 메인과 보조 레이어를 각각 꺼 독립적인 역할을 확인할 수 있다. 원본 프로필을 바꾸면 사용자 작업으로 저장되므로 비교만 필요하면 Play Mode의 임시 프로필을 이용한다. QA 레이어 분리 캡처는 복제한 프로필로 생성했고 저장 Asset은 오염시키지 않았다.

기술 검증 상태까지만 기록하며 최종 시각 승인(USER VERIFIED)은 사용자의 Play Mode 검수 이후다. 여기서 추가 자동 polish나 다음 기능 구현 없이 멈춘다.
