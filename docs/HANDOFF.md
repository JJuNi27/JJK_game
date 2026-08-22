# JJK Game 프로젝트 인수인계서

작성 기준일: 2026-08-23

> 새 ChatGPT 대화에서는 이 문서와 `docs/PROJECT_DIRECTION.md`, `docs/ROADMAP_SCOPE_REFINEMENT.md`, `docs/DEVELOPMENT_ROADMAP.md`, `docs/GATE4_PRODUCTION_PIPELINE_PLAN.md`, `docs/GATE4_PRODUCTION_BOUNDARIES.md`, `docs/GATE4_FINAL_REGRESSION.md`, 최신 코드를 먼저 읽는다.

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
→ 여러 파일 구조 변경은 agent/Codex가 실제 사용 가능할 때 우선 고려
→ 사용 불가능하면 GitHub 직접 수정했다고 명확히 말함
→ Git diff/코드 검토
→ 사용자 pull
→ 사용자 Unity 실제 테스트
→ USER VERIFIED만 완료 처리
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

속도 원칙:

```text
관련 작업은 2~5개 묶어서 검증
단, 핵심 Gameplay/컴파일 위험 migration은 작은 단위로 검증
```

---

# 2. 게임 방향

```text
Character-Authentic 3D Arena Action Fighter
```

```text
Core: 1v1
핵심 확장: 2~3인 Active/Reserve Team Battle
후속: 4~8인 Incident Battle
```

핵심 Pillar:

```text
Character Authenticity
Asymmetric Combat
Domain as Match State
Spectacle + Destruction
Optional Hand Sign / Voice Immersion
```

원작 우선. 캐릭터의 고유 Gameplay Rule을 거대한 범용 Ability System에 억지로 합치지 않는다.

---

# 3. Gate 상태

```text
Gate 0 Vision Lock: 충분히 진행됨
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: FINAL REGRESSION PENDING
Gate 5A Third Character Stress Test: PENDING
Gate 5B First Production-Quality Vertical Slice: PENDING
Gate 6 Production: PENDING
```

Gate 3 초기 계획과 실제 prototype scope 차이는:

```text
docs/ROADMAP_SCOPE_REFINEMENT.md
```

에 기록했다.

---

# 4. Gate 1 — Core Combat · USER VERIFIED

현대 고죠:

```text
이동 / 기본 3타 / 회피
Q 창
E 혁
R 허식 자
V 무량공처
무하한
영역전연 우회
DomainSureHit
술식 번아웃
```

시부야 스쿠나:

```text
Q 해
E 팔
R 푸가
V 복마어주자
영역 밖 푸가
영역 안 광역 푸가
```

---

# 5. Gate 2 — Match Architecture · USER VERIFIED

Player Team:

```text
Gojo ↔ Sukuna T Tag
HP / CE 독립 보존
Reserve freeze
Action Lock Tag 차단
첫 KO Auto Tag
마지막 Player KO에만 DEFEAT
Tag 시 콤보 초기화
카메라/Target Lock 유지
```

Opponent Team:

```text
F2 Training / Team Battle
Active + Reserve
첫 KO → Reserve Entry
첫 KO에는 VICTORY 없음
Reserve 등장 시 Target Lock 자동 승계
마지막 상대 KO에만 VICTORY
```

---

# 6. Gate 3 — Beauty Corner Prototype Proof · USER VERIFIED

사용자 최종 판정:

```text
다 정상!
```

검증:

```text
Combat Feel
Prototype Character Presentation
Signature Technique Presentation
Arena Mood
HUD Information Hierarchy
```

현재 Production 자산은 아직 없음:

```text
실제 rigged Gojo/Sukuna model
Animator animation set
final environment art
Canvas/TMP final HUD
production Voice/SFX/Music
```

허식 자 prototype debt:

```text
Visual = 이동하는 보라 구체
Damage = 기존 즉시 Capsule
```

---

# 7. Gate 4 — Production Pipeline Extraction

상세:

```text
docs/GATE4_PRODUCTION_PIPELINE_PLAN.md
```

현재 결과:

```text
4A Character Presentation Contract: USER VERIFIED
4B HUD Data Binding: USER VERIFIED
4C Technique Presentation Request: USER VERIFIED
4D VFX Lifecycle Contract: USER VERIFIED
4E Animation Contract: USER VERIFIED
4F Audio Event Contract: USER VERIFIED
Prototype / Production Boundary Review: COMPLETE
Gate 4 Final Regression: USER TEST PENDING
```

Production-candidate contract:

```text
CharacterPresentationProfile
Player/Opponent HUD Snapshot
TechniquePresentationRequest
Presentation VFX Lifecycle
Fighter Animation State/Cue
CombatAudioEvent
```

Prototype-only renderer/runtime과 구분:

```text
docs/GATE4_PRODUCTION_BOUNDARIES.md
```

주의:

```text
현재 procedural avatar / LineRenderer VFX / IMGUI HUD / fallback audio는 최종 자산이 아니다.
Gate 5B에서 contract를 유지하고 실제 모델/Animator/VFX/UI/Audio consumer로 교체한다.
```

---

# 8. 현재 바로 할 일

```text
Gate 4 Final Regression
```

체크리스트:

```text
docs/GATE4_FINAL_REGRESSION.md
```

한 세션에서 확인:

```text
Gojo combat + Purple + Unlimited Void
Player Tag + HP/CE 보존
Sukuna 해/팔/푸가/복마어주자
Player first KO auto-tag / final defeat
Opponent Team reserve entry / target handoff / final victory
HUD snapshot 상태
Camera / HitStop
VFX lifecycle 중복/잔류 없음
Audio 중복/누락 없음
F2 scene reload
Console 핵심 오류 없음
```

사용자가 전체를 정상 확인하면:

```text
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED
Gate 5A Third Character Stress Test: START
```

---

# 9. Gate 5A / 5B / 6

## Gate 5A — Third Character Stress Test

목적:

```text
고죠/스쿠나와 구조가 다른 세 번째 Fighter 추가
→ Gate 4 contract 실제 재사용성 검증
→ 공통 Presentation/HUD를 복붙해야 하면 contract 보강
```

후보:

```text
메구미 — 식신/소환 구조
유타 — 리카 동반체/상태 변화
```

## Gate 5B — First Production-Quality Vertical Slice

Gate 3에서 자산 부재로 미룬 production-quality 목표를 회수한다.

```text
실제 rigged character model
Animator + animation set
production-grade technique VFX
Canvas/TMP final-style HUD
production SFX / Voice / Music
대표 맵 art / material / lighting / post process
```

목표:

```text
"이제 실제 주술회전 게임처럼 보이고 느껴진다"
```

## Gate 6 — Production

Gate 5A 구조 검증 + Gate 5B 품질 검증 뒤:

```text
캐릭터 로스터 확장
맵 확장
게임 모드 확장
온라인
Incident Battle
PvE / Story
```

을 순차 생산한다.

---

# 10. Codex 사용 기준

Codex가 유리한 작업:

```text
여러 파일 구조 변경
Character/Team/Match 리팩터링
반복 migration
테스트 추가
코드베이스 전체 탐색이 필요한 변경
```

작은 1~3파일 수정은 직접 수정이 더 빠를 수 있다.

중요:

```text
Codex가 실제 실행 가능한 경우에만 "Codex 사용"이라고 말한다.
현재 대화에 Codex 실행 도구가 없다면 GitHub 직접 작업으로 진행한다.
```
