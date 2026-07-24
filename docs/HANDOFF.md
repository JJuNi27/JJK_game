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

```text
Assistant가 GitHub 수정
→ 사용자 git pull origin master
→ 사용자 단위 테스트와 python main.py 실행
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

## 4. 설정 운영 원칙

- 원작 만화와 공식 애니메이션·극장판을 통합
- 팬 해석과 공식 설정 분리
- 캐릭터 구현 전 설정 카드 작성 → 사용자 검토 → 구현
- 몸·시기 차이가 크면 별도 캐릭터 슬롯
- 영역명은 음성으로 요구하지 않음
- 모든 영역전개 캐릭터는 `료이키 텐카이`까지만 말함

설정 카드:

- [`characters/gojo.md`](characters/gojo.md): 고죠 v1.0, 검토 완료
- [`characters/sukuna.md`](characters/sukuna.md): 스쿠나(이타도리) v1.0, 검토 완료
- [`characters/megumi.md`](characters/megumi.md): 메구미 v1.0, 검토 완료
- [`characters/yuta.md`](characters/yuta.md): 유타 v0.1, 검토 전

## 5. 공통 영역 음성

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

```text
목표 해제 0.90초
허용 오차 ±0.22초
```

사용자 결과:

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
- 몸 형태별 스쿠나는 추후 별도 캐릭터

```text
V 또는 료이키 텐카이
→ E 키 유지
→ E 유지 중 좌·우클릭을 거의 동시에 누르고 유지
→ 초록 구간에서 좌·우클릭을 거의 동시에 해제
→ 복마어주자
```

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
→ 이어서 좌우로 그림자 펼치기
→ Q 유지 상태에서 좌클릭 해제
→ 감합암예정
```

세 캐릭터 모두 사용자가 로컬에서 정상 작동을 확인했다.

## 7. 다음 캐릭터 — 옷코츠 유타

중요한 정정:

```text
진안상애 = 옷코츠 유타의 영역전개
자폐원돈과 = 마히토의 영역전개
```

원래 프로젝트에서 계획한 네 번째 영역은 유타의 **진안상애**다. 마히토가 아니다.

현재 상태:

- `characters/yuta/definition.py`에 잠금 프로필 존재
- 선택 화면 네 번째 카드에 표시
- `docs/characters/yuta.md` 설정 카드 v0.1 작성 완료
- 아직 builder가 없어 선택·실행 불가

설정 카드의 현재 권장 방향:

- 첫 구현은 본편 2학년 유타
- 주술회전 0 유타는 필요하면 추후 별도 변형
- 모방과 리카를 핵심으로 운영
- 진안상애의 필중 술식 1개 선택 유지
- 나머지 복사 술식은 무작위 검에 배정
- 검은 1회 사용 후 소멸
- 첫 장인 후보는 `왼손 키 유지 + 아래에서 위로 검 뽑기 드래그`
- 리카 완전 현현 5분 시스템은 영역 장인 이후 별도 단계

사용자 검토가 필요한 항목은 `docs/characters/yuta.md` 8절을 따른다.

## 8. 현재 아키텍처

구현 캐릭터는 모두 동일한 패키지 구조를 사용한다.

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
├─ gojo/             # 네 파일 구현
├─ sukuna_itadori/   # 네 파일 구현
├─ megumi/           # 네 파일 구현
└─ yuta/             # 잠금 definition.py만 존재
```

`characters/registry.py`가 카드 순서와 런타임 builder를 연결한다. `main.py`와 UI는 캐릭터별 구현 클래스를 직접 알지 않는다.

과거의 다음 호환 파일은 삭제 완료했다.

```text
game/character.py
game/runtime.py
game/domain_controller.py
game/sukuna_domain_controller.py
game_input/mouse_seal_input.py
game_input/sukuna_two_hand_input.py
effects/muryang_effect.py
effects/fukuma_effect.py
```

실제 캐릭터 구현은 `characters/<id>/`에만 둔다.

상세 기준: [`ARCHITECTURE.md`](ARCHITECTURE.md)

## 9. UI 구조

```text
ui/common.py
- 한글 폰트, 텍스트, 자동 줄바꿈

ui/character_select_screen.py
- 현재 네 캐릭터용 2×2 카드
- 숫자·ENTER·클릭 선택
- 잠긴 유타 카드 표시

ui/practice_screen.py
- 프로필의 seal_steps를 읽어 안내
- 프로필 데이터에 따라 타이밍 바와 오차 통계 표시 여부 결정
```

다섯 번째 캐릭터 추가 전에는 선택 화면 페이지 기능을 추가한다.

## 10. 자동 테스트

```bash
python -m unittest discover -s tests -v
```

현재 검사:

- 캐릭터 ID 중복 없음
- 선택 가능한 캐릭터 런타임 생성
- 컨트롤러·입력·이펙트가 해당 캐릭터 패키지에 위치
- 잠긴 유타 런타임 생성 차단
- 메구미 그림자 궤적 성공·실패

GitHub Actions는 Python compileall과 단위 테스트를 실행한다.

## 11. 바로 다음 작업

사용자에게 먼저 다음을 실행하게 한다.

```bash
git pull origin master
python -m unittest discover -s tests -v
python main.py
```

확인 항목:

```text
1. 테스트가 OK인지
2. 2×2 선택 카드가 깨지지 않는지
3. 고죠·스쿠나·메구미가 이전과 동일하게 작동하는지
4. 유타 카드가 보이지만 선택되지 않는지
5. B로 캐릭터를 바꿔도 입력 상태가 남지 않는지
```

그 다음 `docs/characters/yuta.md`의 결정사항을 사용자와 검토하고, 승인되면 다음 구조를 만든다.

```text
characters/yuta/
├─ definition.py
├─ controller.py
├─ input.py
└─ effect.py
```

## 12. 이후 순서

```text
유타 설정 카드 승인
→ 진안상애 장인 프로토타입
→ 유타 복사 술식·리카 단계 설계
→ 캐릭터별 일반 술식
→ Unity 전투 MVP
→ 카메라 장인 입력
→ 영역 충돌·번아웃·흑섬
```

## 13. 새 대화 절차

```text
1. README.md
2. docs/HANDOFF.md
3. docs/ARCHITECTURE.md
4. docs/characters/gojo.md, sukuna.md, megumi.md, yuta.md
5. characters/registry.py와 최신 캐릭터 패키지
6. 코드와 문서 차이 확인
7. 바로 다음 작업부터 진행
```
