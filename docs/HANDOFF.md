# JJK Game 프로젝트 인수인계서

> 새 ChatGPT 대화에서 사용자가 저장소 링크와 함께 `이 깃허브 읽고 우리 하던 거 계속하자`라고 말하면 **README, 이 문서, 캐릭터 설정 카드, 최신 코드를 먼저 읽고 현재 상태에서 이어간다.**

## 1. 프로젝트 정의

주술회전의 **술식 선언, 장인(손동작), 영역전개, 공간 변화 연출**을 게임 입력 시스템으로 옮기는 개인 프로젝트다.

현재는 Python + Pygame으로 캐릭터별 장인 커맨드의 조작감과 성공·실패 판정을 검증한다. 장기적으로 Unity에서 캐릭터를 선택하고 봇과 싸우는 3D 전투 게임으로 확장한다.

## 2. 사용자와 작업 방식

- 사용자: 박민준
- 저장소: `JJuNi27/JJK_game`
- 기본 브랜치: `master`
- 환경: Windows + VS Code
- UI는 가능한 한 한국어
- 공식 설정과 `[GAME_ORIGINAL]` 각색을 구분
- 사용자가 직접 수정했다면 GitHub에 push한 뒤 최신 파일을 다시 읽음

```text
Assistant가 GitHub 수정
→ 사용자 git pull origin master
→ 사용자 python main.py 실행
→ 화면·오류·조작감 전달
→ Assistant가 다시 수정
```

## 3. 장기 방향

### 메인 전투 모드

- 전투 전에 캐릭터 선택
- 선택 캐릭터의 술식과 영역만 사용
- Unity에서 플레이어와 봇 전투
- 첫 봇은 상태 머신

첫 Unity MVP 후보:

```text
플레이어: 고죠
상대: 단순 주령 봇 1마리
맵: 작은 전투장
기능: 이동, 기본 공격, 체력, 봇 추적·공격, 무량공처, 승패
```

### 자유 연습장

- 캐릭터별 장인 커맨드
- 성공률·연속 성공·타이밍 오차
- 음성·마우스·미래 카메라 입력 테스트
- 현재 Pygame 프로그램이 원형

### 카메라 입력

초기 계획은 MediaPipe Hand Landmarker와 직접 수집한 데이터로 실제 손 장인을 인식하는 것이었다. 웹캠 고장으로 후순위로 미뤘으며 폐기하지 않았다.

```text
현재: 키보드·마우스 장인
미래: CameraSealInput
```

## 4. 설정 운영 원칙

- 원작 만화와 공식 애니메이션·극장판을 통합
- 팬 해석과 공식 설정 분리
- 공식 설정: `[OFFICIAL_CONFIRMED]`
- 게임 각색: `[GAME_ORIGINAL]`
- 캐릭터 구현 전 설정 카드 작성 → 사용자 검토 → 구현
- 몸·시기 차이가 큰 형태는 별도 캐릭터 슬롯

설정 카드:

- [`characters/gojo.md`](characters/gojo.md): 고죠 v1.0, 검토 완료
- [`characters/sukuna.md`](characters/sukuna.md): 스쿠나(이타도리) v1.0, 검토 완료

## 5. 공통 영역 음성

모든 영역전개 캐릭터는 **`료이키 텐카이`까지만** 말한다.

```text
료이키 텐카이
→ DOMAIN_READY
→ 선택 캐릭터의 고유 장인
→ 해당 캐릭터의 영역
```

영역명은 추가로 말하지 않는다.

현재 음성인식:

- Vosk + sounddevice
- 일본어 소형 모델 `vosk-model-small-ja-0.22`
- 확정 결과만 사용
- 허용 문구 전체 일치
- 평균 신뢰도 88% 이상
- V 키는 항상 대체 입력
- 사용자 로컬에서 일본어 인식 정상 확인

## 6. 고죠 현재 상태

설정 카드: [`characters/gojo.md`](characters/gojo.md)

커맨드:

```text
V 또는 료이키 텐카이
→ 우클릭 유지
→ 좌클릭
→ 초록 구간에서 우클릭 해제
→ 무량공처
```

판정:

```text
목표 해제 0.90초
허용 오차 ±0.22초
```

사용자 결과:

```text
10회 중 8회 성공
평균 해제 오차 0.123초
```

현재 수치 유지.

## 7. 스쿠나(이타도리) 확정 범위

설정 카드: [`characters/sukuna.md`](characters/sukuna.md)

- 첫 구현은 이타도리 유지 몸
- 십종영법술 없음
- 푸가 포함
- 세계참 없음
- 음성은 `료이키 텐카이`까지만
- 몸 형태별 스쿠나는 추후 별도 캐릭터

예정 변형:

```text
스쿠나(이타도리)
스쿠나(메구미)
스쿠나(헤이안 본체)
```

## 8. 스쿠나 양손 장인

현재 코드 구현 완료, 사용자 로컬 테스트 대기.

```text
V 또는 료이키 텐카이
→ 왼손 E 키 유지
→ E 유지 중 오른손 좌·우클릭을 거의 동시에 누르고 유지
→ 초록 구간에서 좌·우클릭을 거의 동시에 해제
→ 복마어주자
```

초기 수치:

```text
E 후 양쪽 마우스 완성 제한 0.70초
좌·우클릭 누름 동시 인정 0.18초
목표 유지 0.85초
허용 오차 ±0.22초
좌·우클릭 해제 동시 인정 0.18초
```

E가 장인의 한쪽 손, 마우스 양 버튼이 다른 쪽 손을 표현한다. 너무 어렵다면 동시 입력·해제 간격부터 넓힌다.

## 9. 현재 구현 상태

사용자 로컬 확인 완료:

- Pygame 실행
- 캐릭터 선택과 B 복귀
- 고죠 선택·음성·장인·무량공처
- 성공·실패와 연습 통계
- 캐릭터 카드 자동 줄바꿈

새로 구현되어 로컬 확인이 필요한 기능:

- 스쿠나(이타도리) 슬롯 선택
- E + 좌·우클릭 양손 장인
- 스쿠나 전용 성공·실패
- 임시 복마어주자 화면
- 고죠를 다시 선택했을 때 기존 조작이 유지되는지

## 10. 현재 코드 구조

```text
main.py
- 캐릭터 선택과 공통 연습 UI
- 캐릭터별 안내 문구

game/character.py
- 고죠·스쿠나 선택 데이터

game/domain_protocol.py
- 컨트롤러 공통 규약

game/domain_controller.py
- 고죠 무량공처 판정

game/sukuna_domain_controller.py
- 스쿠나 양손 복마어주자 판정

game/runtime.py
- 캐릭터별 컨트롤러·입력·이펙트 팩토리

game/practice_stats.py
- 연습 통계

game_input/mouse_seal_input.py
- 고죠 한 손 마우스 장인

game_input/sukuna_two_hand_input.py
- 스쿠나 E + 마우스 양 버튼 장인

game_input/voice_domain_trigger.py
- 공통 일본어 영역 선언

effects/muryang_effect.py
- 임시 무량공처

effects/fukuma_effect.py
- 임시 복마어주자

.github/workflows/python-syntax.yml
- push 시 Python compileall 문법 검사
```

문서보다 최신 코드와 사용자의 최신 발언이 우선이다.

## 11. 바로 다음 작업

사용자에게 다음을 실행하게 한다.

```bash
git pull origin master
python main.py
```

테스트 항목:

```text
1. 고죠가 기존처럼 정상인지
2. 2키 또는 클릭으로 스쿠나 선택 가능한지
3. V/음성 → E 유지 → 좌우클릭 동시 유지가 되는지
4. 초록 구간에서 좌우클릭 동시 해제로 성공하는지
5. 누름·해제 간격이 크거나 E를 놓으면 실패하는지
6. 성공 시 임시 복마어주자 화면이 나오는지
7. B로 돌아간 뒤 캐릭터를 바꿔도 정상인지
```

결과를 보고 우선 조절할 값:

```text
mouse_press_sync_tolerance
mouse_release_sync_tolerance
target_release_time
release_tolerance
left_hand_to_mouse_timeout
```

## 12. 이후 순서

```text
스쿠나 입력 수치 안정화
→ 캐릭터별 일반 술식 설계
→ 고죠 창·혁·자
→ 스쿠나 해·팔·푸가
→ Unity 전투 MVP
→ 카메라 장인 입력
→ 영역 충돌·번아웃·흑섬
```

## 13. 새 대화 절차

```text
1. README.md
2. docs/HANDOFF.md
3. docs/characters/gojo.md, sukuna.md
4. 최신 main.py, game/, game_input/, effects/
5. 코드와 문서 차이 확인
6. 바로 다음 작업부터 진행
```
