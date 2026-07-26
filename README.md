# JJK Game

주술회전의 **술식 선언, 장인(손동작), 영역전개와 전투 연출**을 실제 입력 시스템으로 옮기는 개인 프로젝트입니다.

> 새 ChatGPT 대화나 새로운 개발 환경에서 이어서 작업할 때는 [`docs/HANDOFF.md`](docs/HANDOFF.md)를 먼저 읽어주세요.  
> 원작 구현 기준은 [`docs/CANON_SKILL_RULES.md`](docs/CANON_SKILL_RULES.md), Unity 실행과 현재 조작은 [`unity/README.md`](unity/README.md)에 정리되어 있습니다.

## 현재 개발 우선순위

```text
Unity 고죠 전투 프로토타입 안정화
→ 주력·행동 상태·술식 번아웃
→ 캐릭터 공통 전투 구조
→ 두 번째 캐릭터 Unity 구현
```

현재는 모델과 완성형 VFX보다 캡슐 형태의 전투 루프와 원작 기술 조건을 먼저 검증합니다.

## Unity 전투 MVP

현재 구현된 전투:

```text
플레이어: 고죠 사토루
상대: 주령 봇 2마리
맵: 약 65×65 전투장 + 안전벽

WASD 이동
SPACE 회피와 짧은 무적
좌클릭 기본 공격 3연계
TAB 락온과 대상 전환
Q 술식순전 「창」
E 술식반전 「혁」
R 허식 「자」
V + 장인 입력 무량공처
체력·주력·쿨타임·승패
전투 사운드·화면 플래시·카메라 흔들림
```

### 고죠 술식

- `Q · 창`: 짧은 시전 뒤 락온 대상 근처 또는 정면에 지속 수렴장을 생성
- `E · 혁`: 짧은 시전 뒤 실제 이동하는 전방 반발 발사체 생성
- `R · 허식 「자」`: 창과 혁의 **사용 자체**를 준비 조건으로 사용하며 적중은 요구하지 않음
- `V · 무량공처`: 우클릭 유지 → 좌클릭 → 초록 타이밍에서 우클릭 해제
- 창 적중 뒤 같은 대상에게 혁 적중 시 `BLUE → RED CHAIN` 게임용 보너스

### 주력 자원

현재 원격 저장소에는 다음 `GAME_ORIGINAL` 프로토타입 수치가 반영되어 있습니다.

```text
최대 주력 100
초당 회복 12
사용 후 회복 지연 0.8초

창 16
혁 24
허식 자 45
무량공처 60
```

주력 게이지와 정확한 소비량은 원작 공식 숫자가 아니라 플레이 테스트용 수치입니다.

### 공통 행동 상태

`CombatActionGate`가 회피, 술식 시전, 영역 입력·전개와 사망 상태를 한곳에서 판정합니다.

```text
Normal
Dodging
TechniqueCasting
DomainInput
DomainActive
Disabled
```

이 구조는 이후 다른 캐릭터에도 재사용할 예정입니다.

### 사운드

외부 파일이 없어도 기본 공격, 피격, 회피, 창·혁·자, 무량공처와 승패에 임시 합성 효과음이 재생됩니다.

개인 테스트용 BGM·대사·효과음 교체 방법:

- [`docs/AUDIO_SETUP.md`](docs/AUDIO_SETUP.md)

## Unity 실행

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

Unity Hub에서 아래 프로젝트를 엽니다.

```text
D:\GitHub\JJK_game\unity
```

권장 환경:

- Unity 6.3 LTS `6000.3.20f1`
- Universal Render Pipeline
- Active Input Handling: Both

기존 `CombatMVP` 장면은 대부분 런타임 자동 확장을 사용하므로 일반적인 스크립트 변경 뒤에는 장면을 다시 만들 필요가 없습니다.

최초 장면 생성이 필요한 경우:

```text
Tools → JJK Game → Build Combat MVP Scene
```

## Python/Pygame 장인 연습장

Unity 이전에 아래 네 캐릭터의 영역전개 입력과 일본어 음성인식을 검증했습니다.

```text
고죠 사토루 → 무량공처
스쿠나(이타도리 몸) → 복마어주자
후시구로 메구미 → 감합암예정
옷코츠 유타(본편 2학년) → 진안상애
```

Python 실행:

```bash
python -m pip install -r requirements.txt
python -m unittest discover -s tests -v
python main.py
```

처음 일본어 음성 모델을 설치할 때만:

```bash
python scripts/setup_voice_model.py
```

## 원작 우선 구현 원칙

- 만화 본편과 공식 애니메이션·극장판 설정을 우선
- 몸·시기에 따라 능력 구성이 크게 다르면 별도 캐릭터 슬롯으로 분리
- 본편 2학년 유타와 주술회전 0 유타 분리
- 공식 조건과 게임 밸런스용 각색을 구분
- 원작에 없는 적중 횟수, 게이지, 소비량과 쿨타임을 원작 조건처럼 설명하지 않음

설정 카드:

- [고죠 사토루](docs/characters/gojo.md)
- [료멘 스쿠나](docs/characters/sukuna.md)
- [후시구로 메구미](docs/characters/megumi.md)
- [옷코츠 유타](docs/characters/yuta.md)

## 주요 코드 위치

```text
unity/Assets/Scripts/Core/
- 체력, 주력, 공통 행동 상태, 전투 HUD·승패, 입력, 사운드

unity/Assets/Scripts/Player/
- 이동, 회피, 기본 공격, 락온, 고죠 창·혁·자, 무량공처

unity/Assets/Scripts/Enemy/
- 주령 상태 머신과 피격·경직·넉백

unity/Assets/Scripts/Camera/
- 플레이어 추적, 카메라 흔들림과 화면 플래시
```

## 다음 큰 단계

- 현재 주력 시스템 사용자 테스트
- 영역전개 종료 후 술식 번아웃
- 반전술식 회복을 붙일 수 있는 상태 구조
- 캐릭터 공통 스킬 인터페이스
- 원작 조건 검토 후 두 번째 Unity 캐릭터 설계
