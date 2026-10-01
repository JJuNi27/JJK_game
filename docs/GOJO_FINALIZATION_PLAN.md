# Gojo Finalization Plan

Status: **FUTURE PLAN / NOT IMPLEMENTED**

이 문서는 현재 Hollow Purple 작업 이후 Gojo를 첫 production-quality 캐릭터로 완성하기 위한 큰 방향을 보존한다. 현재 Purple 후보를 USER VERIFIED로 승격하거나 바로 각성 구현을 시작한다는 뜻이 아니다.

## 목표

Gojo를 단순히 기술이 몇 개 있는 테스트 캐릭터가 아니라 다음 요소가 연결된 하나의 완성 캐릭터로 만든다.

- 기술별 최종 VFX
- 타격감
- 카메라/화면 연출
- 음성/사운드
- 캐릭터 전용 모션
- 각성 전/후 상태 변화
- Six Eyes / blindfold presentation

## Phase 1 — Gojo Combat & VFX Finalization

### 1. Hollow Purple

현재 최우선 작업.

현재 checkpoint:
- branch: `feat/gojo-blue-screen-distortion`
- Purple code checkpoint: `22cea8ed60ecafdf83ccc76860cd6bd87f7c4df4`
- latest candidate: `PurpleTempestPolish`
- saved default OFF
- **CODEX VALIDATED / PENDING USER VISUAL REVIEW / NOT USER VERIFIED**

현재 사용자 visual 판단:
- 전체 방향은 이전보다 CLOSER
- sphere/body는 KEEP
- neon discharge는 KEEP + POLISH
- Charge/Travel wind 분리는 성과
- wind의 물성/공간감은 아직 부족
- dark cursed energy는 검은 국수 인상은 줄었지만 무게감도 너무 약해짐
- 승격/Production 전환은 아직 아님

자세한 현재 상태는:
- `docs/PURPLE_TEMPEST_POLISH_REVIEW.md`
- `docs/active/CURRENT_TASK.md`

### 2. Blue / Red 최종 리워크

Purple에서 확립한 시각 언어를 독립 Blue/Red에도 적용한다.

#### Blue
- inward suction / vortex / pressure
- cyan/blue identity
- sphere 자체보다 주변 공간이 끌려가는 느낌 강화
- 과도한 lightning-first 해석 금지
- 필요한 경우 환경 반응/왜곡/particles 재정리

#### Red
- outward pressure / repulsion / eruptive flow
- white circular core/sticker 인상 제거
- 전방 압력과 주변 공간 밀어냄을 우선
- 과도한 plastic shard / clean ribbon 인상 방지

Blue와 Red는 Purple 재료 역할뿐 아니라 독립 Q/E 기술로도 읽혀야 한다.

### 3. Skill Presentation Final Pass

각 기술을 `발사 → 폭발`만으로 끝내지 않는다.

검토 항목:
- anticipation
- charge / formation
- release
- travel
- impact
- environment reaction
- aftermath
- camera/screen feedback
- audio/voice beat

현재 `ConvergenceDiagnostic` 계열에서 확인한 flash/hit-stop/camera/bloom/exposure 경험을 필요한 기술에 선택적으로 적용한다. 화면 효과로 본체 VFX 약점을 가리지 않는다.

### 4. Audio / Voice

추후 실제 음성 asset을 넣을 때:
- 기술 발동 대사
- 상황별 짧은 캐릭터 대사
- impact / charge / travel SFX beat
- animation과 voice timing 동기화

저작권/사용권이 불명확한 음성 파일을 repository에 임의 추가하지 않는다.

## Phase 2 — Gojo Awakening / Six Eyes

### 핵심 컨셉

기본 Gojo의 장난스럽고 여유로운 분위기에서 벗어나 진지한 전투 상태로 전환한다.

사용자 컨셉:

> "진심으로 가볼까"

이후 blindfold를 실제로 내리고 Six Eyes를 드러낸다.

### 입력 구조 계획

#### 기본 상태

```text
LMB : 기본 평타
Q   : Blue
E   : Red
R   : Awakening
V   : Unlimited Void
```

#### 각성 완료 후

```text
LMB : 각성 상태 기본 전투
Q   : Blue를 이용한 타격/콤보형 기술
E   : Red를 이용한 타격/콤보형 기술
R   : Hollow Purple
V   : Unlimited Void
```

세부 gameplay/balance는 아직 확정하지 않는다.

## Blindfold Lowering — Hero Animation 계획

목표는 단순 `blindfold mesh OFF → eyes ON`이 아니다.

사용자가 원하는 것은:
- 손이 얼굴로 올라간다.
- 손가락이 blindfold에 접촉한다.
- blindfold가 눈 위치에서 실제로 아래로 내려간다.
- 눈이 자연스럽게 드러난다.
- 내려간 blindfold가 턱/목 주변으로 이동한다.
- Six Eyes reveal과 카메라/음성/VFX가 이어진다.

### 현실적인 구현 목표

**완전한 cloth simulation은 기본 목표로 하지 않는다.**

권장 방식:
- blindfold를 별도 skinned mesh로 유지
- blindfold 전용 bone 또는 Blender shape key 사용
- 큰 이동은 bone/animation
- 얼굴 곡면/접힘 보정은 shape key
- 손 animation과 blindfold deformation을 같은 timeline에 맞춤
- 필요한 순간 camera framing과 손의 가림을 활용해 작은 clipping을 숨김

예상 단계:

```text
Idle
→ 손이 얼굴로 이동
→ blindfold edge contact
→ 눈 바로 아래로 pull
→ 코/볼 통과
→ 턱 아래/목 위치
→ Six Eyes reveal
→ awakening pose
```

### 피할 구현

초기 버전에서 아래까지 강제하지 않는다.
- 실제 cloth가 손가락에 물리적으로 잡힘
- 코/귀/머리카락과 완전한 cloth collision
- 실시간 천 folding simulation

이 수준은 비용이 급격히 커지며 게임 연출상 필수가 아니다.

## Awakening Presentation 후보

각성 순간 후보:
- close-up camera
- 표정/눈 reveal
- Six Eyes 전용 eye material/VFX
- 짧은 lighting 변화
- voice sync
- UI에서 R icon/name을 Awakening → Hollow Purple로 교체
- 각성 완료 상태를 명확하게 전달

카메라/화면 연출은 `docs/architecture/CAMERA_PRESENTATION.md` 규칙을 따른다.

## Motion 작업 원칙

Gojo의 모든 모션을 처음부터 cinematic quality로 만들지 않는다.

분류:

### Shared / reusable
- locomotion
- generic hit/down
- 일부 dodge/recovery

### Gojo combat-specific
- 평타 combo
- Blue/Red 응용 타격
- Purple cast/release
- Domain cast

### Hero animation
- blindfold lowering / Six Eyes awakening

Hero animation은 비싸도 괜찮지만 수를 제한한다.

캐릭터 생산 공통 전략은 `docs/architecture/CHARACTER_PRODUCTION_PIPELINE.md`를 따른다.

## 추천 작업 순서

```text
1. Current Purple refinement
2. Purple visual decision / usable baseline 확정
3. Blue final rework
4. Red final rework
5. Gojo combat presentation/audio polish
6. 공통 Character Production Pipeline prototype
7. Gojo 평타/전투 모션 정리
8. Awakening state/data/input 구조
9. Blindfold-lowering Hero Animation
10. Six Eyes presentation
11. Awakening Q/E melee applications
12. Awakening R = Hollow Purple 연결
13. 전체 Gojo regression / user Play Mode review
```

상황에 따라 6~8 순서는 바뀔 수 있지만 Purple 진행 중 blindfold/각성 작업을 동시에 벌리지 않는다.

## 완료 기준

Gojo를 `완성`이라고 부르기 위한 최종 체크는 최소:

- Blue/Red/Purple/Domain의 정체성이 명확함
- 각 기술의 impact/presentation이 서로 구분됨
- 이동/평타/스킬 transition이 부자연스럽지 않음
- audio/voice/camera가 주요 beat와 동기화됨
- 각성 전/후 R 동작이 명확함
- blindfold lowering이 실제 animation으로 읽힘
- Six Eyes reveal이 캐릭터의 Hero Moment로 작동함
- USER VERIFIED 설정 회귀 없음
- 사용자 Play Mode 최종 시각 승인

## 현재 결정

이 문서는 **future queue**다.

지금은 Hollow Purple의 다음 시각 refinement가 현재 작업이다. 각성/모델/모션 구현은 사용자가 명시적으로 다음 track으로 전환하기 전까지 시작하지 않는다.
