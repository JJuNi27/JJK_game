# Hollow Purple — Purple Body Prototype Exploration Review

2026-09-15 · **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**

A/B/C를 기존 생성 결과에서 이어서 비교했다. **A의 어두운 질량, B의 불규칙 외곽, C의 내부층은 실제 렌더에서 구별된다.** B의 가시가 빽빽한 경계와 C의 층 전체를 물들이던 발광을 prototype 안에서 보정했다. Production Purple에 가져간 요소는 없다. 다음 단계는 사용자의 요소 선택이다.

## 비교 자료와 실행

- [비교 페이지: 정지 화면과 동시 재생 영상](../unity/Logs/PurpleBodyExplorationQA/comparison.html)
- [최종 3방향 비교](../unity/Logs/PurpleBodyExplorationQA/ABC_static_final5.jpg)
- [고정 정면 3초 A/B/C 동시 재생](../unity/Logs/PurpleBodyExplorationQA/ABC_Front_motion_final5.mp4)
- [같은 카메라 궤도 3초 A/B/C 동시 재생](../unity/Logs/PurpleBodyExplorationQA/ABC_Orbit_motion_final5.mp4)
- [후처리 OFF 비교](../unity/Logs/PurpleBodyExplorationQA/ABC_no_post_final5.jpg)
- [A 확대 시간 비교](../unity/Logs/PurpleBodyExplorationQA/A_motion_detail_final5.jpg), [B 확대 시간 비교](../unity/Logs/PurpleBodyExplorationQA/B_motion_detail_final5.jpg), [C 확대 시간 비교](../unity/Logs/PurpleBodyExplorationQA/C_motion_detail_final5.jpg)

Unity에서는 기존 **VFXLab을 Play Mode로 실행**한 뒤 `Tools > JJK Game > VFXLab > Purple 본체 A B C 비교` → `별도 비교 화면 열기`를 사용한다. 후보와 정면/측면/약간 낮게/동일 궤도를 선택할 수 있다. 후보 전환은 같은 시각에 정지하며, `0 → 3초 재생`으로 같은 구간을 재생한다. `비교 종료` 또는 창 닫기는 비교용 오브젝트만 정리한다. 새 단축키는 없다. 원본 Scene을 자동으로 열거나 저장하지 않는다.

비교 자료는 로컬 전용 `unity/Logs/` 아래에 있다. Git에 바이너리 결과를 추가하지 않았다. 이전 `ABC_static_v1.jpg`, `ABC_static_v2.jpg`, `Frames_render2/` 및 이후 검토 단계 자료도 유지했다. 최종 검토 화면의 원본은 `Frames_final5/`이다. 이제 자동 테스트를 다시 실행하면 `Runs/<UTC 시각_GUID>/`에 새 결과를 만들므로 검토된 결과를 덮어쓰지 않는다.

## 공통 조건

| 항목 | 모든 후보에 적용한 조건 |
|---|---|
| Unity / GPU | Unity 6000.3.20f1, URP 17.3.0, Direct3D 12, NVIDIA GeForce RTX 5060 |
| 구현 기반 | 새 exploration 전용 URP HLSL shader. Shader Graph 17.3 사용 가능 여부는 확인했지만 이 구현은 HLSL 사용. VFX Graph 없음, 패키지 추가 없음 |
| 지름 / 위치 | 명목 지름 5, 동일 중심·transform·거리. B의 변위와 C의 투명층 때문에 보이는 경계는 다름 |
| 코어 | 공통 지름 비율 0.175, 발광 7, 얇은 hot-pink rim. 동일한 작은 투영 중심광 |
| 몸체 | 공통 density 1.15, emission 0.7, near-black / deep-violet / magenta / violet / hot-pink 팔레트 |
| 카메라 | 별도 Untagged camera, FOV 55, 960×540, HDR 렌더 경로와 sRGB 표시용 target |
| 정지 화면 | Front / Side / Slight Low 모두 공통 시각 1.50초 |
| 움직임 | Front 고정 및 동일 Orbit, 0–3초를 30fps로 직접 렌더. 0초와 3초 끝점을 포함해 각 91프레임 |
| 조명·배경·후처리 | 동일한 비교 바닥·배경, 기존 VFXLab 조명/volume 조건. 후보별 노출·bloom 변경 없음 |
| 후처리 OFF | 비교용 카메라만 후처리 비활성화해 3개 추가 캡처. Bloom뿐 아니라 tone mapping도 꺼진 진단 화면 |
| 최종 원본 | 3방향 정지 9 + 후처리 OFF 3 + motion 546 = **558 PNG** |

Triptych 영상은 실제 PNG를 축소 배치한 것이다. 새 효과를 합성하거나 보간 프레임을 만들지 않았다. 91프레임 MP4의 컨테이너 길이는 약 3.03초이며 표현된 효과 시각은 0–3초다. 반복 재생 경계의 3→0초 전환은 이 실험의 루프 경계이며, 루프가 자연스럽게 이어지도록 authoring한 기술은 아니다.

몸체는 scene-depth를 읽는 3D density field다. 빛과 색은 고정 방향의 shader shading을 포함하므로 이 비교로 도시 맵의 모든 조명 조건을 검증한 것은 아니다. Core도 실제 공간을 비추는 광원이 아니라 공통으로 계산한 내부 중심광이다.

## A — Dense Dark Mass

**실험 목표:** 작은 백색 코어와 넓은 흑자색 면적만으로 압축된 무게를 만들 수 있는지 확인.

**실제 화면 평가:** 세 시점 모두 작은 백색 중심과 어두운 Purple 면이 먼저 읽힌다. 정면 1.50초의 백색 영역 등가 지름은 약 49px, 명목 본체는 약 300px다. 핑크 테두리를 제외한 백색은 약 16.3%로 작게 보이며, 설정값 17.5%와 일관된다. 후처리 OFF에서도 중심과 경계가 유지되어 bloom에 의존하지 않는다. 밝은 성운 같은 면 발광은 없다.

**구현:** 불투명에 가까운 흡수 질량, 제한된 보라색 결/방전, 공통 중심광. 본체 transform을 통째로 회전시키지 않고 재료 좌표의 움직임을 사용한다. 재개 이후 A의 크기·팔레트·밀도·패턴 방향은 유지했다. 공통 수치 샘플링 품질만 개선해 동심원 계단과 미세 입자무늬를 줄였다.

**장점:** 가장 명확한 코어/질량 대비, 높은 암부 비중, 세 후보의 무게감 기준으로 사용하기 좋다. 작은 화면에서도 정체성이 유지된다.

**단점·한계:** 완벽한 원형 외곽은 그대로다. 정면에서는 둥근 물체보다 어두운 원판으로 읽힐 여지가 있고, C와 비교하면 내부 깊이가 적다. 본체만 놓고 볼 때 움직임의 강도는 낮은 편이다.

**레퍼런스 접근도:** 사용자가 요구한 ‘밝은 성운 대신 압축된 저주 질량’의 색·명도 구조에 가장 가깝다. 영상 레퍼런스의 난류 경계와 입체적인 운동까지 충족한 후보는 아니다.

**예상 비용:** A/B 중 기본 경로. 본체 1 draw, ray당 최대 128 density sample, 불투명 구간에서 조기 종료. 일반 단일 표면 shader보다 비싸다. 확정 GPU ms나 목표 플랫폼 FPS는 측정하지 않았다.

**가져올 가치가 있는 요소:** 작은 코어, near-black 면적, 선택적인 magenta 방전과 대비. Production 적용 결정은 보류.

## B — Broken Turbulent Sphere

**실험 목표:** A의 공통 색·코어·명목 크기를 유지하면서 실제 외곽 변화만으로 다른 후보로 읽히게 하기.

**실제 화면 평가:** 같은 정면/측면/낮은 시점에서 A의 원형과 B의 굴곡진 외곽이 즉시 구별된다. 0–3초 확대 프레임에서 위쪽 돌기와 왼쪽 경계가 서서히 형태를 바꾼다. 작은 조각을 무작위로 뿌린 노이즈보다 연결된 질량이 먼저 보인다. 중앙 코어와 보라색 덩어리는 유지된다.

**최소 보정:** 이전 렌더의 고주파 angular displacement가 빽빽한 가시/뿔처럼 보였다. 외곽 패턴의 주파수를 낮추고 추가 돌출 항을 줄여 굵은 굴곡과 얕은 침식을 남겼다. 몸체 밖에 별도 smoke나 particle cloud를 추가하지 않았다.

**장점:** 색을 달리하지 않아도 형태만으로 A와 구별된다. 어두운 질량의 실루엣을 살리는 후보로 가장 직접적이다. 이전 가시형 버전보다 구형 덩어리의 응집성이 좋다.

**단점·한계:** 굴곡이 둥글고 이어져 있어 단단한 암석 또는 유기적인 덩어리로 읽힐 수 있다. 미세한 난류보다는 굵은 변형이 우세하다. 난류 방향이 뚜렷한 C와 같은 내부 깊이 실험은 아니다.

**레퍼런스 접근도:** 불규칙하고 안정되지 않은 경계라는 목표는 A보다 가깝다. 다만 레퍼런스의 날카롭게 찢어지는 에너지 경계를 완전히 재현한 것은 아니다. 변위 크기 전체를 그대로 가져갈지, 외곽 굴곡 언어만 가져갈지 선택이 필요하다.

**예상 비용:** 본체 1 draw, 최대 128 sample. A와 같은 샘플 상한이지만 더 넓은 실루엣과 경계 계산/가시 구간 때문에 실제 GPU 부담은 A와 비슷하거나 더 높을 수 있다. 측정된 배수는 없다.

**가져올 가치가 있는 요소:** 연결된 불규칙 외곽, 부분적으로 강조되는 경계, 구형 실루엣을 완전히 무너뜨리지 않는 변위. Production 적용 결정은 보류.

## C — Deep Implosion Mass

**실험 목표:** 안쪽 흡수 질량, 중간 수렴 에너지, 표면 난류, 작은 바깥 파열이 독립적으로 읽히는지 확인.

**실제 화면 평가:** A/B보다 내부 깊이 차이는 명확하다. 어두운 안쪽 면과 보라색 띠, 앞쪽 붉은 조각의 가림 관계가 보이며 측면/동일 Orbit에서 상대적인 겹침이 바뀐다. 같은 구체를 단순히 밝기만 달리 겹친 인상은 줄었다. 그러나 규칙적인 나선 띠와 바깥의 어두운 껍질 인상은 남아 있다.

**3초 움직임 판정:** 내부 띠의 말려드는 이동, 그와 다른 표면 결, 바깥쪽의 짧은 파열은 서로 다르게 변한다. 층별 발광을 분리한 뒤에는 바깥층의 밝기 변화가 내부 전체를 덮거나 모든 외곽 조각이 동시에 꺼지는 현상이 줄었다. **순수한 radial inward implosion의 가독성은 부분 충족이다.** 실제 화면에서는 회전하며 말려드는 동작이 더 강하고, 수렴·접선·외향 3개 벡터를 누구나 즉시 구분한다고 단정할 수 없다. Optical-flow 보조 계산도 단일한 화면상 내향 흐름을 입증하지 못했다. 코드에 세 속도가 있다는 이유로 이 항목을 통과 처리하지 않았다.

**최소 보정:** 내부의 넓은 magenta 면을 줄여 violet 띠와 암부를 드러냈고, 중간층 밀도에 공간을 만들었다. 기존 네 density 항을 유지하면서 각 층의 밀도로 발광을 가중해 다른 층을 물들이는 혼합을 수정했다. 바깥 파열의 위상을 공간적으로 나누고 내부 띠의 수렴 변화를 강화했다. 공통 코어·팔레트·지름과 기존 architecture는 유지했다.

**장점:** 가장 큰 입체감과 내부 움직임의 차이. 겉과 속이 다른 속도로 살아 움직이는 방향을 비교하기에 좋다. A의 암부에 제한적으로 결합할 내부 구조의 후보를 제공한다.

**단점·한계:** A/B보다 속이 비어 있고 가벼운 에너지 껍질로 보일 수 있다. 내부 띠가 정돈된 회전 형태로 읽히며 ‘압축된 위험한 질량’이라는 목표에서는 A보다 멀어지는 순간이 있다. 완성형 implosion으로 승인할 수준이라는 결론은 내리지 않는다.

**레퍼런스 접근도:** 층의 깊이와 서로 다른 움직임은 가장 가까운 실험이다. 질량의 어두운 무게감과 비정형 저주 에너지 형상은 부분 접근이다. 레퍼런스의 폭발/빔/주변 환경 효과를 본체 깊이 대신 추가하지 않았다.

**예상 비용:** 본체 1 draw, 최대 160 sample. A/B보다 sample 상한이 25% 높으며 여러 밀도층, 회전 좌표, 삼각함수·나선 계산과 더 늦은 opacity 조기 종료 때문에 가장 비쌀 것으로 예상한다. 25%는 샘플 수 차이이며 GPU 시간 차이가 25%라는 뜻이 아니다.

**가져올 가치가 있는 요소:** 앞뒤 가림, 내부 띠와 표면의 분리, 국소적인 바깥 파열. C의 전체 외형 또는 투명도를 그대로 가져갈지는 별도 선택이 필요하다.

## 화면 차이와 비용 해석

| 질문 | 판정 |
|---|---|
| A의 작은 코어·near-black 면이 읽히는가? | 충족. 후처리 OFF에서도 유지 |
| B의 외곽이 A와 즉시 구분되는가? | 충족. 대신 굵은 암석/유기적 굴곡 인상이라는 한계 있음 |
| C의 내부 depth가 A/B와 구분되는가? | 충족. 시점과 시간에 따라 가림이 달라짐 |
| C의 세 운동 방향이 각각 즉시 읽히는가? | 부분 충족. 말려드는 회전이 우세하며 순수 수렴은 약함 |
| 세 후보가 너무 비슷해 선택하기 어려운가? | 현재는 아님. 형태·층·무게감 차이가 같은 조건에서 드러남 |

참고 수치: 최종 정면의 공통 원형 구간(중심 반경 32–145px, 코어 제외)에서 표시 RGB 휘도 0.08 미만인 비율은 A 약 99.5%, B 약 98.8%, C 약 57.1%다. 이는 어두운 화면 면적의 보조 지표이며 물리적 질량/밀도 또는 품질 점수는 아니다. C가 상대적으로 가볍게 보인다는 화면 평가와 일치한다.

세 후보 모두 proxy cube 1개로 밀도장을 렌더한다. ParticleSystem / LineRenderer / gameplay collider를 사용하지 않는다. 비교 창에는 3개의 body material이 존재하지만 후보 하나만 활성화된다. 공통 render target의 색 버퍼는 약 1.98MiB이며 depth·URP 중간 버퍼·후처리는 추가다. 별도 카메라로 비교 화면 전체를 렌더하는 비용은 본체 비용과 구분해야 한다. 이번 pass는 정밀 GPU profiler나 여러 동시 시전/저사양 하드웨어 예산 검증을 수행하지 않았다.

## 검증과 보호

최종 prototype 검증 **3/3 PASS**와 기존 Purple·공유 영역 regression **36/36 PASS**, 합계 관련 검사 **39/39 PASS**. 전체 프로젝트 모든 테스트를 실행했다는 뜻은 아니다. C# 및 shader compile error 0.

- [최종 prototype 테스트](../unity/Logs/AstraCombatQA/purple-body-exploration-preserved-final.xml): 공통 profile 한글 Inspector, production reference 부재, A/B/C 전환 시 동일 시각·카메라·위치, 사용자 저장 Scene의 prototype 위치/자식, 기존 카메라 설정, 외부 material 참조와 소유 material 정리, 3회 열기/종료, 실제 렌더, 실제 Update로 3초 진행.
- [기존 영역 regression](../unity/Logs/AstraCombatQA/purple-body-protected-regression.xml): 실제 Purple 시전/피해 경로, 첫 적중 종료, 사거리 종점, 취소/카메라/자원 복구, scar, 공유 Data/Inspector, beat clock.
- 이전 3차의 37개 검증에서 기존 production 캡처를 다시 쓰는 render-review 1개를 제외한 36개를 재실행했다. 3차 기준 자료는 보존했다. 이후 C 전용 보정은 새 exploration shader 분기에만 있으며 마지막 prototype 검사로 다시 검증했다.
- 정면/측면/낮은 시점 실제 PNG와 0–3초 frame sequence를 직접 검토했다. 두 triptych MP4 모두 **91/91 프레임 디코딩, 30fps** 확인. 원본 Editor의 실시간 Play Mode를 사용자 대신 시각 승인한 것은 아니다.
- 시작 snapshot 512개는 보고서 문서 편집 직전 모두 동일했다. 최종 감사에서도 의도한 handoff/current task 2개 이외의 **510개**, 3차 frozen baseline **49개**가 모두 같은 해시였다. 새 실행 폴더에서 재렌더한 558개 PNG도 검토된 `Frames_final5`와 전부 동일했다. C만 추가 보정한 뒤 A/B 원본 프레임은 변하지 않았다. [보호·Git 최종 감사](../unity/Logs/PurpleBodyExplorationQA/final-audit.json)
- Production Purple / 3차 baseline / gameplay / `Purple_UserPrototype`이 저장된 Scene / Package / ProjectSettings / 기존 Shader Graph·material·VFX 파일을 수정하지 않았다. 원본 Unity Editor의 미저장 작업을 제어하거나 저장하지 않았다. 테스트는 기존 독립 QA 복사본에서 수행했다.
- `git diff --check`는 기존 `VFXLab.unity`의 빈 YAML 값 뒤 공백 8개를 보고한다. 시작 snapshot과 동일한 사용자 Scene이므로 정리하지 않았다. Exploration C#/shader에는 trailing whitespace가 없다. Unity가 생성한 profile `.meta`의 빈 YAML 값 뒤 공백 3개는 별도로 감사 기록에 남겼다.

## 파일과 Git 상태

기존에 생성된 exploration 파일을 이어서 사용했다. 재개 후 실제 본체 보정은 exploration shader 안에서 수행했고, test에는 후처리 OFF 캡처와 결과별 보관을 추가했다.

| 파일 | 역할 |
|---|---|
| `unity/Assets/Resources/VFX/HollowPurpleBodyExploration.shader` | 공통 density renderer와 A/B/C 분기, B/C 보정, 비교용 sampling 품질 |
| `unity/Assets/Resources/VFX/PurpleBodyExplorationProfile.asset` | 생성된 A/B/C 공통 설정 유지 |
| `unity/Assets/Scripts/Dev/VFXLab/PurpleBodyExplorationProfile.cs` | 별도 공통 데이터 타입 |
| `unity/Assets/Scripts/Dev/VFXLab/PurpleBodyPrototype.cs` | 새 비교 본체·material 소유 및 정리 |
| `unity/Assets/Scripts/Dev/VFXLab/PurpleBodyExplorationBench.cs` | 별도 카메라·render target·동일 시각/시점 제어 |
| `unity/Assets/Editor/PurpleBodyExplorationWindow.cs` | 한글 비교 창 및 scoped 한글 profile Inspector |
| `unity/Assets/Editor/PurpleBodyExplorationTests.cs` | 3개 focused test와 실행별 캡처 보관 |
| 위 7개 `.meta` | 독립 Unity asset 식별자 |
| `docs/PURPLE_BODY_EXPLORATION_REVIEW.md` | 이번 비교 보고서 |
| `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md` | production 동결 유지와 prototype 선택 대기 기록 |

별도의 QA용 스크립트·PNG·MP4·HTML·감사 파일은 `unity/Logs/PurpleBodyExplorationQA/`에 보관했다.

현재 branch는 `feat/gojo-blue-screen-distortion`, LOCAL HEAD는 `a9c6ccf9c09b2effdd559e83e865296e45f600b2`. Index는 비어 있다. 최종 상태는 tracked modified 20개, untracked 56개이며, 이 중 상당수는 이전 작업부터 존재한 변경이다. 정확한 전체 목록은 [final-git-status.txt](../unity/Logs/PurpleBodyExplorationQA/final-git-status.txt) 참조. reset / clean / restore / staging / commit / push / merge / fetch는 수행하지 않았다. Remote를 새로 확인하거나 갱신하지 않았다.

## 사용자 선택 대기

권장 출발점은 **A의 암부·코어 대비 + B의 외곽 언어**다. C에서는 필요할 때 내부 가림/흐름 분리만 가져오는 선택이 가능하다. 이것은 적용 결과가 아니라 비교 후 제안이다.

사용자가 가져갈 요소와 강도를 고르기 전에는 production 작업을 시작하지 않는다. C 전체를 선택하는 경우에는 회전보다 수렴이 먼저 읽히게 만드는 다음 보정과 무게감 회복이 필요하다는 한계를 함께 고려한다. 세 후보 중 하나를 통째로 선택할 의무는 없다.
