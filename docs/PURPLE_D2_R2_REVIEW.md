# Purple D2-R2 — 입체 아크 / 찢어진 플라즈마

2026-09-16 · **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**

R1의 밝은 magenta/purple와 내부 source·내향 흐름 기반을 유지하며 별도 **D2-R2**를 만들었다. 기존 D2, D2-R1, A/B/C/D, production Purple과 3차 baseline은 덮어쓰지 않았다. Production 적용 없음.

## 실제 비교 자료

- [R1 ↔ R2 비교 페이지](../unity/Logs/PurpleD2R2QA/comparison.html)
- [Front / Side / SlightLow 비교 시트](../unity/Logs/PurpleD2R2QA/R1_vs_R2_stills.jpg)
- [Front 3초 비교](../unity/Logs/PurpleD2R2QA/R1_vs_R2_Front_3s.mp4), [Orbit 3초 비교](../unity/Logs/PurpleD2R2QA/R1_vs_R2_Orbit_3s.mp4)
- R2 원본: [Front still](../unity/Logs/PurpleD2R2QA/Motion/20260915_201636_711/R2_Front.png), [Side still](../unity/Logs/PurpleD2R2QA/Motion/20260915_201636_711/R2_Side.png), [SlightLow still](../unity/Logs/PurpleD2R2QA/Motion/20260915_201636_711/R2_SlightLow.png)
- R2 단독 영상: [Front](../unity/Logs/PurpleD2R2QA/R2_Front_3s.mp4), [Orbit](../unity/Logs/PurpleD2R2QA/R2_Orbit_3s.mp4)

조건: 같은 지름 5, FOV 55, 960×540, 상대 카메라 위치·바닥·조명·후처리. Still 1.5초, motion 30fps 0–3초 양 끝 포함 91프레임(컨테이너 약 3.03초). R1의 보존된 raw render를 읽어 비교했으며 R1은 재렌더하지 않았다.

Unity: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple Hybrid D2-R2 비교`. 별도 R1/R2 비교 창과 한글 Inspector 제공. 기존 창·Scene·production 카메라를 수정하지 않았다.

## 무엇을 어떻게 바꿨는가

| 우선순위 | R2 구현 | 실제 화면에서 확인한 변화 |
|---|---|---|
| 평면 번개 → 3D 아크 | R1의 카메라를 향하는 LineRenderer를 R2에서 사용하지 않음. 두께·단면·법선이 있는 튜브 mesh. 내부 광원에서 표면을 타고 외부로 이어지는 경로, 전경/후경/측면 배치. 구체 통과 거리로 색·투명도 감쇠 | R1의 검은 테두리와 넓은 흰 지그재그 인상이 줄었다. Orbit에서 앞쪽 아크는 분홍 단면이 보이고 안쪽/뒤쪽 구간은 약해지거나 가려진다. 단, 밝은 코어 위에서는 여전히 가는 선처럼 합쳐질 수 있다 |
| Cellular → directional plasma | 둥근 noise 등고선 대신 늘어난 방향성 noise·전단 변형·좁은 고발광 섬유. 외곽도 같은 찢김 언어로 변형 | 둥근 세포/비늘 인상은 감소했고 길게 찢긴 결과 불규칙 외곽이 명확하다. 다만 긴 결이 일부 시점에서 줄무늬·겹친 띠처럼 반복된다 |
| 흰 원 → 폭주 광원 묶음 | 3개 중앙 emitter가 서로 다른 깊이·방향에서 겹치는 soft density cluster. 별도 secondary hotspot 3개 유지. 뚜렷한 SDF 흰 원 경계를 제거하고 주변 발광 확대 | R1의 작은 흰 원보다 큰 비정형 white-hot 덩어리로 읽힌다. Orbit에서 source 겹침과 밝기가 변한다. 일부 각도에서는 암부가 밝은 부위를 잘라 둥근 패치나 구멍처럼 보이는 순간이 남는다 |
| Pitch-black negative space | 얇고 밀도 높은 surface void가 빛을 가리는 별도 흡수 항. 본체 주색은 magenta/purple 유지 | 밝은 섬유 사이에 거의 검은 틈이 생겨 대비가 증가했다. 어두운 D로 복귀하지 않았다. 일부 틈은 자연스러운 공간 파열보다 반복된 띠로 보인다 |

VFX Graph, 새 package, 외부 texture를 사용하지 않았다. 기존 URP 안의 별도 HLSL volume shader와 절차적 mesh 방식이다. 후처리 asset은 변경하지 않고 emitter의 HDR과 경계만 조정했다. 검증용 구체 내부에서 배경 바닥이 비치지 않게 한 R1 합성도 유지했다.

## 실제 화면 기준 평가

최종 3방향 still과 Front/Orbit 연속 렌더 샘플을 직접 비교했다. 코드가 3D라는 사실만으로 입체감이 완성됐다고 판정하지 않았다.

**R1 대비 좋아진 점:** 둥근 표면 무늬와 매끈한 공 인상이 줄고, 중심 발광 덩어리가 커졌다. 날카로운 암부와 외곽 찢김으로 밀도·대비가 강해졌다. 아크는 R1보다 본체를 감싸고 외부로 빠져나오는 관계가 읽힌다. 특히 Orbit 0–0.6초와 1.2–1.8초 샘플에서 앞뒤 아크의 색·가림 차이를 확인할 수 있다.

**아직 남은 한계:**

1. 아크의 입체감은 부분 개선이다. core와 겹칠 때 단면 정보가 포화되고, 작은 화면에서는 가는 선으로 읽힌다. 본체 가림은 구체 통과 거리 기반 근사이므로 실제 불규칙 volume과 완전히 일치하지 않는다.
2. 표면의 긴 결이 너무 같은 방향으로 정렬되는 구간이 있다. cellular는 줄었지만 띠·찢어진 천 같은 인상이 새로 생길 수 있다. 장난감 인상이 완전히 제거됐다고 보지 않는다.
3. 중앙 white-hot 강도는 올라갔지만 여러 hotspot이 항상 별개로 읽히지는 않는다. 좁은 void와 겹쳐 일부 밝은 영역이 둥글게 갈라지는 순간도 있다.
4. 내향 흐름은 보존했으나 첫인상은 여전히 표면 전단과 회전이 우세하다. 자폭 직전의 불규칙한 내부 폭주·대규모 외곽 방전은 레퍼런스보다 약하다.

Reference는 사용자 자폭 무라사키 영상의 발광 질량·날카로운 파열·입체 방전을 기준으로 봤다. R2의 강한 중심광과 찢어진 형태는 그 방향에 가까워졌으나, reference의 압도적인 내부 에너지와 거친 움직임까지 충족했다고 평가하지 않는다.

**추천:** R1 다음의 refinement 후보로는 조건부 추천한다. 특히 core cluster와 검은 균열·찢어진 외곽은 비교 가치가 있다. 현재 결과 전체를 production 완성품으로 추천하지 않는다. 다음 선택은 사용자가 실제 영상에서 입체 아크와 표면 결을 보고 결정한다. 임의로 이식하지 않는다.

## 최소 검증 / 비용

- [최종 preview 검사](../unity/Logs/AstraCombatQA/purple-r2-preview-04.xml) **1/1 PASS**; [최종 shader·still·motion 검사](../unity/Logs/AstraCombatQA/purple-r2-motion-final.xml) **1/1 PASS**. 최종 **C# / shader compile error 0**, targeted 합계 **2/2**.
- 최초 시도에서 shader loop 인덱스와 mesh 끝점의 NaN이 발생했다. 안전한 인덱스 및 삼각함수 값 clamp로 수정했고 이후 정상 렌더·검증을 확인했다. 실패 로그는 보존했다.
- Full regression·production-level QA는 실행하지 않았다. 최종 still 3장과 raw motion 2개를 만들었다. 시도별 preview 포함 전체 PNG **19장**이며 수백 장 캡처는 없다.
- 효과 본체 기준 renderer 2개(volume + 합친 arc mesh), runtime material 2개. 아크 mesh 1,792 vertex / 3,472 triangle, 최대 raymarch 160 step. R1의 10개 renderer보다 수는 적지만 mesh 갱신과 volume 연산 비용이 있어 더 빠르다고 결론 내리지 않는다. GPU ms·실제 플레이 FPS는 미측정.

## 변경 파일 / 보존 확인

신규 R2 8개 source/asset와 각각의 `.meta`:

- [Profile](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2R2Profile.cs), [Prototype](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2R2Prototype.cs), [Bench](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2R2Bench.cs)
- [비교 창·한글 Inspector](../unity/Assets/Editor/PurpleBodyHybridD2R2Window.cs), [최소 검사](../unity/Assets/Editor/PurpleBodyHybridD2R2Tests.cs)
- [volume shader](../unity/Assets/Resources/VFX/HollowPurpleHybridD2R2.shader), [3D arc shader](../unity/Assets/Resources/VFX/HollowPurpleHybridD2R2Arc.shader), [설정 asset](../unity/Assets/Resources/VFX/PurpleBodyHybridD2R2Profile.asset)

이 보고서를 추가하고 `CURRENT_HANDOFF.md`·`active/CURRENT_TASK.md` 상단 상태를 갱신했다. QA 스크립트·렌더는 Git에서 무시되는 `unity/Logs/PurpleD2R2QA`에 저장했다.

[보호 manifest](../unity/Logs/PurpleD2R2QA/final-audit.json): 시작 snapshot **576개** 중 상태 문서 2개를 제외한 **574개 동일**, 3차 baseline **49/49 동일**. D2와 D2-R1의 원본 source/asset·보존된 비교 영상, production, 기존 Scene과 관련 없는 로컬 변경을 보존했다. Package/ProjectSettings·gameplay 수치 수정 없음.

[최종 git status -sb](../unity/Logs/PurpleD2R2QA/final-git-status.txt): 기존 dirty tree 포함 **modified 20 / untracked 121**, index 비어 있음. 이번 신규는 R2 16개(source/asset + meta)와 보고서 1개다. Branch `feat/gojo-blue-screen-distortion`, HEAD `a9c6ccf9c09b2effdd559e83e865296e45f600b2` 유지. commit / push / merge / reset / clean / restore / staging 없음.
