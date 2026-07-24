# JJK Game 프로젝트 인수인계서

> 새 ChatGPT 대화에서 사용자가 저장소 링크와 함께 `이 깃허브 읽고 우리 하던 거 계속하자`라고 말하면 **README, 이 문서, 아키텍처 문서, 캐릭터 설정 카드, 최신 코드를 먼저 읽고 현재 상태에서 이어간다.**

## 1. 프로젝트 정의

주술회전의 **술식 선언, 장인(손동작), 영역전개, 공간 변화 연출**을 실제 게임 입력으로 옮기는 개인 프로젝트다.

현재는 Python + Pygame으로 캐릭터별 장인 입력의 조작감과 성공·실패 판정을 검증한다. 장기적으로 Unity에서 캐릭터를 선택하고 봇과 싸우는 3D 전투 게임으로 확장한다.

## 2. 사용자와 작업 방식

- 사용자: 박민준
- 저장소: `JJuNi27/JJK_game`
- 기본 브랜치: `master`
- 환경: Windows + VS Code
- 화면 문구는 가능한 한 한국어
- 공식 설정은 `[OFFICIAL_CONFIRMED]`, 게임 각색은 `[GAME_ORIGINAL]`
- 사용자가 직접 수정했다면 GitHub에 push한 뒤 최신 파일을 다시 읽는다.

```text
Assistant가 GitHub 수정
→ 사용자 git pull origin master
→ 사용자 테스트 명령과 python main.py 실행
→ 화면·오류·조작감 전달
→ Assistant가 저장소 수정
```

## 3. 장기 방향

### 메인 전투 모드

- 전투 전에 캐릭터 선택
- 선택 캐릭터의 술식과 영역만 사용
- Unity에서 플레이어와 봇 전투
- 첫 봇은 강화학습이 아니라 상태 머신

첫 Unity MVP 후보:

```text
플레이어: 고죠
상대: 단순 주령 봇 1마리
맵: 작은 전투장
기능: 이동, 기본 공격, 체력, 봇 추적·공격, 무량공처, 승패
```

### 자유 연습장

- 캐릭터별 장인 커맨드
- 성공률·연속 성공·판정 지표
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
- 캐릭터 구현 전 설정 카드 작성 → 사용자 검토 → 구현
- 몸·시기 차이가 큰 형태는 별도 캐릭터 슬롯
- 영역 이름은 음성으로 요구하지 않음

설정 카드:

- [`characters/gojo.md`](characters/gojo.md): 고죠 v1.0, 검토 완료
- [`characters/sukuna.md`](characters/sukuna.md): 스쿠나(이타도리) v1.0, 검토 완료
- [`characters/megumi.md`](characters/megumi.md): 메구미 v1.0, 검토 완료

## 5. 공통 영역 음성

모든 영역전개 캐릭터는 **`료이키 텐카이`까지만** 말한다.

```text
료이키 텐카이
→ DOMAIN_READY
→ 선택 캐릭터의 고유 장인
→ 해당 캐릭터의 영역
```

현재 음성인식:

- Vosk + sounddevice
- 일본어 소형 모델 `vosk-model-small-ja-0.22`
- 부분 결과로 발동하지 않음
- 확정 결과 전체 문구 일치
- 평균 신뢰도 88% 이상
- V 키는 항상 대체 입력
- 사용자 로컬에서 일본어 인식 정상 확인

## 6. 현재 플레이 가능 캐릭터

### 고죠 사토루

```text
V 또는 료이키 텐카이
→ 우클릭 유지
→ 우클릭 유지 중 좌클릭
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

### 스쿠나(이타도리)

확정 범위:

- 이타도리 유지 몸
- 십종영법술 없음
- 해·팔·푸가 포함
- 세계참 없음
- 몸 형태별 스쿠나는 추후 별도 캐릭터

```text
V 또는 료이키 텐카이
→ E 키 유지
→ E 유지 중 좌·우클릭을 거의 동시에 누르고 유지
→ 초록 구간에서 좌·우클릭을 거의 동시에 해제
→ 복마어주자
```

초기 수치:

```text
E 후 양쪽 마우스 완성 제한 0.70초
누름 동시 인정 0.18초
목표 유지 0.85초
허용 오차 ±0.22초
해제 동시 인정 0.18초
```

사용자가 로컬에서 기본 입력과 발동이 정상 작동한다고 확인했다.

### 후시구로 메구미

확정 범위:

- 메구미 본인 버전
- 첫 기본 식신은 옥견 「혼」
- 전투에서는 실내 또는 기존 구조물이 있을 때 감합암예정 사용
- 연습장에서는 환경 제한 없음
- 마허라는 후순위 최후 수단
- 흑섬·반전술식·임의 대영역 기술 없음

```text
V 또는 료이키 텐카이
→ Q 키 유지
→ 좌클릭 유지 상태로 아래 방향 드래그
→ 이어서 왼쪽 또는 오른쪽으로 그림자 펼치기
→ Q 유지 상태에서 좌클릭 해제
→ 감합암예정
```

초기 수치:

```text
Q 후 드래그 시작 제한 0.80초
아래 방향 최소 이동 130px
아래 구간 좌우 흔들림 허용 80px
수평 펼침 최소 이동 180px
수평 구간 상하 흔들림 허용 100px
전체 궤적 제한 1.80초
```

메구미는 타이밍형이 아니라 방향·거리·궤적 완성도를 판정한다. UI에서 타이밍 바와 초 단위 오차 통계를 숨긴다. 임시 이펙트에는 감합암예정의 그림자 바닥과 식신·분신 실루엣을 표현한다.

## 7. 현재 아키텍처

세 캐릭터 모두 동일한 패키지 구조를 사용한다.

```text
characters/<character_id>/
├─ definition.py
├─ controller.py
├─ input.py
└─ effect.py
```

현재:

```text
characters/
├─ gojo/
├─ sukuna_itadori/
└─ megumi/
```

역할:

```text
definition.py
- CharacterProfile
- build_runtime()

controller.py
- 상태 전이
- 성공·실패 판정
- 캐릭터 고유 수치

input.py
- 키·마우스 이벤트를 의미 단위 명령으로 변환

effect.py
- 임시 영역 성공 연출
```

`characters/registry.py`가 프로필 순서와 런타임 builder를 연결한다. `main.py`는 캐릭터 ID 조건문을 가지지 않는다.

상세 기준: [`ARCHITECTURE.md`](ARCHITECTURE.md)

## 8. 호환 facade

고죠·스쿠나 실제 구현 원본은 모두 캐릭터 폴더로 이동했다.

아래 예전 경로는 현재 새 클래스를 다시 내보내는 호환용 얇은 파일이다.

```text
game/domain_controller.py
game/sukuna_domain_controller.py
game_input/mouse_seal_input.py
game_input/sukuna_two_hand_input.py
effects/muryang_effect.py
effects/fukuma_effect.py
game/character.py
game/runtime.py
```

새 기능을 이 파일에 추가하지 않는다. 기존 import가 모두 제거되고 회귀 테스트가 안정되면 삭제한다.

## 9. UI 구조

```text
ui/common.py
- 한글 폰트, 텍스트, 자동 줄바꿈

ui/character_select_screen.py
- 캐릭터 카드와 숫자/클릭 선택

ui/practice_screen.py
- 캐릭터 프로필의 seal_steps를 읽어 안내
- 프로필에 따라 타이밍 바와 오차 통계 표시 여부 결정
```

캐릭터별 UI 차이는 `CharacterProfile` 데이터로 결정하며 `main.py`에서 이름 문자열을 비교하지 않는다.

## 10. 자동 테스트

```bash
python -m unittest discover -s tests -v
```

현재 검사:

- 캐릭터 ID 중복 없음
- 선택 가능한 캐릭터 런타임 생성
- 컨트롤러·입력·이펙트가 해당 캐릭터 패키지에 위치
- 잠긴 캐릭터 런타임 차단
- 메구미 그림자 궤적 성공
- Q 조기 해제 실패
- 아래 방향 드래그 부족 실패

GitHub workflow:

```text
.github/workflows/python-syntax.yml
- push 시 Python compileall 문법 검사
```

## 11. 현재 구현 상태

사용자 로컬 확인 완료:

- Pygame 실행
- 캐릭터 선택과 B 복귀
- 고죠 음성·장인·무량공처
- 스쿠나 음성·양손 장인·복마어주자
- 성공·실패와 연습 통계
- 카드 자동 줄바꿈

새로 구현되어 로컬 확인이 필요한 기능:

- 구조 변경 후 고죠 회귀 테스트
- 구조 변경 후 스쿠나 회귀 테스트
- 3키 또는 카드 클릭으로 메구미 선택
- 메구미 Q + 그림자 드래그 성공·실패
- 임시 감합암예정 화면
- 캐릭터를 연속으로 바꿔도 입력 상태가 남지 않는지

## 12. 바로 다음 작업

사용자에게 다음을 실행하게 한다.

```bash
git pull origin master
python -m unittest discover -s tests -v
python main.py
```

확인 항목:

```text
1. 테스트 마지막에 OK가 나오는지
2. 1번 고죠가 기존처럼 작동하는지
3. 2번 스쿠나가 기존처럼 작동하는지
4. 3번 메구미가 선택되는지
5. V/음성 → Q 유지 → 좌클릭 아래 드래그 → 좌우 펼치기 → 좌클릭 해제로 성공하는지
6. 아래 이동 부족, 수평 이동 부족, Q 조기 해제 시 실패하는지
7. 성공 시 감합암예정 임시 화면이 나오는지
8. B로 돌아가 캐릭터를 교체해도 정상인지
```

메구미 조작이 너무 어렵거나 쉬우면 우선 조절할 값:

```text
q_to_drag_timeout
downward_min_distance
downward_horizontal_tolerance
horizontal_min_distance
horizontal_vertical_tolerance
total_gesture_timeout
```

## 13. 이후 순서

```text
세 캐릭터 입력 수치 안정화
→ 일반 술식 시스템의 공통 인터페이스 설계
→ 고죠 창·혁·자
→ 스쿠나 해·팔·푸가
→ 메구미 옥견 「혼」과 기본 식신
→ Unity 전투 MVP
→ 카메라 장인 입력
→ 영역 충돌·번아웃·흑섬
```

## 14. 새 대화 절차

```text
1. README.md
2. docs/HANDOFF.md
3. docs/ARCHITECTURE.md
4. docs/characters/gojo.md, sukuna.md, megumi.md
5. characters/registry.py와 각 characters/<id>/ 폴더
6. main.py, ui/, 공통 game_input/
7. 문서와 코드 차이 확인
8. 바로 다음 작업부터 진행
```

문서보다 최신 코드와 사용자의 최신 발언이 우선이다.
