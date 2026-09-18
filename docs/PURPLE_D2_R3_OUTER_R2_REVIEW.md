# D2-R3-OuterR2 — Outer-only refinement

2026-09-16 · **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**

현재 working tree의 중단 지점에서 이어서 완료했다. **Production 미적용. R3 본체와 OuterR1은 동결**했다. 이미 완료된 Halo를 다시 수정하지 않았고, Hero Fragment의 실제 발생 구간 확인 → 최소 크기/잔광 보정 → 외향 번개 → 국소 왜곡 → 최종 비교 순서로 진행했다.

## 비교 결과물

- [전체 비교 페이지](../unity/Logs/PurpleOuterR2QA/comparison.html)
- [R3 / OuterR1 / OuterR2 — Front·Side·SlightLow](../unity/Logs/PurpleOuterR2QA/R3_OuterR1_OuterR2_stills.jpg), [정면 비교](../unity/Logs/PurpleOuterR2QA/R3_OuterR1_OuterR2_Front.jpg)
- **3초 비교 영상:** [Front](../unity/Logs/PurpleOuterR2QA/R3_OuterR1_OuterR2_Front_3s.mp4), [Orbit](../unity/Logs/PurpleOuterR2QA/R3_OuterR1_OuterR2_Orbit_3s.mp4)
- **OuterR2 단독:** [Front](../unity/Logs/PurpleOuterR2QA/OuterR2_Front_3s.mp4), [Orbit](../unity/Logs/PurpleOuterR2QA/OuterR2_Orbit_3s.mp4)
- 원본 still: [Front](../unity/Logs/PurpleOuterR2QA/Motion/20260916_084939_260/OuterR2_Front.png), [Side](../unity/Logs/PurpleOuterR2QA/Motion/20260916_084939_260/OuterR2_Side.png), [SlightLow](../unity/Logs/PurpleOuterR2QA/Motion/20260916_084939_260/OuterR2_SlightLow.png)
- [밝은 배경 비교](../unity/Logs/PurpleOuterR2QA/Bright_comparison.jpg), [밝은 배경 Orbit](../unity/Logs/PurpleOuterR2QA/Previews/Distortion_20260916_084815_749/Bright_Orbit.png)
- **Hero Fragment 실제 출현:** [밝은 배경 2.4~3.0초](../unity/Logs/PurpleOuterR2QA/HeroFragment_bright_review.mp4), [2.933초 peak](../unity/Logs/PurpleOuterR2QA/HeroFragment_bright_peak.jpg), [연속 프레임](../unity/Logs/PurpleOuterR2QA/HeroFragment_bright_sequence.jpg)
- [OuterR1 ↔ OuterR2 번개 연속 프레임](../unity/Logs/PurpleOuterR2QA/OuterR1_vs_OuterR2_arc_sequence.jpg), [밝은 배경 왜곡 On/Off 확대](../unity/Logs/PurpleOuterR2QA/Distortion_bright_detail.jpg)

최종 원본 폴더: `unity/Logs/PurpleOuterR2QA/Motion/20260916_084939_260`. 960×540, 30fps, t=0…3초 양 끝 포함 91프레임. 비교 영상은 1920×396이며 세 후보에 같은 시각·지름·카메라·배경·후처리를 사용한다. R3와 OuterR1의 기존 최종 RGB24를 재사용했다. 기존 비교 파일은 덮어쓰지 않았다.

밝은 배경 Hero 전용 짧은 영상은 파편 판별을 위해 **Debris 단계까지만 켠 보조 자료**다. 최종 Front/Orbit 영상은 Distortion까지 모두 켠 결과다. 1.5초 still에는 Hero가 없을 수 있으므로 파편 판정은 영상의 발생 구간을 기준으로 한다.

실행: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple D2-R3-OuterR2 비교`. R3 / OuterR1 / OuterR2 전환, 공통 시각 스크럽, Front / Side / SlightLow / Orbit을 제공한다. 전용 Profile은 한국어 Inspector를 사용한다.

## 중단 지점 확인과 최종 구현

| 항목 | 확인 및 변경 | 실제 화면 판단 |
|---|---|---|
| Hero Fragment | **마지막 구현은 이미 적용되어 있었다.** 3개 Hero slot, 흰 중심+핑크 shard+2개 짧은 잔광층이 존재했다. 실제 카메라 투영에서 보이는 발생 시각을 찾아 2.4~3.0초를 확인했다. 크기 2→2.6, 폭/두께 소폭 증가, 잔광 시차 0.026/0.052→0.055/0.110초, 감쇠만 보정했다. 수량·주기·작은 spark 수는 유지했다. | 약 2.87~3.0초 오른쪽에서 일반 spark보다 큰 흰/핑크 파편이 바깥으로 이동한다. 짧은 핑크 잔광이 뒤따른다. 둥근 먼지가 아니라 길쭉한 shard로 읽힌다. 밝은 배경에서도 일부는 보이지만 잔광은 섬세하며, 축소 영상에서는 작은 발광 바늘처럼 보일 수 있다. |
| Halo | 중단 전에 완료된 불균일한 체적 aura를 그대로 보존했다. 방향별 두께·밝기·끊김, 국소 파열을 가진 1.55배 규모의 shell이다. 재개 이후 shader hash와 Halo 설정 유지. | OuterR1의 연속된 매끈한 bubble 경계가 크게 줄었다. 세 방향과 Orbit에서 큰 aura가 남고 코어가 가장 밝다. 대신 부드럽고 구름 같은 연무 인상이 늘었다. |
| Outward Lightning | 기본 Particle System 유지. 소수 2개 event lane과 약 0.091~0.169초 수명 유지. 굵은 시작부→가는 끝, 좁힌 fringe, 밝기 flicker, 사건별 도달 거리·분기 시작점·길이·방향 변화, 더 큰 국소 꺾임을 추가했다. R3 내부 Arc는 수정하지 않았다. | 1.467초 부근 발생, 1.50~1.567초 형태/밝기 변화, 1.60초 약화, 1.633초 소멸을 확인했다. OuterR1의 일정 두께 네온 막대보다 짧은 방전으로 읽힌다. 여전히 한 프레임 확대에서는 꺾인 선분과 계단 현상이 남는다. |
| Distortion | 마지막 단계에서만 조정. 범위 1.36→1.50배, 강도 0.0035→0.006, 공간별 강도 변화와 약한 불규칙 pulse를 추가했다. Opaque Texture 기반이며 본체보다 먼저 렌더하고 본체 투영 영역을 제외한다. | 밝은 격자에서 본체 주변 선이 국소적으로 휘며 Front/Orbit 모두 읽힌다. 회색 테스트 바닥에서는 여전히 미묘하다. 본체 중심부를 흐리지 않고 화면 전체도 흔들지 않는다. |

## 실제 화면 비교와 acceptance

Still 세 방향, Front/Orbit 시간별 프레임, 번개의 연속 프레임과 Hero 발생 구간을 직접 비교했다. 이는 **CODEX PREVIEW**이며 사용자 Play Mode 최종 승인을 대체하지 않는다.

1. **R3 본체 untouched:** shader/core/surface/color/silhouette/animation/internal arc 원본을 수정하지 않았다. 같은 frozen R3 runtime/profile을 참조한다. 최종 정면 1.5초에서 중심 지름 240px 영역의 Outer on/off RGB 차이는 평균·최대 모두 **0**이다. 모든 각도·모든 silhouette pixel의 동일성을 주장하는 수치는 아니다.
2. **Halo 보호막 느낌 감소:** OuterR1과 차이가 명확하다. 완전한 원형 경계 대신 넓이가 다른 끊어진 밝은 구역으로 읽힌다.
3. **큰 Aura / 공간감:** Front·Side·SlightLow에서 존재하며 Orbit에서도 판처럼 사라지지 않는다. 본체 밖의 규모감과 배경 연결이 유지된다.
4. **Hero / debris:** 간헐적인 큰 shard가 작은 spark와 구분된다. 파편 수를 늘려 가독성을 해결하지 않았고, 본체를 가리는 debris storm도 아니다. 1.5초 한 장만으로는 이 개선을 평가하기 어렵다.
5. **Lightning:** 짧은 출현·깜박임·소멸과 가늘어지는 끝이 개선됐다. 고정 나뭇가지처럼 오래 남지 않는다. 모든 정지 프레임에서 neon 인상이 완전히 제거된 것은 아니다.
6. **Distortion:** 평평한 기본 배경에서는 1/255를 넘는 차이 **512px**, 최대 채널 차이 **39/255**. 밝은 격자에서는 **10,725px**, 최대 **90/255**. 두 경우 모두 중심부 차이 0, 중심에서 300px 바깥 차이 0. 배경의 구조가 있어야 굴절을 볼 수 있다는 한계가 있다. 차이 이미지는 진단용 8배 증폭이며 실제 강도를 뜻하지 않는다.
7. **밝은 배경:** Halo·일부 spark/Hero와 외부 번개가 남는다. 핑크 Halo와 얇은 trail의 대비는 어두운 배경보다 낮다. 흰색 단색 배경 전체에서 동일하게 잘 보인다는 의미는 아니다.
8. **전체 폭주감:** OuterR1보다 containment가 깨져 에너지가 새는 인상이 강하다. 내부 본체 재작업 없이 외부 aura·shard·방전·주변 굴절의 차이를 만들었다.

## 새로 생긴 trade-off와 남은 부족점 Top 3

1. **Halo의 연무 인상.** 보호막 경계는 줄었지만 더 부드럽고 구름처럼 보인다. 레퍼런스의 날카로운 압력 파열까지 도달한 것은 아니다. 이번 재개에서 완료된 Halo를 다시 손대지는 않았다.
2. **번개 끝의 aliasing / 순간 대비 감소.** Taper와 flicker로 균일한 막대 인상은 줄었으나 가는 끝이 일부 프레임에서 점선·계단처럼 보인다. 일부 event는 OuterR1보다 짧고 약하게 보이며 정지 프레임에서의 직선 조각도 남는다.
3. **파편·왜곡의 배경/거리 의존성.** Hero는 확실히 커졌지만 잔광은 본체와 밝은 Halo 옆에서 묻힐 수 있다. 회색 평면에서는 왜곡이 미묘하고, 강한 격자에서는 두 겹 경계가 약하게 보인다. 실제 도시 배경·전투 거리 검증은 하지 않았다.

## Production 가치 / 다음 결정

**반영을 검토할 가치는 있다.** OuterR1보다 보호막·균일한 번개 문제를 줄였고, 소수 Hero shard와 국소 굴절로 환경 장악력이 더 잘 읽힌다. 다만 현 후보를 그대로 production 완성본으로 승인하지 않는다. 실제 전투 거리에서 위 Top 3와 비용을 판단한 뒤 채택 범위를 정하는 것이 맞다.

**추가 refinement는 조건부 권장**이다. 사용자 영상 검토 후 남은 aliasing·거리 가독성이 거슬릴 때 Outer만 대상으로 좁히는 것이 적절하다. 이번 범위에서 새 방향이나 추가 polish를 이어가지 않는다. **Production 적용 없음. 사용자 visual approval 대기.**

## 최소 validation / 보호 / 비용

- 최종 preview [purple-outer2-final-preview.xml](../unity/Logs/AstraCombatQA/purple-outer2-final-preview.xml) **1/1 PASS**: C# compile, shader 지원/오류, preview spawn, 후보 전환, cleanup, 사용자 prototype와 기존 camera 보존.
- 최종 render [purple-outer2-final-motion.xml](../unity/Logs/AstraCombatQA/purple-outer2-final-motion.xml) **1/1 PASS**. 최종 합계 **2/2 PASS**, C# compile error **0**, shader error **0**.
- 재개 중 Hero motion 2회, Lightning preview 1회는 각각 PASS. Halo 단독 검증을 다시 실행하지 않았다. Full regression 없음.
- 최종 MP4 4개 모두 91프레임/30fps decode 성공. Hero 보조 영상은 19프레임/30fps. PNG는 중단 전 자료를 포함하여 **43장**, 최종 motion 폴더의 still은 **3장**. 수백 장 PNG 대신 raw RGB24 stream을 사용했다.
- 보호 snapshot **795개** 중 handoff/task 문서 2개만 갱신, **793개 동일**. R3 source/meta **16개 동일**, OuterR1의 18개 자산/source/meta와 기존 결과 보존, frozen 3차 baseline **49/49 동일**. Production/R2/R1/D2/D/A/B/C, 사용자 prototype/Scene, gameplay, Package/ProjectSettings 보존.
- 재개 시점 Halo shader SHA-256 동일. 보호 검증: [final-audit.json](../unity/Logs/PurpleOuterR2QA/final-audit.json), [픽셀·왜곡·Halo 검증](../unity/Logs/PurpleOuterR2QA/visual-metrics.json).
- 추가 renderer 4개: Halo / Debris PS / Lightning PS / Distortion. 작은 파편 최대 24 slot + Hero 3 slot×4층, 두 arc 동시 최대 92개 선분 입자. Halo는 80-step 체적 sampling이므로 OuterR1보다 shader 비용 증가 가능성이 크다. 실제 GPU frame-time benchmark는 수행하지 않았다.
- HEAD 유지, index 비어 있음. commit/push/merge/reset/clean/restore 없음.

## 수정 파일 / Git

OuterR2 전용 **9개 + metadata 9개**:

- [Profile](../unity/Assets/Scripts/Dev/VFXLab/PurpleOuterR2Profile.cs), [Runtime](../unity/Assets/Scripts/Dev/VFXLab/PurpleOuterR2Runtime.cs), [Bench](../unity/Assets/Scripts/Dev/VFXLab/PurpleOuterR2Bench.cs)
- [비교 Window / 한국어 Inspector](../unity/Assets/Editor/PurpleOuterR2Window.cs), [targeted tests](../unity/Assets/Editor/PurpleOuterR2Tests.cs)
- [Halo shader](../unity/Assets/Resources/VFX/PurpleOuterR2Halo.shader), [Particle shader](../unity/Assets/Resources/VFX/PurpleOuterR2Particle.shader), [Distortion shader](../unity/Assets/Resources/VFX/PurpleOuterR2Distortion.shader), [Profile asset](../unity/Assets/Resources/VFX/PurpleOuterR2Profile.asset)

재개 이후 구현 변경은 Runtime·Profile·Profile asset·Distortion shader·Tests·Window에 한정했다. Halo shader·Particle shader·Bench는 중단 당시 구현을 유지했다. 문서는 이 보고서와 `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md`를 정리했다. QA 자료·보조 script는 Git 제외 경로인 `unity/Logs/PurpleOuterR2QA/`에 있다.

최종 `git status -sb`: **feat/gojo-blue-screen-distortion**, tracked modified **20**, untracked **176**. 기존 dirty 작업을 포함한 숫자다. [전체 status](../unity/Logs/PurpleOuterR2QA/final-git-status.txt). 기존 production/Scene/Settings의 dirty 표시는 이번 수정으로 생긴 것이 아니며 시작 시점 hash를 보존했다.
