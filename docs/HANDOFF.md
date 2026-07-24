# JJK Game 프로젝트 인수인계서

> 새 ChatGPT 대화에서 사용자가 저장소 링크와 함께 `이 깃허브 읽고 우리 하던 거 계속하자`라고 말하면 **README, 이 문서, ARCHITECTURE, 캐릭터 설정 카드, 최신 Python·Unity 코드를 먼저 읽고 현재 상태에서 이어간다.**

## 1. 프로젝트 정의

주술회전의 **술식 선언, 장인(손동작), 영역전개, 공간 변화 연출**을 실제 게임 입력과 전투 시스템으로 옮기는 개인 프로젝트다.

현재 두 축으로 진행한다.

```text
Python + Pygame
- 캐릭터별 영역전개 입력 검증
- 음성인식
- 성공·실패와 연습 통계

Unity + C#
- 실제 이동과 공격
- 주령 봇 전투
- 검증된 영역 입력을 전투에 적용
```

## 2. 사용자와 작업 방식

- 사용자: 박민준
- 저장소: `JJuNi27/JJK_game`
- 기본 브랜치: `master`
- 환경: Windows + VS Code
- 화면 문구는 가능한 한 한국어
- 공식 설정은 `[OFFICIAL_CONFIRMED]`, 게임 각색은 `[GAME_ORIGINAL]`

```text
Assistant가 GitHub 수정
→ 사용자가 git pull origin master
→ 사용자 컴퓨터에서 실행
→ 화면·오류·조작감 전달
→ Assistant가 저장소 수정
```

Python 실행:

```bash
git pull origin master
python -m unittest discover -s tests -v
python main.py
```

## 3. 설정 운영 원칙

- 원작 만화와 공식 애니메이션·극장판 통합
- 팬 해석과 공식 설정 분리
- 캐릭터 구현 전 설정 카드 작성 → 사용자 검토 → 구현
- 몸·시기 차이가 크면 별도 캐릭터 슬롯
- 모든 영역전개 캐릭터는 음성으로 `료이키 텐카이`까지만 말함
- 영역명은 음성으로 요구하지 않음
- 실제 발동 영역은 선택 캐릭터와 고유 장인 커맨드가 결정

설정 카드:

- `docs/characters/gojo.md`: 고죠 v1.0
- `docs/characters/sukuna.md`: 스쿠나(이타도리) v1.0
- `docs/characters/megumi.md`: 메구미 v1.0
- `docs/characters/yuta.md`: 본편 2학년 유타 v1.0

## 4. Python/Pygame v0.1 완료 상태

사용자가 아래 네 캐릭터를 로컬에서 모두 정상 작동한다고 확인했다.

### 고죠 사토루

```text
V 또는 료이키 텐카이
→ 우클릭 유지
→ 우클릭 유지 중 좌클릭
→ 초록 구간에서 우클릭 해제
→ 무량공처
```

```text
목표 해제 0.90초
허용 오차 ±0.22초
```

사용자 1차 통계:

```text
10회 중 8회 성공
평균 해제 오차 0.123초
```

### 스쿠나(이타도리)

확정 범위:

- 이타도리 유지 몸
- 십종영법술 없음
- 해·팔·푸가 포함
- 세계참 없음
- 메구미 몸·헤이안 본체는 추후 별도 캐릭터

```text
V 또는 료이키 텐카이
→ E 유지
→ 좌·우클릭을 거의 동시에 누르고 유지
→ 초록 구간에서 좌·우클릭을 거의 동시에 해제
→ 복마어주자
```

### 후시구로 메구미

확정 범위:

- 메구미 본인
- 첫 기본 식신은 옥견 「혼」
- 전투에서는 실내 또는 기존 구조물이 있을 때 감합암예정 사용
- 연습장에서는 환경 제한 없음
- 마허라는 후순위 최후 수단

```text
V 또는 료이키 텐카이
→ Q 유지
→ 좌클릭 유지 상태로 아래 방향 드래그
→ 이어서 좌우로 그림자 펼치기
→ Q 유지 상태에서 좌클릭 해제
→ 감합암예정
```

### 옷코츠 유타(본편 2학년)

분리 원칙:

```text
characters/yuta
= 본편 2학년 유타

추후 characters/yuta_zero
= 주술회전 0의 1학년 유타
```

본편 유타와 제로 유타의 리카 상태와 능력 범위를 섞지 않는다.

```text
V 또는 료이키 텐카이
→ C 키로 반지 연결 유지
→ 화면 아래쪽을 좌클릭
→ 검을 뽑듯 위로 드래그
→ C 유지 상태에서 좌클릭 해제
→ 진안상애
```

진안상애 전투 정체성:

```text
필중 복사 술식 1개 선택
+
나머지 복사 술식은 무작위 검에 배정
+
검을 뽑아야 술식 확인
+
한 검은 1회 사용
```

첫 복사 술식 우선순위:

- 주언
- 하늘 조작
- 우스라비

후순위:

- 드루브 술식
- 야곱의 사다리
- 주복사 참격
- 리카 완전 현현 5분 시스템

## 5. Python 아키텍처

네 캐릭터 모두 같은 구조를 사용한다.

```text
characters/<character_id>/
├─ definition.py
├─ controller.py
├─ input.py
└─ effect.py
```

```text
characters/registry.py
- 카드 순서
- 프로필과 build_runtime 연결

main.py
- 앱 루프와 화면 전환
- 캐릭터별 세부 구현을 직접 알지 않음

ui/
- 캐릭터 선택과 연습 화면

game_input/
- 공통 V 키와 일본어 음성 입력
```

과거 공용 경로의 고죠·스쿠나 호환 facade는 삭제했다. 실제 캐릭터 구현은 `characters/<id>/`에만 둔다.

상세 기준은 `docs/ARCHITECTURE.md`.

## 6. 음성인식

- Vosk + sounddevice
- 일본어 소형 모델 `vosk-model-small-ja-0.22`
- 부분 결과로 발동하지 않음
- 확정 결과 전체 문구 일치
- 평균 신뢰도 88% 이상
- 한자·히라가나·가타카나 표기 허용
- V 키는 항상 대체 입력
- 사용자 로컬에서 `료이키 텐카이` 인식 정상 확인

## 7. 현재 핵심 단계 — Unity 전투 MVP

Python 장인 연습장에 캐릭터를 더 추가하는 것보다, 실제 전투 루프를 먼저 검증한다.

첫 MVP:

```text
플레이어: 고죠
상대: 단순 주령 봇 1마리
맵: 작은 전투장

WASD 이동
좌클릭 기본 공격
플레이어·주령 체력
주령 추적·근접 공격
V + 고죠 마우스 장인
무량공처 성공 시 주령 행동 정지
승리 / 패배
```

### Unity 코드 위치

```text
unity/Assets/Scripts/Core/
├─ Health.cs
├─ IDomainStunnable.cs
└─ MatchController.cs

unity/Assets/Scripts/Player/
├─ ThirdPersonPlayerController.cs
├─ BasicAttack.cs
└─ GojoDomainController.cs

unity/Assets/Scripts/Enemy/
└─ CurseBotController.cs

unity/Assets/Scripts/Camera/
└─ SimpleCameraFollow.cs

unity/Assets/Editor/
└─ JJKMvpSceneBuilder.cs
```

### Unity 구현 내용

- `Health`: 공통 체력·피해·사망 이벤트
- `ThirdPersonPlayerController`: CharacterController 기반 WASD 이동
- `BasicAttack`: 좌클릭 범위 공격
- `CurseBotController`: Idle/Chase/Attack/Frozen/Dead 상태 머신
- `IDomainStunnable`: 영역 행동 불능 대상 공통 규약
- `GojoDomainController`: Python에서 검증한 V → 우클릭 → 좌클릭 → 우클릭 해제 판정
- `MatchController`: HUD, 승리·패배, ENTER 재시작
- `SimpleCameraFollow`: 플레이어 추적
- `JJKMvpSceneBuilder`: 메뉴 한 번으로 테스트 장면 자동 생성

### 장면 자동 생성

Unity 상단 메뉴:

```text
Tools
→ JJK Game
→ Build Combat MVP Scene
```

자동 생성:

```text
ArenaGround
GojoPlayer
CurseBot
Main Camera
Directional Light
MatchController
Assets/Scenes/CombatMVP.unity
```

상세 최초 설정은 `unity/README.md`를 따른다.

## 8. Unity 프로젝트 최초 준비

현재 Git에는 `unity/Assets/`와 코드만 있다. Unity가 생성하는 `Packages/`와 `ProjectSettings/`는 사용자 컴퓨터에서 한 번 만들어야 한다.

권장 절차:

```text
1. Unity Hub에서 Unity 6.3 LTS + 3D Core 임시 프로젝트 생성
2. 에디터 종료
3. 임시 프로젝트의 Packages/와 ProjectSettings/를 저장소 unity/로 복사
4. Unity Hub에서 저장소 unity/ 폴더 열기
5. 컴파일 완료
6. Tools → JJK Game → Build Combat MVP Scene
7. Play
```

Unity 자동 생성 폴더는 `.gitignore`에 추가되어 있다.

## 9. Unity에서 아직 확인되지 않은 것

Unity 코드는 GitHub에 반영했지만 사용자의 Unity 에디터에서는 아직 실행하지 않았다.

다음 로컬 확인이 필요하다.

```text
1. C# 컴파일 오류가 없는지
2. 장면 자동 생성 메뉴가 보이는지
3. CombatMVP 씬이 생성되는지
4. WASD 이동이 되는지
5. 좌클릭 공격으로 주령 체력이 감소하는지
6. 주령이 추적하고 플레이어를 공격하는지
7. 승리·패배와 ENTER 재시작이 되는지
8. V → 우클릭 유지 → 좌클릭 → 타이밍 해제로 무량공처가 되는지
9. 무량공처 동안 주령이 멈추는지
10. 장인 입력 중 좌클릭이 기본 공격으로 중복되지 않는지
```

## 10. 바로 다음 작업

사용자의 Unity 설치 여부를 확인하고 `unity/README.md` 절차대로 최초 프로젝트를 연다.

오류가 발생하면 Console의 첫 번째 빨간 오류 전체와 Unity 버전을 받는다.

정상 실행 뒤 조정 순서:

```text
이동 속도와 카메라
→ 공격 범위·피해·주령 속도
→ 고죠 영역 입력 타이밍
→ 임시 무량공처 화면 효과
→ Python 일본어 음성과 Unity 연결
→ 창·혁·자
```

## 11. 장기 방향

```text
Unity 고죠 vs 주령 MVP
→ 전투 조작감 안정화
→ 캐릭터별 일반 술식
→ 캐릭터 선택과 데이터 구조
→ 다른 영역 캐릭터 Unity 이전
→ 카메라 실제 장인 입력
→ 영역 충돌·번아웃·흑섬
```

첫 봇은 강화학습이 아니라 단순 상태 머신이다. 전투 기본기가 안정되기 전에는 ML을 넣지 않는다.
