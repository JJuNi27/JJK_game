# JJK Game 아키텍처

이 문서는 캐릭터와 기능이 늘어나도 기존 코드를 과도하게 수정하지 않도록 프로젝트의 모듈 경계를 정의한다.

## 핵심 원칙

1. `main.py`는 앱 실행, 화면 전환, 공통 루프만 담당한다.
2. UI는 `ui/`에서 화면 표시만 담당하며 캐릭터 고유 판정 규칙을 알지 않는다.
3. 공통 음성·키보드 입력은 `game_input/`에 두고, 캐릭터 고유 장인 입력은 해당 캐릭터 폴더에 둔다.
4. 모든 캐릭터는 `characters/<character_id>/` 아래에서 같은 구조를 사용한다.
5. 새 캐릭터는 기존 캐릭터 조건문을 늘리지 않고 `characters/registry.py`에 등록한다.
6. 프로필, 컨트롤러, 입력, 이펙트는 각각 분리한다.
7. 공통 코드는 Protocol에만 의존하고 구체 캐릭터 클래스를 직접 알지 않는다.
8. 설정 카드 승인 후에만 해당 캐릭터 구현을 해금한다.

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
│  │  └─ 캐릭터 슬롯과 런타임 builder 등록소
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
│  └─ megumi/
│     ├─ definition.py
│     ├─ controller.py
│     ├─ input.py
│     └─ effect.py
│
├─ game/
│  ├─ state.py
│  ├─ domain_protocol.py
│  ├─ practice_stats.py
│  ├─ app_scene.py
│  ├─ character.py                 # 임시 호환 facade
│  ├─ runtime.py                   # 임시 호환 facade
│  ├─ domain_controller.py         # 고죠 이전 경로 호환 facade
│  └─ sukuna_domain_controller.py  # 스쿠나 이전 경로 호환 facade
│
├─ game_input/
│  ├─ keyboard_domain_trigger.py
│  ├─ voice_domain_trigger.py
│  ├─ mouse_seal_input.py          # 고죠 이전 경로 호환 facade
│  └─ sukuna_two_hand_input.py     # 스쿠나 이전 경로 호환 facade
│
├─ effects/
│  ├─ muryang_effect.py            # 고죠 이전 경로 호환 facade
│  └─ fukuma_effect.py             # 스쿠나 이전 경로 호환 facade
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
   └─ megumi.md
```

## 캐릭터 패키지 규칙

모든 캐릭터는 아래 역할을 동일하게 지킨다.

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

현재 세 캐릭터 모두 이 구조를 사용한다.

## 등록소 방식

`characters/registry.py`는 다음만 담당한다.

```text
캐릭터 카드 순서
캐릭터 ID 중복 없는 목록
캐릭터 ID → build_runtime 함수 연결
잠긴 캐릭터의 런타임 생성 차단
```

새 캐릭터를 추가할 때 `main.py`에 조건문을 추가하지 않는다.

```python
CHARACTER_SLOTS = (
    GOJO,
    SUKUNA_ITADORI,
    MEGUMI,
    NEW_CHARACTER,
)

_RUNTIME_BUILDERS = {
    GOJO.character_id: build_gojo_runtime,
    SUKUNA_ITADORI.character_id: build_sukuna_runtime,
    MEGUMI.character_id: build_megumi_runtime,
    NEW_CHARACTER.character_id: build_new_character_runtime,
}
```

## UI의 캐릭터별 차이 처리

캐릭터마다 판정 방식이 다르므로 UI가 모든 영역을 타이밍형으로 가정하지 않는다.

```text
고죠
- 타이밍 바 표시
- 해제 오차 통계 표시

스쿠나
- 타이밍 바 표시
- 해제 오차 통계 표시

메구미
- 그림자 궤적 거리 표시
- 타이밍 바 숨김
- 초 단위 해제 오차 통계 숨김
```

이 차이는 `CharacterProfile` 데이터로 결정하며 `main.py`에 캐릭터 이름 비교문을 넣지 않는다.

## 새 캐릭터 추가 절차

1. `docs/characters/<id>.md` 설정 카드 작성
2. 사용자 검토와 구현 범위 승인
3. `characters/<id>/`에 네 파일 생성
4. `definition.py`에서 독립 런타임 조립
5. `characters/registry.py`에 프로필과 builder 등록
6. 캐릭터 고유 판정 자동 테스트 추가
7. 기존 캐릭터 회귀 테스트
8. 로컬 조작감 확인 후 수치 조절

## 호환 facade 정책

고죠와 스쿠나 구현은 이미 캐릭터 패키지로 완전히 이동했다.

예전 `game/`, `game_input/`, `effects/` 경로의 해당 파일에는 실제 구현을 두지 않고 새 클래스를 다시 내보내는 코드만 둔다. 기존 import가 모두 사라지고 로컬 회귀 테스트가 안정되면 호환 facade를 제거한다.

## 금지할 구조

```text
main.py 안에 캐릭터별 판정 조건 추가
하나의 거대한 controller.py에 모든 영역 구현
모든 캐릭터의 입력·효과를 한 파일에 누적
캐릭터 이름 문자열을 UI와 입력 곳곳에서 직접 비교
음성인식 모듈이 특정 캐릭터의 영역을 직접 결정
한 캐릭터 변경이 다른 캐릭터 파일 수정으로 이어지는 구조
```

## 자동 검사

테스트는 다음을 확인한다.

- 캐릭터 ID 중복 없음
- 선택 가능한 캐릭터는 런타임 생성 가능
- 런타임의 컨트롤러·입력·이펙트가 해당 `characters/<id>/` 패키지에 위치
- 잠긴 캐릭터 런타임 생성 차단
- 메구미 그림자 궤적 성공·실패 상태 전이

## Unity 이전 시 대응

Python 코드를 그대로 복사하지는 않더라도 모듈 경계를 그대로 대응시킨다.

```text
CharacterDefinition / ScriptableObject
CharacterRegistry
DomainController
SealInput
EffectPlayer
PracticeStats
```

검증된 입력 순서와 상태 전이를 캐릭터별 C# 컴포넌트로 재구현하고, 공통 전투 시스템은 캐릭터 구현 세부사항에 직접 의존하지 않게 한다.
