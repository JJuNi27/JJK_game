# Unity 전투 MVP

이 폴더는 Python/Pygame 장인 연습장에서 검증한 입력 규칙을 실제 전투로 옮기기 위한 Unity 코드입니다.

현재 `Assets/`만 저장소에서 관리합니다. Unity가 자동 생성하는 `Library`, `Temp`, `Logs` 등은 Git에 올리지 않습니다.

## 권장 에디터

- Unity 6.5 Supported (`6000.5.5f1` 또는 최신 6.5 패치)
- 템플릿: 3D Core
- 첫 MVP는 외부 에셋과 추가 패키지 없이 진행

Unity 6의 Supported 릴리스는 LTS와 같은 수준의 QA를 거친 정식 제작용 버전입니다. 이 프로젝트는 새로 시작하는 단계이고 오래된 플러그인 호환성 제약이 없으므로, 최신 기능·플랫폼 지원·성능 개선을 받을 수 있는 6.5를 기준으로 사용합니다.

## 최초 프로젝트 준비

현재 저장소의 `unity/Assets`를 유지하면서 Unity 프로젝트 메타데이터를 만드는 절차입니다.

1. Unity Hub에서 Unity 6.5의 `3D Core` 프로젝트를 임시 위치에 생성합니다.
2. 에디터가 열린 뒤 닫습니다.
3. 임시 프로젝트의 `Packages/`와 `ProjectSettings/` 폴더를 이 저장소의 `unity/` 안으로 복사합니다.
4. Unity Hub에서 저장소의 `unity/` 폴더를 프로젝트로 추가해 엽니다.
5. 스크립트 컴파일이 끝나면 상단 메뉴에서 아래 항목을 실행합니다.

```text
Tools
→ JJK Game
→ Build Combat MVP Scene
```

자동으로 `Assets/Scenes/CombatMVP.unity`가 생성되고 빌드 목록에 등록됩니다.

## 자동 생성되는 장면

```text
ArenaGround
GojoPlayer
├─ CharacterController
├─ Health
├─ ThirdPersonPlayerController
├─ BasicAttack
├─ GojoDomainController
└─ AttackOrigin

CurseBot
├─ CharacterController
├─ Health
└─ CurseBotController

Main Camera
└─ SimpleCameraFollow

Directional Light
MatchController
```

## 조작

```text
WASD: 이동
좌클릭: 기본 공격
V: 영역전개 준비
우클릭 유지 → 좌클릭 → 목표 타이밍에 우클릭 해제: 무량공처
R: 영역 입력 초기화
ENTER: 승패 결정 뒤 장면 재시작
```

무량공처가 성공하면 영역 반경 안의 `IDomainStunnable` 대상이 일정 시간 행동 불능 상태가 됩니다. 현재 주령 봇이 이 인터페이스를 구현합니다.

## 현재 성공 기준

- 플레이어가 이동할 수 있음
- 좌클릭으로 주령에게 피해를 줌
- 주령이 플레이어를 추적하고 근접 공격함
- 어느 한쪽의 체력이 0이 되면 승리 또는 패배 표시
- 고죠 장인 입력 성공 시 주령이 일시 정지
- 장인 입력 중 좌클릭이 기본 공격으로 중복 처리되지 않음

## 폴더 역할

```text
Assets/Scripts/Core/
- 체력, 승패, 공통 인터페이스

Assets/Scripts/Player/
- 이동, 기본 공격, 고죠 영역 입력

Assets/Scripts/Enemy/
- 단순 주령 상태 머신

Assets/Scripts/Camera/
- 플레이어 추적 카메라

Assets/Editor/
- MVP 장면 자동 생성 도구
```

## 아직 포함하지 않는 기능

- 음성인식 연결
- 캐릭터 모델과 애니메이션
- 창·혁·자
- 정교한 카메라 조작
- 영역 시각 효과
- 여러 주령과 경로 탐색
- 캐릭터 선택

먼저 캡슐 형태로 전투 루프를 검증한 뒤 단계적으로 추가합니다.
