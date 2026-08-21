# JJK Game 프로젝트 인수인계서

작성 기준일: 2026-08-21

> 새 ChatGPT 대화에서는 이 문서와 `docs/PROJECT_DIRECTION.md`, `docs/ORIGINAL_FIDELITY_POLICY.md`, `docs/CANON_SKILL_RULES.md`, `docs/DEVELOPMENT_ROADMAP.md`, `docs/GATE3_BEAUTY_CORNER.md`, 최신 코드를 먼저 읽는다.

이 문서는 현재 사용자 확인 상태와 다음 작업을 빠르게 복구하기 위한 단일 인수인계 기준이다. 세부 설계/테스트 이력은 연결된 개별 문서를 기준으로 한다.

## 1. 환경과 작업 방식

```text
프로젝트: 주술사 되어보기
저장소: JJuNi27/JJK_game
브랜치: master
로컬: D:\GitHub\JJK_game
Unity: D:\GitHub\JJK_game\unity
Unity 6.3 LTS 6000.3.20f1 / URP
```

사용자 pull:

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

작업 원칙:

```text
Assistant 조사/설계
→ 작은 변경은 master 직접 수정
→ 큰 구조 변경은 Codex 검토
→ 사용자 git pull
→ 사용자 Unity 실제 테스트
→ USER VERIFIED만 완료로 기록
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

개발 방법:

```text
Risk-Driven Vertical Slice Production
```

새 작업 분류:

```text
NOW
FOUNDATION_ONLY
LATER
REJECT
```

## 2. 게임 방향

현재 본체:

```text
주술회전 캐릭터의 원작 규칙을 최대한 살린
Character-Authentic 3D Arena Action Fighter
```

구조:

```text
기본: 1v1
핵심 확장: 2~3인 팀 / Active-Reserve 교대전
후속: 4~8인 Incident Battle
```

참고 방향:

```text
NARUTO STORM 4
- 팀 편성 / 교대

DRAGON BALL: Sparking! ZERO
- 원작 강함 / 비대칭 캐릭터성
- 스펙터클

Jujutsu Shenanigans
- 타격감
- 환경 파괴
- 후속 난전 감각
```

메인으로 하지 않음:

```text
20~50인 상시 FFA
오픈월드 MMO/RPG
초기부터 완전 동적 파괴
```

## 3. 원작 / 주팬퍼 정책

상세:

```text
docs/ORIGINAL_FIDELITY_POLICY.md
docs/CANON_SKILL_RULES.md
docs/PHANTOM_PARADE_REFERENCE.md
```

우선순위:

```text
1. 만화 완결 범위 + 공식 팬북/애니/극장판/공식 설정
2. 주술회전 팬텀 퍼레이드 캐릭터 설계
3. 글로벌 한국어 공식 명칭
4. 실시간 격투용 GAME_ORIGINAL 보완
5. 경쟁 밸런스
```

핵심:

```text
밸런스보다 원작 구현 우선
```

주팬퍼는 턴제 수치를 그대로 복사하지 않고 캐릭터 특성을 해석해 실시간 3D 규칙으로 재설계한다.

같은 캐릭터라도 시대/육체/전투 방식이 크게 다르면 별도 Fighter Slot로 분리한다.

사용자 검수 글로벌 한국어:

```text
스쿠나 R = 푸가
내부 ID = Fuga
```

## 4. 입력 인식 방향

기존 장인/음성 인식 아이디어는 폐기하지 않는다.

```text
Standard Input Mode
- 키보드 / 패드

Immersion Input Mode
- 카메라 장인 인식
- 음성 인식
```

둘 다 나중에 같은 Gameplay Command를 발생시키는 구조를 목표로 한다.

## 5. Gate 상태

```text
Gate 0 Vision Lock: 충분히 진행됨
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner: STARTED
Gate 4 Production Pipeline Extraction: PENDING
Gate 5 Third Character Stress Test: PENDING
Gate 6 Production: PENDING
```

## 6. Gate 1 — Core Combat USER VERIFIED

### 현대 고죠

```text
GOJO SATORU · 현대 · 교사

이동 / 기본 3타 / 회피
Q 창
E 혁
R 허식 자
V 무량공처
무하한
영역전연 우회
DomainSureHit 상호작용
술식 번아웃
육안 효율 CE
```

### 시부야 스쿠나 · 이타도리 육체

```text
RYOMEN SUKUNA · 시부야 사변

Q 해
E 팔
R 푸가
V 복마어주자
영역 밖 푸가
복마어주자 안 광역 푸가
스쿠나 전용 영역 사운드
```

스쿠나 CE 프로토타입:

```text
최대 160
시작 95
초당 회복 4
```

### 훈련 주령

두 CurseBot이 한 위치에 겹치던 문제는 engagement slot으로 수정했고 사용자 확인 완료.

```text
Curse A / Curse B 동시 표시
두 대상 TAB 전환
일반 공격 / 영역전연 훈련
```

모두 정상.

## 7. Gate 2A — Player Team USER VERIFIED

상세:

```text
docs/TEAM_MATCH_GATE2.md
docs/TEAM_HUD_CLEANUP.md
```

현재 플레이어 팀:

```text
GOJO SATORU · 현대 · 교사
RYOMEN SUKUNA · 시부야 사변
```

임시 입력:

```text
T · TAG
```

구조:

```text
Player GameObject = Fighter Shell
PrototypePlayerTeamController
- Active / Reserve
- 캐릭터별 HP snapshot
- 캐릭터별 CE snapshot
- 수동 Tag
- KO Auto Tag
```

사용자 검증:

```text
고죠 ↔ 스쿠나 장면 재시작 없는 T Tag ✅
HP 독립 보존 ✅
CE 독립 보존 ✅
Reserve 중 상태 정지 ✅
Action Lock 중 Tag 차단 ✅
첫 KO Auto Tag / DEFEAT 미발생 ✅
KO 캐릭터 재Tag 차단 ✅
마지막 팀원 KO에만 DEFEAT ✅
기본 공격 콤보 Tag 시 초기화 ✅
카메라 추적 유지 ✅
Target Lock 유지 ✅
고죠/스쿠나 기술 회귀 ✅
```

Player Team-aware HUD도 사용자 캡처로 검증 완료.

## 8. Gate 2B — Opponent Team USER VERIFIED

상세:

```text
docs/GATE2B_OPPONENT_TEAM.md
docs/GATE2B_ENEMY_HUD_CLEANUP.md
docs/GATE2B_TARGET_HANDOFF.md
```

모드:

```text
TRAINING · MULTI CURSE
- Curse A + Curse B 동시 전투

TEAM BATTLE
- Curse A Active
- Curse B Reserve
```

임시 개발 입력:

```text
F2 · MODE
```

사용자 검증:

```text
Training / Team Battle 분리 ✅
상대가 한 번에 한 명씩 등장 ✅
첫 Active KO 뒤 Reserve 자동 등장 ✅
첫 상대 KO에는 VICTORY 없음 ✅
마지막 상대 KO에만 VICTORY ✅
Team Battle Enemy HUD 단일 OPPONENT TEAM 패널 ✅
최종 0/2 + VICTORY 캡처 확인 ✅
새 Reserve 등장 시 Target Lock 자동 승계 ✅
```

첫 F2 구현에서는 Scene reload 뒤 controller가 사라지는 런타임 버그가 있었고 사용자 테스트에서 발견했다.

수정:

```text
PrototypeOpponentTeamController DontDestroyOnLoad
sceneLoaded 재연결
새 Scene의 Curse A/B 재탐색
requestedMode 재적용
```

이후 정상 검증 완료.

현재 Team Battle Target 정책:

```text
상대 Active KO
→ Reserve 자동 입장
→ 플레이어 Target Lock 자동 승계
```

`GAME_ORIGINAL` 프로토타입 규칙.

## 9. Gate 2 종료 판정

2026-08-21 누적 사용자 Unity 검증으로:

```text
Gate 2 Match Architecture Proof: USER VERIFIED
```

로 종료한다.

Gate 2가 증명한 것:

```text
플레이어와 상대 모두 Active / Reserve 전투가 가능
캐릭터별 상태 보존이 가능
KO 다음 멤버 입장 가능
팀 마지막 멤버 기준 승패 가능
카메라/Target Lock 흐름 유지 가능
Training과 Team Battle을 분리 가능
```

Beauty Corner 전에는 다음을 확장하지 않는다.

```text
지원 공격
합동 궁극기
3인 팀 완성형
Character Cost 실제 밸런스
온라인
```

## 10. Gate 3 — Beauty Corner STARTED

상세:

```text
docs/GATE3_BEAUTY_CORNER.md
```

목표:

```text
대표 맵 한 구역
실제 고죠/스쿠나 모델
공통 이동/기본공격/회피 애니메이션
허식 자 / 무량공처 대표 연출
푸가 / 복마어주자 대표 연출
Voice / SFX
Camera / Hit Stop / Hit Feedback
HUD polish
```

진행 순서:

```text
3A Combat Feel Pass
3B Representative Character Presentation
3C Signature Technique Presentation
3D Representative Arena + HUD Polish
```

## 11. 현재 구현 — Gate 3A Combat Feel Pass 1

상태:

```text
REMOTE IMPLEMENTED / USER TEST PENDING
```

변경:

```text
unity/Assets/Scripts/Player/BasicAttack.cs
```

기존 `SimpleCameraFollow.AddShake()`를 기본 3타의 실제 적중에 연결했다.

규칙:

```text
헛공격 → Shake 없음
방어로 실제 피해 미적용 → Shake 없음
DamageResolution.Applied → Shake

1타 < 2타 < 3타 FINISH
```

현재 수치:

```text
1타 0.075 / 0.07s
2타 0.11  / 0.085s
3타 0.22  / 0.13s
```

피해량, 범위, 넉백, 히트스턴, 쿨타임은 변경하지 않았다.

사용자에게 다음을 확인받는다.

```text
기본 3타 적중 카메라 충격 체감
1 < 2 < 3 강도 차이
과도한 흔들림 여부
허공 공격 시 Shake 미발생
T Tag / TAB Lock 회귀
```

이 Pass가 통과하면 Gate 3A Pass 2에서 짧은 Hit Stop을 별도 구현/검증한다.

## 12. 다음 우선순위

```text
NOW
Gate 3A Combat Feel Pass 1 사용자 검증

PASS 후
Gate 3A Combat Feel Pass 2 — Hit Stop

그 뒤
실제 모델/애니메이션/대표 기술 연출 자산 준비 수준에 맞춰 3B/3C 진행
```

Gate 4에서는 Beauty Corner에서 실제 반복된 구조만 다음 형태로 추출한다.

```text
Character Asset Contract
Animation Event Contract
Technique Presentation Profile
VFX lifecycle
Voice/SFX event
Camera feedback
Hit Stop
```

거대한 범용 Ability System을 미리 만들지 않는다.
