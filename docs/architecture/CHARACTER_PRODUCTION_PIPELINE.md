# Character Production Pipeline — 3D / zero-budget hobby workflow

Status: **PLANNING DOCUMENT / NOT IMPLEMENTED**

이 문서는 Gojo 한 명을 수작업으로 완성한 뒤 캐릭터마다 같은 과정을 처음부터 반복하는 문제를 줄이기 위한 장기 파이프라인이다. 현재 VFX 작업을 중단하거나 2D로 전환하기 위한 문서가 아니다.

## 목표

- 게임은 **3D 액션 격투** 방향을 유지한다.
- 외주/유료 제작을 전제로 하지 않는다.
- 캐릭터를 매번 새 프로젝트처럼 만들지 않고, **공통 인간형 기반 + 전투 archetype + 캐릭터 전용 hero 작업**으로 분해한다.
- 서로 다른 출처의 모델을 그대로 섞어 생기는 스타일 불일치를 줄인다.
- 공통 모션은 재사용하고, 캐릭터성이 필요한 모션만 별도 제작한다.
- 현재 Gojo에서 만든 Blender → Humanoid → Unity 경험을 이후 캐릭터에 재사용한다.

## 중요한 현실 인식

이 파이프라인은 "캐릭터 전용 평타를 자동으로 없애는 시스템"이 아니다.

Gojo의 주먹 콤보, Sukuna의 공격 자세, Yuta의 검술은 최종적으로 서로 다른 모션이 필요할 수 있다. 줄이는 것은 **0에서 다시 만드는 양**이다.

권장 구조:

```text
공통 locomotion / hit / knockdown / recovery
        ↓
전투 archetype base motion
  ├─ Fist Base
  └─ Sword Base
        ↓
캐릭터 전용 변형
  ├─ Gojo
  ├─ Sukuna
  ├─ Yuta
  ├─ Megumi
  └─ Choso
        ↓
Hero / cinematic animation
```

## 1. 모델 소스 전략

### 장기 원칙

랜덤한 게임 추출 모델을 캐릭터마다 하나씩 구해서 그대로 최종 자산으로 쓰는 방식을 표준 파이프라인으로 만들지 않는다.

이유:
- 얼굴/신체 비율과 topology가 제각각이다.
- material/shader 품질이 다르다.
- skeleton/weight가 다르다.
- 스타일 통일 비용이 캐릭터가 늘수록 커진다.
- 공개 배포 시 라이선스/사용권 문제가 생길 수 있다.

현재 Gojo 모델은 **프로토타입/현재 작업 자산으로 보존**한다. 이 문서 때문에 교체하지 않는다.

### 무료 기반 후보

1. **VRoid Studio**
   - 인간형 anime base를 빠르게 만들기 위한 후보.
   - 그대로 최종 사용하기보다 Blender에서 얼굴, 머리, 옷, 체형을 수정하는 출발점으로 검토한다.
2. **Blender**
   - 최종 수정, 머리/의상/소품, topology 정리, weight, shape key, animation 편집의 기준 툴.
3. **무료 배포 모델**
   - 라이선스가 명확한 경우에만 사용한다.
   - 완성 캐릭터를 그대로 끼우기보다 머리/의상/소품 참고 또는 파이프라인 검증용으로 본다.

### Human Base 목표

가능하면 장기적으로 하나의 `JJK_Human_Base` 계열을 만든다.

```text
JJK_Human_Base
├─ Gojo-derived human
├─ Sukuna
├─ Yuta
├─ Megumi
└─ Choso
```

완전히 동일한 mesh여야 하는 것은 아니다. 핵심은 아래 규칙을 공유하는 것이다.

- 비슷한 머리:몸 비율
- 어깨/손/다리 크기 규칙
- 얼굴 눈/코/턱 비율 범위
- 동일 toon/shadow/outline 계열
- 피부/머리/옷 material 규칙
- 동일 Humanoid bone naming/mapping 목표

Mahoraga처럼 체형과 움직임이 크게 다른 캐릭터는 `Monster / Large Humanoid` 별도 pipeline으로 분리한다.

## 2. Skeleton / Rig 규격

### Canonical target

Unity **Humanoid**를 인간형 캐릭터의 공통 호환 규격으로 사용한다.

목표:
- Idle/Walk/Run/Hit/Dodge 등 공유 가능한 clip을 retarget한다.
- Animator state machine을 캐릭터마다 처음부터 다시 만들지 않는다.
- Blender나 외부 mocap에서 들어온 clip도 같은 규격으로 정리한다.

캐릭터별 FBX가 skeleton 구조까지 완전히 같을 필요는 없지만 Unity Humanoid mapping이 안정적으로 성립해야 한다.

### Rigging 자동화 후보

- Mixamo auto-rig / motion library
- 무료 auto-rig 도구가 필요할 때 AccuRIG 계열 검토

단, auto-rig 결과는 최종 품질 보장이 아니다. 얼굴/손/어깨/무릎 weight와 무기 손잡이 접촉은 Blender/Unity에서 보정할 수 있다.

## 3. Animation을 4단계로 분류

### A. 전 캐릭터 공용

캐릭터 정체성이 약한 것부터 공유한다.

- 기본 locomotion
- jump / landing
- hit reaction
- knockdown / get-up
- generic dodge
- turn / locomotion transition
- 필요 시 death / stun

### B. 전투 archetype 공용 base

#### Fist Base

- combat idle base
- jab
- straight
- hook
- kick
- dash attack base

#### Sword Base

- sword idle base
- left/right slash
- thrust
- overhead
- dash slash base

이것들은 최종 캐릭터 모션이 아니라 **수정 가능한 출발점**이다.

### C. 캐릭터 전용 combat clip

캐릭터 정체성이 강하게 보여야 하는 부분.

예:
- Gojo 기본 콤보
- Sukuna의 공격적인 자세/타격
- Yuta의 검 3~4타 콤보
- Megumi의 식신 연계 자세
- Choso의 혈액술 연계

제작 흐름:

```text
Base clip
→ 캐릭터 Humanoid에 retarget
→ 핵심 key pose 수정
→ 상체/골반/발 timing 수정
→ 손/무기 궤도 수정
→ hit frame 확정
→ Unity에서 gameplay/VFX/audio sync
```

즉 캐릭터별 평타는 필요하지만 매번 skeleton부터 전체 키프레임을 새로 찍는 방식은 피한다.

### D. Hero / cinematic animation

비용이 높은 특수 장면은 캐릭터별 핵심 순간에만 집중한다.

예:
- Gojo: 안대를 실제로 내리고 Six Eyes를 드러내는 각성
- Sukuna: 대표적인 영역/궁극 연출
- Yuta: Rika와의 대표 연계

모든 평타를 cinematic quality로 만들지 않는다.

## 4. 무료 모션 확보 / 변형 흐름

무료 기반 우선순위:

1. Mixamo 등에서 가까운 base motion 확보
2. 공개 mocap dataset을 base로 활용
3. 직접 촬영한 동작을 무료/무료-tier video mocap으로 rough motion 생성 가능
4. Blender에서 최종 수정

AI/video mocap은 완성 자산이 아니라 **초안 생성기**로 취급한다.

주요 보정 대상:
- foot sliding
- hand/weapon contact
- shoulder deformation
- 과한 hip motion
- attack anticipation / hit timing
- 캐릭터 고유 실루엣

## 5. Blender 편집 방식

모션을 항상 처음부터 새 Action으로 만들기보다 기존 Action을 복제/변형한다.

활용 후보:
- Action 복제
- NLA strip
- key pose 수정
- additive/upper-body 보정
- IK/FK 전환
- 손/발 접촉 보정

예:

```text
Sword_Base_Slash01
→ Yuta_Slash01
→ 허리 더 낮춤
→ 시작 anticipation 변경
→ 검 궤도 변경
→ 왼손 위치 보정
→ recovery timing 변경
```

## 6. Unity Animator 구조

목표는 캐릭터마다 Animator 전체를 복제해 관리하지 않는 것이다.

```text
JJK_BaseAnimator
├─ Locomotion
├─ Hit / Down
├─ Dodge
├─ Attack1
├─ Attack2
├─ Attack3
├─ SkillQ
├─ SkillE
├─ SkillR
└─ Domain / Special
```

후보 구조:
- `AnimatorOverrideController`로 캐릭터별 clip 교체
- Avatar Mask / Layer로 상체 전투 자세와 공통 하체 locomotion을 조합
- Unity Animation Rigging으로 hand/weapon/aim contact 보정

실제 구현 전 현재 Animator와 충돌 여부를 확인해야 하며, 기존 USER VERIFIED animation 설정을 임의 교체하지 않는다.

## 7. 무기 캐릭터

무기는 skeleton 전체를 새로 만드는 이유가 아니다.

기본 구조 후보:

```text
RightHand
└─ WeaponSocket
   └─ Katana / Weapon
```

필요 시:
- off-hand target
- two-hand IK
- weapon trail socket
- hitbox socket
- VFX socket

Yuta는 Human pipeline + Sword archetype의 첫 검증 캐릭터 후보로 적합하다.

## 8. 스타일 통일 규칙 — Character Style Bible

모델 출처가 달라도 아래 항목을 통일해 같은 게임의 캐릭터처럼 보이게 한다.

### Proportion
- head/body ratio
- shoulder width
- hand size
- leg length

### Face
- eye scale/range
- nose depth
- jaw silhouette
- eyebrow/eye expression style

### Material
- skin shading
- hair highlight
- cloth roughness
- metallic/accessory handling

### Rendering
- toon shadow steps
- outline thickness/rule
- rim light rule
- common lighting response

### Texture
- 비슷한 texel density
- 얼굴 shading 방식 통일
- 색 saturation/value 범위 관리

최종적으로 "모델 출처가 같아서"가 아니라 **게임의 rendering/style rule이 같아서** 통일되어 보여야 한다.

## 9. Gojo 이후 첫 파이프라인 검증

Gojo 완료 직후 곧바로 Yuta 전체를 제작하지 않는다.

먼저 `Yuta Dummy` 또는 별도 human dummy로 아래만 증명한다.

1. 다른 human model 준비
2. Unity Humanoid mapping
3. 기존 공용 Idle/Walk/Run retarget
4. Hit/Dodge 공용 clip 적용
5. Sword socket 연결
6. Sword Base 1개 적용
7. 캐릭터별 clip override 가능 여부 확인
8. 동일 toon/rendering rule 적용

이 실험이 성공하면 "새 캐릭터 추가"가 Gojo 제작 전체를 다시 반복하는 일이 아님을 확인할 수 있다.

## 10. 현재 예상 archetype

### Human / hand combat 중심
- Gojo
- Sukuna
- Megumi
- Choso

공유 가능:
- human base
- humanoid rig
- locomotion
- hit/down
- 일부 fist base

### Human / weapon 중심
- Yuta

공유 가능:
- human base
- humanoid rig
- locomotion/hit
- 별도 Sword Base

### Large / monster
- Mahoraga

별도 체형/rig/motion 계열이 필요할 가능성이 높다. 그러나 gameplay/presentation/data 시스템은 공통 구조를 최대한 재사용한다.

## 11. 각 캐릭터 추가 체크리스트

```text
[Model]
□ 사용권/출처 확인
□ 스타일 비율 보정
□ 공통 shader/material 규칙 적용
□ Humanoid mapping
□ skin/weight 확인

[Shared Animation]
□ Idle/Walk/Run
□ Hit/Down/GetUp
□ Dodge

[Archetype]
□ Fist 또는 Sword base 적용
□ weapon/socket/IK 필요 여부

[Character Identity]
□ 전용 combat idle
□ 기본 콤보 핵심 clip
□ Q/E/R/V 등 기술 clip
□ 대표 Hero animation

[Presentation]
□ VFX socket
□ Camera ownership
□ Audio/voice beat
□ hit-stop / screen FX

[QA]
□ clipping
□ foot sliding
□ weapon contact
□ retarget regression
□ character select / VFXLab compatibility
```

## 12. 지금 하지 않을 것

- Purple 작업 도중 Human Base를 새로 만들지 않는다.
- 현재 Gojo FBX/Animator/Scene을 이 계획 때문에 교체하지 않는다.
- 모든 캐릭터의 모델을 미리 수집하지 않는다.
- 모든 캐릭터용 평타를 한 번에 제작하지 않는다.
- AI 생성 3D 모델을 clean topology/rig 검증 없이 production에 넣지 않는다.
- 라이선스 불명확한 추출 모델을 장기 배포 전략의 전제로 삼지 않는다.

## 결정 요약

게임을 2D로 바꾸는 것이 아니라 **3D 캐릭터 제작 비용을 파이프라인으로 낮춘다.**

핵심은:

> Shared Human Base + Unity Humanoid + Shared Motion Library + Fist/Sword Base + Character-specific edits + a small number of Hero animations

현재 단계에서는 계획만 보존한다. 실제 Character Production Pipeline 구현은 Gojo VFX/전투 정리가 충분히 끝난 뒤 별도 Change Contract로 시작한다.
