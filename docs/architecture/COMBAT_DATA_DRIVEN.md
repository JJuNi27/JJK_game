# Combat Data-driven / Inspector 설계

상태: **장기 아키텍처 기준 — 실제 migration은 CURRENT_TASK 승인 범위에서 단계적으로 수행**

## 목표
사용자가 C# 코드를 열지 않고도 주요 전투/기술 수치를 Unity Inspector 또는 Data Asset에서 직접 조절할 수 있게 한다.

예:
- Purple 데미지 20 → 40 → 60
- Red 투사체 속도 증가
- Domain 포획 반경 변경
- VFX 크기/강도 변경

코드를 수정하고 재컴파일하지 않고 값을 바꾸는 것이 목표.

## 사용자-facing 표시 언어
사용자가 조절하는 Inspector/Data UI는 가능하면 **한글**로 표시한다.

원칙:
- C# 변수/클래스/enum 식별자: 영어
- Inspector Header: 한글
- Tooltip: 한글
- Custom Inspector를 만들 경우 Label: 한글
- 파일/클래스 이름은 영어 유지

예:
```csharp
[SerializeField]
[Header("피해량")]
[Tooltip("기술이 1회 적중했을 때 주는 기본 피해량")]
private float baseDamage = 20f;
```

과도한 custom editor는 피하고, 기본 Inspector + Header/Tooltip만으로 충분하면 그 방식을 우선한다.

---

## 공통 Ability Data 후보

### 피해량
- **기본 피해량** — Base Damage
- **다단 히트 피해량** — Multi-hit Damage
- **가드 피해량** — Guard Damage
- **가드 관통 피해량** — Chip Damage
- **환경 피해량** — Environmental Damage
- **최소/최대 피해량** — 필요 기술에서만
- **피해 배율** — 상황별 multiplier가 필요한 경우

### 사거리 / 판정
- **시전 가능 거리** — Cast Range
- **적중 반경** — Hit Radius
- **폭발 반경** — Explosion Radius
- **최대 이동 거리** — Max Travel Distance
- **판정 폭/높이** — Width / Height
- **추적 거리** — Homing/Target Range가 있는 기술만

### 투사체
- **투사체 속도** — Projectile Speed
- **가속도** — Acceleration
- **수명** — Lifetime
- **관통 횟수/정책** — Penetration
- **중력 영향** — Gravity Scale가 필요한 경우
- **회전/유도 강도** — Homing Strength가 필요한 경우

### 전투 타이밍
- **재사용 대기시간** — Cooldown
- **발동 준비시간** — Startup
- **활성 시간** — Active Time
- **후딜레이** — Recovery
- **경직 시간** — Hit Stun
- **가드 경직 시간** — Block Stun
- **무적 시작/종료 구간** — 필요한 기술만
- **캔슬 가능 시점** — 필요한 기술만

### 힘 / 물리 반응
- **넉백 힘** — Knockback
- **띄우기 힘** — Launch Force
- **슈퍼아머** — Super Armor
- **끌어당김 힘** — Pull Force
- **밀어냄 힘** — Push Force
- **낙하/바운드 강도** — 필요한 기술만

### 자원
- **주력 소모량** — Cursed Energy Cost
- **영역 소모량** — Domain Cost
- **반전술식 소모량** — RCT Cost
- **유지 소모량/초** — Channel/Drain Cost
- **최소 필요 주력** — Minimum Required CE

### 타게팅 / 충돌 반응
- **적 반응**
- **캐릭터 반응**
- **파괴 가능 오브젝트 반응**
- **관통 가능 건물 반응**
- **절대 관통 불가 월드 반응**
- **Trigger 무시 여부**
- **최대 사거리 종료 반응**

각 대상에 대해:
- Ignore
- HitAndStop
- HitAndContinue
- DamageOnly
- Destroy/BreakEvent
등으로 확장 가능하게 설계.

### VFX
- **시각 크기** — Visual Scale
- **충돌 이펙트 크기** — Impact Scale
- **이펙트 지속시간** — VFX Lifetime
- **이펙트 강도** — Intensity
- **Emission/Glow 강도**
- **잔상 크기/폭**
- **잔상 지속시간**
- **파티클 양**
- **왜곡 강도**
- **빛 강도**

VFX parameter는 gameplay damage/radius와 필요 이상으로 강결합하지 않는다.
다만 Purple scar처럼 명백히 비례해야 하는 값은 multiplier로 연결 가능.

### 카메라
- **카메라 흔들림 강도** — Shake Strength
- **카메라 흔들림 시간** — Shake Duration
- **FOV 변화량** — FOV Kick
- **FOV 복원 시간**
- **Hit Stop**
- **카메라 줌/거리**
- **시네마틱 전환 시간**

### 오디오
- **볼륨** — Volume
- **재생 타이밍 오프셋** — Timing Offset
- **피치** — Pitch
- **랜덤 피치 범위**
- **루프 여부**
- **Fade In/Out**
- **Beat Event 연결**

---

## Domain Data 후보

### 기본
- **포획 반경**
- **시각 결계 크기**
- **내부 공간 크기**
- **영역 유지시간**
- **발동 준비시간**
- **주력 소모량**

### 고증 / Clash
- **영역 정교함** — Refinement
- **영역 출력** — Output
- **발동 속도** — Activation Speed
- **결계 유형** — Closed / Open / Incomplete / Special
- **안정성** — Stability
- **필중 유형** — Sure-Hit Type
- **대상 정책** — Target Policy
- **Zero CE 인식 여부**
- **물리 물체 대상 여부**
- **외부 공격 취약성**
- **내부/외부 결계 강도**

현재 Gojo Domain의 capture radius / visual barrier / interior size 분리는 반드시 유지.

---

## Character / Passive Data 후보

### CharacterDefinition
- 체력
- 이동속도
- 대시/회피
- 주력 최대치
- 주력 회복
- 기본 물리 공격력
- 방어/경직 저항
- Trait 목록
- Passive 목록
- Ability loadout
- DomainDefinition

### PassiveDefinition
- 발동 조건
- 내부 cooldown
- 자원 조건
- 상태 조건
- stack/progress
- VFX/SFX hook

### Trait 예
- Zero Cursed Energy
- Heavenly Restriction
- Can Use RCT
- Six Eyes
- Open Barrier Domain
- Adaptive Phenomenon
- Cursed Spirit
- Special Physiology

---

## 단계적 migration 원칙

### 1단계
현재 Gojo의 기존 hardcoded numeric value를 조사하고 목록화.
동작은 바꾸지 않는다.

### 2단계
공통적이고 안전한 값부터 Data Asset로 이동:
- damage
- range
- speed
- cooldown
- hit radius
- resource cost
- selected VFX tuning

### 3단계
Red / Purple / Domain처럼 기술별 특수 parameter를 전용 subsection/profile로 이동.

### 4단계
Trait / Passive / Domain policy framework를 추가.

### 5단계
새 캐릭터를 추가하면서 framework가 실제로 재사용 가능한지 검증.

## 금지
- 한 번에 모든 Player/Technique 코드를 대규모 재작성
- 현재 USER VERIFIED Blue/Movement를 migration 과정에서 시각/동작 변경
- 데이터화와 VFX polish를 한 패스에서 광범위하게 섞기
- 캐릭터 이름으로 고증 rule 하드코딩
- Inspector 한글화를 위해 C# 내부 변수명을 한글로 변경
