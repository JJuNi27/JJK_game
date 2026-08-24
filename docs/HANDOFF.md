# JJK Game 프로젝트 인수인계서

작성 기준일: 2026-08-24

> 새 ChatGPT 대화에서는 이 문서와 `docs/PROJECT_DIRECTION.md`, `docs/ROADMAP_SCOPE_REFINEMENT.md`, `docs/DEVELOPMENT_ROADMAP.md`, `docs/CHARACTER_TEAM_SELECT_PLAN.md`, `docs/GATE5A_THIRD_CHARACTER_STRESS_TEST.md`, `docs/GATE5B_FIRST_PRODUCTION_SLICE.md`, 최신 코드를 먼저 읽는다.

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

Codex 환경:

```text
VS Code OpenAI Codex 사용
Context7 MCP 연결 및 실제 query tool 호출 확인 완료
Context7은 Unity/package API 문서가 버전 민감할 때 선택적으로 사용
```

---

# 2. 게임 방향

```text
Character-Authentic 3D Arena Action Fighter
Core 1v1
1~3인 Active/Reserve Team Battle
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
Gate 5A Third Character Stress Test: USER VERIFIED / CLOSED
Gate 5B First Production-Quality Vertical Slice: STARTED
- Pass 1 Character / Team Select Data Boundary: USER VERIFIED
- Pass 2 Runtime 1~3인 Team Wiring: REMOTE IMPLEMENTED / USER TEST PENDING
Gate 6 Production: PENDING
```

Gate 3 초기 production asset 목표가 뒤로 이동한 이유:

```text
docs/ROADMAP_SCOPE_REFINEMENT.md
```

---

# 4. 검증 완료 핵심

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

Gate 2 Team:

```text
2인 Active / Reserve
T prototype Tag
HP / CE 독립 보존
KO Auto Tag
마지막 Player KO만 DEFEAT
Opponent Reserve Entry
Target Lock handoff
마지막 상대 KO만 VICTORY
```

Gate 3:

```text
Combat Feel
Prototype Character Presentation
Signature Technique Presentation
Arena Mood
HUD / Skill Deck
```

Gate 4 production-candidate contracts:

```text
CharacterPresentationProfile
Player/Opponent HUD Snapshot
TechniquePresentationRequest
Presentation VFX Lifecycle
Fighter Animation State/Cue
CombatAudioEvent
```

---

# 5. Gate 5A · USER VERIFIED / CLOSED

세 번째 캐릭터:

```text
후시구로 메구미
```

검증:

```text
Character ID / Profile / HUD
Team HP / CE 보존
Animation contract
Q 옥견 별도 combat actor
옥견 추적 / 공격 / Damage
Technique Presentation / VFX Lifecycle / Audio Event 재사용
```

초기 옥견 Q 입력 실패는 `ApplyMegumi()`에서 `MegumiTechniqueController` attach/enable을 보장하는 방식으로 해결했다.
사용자 최종 결과: 옥견 소환/추적/공격 정상, cleanup regression 정상.

F3/T 등은 developer harness다.

---

# 6. Gate 5B 최종 방향

production-quality representative slice:

```text
고죠 vs 스쿠나 대표 매치
실제 rigged model
Animator + 실제 animation set
production-grade VFX
Canvas/TMP final-style HUD
production SFX / Voice / Music
대표 맵 art / material / lighting / post process
정식 Character / Team Select
```

최종 Team Select / Tag:

```text
1명: MAIN
2명: MAIN + R1
3명: MAIN + R1 + R2

1 → Active ↔ R1
2 → Active ↔ R2
```

UI 흐름:

```text
Character / Team Select UI
→ MatchTeamSelection
→ MatchTeamSelectionStore
→ Battle Team Runtime
```

---

# 7. 현재 바로 할 일 — Gate 5B Pass 2 회귀

Pass 1에서 다음 데이터 경계를 만들고 사용자 compile/start 확인 완료:

```text
MatchTeamSelection.Solo / Duo / Trio
MatchTeamSlot Main / Reserve1 / Reserve2
MatchTeamSelectionStore
```

Pass 2 REMOTE IMPLEMENTED:

```text
PrototypePlayerTeamController
- MatchTeamSelectionStore를 실제 roster source로 사용
- Team Size 1~3
- members[0] Active / [1] R1 / [2] R2
- 1 = Active ↔ R1
- 2 = Active ↔ R2
- HP / CE / KO state와 slot 자체를 교환
- KO 시 첫 living reserve auto tag

PlayerCombatHudDataSource
- TeamSize
- Reserve2Member
- R1/R2 tag state

CombatInputBindings
- Reserve1Tag = 1
- Reserve2Tag = 2
```

Developer harness:

```text
F3 = G/S ↔ G/M Duo
F4 = Duo G/S → Trio G/S/M → Solo Gojo → Duo G/S
T = R1 compatibility tag
```

현재 F3/F4/T는 production UX가 아니다.

다음 사용자 테스트는 `docs/GATE5B_FIRST_PRODUCTION_SLICE.md`의 Pass 2 회귀를 따른다.
특히 3인 팀에서:

```text
1/2 slot swap
각 캐릭터 HP/CE 보존
Megumi Q 옥견
첫 KO → R1
두 번째 KO → R2
마지막 KO에만 DEFEAT
Solo에서 1/2 비활성
F2 Opponent Team 기존 회귀
Console red error 없음
```

통과 후:

```text
Gate 5B Pass 2: USER VERIFIED
NEXT: Character / Team Select Canvas/TMP front-end
```

---

# 8. 아직 production 자산 아님

```text
실제 rigged character model 없음
production Animator animation set 없음
현재 procedural prototype avatars
현재 prototype/IMGUI HUD
prototype city/environment
production Voice/SFX/Music 미연결
```

허식 자 prototype debt:

```text
Visual = 이동 구체
Damage = 즉시 Capsule
```

---

# 9. Codex / Agent 기준

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
