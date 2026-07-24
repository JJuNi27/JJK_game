# JJK Game

주술회전의 **술식 선언, 장인(손동작), 영역전개, 공간 변화 연출**을 실제 게임 입력과 전투 시스템으로 옮기는 개인 프로젝트입니다.

> 새 ChatGPT 대화나 새로운 개발 환경에서 이어서 작업할 때는 [`docs/HANDOFF.md`](docs/HANDOFF.md)를 먼저 읽어주세요.  
> Python 구조 기준은 [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md), Unity 시작 방법은 [`unity/README.md`](unity/README.md)에 정리되어 있습니다.

## 현재 단계

### Python/Pygame 장인 연습장 v0.1

아래 네 캐릭터의 음성·키보드·마우스 영역전개 입력을 구현했고, 사용자가 로컬에서 정상 작동을 확인했습니다.

```text
고죠 사토루
→ 한 손 마우스 타이밍 장인
→ 무량공처

스쿠나(이타도리)
→ E 키 + 마우스 양 버튼 동시 장인
→ 복마어주자

후시구로 메구미
→ Q 키 + 그림자 궤적 장인
→ 감합암예정

옷코츠 유타(본편 2학년)
→ C 키 반지 연결 + 검 뽑기 궤적
→ 진안상애
```

`주술회전 0`의 1학년 유타는 추후 별도 캐릭터로 추가합니다.

### Unity 전투 MVP

현재 다음 전투 루프를 C#으로 옮기기 시작했습니다.

```text
플레이어: 고죠
상대: 단순 주령 봇 1마리
맵: 작은 전투장

WASD 이동
좌클릭 기본 공격
체력과 승패
주령 추적·근접 공격
V + 고죠 장인 입력
무량공처 성공 시 주령 행동 정지
```

Unity 코드 위치:

```text
unity/Assets/Scripts/
├─ Core/
│  ├─ Health.cs
│  ├─ IDomainStunnable.cs
│  └─ MatchController.cs
├─ Player/
│  ├─ ThirdPersonPlayerController.cs
│  ├─ BasicAttack.cs
│  └─ GojoDomainController.cs
├─ Enemy/
│  └─ CurseBotController.cs
└─ Camera/
   └─ SimpleCameraFollow.cs
```

장면 자동 생성 도구:

```text
unity/Assets/Editor/JJKMvpSceneBuilder.cs

Unity 메뉴:
Tools → JJK Game → Build Combat MVP Scene
```

## Python 실행

```bash
git pull origin master
python -m pip install -r requirements.txt
python -m unittest discover -s tests -v
python main.py
```

처음 음성 모델을 설치하는 경우에만:

```bash
python scripts/setup_voice_model.py
```

음성 모델이나 마이크가 없어도 V 키를 사용할 수 있습니다.

## Python 공통 조작

```text
V 또는 음성 "료이키 텐카이": 영역 준비
R: 현재 시도 초기화
T: 누적 통계 초기화
B: 캐릭터 선택으로 복귀
ESC: 종료
```

캐릭터 선택:

```text
1 / ENTER / 클릭: 고죠
2 / 클릭: 스쿠나(이타도리)
3 / 클릭: 메구미
4 / 클릭: 유타(본편 2학년)
```

## 캐릭터별 영역 입력

### 고죠 · 무량공처

```text
료이키 텐카이 또는 V
→ 우클릭 유지
→ 우클릭 유지 중 좌클릭
→ 초록색 구간에서 우클릭 해제
```

```text
목표 해제: 0.90초
허용 오차: ±0.22초
```

### 스쿠나(이타도리) · 복마어주자

```text
료이키 텐카이 또는 V
→ E 유지
→ 좌·우클릭을 거의 동시에 누르고 유지
→ 초록색 구간에서 좌·우클릭을 거의 동시에 해제
```

### 메구미 · 감합암예정

```text
료이키 텐카이 또는 V
→ Q 유지
→ 좌클릭을 누른 채 아래로 드래그
→ 이어서 좌우로 그림자 펼치기
→ Q를 유지한 채 좌클릭 해제
```

### 유타 · 진안상애

```text
료이키 텐카이 또는 V
→ C를 눌러 반지 연결 유지
→ 화면 아래쪽을 좌클릭
→ 검을 뽑듯 위로 드래그
→ C를 유지한 채 좌클릭 해제
```

진안상애 전투에서는 다음 정체성을 유지합니다.

```text
필중 복사 술식 1개 선택
+
나머지 복사 술식은 무작위 검에 배정
+
검을 뽑아야 술식 확인
+
한 검은 1회 사용
```

## 설정 카드

- [고죠 사토루 v1.0](docs/characters/gojo.md)
- [료멘 스쿠나 v1.0](docs/characters/sukuna.md)
- [후시구로 메구미 v1.0](docs/characters/megumi.md)
- [옷코츠 유타 v1.0](docs/characters/yuta.md)

확정 원칙:

- 몸·시기에 따라 능력 구성이 크게 달라지면 별도 캐릭터로 분리
- 본편 2학년 유타와 주술회전 0 유타 분리
- 모든 영역 음성은 `료이키 텐카이`까지만 요구
- 실제 영역은 선택 캐릭터와 고유 장인 커맨드로 결정
- 공식 설정과 `[GAME_ORIGINAL]` 각색을 구분

## Python 모듈 구조

모든 구현 캐릭터는 같은 구조를 사용합니다.

```text
characters/<character_id>/
├─ definition.py
├─ controller.py
├─ input.py
└─ effect.py
```

`main.py`는 캐릭터별 구현을 직접 알지 않으며, `characters/registry.py`가 프로필과 런타임을 연결합니다.

## 음성인식

- Vosk + sounddevice
- 일본어 소형 모델 `vosk-model-small-ja-0.22`
- 확정 결과만 사용
- 전체 문구 정확히 일치
- 평균 신뢰도 88% 이상
- 한자·히라가나·가타카나 표기 허용

## 개발 체크리스트

- [x] 고죠 무량공처 장인
- [x] 스쿠나 복마어주자 장인
- [x] 메구미 감합암예정 장인
- [x] 유타 진안상애 장인
- [x] 일본어 음성인식
- [x] 연습 통계
- [x] 네 캐릭터 동일 패키지 구조
- [x] 사용자 로컬 회귀 테스트
- [x] Unity 폴더와 Git 제외 설정
- [x] Unity 이동·공격·체력·주령 봇 C# 뼈대
- [x] Unity 고죠 장인과 무량공처 행동 정지 로직
- [x] Unity MVP 장면 자동 생성 Editor 도구
- [ ] Unity 프로젝트 메타데이터 생성 및 첫 장면 실행
- [ ] Unity 전투 조작감 조절
- [ ] Unity 영역 시각 효과
- [ ] Python 음성과 Unity 연결
- [ ] 고죠 창·혁·자
- [ ] 카메라 장인 입력
