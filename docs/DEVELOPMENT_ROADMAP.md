# 개발 로드맵과 우선순위 판단 기준

작성 기준일: 2026-08-21

상세 게임 방향과 제작 방법은 `docs/PROJECT_DIRECTION.md`를 기준으로 한다.

## 판단 질문

새 제안이 들어오면 다음 순서로 평가한다.

1. 현재 전투 루프의 정확성이나 테스트 품질을 높이는가?
2. 다음 캐릭터에도 재사용될 실제 공통 규칙인가?
3. 지금 하지 않으면 이후 코드를 크게 다시 작성해야 하는가?
4. 반대로 지금 하면 모델·연출·콘텐츠 변경 때문에 반복 작업이 커지는가?
5. 원작 조건인지, 공식 게임의 각색인지, 우리 게임용 편의인지 구분됐는가?
6. 최종 목표인 캐릭터 중심 3D 아레나/팀 전투 구조에 맞는가?

판정:

```text
NOW
- 현재 구조에 필요하며 바로 구현

FOUNDATION_ONLY
- 지금은 기반만 만들고 콘텐츠는 보류

LATER
- 좋은 아이디어지만 현재 단계에서는 반복 작업이 커서 보류

REJECT
- 프로젝트 방향·원작 기준·기술 구조와 맞지 않아 채택하지 않음
```

## 현재 프로젝트 단계

대형 스튜디오식 표현으로는 `Preproduction 후반 / First Playable 완료 직전`에 가깝다.

완료:

```text
현대 고죠 Core Gameplay ✅
시부야 스쿠나 Core Gameplay ✅
DamageContext / 방어 우회 구조 ✅
무하한 / 영역전연 / 필중 상호작용 ✅
주력 자원 ✅
고죠 무량공처 ✅
스쿠나 복마어주자 ✅
푸가 ✅
```

사용자 확인 완료 상태는 `docs/HANDOFF.md` 기준을 따른다.

## 잠정 게임 방향

`docs/PROJECT_DIRECTION.md`의 추천안:

```text
캐릭터 중심 3D 아레나 액션 격투

Core A: 1v1 Solo Battle
Core B: 2~3인 Team Battle
- 전장에는 기본적으로 한 명 활성
- 예비 캐릭터와 교대
- 캐릭터 HP/주력 독립 보존

LATER:
- 4~8인 Incident Battle
- 환경 파괴 강화
- PvPvE/사건전
```

20~50인 상시 FFA나 오픈월드 MMO는 메인 방향으로 잡지 않는다.

## 개발 방법 — Risk-Driven Vertical Slice

### Gate 1 — Core Combat Proof

거의 완료.

남은 마감:

```text
1. 스쿠나 전용 영역 사운드 분리
2. 복마어주자 안 푸가 광역 전환
3. 고죠/스쿠나 전체 회귀 테스트
```

### Gate 2 — Match Architecture Proof

게임 방향이 팀 아레나로 최종 확정되면 아트 본작업 전에 진행한다.

```text
Solo Match 구조
2v2 Team Prototype
Active Fighter / Reserve Fighter
전투 중 교대
캐릭터별 HP/CE 보존
KO 후 다음 캐릭터 입장
팀 HUD
교대 뒤 카메라/락온 유지
```

현재 `1/2키 → 장면 재시작` 캐릭터 전환은 개발용 임시 구조이므로 이 단계에서 교체한다.

지원 공격, 합동기, 3인 팀 완성형 UI는 아직 만들지 않는다.

### Gate 3 — Beauty Corner

작은 범위를 거의 완성품 품질로 만든다.

```text
대표 맵 한 구역
실제 고죠/스쿠나 모델
공통 이동/기본공격/회피 애니메이션
고죠: 허식 자 + 무량공처 대표 연출
스쿠나: 푸가 + 복마어주자 대표 연출
전용 보이스/SFX
카메라/히트스톱/피격 피드백
HUD
```

게임 전체를 90% 완성하는 것이 아니다.

### Gate 4 — Production Pipeline Extraction

Beauty Corner에서 실제 반복된 것만 공통화한다.

```text
Character Asset Contract
Animation Event Contract
Technique Presentation Profile
VFX Spawn / Follow / Stop
Voice / SFX Event
Camera Feedback
Hit Stop
```

거대한 범용 Ability System을 미리 만들지 않는다.

### Gate 5 — Third Character Stress Test

구조가 다른 세 번째 캐릭터로 파이프라인을 검증한다.

우선 후보:

```text
메구미 — 식신/소환 구조 검증
유타 — 리카 동반체/상태 변화 검증
```

### Gate 6 — Production

그 뒤 캐릭터, 맵, 온라인, 사건전, PvE를 본격 확장한다.

## 캐릭터 설계 원칙

- 모든 캐릭터를 같은 강도로 평준화하지 않는다.
- 원작상 강한 캐릭터는 실제로 강한 플레이 경험을 준다.
- 경쟁 팀전은 Character Cost/편성 제한 방식으로 균형을 잡는 것을 검토한다.
- 범용 방어는 최소화하고 캐릭터별 고유 방어 규칙을 적극 사용한다.
- 영역전개는 단순 큰 피해 궁극기가 아니라 전투 규칙을 바꾸는 Match State로 취급한다.

## 장인/음성 인식 방향

모델 인식 아이디어는 폐기하지 않는다.

```text
Standard Input Mode
- 키/패드 커맨드
- 안정적인 기본 전투 입력

Immersion Input Mode
- 카메라 장인 인식
- 음성 인식
- 싱글/연습/친선용 몰입 옵션
```

둘 다 최종적으로 같은 Gameplay Command를 발생시키는 구조를 목표로 한다.

## Codex 사용 기준

직접 수정:

```text
작은 버그
1~3개 파일 변경
수치/조건 변경
간단한 연결
문서
```

Codex 우선 검토:

```text
Team/Match 구조 같은 다중 파일 리팩터링
반복 코드 마이그레이션
테스트 추가
코드베이스 전체 탐색이 필요한 변경
```

절차:

```text
설계 확정
→ Acceptance Criteria 작성
→ Codex 구현
→ Diff 검토
→ 사용자 Unity 테스트
→ HANDOFF 업데이트
```

Codex에게 원작 규칙이나 게임 디자인 결정을 맡기지 않는다.

## 현재 NEXT

사용자 추가 아이디어를 검토한 뒤 게임 방향을 확정한다.

그와 무관하게 안전하게 진행 가능한 다음 코드는:

```text
1. 스쿠나 전용 영역 사운드 분리
2. 영역 안 푸가 광역 전환
3. 고죠/스쿠나 회귀 테스트
```

게임 방향을 팀 아레나 추천안으로 확정하면 이후 `Team Match Architecture`가 Beauty Corner보다 먼저다.
