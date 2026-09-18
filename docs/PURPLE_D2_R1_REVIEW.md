# Hollow Purple — D2-R1 refinement review

2026-09-15 · **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**

현재 D2를 동결된 비교 기준으로 보존하고 별도 **D2-R1 — Internal Supernova + Violent Outer Energy**를 추가했다. 밝은 Purple/Magenta, 구형 본체, 내향 흐름·표면 난류 기반을 유지했다. Production에는 적용하지 않았다.

**중단 후 마무리:** 완성된 R1 구현·렌더·검증 결과를 그대로 확인하고 이 보고서의 경로와 시각 판독만 보완했다. 새 visual 수정, 추가 polish, Unity 검증 재실행, 새 캡처는 없다.

## 비교 자료

- [D2 ↔ R1 비교 페이지](../unity/Logs/PurpleD2R1QA/comparison.html)
- [Front / Side / SlightLow still](../unity/Logs/PurpleD2R1QA/D2_vs_R1_stills.jpg)
- [Front 3초 비교](../unity/Logs/PurpleD2R1QA/D2_vs_R1_Front_3s.mp4), [Orbit 3초 비교](../unity/Logs/PurpleD2R1QA/D2_vs_R1_Orbit_3s.mp4)
- [R1 단독 Front](../unity/Logs/PurpleD2R1QA/R1_Front_3s.mp4), [R1 단독 Orbit](../unity/Logs/PurpleD2R1QA/R1_Orbit_3s.mp4)
- 원본 PNG: [Front still](../unity/Logs/PurpleD2R1QA/Motion/20260915_091841_659/R1_Front.png), [Side still](../unity/Logs/PurpleD2R1QA/Motion/20260915_091841_659/R1_Side.png), [SlightLow still](../unity/Logs/PurpleD2R1QA/Motion/20260915_091841_659/R1_SlightLow.png)

Unity: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple Hybrid D2-R1 비교`. 별도 창에서 D2/R1 전환, 동일 시각·각도 선택과 재생, R1 전용 한글 Inspector를 제공한다. 기존 A/B/C·D·D2 창과 source는 수정하지 않았다.

동일 지름 5, FOV 55, 960×540, 같은 상대 카메라·바닥·배경·후처리. Still은 1.5초, motion은 30fps 0–3초 양 끝 포함 91프레임(컨테이너 약 3.03초). D2 비교 영상은 기존 raw render를 읽어 사용했다. 생성 이미지는 실제 Unity 렌더이며 reference를 합성한 그림이 아니다.

## 문제와 refinement 방식

D2의 가장 큰 문제는 넓은 흰 영역이 표면 noise와 함께 움직여 내부 광원보다 **표면에 칠한 흰 무늬**로 보이는 것이었다. R1은 단순히 전체 발광을 낮추는 대신 흰 빛의 발생 위치를 내부 공간으로 제한한다.

| 항목 | R1 구현 |
|---|---|
| Surface / internal 분리 | 하나의 volume raymarch 안에서 shell·중간층·내부 source의 밀도와 색을 분리. Surface noise에는 넓은 white emission을 주지 않고 Purple/Magenta와 얇은 파열을 배치 |
| Primary plasma core | 내부 중심의 불규칙한 3D 밀도장. 경계 boiling과 미세한 반경 변화. 화면 위에 완벽한 흰 원을 덧그리지 않음 |
| Secondary hotspots | 서로 다른 3D 위치·깊이에 3개. 별도 속도로 이동하고 발광 위상이 다름. 앞 레이어를 통과한 빛은 밝기와 색이 감쇠 |
| Small energy | 8개 제한된 내향 streak가 수렴하며 사라짐. 작은 발광 요소를 무제한 noise로 늘리지 않음 |
| Hero lightning | 최대 3개 주 경로 + 각 1개 분기. 실제 core/hotspot 좌표에서 시작해 내부를 가로지르며 일부가 외곽으로 돌출. White 중심·magenta edge·좁은 purple 대비 공간 |
| Instability | source별 서로 다른 발광 spike, 비동기적인 번개 강도·수명, 작은 국소 외곽 변형. 전체 구체를 크게 호흡시키지 않음 |
| Outer energy | 국소 외향 번개 끝과 최대 3개 짧은 magenta 조각. 원형 orbit ring, fog, particle storm 없음 |
| 채워진 본체 | 내부 에너지끼리의 가림은 유지하고 배경 바닥이 구체를 관통해 보이던 합성 결함을 수정 |

새 package / texture / Scene asset 없이 기존 URP에서 구현했다. Reference는 기존 `purple_selfdestruct_reference.mp4`의 강한 내부 에너지와 큰 방전 언어를 참고했다. 낮은 해상도·bloom 번짐 자체를 복제하지 않았다.

## Visual QA와 남은 한계

정지 프레임에서 R1의 표면은 보라·magenta이고, 밝은 source들이 분리되어 D2의 흰 대륙 같은 패턴이 줄었다. 가까운 source와 뒤쪽 source의 색·가림 차이가 생겼다. 같은 카메라에서 번개는 D2보다 분명하게 읽힌다. 어두운 D로 돌아가지는 않았지만, 화면 전체의 백색 점유율과 백색 폭주감은 D2보다 낮아졌다.

최종 Front/Orbit 0–3초 연속 렌더 샘플을 직접 검토했다. 1.8초 부근에서는 번개가 잠시 약해지며 core와 분리된 hotspot들이 보이고, 2.4초 부근에는 큰 방전이 외곽을 가로지른다. Orbit에서는 광원의 겹침·가림과 pink/white 강도가 달라진다. 순수 내향 수렴은 여전히 표면 흐름과 번개보다 약하게 읽힌다. 작은 외곽 조각은 일부 구간에서 주 번개에 묻힌다. 실제 Play Mode 시각 승인을 대신하지 않는다.

최종 판정은 **분리와 가독성의 개선, 폭주 강도의 감소가 함께 있는 결과**다.

| 사용자 확인 항목 | 실제 렌더 기준 판독 |
|---|---|
| Hotspot separation | D2보다 분명하다. 번개가 약해지는 구간에는 core 주변의 별도 광원이 읽힌다. 3개가 매 순간 모두 또렷한 것은 아니며, 뒤쪽 광원은 pink 얼룩으로 보이기도 한다 |
| Internal depth | Orbit에서 앞뒤 가림과 색·강도 차이가 보여 부분적으로 좋아졌다. 정지 화면에서는 깊이보다 서로 떨어진 발광 덩어리로 먼저 보일 수 있다. 내부 전역이 입체적으로 폭주한다는 인상까지 완성된 것은 아니다 |
| Hero lightning | 밝은 magenta 본체 위에서는 D2보다 잘 읽힌다. 흰 core와 겹치는 부분은 여전히 합쳐진다. 밝은 전투 맵·다른 VFX 중첩 조건은 검증하지 않아 모든 밝은 배경에서 충분하다고 단정할 수 없다 |
| D2보다 약해진 점 | **넓은 백색 폭주감이 줄었다.** 표면의 흰 무늬를 줄인 대신 화면을 채우는 백색 에너지와 자폭 직전의 압도감도 낮아졌다 |
| 자폭 무라사키 reference와 거리 | 광원과 방전 구조는 정리됐지만, 더 거친 내부 불안정성·여러 깊이의 강한 white-hot 파열·큰 외곽 방전은 아직 부족하다. R1이 모든 면에서 D2보다 우수하다는 판정은 아니다 |

현재 한계:

- 광원들이 분리되어도 일부 hotspot은 둥근 작은 덩어리처럼 보인다. 연속적인 내부 plasma 파열로 더 연결할 여지가 있다.
- 번개의 좁은 대비 공간 덕분에 잘 읽히지만, 강한 구간은 외곽선이 있는 그래픽 지그재그처럼 보인다.
- 표면의 부드러운 흐름이 여전히 눈에 들어온다. 내부 수렴과 폭주가 항상 첫인상이 되는 수준인지는 사용자 motion 평가가 필요하다.
- 실제 전투 배경, 원거리 크기, 다른 VFX와 중첩된 상태, 실시간 FPS는 이번 승인용 preview 범위에서 검증하지 않았다.

레퍼런스에 가까워진 점은 **분리된 내부 광원·굵은 방전·국소 외곽 파열**이다. 레퍼런스 수준의 거친 불안정성과 내부 전역의 강한 백색 에너지가 완성됐다고 보지는 않는다.

## 선택과 성능

| 다음 조정 방향 | 장점 | 단점 / reference 접근도 | 비용 |
|---|---|---|---|
| 내부 발광 spike를 더 폭발적으로 | 자폭 Purple의 불안정성과 순간 백색 에너지 강화 | source와 번개가 다시 합쳐질 위험. 레퍼런스의 폭주 강도에 가까움 | 같은 구조에서 수치 조정은 renderer 수 변화 없음. HDR·후처리 실제 비용 미측정 |
| 현재 분리를 유지하고 깊이·번개 형태 정교화 | 내부 source와 표면의 관계, 가림이 더 명확 | 백색 폭주감은 상대적으로 약할 수 있음 | 현재 shader 비용을 기본으로 검토 |

추천은 **R1의 분리를 유지하면서 짧은 내부 spike를 검토**하는 것이다. 추가 조정 방향을 임의로 확정하거나 production에 옮기지는 않는다. 현재 R1을 먼저 사용자에게 보여주고 선택을 기다린다.

R1은 본체 renderer 1개 + LineRenderer 9개, runtime material 2개, raymarch 최대 160 step이다. D2는 1+12 renderer, 최대 144 step. R1은 선이 줄어도 내부 source 4개의 noise·밀도 연산과 색 감쇠가 추가돼 더 빠르다고 주장할 수 없다. GPU ms·실제 draw call은 측정하지 않았다.

## 최소 QA / 보호 / Git

- [Preview 생성·D2/R1 전환·cleanup](../unity/Logs/AstraCombatQA/purple-r1-preview-02-retry.xml) **1/1 PASS**. 기존 카메라 위치/FOV/render target/후처리와 사용자 오브젝트 위치·자식 수 보존, R1 material cleanup 확인.
- [최종 shader·still·motion](../unity/Logs/AstraCombatQA/purple-r1-motion-filled-retry.xml) **1/1 PASS**. 최종 targeted **2/2 PASS**, C#·shader compile error **0**. 마지막 합성 수정은 이 최종 렌더 검사에서 확인했다.
- Unity 외부 빌드 도구의 접근 거부 1회와 ILPP 인수 처리 오류 1회가 렌더 시작 전에 발생했다. 같은 격리 QA에서 재시도 후 통과했다. 실패 로그도 보존했고 source 오류로 숨기거나 삭제하지 않았다.
- 전체 regression / production-level QA / 동일 렌더 재현성 검사 미실행. Preview 2회 각각 4 PNG, motion 자료 각각 3 PNG + raw stream 두 개. 배경 비침 수정으로 motion을 한 번 갱신했다. **이번 R1 전체 생성 PNG 14장**, 수백 장 캡처 없음. 기존 D2 motion은 재생성하지 않았다.
- [보호 manifest](../unity/Logs/PurpleD2R1QA/final-audit.json): 시작 snapshot **557개** 중 상태 갱신용 문서 2개 외 **555개 동일**. 3차 frozen baseline **49/49 동일**. Production, D/D2, A/B/C, 기존 사용자 Scene·prototype, gameplay 및 Package/ProjectSettings 보존.
- [최종 git status](../unity/Logs/PurpleD2R1QA/final-git-status.txt): 기존 dirty tree 포함 **modified 20 / untracked 104**, index 비어 있음. 이번 추가는 R1 16개(source/asset 8 + meta 8), 보고서 1개. 기존 문서 2개 상단 상태만 갱신했다.
- Branch `feat/gojo-blue-screen-distortion`, HEAD `a9c6ccf9c09b2effdd559e83e865296e45f600b2` 유지. commit / push / merge / reset / clean / restore / staging 없음. **Production 미적용, 사용자 시각 승인 대기**.
- 중단 후 [읽기 중심 재확인](../unity/Logs/PurpleD2R1QA/resume-check.json): 보호 파일 **555개**, 3차 baseline **49개**, 완료된 R1 source/meta **16개** 해시 동일. [재확인 git status -sb](../unity/Logs/PurpleD2R1QA/resume-git-status.txt)도 modified 20 / untracked 104다. 위 2/2 PASS는 기존 검증 기록이며 이번 재개에서 테스트를 새로 실행한 결과가 아니다.

신규 R1 source/asset 8개(각 `.meta` 포함):

- [Profile](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2R1Profile.cs), [Prototype](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2R1Prototype.cs), [Bench](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2R1Bench.cs)
- [비교 창·한글 Inspector](../unity/Assets/Editor/PurpleBodyHybridD2R1Window.cs), [최소 테스트](../unity/Assets/Editor/PurpleBodyHybridD2R1Tests.cs)
- [본체 shader](../unity/Assets/Resources/VFX/HollowPurpleHybridD2R1.shader), [rupture shader](../unity/Assets/Resources/VFX/HollowPurpleHybridD2R1Rupture.shader), [설정 asset](../unity/Assets/Resources/VFX/PurpleBodyHybridD2R1Profile.asset)

이 보고서 신규, `CURRENT_HANDOFF.md`·`active/CURRENT_TASK.md` 상태 갱신. QA 도구·raw stream·비교 자료는 Git에서 무시되는 `unity/Logs/PurpleD2R1QA`에 저장한다.
