# JJK Game 프로젝트 인수인계서

> 새 ChatGPT 대화에서는 `README.md`, 이 문서, `docs/PROJECT_DIRECTION.md`, `docs/DEVELOPMENT_ROADMAP.md`, `docs/CANON_SKILL_RULES.md`, `docs/ADAPTATION_REFERENCES.md`, `docs/PHANTOM_PARADE_REFERENCE.md`, `docs/SUKUNA_FIRST_VERSION_RESEARCH.md`, `docs/AUDIO_SETUP.md`, `unity/README.md`와 최신 코드를 먼저 읽는다.

작성 기준일: 2026-08-21

이 문서는 **사용자 확인 완료**, **현재 게임 방향**, **작업 방식**, **다음 우선순위**를 분리하는 단일 인수인계 기준이다.

## 1. 환경과 작업 방식

- 프로젝트: **주술사 되어보기**
- 저장소: `JJuNi27/JJK_game`
- 브랜치: `master`
- 로컬: `D:\GitHub\JJK_game`
- Unity 프로젝트: `D:\GitHub\JJK_game\unity`
- Unity 6.3 LTS `6000.3.20f1`, URP

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

기본 작업 흐름:

```text
Assistant가 설계/조사
→ master 수정 또는 Codex 작업 명세 작성
→ Diff 검토
→ 사용자가 git pull
→ 사용자 Unity에서 실제 테스트
→ 결과를 HANDOFF에 기록
```

Assistant는 Unity를 직접 컴파일했다고 주장하지 않는다.

## 2. 기술 리드 원칙

사용자의 제안을 무조건 구현하지 않는다.

```text
NOW             현재 구조에 필요해 바로 구현
FOUNDATION_ONLY 기반만 준비하고 콘텐츠는 보류
LATER           좋은 아이디어지만 후반이 효율적
REJECT          프로젝트 방향·원작·구조와 맞지 않음
```

원작 우선순위:

```text
만화/공식 팬북/공식 애니메이션·영화·사이트
→ 팬텀 퍼레이드 공식 각색 참고
→ GameWith 일본 서버 상세 조사
→ 글로벌 한국어 클라이언트 명칭
→ 프로젝트용 GAME_ORIGINAL 수치
```

사용자 검수 완료:

```text
스쿠나 R 표시명: 푸가
내부 ID: Fuga
```

`화로`는 원리 설명에만 사용하고 UI에는 `푸가`를 사용한다.

## 3. 현재 게임 방향 — 연구 기반 추천안

상세: `docs/PROJECT_DIRECTION.md`

현재 추천 한 줄 정의:

```text
주술회전 캐릭터의 원작 규칙을 적극적으로 살린
캐릭터 중심 3D 아레나 액션 격투게임
```

참고 비중:

```text
NARUTO STORM 4
- 소규모 팀 편성
- 전투 중 리더/캐릭터 교대
- 지원 자원

DRAGON BALL: Sparking! ZERO
- 캐릭터 강함/개성의 비대칭성
- 다양한 방어/카운터 층
- 팀 편성 비용 개념
- 강력한 기술의 맵/카메라 파괴 연출

Jujutsu Shenanigans
- 즉각적인 타격감
- 캐릭터별로 공통 규칙을 과감히 바꾸는 설계
- 파괴 가능한 전장
- 후속 사건전의 난전 감각
```

메인 방향으로 채택하지 않는 것:

```text
20~50인 상시 FFA
오픈월드 MMO/RPG
완전 동적 파괴부터 시작하는 구조
```

후속 `Incident Battle`에서 4~8인 정도의 소규모 다인전/PvPvE를 검토한다.

## 4. 추천 매치 구조

### Solo Battle

```text
1v1
```

전투 규칙 검증과 경쟁 기본 모드.

### Team Battle

최종 목표:

```text
2~3인 팀
기본적으로 한 명 Active Fighter
Reserve Fighter와 교대
캐릭터별 HP/주력 독립 보존
교대 쿨타임 또는 팀 자원
KO 시 다음 캐릭터 입장
```

첫 기술 프로토타입은 2v2부터 시작한다.

현재 `1/2 키 → 장면 재시작` 캐릭터 전환은 개발용 임시 구조라 Team Match Architecture 단계에서 교체한다.

지원 공격/합동 궁극기/3인 완성 UI는 이후 단계다.

## 5. 프로젝트 고유 핵심 기둥

### Character Authenticity

`정말 고죠/스쿠나처럼 느껴지는가?`를 그래픽보다 먼저 평가한다.

### Asymmetric Combat

모든 캐릭터를 동일 규격/동일 강도로 평준화하지 않는다.

예:

```text
고죠: 무하한 자동 방어
토도: 부기우기 위치 교환
메구미: 식신/그림자
마키/토우지: 비술사형 피지컬
```

### Domain as Match State

영역전개는 단순 대미지 궁극기가 아니라 전투 규칙을 일정 시간 바꾸는 상태로 취급한다.

### Spectacle + Controlled Destruction

파괴 단계:

```text
Tier 1 소품/유리
Tier 2 벽/기둥/부분 건물
Tier 3 허식 자/푸가/영역 대형 파괴 연출
```

### Hand Sign / Voice

기존 모델/음성 인식 아이디어는 폐기하지 않는다.

```text
Standard Input Mode
- 키/패드 커맨드

Immersion Input Mode
- 장인 카메라 인식
- 음성 인식
```

두 입력 방식은 최종적으로 같은 Gameplay Command를 발생시키도록 한다.

경쟁 전투에서 인식 실패/지연이 승패를 결정하지 않도록 장인/음성은 선택형 몰입 계층으로 두는 방향이 현재 추천이다.

## 6. 개발 방법 — Risk-Driven Vertical Slice

상세: `docs/PROJECT_DIRECTION.md`, `docs/DEVELOPMENT_ROADMAP.md`

```text
Gate 0 Vision Lock
→ 게임 방향/하지 않을 것 확정

Gate 1 Core Combat Proof
→ 고죠/스쿠나 핵심 전투

Gate 2 Match Architecture Proof
→ 1v1 + 2v2 Active/Reserve/교대

Gate 3 Beauty Corner
→ 작은 한 구역을 거의 완성품 품질로 제작

Gate 4 Production Pipeline Extraction
→ 실제 반복된 모델/애니/VFX/오디오 규격만 공통화

Gate 5 Third Character Stress Test
→ 메구미/유타처럼 구조가 다른 캐릭터로 검증

Gate 6 Production
→ 캐릭터/맵/온라인/사건전 본격 확장
```

중요한 변경:

- 스쿠나 뒤 바로 세 번째 캐릭터를 만들지 않는다.
- 그렇다고 고죠/스쿠나 게임 전체를 90% 폴리싱하지도 않는다.
- 게임 방향이 팀 아레나로 확정되면 **Team Match Architecture를 Beauty Corner보다 먼저** 검증한다.
- 이후 대표 작은 구역을 90% 가까운 `Beauty Corner`로 만든다.

## 7. Codex 사용 원칙

Codex는 무조건 사용하지 않는다.

직접 수정 적합:

```text
작은 버그
1~3개 파일 변경
간단한 조건/수치
문서
```

Codex 적합:

```text
여러 파일에 걸친 리팩터링
Team/Match Architecture
반복 코드 마이그레이션
테스트 추가
전체 코드베이스 탐색이 필요한 구조 변경
```

Codex 사용 절차:

```text
1. 설계/원작 규칙/Acceptance Criteria를 먼저 확정
2. Codex에 구체적 작업 명세
3. 변경 Diff 검토
4. 사용자 Unity 테스트
5. HANDOFF 갱신
```

Codex에게 게임 디자인이나 원작 판정을 맡기지 않는다.

## 8. Python/Pygame 확인 완료

```text
고죠 · 무량공처
스쿠나(이타도리 몸) · 복마어주자
후시구로 메구미 · 감합암예정
옷코츠 유타(본편 2학년) · 진안상애
```

일본어 Vosk 음성인식과 장인 입력을 사용자가 확인했다.

Unity 최종 연결은 아직 하지 않았다.

## 9. Unity 공통 조작 — 현재 프로토타입

```text
WASD 이동
SPACE 회피
TAB 타깃 전환
LMB 3단 기본 공격
F1 도움말
ENTER 승패 뒤 재시작

1 현대 고죠 선택 후 장면 재시작
2 시부야 스쿠나 선택 후 장면 재시작
```

고죠:

```text
Q 창
E 혁
R 허식 자
V 무량공처 입력
X 영역 입력 취소
```

스쿠나:

```text
Q 해
E 팔
R 푸가
V 복마어주자
```

스쿠나 V 단발은 핵심 영역 로직 검증용 임시 입력이다.

## 10. 사용자 확인 완료 — 현대 고죠

- 기본 이동/3단 공격/회피
- 창 지속 수렴장
- 혁 이동·관통 발사체
- 창→혁 보너스
- 허식 자
- 무량공처 장인 입력
- 영역 종료 후 약 5초 술식 번아웃
- 육안 효율 주력 프로필
- 현대 고죠 무하한 자동 방어
- 일반 공격 차단
- 영역전연 우회
- 회피 무적 우선
- 공통 `DamageContext` 마이그레이션

공통 피해 속성:

```text
DamageDeliveryType
PhysicalStrike
CursedTechnique
DomainSureHit
Environmental

DamageTraits
DomainAmplification
TechniqueNullification
IgnoresInfinity
Unblockable
```

처리 순서:

```text
유효 피해
→ 회피 무적
→ 활성 IDamageGuard
→ 실제 HP 감소
```

## 11. 사용자 확인 완료 — 시부야 스쿠나

버전:

```text
SukunaShibuyaYujiBody
시부야 사변 · 이타도리 유지 육체
```

주력:

```text
최대 160
시작 95
```

기술:

```text
Q 해
- 다중 적 2히트
- 단일 적 집중 4히트

E 팔
- 근거리 단일 7히트
- 마지막 넉백

R 푸가
- 해/팔 사용 후 준비
- 영역 밖 살아 있는 적 1명 제약
- 현재 약 24m 거리 테스트 조건
- 추적 화염탄/폭발/피해 정상 확인

V 복마어주자
- V 단발 임시 입력
- 개방형 사당/지면 범위선
- 영역 안 다중 적 반복 필중 참격
- DomainSureHit
- 종료/쿨타임 정상
```

푸가 약 24m 조건은 원작 발동 조건이 아니라 조정 가능한 `GAME_ORIGINAL` 값이다.

## 12. 현재 알려진 한계

- 스쿠나 영역 전용 사운드/보이스 미분리
- 영역 안 푸가 광역형 미구현
- 영역 후 주력 증가/회복/스킬 변화 미구현
- 영역 충돌/상쇄/덮어쓰기 미구현
- 복마어주자는 V 직접 입력이며 장인 인식 미연결
- 현재 캐릭터 전환은 장면 재시작 방식
- 고죠/스쿠나 외형과 영역 VFX는 최종 자산이 아님
- 완성형 맵/파괴 시스템 없음
- Team Match Architecture 없음

## 13. 바로 다음 코드 작업

게임 방향 최종 확정과 무관하게 안전한 작업:

```text
1. 스쿠나 전용 영역 사운드 분리
2. 복마어주자 안 푸가 광역 전환 조건 재조사/구현
3. 고죠/스쿠나 전체 회귀 테스트
```

그 뒤 게임 방향을 `PROJECT_DIRECTION.md` 추천안으로 확정하면:

```text
4. 기존 PrototypeCharacterController의 장면 재시작 전환 구조 분석
5. Team Match Architecture 설계
6. 2v2 Active/Reserve 교대 프로토타입
7. HP/CE 보존, KO, 팀 HUD, 카메라/락온 검증
8. Beauty Corner 진입
```

## 14. Beauty Corner 목표

작은 범위를 90%에 가깝게 만든다.

후보:

```text
실제 고죠 모델
실제 스쿠나 모델
공통 이동/공격/회피 애니메이션
고죠 허식 자 + 무량공처
스쿠나 푸가 + 복마어주자
대표 도심 전투장 한 구역
전용 보이스/SFX
히트스톱/카메라/피격 피드백
선택적 파괴 연출
```

이 결과를 바탕으로 Character/Animation/VFX/Audio 제작 규격을 추출한 뒤 세 번째 캐릭터로 확장성을 검증한다.

## 15. 새 채팅에서 Assistant가 먼저 할 일

1. 이 문서와 `docs/PROJECT_DIRECTION.md`를 읽는다.
2. 고죠와 스쿠나 Core Gameplay는 사용자 확인 완료로 유지한다.
3. 사용자의 추가 게임 방향 아이디어가 있으면 먼저 검토하고 추천안을 확정/수정한다.
4. 확정 전에도 스쿠나 영역 사운드와 영역 안 푸가는 안전한 NEXT다.
5. 팀 아레나 방향이 확정되면 Beauty Corner보다 Team Match Architecture가 먼저다.
6. 모델/VFX/애니메이션을 전면 제작하기 전에 실제 반복된 제작 계약만 공통화한다.
7. 대규모 리팩터링은 Codex 사용을 우선 검토한다.
8. 작업 뒤 이 문서를 반드시 갱신한다.
