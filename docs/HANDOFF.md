# JJK Game 프로젝트 인수인계서

작성 기준일: 2026-08-23

> 새 ChatGPT 대화에서는 이 문서와 `docs/PROJECT_DIRECTION.md`, `docs/ROADMAP_SCOPE_REFINEMENT.md`, `docs/DEVELOPMENT_ROADMAP.md`, `docs/GATE4_PRODUCTION_PIPELINE_PLAN.md`, `docs/GATE4_PRODUCTION_BOUNDARIES.md`, `docs/GATE4_FINAL_REGRESSION.md`, `docs/GATE5A_THIRD_CHARACTER_STRESS_TEST.md`, 최신 코드를 먼저 읽는다.

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
Core 1v1
2~3인 Active/Reserve Team Battle
후속 4~8인 Incident Battle
```

핵심 Pillar:

```text
Character Authenticity
Asymmetric Combat
Domain as Match State
Spectacle + Destruction
Optional Hand Sign / Voice Immersion
```

캐릭터 고유 Gameplay Rule을 거대한 범용 Ability System에 억지로 합치지 않는다.

---

# 3. Gate 상태

```text
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED
Gate 5A Third Character Stress Test: IN PROGRESS
Gate 5B First Production-Quality Vertical Slice: PENDING
Gate 6 Production: PENDING
```

Gate 3 초기 production asset 목표가 뒤로 이동한 이유:

```text
docs/ROADMAP_SCOPE_REFINEMENT.md
```

---

# 4. Gate 1 / 2 핵심 검증

Gojo:

```text
기본 3타 / 회피
창 / 혁 / 허식 자 / 무량공처
무하한 / 영역전연 우회 / 필중 / 번아웃
```

Sukuna:

```text
해 / 팔 / 푸가 / 복마어주자
영역 밖 푸가
영역 안 광역 푸가
```

Team:

```text
T Tag
HP / CE 독립 보존
KO Auto Tag
마지막 Player KO만 DEFEAT
Opponent Reserve Entry
Target Lock handoff
마지막 상대 KO만 VICTORY
```

---

# 5. Gate 3 · USER VERIFIED

검증:

```text
Combat Feel
Prototype Character Presentation
Signature Technique Presentation
Arena Mood
HUD / Skill Deck
```

아직 production 자산 아님:

```text
실제 rigged character model
Animator animation set
final environment art
Canvas/TMP final HUD
production Voice/SFX/Music
```

허식 자 prototype debt:

```text
Visual = 이동 구체
Damage = 즉시 Capsule
```

---

# 6. Gate 4 · USER VERIFIED / CLOSED

Production-candidate contract:

```text
CharacterPresentationProfile
Player/Opponent HUD Snapshot
TechniquePresentationRequest
Presentation VFX Lifecycle
Fighter Animation State/Cue
CombatAudioEvent
```

Prototype-only 구현과의 경계:

```text
docs/GATE4_PRODUCTION_BOUNDARIES.md
```

최종 전체 회귀에서 사용자가 `정상` 확인.

```text
docs/GATE4_FINAL_REGRESSION.md
```

---

# 7. 현재 바로 할 일 — Gate 5A Pass 1

세 번째 캐릭터:

```text
후시구로 메구미
```

선택 이유:

```text
식신/소환체가 별도 전투 주체가 되어
고죠/스쿠나와 다른 구조로 Gate 4 contract를 stress test할 수 있음
```

현재 REMOTE IMPLEMENTED:

```text
PrototypeCharacterId.MegumiStudent
CharacterPresentationProfile 등록
Q 옥견 / E 누에 / R 만상 / V 감합암예정 metadata
MegumiPrototypeAvatar
PrototypeFighterPresentationController에서 Megumi root 선택
F3 Stress Roster toggle
- GOJO + SUKUNA
- GOJO + MEGUMI
```

중요:

```text
Gate 5A Pass 1에서는 메구미 Q/E/R/V Gameplay가 아직 없는 것이 정상.
먼저 identity / HUD / Team state / Animation contract가 세 번째 Character ID에서 버티는지 확인한다.
```

테스트:

```text
F3 → G/M roster
T → Megumi
HUD / Skill Deck / avatar
movement / basic 1-2-3 / Dodge
HP/CE Tag 보존
F3 → G/S 복귀
Gojo/Sukuna 회귀
Console 오류
```

세부:

```text
docs/GATE5A_THIRD_CHARACTER_STRESS_TEST.md
```

통과 후:

```text
Gate 5A Pass 2 · First Summon Gameplay
→ 옥견부터 구현
→ summon lifetime/target ownership 검증
→ Technique Presentation / VFX Lifecycle / Audio Event 재사용
```

---

# 8. Gate 5B

Gate 5A 이후 최초 production-quality 한 판을 만든다.

```text
실제 rigged model
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

---

# 9. Codex 사용 기준

```text
여러 파일 구조 변경 + Codex/agent 실행 인터페이스 실제 사용 가능
→ agent 우선 검토
→ diff review
→ 사용자 Unity test

작은 변경 또는 현재 agent 실행 인터페이스 없음
→ GitHub 직접 수정
→ 사용자 Unity test
```

실제로 실행하지 않았다면 Codex를 사용했다고 말하지 않는다.
