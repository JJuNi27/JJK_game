# JJK Game 프로젝트 인수인계서

작성 기준일: 2026-08-23

> 새 ChatGPT 대화에서는 이 문서와 `docs/PROJECT_DIRECTION.md`, `docs/ORIGINAL_FIDELITY_POLICY.md`, `docs/CANON_SKILL_RULES.md`, `docs/DEVELOPMENT_ROADMAP.md`, `docs/GATE3_BEAUTY_CORNER.md`, 최신 코드를 먼저 읽는다.

이 문서는 현재 사용자 확인 상태와 다음 작업을 빠르게 복구하기 위한 단일 인수인계 기준이다.

---

# 1. 환경 / 작업 방식

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
조사/설계
→ 작은 변경은 master 직접 수정
→ 여러 파일 구조 변경은 agent/Codex 활용 가능
→ Git diff 검토
→ 사용자 pull
→ 사용자 Unity 실제 테스트
→ USER VERIFIED만 완료 처리
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

개발 방법:

```text
Risk-Driven Vertical Slice Production
```

새 아이디어 분류:

```text
NOW
FOUNDATION_ONLY
LATER
REJECT
```

작업 속도 원칙:

```text
국소 변경마다 pull/test를 반복하지 않고
관련 작업 2~5개를 가능한 범위에서 묶어서 한 번에 검증한다.

단, 컴파일/핵심 Gameplay 위험이 큰 변경은 작은 migration 단위로 검증한다.
```

---

# 2. 게임 방향

```text
Character-Authentic 3D Arena Action Fighter
```

구조:

```text
기본: 1v1
핵심 확장: 2~3인 팀 / Active-Reserve 교대전
후속: 4~8인 Incident Battle
```

참고:

```text
NARUTO STORM 4
- 팀 편성 / 교대

DRAGON BALL: Sparking! ZERO
- 원작 비대칭 강함
- 캐릭터 특화 방어/이동
- 스펙터클

Jujutsu Shenanigans
- 타격감
- 환경 파괴
- 후속 난전 감각
```

초기 메인으로 하지 않음:

```text
20~50인 상시 FFA
오픈월드 MMO/RPG
완전 동적 파괴
```

원작 우선순위:

```text
1. 만화 완결 범위 + 공식 팬북/애니/극장판/공식 설정
2. 주술회전 팬텀 퍼레이드 캐릭터 설계 참고
3. 글로벌 한국어 공식 명칭
4. 실시간 격투용 GAME_ORIGINAL 보완
5. 경쟁 밸런스
```

사용자 검수 명칭:

```text
스쿠나 R = 푸가
내부 ID = Fuga
```

---

# 3. Gate 상태

```text
Gate 0 Vision Lock: 충분히 진행됨
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner: FINAL REGRESSION
Gate 4 Production Pipeline Extraction: PREPARED / PENDING
Gate 5 Third Character Stress Test: PENDING
Gate 6 Production: PENDING
```

Gate 3 최종 체크:

```text
docs/GATE3_FINAL_REGRESSION.md
```

Gate 4 계획:

```text
docs/GATE4_PRODUCTION_PIPELINE_PLAN.md
```

---

# 4. Gate 1 — Core Combat · USER VERIFIED

## 현대 고죠

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

## 시부야 스쿠나 · 이타도리 육체

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

스쿠나 CE prototype:

```text
최대 160
시작 95
초당 회복 4
```

훈련 주령:

```text
Curse A / Curse B
TAB 전환
engagement slot 겹침 방지
```

---

# 5. Gate 2 — Match Architecture · USER VERIFIED

## Player Team

현재 팀:

```text
GOJO SATORU · 현대 · 교사
RYOMEN SUKUNA · 시부야 사변
```

입력:

```text
T · TAG
```

검증 완료:

```text
고죠 ↔ 스쿠나 장면 재시작 없는 Tag
HP 독립 보존
CE 독립 보존
Reserve 상태 정지
Action Lock 중 Tag 차단
첫 KO Auto Tag / DEFEAT 없음
KO Fighter 재Tag 차단
마지막 Player KO에만 DEFEAT
Tag 시 기본 콤보 초기화
카메라/Target Lock 유지
```

상세:

```text
docs/TEAM_MATCH_GATE2.md
docs/TEAM_HUD_CLEANUP.md
```

## Opponent Team

모드:

```text
F2
TRAINING · MULTI CURSE
TEAM BATTLE
```

검증 완료:

```text
상대 Active + Reserve
첫 Active KO 뒤 Reserve Entry
첫 KO에는 VICTORY 없음
마지막 상대 KO에만 VICTORY
Enemy Team HUD
Reserve 등장 시 Target Lock 자동 승계
```

상세:

```text
docs/GATE2B_OPPONENT_TEAM.md
docs/GATE2B_ENEMY_HUD_CLEANUP.md
docs/GATE2B_TARGET_HANDOFF.md
```

---

# 6. Gate 3A — Combat Feel · USER VERIFIED

```text
기본 공격 실제 적중 Camera Shake
1 < 2 < 3 FINISH 강도
Hit Stop
Hit Flash
Impact position procedural Burst
```

헛공격/실제 Damage 미적용 상황에는 적중 피드백이 나오지 않게 유지.

핵심:

```text
BasicAttack.cs
PrototypeHitStopController.cs
PrototypeHitImpactVfx.cs
SimpleCameraFollow.cs
```

---

# 7. Gate 3B — Prototype Character Presentation · USER VERIFIED

현재 실제 rigged model/Animator 자산은 저장소에 없다.

따라서 procedural avatar에 대해 다음만 검증했다.

```text
Gojo/Sukuna 기본 pose 차이
locomotion lean
dodge presentation
tag entry presentation
Fighter Shell 유지 상태에서 Active identity 교체
```

사용자 피드백상 동작은 정상이나 자세는 placeholder답게 하찮은 느낌이 강함.

최종 목표:

```text
실제 rigged model
Animator Controller
전용 locomotion/basic/dodge/technique/domain animation
```

상세:

```text
docs/GATE3B_CHARACTER_PRESENTATION.md
```

---

# 8. Gate 3C — Signature Technique Presentation · USER VERIFIED

검증:

```text
Signature Flash / Shake
Hit Stop
FOV Kick
Camera World Focus
Spatial Ring / Burst
푸가 Release / Explosion emphasis
무량공처 / 복마어주자 anticipation + activation
```

## 허식 「자」 중요 수정

기존:

```text
PurpleOuterBeam + PurpleCoreBeam
→ 긴 레이저처럼 표시
```

사용자 원작 기준:

```text
청색(창) + 적색(혁)
→ 합류
→ 하나의 큰 보라 구체
→ 전방 발사
```

첫 override/Bootstrap 방식은 실제 Unity에서 실패했고 compile error도 한 번 발생.

최종:

```text
PrototypeHollowPurplePresentationRuntime
```

독립 Runtime runner가 legacy Beam을 지속 비활성화하고:

```text
청색 + 적색 구체 수렴
→ 큰 보라 구체 생성
→ 약 18m 전방 이동
```

을 표시한다.

사용자 실제 확인:

```text
"그래 이제 좀 정상적이구나"
```

중요 Prototype 부채:

```text
Visual = 이동하는 구체
Damage = 기존 즉시 Capsule 판정
```

따라서 Visual/Damage timing은 아직 완전히 일치하지 않는다.

상세:

```text
docs/GATE3C_SIGNATURE_TECHNIQUE_PRESENTATION.md
docs/GATE3C_PASS3_SPATIAL_VFX.md
docs/GATE3C_PASS4_CINEMATIC_FRAMING.md
```

---

# 9. Gate 3D — Arena + HUD · FINAL REGRESSION

## Pass 1 Arena Mood · USER VERIFIED

```text
야간 Ambient / Fog
비충돌 도심 실루엣
Neon
Arena ring
Mood light
```

현재 건물은 배경 장식이라 통과 가능이 정상.

## Pass 2 Team HUD · USER VERIFIED

```text
Player Active / Reserve HUD
Opponent Active / Reserve HUD
HP / CE
Tag state
KO
Reserve Entry
F2 Mode strip
```

## Pass 3 Skill Deck

```text
PrototypeSkillDeckHud
```

고죠:

```text
Q 창
E 혁
R 허식 자
V 무량공처
```

스쿠나:

```text
Q 해
E 팔
R 푸가
V 복마어주자
```

상태:

```text
READY
DODGE
CASTING
DOMAIN INPUT
DOMAIN ACTIVE
TECHNIQUE BURNOUT
DISABLED
```

사용자 화면 첫 피드백은 긍정적이지만 전체 상태 전환까지 최종 회귀로 묶어 확인 예정.

상세:

```text
docs/GATE3D_ARENA_HUD_POLISH.md
```

---

# 10. 현재 바로 할 일

새 기능 추가보다 Gate 3 종료 회귀가 우선.

```text
NOW
1. git pull origin master
2. docs/GATE3_FINAL_REGRESSION.md 순서로 CombatMVP 한 세션 테스트
3. Console error / Tag / HUD / Purple / Fuga / Domain / Opponent Team / Victory/Defeat 확인
```

사용자가 최종적으로 정상 확인하면:

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: STARTED
```

로 갱신한다.

---

# 11. Gate 4 — Production Pipeline Extraction

목표:

```text
새 캐릭터 추가 시
고죠/스쿠나 코드를 복사하지 않고
공통 제작 절차를 반복 가능하게 만들기
```

우선:

```text
4A Character Presentation Contract
4B HUD Data Binding
4C Technique Presentation Request
4D VFX Lifecycle
4E Animation Contract
4F Audio Event Contract
```

거대한 범용 Ability System은 만들지 않는다.

## Codex / Agent

Gate 4는 여러 파일에 걸친 중복 탐색과 migration이 많아 Codex류 agent가 특히 유용한 단계다.

```text
agent multi-file edit
→ Git diff review
→ 작은 migration 단위
→ 사용자 Unity test
```

현재 ChatGPT 대화에 별도 로컬 Codex/Unity Editor 제어 도구가 연결되지 않았다면 실행했다고 가장하지 않는다.

사용자가 Unity AI Gateway / Codex를 로컬에 구성하면 Editor-side hierarchy/component/animator 작업과 코드 작업을 나눌 수 있다.

---

# 12. Gate 5 이후

Gate 5 Third Character Stress Test 후보:

```text
메구미 — 식신/소환 구조
유타 — 리카 동반체/상태 변화
```

Gate 5가 Gate 4 계약을 깨뜨리면 공통 구조를 수정한다.

그 뒤 Gate 6에서 캐릭터/맵/온라인/Incident Battle/PvE를 본격 확장한다.
