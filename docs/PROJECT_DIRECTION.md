# 프로젝트 게임 방향과 개발 방법

작성 기준일: 2026-08-21

이 문서는 `주술사 되어보기`의 장기 게임 방향과, 소규모 개발 + AI 도구를 전제로 한 제작 방법을 기록한다.

현재 게임 방향은 **연구 기반 추천안**이다. 사용자가 추가 아이디어를 검토한 뒤 최종 확정한다. 개발 방법은 지금부터 기본 작업 방식으로 적용한다.

## 1. 추천 게임 방향

### 한 줄 정의

```text
주술회전 캐릭터의 원작 규칙을 최대한 살린
캐릭터 중심 3D 아레나 액션 격투게임

기본: 1v1 + 소규모 팀/교대전
후속: 대규모 사건전/난전 모드
```

핵심은 `STORM 4를 그대로 복제`하거나 `Sparking! ZERO를 그대로 복제`하는 것이 아니라, 주술회전에서만 가능한 상호작용을 중심으로 두 게임의 장점을 가져오는 것이다.

## 2. 참고 게임에서 가져올 것과 가져오지 않을 것

### NARUTO SHIPPUDEN: Ultimate Ninja STORM 4

공식 자료에서 확인되는 핵심:

- 최대 3인 팀 구성
- 전투 중 Leader Change
- Support Gauge를 사용하는 지원/교대
- 콤보 도중에도 리더 교대 가능
- 캐릭터별 고유 술법과 각성

우리 프로젝트에 적용할 부분:

```text
TAKE
- 한 명이 전장에 나와 싸우고 예비 멤버와 교대하는 구조
- 팀 편성 자체가 전략이 되는 방식
- 교대/지원에 별도 자원 또는 쿨타임을 두는 방식

DO NOT COPY DIRECTLY
- 나루토의 바꿔치기 술을 범용 회피 시스템으로 복제
- 모든 캐릭터에게 같은 지원 공격 규격 강제
```

### DRAGON BALL: Sparking! ZERO

공식 자료에서 확인되는 핵심:

- 캐릭터 개성과 원작의 강함을 적극적으로 전투에 반영
- 고속 이동, 카운터, 회피 계열 선택지가 여러 층으로 존재
- 팀 전투와 DP 포인트 제한을 통한 강캐/약캐 편성 전략
- 최대 5인 팀을 구성할 수 있는 전투 규칙
- 필살기로 건물과 지형이 파괴되는 연출
- 제작진이 `캐릭터가 된 느낌`을 핵심 경험으로 강조

우리 프로젝트에 적용할 부분:

```text
TAKE
- 모든 캐릭터를 동일한 힘으로 평준화하지 않음
- 고죠/스쿠나처럼 원작에서 강한 캐릭터는 실제로 강하게 설계
- 대신 팀 편성 비용, 매치 규칙, 자원으로 경쟁 모드 균형을 잡는 방식 검토
- 캐릭터 고유 방어/카운터/이동 규칙
- 강한 기술이 맵과 카메라에 영향을 주는 연출

DO NOT COPY DIRECTLY
- 대부분의 캐릭터가 자유 비행하는 드래곤볼식 3차원 이동
- 드래곤볼의 Ki/Skill Count 시스템을 그대로 이식
```

### Roblox Jujutsu Shenanigans

Roblox 공식 게임 설명에서 확인되는 기본 구조:

```text
M1 근접 콤보
Q 대시
F 방어
R 캐릭터 특수 행동
G 각성
Battlegrounds 기반 전투
파괴 가능한 전장
```

현재 공개 서버는 다인 난전 구조이며, 커뮤니티 문서에서는 기본 M1/대시/방어 위에 캐릭터마다 규칙을 크게 바꾸는 설계가 강점으로 평가된다.

우리 프로젝트에 적용할 부분:

```text
TAKE
- 타격감과 즉각적인 기술 피드백
- 캐릭터마다 공통 전투 규칙을 과감하게 변형하는 방식
- 건물/벽/소품이 공격에 반응하는 파괴 연출
- 후속 대규모 사건전에서 발생하는 예측 불가능한 난전 감각

DO NOT USE AS CORE
- 20명 안팎의 상시 FFA를 메인 게임 규칙으로 사용
- 모든 시스템을 대규모 네트워크 난전을 기준으로 먼저 설계
- 완전 동적 파괴를 첫 버전부터 구현
```

### Roblox Jujutsu Infinite 계열 MMO/RPG

공식 게임 설명은 광대한 오픈월드, 저주령 토벌, 다른 술사와 전투, 아이템/술식 수집을 핵심으로 한다.

우리 프로젝트 판단:

```text
REJECT AS MAIN DIRECTION
- 오픈월드 MMO/RPG는 현재 전투 프로토타입과 거리가 너무 큼
- 맵/퀘스트/진행/인벤토리/서버 콘텐츠 부담이 급증
- 현재 프로젝트의 가장 독특한 장점인 캐릭터별 원작 상호작용과 장인 입력이 희석됨
```

## 3. 우리 게임만의 핵심 기둥

### Pillar A — Character Authenticity

평가 질문:

```text
이 캐릭터를 플레이할 때 정말 고죠처럼 느껴지는가?
정말 스쿠나처럼 느껴지는가?
```

그래픽 품질보다 먼저 본다.

예:

- 현대 고죠는 무하한을 자동 방어로 사용
- 일반 공격은 단순 공격력이 높다는 이유로 무하한을 뚫지 못함
- 영역전연/필중/무효화처럼 원작 이유가 있어야 우회
- 스쿠나는 큰 주력통과 큰 소비
- 해/팔/푸가/복마어주자의 조건 관계 유지

### Pillar B — Asymmetric Combat

모든 캐릭터를 같은 규격으로 만들지 않는다.

공통 최소 기반:

```text
이동
기본 공격
기본 방어 또는 회피 계층
피격/경직/넉백
주력 또는 캐릭터 고유 자원
```

그 위를 캐릭터가 바꾼다.

예:

```text
고죠: 무하한 자동 방어
토도: 부기우기로 위치 교환
마키/토우지: 비술사형 기동과 피지컬
쿠사카베: 간이영역 기반 방어
하카리: 영역/잭팟 이후 상태 변화
메구미: 식신과 그림자 공간
```

### Pillar C — Domain as Match State

영역전개를 단순 궁극기 컷신 + 큰 피해로 만들지 않는다.

영역은 일정 시간 전투 규칙 자체를 바꾸는 상태다.

향후 검토 대상:

- 필중
- 영역 충돌
- 영역 안/밖 대상 분리
- 간이영역/영역전연 대응
- 영역 내부에서 기술 성질 변화
- 팀전에서 영역 밖 예비 캐릭터의 취급

### Pillar D — Spectacle + Destruction

Jujutsu Shenanigans와 Sparking! ZERO에서 배울 부분이다.

완전 물리 파괴 대신 단계형 파괴를 우선한다.

```text
Tier 1: 소품/유리/간판
Tier 2: 벽/기둥/부분 건물
Tier 3: 허식 자, 푸가, 영역용 대형 연출 파괴
```

프리프랙처, 오브젝트 풀, 교체 프리팹 같은 통제 가능한 방식을 우선한다.

### Pillar E — Hand Sign / Voice as Optional Immersion Layer

원래 프로젝트의 모델 인식 아이디어는 폐기하지 않는다.

다만 경쟁 전투의 필수 입력으로 만들지 않는다.

```text
Standard Input Mode
- 키/패드 커맨드
- 온라인/대전에서 안정적인 기본 입력

Immersion Input Mode
- 카메라 장인 인식
- 음성 인식
- 싱글/연습/친선전에서 높은 몰입감
```

두 모드는 최종적으로 같은 `Gameplay Command`를 발생시키도록 설계한다.

장인 인식의 실패/지연이 전투 밸런스 자체를 결정하지 않도록 한다.

## 4. 추천 매치 구조

### Core Mode A — Solo Battle

```text
1v1
```

모든 전투 규칙을 가장 명확하게 검증하는 기준 모드다.

### Core Mode B — Team Battle

최종 목표:

```text
2~3인 팀
전장에는 기본적으로 한 명이 활성
예비 캐릭터와 교대 가능
지원 행동은 캐릭터별 설계
```

첫 기술 프로토타입은 2v2부터 시작한다.

고죠 + 스쿠나 대 고죠 + 스쿠나 같은 미러 조합도 기술 검증에는 허용한다.

초기 팀 시스템:

```text
- Active Fighter
- Reserve Fighter
- 각 캐릭터 HP/주력 독립 보존
- 교대 쿨타임 또는 Team Gauge
- 전투 중 즉시 교대
- KO 시 다음 캐릭터 입장
```

지원공격/합동기는 이후 추가한다.

### LATER — Incident Battle

로블록스식 난전의 장점을 별도 모드로 가져온다.

예:

```text
Shibuya Incident / Culling Game 스타일
4~8명 정도의 소규모 다인전 또는 팀전
저주령/PvE 개입 가능
환경 파괴 강화
여러 영역이 동시에 발생할 수 있는 특수 규칙
```

첫 목표부터 20~50인 FFA를 노리지 않는다.

대규모 인원은 네트워크, 타깃팅, 카메라, 이펙트 가독성, 영역 충돌, 파괴 동기화 비용이 지나치게 크다.

## 5. 경쟁 밸런스에 대한 방향

캐릭터를 억지로 동일 전투력으로 만들지 않는다.

향후 Team Battle에서 `Character Cost` 방식 검토:

```text
예시 개념
고죠 / 스쿠나: 높은 편성 비용
1급 술사: 중간 비용
학생/보조형 캐릭터: 낮은 비용
```

Casual에서는 자유 편성, 경쟁 규칙에서는 총 비용 제한을 둘 수 있다.

이는 Sparking! ZERO의 DP Battle에서 얻은 아이디어이며, 실제 수치/등급은 JJK 기준으로 별도 설계한다.

## 6. 우리에게 맞는 개발 방법

방법 이름:

```text
Risk-Driven Vertical Slice Production
리스크 우선 버티컬 슬라이스 개발
```

대형 스튜디오의 Preproduction → First Playable → Vertical Slice → Production 흐름을 소규모 프로젝트에 맞게 축소한다.

### Gate 0 — Vision Lock

먼저 정할 것:

- 게임의 핵심 매치 형태
- 플레이 감각
- 캐릭터 설계 원칙
- 하지 않을 것

현재 이 문서가 그 역할을 한다.

### Gate 1 — Core Combat Proof

상태: 거의 완료

```text
고죠 Core Gameplay ✅
스쿠나 Core Gameplay ✅
공통 DamageContext ✅
무하한/영역전연/필중 상호작용 ✅
주력 ✅
영역 상태 ✅
```

남은 스쿠나 기능 마감:

```text
스쿠나 전용 영역 사운드
영역 안 푸가 광역 전환
전체 회귀 테스트
```

### Gate 2 — Match Architecture Proof

게임 방향이 팀 아레나이므로 아트 본작업 전에 최소 구조를 검증한다.

```text
Solo 1v1 Match
2v2 Team Prototype
Active / Reserve Fighter
교대
캐릭터별 HP/자원 보존
KO와 다음 캐릭터 입장
팀 HUD
카메라/락온이 교대에서 깨지지 않는지 확인
```

이 단계에서 지원 공격/합동 궁극기를 모두 만들지 않는다.

### Gate 3 — Beauty Corner

작은 부분을 90%에 가깝게 만든다.

목표 예:

```text
대표 전투장 한 구역
실제 고죠 모델
실제 스쿠나 모델
기본 이동/공격/회피 애니메이션
허식 자
푸가
무량공처
복마어주자
대표 피격/카메라/사운드/보이스
```

게임 전체를 90% 만드는 것이 아니다.

### Gate 4 — Production Pipeline Extraction

Beauty Corner를 만들며 반복된 제작 규격만 도구화한다.

예:

```text
Character Asset Contract
Animation Event Contract
Technique Presentation Profile
VFX Spawn/Follow/Stop
Voice/SFX Event
Camera Feedback
Hit Stop
```

거대한 범용 Ability System을 미리 만들지 않는다.

### Gate 5 — Third Character Stress Test

고죠/스쿠나와 구조가 다른 캐릭터를 넣는다.

후보:

```text
메구미: 소환/식신 시스템 검증에 유리
유타: 외부 동반체 리카 + 상태 변화 검증
```

새 캐릭터에서 기존 제작 파이프라인이 버티는지 확인한다.

### Gate 6 — Production

그 이후에:

```text
캐릭터 로스터 확장
맵 확장
게임 모드 확장
온라인 구조
대규모 사건전
스토리/PvE 콘텐츠
```

를 순차 생산한다.

## 7. Codex 사용 원칙

Codex가 사용 가능하더라도 무조건 사용하지 않는다.

### 직접 수정이 더 좋은 작업

```text
- 파일 1~3개의 명확한 버그 수정
- 작은 수치/조건 변경
- 문서 업데이트
- 기능의 한 부분만 연결하는 작업
```

### Codex가 유리한 작업

```text
- 여러 파일에 걸친 구조 변경
- Character / Team / Match 구조 리팩터링
- 반복 코드 마이그레이션
- 테스트 코드 추가
- 기존 코드베이스 전체를 검색해야 안전한 변경
```

Codex 사용 절차:

```text
1. 우리가 먼저 설계/원작 규칙/Acceptance Criteria 확정
2. Codex에 구체적 작업 명세 제공
3. Codex가 코드베이스 탐색 후 변경
4. Diff 검토
5. Unity에서 사용자 테스트
6. 결과를 HANDOFF에 기록
```

Codex에게 게임 디자인이나 원작 규칙 결정을 맡기지 않는다.

AI는 `구현 속도 도구`이며 `프로젝트 디렉터`가 아니다.

## 8. 다음 실제 작업 순서

게임 방향 최종 확정 전에도 안전하게 할 수 있는 순서:

```text
1. 스쿠나 전용 영역 사운드 분리
2. 영역 안 푸가 광역 전환
3. 고죠/스쿠나 전체 회귀 테스트
```

게임 방향을 본 문서의 추천안으로 확정하면 그 다음은:

```text
4. 기존 1/2키 장면 재시작 전환 구조 분석
5. Team Match 최소 아키텍처 설계
6. 2v2 Active/Reserve 교대 프로토타입
7. 팀 HUD / 카메라 / 락온 / HP·CE 보존 검증
8. 그 뒤 Beauty Corner 제작 시작
```

## 9. 참고 자료

- Ubisoft `How We Make Games / Creative Process`
  - Conception → Preproduction → First Playable → Production → Alpha → Beta → Master
- GDC 2026 `Production Workshop Part 2`
  - Preproduction deliverables: Tools & Pipelines, Core Mechanics, Beauty Corner, First Playable, Vertical Slice
- CyberConnect2 `NARUTO Shippuden: Ultimate Ninja STORM 2` 개발 사례
  - Skill Editor / Flow Editor 등 콘텐츠 제작 파이프라인 선행 투자
- NARUTO SHIPPUDEN: Ultimate Ninja STORM 4 공식 자료
  - Change Leader System과 3인 팀
- DRAGON BALL: Sparking! ZERO 공식 자료
  - 캐릭터 개성, 팀/DP Battle, 카운터 계층, 환경 파괴
- Roblox `Jujutsu Shenanigans` 공식 게임 설명
  - Battlegrounds 기반 M1/대시/방어/특수/각성 및 파괴 전장
- Roblox `Jujutsu Infinite` 공식 게임 설명
  - 오픈월드 MMO RPG 방향 비교용
