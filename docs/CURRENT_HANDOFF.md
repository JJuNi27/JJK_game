# JJK_game — Current Project Handoff

작성 기준: **2026-10-20**  
저장소: `JJuNi27/JJK_game` · 브랜치: `feat/shibuya-combat-mvp`  
마지막 GitHub 승인 HEAD: `6655f19e7ef64f7a6e6a5c36876057a9d881c3f0` (PASS 3C USER VERIFIED)

> **원격/로컬 구분:** PASS 3D는 로컬 미커밋 작업으로 보고되었다. 이 문서의 PASS 3D 세부 상태는 사용자가 가져온 2026-10-20 Codex 보고 기반으로, GitHub의 코드에 존재한다는 뜻이 아니다. 새 세션은 실제 로컬 Git/QA 상태를 확인해야 한다.

## 1. 프로젝트 방향

Unity 6 URP 기반 주술회전 3D 자유 이동 격투게임 `JJK_game`.

- **현재:** 시부야 교차로 블록아웃·임시 파이터/더미로 공통 전투의 조작감, 피격/추적/공중전, 이동·충돌 시스템 검증. 첫 MVP는 1:1.
- **장기:** 캐릭터 선택, 개별 무브셋·술식·영역전개·각성, 넓은 시부야 도시 전투, 환경 파괴, 자유 난전과 다인전·멀티플레이.
- **고죠:** 첫 제작 캐릭터이지 고정 주인공 아님. 기존 고죠 모델·애니메이션, VFXLab, Blue/Red/Purple, 무량공처 및 USER VERIFIED 설정은 보존하고 나중에 새 엔진에 통합.
- **개발 규칙:** Data Asset에서 게임플레이 튜닝(코드는 HOW, 데이터는 WHAT), Unity Inspector의 실제 사용자 설정 필드는 한국어, 기존 승인 기능 보호.

상세: `AGENTS.md`, `docs/design/GAME_VISION.md`, `docs/design/SHIBUYA_COMBAT_REBOOT.md`.

## 2. 2026-10-17 개발 방향 전환

이전에는 고죠 1명의 고품질 기술/VFX를 먼저 완성하는 순서였다. 현재는 **누구나 사용할 수 있는 재미있는 공통 전투 엔진을 먼저 만들고**, 그 위에 실제 캐릭터·전용 무브셋·고급 VFX를 결합하는 순서로 바꿨다.

이는 고죠 VFX를 폐기하거나 품질을 포기한 결정이 아니다. 기존 백업 브랜치 `backup/gojo-vfx-wip-20261017`의 체크포인트 `540272aaf609f7498d01bc9a96ea85ba678ed1cd`와 별도 ZIP 백업이 있다고 이전 Codex가 보고했다. 백업에 모든 QA 원시 로그까지 담겨 있다고 가정하지 않는다.

이 파일을 덮어쓰기 전의 **고죠 Purple 인수인계 원문은 Git 체크포인트 `6655f19`에서의 `docs/CURRENT_HANDOFF.md`** 및 `docs/PURPLE_TEMPEST_POLISH_REVIEW.md`에서 확인한다. 과거 Purple의 시각 승인 대기 상태는 현재 PASS 3D 승인 상태와 별개다.

## 3. 승인 완료된 시부야 PASS

마지막 USER VERIFIED 원격 커밋: `6655f19e7ef64f7a6e6a5c36876057a9d881c3f0`.

- **PASS 1:** 교차로 전투 씬, 이동/카메라, HP, 승패·재시작.
- **PASS 2~2.3:** 기본 LMB 4타, 피격·넉백·방향 정렬·국소 히트스톱.
- **PASS 3A:** 데이터 기반 공통 무브셋, 한 입력 안의 순차 다중 타격, 제한된 넉백 후 접근.
- **PASS 3B/3B.1:** 마지막 4타에서 W 어퍼컷 / S 다운, 근접 유한 추적, 공중 궤도·착지, 지면 균열.
- **PASS 3C:** 어퍼컷 적중 후 선택적 Space 공중 추격, 일반 점프와 공통 공중 LMB, 제한된 체공·착지. 공중 LMB 세 번의 최대 2+2+1 이벤트는 서로 다른 공중 기술 데이터 세 개가 아니라 유효 입력으로 공통 단계를 연계하는 구조다.

승인 범위는 `docs/SHIBUYA_PASS3C_REVIEW.md` 및 각 PASS 보고서에 명시된 직접 플레이 범위로 제한한다. 자동 검사 합격은 USER VERIFIED와 다르다. `docs/locked/USER_VERIFIED_SETTINGS.md`는 고죠의 보호값도 포함한다.

## 4. 현재 작업 — PASS 3D (로컬)

**LOCAL UNCOMMITTED / CODEX VALIDATED 미완료 / USER VERIFIED 미승인.**

목표: 실제 피해가 적용된 **비치명적 마무리 적중**에만 짧은 2인 시네마틱. 미적중/무적/치명적 종료에 오작동하지 않고, 제3자 피해·카메라·전투 시간을 전역 정지하지 않는다. 참여자 두 명에게 한정된 카메라 및 보호 상태를 적용하고 취소·전투 종료에 복구한다. 약 0.5초/3샷 예제, 데이터/한국어 Inspector, 저장 기본 OFF, Play Mode 전용 선택.

**사용자가 전달한 2026-10-20 Codex 결과:**
- PASS 3D 신규 시네마틱 **18개 고유 검사 PASS** (기존 XML 재검토 보고).
- 기존 회귀 135개: **케이스별 최신 134 PASS / 1 FAIL / 미실행 0**. 여러 실행에서 최신 결과를 모은 수치로 **단일 전체 135 PASS 실행이 아니다**.
- 남은 실패: `ShibuyaPass3CAirCountTests.ThreeClicksOnHitRequireThreeAcceptedSteps`.
- 입력 3회/수락 단계 3개/타격 이벤트 5개는 정상이나, 실제 적중은 **4/5**, 피해 **50/62**.
- 대표 실패에서 마지막 이벤트 시 공격자·피격자 Z 간격 약 **2.125m**, 높이 차 약 **0.671m**. 별도 물리 검사에서 `OverlapSphere` 제외도 확인.
- 확인된 직접 현상은 공중 수평 거리 증가에 따른 물리 사거리 이탈. **PASS 3D 신규 회귀인지, PASS 3C 잠재 문제인지는 INCONCLUSIVE**. 동일 환경 A/B 비교 미수행. PASS 3D 공통 `ShibuyaFighter` 연결 코드는 변경됨.
- 원본 QA XML·실패 기록을 보존했다고 보고. 실패가 남았으므로 전체 CODEX VALIDATED/USER VERIFIED 선언 금지.

현재 작업 상세: `docs/active/CURRENT_TASK.md`.
로컬 전용 `docs/SHIBUYA_PASS3D_REVIEW.md`는 untracked라고 보고됐으며 이 GitHub 문서 갱신에 포함하지 않는다.

## 5. 로컬 Git/Unity 상태 (2026-10-20 Codex 읽기 전용 보고)

- 로컬 브랜치 및 이전 원격 추적 ref `feat/shibuya-combat-mvp`, HEAD `6655f19`, ahead/behind 0/0 (fetch 없이 확인한 값).
- staged **0**, unstaged **8**, untracked **15**. PASS 3D 코드·데이터와 위 보고서 중심.
- `AGENTS.md`, `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md`에는 로컬 미커밋 변경 없음.
- Unity `Play Unfocused` 설정/백업은 확인했으나 실제 Windows 포커스 문제 해결은 아직 미확정.
- 이 현황은 그때의 로컬 조사 결과다. 새 세션 시작 시 재확인한다.

## 6. 당장 해야 할 일과 이후 후보

1. 로컬 dirty tree, QA 증거 및 Unity 프로세스를 읽기 전용으로 확인.
2. 공중 마지막 타격 실패의 사거리/타이밍/추적 호출 경로를 조사. PASS 3C 승인 기준과 PASS 3D WIP를 동등 조건에서 비교할 **비파괴적 A/B 방법**을 먼저 설계.
3. 기존 PASS 3C 수치·타격 판정 assertion을 느슨하게 바꾸지 않고 최소 수정·타깃 검증. 원본 실패 이력 보존.
4. 사용자가 PASS 3D 시네마틱을 Play Mode에서 직접 검수, USER VERIFIED 여부 결정.
5. 사용자 승인 후에만 별도 Git checkpoint.

PASS 3D 이후에는 **방향 대시·회피, 가드, 패링, 잡기, 긴급 탈출**, 이후 주력·술식·각성·AI·HUD·캐릭터별 모션·기존 고죠 VFX 통합·다인전 확장이 후보. **세부 PASS 번호와 구현 순서까지 확정된 것은 아니므로 사용자와 토론해 합의한다.**

## 7. 중요 보호 및 문서 확인 순서

문서: `AGENTS.md` → 이 문서 → `docs/active/CURRENT_TASK.md` → 관련 PASS 보고서 → `docs/locked/USER_VERIFIED_SETTINGS.md` → `docs/design/SHIBUYA_COMBAT_REBOOT.md`.

로컬 선호 경로 `D:\JJK_game` 한 곳. 기존 Gojo/VFX, 승인된 씬·데이터, 로컬 PASS 3D, QA 로그 보호. 사용자 승인 없는 commit/push, reset/clean, 위험한 checkout/restore, worktree 생성, 광범위한 자산 변경 금지.
