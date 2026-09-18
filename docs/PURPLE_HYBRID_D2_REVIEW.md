# Purple Hybrid D / D2 exploration review

2026-09-15 · **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**

첨부의 최신 D2 지시를 반영해, 어두운 Hybrid D를 동결하고 별도 **D2 — Violent Luminous Mass**를 만들었다. Production 적용은 없다. A/B/C와 기존 결과, D 원본, 3차 baseline, gameplay 수치, Purple_UserPrototype, Scene, Package/ProjectSettings를 보존했다.

## 먼저 볼 자료

- [D ↔ D2 비교 페이지: still + 두 motion](../unity/Logs/PurpleHybridD2QA/comparison.html)
- [Front / Side / SlightLow 비교 시트](../unity/Logs/PurpleHybridD2QA/D_vs_D2_stills.jpg)
- [Front 3초 비교 영상](../unity/Logs/PurpleHybridD2QA/D_vs_D2_Front_3s.mp4), [Orbit 3초 비교 영상](../unity/Logs/PurpleHybridD2QA/D_vs_D2_Orbit_3s.mp4)
- [D2 단독 Front](../unity/Logs/PurpleHybridD2QA/D2_Front_3s.mp4), [D2 단독 Orbit](../unity/Logs/PurpleHybridD2QA/D2_Orbit_3s.mp4)
- [A / B / C / D / D2 정면](../unity/Logs/PurpleHybridD2QA/ABCDD2_Front.jpg)
- [보존된 D의 A/B/C/D 3방향 비교](../unity/Logs/PurpleHybridDQA/ABCD_static_v4.jpg)

Unity: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple Hybrid D2 비교`. D/D2 전환, 동일 시각·각도 선택, 0–3초 재생, D2 한글 Inspector 제공. D 창과 A/B/C 창은 그대로 유지했다. 원본 Editor의 미저장 작업을 조작하지 않고 격리 QA 복사본에서 검증했다.

## 가져온 요소와 달라진 원칙

| 기반 | D에 남긴 요소 | D2에서의 변화 |
|---|---|---|
| A | near-black 질량, 직경 17.5% white core, 얇은 magenta rim | 찬란한 질량을 주역으로 전환. 검정은 틈·깊이로 제한하고, 원형 core는 변형되는 plasma로 교체 |
| B | 얕은 directional tearing으로 둥근 외곽을 약하게 파괴 | 돌 같은 큰 돌출 대신 짧은 찢김과 불연속 외곽 발광 유지 |
| C | 내부 수렴과 느린 표면 흐름의 속도 차이, 채워진 내부 | 내향 에너지와 표면 난류를 유지하고 백색 hotspot 및 분기 방전을 추가 |

D2는 D shader의 밝기만 올린 후보가 아니다. 별도 volume shader에 불규칙한 plasma core, 백색 hotspot, magenta 파열, dark separation을 배치했다. 고정 원형 코어 대신 움직이는 여러 lobe와 경계 변형을 사용한다. 흰 번개 중심과 넓은 hot pink 가장자리가 서로 다른 폭으로 겹치며, 세 사건 시계가 경로·강도·수명을 바꾼다. 본체 내부부터 외부로 뻗는 3D 분기 경로이며 원형 halo mesh는 없다.

사용자의 색 비율은 시각 지침으로 사용했다. 화면 픽셀 비율을 정확히 맞췄다고 주장하지 않는다. 공통 bloom·tone mapping은 변경하지 않았으며, D2의 HDR 발광이 같은 후처리에서 더 강하게 반응한다.

## 실제 화면 기준 visual QA

동일 지름 5, FOV 55, 960×540, 같은 상대 카메라·바닥·배경·후처리. Still은 1.5초. Motion은 30fps, 0–3초 양 끝 포함 91프레임(컨테이너 길이 약 3.03초). D는 동결된 v4 렌더를 재사용했으며 D2 motion을 한 번 생성했다. 실제 렌더의 정면·측면·낮은 각도와 연속 motion 샘플을 직접 비교했다. 실시간 플레이 체감·사용자 시각 승인은 미확인이다.

| 항목 | 화면에서 확인한 결과 | 남은 한계 |
|---|---|---|
| D와 차이 | D는 거의 검은 질량과 작은 흰 원, D2는 백색·hot pink가 지배해 즉시 구분된다 | 어두운 압축 질량의 선명한 중심 대비는 D가 우세 |
| Plasma core | 경계와 모양이 시간에 따라 바뀌고 옆 hotspot과 순간 합쳐진다 | 내부가 하얗게 포화되어 코어 자체의 미세 흐름은 읽기 어렵다 |
| 색·질량 | 어두운 보라 틈이 남고 구체는 속이 찬 형태다. 빈 도넛이나 검은 중심의 눈으로 보이지 않는다 | 흰 영역이 합쳐질 때 일부 표면이 매끄러운 용암 얼룩처럼 보인다 |
| 번개 | 어두운 틈 위에서 굵기 변화와 분기, 출현·소멸이 보인다 | 백색 hotspot 위에서는 번개가 묻힌다. 레퍼런스의 큰 외곽 번개보다 존재감이 작다 |
| Motion / depth | 고정 Front에서도 표면 변형과 core 변화가 있고 Orbit에서 가림과 앞뒤 차이가 보인다 | 순수 내향 수렴은 부분 충족. 눈에는 표면 회전·변형과 방전이 더 먼저 들어온다 |
| 외곽 | 짧은 tearing이 실루엣을 깨며 큰 암석 돌출은 없다 | 전체 외곽은 아직 상당히 원형이다 |

1차 D2 still의 과도한 미세 점광은 큰 에너지 구조로 교체했고, 지나치게 둥근 hotspot은 찢어진 ridge로 보정했다. 최종 결과에도 넓은 흰 포화와 둥근 패치가 일부 남아 있다. 이를 production 완성도로 간주하지 않는다.

레퍼런스 `_local_refs/videos/purple_selfdestruct_reference.mp4`를 직접 확인했다. D2는 D보다 백색·magenta의 폭주 강도에 가깝다. 그러나 레퍼런스의 날카로운 그래픽 파열, 거대한 외곽 방전, 폭발 단계 전체를 재현한 것은 아니다. 이번 결과는 본체 비교용이다. 레퍼런스 미디어를 runtime asset으로 복사하지 않았다.

## 선택할 때의 장단점 / 비용

| 방향 | 장점 | 단점 / 비용 |
|---|---|---|
| D | 암부·core 대비, 압축된 질량, 본체 구조 판독 | 초신성형 레퍼런스에는 어둡다. 본체 1 renderer, raymarch 최대 176 step |
| D2 | 자폭 Purple의 밝은 에너지, 변형 core, 여러 hotspot, 분기 방전 | 흐름 가독성 저하·백색 합침. 본체 1 renderer + LineRenderer 12개, 전용 material 2개, raymarch 최대 144 step |

Renderer 수는 GPU draw 측정값이 아니다. 실제 batching·GPU ms·최저 FPS는 측정하지 않았다. D2는 step 수가 적어도 shader 연산, 12개 선, 투명 중첩과 HDR 비용이 추가되므로 D보다 빠르다고 결론 내릴 수 없다. VFX Graph·새 package·texture 없이 기존 URP에서 실행된다.

추천은 **자폭/초신성 목표에는 D2 방향을 검토하되, 즉시 production 이식은 보류**하는 것이다. 어두운 압축 질량을 목표로 한다면 D가 더 명확하다. 최종 방향과 production 채택 요소는 사용자가 선택한다.

다음 수정 후보 3개:

1. 백색 hotspot과 core가 붙는 구간을 분리하고, 둥근 얼룩을 날카로운 파열로 바꾸기.
2. 내향 흐름이 지나갈 어두운 통로를 확보해 표면 회전과 수렴을 구별하기.
3. 밝은 본체 위에서도 주 번개와 분기가 읽히도록 경로·두께·강도 spike를 정리하기.

## 최소 검증 결과

- [Preview 생성·D/D2 전환·종료](../unity/Logs/AstraCombatQA/purple-d2-preview-04.xml): **1/1 PASS**. 기존 카메라 위치/FOV/target/후처리, 사용자 오브젝트 위치·자식 수 보존, D2 material 정리, gameplay Health 없음 확인.
- [최종 still / motion 생성](../unity/Logs/AstraCombatQA/purple-d2-motion-final.xml): **1/1 PASS**. 두 shader의 지원·compiler 메시지 확인. **최종 targeted 합계 2/2**, C#·shader compile error **0**.
- 첫 preview 시도는 테스트의 중첩 coroutine이 Play Mode 진입을 전달하지 못해 실패했다. 테스트 진입부만 수정한 뒤 통과했다. 실패 로그도 보존했다.
- 최신 효율 지시에 따라 전체 regression / production-level QA / 동일 렌더 재현성 검사 미실행. D2 최종은 still 3장과 raw motion 두 개에서 영상을 만들었다. 기존 A/B/C/D 영상은 재렌더하지 않았다.
- `git diff --check`는 보존된 `VFXLab.unity`의 기존 trailing whitespace 8곳 때문에 실패한다. Scene 파일의 해시는 동일하며 이 작업에서 수정하지 않았다.

## 파일 / 보호 / Git

D2 신규 파일(각 `.meta` 포함):

- [Profile](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2Profile.cs), [Prototype](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2Prototype.cs), [Bench](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2Bench.cs)
- [비교 창·한글 Inspector](../unity/Assets/Editor/PurpleBodyHybridD2Window.cs), [targeted tests](../unity/Assets/Editor/PurpleBodyHybridD2Tests.cs)
- [본체 shader](../unity/Assets/Resources/VFX/HollowPurpleHybridD2.shader), [분기 방전 shader](../unity/Assets/Resources/VFX/HollowPurpleHybridD2Rupture.shader), [D2 설정 asset](../unity/Assets/Resources/VFX/PurpleBodyHybridD2Profile.asset)

이번 연속 작업의 앞선 D 단계에서 추가한 파일도 남긴다(각 `.meta` 포함). D2 시작 이후 아래 7개는 모두 동결했다:

- [D Profile](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridDProfile.cs), [D Prototype](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridDPrototype.cs), [D Bench](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridDBench.cs)
- [D 비교 창](../unity/Assets/Editor/PurpleBodyHybridDWindow.cs), [D tests](../unity/Assets/Editor/PurpleBodyHybridDTests.cs), [D shader](../unity/Assets/Resources/VFX/HollowPurpleHybridD.shader), [D 설정 asset](../unity/Assets/Resources/VFX/PurpleBodyHybridDProfile.asset)

문서: 이 보고서 신규, `CURRENT_HANDOFF.md`·`active/CURRENT_TASK.md` 상단 상태 갱신. QA 스크립트와 렌더는 Git에서 무시되는 `unity/Logs/PurpleHybridD2QA`에만 저장한다.

[보호 확인](../unity/Logs/PurpleHybridD2QA/final-audit.json): D2 시작 snapshot 541개 중 보고서용 기존 문서 2개를 제외한 **539개 해시 동일**. D 원본·선택된 D v4 증거 파일 포함. 3차 고정 baseline **49/49 동일**. 별도로 기존 영상 수백 장의 재현성 검사는 하지 않았다.

[최종 git status 전체](../unity/Logs/PurpleHybridD2QA/final-git-status.txt): tracked modified **20**, untracked **87**, index 비어 있음. 이 숫자는 기존 dirty tree를 포함한다. D2 신규 16개와 이 보고서 1개가 D2 시작 대비 추가되었다. branch `feat/gojo-blue-screen-distortion`, HEAD `a9c6ccf9c09b2effdd559e83e865296e45f600b2` 유지. commit / push / merge / reset / clean / restore / staging 없음.

**사용자 선택 대기:** 이 Hybrid D2를 production에 반영할 가치가 있는가? 다음에 더 손봐야 할 3가지는 무엇인가?
