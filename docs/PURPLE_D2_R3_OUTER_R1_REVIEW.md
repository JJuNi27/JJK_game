# D2-R3-OuterR1 — 외부 환경 효과 exploration

2026-09-16 · **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**

사용자가 목표 방향에 도달했다고 판단한 D2-R3 본체를 동결하고, 외부 효과만 추가했다. **Production 미적용.** 본체 shader·core·surface·기본 silhouette·기존 body arc는 수정하지 않았다. 별도 bench가 기존 R3 runtime과 profile을 그대로 참조하며 Outer 동반 오브젝트만 켜고 끈다.

## 결과물

- [비교 페이지](../unity/Logs/PurpleOuterR1QA/comparison.html)
- [R3 단독 ↔ OuterR1 정면](../unity/Logs/PurpleOuterR1QA/R3_vs_OuterR1_Front.jpg), [Front / Side / SlightLow 시트](../unity/Logs/PurpleOuterR1QA/R3_vs_OuterR1_stills.jpg)
- [Front 3초 비교](../unity/Logs/PurpleOuterR1QA/R3_vs_OuterR1_Front_3s.mp4), [Orbit 3초 비교](../unity/Logs/PurpleOuterR1QA/R3_vs_OuterR1_Orbit_3s.mp4)
- 단독 영상: [OuterR1 Front](../unity/Logs/PurpleOuterR1QA/OuterR1_Front_3s.mp4), [OuterR1 Orbit](../unity/Logs/PurpleOuterR1QA/OuterR1_Orbit_3s.mp4)
- 원본 still: [Front](../unity/Logs/PurpleOuterR1QA/Motion/20260916_045525_391/OuterR1_Front.png), [Side](../unity/Logs/PurpleOuterR1QA/Motion/20260916_045525_391/OuterR1_Side.png), [SlightLow](../unity/Logs/PurpleOuterR1QA/Motion/20260916_045525_391/OuterR1_SlightLow.png)
- 시퀀스 확인: [Front](../unity/Logs/PurpleOuterR1QA/OuterR1_Front_motion_samples.jpg), [Orbit](../unity/Logs/PurpleOuterR1QA/OuterR1_Orbit_motion_samples.jpg), [외향 방전 연속 프레임](../unity/Logs/PurpleOuterR1QA/OuterR1_arc_event_sequence.jpg)
- [Distortion On/Off 확대 비교](../unity/Logs/PurpleOuterR1QA/Distortion_background_detail.jpg): 오른쪽 차이 이미지만 진단용으로 16배 증폭했다. 실제 효과 강도가 아니다.

최종 원본: `unity/Logs/PurpleOuterR1QA/Motion/20260916_045525_391`. 960×540, 30fps, t=0…3초를 양 끝 포함 91프레임으로 샘플링했다. 원본 RGB24와 format.json을 보존했다. 비교의 R3 단독은 기존 `PurpleD2R3QA/Motion/20260915_205220_113` 최종 렌더를 재사용했다. 지름·카메라·시각·배경·후처리 조건은 동일하다.

Unity: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple D2-R3-OuterR1 비교`. R3 본체 단독과 OuterR1 토글, 공통 시각 스크럽, 4개 시점 제공. OuterR1 Profile의 한국어 Inspector에서 누적 단계와 외부 효과 값만 조절할 수 있다.

## 요청된 순서대로 구현·확인

| 단계 | 구현 | 화면에서 확인한 내용 |
|---|---|---|
| 1. Halo | 본체 지름의 1.55배를 중심으로 하는 구형 발광 shell. 정면 billboard 없음. 공간 좌표별 밝기 변화와 얇은 rim·넓은 약한 aura를 합성. | 정면과 측면 모두 큰 아우라로 읽힌다. 첫 버전의 동심원 샘플링 줄무늬는 analytic shell 계산으로 제거했다. |
| 2. Debris / Sparks | 기본 Particle System의 작은 뾰족한 입체 mesh 입자. 최대 24개 slot, 수명 약 0.28~0.55초. radial outward 위치에 접선 방향 난류를 더했다. | 둥근 먼지가 아닌 짧은 흰색/핑크 선형 spark. 본체를 가리지 않지만 축소 화면에서는 약하다. |
| 3. Outward Lightning | 기본 Particle System 1개가 짧은 선분 입자들을 조합한다. 독립 burst 2개 lane, 흰 중심과 magenta fringe, 분기. root는 표면 안쪽 0.93R, 끝은 약 1.72~2.03R. | 외곽에서 허공으로 뻗는다. 첫 버전보다 두께·도달 범위를 높여 외부 존재감을 확보했다. 사건마다 위치·길이·방향·분기 변화. |
| 4. Distortion | 본체보다 1.36배 큰 field. URP Opaque Texture를 약하게 굴절. Body/Halo보다 먼저 렌더하고 본체 투영 영역은 제외한다. | 바닥/배경 경계에서 미세한 흔들림. 본체를 흐리게 하지 않으며, 평평한 배경에서는 존재감이 작다. |

Lightning 수명은 기본 0.13초의 변주로 **약 0.091~0.169초**다. 긴 번개 나뭇가지 오브젝트를 회전시키는 방식이 아니다. 기본 Particle System이 입체 선분을 렌더하고, preview clock에 맞춰 짧게 살아 있는 입자 packet을 생성한다. 스크럽·동일 조건 비교를 위해 위치/난류/event는 결정적으로 계산한다. 새 VFX Graph·package는 사용하지 않았다. Shader Graph asset을 새로 만들지는 않았으며, 기존 프로젝트 방식의 URP custom shader를 사용했다.

## 실제 화면 기준 평가

세 방향 still, Front/Orbit 시간별 프레임, 1.400~1.767초 연속 프레임을 직접 비교했다. 최종 시각 승인은 사용자에게 남겨둔다.

| 기준 | 판단 |
|---|---|
| R3 본체 유지 | **유지.** bright mass, chunked surface, white-out core는 같은 R3 runtime이다. 최종 1.5초 정면에서 본체 중심 지름 240px 영역은 Outer on/off의 RGB 차이가 0이었다. 전체 모든 각도·모든 silhouette pixel의 동일성을 뜻하는 것은 아니다. |
| 외부 규모감 | **명확히 증가.** Halo가 배경과 구체 사이에 큰 에너지 영역을 만들고, 간헐적 외향 방전이 그 영역 밖까지 뻗는다. |
| Front / Orbit | **모두 읽힘.** Halo는 측면에서 얇은 판으로 사라지지 않는다. Orbit에서도 공간상 밝은 구역과 외부 아크의 방향이 바뀐다. |
| Halo의 위계 | 본체 코어가 가장 밝다. 외부 aura는 이를 둘러싸는 보조 역할이다. 다만 매끈한 구형 rim이 간혹 보호막/비눗방울처럼 보인다. |
| Lightning의 사건성 | 1.40~1.47초에는 큰 외향 번개가 없다가 1.50초 부근에 오른쪽 방전이 생기고, 1.60초 부근에는 다른 방향의 방전이 겹친 뒤 1.70초에는 사라진다. 고정 가지 인상은 줄었지만 한 프레임에서는 각진 네온 선분처럼 보일 수 있다. |
| 파편 | 둥근 먼지 인상 없음. 짧고 가는 spark로 보인다. 현재 값은 본체 보호에 유리한 대신 멀리서 읽히는 파편 질량감은 약하다. |
| Distortion | 최종 On/Off 비교에서 1/255보다 큰 차이를 보인 pixel 215개, 최대 채널 차이 25/255. 본체 중심부 차이는 0. 이 수치는 현재 평평한 테스트 배경에서의 기능 확인이며 복잡한 도시 배경의 품질 검증은 아니다. |

## 최종 질문에 대한 판단

**1. Production 반영 가치가 있는가?**

있다. 본체를 다시 authoring하지 않고도 스케일감·외향 에너지·배경 연결감을 확실히 더한다. 특히 입체 Halo와 짧은 외향 방전의 방향은 선택적으로 가져갈 가치가 있다. 다만 현 후보를 그대로 production 완성본으로 승인하지는 않는다. 이번 작업에서는 이식하지 않았다.

**2. 남은 부족점 Top 3**

1. **Halo의 매끈한 보호막 인상.** 크기와 가독성은 좋지만 레퍼런스의 거친 압력 파열보다 안정된 shell에 가깝다. 외곽의 국소 끊김·밝기 피크를 다듬을 여지가 있다.
2. **외향 번개의 선분/네온 인상.** motion에서는 짧은 event로 읽히지만 일부 정지 프레임은 두께가 일정한 꺾인 선처럼 보인다. 흰 배경·깊이 방향으로 뻗는 burst는 별도 가독성 확인이 필요하다.
3. **작은 효과의 배경·거리 의존성.** 파편은 작고, distortion은 평평한 배경에서 매우 미묘하다. 실제 전투 거리와 도시형 배경에서 둘의 가독성/왜곡 강도를 함께 판단해야 한다.

**3. 다음 refinement가 필요한가?**

Production 이식 전 **Outer 전용 refinement 1회**를 권한다. 위 세 항목에 한정하며 D2-R3 본체는 계속 동결하는 것이 맞다. 사용자 시각 확인 전에는 추가 구현·이식을 진행하지 않는다.

## 검증·비용·보호

- 단계별 최소 프리뷰: Halo 2회(줄무늬 수정 포함), Debris 1회, Lightning 2회(가독성 보정 포함). 각 생성/전환/정리 PASS.
- 최종 [purple-outer-final-01.xml](../unity/Logs/AstraCombatQA/purple-outer-final-01.xml): **2/2 PASS**, C# compile / shader error **0**. Preview spawn/cleanup, 두 Particle System 존재·활성 입자, 기존 사용자 object와 camera 보존 확인. Full regression 없음.
- MP4 4개 모두 91프레임 / 30fps decode 성공. [미디어 검증](../unity/Logs/PurpleOuterR1QA/media-review.json).
- PNG 전체 **32장**, 최종 motion 출력의 still **3장**. 영상은 raw stream으로 기록하여 수백 장 PNG capture를 만들지 않았다.
- 추가 renderer는 Halo / Debris PS / Lightning PS / Distortion 총 4개. Lightning은 두 event가 동시에 있을 때 최대 92개 선분 입자(흰 중심+fringe 포함), 파편 slot 24개. 실제 GPU frame time benchmark는 이번 범위에서 수행하지 않았다.
- 보호 snapshot **709개** 중 handoff/task 문서 2개만 갱신, **707개 동일**. R3 소스/metadata **16개 동일**, frozen 3차 baseline **49/49 동일**. Production/R2/R1/D2/D/A/B/C·기존 결과·사용자 prototype/Scene·gameplay·Package/ProjectSettings 보존.
- [보호·Git 검증](../unity/Logs/PurpleOuterR1QA/final-audit.json). HEAD 유지, index 비어 있음. commit/push/merge/reset/clean/restore 없음.

## 변경 파일

OuterR1 전용 9개 + 각 metadata 9개 추가:

- [Profile](../unity/Assets/Scripts/Dev/VFXLab/PurpleOuterR1Profile.cs), [Runtime](../unity/Assets/Scripts/Dev/VFXLab/PurpleOuterR1Runtime.cs), [Bench](../unity/Assets/Scripts/Dev/VFXLab/PurpleOuterR1Bench.cs)
- [비교 Window / 한국어 Inspector](../unity/Assets/Editor/PurpleOuterR1Window.cs), [최소 검증](../unity/Assets/Editor/PurpleOuterR1Tests.cs)
- [Halo shader](../unity/Assets/Resources/VFX/PurpleOuterHalo.shader), [Particle shader](../unity/Assets/Resources/VFX/PurpleOuterParticle.shader), [Distortion shader](../unity/Assets/Resources/VFX/PurpleOuterDistortion.shader), [Profile asset](../unity/Assets/Resources/VFX/PurpleOuterR1Profile.asset)

문서: 이 보고서 추가, `docs/CURRENT_HANDOFF.md`와 `docs/active/CURRENT_TASK.md` 최신 상태 추가. QA 결과는 로컬 `unity/Logs/PurpleOuterR1QA/`에 보존하며 Git에서 제외된다.

최종 `git status -sb`: **feat/gojo-blue-screen-distortion**, tracked modified **20**, untracked **157**. 기존 로컬 변경을 포함한 숫자다. [전체 상태](../unity/Logs/PurpleOuterR1QA/final-git-status.txt).
