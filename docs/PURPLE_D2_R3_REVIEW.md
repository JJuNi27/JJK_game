# Hollow Purple D2-R3 — Chunky Plasma / Fused Core / Dynamic Arc

2026-09-16 · **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**

별도 exploration 후보를 완료했다. **Production 적용 없음.** R2와 이전 후보는 동결 상태로 보존했다. 최종 재개 요청 이후에는 시각 구현을 더 수정하지 않았다. 중단 직전의 flow speed / localized rupture variation이 source와 QA 복사본에 적용된 것을 확인하고, 이미 완료된 최종 Unity 결과를 사용해 비교 자료와 보고서만 마무리했다.

## 최종 결과물

모든 비교는 동일 지름 5, 카메라/시각/배경/후처리 조건이다. R2는 보존된 `PurpleD2R2QA/Motion/20260915_201636_711`의 실제 렌더를 재사용했다. R2 재렌더·재authoring 없음.

- [최종 비교 페이지](../unity/Logs/PurpleD2R3QA/Final/comparison.html)
- [R2 ↔ R3 세 방향 비교](../unity/Logs/PurpleD2R3QA/Final/R2_vs_R3_stills.jpg), [정면 비교](../unity/Logs/PurpleD2R3QA/Final/R2_vs_R3_Front.jpg)
- [Front 3초 비교](../unity/Logs/PurpleD2R3QA/Final/R2_vs_R3_Front_3s.mp4), [Orbit 3초 비교](../unity/Logs/PurpleD2R3QA/Final/R2_vs_R3_Orbit_3s.mp4)
- R3 단독: [Front motion](../unity/Logs/PurpleD2R3QA/Final/R3_Front_3s.mp4), [Orbit motion](../unity/Logs/PurpleD2R3QA/Final/R3_Orbit_3s.mp4)
- 원본 still: [Front](../unity/Logs/PurpleD2R3QA/Motion/20260915_205220_113/R3_Front.png), [Side](../unity/Logs/PurpleD2R3QA/Motion/20260915_205220_113/R3_Side.png), [SlightLow](../unity/Logs/PurpleD2R3QA/Motion/20260915_205220_113/R3_SlightLow.png)
- 시퀀스 검토: [Front](../unity/Logs/PurpleD2R3QA/Final/R3_Front_motion_samples.jpg), [Orbit](../unity/Logs/PurpleD2R3QA/Final/R3_Orbit_motion_samples.jpg), [방전 연속 12프레임](../unity/Logs/PurpleD2R3QA/Final/R3_arc_event_sequence.jpg)

최종 원본 폴더: `unity/Logs/PurpleD2R3QA/Motion/20260915_205220_113`. 960×540, 30fps, t=0…3초를 양 끝 포함 91프레임으로 샘플링했다. Front/Orbit raw RGB24와 format.json을 보존했다. 마지막 flow 보정 이전 비교 자료도 QA 루트와 이전 Motion 폴더에 남겼으며, 최종 자료는 `Final/`로 분리했다.

Unity 실행: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple Hybrid D2-R3 비교`. R2/R3 전환, 공통 시각 스크럽, Front/Side/SlightLow/Orbit 제공. 한국어 Inspector 포함.

## 무엇을 바꿨는가

1. **Hairball 제거 / pattern scale:** R2의 강하게 늘어난 축별 고주파 필드를, 기본 빈도 11의 큰 필드 3개로 교체했다. 서로 다른 방향·속도·위상을 사용한다. 중간 크기 세부는 큰 파열 경계에만 제한했다. 가는 섬유를 전체에 덮는 방식은 제거했다.
2. **Hotspot fusion / core:** 여러 깊이의 emitter를 유지하되 보조 광원을 중심에 더 가깝게 배치했다. 변형된 soft kernel, 광원 사이 발광 통로, 넓은 낮은 강도의 halo를 겹쳤다. 보조 광원은 기본 밝기가 낮고 개별적으로 짧게 peak한다. 흡수에 따른 후방 magenta/violet 영향은 유지했다. 전역 Bloom·카메라 설정은 변경하지 않았다.
3. **Dynamic lightning / Hero Arc:** 재사용하는 3D tube mesh를 짧은 사건으로 구동한다. 기본 수명 0.105초에 seed별 변주를 적용해 약 0.079~0.147초 지속한다. 발생 간격·방향·길이·분기·두께가 달라지며, 세 Hero lane과 작은 보조 spark만 사용한다. 내부 광원에서 표면과 외부 공간으로 연결되고 후방 경로는 감쇠된다. 에너지 가산 합성으로 밝은 core 위의 삼각형 겹침 자국을 줄였다.
4. **Pulse / instability:** core와 보조 광원의 비동기 밝기 변화, 독립된 arc event, 표면 경계의 국소 압력 반응을 사용한다. 본체 전체 scale의 heartbeat loop는 없다.
5. **마지막으로 적용된 수정:** 큰 필드의 advection을 각각 `.68/-.42`, `.53/-.47`, `-.61/.32`로 높였다. 파열 경계에 한정된 shard mask와 `.012 + pressure*.013` 국소 반경 변화를 추가했다. 이 마지막 수정 이후 새로운 방향·polish를 추가하지 않았다.

## 실제 렌더 기준 평가

Still 3방향, Front/Orbit 시퀀스의 시간별 프레임, 1.100~1.467초 방전 연속 프레임을 직접 비교했다. 아래는 CODEX의 시각 평가이며 사용자 승인이 아니다.

| 확인 항목 | 최종 판단 |
|---|---|
| Hairball / yarn | **큰 개선.** R2의 빗질한 가는 줄무늬가 사라지고 큰 밝은 영역과 틈이 먼저 읽힌다. |
| Chunky plasma mass | **대체로 개선.** 굵은 발광 덩어리와 찢어진 외곽이 보인다. 단, 일부 영역은 플라즈마보다 부드러운 판/액체 막으로 읽힐 수 있다. |
| 접힌 막 같은 표면 | **부분 개선.** 마지막 속도·경계 변화로 같은 면이 오래 고정되는 인상은 줄었다. 곡선형 검은 홈과 매끈한 큰 면은 남아 있으므로 해결 완료라고 보지 않는다. |
| 흰 구슬 여러 개 | **큰 개선.** Side의 분리된 원형 광원들이 중앙의 불규칙한 빛으로 연결된다. 일부 각도·시점에는 core가 하나의 흰 blob/disc처럼 보이고 작은 타원형 내향 streak도 보인다. |
| Dynamic arc | **움직임에서 개선.** 1.10~1.20초와 1.23~1.33초의 방향이 다른 burst 이후 1.37~1.47초에는 큰 아크가 사라진다. 계속 붙어 회전하는 가지 인상은 줄었다. 정지 프레임에서는 긴 직선 구간이 여전히 철사/꺾인 막대처럼 보일 수 있다. |
| Hero rupture | 코어 근처에서 이어진 magenta 경로와 외부 white-hot 끝이 읽힌다. 어두운 배경·회색 바닥에서는 확인 가능하지만, 밝은 body/core 위는 묻힌다. 고휘도 낮 장면 전반을 검증한 것은 아니다. |
| Sphere mass / depth | 전체 구형 질량은 유지된다. Orbit에서 전면 홈·중앙 광원·후방 아크의 상대 가림이 남는다. 광원 융합 때문에 R2보다 개별 hotspot의 위치·깊이 구분은 약해졌다. |
| 레퍼런스 접근도 | R2보다 굵은 질량, 연결된 광원, 짧은 외부 파열에 가까워졌다. 레퍼런스의 거칠고 강한 백색 폭주·큰 방전·압력감에는 아직 부족하다. |

**남은 한계와 trade-off:** surface detail을 크게 만든 대가로 부드러운 판/막 같은 면이 드러난다. 광원을 연결한 대가로 개별 깊이 가독성이 약해졌다. 일부 내향 streak는 밝은 타원으로 보이며, 중심부 HDR 합침과 미세한 층 무늬가 남는다. 아크의 사건성은 좋아졌지만 정지 형태와 core 위 가독성은 완전하지 않다. 구체 전체가 아직 레퍼런스보다 정돈되고 부드럽다.

**추천:** R2의 세 핵심 문제를 줄인 비교 후보로 사용자 검토를 권한다. Production 이식 완성본으로 승인할 수준이라고 단정하지 않는다. 추가 수정이나 자동 이식은 하지 않고 사용자 확인을 기다린다.

## 최소 검증 및 보호

- 기존 최종 실행 [purple-r3-final.xml](../unity/Logs/AstraCombatQA/purple-r3-final.xml): **2/2 PASS**, skipped 0. `SafePreviewCompileAndClose`와 `MinimumStillAndMotion`만 실행했다. 최종 재개 후 반복 실행 없음.
- C# compile / shader error **0**. Preview 생성·R2/R3 선택·정리, 기존 camera/user object 상태 보존 확인. Full regression 없음.
- 최종 MP4 4개 모두 30fps / 91프레임 끝까지 decode 성공. [미디어 검증](../unity/Logs/PurpleD2R3QA/Final/media-validation.json).
- 전체 R3 작업의 PNG는 **15장**, 최종 still은 3장. 수백 장 PNG capture 없음. 최종 재개 후 Unity capture 추가 없음.
- 시작 시 보존 snapshot **636개** 중 handoff/task 문서 2개만 의도적으로 갱신했다. **634개 동일**, 예상하지 않은 변경 0. R2 소스·최종 결과, R1/D2/D/A/B/C, production, 사용자 prototype/Scene, gameplay, Package/ProjectSettings는 시작 상태 그대로다.
- 3차 frozen baseline **49/49 동일**. 재개 시점의 R3 source/metadata **16개도 동일**. [보호·Git 검증](../unity/Logs/PurpleD2R3QA/final-audit.json).
- HEAD `a9c6ccf9c09b2effdd559e83e865296e45f600b2` 유지, index 비어 있음. commit/push/merge/reset/clean/restore 미실행.

## 변경 파일 / Git

R3 전용 8개 + 각 `.meta` 8개 추가:

- [Profile](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2R3Profile.cs), [Prototype](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2R3Prototype.cs), [Bench](../unity/Assets/Scripts/Dev/VFXLab/PurpleBodyHybridD2R3Bench.cs)
- [Window / 한국어 Inspector](../unity/Assets/Editor/PurpleBodyHybridD2R3Window.cs), [최소 테스트](../unity/Assets/Editor/PurpleBodyHybridD2R3Tests.cs)
- [Body shader](../unity/Assets/Resources/VFX/HollowPurpleHybridD2R3.shader), [Arc shader](../unity/Assets/Resources/VFX/HollowPurpleHybridD2R3Arc.shader), [Profile asset](../unity/Assets/Resources/VFX/PurpleBodyHybridD2R3Profile.asset)

문서: 이 보고서 추가, `docs/CURRENT_HANDOFF.md` / `docs/active/CURRENT_TASK.md` 최신 상태 추가. 최종 재개 구간의 작업은 보고서·비교 자료 정리뿐이다.

`git status -sb`: **feat/gojo-blue-screen-distortion**, tracked modified **20**, untracked **138**. 기존 로컬 변경 포함 숫자이며 이번 패스가 production 파일을 수정했다는 뜻이 아니다. [전체 Git 상태](../unity/Logs/PurpleD2R3QA/final-git-status.txt). QA/영상은 로컬 `unity/Logs/PurpleD2R3QA/`에 저장되며 Git에서 제외된다.
