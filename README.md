# JJK Game

주술회전의 **장인(손동작), 술식 선언, 영역전개 연출**을 게임 입력 시스템으로 옮기는 개인 프로젝트입니다.

> 새 ChatGPT 대화나 새로운 개발 환경에서 이어서 작업할 때는 [`docs/HANDOFF.md`](docs/HANDOFF.md)를 먼저 읽어주세요.

## 현재 목표

첫 MVP는 고죠 사토루의 **무량공처 장인 커맨드 프로토타입**입니다.

1. `V` 키 또는 추후 음성인식으로 `DOMAIN_READY` 상태 진입
2. 한 손 마우스 장인 입력
   - 우클릭 유지
   - 제한 시간 안에 좌클릭 1회
   - 수렴 타이밍에 우클릭 해제
3. 성공/실패 판정
4. 성공 시 임시 무량공처 이펙트 실행

## 현재 실행 가능한 기능

- `NORMAL`, `DOMAIN_READY`, `WAIT_LEFT_CLICK`, `RELEASE_TIMING`, `DOMAIN_ACTIVE`, `FAILED` 상태 머신
- 마우스 장인 입력 성공/실패 판정
- 타이밍 연습 UI
- 성공 시 코드로 그린 임시 무량공처 이펙트
- 한국어 UI 및 실패 사유 표시
- `R` 키 초기화, `ESC` 종료

## 실행 방법

### 1. 저장소 내려받기

```bash
git clone https://github.com/JJuNi27/JJK_game.git
cd JJK_game
```

이미 내려받았다면 프로젝트 폴더에서 다음 명령을 실행합니다.

```bash
git pull origin master
```

### 2. 가상환경 만들기

Windows PowerShell:

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
```

PowerShell 실행 정책 때문에 활성화되지 않으면 VS Code 터미널 프로필을 `Command Prompt`로 바꾸고 다음을 실행합니다.

```bat
.venv\Scripts\activate.bat
```

### 3. 패키지 설치

```bash
python -m pip install -r requirements.txt
```

### 4. 실행

```bash
python main.py
```

## 프로젝트 방향

- 메인 전투 모드: 선택한 캐릭터의 술식과 영역만 사용
- 자유 연습장: 캐릭터 제한 없이 장인 커맨드와 이펙트 연습
- 향후 일반 술식, 음성인식, 영역 충돌, 술식 번아웃 추가
- 웹캠이 생기면 마우스 장인 입력을 실제 손동작 인식 모듈로 교체 가능하게 설계
- 장기적으로 Unity에서 봇과 싸우는 전투 게임으로 확장

## 목표 입력 모듈 구조

```text
DomainIntentTrigger
├─ KeyboardDomainTrigger
└─ VoiceDomainTrigger        # 추후

SealInput
├─ MouseSealInput
└─ CameraSealInput           # 추후

DomainController
EffectPlayer
```

## 1차 개발 순서

- [x] 프로젝트 실행 환경 구성
- [x] 상태 머신 구현
- [x] 마우스 장인 입력 판정
- [x] 자유 연습장 기본 UI
- [x] 임시 무량공처 이펙트 연결
- [x] 한국어 UI 적용
- [ ] 실제 플레이 후 타이밍 수치 조절
- [ ] 입력 모듈 인터페이스 분리
- [ ] 음성인식 모듈 추가
- [ ] 캐릭터 선택 시스템
- [ ] 카메라 장인 모듈 확장
- [ ] Unity 전투 프로토타입 이전

## 기술 스택

첫 프로토타입은 빠르게 검증하기 위해 **Python + Pygame**을 사용합니다. 입력과 이펙트 모듈을 단계적으로 분리해, 필요할 경우 이후 게임 엔진으로 옮길 수 있도록 구성합니다.
