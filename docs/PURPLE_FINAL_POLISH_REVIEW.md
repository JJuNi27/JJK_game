# Hollow Purple — Final Polish 후보 비교

2026-09-18 · **CODEX VALIDATED / PENDING USER VISUAL REVIEW**

현재 Production 기준은 D2-R3 Body + OuterR2 + Release→Travel refinement다. 이번 작업은 이를 교체하지 않는 **독립된 Outer / Fusion Birth 후보**다. 두 후보의 저장된 기본값은 OFF이며, commit / push / merge / staging은 하지 않았다. HEAD는 `7f5c6979e893f3a79a291934403f7cc28cc232af`다.

## 실제 비교 자료

최종 자료 폴더: `unity/Logs/PurpleFinalPolishQA/Review/20260918_093348_763/`

- [비교 영상 모음](../unity/Logs/PurpleFinalPolishQA/Review/20260918_093348_763/index.html)
- [Side — Baseline vs Outer](../unity/Logs/PurpleFinalPolishQA/Review/20260918_093348_763/Side_Outer_AB.mp4)
- [Caster — Baseline vs Outer](../unity/Logs/PurpleFinalPolishQA/Review/20260918_093348_763/Caster_Outer_AB.mp4)
- [Side — Birth OFF vs ON](../unity/Logs/PurpleFinalPolishQA/Review/20260918_093348_763/Side_Birth_AB.mp4)
- [Caster — Birth OFF vs ON](../unity/Logs/PurpleFinalPolishQA/Review/20260918_093348_763/Caster_Birth_AB.mp4)
- [Side 탄생 상세 비교](../unity/Logs/PurpleFinalPolishQA/Review/20260918_093348_763/Side_Birth_detail.jpg)
- [Caster 외부 효과 상세 비교](../unity/Logs/PurpleFinalPolishQA/Review/20260918_093348_763/Caster_Outer_detail.jpg)
- 동일 폴더의 `Side_Sequence.jpg`, `Caster_Sequence.jpg`: Baseline / Outer / OuterBirth 3열 비교.
- 원본 still / RGB: `unity/Logs/PurpleFinalPolishQA/Final/20260918_093348_763/`. `Side_*_058.png` 합체 말기, `*_061.png` 탄생 직후, `*_105.png` hold, `*_160.png` release 이후, `*_171.png` 초기 travel.

각 원본 영상은 960×540, 30fps, 175프레임(약 5.83초)이다. 0초부터 Blue 등장, Red 대치, fusion, 2초 birth, 3초 completed hold, 기존 impact/release, 초기 travel까지 끊지 않고 담았다. 비교 영상은 1920×540 좌우 배치다. Birth 비교는 **Outer를 양쪽 모두 ON**으로 고정했다. Side는 release 이후 projectile을 추적하고, Caster는 기존 presentation camera를 사용한다. CombatMVP를 격리된 QA 프로젝트에 열고 같은 원거리 바닥/격자 stage, camera, clock, 배경, 기존 post process로 렌더했다. 원본 Scene/Volume은 수정하지 않았다.

## 실제 화면 판단

**Outer 후보는 비교할 가치가 있다.** 넓게 번진 Halo 바탕이 줄어 빈 공간과 밝은 조각의 경계가 더 잘 보인다. Hero fragment는 조금 더 크고 선명하며, 발생 중인 외향 번개의 굵기·도달 범위가 소폭 증가한다. 이미 있던 짧은 lifetime, taper, branch, flicker, 방향 변화는 그대로 재사용했다. 따라서 정전기를 대형 번개로 완전히 바꾼 결과는 아니다. 특히 먼 Caster travel에서는 개선 폭이 작다. 기존 bloom을 보존했으므로 Halo에 남은 부드러운 분홍 인상까지 제거되지는 않았다.

**Birth 후보는 탄생의 한 박자가 명확해졌지만 선택 보류를 권한다.** 약 1.70~1.90초에는 기존 Blue/Red mote 일부가 중심으로 모이고, 2.0초에는 국소 flash, 직후에는 외향 pressure shell과 소수 electric fragments가 나온다. Full-screen flash가 아니며 발사 impact와 약 3초 떨어져 있다. 다만 peak 한 프레임은 본체 표면을 잠깐 가리고, 2.1~2.2초 shell은 파열보다는 팽창하는 얇은 보호막으로 읽히는 순간이 있다. 작은 fragment보다 flash/expansion이 더 강하게 인지된다. 정지 화면의 장점만으로 Production 채택을 권하지 않는다.

초기 birth preview의 radial sampling band와 규칙적인 격자 패턴은 실제 렌더에서 확인하고 수정했다. 최종은 analytic shell 교차와 3D irregular mask를 사용한다. 중간 결과도 별도 timestamp 폴더에 보존했다. 위 링크는 최종본만 가리킨다.

## Profile 변경 — 원본 자산은 유지

새 자산: `Assets/Resources/VFX/PurpleFinalPolishProfile.asset`. 한국어 CustomEditor에서 두 후보를 독립적으로 켤 수 있으며 **새 시전부터 적용**된다. 사용자 검토 시 Play Mode에서만 토글하면 저장된 OFF 기준으로 돌아오기 쉽다.

| 값 | Production baseline | Outer 후보 |
|---|---:|---:|
| arcWidth | 0.095 | 0.11 (+15.8%) |
| arcReach | 1.95 | 2.05 |
| arcEmission | 7 | 8.5 |
| arcLifetime | 0.13 | 동일 |
| sparkCount | 24 | 동일 |
| sparkSpeed | 3 | 3.4 |
| sparkEmission | 4 | 5.5 |
| heroFragments | 3 | 동일 |
| heroFragmentScale | 2.6 | 3 |
| haloRatio / haloIntensity | 1.55 / 0.38 | 동일 |
| distortionRatio / strength | 1.5 / 0.006 | 동일 |

Residual seconds / inheritance / distance limit도 동일하다. 새 `coronaContrast=1`은 후보 shader에서만 작동한다. Birth 설정은 compressionStrength=1, birthStrength=1, birthDuration=0.28초다. canonical hold/release 시각을 변경하는 설정이 아니다.

## Shader / layer 변경

- `PurplePolishHalo.shader`: 기존 Travel Halo의 velocity/rear response를 복제해 보존하고, 후보에서만 diffuse 계수를 0.20→0.035로 낮췄다. thickness 범위 0.045~0.14→0.025~0.095, crest 2.8→4.8, gap/rupture threshold를 조절했다. raymarch 80회는 동일하다. Charge에서는 motion=0으로 작동한다. 기존 두 Halo shader는 수정하지 않았다.
- `PurpleFusionIngredient`: 기존 20 mote 중 8개씩만 late fusion에 재사용한다. 기존 부모가 마지막에 4%로 축소되어 단순 밝기 증가가 읽히지 않으므로, 압축 구간에만 world 위치를 local로 변환하고 폭을 보정한다. Blue/Red sphere와 이동 경로, 기존 curves/bridge는 그대로다.
- `PurpleFusionBirthAccent`: Blue/Red ingredient가 hold 시작에 비활성화된 뒤에도 약 0.28초의 outward birth를 표현해야 하므로 추가한 **root 소유의 짧은 layer**다. localized flash/pressure shell 1개와 기존 filament shader를 사용하는 electric fragment 6개다. Body의 material/property, gameplay, camera를 쓰지 않는다.
- `PurpleFusionBirth.shader`: scene depth로 제한된 local flash와 front/back sphere 교차의 irregular pressure mask. global Bloom/Volume, fullscreen distortion, camera impulse/FOV/impact 추가 없음.

## 보호 근거

- 작업 시작 시 Git 관리/비무시 파일 **673개**의 SHA-256을 보관했다. 코드 작업 후 변경된 기존 파일은 아래 세 개의 opt-in hook뿐이었다. 누락/삭제 0. 문서 갱신은 이후 별도로 기록했다.
- D2-R3 Body runtime/mesh 생성 코드/두 shader, Production profile, Travel profile/wake/halo/distortion, Gojo gameplay controller 및 잠금 설정, terminal, camera presentation, 기존 exploration은 원본 hash와 동일하다.
- 기존 dirty Blue / Red / Domain / VFXLab / ProjectSettings / 진단 파일도 작업 시작 hash와 동일하다. ignored LocalModels/LocalAudio/user prototype/reference/기존 QA에는 쓰기 작업을 하지 않았다.
- 자동 비교: 9개 시점 × Blue/Red/Purple의 position/scale/forward = **81개 Vector3 값 차이 0**. Production 및 timing profile 직렬화 값 변화 0.
- 동일 최종 실행의 Outer OFF/ON: hold 중앙 11,000픽셀 차이 0. 이것은 전체 화면의 bloom까지 동일하다는 뜻은 아니다.
- Birth OFF/ON: Side/Caster 모두 차이는 frame 51~68(1.70~2.267초)에만 있다. **2.3초부터 종료까지 106프레임 전체 RGB 차이 0**. completed hold / release / travel에 Birth 영향이 남지 않는다.
- Birth 구현 전 preview와 구현 후 OFF 화면의 교차 실행 비교는 293픽셀이 달랐다. 모두 캐릭터 영역(x370~402, y322~368)에 한정되고 Purple 영역은 동일했다. 교차 실행의 캐릭터 pose까지 완전 일치했다고 주장하지 않는다.
- 상세: `unity/Logs/PurpleFinalPolishQA/preservation.json`, `locks.json`, `Review/20260918_093348_763/comparison_metrics.json`.

## Targeted validation

최종 관련 테스트 **6종 PASS / 0 FAIL**, 초기 Outer preview 별도 1 PASS. Render를 shader 보정 후 두 번 갱신했으므로 전체 실행 기록은 9 PASS / 0 FAIL이다. 반복 실행을 새 테스트 종류로 세지 않았다. **C# compile error 0 / Shader compile error 0**. Full regression은 실행하지 않았다.

| 테스트 | 결과 |
|---|---|
| PurpleFinalPolishTests.CanonicalPoseAndBodyLock | PASS |
| PurpleFinalPolishTests.FullComparison | PASS, 최종 render-03 |
| PurpleFinalPolishTests.TravelTargetsRangeAndRepeatedCleanup | PASS, 두 후보 ON |
| AstraCombatVfxTests.PurpleHoldReleaseIsContinuousAndDamageUsesSameCorridor | PASS, 후보 OFF |
| AstraCombatVfxTests.PurpleCameraCancellationAndResourceCleanup | PASS, 후보 OFF |
| ProductionPurpleIntegrationTests.ActualCombatNoHitMaxRange | PASS, 후보 OFF |

후보 ON lifecycle 4회: 첫 대상 HP45 / 뒤 대상 HP100, 첫 적중 종료, max-range terminal, travel 중 취소, birth flash 직전 취소, material/mesh 증가 0. Terminal은 legacy PurpleEnergyBody이며 새 Production body/outer가 들어가지 않는다. Side/Caster 렌더 종료 후 material 0, Caster FOV/position 복구 확인. 최대 active outer particle은 116, 잔류 거리 최대 약 4.195m로 기존 travel 상한 안이다.

로그/XML: `unity/Logs/AstraCombatQA/purple-final-outer-preview-01.*`, `purple-final-polish-01.*`, `purple-final-existing-targeted-01.*`, `purple-final-polish-render-02.*`, `purple-final-polish-render-03.*`. 최종 shader 결과는 render-03을 기준으로 한다. Lifecycle: `unity/Logs/PurpleFinalPolishQA/lifecycle.json`.

## 비용 / 남은 한계

- Outer: 기존 2 Particle System / 기존 material 수 유지, spark/hero 개수 증가 없음. Halo raymarch 수와 texture fetch 구성은 기존 수준이다. 더 밝고 큰 fragment 때문에 화면 overdraw가 소폭 늘 수 있다.
- Birth: 후보 ON 시 owned material **2개**, cube renderer 1개, LineRenderer 6개 추가. 최대 0.335초의 local shell window, fragment는 0.28초만 활성. 기존 mote 재사용으로 compression용 renderer 추가 없음. 매 프레임 material/mesh를 만들지 않는다. root dispose 시 제거된다.
- 실제 GPU ms / 저사양 기기 / 최대 동시 시전 stress는 측정하지 않았다. Batch render 속도를 실게임 GPU 성능으로 해석하지 않는다.
- 시각 한계 Top 3: ① 멀어진 Caster에서는 번개/파편의 micro-polish가 약함. ② Birth flash의 잠깐 white-out 및 팽창 shell의 보호막 인상. ③ inward compression/소수 fragments보다 이미 밝은 본체와 shell이 먼저 읽힘.

**추천:** Outer 후보는 채택 검토를 권한다. Birth는 영상에서 peak의 이득과 shell 인상을 비교한 뒤 별도로 선택한다. 두 후보 모두 현재 OFF이며 Production 최종 승인이나 USER VERIFIED 표시는 하지 않는다.

## 실제 수정 파일

기존 코드 3개:

- `unity/Assets/Scripts/Player/ProductionPurpleOuterRuntime.cs` — 후보 outer settings/shader 선택만 추가.
- `unity/Assets/Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs` — 후보가 켜진 시퀀스에 compression/Birth 소유권 연결.
- `unity/Assets/Scripts/Player/PurpleFusionIngredient.cs` — 선택적 late-fusion mote 재사용.

신규 Unity 파일 7개 및 각각 `.meta`:

- `unity/Assets/Scripts/Player/PurpleFinalPolishProfile.cs`
- `unity/Assets/Scripts/Player/PurpleFusionBirthAccent.cs`
- `unity/Assets/Resources/VFX/PurpleFinalPolishProfile.asset`
- `unity/Assets/Resources/VFX/PurplePolishHalo.shader`
- `unity/Assets/Resources/VFX/PurpleFusionBirth.shader`
- `unity/Assets/Editor/PurpleFinalPolishProfileEditor.cs`
- `unity/Assets/Editor/PurpleFinalPolishTests.cs`

문서: 이 보고서 및 `CURRENT_HANDOFF.md`, `active/CURRENT_TASK.md`, `architecture/GOJO_VFX.md`. QA script/media/log는 `unity/Logs/`의 local-only 자료다. 기존 dirty 파일은 이번 변경 목록에 포함하지 않는다.
