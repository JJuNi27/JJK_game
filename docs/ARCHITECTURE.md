# JJK Game 아키텍처

이 문서는 캐릭터와 기능이 늘어나도 기존 코드를 과도하게 수정하지 않도록 프로젝트의 모듈 경계를 정의한다.

## 핵심 원칙

1. `main.py`는 앱 실행, 화면 전환, 공통 루프만 담당한다.
2. UI는 화면 표시만 담당하며 캐릭터 고유 판정 규칙을 알지 않는다.
3. 공통 음성·키보드 입력은 `game_input/`에 둔다.
4. 캐릭터 고유 프로필·판정·입력·이펙트는 `characters/<character_id>/`에 둔다.
5. 새 캐릭터는 `main.py`에 조건문을 추가하지 않고 `characters/registry.py`에 등록한다.
6. 공통 코드는 Protocol과 프로필 데이터에만 의존한다.
7. 설정 카드 승인 전에는 캐릭터 프로필만 잠금 상태로 등록한다.
8. 같은 기능의 실제 구현을 두 경로에 중복해서 두지 않는다.

## 현재 구조

```text
JJK_game/
├─ main.py
│  └─ 앱 루프와 화면 전환
│
├─ characters/
│  ├─ base.py
│  │  └─ CharacterProfile, CharacterRuntime, 공통 Protocol
│  ├─ registry.py
│  │  └─ 카드 순서와 런타임 builder 등록소
│  │
│  ├─ gojo/
│  │  ├─ definition.py
│  │  ├─ controller.py
│  │  ├─ input.py
│  │  └─ effect.py
│  │
│  ├─ sukuna_itadori/
│  │  ├─ definition.py
│  │  ├─ controller.py
│  │  ├─ input.py
│  │  └─ effect.py
│  │
│  ├─ megumi/
│  │  ├─ definition.py
│  │  ├─ controller.py
│  │  ├─ input.py
│  │  └─ effect.py
│  │
│  └─ yuta/
│     └─ definition.py
│        └─ 설정 검토용 잠금 프로필
│
├─ game/
│  ├─ state.py
│  ├─ domain_protocol.py
│  ├─ practice_stats.py
│  └─ app_scene.py
│
├─ game_input/
│  ├─ keyboard_domain_trigger.py
│  └─ voice_domain_trigger.py
│
├─ ui/
│  ├─ common.py
│  ├─ character_select_screen.py
│  └─ practice_screen.py
│
├─ tests/
│  ├─ test_character_registry.py
│  └─ test_megumi_gesture.py
│
└─ docs/characters/
   ├─ gojo.md
   ├─ sukuna.md
   ├─ megumi.md
   └─ yuta.md
```

과거 고죠·스쿠나 구현이 있던 `game/`, `game_input/`, `effects/` 호환 facade는 로컬 회귀 확인 뒤 제거했다. 실제 캐릭터 구현은 현재 `characters/<character_id>/`에만 존재한다.

## 캐릭터 패키지 규칙

구현이 승인된 캐릭터는 아래 네 파일을 사용한다.

```text
definition.py
- 선택 화면용 프로필
- build_runtime()에서 컨트롤러·입력·이펙트 조립

controller.py
- 상태 전이
- 입력 성공·실패 판정
- 캐릭터 고유 제한시간과 허용 오차

input.py
- Pygame 키·마우스 이벤트 수신
- Controller의 의미 단위 메서드 호출

effect.py
- 영역 성공 후 임시 화면 연출
- 최종 에셋과 분리된 프로토타입 효과
```

설정 검토 중인 캐릭터는 `definition.py`만 만들고 `available=False`로 둔다. 승인 후 나머지 세 파일과 `build_runtime()`을 추가한다.

## 등록소 방식

`characters/registry.py`는 다음만 담당한다.

```text
캐릭터 카드 순서
캐릭터 ID 중복 없는 목록
캐릭터 ID → build_runtime 함수 연결
잠긴 캐릭터의 런타임 생성 차단
```

현재 유타는 카드 목록에는 있지만 builder 등록은 없으므로 선택할 수 없다.

## UI의 캐릭터별 차이

캐릭터마다 판정 방식이 다르므로 UI가 모든 영역을 타이밍형으로 가정하지 않는다.

```text
고죠
- 타이밍 바 표시
- 해제 오차 통계 표시

스쿠나
- 타이밍 바 표시
- 해제 오차 통계 표시

메구미
- 그림자 궤적 거리 판정
- 타이밍 바 숨김
- 초 단위 오차 통계 숨김
```

이 차이는 `CharacterProfile` 데이터로 결정한다. `main.py`나 UI에서 캐릭터 ID를 직접 비교하지 않는다.

선택 화면은 현재 네 슬롯용 2×2 그리드다. 다섯 번째 캐릭터를 추가할 때는 `ui/character_select_screen.py`에 페이지 이동을 추가하되 캐릭터 패키지는 수정하지 않는다.

## 새 캐릭터 추가 절차

1. `docs/characters/<id>.md` 설정 카드 작성
2. 잠금 프로필을 `characters/<id>/definition.py`에 추가
3. 사용자 검토와 구현 범위 승인
4. `controller.py`, `input.py`, `effect.py` 구현
5. `definition.py`에서 독립 런타임 조립
6. `characters/registry.py`에 builder 등록 후 해금
7. 캐릭터 고유 판정 자동 테스트 추가
8. 기존 캐릭터 회귀 테스트
9. 로컬 조작감 확인 후 수치 조절

## 금지할 구조

```text
main.py 안에 캐릭터별 판정 조건 추가
하나의 거대한 controller.py에 모든 영역 구현
모든 캐릭터의 입력·효과를 한 파일에 누적
캐릭터 이름 문자열을 UI와 입력 곳곳에서 직접 비교
음성인식 모듈이 특정 캐릭터의 영역을 직접 결정
한 캐릭터 변경이 다른 캐릭터 파일 수정으로 이어지는 구조
옛 경로와 새 경로에 실제 구현을 동시에 유지
```

## 자동 검사

테스트는 다음을 확인한다.

- 캐릭터 ID 중복 없음
- 선택 가능한 캐릭터는 런타임 생성 가능
- 런타임 구성 요소가 해당 `characters/<id>/` 패키지에 위치
- 잠긴 캐릭터 런타임 생성 차단
- 메구미 그림자 궤적 성공·실패 상태 전이

GitHub Actions는 Python 문법 검사와 단위 테스트를 함께 실행한다.

## Unity 이전 시 대응

Python 코드를 그대로 복사하지는 않더라도 모듈 경계를 비슷하게 대응시킨다.

```text
CharacterDefinition / ScriptableObject
CharacterRegistry
DomainController
SealInput
EffectPlayer
PracticeStats
```

검증된 입력 순서와 상태 전이를 캐릭터별 C# 컴포넌트로 재구현하고, 공통 전투 시스템은 캐릭터 구현 세부사항에 직접 의존하지 않게 한다.
