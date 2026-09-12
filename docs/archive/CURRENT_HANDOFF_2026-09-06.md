# Current Project Handoff

작성 기준: 2026-09-06

## 현재 단계

Gate 5B First Production-Quality Vertical Slice 진행 중.

현재 상태:

- Pass 8 JJK-Referenced Production Particle VFX Runtime:
  USER VISUAL REVIEW FAILED / REWORK REQUIRED
- Pass 8R-1 Gojo Signature VFX Reference-First Rework:
  USER VISUAL PARTIAL / DETAIL REWORK REQUIRED
- Pass 8R-2A Gojo Blue Production VFX Benchmark:
  USER VERIFIED / CLOSED
- Gojo Blue 4-hit gameplay:
  USER VERIFIED as part of accepted Blue slice
- Gojo Blue local voice playback:
  USER VERIFIED LOCALLY
- Gojo adult blindfold model + Humanoid pipeline:
  USER VERIFIED LOCALLY
- Gojo custom Idle animation:
  USER VERIFIED LOCALLY
- Gojo Run clip retarget/bake/export:
  USER VERIFIED LOCALLY
- Gojo Idle ↔ Run locomotion wiring:
  USER VERIFIED LOCALLY
- Gojo face/jaw skin-weight repair:
  USER VERIFIED LOCALLY
- Next animation task:
  Blue casting motion + hand/anchor integration

Pass 8R-1은 Pass 8 대비 방향성은 개선됐지만 final-quality VFX로 승인되지 않았다.
Pass 8R-2A는 이후 반복 polish를 거쳐 사용자가 최종적으로 "이 정도면 합격" 판정을 했으므로 CLOSED다.

> 중요: 아래에 남아 있는 Pass 8R-2A 세부 섹션의 `REMOTE IMPLEMENTED / USER VISUAL TEST PENDING`
> 문구는 당시 진행 이력을 보존한 historical 기록이다. 현재 상태 판단은 이 문서 상단의 2026-09-06 최신 상태가 우선한다.

## 2026-09-06 최신 로컬 작업 인수인계

### 원격 branch 기준

현재 작업 branch:

`feat/gojo-blue-screen-distortion`

locomotion fix 직후 원격 HEAD:

`751e3e4b9134576e84bf46bc73c926ed3dc0c0cd`
(`fix: bind VFXLab to scene animator`)

이후의 고죠 모델/Blender/Animator/Idle 작업은 사용자 PC의 로컬 Unity 프로젝트에서 진행됐다.
따라서 GitHub에 해당 FBX/텍스처/Animator/scene 변경이 이미 올라갔다고 가정하면 안 된다.
다음 Git 작업 시작 전에 반드시 실제 `git status`와 diff를 확인한다.

### Blue VFX / Audio

- Gojo Blue production VFX는 사용자 시각 합격으로 `USER VERIFIED / CLOSED`.
- Red는 아직 시작하지 않는다.
- 사용자 지시 순서: Blue 완성 → Blue 음성 → Gojo 모델/애니메이션 → Red.
- Blue 음성은 로컬 `unity/Assets/Resources/LocalAudio/Gojo_Blue.ogg`에서 재생되며 사용자가 Unity에서 실제 출력 확인.
- `LocalAudio`와 루트 `사운드 모음/`은 현재 원격 `.gitignore`에 포함되어 있어 공개 저장소에 올라가지 않는다.
- 음성은 로컬 개발용 자산으로 취급한다.

### Gojo 모델

현재 실제 사용 기준은 성인 붕대/안대 고죠 모델.

로컬 작업 흐름:

1. 원본 `Gojo_Blindfold_Rigged.fbx`를 Mixamo로 Humanoid 리깅.
2. Unity에서 Humanoid 주요 bone mapping 정상 확인.
3. 모델 visual scale은 VFXLab child 기준 `0.5 / 0.5 / 0.5`.
4. 재질/텍스처는 사용자가 Unity에서 직접 비교해 올바르게 연결했고 시각 확인 완료.
5. 원본 `Gojo_Blindfold_Rigged`는 이제 기준 본체가 아니라 백업 원본으로만 유지.
6. 실제 씬 기준 본체는 `Gojo_Blender_Master`.

모델 출처/권리 provenance가 명확하지 않으므로 현재 정책은 local-only다.
`unity/Assets/LocalModels/`와 `unity/Assets/LocalModels.meta`는 원격 `.gitignore`에 추가했다.
모델/텍스처/Blender-export FBX는 계속 공개 GitHub에 올리지 않는다.

### Blender animation pipeline — 중요

Blender 작업 파일:

`Gojo_Animation_Work.blend`

현재 custom Action:

`Gojo_Idle`

Idle:
- 30 fps
- frame 1 / 30 / 60
- 양손 주머니
- 한쪽 다리에 체중을 둔 건방진/여유로운 포즈
- frame 30에서 Head/Neck 중심의 아주 작은 breathing motion
- loop 재생 정상

처음에는 `Gojo_Idle.fbx`를 별도 Humanoid Avatar로 만들면서 Unity에서 손목이 펴지고
짝다리 포즈가 소실되는 문제가 발생했다.

실제 원인:
- 원본 Mixamo FBX Avatar와 Blender-export animation FBX의 Transform hierarchy가 달랐음.
- Unity에서 `Copied Avatar Rig Configuration mis-match`
  / `Transform 'Armature' not found in HumanDescription` 오류 발생.
- 별도 `Create From This Model` Avatar를 쓰면 Humanoid retarget 결과가 Blender 원본 포즈와 달라져
  손목/다리 포즈가 틀어졌음.

확정 해결 pipeline:

1. Blender에서 같은 Mesh + Armature를 animation 없이
   `Gojo_Blender_Master.fbx`로 export.
2. Unity에서 `Gojo_Blender_Master`:
   `Humanoid → Create From This Model`.
3. 이것을 새 기준 `Gojo_Blender_MasterAvatar`로 사용.
4. 같은 Blender Armature에서 export한 `Gojo_Idle.fbx`:
   `Humanoid → Copy From Other Avatar → Gojo_Blender_MasterAvatar`.
5. VFXLab 실제 모델도 `Gojo_Blender_Master`를 사용.
6. Animator:
   - Controller: `Gojo_Animator`
   - Avatar: `Gojo_Blender_MasterAvatar`
   - Apply Root Motion: OFF
7. 이 구조로 바꾼 뒤 Blender의 손목 꺾임 + 짝다리 포즈가 Unity에서도 정상 재생되는 것을
   사용자가 직접 확인함: `USER VERIFIED LOCALLY`.

이 pipeline은 이후 Gojo locomotion / Blue cast / attacks에도 그대로 유지한다.
다시 기존 `Gojo_Blindfold_RiggedAvatar`와 Blender animation FBX를 섞지 않는다.

### VFXLab 현재 로컬 hierarchy 방향

```text
VFXLab
└─ PreviewCharacter
   └─ AuthoredModelRoot
      └─ Gojo_Blender_Master   ← current local working model
```

기존 `Gojo_Blindfold_Rigged` Hierarchy 인스턴스는 제거 가능하며,
Project의 원본 FBX는 비교/백업용으로 당분간 남긴다.

Unity Editor에서 반복적으로 보이는:

- `MissingReferenceException: m_Targets of GameObjectInspector...`
- `SerializedObjectNotCreatableException: Object at index 0 is null`

은 현재 확인된 stack 상 Unity Editor Inspector stale-reference 계열이며,
게임/animation 기능은 정상 동작했다. 작업을 막지 않는 한 별도 우선순위로 올리지 않는다.

### 2026-09-07 얼굴 웨이트 + locomotion 완료

Mixamo auto-rig 이후 특정 motion에서 턱/아랫얼굴이 길게 늘어나는 문제가 있었다.
얼굴 mesh의 `mixamorig:Neck` / `mixamorig:Spine2` 영향이 턱 변형을 만들고 있었고,
얼굴 mesh 전체를 `mixamorig:Head = 1.0` 기준으로 고정한 뒤 Master FBX를 다시 export해
Idle/Run에서 턱 변형이 크게 개선된 것을 사용자가 확인했다: `USER VERIFIED LOCALLY`.

Run animation clip은 Blender retarget/bake → Master Avatar export까지 성공했고 사용자 Unity 재생 확인 완료.
이후 Animator `PlanarSpeed` 기반 Idle ↔ Run 전환도 실제 Play Mode에서 검증 완료했다.
정지 시 Idle, WASD 이동 시 Run, 입력 해제 시 Idle 복귀가 정상 동작한다: `USER VERIFIED LOCALLY`.

VFXLab locomotion 연결 중 `VfxLabPreviewCharacter.animator`가 씬 인스턴스 Animator가 아니라
원본 FBX asset Animator를 참조해 `runtimeAnimatorController == null`이 되는 문제가 확인됐다.
`RefreshAnimatorBinding()`이 Animator가 `AuthoredModelRoot` 계층에 속하는지도 검사하도록 수정해
씬 자식 Animator로 재바인딩한다. 커밋: `751e3e4b9134576e84bf46bc73c926ed3dc0c0cd`.

### 다음 작업 — 사용자 지정

다음 작업은 **Gojo Blue casting motion + hand/anchor integration**.

권장 순서:

1. Run clip retarget/bake/export — DONE / USER VERIFIED LOCALLY
2. Animator Idle ↔ Run transition using PlanarSpeed — DONE / USER VERIFIED LOCALLY
3. 얼굴/jaw skin-weight repair — DONE / USER VERIFIED LOCALLY
4. Gojo Blue 시전 모션 + hand/anchor integration — NEXT
5. Basic Attack 1 / 2 / Finisher
6. 그 뒤 Red

Run은 처음부터 전부 손으로 만들기보다 Mixamo `In Place` clip을 같은 기준 rig에 맞춰 가져온 뒤
Gojo 느낌을 Blender에서 보정하는 방식이 우선 후보다.

2026-09-07 Run pipeline result:
- Mixamo Fast Run (In Place, 30 fps) source clip 사용.
- source Armature.001의 Action을 Master Armature에 직접 지정하면 팔이 안쪽으로 꼬였음.
- 원인은 source/target rig의 기준축/rest pose 차이로 같은 local rotation 값이 다르게 해석된 것.
- Blender에서 Master Armature가 source Armature.001의 실제 world-space pose를 constraint로 따라가게 하고,
  frame 1~17을 Visual Keying + Clear Local Constraints로 Bake.
- Bake 결과를 Master 기준 Run Action으로 export.
- FBX export 핵심: Selected Objects ON, Add Leaf Bones OFF, NLA Strips OFF, All Actions OFF.
- Unity에서 Gojo_Run.fbx를 Humanoid / Copy From Other Avatar / Gojo_Blender_MasterAvatar로 연결.
- 사용자가 Unity에서 실제 재생 후 "잘 뛴다" 확인.
- 따라서 Run clip 자체는 USER VERIFIED LOCALLY.
- VFXLab의 PlanarSpeed 기반 Idle↔Run 전환도 2026-09-07 USER VERIFIED LOCALLY.

## 사용자 시각 피드백 핵심

- Blue/Red/Purple/Unlimited Void 모두 Pass 8 대비 개선됐지만 디테일이 부족함
- primitive sphere / flat material / simple particle / line 느낌이 아직 강함
- 저퀄리티 “뿅뿅 발사” 인상을 벗어나려면 shader / distortion / layered timing이 필요
- Sukuna slash 계열은 상대적으로 개선되었으므로 현재 rework 대상에서 제외
- localized distortion은 오류 없이 동작하지만 field 강도가 흐림처럼 보일 정도로 약했음
- `FastClockwiseInwardStreaks`의 긴 stretched billboard가 laser bar처럼 보여 우선 rework 대상이 됨
- distortion `0.19`와 procedural particle mask는 개선됐지만 Blue 주변 공간이 비어
  “파란 구체 + arc”처럼 보이는 문제가 다음 우선순위가 됨

## 기준 문서

- docs/JJK_VFX_REFERENCE_GOJO.md
- docs/JJK_VFX_EXTERNAL_IMPLEMENTATION_RESEARCH.md
- docs/NEW_CHAT_BOOTSTRAP.md
- docs/GATE5B_FIRST_PRODUCTION_SLICE.md
- docs/DEVELOPMENT_ROADMAP.md
- 사용자 조사 요약: Unity 고죠 VFX 구현 조사 요약
- docs/JJK_VFX_EXTERNAL_REFERENCE_SURVEY.md

## 외부 조사에서 확인된 다음 방향

좋은 Gojo VFX는 단일 ParticleSystem이 아니라 다음을 중첩하는 방식이 적합:

- Mesh / Sphere or custom mesh
- Shader / Shader Graph
- Particle System / VFX Graph
- Screen Distortion
- Post Processing
- Camera / feedback
- Timing

기술별 핵심:

Blue:
- Core shader
- inward + spiral particle
- distortion

Red:
- core
- outward particle
- layered shockwave
- distortion

Hollow Purple:
- Blue/Red fusion
- flash
- purple energy body
- short trail
- distortion

Unlimited Void:
- dedicated environment
- galaxy/star/void layers
- screen-space transition
- lighting

## 현재 렌더링/패키지 제약

- Unity 6 / URP 17.3
- VFX Graph 패키지는 현재 `Packages/manifest.json`에 설치되어 있지 않음
- PC Renderer에는 현재 Screen Space Ambient Occlusion만 Renderer Feature로 등록되어 있음
- Full Screen Pass / distortion Renderer Feature는 아직 구성되어 있지 않음
- 따라서 다음 pass에서 VFX Graph나 Full Screen Pass를 전제로 코드를 쓰지 말고,
  기존 ParticleSystem + custom URP shader를 먼저 production benchmark로 만든 뒤
  distortion pipeline은 별도 검증 가능한 단계로 추가할 것

## 다음 작업

Pass 8R-2를 단순 detail tuning이 아니라:

Gojo VFX Production Pipeline Upgrade

로 진행.

한 번에 네 기술을 모두 크게 고치지 않는다.

순서:

1. Blue Production VFX
2. Red Production VFX
3. Hollow Purple Production VFX
4. Unlimited Void Production Environment / Transition

Blue 하나에서 먼저 production-quality 기준을 만든 뒤 다음 기술에 같은 파이프라인을 재사용한다.

## Pass 8R-2A 구현 handoff

Blue에만 다음 production benchmark를 원격 구현했다.

- Resources에 포함되는 custom URP procedural energy shader
- deep body / Fresnel shell / distortion-like outer shell 3-layer core
- scene-owned shared material template + renderer별 MaterialPropertyBlock
- fast clockwise inward streak + slow counter spiral mote
- 서로 다른 길이/폭/alpha/timing을 가진 outer/mid broken arc 8개
- outward burst가 아닌 impact collapse
- renderer feature와 분리된 `GojoBlueDistortionSource` 기반 localized screen distortion shell
- procedural mask 기반 Blue 전용 particle shader와 shared material template
- presentation-only ground dust와 sparse dark debris inward suction layer

VFX Graph, Full Screen Pass, PC Renderer YAML, external texture는 추가하지 않았다.
Red/Purple/Unlimited Void/Sukuna/Megumi와 gameplay 값도 변경하지 않았다.

Blue benchmark를 Unity에서 처음 실행하면서 실제 runtime/shader 문제 2종을 발견했고 master에서 수정했다.

- `6487acd`: Particle orbital X/Y/Z MinMaxCurve mode 불일치 + 생성 직후 playing 상태에서 duration 변경 오류 수정
- `8247db9`: HLSL/D3D11에서 예약어와 충돌한 shader 함수 인자 `point`를 `samplePos`로 변경

이후 사용자는 실제 Blue effect를 확인할 수 있는 상태까지 도달했다.
다만 Blue를 USER VERIFIED로 승인한 것은 아니다. 현재는 외부 고퀄 Unity/Unreal 구현과 비교하면서
detail ceiling과 다음 polish 방향을 정하는 단계다.

추가 외부 구현 조사 결과는:
`docs/JJK_VFX_EXTERNAL_IMPLEMENTATION_RESEARCH.md`

에 기록했다.

현재 reference 역할:
- starkim1999: lore/readability + inward/outward motion + distortion + Domain tunnel/environment
- Zepwlert "The Strongest Sorcerer": 236 ParticleSystem, screen-space/distortion/sound를 겹친 layer-rich benchmark
- Floppiii: particle texture / anime particle shader / screen-space cinematic set-piece benchmark
- Swammy JJK Fan Game (UE5.4+): 실제 action-game scale/readability/integration benchmark
- Deadlyxrebel Hollow Purple: debris suction / material cracking / impact decal / stage transition detail 아이디어

현재 다음 단계는 Blue를 바로 완료 처리하는 것이 아니라
`Blue Production VFX Benchmark 2차 polish`를 검토하는 것이다.

권장 우선순위:
1. visual-only dust/debris suction Unity 시각 검수
2. field distortion과 short tapered particle 유지 확인
3. dust/debris density와 readability 최종 튜닝
4. presentation layer onset stagger
5. 사용자 영상/스크린샷 기반 final art tuning

Blue는 2026-09-06 기준 사용자 합격으로 CLOSED다.
단, 사용자 지시에 따라 Red로 바로 넘어가지 않고 Gojo 모델/애니메이션 slice를 먼저 완성한다.

## Gojo Blue Localized Screen Distortion

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- `GojoBlueDistortion.shader`가 URP `_CameraOpaqueTexture`를 실제 sample한다.
- Blue instance마다 `BlueScreenDistortionShell`을 만들고 world radius/strength/impact metadata를
  `MaterialPropertyBlock`으로 shader에 연결한다.
- field Blue base strength는 사용자 피드백에 따라 `0.15 → 0.19`로 조정했고,
  impact base strength `0.24`는 유지한다.
- distortion shell은 Blue energy/particle보다 먼저 그려지고 Screen Space Overlay HUD에는 영향을 주지 않는다.
- PC RP asset의 기존 opaque texture 설정을 사용하며 Renderer Feature/YAML은 변경하지 않았다.
- shell과 binding은 기존 `GojoBlueVfxInstance` lifetime/fade를 그대로 따른다.
- Unity shader compile과 실제 시각 결과는 아직 사용자 검수 전이다.

## Gojo Blue Procedural Particle Mask 1차

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- 외부 texture 없이 `GojoBlueParticle.shader`의 procedural UV mask를 사용한다.
- 하나의 shared material template에서 soft mote / tapered streak / broken wisp를 renderer별
  `MaterialPropertyBlock` mode로 선택한다.
- Fast streak의 inward speed는 유지하면서 lifetime, velocity stretch, length scale을 줄였다.
- SlowCounterSpiralMotes는 soft mote, BlueCorona는 random-rotation broken wisp를 사용한다.
- generic particle factory의 기본 material/stretch 값과 다른 technique VFX는 변경하지 않았다.
- 실제 particle silhouette, warning, replay/cleanup 결과는 Unity 사용자 검수 전이다.

## Gojo Blue Visual-Only Environment Suction

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- `GojoBlueVfxInstance`가 `GroundDustSuction`과 `DarkDebrisFragments` ParticleSystem을 직접 소유한다.
- dust는 Blue anchor 기준 낮은 얇은 영역에서 soft mote로 떠오르며 중심으로 수렴한다.
- debris는 sparse dark blue-gray fragment로 생성되어 약하게 orbit한 뒤 중심에서 축소·소멸한다.
- inward radial speed가 orbit보다 크고 noise는 가장 약하게 제한된다.
- impact cue는 짧은 lifetime/높은 inward speed와 normalized compression 기반 simulation 가속을 사용한다.
- `GojoBlueParticle.shader`에 procedural `DarkFragment` mode 하나만 추가했다.
- Rigidbody, collider, Physics query, scene object 이동, gameplay 값 변경은 없다.
- 두 system은 기존 Blue instance particle/material lifecycle과 cleanup을 그대로 따른다.
- 실제 밀도, 바닥 착시, fighter readability와 cleanup 결과는 Unity 사용자 검수 전이다.

## Gojo Blue Final Timing / Readability Tuning

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- gameplay duration이나 spawn request를 바꾸지 않고 `GojoBlueVfxInstance` 내부 normalized time만 사용한다.
- field는 dense core 0~10% → Fresnel/distortion 8~22% → outer/corona 12~28% → dust 18~34%
  → debris 22~38% → slow spiral 24~42% → arc 30~50% → fast spiral 42~58% 순서다.
- field distortion base `0.19`와 impact base `0.24`는 유지하며 field distortion만 거의 0에서 시작한다.
- field convergence arc는 core/environment 뒤에 등장하며 alpha scale을 `0.80`으로 낮췄다.
- dust의 alpha/size/lift와 debris의 size/blue-gray 대비/lift를 높였고 particle 수와 inward 속도는
  크게 늘리지 않았다.
- impact는 0~20% hold, 20~75% collapse 가속, lifecycle의 기존 종료 fade로 이어진다.
- layer별 `MaterialPropertyBlock` 갱신과 기존 ParticleSystem module을 재사용하며 per-frame Play/Stop이나
  새 managed allocation은 추가하지 않았다.
- Blue 전체는 아직 USER VERIFIED가 아니며 실제 onset 순서, dust/debris 가독성, impact handoff와
  replay/cleanup은 Unity 사용자 검수 전이다.

## Gojo Blue Stylized Suction Wind Layer

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- Blue 전용 `AirflowSuctionWisps`와 `WindRibbonConvergence` ParticleSystem을 추가했다.
- 기존 shared `GojoBlueParticle` material에 procedural `AirflowWisp`/`WindRibbon` mask mode를 추가하고
  renderer별 `MaterialPropertyBlock`으로 선택한다.
- 두 layer는 local 3D height/radius variation, 약한 orbit/noise, 가속되는 inward radial curve를 사용한다.
- airflow는 field 18~34%, sparse ribbon은 28~52%에 등장해 core/shell보다 늦고 fast convergence보다
  먼저 공기 흡입을 보조한다.
- 외부 asset, VFX Graph, fluid/fog simulation, generic particle factory 변경은 없다.
- 기존 Blue instance lifetime과 cleanup을 따르며 실제 3D depth, arc 구분, core readability와 누적 여부는
  Unity 사용자 검수 전이다. Blue 전체 상태도 계속 USER VISUAL TEST PENDING이다.

## Gojo Blue Oppressive Singularity + 4-Hit Gameplay

상태:

```text
REMOTE IMPLEMENTED / USER TEST PENDING
```

- 기존 `0.10s` pull pulse와 field duration은 유지하고 normalized 18/38/58/78%에 damage pulse 4회를 둔다.
- `BlueConvergenceField`가 target별 성공 hit count를 보관하며 각 hit는 기존 총 damage의 1/4이다.
- 첫 성공 hit만 기존 full hit stun을 사용하고 이후 hit/pull reaction은 기존 `0.08s` 상한을 유지한다.
- 첫 impact VFX/audio는 한 번만 발생하고 `BlueHit`은 성공한 각 damage pulse를 전달한다.
- production Blue visual은 같은 4-pulse schedule로 core compression, light/distortion, suction speed를
  미세하게 accent한다. VFXLab도 같은 shared visual beat를 사용하지만 gameplay damage는 실행하지 않는다.
- dust/debris/airflow/ribbon의 visual spawn radius를 넓히고, inward curve의 후반 가속과 near-core
  shrink를 강화해 실제 physics나 map destruction 없이 특이점 착시를 보강했다.
- CE, cooldown, cast time, range, field radius/duration, pull speed와 target selection은 변경하지 않았다.
- 실제 4-hit count/damage 합계/stun 체감과 visual 위압감은 Unity 사용자 검수 전이다.

## Gojo Blue Oppressive Singularity Visual Rework 2 + VFXLab Audio Preview

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- 1차 Oppressive visual은 사용자 검수에서 변화가 너무 미세해 실패했으며, 이번 rework가 이를 대체한다.
- VFXLab sequence host가 `PrototypeCombatAudio.GetOrCreate(gameObject)`를 한 번만 사용하고 runtime-only
  playback entry point를 직접 호출한다. Blue full/field debug/impact debug와 Basic, Dodge, Red, Purple,
  Unlimited Void preview에 fallback SFX parity를 제공하며 replay/cancel 때 transient source만 정지한다.
- gameplay outer radius를 바꾸지 않고 visual spawn 반경을 dust `1.20x`, debris `1.26x`, tidal debris
  `1.30x`, airflow `1.32x`, wind ribbon `1.43x` 수준으로 확대했다.
- `TidalDebrisTrails` sparse layer를 추가해 바깥의 작은 파편이 최대 `24m/s`로 급가속하고 마지막
  lifetime 18%에서 급격히 가늘어져 사라지는 spaghettification proxy를 만든다.
- airflow/ribbon particle 크기와 shader mask 폭을 크게 키우고 alpha/emission을 제한해 얇은 파란 선보다
  넓은 반투명 pressure sheet로 읽히도록 했다.
- field distortion shell은 기본 Blue에서 약 `1.48m -> 4.14m`, base strength는 `0.19 -> 0.26`으로
  확대했다. shader는 약한 outer warp와 강한 inner compression의 2단 radial profile을 사용한다.
- core는 dark navy density, cyan/white pressure rim, 지름 약 `0.18m` white-hot compression point로
  대비를 재구성했다. 단순 emission 증가 대신 중심부 명암과 scale compression을 분리했다.
- 18/38/58/78% pulse는 기존 gameplay schedule을 그대로 공유하며 core contraction `4.5% -> 18%`,
  suction speed accent `10% -> 62%`, distortion accent `8% -> 38%`, light accent `18% -> 44%`로 강화했다.
- Blue 4-hit damage/stun/pull 및 CE/cooldown/cast/range/radius/duration 값은 변경하지 않았다.
- 실제 SFX 출력, 네 번의 inward pulse 가독성, gameplay camera/HUD readability와 Console 상태는
  Unity 사용자 검수 전이다.

## Gojo Blue Final Visual Polish (Palette + Early Onset)

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- Dense body를 near-black navy `(0.001, 0.004, 0.055)`에서 deep vivid blue `(0.004, 0.028, 0.18)`로,
  mid를 `(0.004, 0.035, 0.30)`에서 electric blue `(0.008, 0.16, 0.82)`로 옮겼다.
- Dense emission은 `0.92 -> 1.05`의 제한된 보정만 사용하고 Fresnel/outer shell 및 환경 suction 색을
  azure/cyan 쪽으로 이동해 white-hot point, dark density, cyan rim의 깊이는 유지했다.
- field 시작 weight를 ground dust `20%`, dark debris `24%`, tidal debris `16%`로 주고 각각 normalized
  `42/40/46%`까지 점진적으로 full strength가 되도록 바꿨다.
- 사용자가 말한 상부 나선환은 `FastClockwiseInwardStreaks`와 보조 `SlowCounterSpiralMotes`로 해석했다.
  fast spiral은 기존 `42~58%` fade-in 대신 시작 `22% -> 38%에서 full`, slow spiral은 시작
  `16% -> 46%에서 full`로 변경해 초반부터 보이되 후반 수렴이 계속 강해진다.
- airflow는 시작 `12% -> 44%에서 full`, wind ribbon은 시작 `8% -> 48%에서 full`로 앞당겼다.
  Convergence arc는 기존 `30~50%`를 유지해 core와 suction sign보다 먼저 주인공이 되지 않게 했다.
- Blue gameplay, VFXLab `2.20s` duration, audio preview wiring, Red/Purple/Domain, HUD/camera/RP asset은
  변경하지 않았다. 실제 palette와 early onset 가독성 및 Console 상태는 Unity 사용자 검수 전이다.

## Gate 5B VFXLab developer tool

Gate 5B VFX production iteration을 위해 캐릭터 이동, prototype/future authored motion, 실제 production
Blue VFX, orbit camera, scene-owned lighting/Bloom, replay/loop/slow motion을 한 scene에서 검수하는
developer-only `Assets/Scenes/VFXLab.unity`를 추가했다. Combat gameplay 없이 동일
`GojoBlueVfxInstance`를 호출하며 상세 사용법은 `docs/VFX_LAB.md`를 따른다.

Blue 상태는 계속 `REMOTE IMPLEMENTED / USER VISUAL TEST PENDING`이다.


## Pass 8R-2A Unity test / hotfix 상태

Pass 8R-2A merge 이후 실제 Unity Editor에서 다음 오류가 발견되어 master에서 직접 수정했다.

1. Particle Orbital Velocity curves must all be in the same mode
- Blue fast/slow spiral의 orbital X/Y/Z MinMaxCurve mode를 모두 TwoConstants로 통일
- 새 ParticleSystem을 module 설정 전에 StopEmittingAndClear하여 duration 변경 오류도 같이 수정

2. GojoBlueEnergy.shader compile error: unexpected token 'point'
- HLSL/D3D11 parser에서 예약어 충돌을 피하도록 함수 인자 point를 samplePos로 변경

현재 master의 마지막 shader hotfix 기준 commit:
- 8247db95de7086b9a0ffde361fb7c9afce4df47c

중요:
- 위 오류 수정 후 Blue 효과 자체는 사용자가 확인했으나 최종 시각 합격/불합격 판정은 아직 기록되지 않았다.
- 따라서 Pass 8R-2A 상태는 REMOTE IMPLEMENTED / USER VISUAL TEST PENDING을 유지한다.
- 다음 채팅에서 먼저 사용자에게 최신 Blue의 시각 평을 확인하고, 합격 전에는 Red로 넘어가지 않는다.

## 2026-09-02 외부 Unity/Unreal JJK VFX 재조사

상세:
- docs/JJK_VFX_EXTERNAL_REFERENCE_SURVEY.md

핵심 reference:
- starkim1999 (Unity/VRChat): Blue/Red/Purple/Infinite Void. Distortion Shader + Particle Shader + Uberscreen shader. Blue inward / Red outward를 제작자가 직접 설명.
- FloppiiiVR (Unity/VRChat): Blue를 black-hole-like attraction으로 크게 staging, Unlimited Void에서 world visual을 거의 suppression하여 domain이 공간 전체를 지배.
- CooGee (Unity/VRChat): Hollow Purple에 별도 ScreenJackEffect, flash/shake/screen accent.
- YSA/Youssef Af (Unity): Red Hollow Skill에 3 textures + 7 custom Shader Graph shaders + script-controlled stage timing.
- Deadlyxrebel (RealtimeVFX, engine 미확정): Purple stage transition, lightning decals, material transition, debris/environment response를 polish 항목으로 제시.
- Swammy (UE5.4+): JJK fan game, placeholder Hollow Purple 사용자 피드백 후 rework 사례.
- Fortnite JJK (Unreal production): large-scale 3D gameplay에서 Purple silhouette/readability 참고.
- Raffi Ramadhan (UE5.3): JJK cursed-energy realtime aura.
- VinceVFX (Unity): JJK-style gameplay VFX에서 custom shading, mesh/projector shape, value/readability를 강조.

외부 사례와 사용자 제공 조사 문서의 공통 결론:
- core silhouette
- multiple shader/material layers
- particle motion
- screen-space distortion
- stage timing
- environment response
- gameplay-camera readability
를 함께 써야 production-quality JJK VFX에 가까워진다.

## 다음 권장 작업

먼저 CombatMVP와 VFXLab에서 ground dust 위치, dark debris 밀도, inward/orbit 우선순위,
impact collapse, fighter readability, replay/cleanup을 사용자 시각 검수한다. 기존 distortion과
tapered particle 유지도 함께 확인한 뒤에만 Blue final polish와 Red/Purple 재사용 경계를 결정한다.

Blue가 아직 시각적으로 부족하다고 판정되면:
- R-2B로 넘어가기 전에 Blue shader/noise/core/spiral/arc art tuning을 먼저 수행한다.

이후 순서:
1. Blue final acceptance
2. reusable distortion
3. Red production VFX
4. Hollow Purple production VFX
5. Unlimited Void environment + Enter/Active/Break transition


## 변경 금지

- Blue 총 damage 값과 4-hit 외 gameplay damage
- CE
- cooldown
- range
- hitbox
- pull/push
- Blue 4-hit schedule 외 hit timing
- Purple 0.24 / 0.78 / 18m timing
- Domain duration/radius/state/input
- camera gameplay
- HUD
- Target Lock
- Tag
- AI
- Sukuna/Megumi current gameplay

## 검증 정책

- Codex static build != Unity Play Mode
- REMOTE IMPLEMENTED와 USER VERIFIED 구분
- 사용자 시각 검수 전에는 VFX pass를 USER VERIFIED로 닫지 않음
- 큰 작업은 feature branch → commit/push → GitHub diff review → merge → Unity user test 순서


## 새 채팅 시작용 문구

새 채팅에서는 아래처럼 요청하면 된다.

"GitHub JJuNi27/JJK_game 프로젝트 작업을 이어서 할게.
먼저 feat/gojo-blue-screen-distortion branch의 docs/CURRENT_HANDOFF.md,
docs/NEW_CHAT_BOOTSTRAP.md, docs/GATE5B_FIRST_PRODUCTION_SLICE.md를 읽고 현재 상태를 복구해줘.
Gojo Blue VFX는 이미 USER VERIFIED / CLOSED이고 Blue voice도 로컬 Unity에서 USER VERIFIED야.
현재는 Gojo 모델/애니메이션 integration 단계이며, Blender-exported Gojo_Blender_MasterAvatar를
기준 Avatar로 통일해 Humanoid retarget 문제를 해결했고 custom Gojo_Idle도 Unity에서 USER VERIFIED LOCALLY야.
다만 모델/텍스처/Animator/scene 작업은 로컬일 수 있으니 GitHub에 있다고 가정하지 말고 먼저 git status/diff를 확인해.
다음 작업은 Run locomotion이며, 이후 Blue cast motion/anchor → basic attacks → Red 순서야.
기존 USER VERIFIED 상태를 되돌리지 말고 REMOTE와 LOCAL USER VERIFIED를 구분해.
큰 작업은 feature branch → commit/push → GitHub 실제 diff review → 사용자 Unity test 순서를 유지해줘." 


## 새 채팅 인수인계

새 채팅 시작용 복붙 지시문은 `docs/NEW_CHAT_BOOTSTRAP.md`에 저장되어 있다.
새 채팅은 해당 문서와 CURRENT_HANDOFF를 GitHub에서 직접 읽은 뒤 현재 master를 확인하고 이어간다.
