# 고죠 VFX 아키텍처 / 현재 기준

이 문서는 과거 대화를 매번 다시 설명하지 않도록 현재 받아들여진 기준을 기록한다.

## 전체 비주얼 철학
이 프로젝트는 의도적으로 화려함, 아우라, 강한 타격감, 읽기 쉬운 과장된 VFX를 중요하게 본다.
캐릭터와 gameplay 가독성이 유지되는 한, 효과가 강해 보인다는 이유만으로 보수적으로 약화하지 않는다.

---

## Blue / 아오

### 반드시 보존
- 기존 4-hit gameplay
- 현재 attraction / singularity 방향
- debris 컨셉

### 사용자 방향
- 고죠 앞에 의미 없이 보이는 작은 pre-cast 파란 장식은 제거
- 파편은 밝고 매끈한 얼음보다 어둡고 거친 돌 / 건물 잔해처럼 보이게
- 거대한 파편 몇 개의 지배력을 줄이고 중/소형 파편 다양성 증가
- core가 가려지지 않게 유지
- inward collapse / suction이 명확하게 읽히게

목표:
**주변 환경이 찢겨서 Blue 안으로 끌려 들어가는 느낌.**

---

## Red / 아카

### 반드시 보존
- charge는 1회 생성
- release 시 charge 제거
- 반복 사용 시 오래된 charge object가 남지 않음
- Red는 화염구가 아니라 반발/척력

### 현재 로컬 working tree의 Codex 구현 결과
- projectile: **26 m**
- 속도: **42 m/s**
- flash / shockwave 강화
- 바깥으로 찢겨 나가는 pressure residue 강화

이 값과 시각 품질은 아직 **사용자 시각 검토 대기**다.

목표:
**압축된 척력이 풀리며 공간이 모든 것을 밀어내고, 압력이 눈에 보이는 잔향을 남기는 느낌.**

---

## Hollow Purple

### 매우 강하게 보존
현재 Blue+Red formation / fusion 기반은 프로젝트에서 가장 성공적인 효과 중 하나다.
광범위하게 재설계하지 않는다.

### 현재 로컬 working tree의 Codex 구현 결과
- Fusion-complete hold: **0.32 s 유지**
- travel: **48 m / 1.6 s**
- caster가 덜 가려지도록 hold 위치 조정
- release 강화
- branching violet lightning 강화
- distortion wake 강화

기존 기준:
- Visual size: **1.65×**
- Gameplay hit radius: **3.2 m**
- Residual scar width: **4.5 m**
- Residual lifetime: **1.8 s**

위 시각 결과는 아직 **사용자 시각 검토 대기**다.

### Scar 소규모 polish
- 평평한 보라색 카펫처럼 보이지 않게 edge를 불규칙하게
- 값싼 범위에서 breakup / residue variation 추가 가능
- scar 시스템 전체 재작성 금지

목표:
**Purple이 단순히 맵을 통과하는 것이 아니라, 지나가는 공간 자체를 강제로 왜곡하고 상처 내는 느낌.**
