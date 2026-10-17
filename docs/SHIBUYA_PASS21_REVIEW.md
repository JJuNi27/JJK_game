# 시부야 PASS 2.1 — 최종 검증

> 최신 상태 (2026-10-18): 사용자가 PASS 2~2.3 전체를 Play Mode에서 검수하고 현재 체술에 만족한다고 명시하여 **USER VERIFIED 체크포인트**로 승인했다. 아래는 구현 당시의 검증 기록이다. 최신 동작·기본값·승인 범위는 [통합 체크포인트](SHIBUYA_COMBAT_PASS2_23_CHECKPOINT.md)를 따른다.

상태: **CODEX VALIDATED / 사용자 Play Mode 검수 대기**. USER VERIFIED가 아니다.

- Branch: `feat/shibuya-combat-mvp`
- HEAD: `0f416389c602ed601aa6447238a520f007550ef0` 유지
- staging / commit / push / Computer Use 미실행. PASS 3 미착수.

## 중단 상태와 해결

중단 직전 전체 기록은 14개 중 13 PASS / 1 FAIL이었다. PASS 2.1 신규 7개와 PASS 1 회귀 2개는 이미 통과했고, 기존 PASS 2의 W 전진 연타 검사만 시간 초과로 실패했다. 기존 결과는 `unity/Logs/ShibuyaPass21QA/pre-resume-*`로 보존했다.

재개 시 Editor는 종료되어 있었으며 실행 중인 테스트가 없었다. CLI로 동일 프로젝트를 열고 씬 isDirty=false, Dummy Stats=1000 및 자산 isDirty=false를 확인했다.

진단 결과 W 이동과 자동 전진이 합쳐져 공격자가 더미를 지나쳤고, 3·4타가 빗나갔다. 회귀 검사를 느슨하게 바꾸지 않았다. ShibuyaFighter의 기존 이동 계산 안에서 동일 CharacterController 캡슐을 전방 검사하여 다른 전투원을 통과하지 않게 최소 보정했다. 이는 충돌 여유 처리이며 새로운 타깃 획득이나 추적 강화가 아니다. 실제 위치 이동은 여전히 CharacterController.Move 한 경로에서만 수행한다. 실패 진단과 통과 후 위치/타격 기록은 모두 보존했다.

## 최종 결과

**유효 항목 합계 14 PASS / 0 FAIL / 0 SKIP**.

- 이번 최종 검증에 사용한 재실행: 10 PASS / 0 FAIL.
- 변경의 영향을 받지 않는 기존 데이터·입력·취소 검사: 4 PASS 유지.
- 단일 실행 14 PASS라고 표현하지 않는다. 재개 중 원인 진단에서 재현한 실패는 별도 기록에 남아 있다.

| 영역 | 결과 / 근거 |
|---|---|
| PASS 1 이동·점프·자유 카메라·벽·피격·승패·재도전 | 실제 Play Mode 회귀 재실행 PASS |
| W 전진 + 좌클릭 4타 | 기존 실패 검사 재실행 PASS. 공격 사이 위치 재설정 없음 |
| 제자리 좌클릭 4타 | PASS. 체력 1000 더미에 4회 적중, 85 피해 |
| 개별 피해·판정·타이밍·다중 Collider 중복 방지 | PASS 2 적중 검사 재실행 PASS |
| 입력 버퍼·초기화·누르기 유지·피격/사망 취소 | 기존 유효 PASS 유지 |
| 히트스톱 중 버퍼 | PASS. 국소 정지 중 예약한 한 타가 보존되며 연타로 중복 실행되지 않음 |
| 제한 보정 | PASS. 준비 구간·최대 회전/거리 제한·방향 입력 우선·대상 이탈·벽 차단 |
| 강타의 정지 및 복귀 | PASS. 공격자/피격자 로컬 시계·이동 정지, 이후 넉백 재개 및 경직 종료 후 피격자 공격 가능 |
| 빗나감/무적 | PASS. 피해·히트스톱·경직 미발생 |
| 반복 1타 | 기본 데이터에서 경직 종료 후 피격자가 실제 공격 가능한 구간 확인. 임의의 미래 밸런스/PvP 전체를 보장하지 않음 |
| 한글 Inspector / 데이터 | 기존 SerializedProperty·Undo 검사와 신규 필드 한글 매핑 PASS |

최종 국소 히트스톱 관측: 설정 65ms, 약 **67.1ms**. 전역 Time.timeScale=1. 정지 구간 동안 제3자 로컬 시계는 약 67.1ms 진행했고 약 **0.322m** 이동했다. `impact-metrics.json`에 원시 수치를 기록했다.

최종 컴파일 실패 0, 현재 Console 오류 0 / 경고 3. 경고는 기존 `Deleting invalid font reference.`이며 범위 밖 수정을 하지 않았다. 종료 시 Play Mode OFF, ShibuyaCombatMVP 미저장 변경 없음, QA Profile 잔존 0.

증거 폴더: `unity/Logs/ShibuyaPass21QA/`

- `final-validation-summary.json`: 검사별 최신 결과와 원본 출처
- `pre-resume-results.xml`: 이전 13 PASS / 1 FAIL 기록
- `resume-forward-results.xml`: W 전진 연타 통과
- `resume-pass21-results.xml`: 신규 7개 재검증
- `resume-pass1-results.xml`: PASS 1 플레이 루프 재검증
- `resume-pass2-hit-results.xml`: PASS 2 개별 타격 재검증
- `resume-before-fix-*`, `resume-forward-trace.jsonl`: 원인 재현/위치·타격 진단

## 설정과 보존

설정 위치: `Assets/Shibuya/Data/Shibuya Combat Profile.asset`.

- 타격별 전진 최대 거리·속도·시작·지속 시간. 기본 1타 0.70m, 2·3타 0.55m.
- 보정 사용 여부, 대상 최대 거리 2.7m, 허용 각도 35도, 누적 회전 상한 18도, 회전 속도 180도/초, 추가 전진 상한 0.16m, 속도 3m/s, 준비 구간 적용 시간 0.07초, 방향 입력 우선 각도 60도.
- 힘 모으기 시간·해당 구간 이동 배율·국소 히트스톱·타격음 높낮이. 4타 힘 모으기 0.10초는 기존 준비 시간 안에 포함되며 별도의 추가 지연이 아니다. 기존 피해·준비/활성/회복·판정·경직·넉백·버퍼 설정을 유지한다.
- 타격 연출 강도는 기존 설정을 사용한다. 새로운 카메라 반응은 추가하지 않았다.
- Dummy Stats 1000은 디스크/Editor에서 보존했다. 기존 80 HP 승리 검사는 런타임 현재 체력만 조절하며 Data Asset의 1000을 수정하지 않는다.
- CharacterStatsProfile의 공격·방어 배율 계산은 변경하지 않았다.
- Gojo/VFX, USER VERIFIED 문서, 기존 씬/프리팹, Packages/ProjectSettings는 보존했다.

## 수정 파일 구분

이번 재개에서 실제 수정한 기존 파일:

- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs` — 공격 중 전투원 관통 방지
- `unity/Assets/Shibuya/Editor/ShibuyaPass2Tests.cs` — 실패 경과 기록
- `unity/Assets/Shibuya/Editor/ShibuyaPass21Tests.cs` — 강타 경직 복귀 및 빗나감 비경직 명시 검사
- 추가 문서: `docs/SHIBUYA_PASS21_REVIEW.md`

중단 전부터 있던 Profile·Data·한글 Inspector·모션·타격음·입력·PASS 1/2/2.1 테스트/기록기의 로컬 변경은 유지했다. 전체 Git 상태는 최종 답변과 `final-validation-summary.json`을 참고한다.

## 직접 Play Mode 확인

ShibuyaCombatMVP를 열어 더미 정상 사거리에서 W 없이 좌클릭 연타하고, W를 누른 경우와 비교한다. 1~3타의 짧은 전진·넉백, 4타 준비 자세→적중 멈춤→큰 넉백, 클릭 유지 시 비자동 연타를 확인한다. 타격감·리듬·음량·시각 모션의 만족도는 사용자 검수 대상이다. 세부 밸런스와 여러 움직이는 적 사이의 혼잡 상황은 후속 플레이 피드백이 필요하다.

## 새 무브셋 기획의 범위

구현된 것은 기본 4타의 실제 전진 이동, 제한 보정, 적중 강타의 국소 히트스톱이다.

캐릭터 고유 순간이동/위치 교환, 적중·빗나감·중단별 위치 처리 정책, W/S 4타 커맨드 분기, 선택적 공중 추격, 방향별 대시/회피 무적, 다운 후 긴급 탈출, 체술·Q/E/R 술식 연계는 확정된 후속 설계 기준으로 구분한다. 이번 검증에서 구현하지 않았으며 기존 고죠 기술과 VFX도 수정하지 않았다.
