# 고죠 VFX 아키텍처 / 현재 기준

이 문서는 과거 대화를 매번 다시 설명하지 않도록 현재 받아들여진 기준을 기록한다.

## 전체 비주얼 철학
이 프로젝트는 의도적으로 화려함, 아우라, 강한 타격감, 읽기 쉬운 과장된 VFX를 중요하게 본다.
캐릭터와 gameplay 가독성이 유지되는 한, 효과가 강해 보인다는 이유만으로 보수적으로 약화하지 않는다.

---

## Blue / 아오

### 상태
**USER VERIFIED — 1차 완성**

새로운 사용자 아이디어가 생기기 전까지 수정하지 않는다.

### 반드시 보존
- 기존 4-hit gameplay
- 현재 attraction / singularity 방향
- 현재 debris 스타일
- 현재 pre-cast / active presentation

다른 기술 작업 때문에 Blue를 함께 리팩터링하거나 시각적으로 변경하지 않는다.

---

## Red / 아카

### 현재 사용자 피드백
- 적에게 이미 충돌하여 폭발했는데도 빨간 projectile visual이 계속 전진하는 문제가 있음.
- impact 후 smoke / vapor가 여전히 약함.
- 미래에는 도쿄 도시형 넓은 맵을 만들 예정이므로, 단순 "적만 충돌" 설계로 고정하면 확장성이 부족함.

### 권장 Collision Response 모델
"무엇과 충돌했는가"에 따라 반응을 분리한다.

1. **Enemy / Character**
   - hit / damage / impact
   - 강한 Red 폭발 / 압력 잔향
   - projectile 종료

2. **Destructible World / Building**
   - 경로상의 건물/오브젝트에 파괴/충격 이벤트 전달 가능
   - Red는 계속 통과할 수 있도록 설계 가능
   - 미래 도시 파괴 시스템과 연결할 수 있게 coupling을 약하게 유지

3. **Hard World Solid / Non-penetrable**
   - impact
   - projectile 종료

4. **Trigger / Ignore**
   - 통과

5. **Max Range / Lifetime**
   - 자연 소멸

핵심:
**"Enemy only" 또는 "모든 오브젝트에 막힘" 중 하나로 고정하지 않는다.**
대상별 Collision Response가 미래 확장에 유리하다.

### Impact 연기 / 압력 잔향
불꽃 폭발 연기가 아니라:
- 충격 중심에서 방사형 / 수평으로 밀려나는 회백색 air / dust / vapor
- 더 넓은 volume
- 더 강한 initial burst
- 현재보다 조금 더 오래 남는 residual pressure
- 둥근 cartoon smoke puff보다 찢기고 밀려나는 형태

목표:
**맞는 순간 projectile은 명확히 끝나고, 그 자리에 거대한 척력의 압력이 남는다.**

### 기존 보존
- charge 1회 생성
- release 시 charge cleanup
- repeat cast leak 없음
- Red는 fireball이 아니라 repulsion

---

## Hollow Purple

### 매우 강하게 보존
현재 Blue+Red formation / fusion 기반은 프로젝트에서 가장 성공적인 효과 중 하나다.
광범위하게 재설계하지 않는다.

### 현재 문제 1 — 완성된 Purple 위치
사용자가 "Gojo를 가린다"고 한 피드백을 lateral offset으로 해결하면서,
완성된 Purple이 Gojo 기준 오른쪽에 생기게 됨.

이 해결법은 잘못됨.

### 위치 원칙
- Purple은 **Gojo의 정중앙 축에서 완성**
- local lateral offset: **X = 0 원칙**
- caster 가림 문제는 옆으로 빼지 말고:
  - forward offset
  - 필요 시 약간의 vertical offset
  - hold scale / camera framing
  로 해결

목표:
**중앙에서 태어나되 Gojo의 silhouette는 읽힌다.**

### 현재 문제 2 — Purple scar 폭
사용자 체감상 바닥 잔상의 가로폭이 다시 좁아짐.

권장:
- scar width를 별도 임의 숫자로만 관리하지 말고
- 가능하면 **Purple actual visual diameter × multiplier** 방식으로 연결
- Purple 크기가 바뀌어도 scar가 혼자 가늘어지는 regression 방지

### 보존
- current fusion
- 0.32 s hold는 현재 유지
- 현재 장거리 travel 방향
- 현재 branching lightning / distortion 방향

목표:
**Purple이 단순히 맵을 통과하는 것이 아니라, 지나가는 공간 자체를 강제로 왜곡하고 상처 내는 느낌.**
