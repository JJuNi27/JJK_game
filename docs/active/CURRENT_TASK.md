# CURRENT TASK — 시부야 PASS 3D 검증 마무리

작성 기준: **2026-10-20**  
브랜치: `feat/shibuya-combat-mvp`  
마지막 USER VERIFIED 원격 HEAD: `6655f19e7ef64f7a6e6a5c36876057a9d881c3f0` (PASS 3C)

**현재:** PASS 3D LOCAL UNCOMMITTED / 신규 시네마틱 **18 PASS** / 기존 회귀 최신 케이스별 **134 PASS · 1 FAIL** / 전체 CODEX VALIDATED 미완료 / USER VERIFIED 미승인.

이 상태는 사용자가 전달한 Codex 로컬 조사 결과다. GitHub 문서 갱신은 PASS 3D 소스나 실제 로컬 상태의 변경·검증을 뜻하지 않는다.

## 1. 이번 작업의 범위

유효한 비치명적 마무리 타격에만 발동하는 **참가자 2인 시네마틱**을 보존하며, 남은 PASS 3C 공중 공격 회귀 실패의 직접 원인과 PASS 3D 신규 회귀 여부를 확인한다. 필요한 최소 수정·검증 후 사용자 Play Mode 검수 준비.

**금지:** PASS 4 선행 구현, 시각/술식/모델의 범위 외 변경, 기존 공중 타격 값이나 테스트 기대치 임의 완화.

## 2. 자동검증 현황

- PASS 3D 신규 시네마틱 18개 고유 PASS; Codex가 원본 XML을 재검토했다고 보고.
- 기존 회귀 135개: **최신 케이스별 집계 134 PASS / 1 FAIL / 미실행 0**.
- 첫 135개 실행은 **122 PASS / 13 FAIL**. 이후 실패 중심 재검증을 통해 최신 집계 변경; **단일 135 PASS 실행은 없음**.
- QA 원본 XML/실행 로그/초기 실패 이력 보존. 현재 PASS 3D 전체 CODEX VALIDATED 불가.

실패 테스트:  
`JJKGame.Shibuya.EditorTools.ShibuyaPass3CAirCountTests.ThreeClicksOnHitRequireThreeAcceptedSteps`

| 항목 | 기대 | 실제 |
|---|---:|---:|
| 유효한 공중 LMB 입력 | 3 | 3 |
| 수락된 단계 | 3 | 3 |
| 타격 이벤트 | 5 | 5 |
| 실제 피해 적중 | 5 | **4** |
| 피해 | 62 | **50** |

대표 실패의 타격 이벤트별 Z 간격: **1.720 / 2.019 / 2.055 / 2.089 / 2.125m**. 마지막 타격 시 높이 차 약 0.671m, 체공 상한까지 추정 58ms. 별도 계측에서는 Z 약 2.136m/높이 약 0.714m에서 `OverlapSphere` 범위 이탈 (두 기록은 **서로 다른 실행**).

코드 조사 결과: `AdvanceTargeting`은 공중 세션에 지상 자동 추적을 적용하지 않으며, `AimAirAttack`은 회전 보정만 수행한다. 현 공중 기본 공격 이동 시퀀스는 비어 있고 전진거리 0. 피격자의 수평 넉백·어퍼컷 수평 이동은 공격자와의 간격을 늘릴 수 있다.

**확인:** 수평 거리 증가로 인한 물리 적중 범위 이탈.  
**미확정:** PASS 3D 신규 회귀인지, 기존 PASS 3C 잠재 문제인지 (**INCONCLUSIVE**). 관련 11개 파일은 PASS 3C 승인본과 같다고 보고되지만 `ShibuyaFighter`에는 PASS 3D 연결 변경 존재. 동등 조건 A/B 비교 없음. 별도 60FPS 시도는 준비 단계 시간 초과였으므로 마지막 타격 실패 재현 근거 아님.

## 3. 권장 조사·검증 순서

1. 실제 로컬 브랜치/dirty tree와 Unity Editor 상태부터 확인. PASS 3D 미커밋 파일·QA 로그 보호.
2. 기존 QA XML/물리 계측/프레임 지연 기록을 재활용한다. 증거를 덮어쓰거나 대량 정리하지 않는다.
3. PASS 3C 승인 기준과 PASS 3D 현재 소스의 실행 차이를 **비파괴적**으로 확인할 A/B 검사 방법을 먼저 검토한다. 새 worktree/강제 checkout/reset/clean은 별도 허락 없이 금지.
4. 공통 `ShibuyaFighter` 연결·공중 이동·체공/히트 판정 순서를 비교해, 로직·테스트 조건·프레임 지연의 영향을 구분한다.
5. 원인을 증거로 확인한 뒤 최소 범위만 수정하고 관련 targeted·회귀 검사를 수행한다. 테스트 판정 및 승인값을 완화하지 않는다.
6. 실제 사용자 확인 사항과 미완료·불확실성을 보고한다.

## 4. PASS 3D 사용자 직접 검수

씬: `unity/Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`

Play Mode ON 메뉴:  
`JJK Game → 시부야 PASS3D → 시네마틱 예제 ON (Play Mode 전용)`

테스트:
1. Play Mode 진입 후 예제 ON.
2. W/S 없이 일반 LMB 4타를 더미에 실제 적중시킨다.
3. 약 0.5초의 짧은 **3샷** 시네마틱과 정상 카메라 복귀 확인.
4. 마지막 타격 미적중·취소·전투 종료에서 잘못된 시네마틱/보호 상태가 유지되지 않는지 확인.

OFF 메뉴:  
`JJK Game → 시부야 PASS3D → 시네마틱 예제 OFF (Play Mode 전용)`

예제 기본 저장 OFF. 사용자 직접 플레이 전까지 USER VERIFIED 미승인. 지금 환경의 1인 플레이+더미 검증이 네트워크 다인전 검증을 뜻하지 않는다.

## 5. 안전·완료 보고

- Unity Game View를 `Play Unfocused`로 바꾼 로컬 설정과 원본 백업은 확인됐으나, 실제 Windows 포커스 문제 해결 여부는 미확정.
- 마지막 Codex 읽기 전용 보고 당시 staged 0, unstaged 8, untracked 15. `docs/SHIBUYA_PASS3D_REVIEW.md`는 로컬 untracked. 새 세션에서 재확인.
- 기존 Gojo/VFX 및 PASS 3C USER VERIFIED 기능·데이터·Scene/Prefab/가이드 보존.
- 무단 staging/commit/push/reset/clean/checkout/worktree 생성, Unity 강제 포커스 전환, 기존 QA 증거 삭제 금지.
- 종료 보고: 확인된 원인/미확정 사항, 변경 파일, 테스트별 PASS/FAIL/SKIP·근거 위치, 최종 Git 상태, 사용자 Play Mode 확인 사항.

관련: `docs/CURRENT_HANDOFF.md`, `docs/SHIBUYA_PASS3C_REVIEW.md`, `docs/locked/USER_VERIFIED_SETTINGS.md`.
