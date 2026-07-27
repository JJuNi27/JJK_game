# Unity 전투 MVP

Unity 6.3 LTS URP에서 고죠와 스쿠나의 전투 규칙을 검증하는 프로토타입입니다. 현재 장면은 런타임 자동 확장을 사용하므로 일반적인 코드 변경 뒤에는 장면을 다시 만들 필요가 없습니다.

## 프로젝트 위치

```text
D:\GitHub\JJK_game\unity
```

최초 장면 생성 메뉴:

```text
Tools → JJK Game → Build Combat MVP Scene
```

## 공통 조작

```text
WASD 이동
SPACE 회피
TAB 타깃 전환
LMB 3단 기본 공격
F1 조작 도움말
ENTER 승패 뒤 재시작

1 현대 고죠 선택 후 장면 재시작
2 시부야 스쿠나 선택 후 장면 재시작
```

### 현대 고죠

```text
Q 창
E 혁
R 허식 자
V 무량공처 입력
X 영역 입력 취소
```

### 시부야 스쿠나 Milestone 1

```text
Q 해
E 팔
R 푸가 · 현재 잠김
V 복마어주자 · 현재 잠김
```

`푸가`는 사용자가 글로벌 한국어 클라이언트 기준으로 검수한 UI 명칭입니다.

## 런타임 플레이어 구조

```text
Player
├─ CharacterController
├─ Health
├─ ThirdPersonPlayerController
├─ BasicAttack
├─ TargetLockController
├─ CursedEnergyController
├─ CombatActionGate
├─ PrototypeCombatAudio
├─ PrototypeCharacterController
│
├─ 고죠 선택 시
│  ├─ GojoVariantController
│  ├─ GojoTechniqueController
│  ├─ GojoTechniqueChainController
│  ├─ GojoDomainController
│  ├─ GojoInfinityDefense
│  ├─ TechniqueBurnoutController
│  └─ GojoPrototypeAvatar
│
└─ 스쿠나 선택 시
   ├─ SukunaTechniqueController
   └─ SukunaPrototypeAvatar
```

현재 캐릭터 전환은 완성형 선택 화면이 아니라 `1`·`2` 입력과 장면 재시작을 사용하는 최소 테스트 경로입니다.

## 공통 피해 구조

`DamageContext`가 피해량과 공격 성질을 함께 전달합니다.

```text
DamageDeliveryType
PhysicalStrike
CursedTechnique
DomainSureHit
Environmental

DamageTraits
DomainAmplification
TechniqueNullification
IgnoresInfinity
Unblockable
```

처리 순서:

```text
회피 무적
→ 활성화된 방어 판정
→ 실제 HP 감소
```

캐릭터 전환으로 비활성화된 무하한 같은 방어 컴포넌트는 피해 판정에서 제외됩니다.

## 현대 고죠

### 주력

```text
프로필 SIX EYES · 육안 효율
최대 100
Q/E/R/V 실제 소비 각각 1
초당 회복 12
```

### 기술

- Q 창: 락온 대상 근처 또는 정면에 지속 수렴장 생성
- E 혁: 이동·관통하는 반발 발사체
- R 허식 자: Q와 E 사용 자체로 준비
- V 무량공처: 장인 입력 성공 시 영역 전개
- 영역 종료 후 약 5초 술식 번아웃

### 무하한

- 일반 타격과 일반 술식 차단
- 영역전연, 영역 필중, 술식 무효화와 명시적 특수 우회는 통과
- 차단·우회 사유를 HUD로 표시

## 시부야 스쿠나 Milestone 1

내부 ID:

```text
SukunaShibuyaYujiBody
```

### 주력

```text
프로필 VAST RESERVE · 시부야 스쿠나
최대 160
시작 95
초당 회복 4
사용 후 회복 지연 1.2초
```

### Q · 해

```text
소비 15
쿨타임 약 3초
적 1명: 집중 4히트, 타격당 8
적 2명 이상: 전방 넓은 2히트, 타격당 10
```

락온 대상을 우선하고 `DamageContext.CursedTechnique`을 사용합니다.

### E · 팔

```text
소비 45
쿨타임 약 5초
사거리 약 3.4m
단일 대상 7히트, 타격당 8
마지막 타격 강화 넉백·경직
```

빗나가도 주력 소비와 팔 사용 기록은 남습니다.

### R · 푸가

Milestone 1에서는 발동하지 않습니다.

- 해와 팔 사용 여부를 기록
- 둘 다 사용하면 `푸가 준비 완료` 표시
- 원작 밑준비 조건과 영역 밖 단일 대상 제약을 다시 확인한 뒤 Milestone 2에서 구현

### V · 복마어주자

Milestone 3까지 잠겨 있습니다.

- 개방형 영역
- 반복 참격을 `DomainSureHit`으로 전달
- 무하한 우회 판정과 연결할 예정

## HUD와 외형

- 고죠와 스쿠나 선택에 따라 다른 플레이어 제목·주력 프로필·기술 패널 표시
- 스쿠나 선택 시 고죠 무하한·번아웃·영역 컴포넌트 비활성화
- 고죠: 흰 머리·안대·검은 복장의 Primitive 프록시
- 스쿠나: 분홍 머리·얼굴 문양·붉은 눈의 Primitive 프록시
- 둘 다 최종 모델이 아닌 캐릭터 식별용 외형

## 전투 환경

- 약 65×65 전투장
- 주령 두 마리
- TAB 락온
- 개별 HP와 전체 처치 승리
- 주령 A 일반 공격은 고죠 무하한에 막힘
- 주령 B 영역전연 테스트 공격은 고죠 무하한을 우회
- Y=-12 아래 낙사

## 사운드와 피드백

현재 합성 효과음:

- 기본 공격·3타 마무리
- 피격·회피
- 고죠 창·혁·허식 자·무량공처
- 스쿠나 해·팔은 임시 공용 참격·타격음
- 승리·패배

로컬 음원 교체 방법은 `docs/AUDIO_SETUP.md`를 따릅니다.

## 아직 없는 기능

- 푸가 실제 발동
- 복마어주자 실제 발동
- 완성형 캐릭터 선택 화면
- 최종 고죠·스쿠나 모델과 전용 애니메이션
- 완성형 참격·화염·영역 VFX
- 스쿠나 전용 보이스와 사운드 믹싱
- 회옥·옥절·신주쿠 고죠 버전
- 메구미 육체·헤이안 진신·세계참 스쿠나
- Python 음성인식과 Unity 전투 연결
