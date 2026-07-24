# JJK Game 아키텍처

이 문서는 캐릭터와 기능이 늘어나도 기존 코드를 과도하게 수정하지 않도록 프로젝트의 모듈 경계를 정의한다.

## 핵심 원칙

1. `main.py`는 앱의 화면 전환과 게임 루프만 담당한다.
2. UI는 `ui/`에서 화면 그리기만 담당하고 캐릭터 판정 규칙을 알지 않는다.
3. 음성·키보드·마우스 입력은 입력 모듈이 의미 단위 이벤트로 변환한다.
4. 캐릭터별 프로필과 런타임 구성은 `characters/<character>/` 안에 둔다.
5. 새 캐릭터는 기존 캐릭터의 조건문을 늘리는 대신 `characters/registry.py`에 등록한다.
6. 캐릭터 고유 판정·입력·이펙트는 서로 교체 가능한 모듈로 유지한다.
7. 공통 규약은 Protocol로 정의하고 구체 구현에 의존하지 않는다.

## 현재 구조

```text
JJK_game/
├─ main.py                         # 앱 루프와 화면 전환
├─ characters/
│  ├─ base.py                      # CharacterProfile, Runtime, Protocol
│  ├─ registry.py                  # 캐릭터 목록과 런타임 builder 등록소
│  ├─ gojo/
│  │  └─ definition.py             # 고죠 프로필 + 런타임 조립
│  ├─ sukuna_itadori/
│  │  └─ definition.py             # 스쿠나 프로필 + 런타임 조립
│  └─ megumi/
│     └─ definition.py             # 검토 중인 메구미 프로필
├─ game/
│  ├─ state.py                     # 공통 상태
│  ├─ domain_protocol.py           # 컨트롤러 공통 규약
│  ├─ domain_controller.py         # 고죠 판정
│  ├─ sukuna_domain_controller.py  # 스쿠나 판정
│  ├─ practice_stats.py            # 연습 통계
│  ├─ character.py                 # 기존 import 호환 facade
│  └─ runtime.py                   # 기존 import 호환 facade
├─ game_input/
│  ├─ keyboard_domain_trigger.py
│  ├─ voice_domain_trigger.py
│  ├─ mouse_seal_input.py
│  └─ sukuna_two_hand_input.py
├─ effects/
│  ├─ muryang_effect.py
│  └─ fukuma_effect.py
├─ ui/
│  ├─ common.py
│  ├─ character_select_screen.py
│  └─ practice_screen.py
└─ docs/characters/
   ├─ gojo.md
   ├─ sukuna.md
   └─ megumi.md
```

## 새 캐릭터 추가 절차

예를 들어 메구미를 구현할 때:

```text
characters/megumi/
├─ definition.py       # 프로필과 build_runtime
├─ controller.py       # 감합암예정 판정
├─ input.py            # 그림자 궤적 입력
└─ effect.py           # 임시 감합암예정 이펙트
```

진행 순서:

1. `docs/characters/megumi.md` 설정 카드 승인
2. 메구미 폴더에 컨트롤러·입력·이펙트 구현
3. `definition.py`의 `build_runtime()`에서 세 모듈 조립
4. `characters/registry.py`에 프로필과 builder 등록
5. 기존 고죠·스쿠나 코드 변경 없이 로컬 회귀 테스트

## 단계적 이전

현재 고죠와 스쿠나의 구체 구현 파일은 기존 `game/`, `game_input/`, `effects/` 경로에 있다. 런타임 조립과 프로필은 이미 캐릭터 패키지로 분리했다.

새 기능을 추가할 때 한 번에 전체 경로를 이동해 오류를 만들기보다 다음 순서로 점진 이전한다.

```text
1. 새 캐릭터는 처음부터 characters/<id>/ 내부에 구현
2. 기존 고죠·스쿠나는 기능 수정 시 해당 캐릭터 폴더로 이동
3. 기존 경로는 필요한 기간 동안 호환 facade로 유지
4. 모든 import 전환과 회귀 테스트 후 오래된 facade 제거
```

## 금지할 구조

```text
main.py 안에 캐릭터별 판정 조건 추가
하나의 거대한 controller.py에 모든 영역 구현
캐릭터 이름 문자열을 UI·입력·효과 곳곳에서 직접 비교
음성인식 모듈이 특정 캐릭터의 영역을 직접 결정
새 캐릭터를 위해 기존 캐릭터 코드를 복사해 한 파일에 덧붙이기
```

## Unity 이전 시 대응

현재 모듈 경계는 Unity에서도 비슷하게 유지한다.

```text
CharacterDefinition / ScriptableObject
DomainController
SealInput
EffectPlayer
CharacterRegistry
PracticeStats
```

Python 프로토타입 코드를 그대로 옮기지는 않더라도, 검증된 상태 전이와 입력 규칙을 C# 컴포넌트로 재구현할 수 있게 설계한다.
