# Gate 2B — Opponent Team Architecture

작성 기준일: 2026-08-21

이 문서는 Gate 2B의 최소 상대 팀 구조와 사용자 테스트 결과를 기록한다.

## 상태

```text
Gate 1 Core Combat: USER VERIFIED
Gate 2A Player Active / Reserve: USER VERIFIED
Team-aware HUD Cleanup: USER VERIFIED
Gate 2B Opponent Team Core Runtime: USER VERIFIED
Gate 2B Enemy Team HUD: USER VERIFIED
Gate 2B Automatic Target Lock Handoff: USER VERIFIED
Gate 2B Opponent Team Architecture: USER VERIFIED
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Gate 2B 목적

Gate 2A에서 플레이어 쪽의 Active / Reserve 구조는 실제 Unity에서 검증됐다.

Gate 2B에서는 상대편도 다음 최소 구조를 가질 수 있는지 확인했다.

```text
PLAYER TEAM
Active + Reserve

VS

OPPONENT TEAM
Active + Reserve
```

기존 Curse A + Curse B 동시 전투 환경은 스쿠나의 다중 해, 복마어주자, 영역 안 광역 푸가 등을 회귀 테스트하는 데 필요하므로 없애지 않았다.

따라서 두 모드를 분리했다.

```text
TRAINING · MULTI CURSE
- Curse A + Curse B 동시 활성
- 기존 Gate 1 / Gate 2A 회귀 테스트 환경 보존

TEAM BATTLE
- Curse A Active
- Curse B Reserve
- Active KO 시 Reserve 자동 입장
- 마지막 상대 팀원 KO 시에만 VICTORY
```

## 구현 구조

파일:

```text
unity/Assets/Scripts/Enemy/PrototypeOpponentTeamController.cs
```

이번 단계에서는 MatchController와 CurseBotController를 대규모로 뜯어고치지 않았다.

`PrototypeOpponentTeamController`가 런타임에 현재 두 CurseBot을 찾아 Gate 2B 최소 구조를 얹는다.

이 선택은 현재 개발 방법인 `Risk-Driven Vertical Slice Production`에 따른 것이다.

## 모드 전환

현재 임시 입력:

```text
F2 · MODE
```

첫 Play 시작 기본값:

```text
TRAINING · MULTI CURSE
```

F2:

```text
현재 모드 반전
→ 같은 Scene 재시작
→ 선택한 모드로 다시 구성
```

```text
TRAINING → F2 → TEAM BATTLE
TEAM BATTLE → F2 → TRAINING
```

F2는 최종 게임 조작이 아닌 `GAME_ORIGINAL` 개발용 임시 입력이다.

## 첫 구현 버그와 수정

첫 사용자 테스트에서:

```text
F2 뒤에도 Curse 두 마리가 보임
다시 F2를 눌러도 복귀하지 않음
```

문제가 확인됐다.

원인:

```text
Scene reload 뒤 scene-local PrototypeOpponentTeamController가 파괴됐지만
새 Scene에서 모드 상태를 적용할 controller가 다시 살아나지 않음
```

수정:

```text
PrototypeOpponentTeamController를 DontDestroyOnLoad로 유지
SceneManager.sceneLoaded 이벤트 구독
Scene reload 뒤 새 Scene의 Curse A/B를 다시 탐색
requestedMode를 다시 적용
```

수정 후 사용자가 Training ↔ Team Battle 전환과 Team Battle 진행을 정상 확인했다.

## Team Battle 상대 구조

현재 임시 상대 팀:

```text
CURSE A · Active
CURSE B · Reserve
```

Team Battle 시작:

```text
Curse A 활성
Curse B 비활성 Reserve
```

Reserve는:

```text
- 전장에 보이지 않음
- Target Lock 후보가 아님
- 현재 전장 범위 공격 대상에서 제외
- 자기 HP가 변하지 않음
```

첫 Active KO:

```text
Curse A HP 0
→ VICTORY 미발생
→ Curse A 전장 이탈
→ Curse B 자동 활성
→ Curse B가 새 Active
→ 플레이어 Target Lock이 Curse B로 자동 승계
```

마지막 상대 KO:

```text
Curse B HP 0
→ 상대 팀 0/2
→ VICTORY
```

## Enemy Team HUD

Training:

```text
기존 CURSE A / CURSE B 개별 HP
CURSES 2/2
```

Team Battle:

```text
기존 개별 Enemy HUD 숨김
OPPONENT TEAM 패널만 표시

OPPONENT TEAM · 2/2
ACTIVE · CURSE A
RESERVE · CURSE B
```

첫 KO 뒤:

```text
OPPONENT TEAM · 1/2 · RESERVE ENTRY
ACTIVE · CURSE B
RESERVE · CURSE A · KO
```

최종 KO 뒤 사용자 캡처에서:

```text
OPPONENT TEAM · 0/2
두 행 모두 KO
VICTORY
```

를 확인했다.

## Target Lock 정책

사용자 결정 및 사용자 검증 완료:

```text
상대 Active KO
→ Reserve 자동 입장
→ Target Lock 자동 승계
```

기존 TAB 수동 전환/해제는 유지한다.

비활성 Reserve는 Target Lock 후보가 되지 않는다.

상세: `docs/GATE2B_TARGET_HANDOFF.md`

## 사용자 검증 기록

2026-08-21:

```text
[USER VERIFIED]
- Team Battle에서 상대가 한 번에 한 마리씩 등장
- 첫 상대 KO 뒤 Reserve가 차례로 자동 등장
- 첫 상대 KO에는 VICTORY가 뜨지 않음
- 마지막 상대까지 KO해야 VICTORY
- Team Battle Enemy HUD가 OPPONENT TEAM 단일 패널로 정리됨
- 최종 0/2 + VICTORY 화면 정상
- 새 Reserve 등장 직후 Target Lock 자동 승계 정상
```

Gate 2A에서 이미 별도로 확인된 플레이어 측 항목:

```text
- 고죠 ↔ 스쿠나 Active / Reserve
- HP / CE 독립 보존
- Action Lock
- 첫 KO Auto Tag / 마지막 KO DEFEAT
- 카메라 / Target Lock 유지
- 고죠/스쿠나 기술 회귀
- 두 Curse Training 환경
```

따라서 Gate 2 Match Architecture Proof의 핵심 리스크는 사용자 실제 Unity 검증을 통해 통과한 것으로 판정한다.

## Gate 2B Acceptance Criteria

```text
1. Training과 Team Battle 분리 ✅
2. F2로 Training ↔ Team Battle 전환 ✅
3. Training 두 Curse 동시 환경 보존 ✅
4. Team Battle 1 Active + 1 Reserve ✅
5. Reserve 전장/타깃 제외 ✅
6. 첫 상대 KO에 VICTORY 미발생 ✅
7. 첫 상대 KO 뒤 Reserve 자동 입장 ✅
8. KO된 이전 상대 재등장 없음 ✅
9. 마지막 상대 KO에만 VICTORY ✅
10. 새 Active Target Lock 자동 승계 ✅
11. Team Battle Enemy HUD Active/Reserve 표현 ✅
```

## 완료 판정

```text
Gate 2B Opponent Team Architecture: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
```

이 판정은 Gate 2A와 Gate 2B에서 누적된 사용자 Unity 검증 결과를 합쳐 내린다.

## 아직 하지 않는 것

```text
상대 수동 Tag AI
지원 공격
3인 상대 팀
실제 상대 주술사 캐릭터
교대 애니메이션
Character Cost
온라인
```

이 항목들은 Beauty Corner 전에 확장하지 않는다.

다음 단계:

```text
Gate 3 — Beauty Corner
```
