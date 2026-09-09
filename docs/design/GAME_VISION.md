# JJK_game — 게임 방향 / 장기 비전

상태: **장기 설계 기준**

## 한 문장 정의
**주술회전 팬텀 퍼레이드의 화려한 기술 연출과 원작의 술식/영역/패시브 규칙을 3D Unity에서 최대한 고증하여 구현하는, 넓은 도시형 전투 맵 기반 1:1 캐릭터 액션 격투게임.**

## 핵심 목표

### 1. 원작 고증 우선
게임상 불가피한 예외를 제외하고:
- 만화 설정
- 애니 연출
- 팬텀 퍼레이드의 기술 표현
을 가능한 한 충실하게 반영한다.

단순히 기술 이름/색만 재현하지 않고:
- 발동 조건
- 대상 판정
- 상성
- 영역 규칙
- 패시브
- 특수 체질
까지 gameplay rule로 시스템화한다.

### 2. 1:1 중심
초기 핵심 전투는 1:1.

지원 우선순위:
1. Training
2. VS Bot
3. Local/Online 1v1
4. 추후 Custom / Ranked

2v2 / 3v3 / tag battle은 현재 보류.
코어 1v1이 안정된 뒤 재검토.

### 3. 넓은 도시형 전투 맵
작은 링형 격투장이 아니라:
- 신주쿠
- 시부야
- 도쿄형 도시 공간
처럼 넓은 맵에서 싸우는 구조를 지향.

기술과 환경 상호작용:
- Red가 적을 날림
- projectile이 건물/오브젝트와 충돌
- Purple이 경로를 찢거나 scar를 남김
- 참격/폭발/영역이 환경과 상호작용

환경 파괴는 단계적으로 구현하며, 초기에는 제한된 destructible set부터 시작한다.

### 4. 캐릭터별 강한 정체성
모든 캐릭터를 같은 공통 스킬 템플릿에 억지로 맞추지 않는다.
공통 시스템은 재사용하되 캐릭터별 원작 특성을 충분히 표현할 수 있어야 한다.

예:
- Gojo: Six Eyes / Infinity / RCT / Limitless / Unlimited Void
- Sukuna: Shrine / RCT / Open Barrier Domain
- Yuta: Rika / Copy / RCT / massive cursed energy
- Maki / Toji: Zero cursed energy Heavenly Restriction
- Mahoraga: adaptation to phenomena

## 원작 규칙을 시스템으로 만들기

### Trait / 상태 기반
캐릭터 이름 하드코딩을 피한다.

예:
- `CursedEnergyPresence.Normal`
- `CursedEnergyPresence.Low`
- `CursedEnergyPresence.Zero`
- `HeavenlyRestriction`
- `CanUseRCT`
- `OpenBarrierDomain`
- `AdaptivePhenomenon`

### Domain
Domain은 단순 Ultimate가 아니라 별도 규칙 시스템.

후보 데이터:
- Refinement
- Output
- Activation Speed
- Barrier Type
- Range
- Stability
- Sure-Hit Type
- Target Policy

### Domain Clash
동시 또는 근접 타이밍의 Domain 전개 시:
- 기본 Domain 성능
- 현재 cursed energy/output
- cast timing
- player clash input
등을 조합하는 구조를 지향.

단순 "버튼 먼저 누른 쪽 승리" 또는 "숫자 하나 큰 쪽 승리"만으로 제한하지 않는다.

### Heavenly Restriction / Zero CE
"Toji인가?" 같은 이름 체크 대신 Trait/Target Policy로 처리.

Domain별로:
- Zero CE target을 인식하는가
- trap 가능한가
- physical matter를 대상으로 하는가
를 독립 규칙으로 정의할 수 있어야 한다.

### RCT
RCT 가능 캐릭터는 공통 passive/ability framework를 사용.

후보 튜닝:
- Heal Rate
- CE Cost
- Startup Delay
- Combat Exit Delay
- Can Heal Others
- 특수 재생 조건

### Mahoraga Adaptation
특정 기술 ID만 외우는 방식이 아니라 가능하면 **현상/계열 태그**에 적응.

예:
- Technique
- Damage Type
- Phenomenon
- Exposure
- Adaptation Progress
- Resistance
- Fully Adapted

## Data-driven 원칙
캐릭터/기술 밸런스는 가능한 한 코드 하드코딩이 아니라 Inspector/Data에서 조절.

장기 후보:
- CharacterDefinition
- AbilityDefinition
- PassiveDefinition
- DomainDefinition
- TraitDefinition
- DamageProfile
- TargetingProfile

## 현재 개발 전략
지금은 Gojo를 첫 production-quality 기준 캐릭터로 완성한다.
Gojo에서 검증된 구조를 일반화하여 이후 캐릭터에 확장한다.

초기부터 10개 캐릭터를 동시에 만들지 않는다.
한 캐릭터를 깊게 완성하고 공통 시스템을 추출한다.
