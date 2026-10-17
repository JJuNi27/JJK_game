# 시부야 PASS 2 — 기본 체술 4타

> 최신 상태 (2026-10-18): 사용자가 PASS 2~2.3 전체를 Play Mode에서 검수하고 현재 체술에 만족한다고 명시하여 **USER VERIFIED 체크포인트**로 승인했다. 아래는 구현 당시의 검증 기록이다. 최신 동작·기본값·승인 범위는 [통합 체크포인트](SHIBUYA_COMBAT_PASS2_23_CHECKPOINT.md)를 따른다.

상태: **CODEX VALIDATED / PENDING USER PLAY MODE REVIEW**

- Branch: `feat/shibuya-combat-mvp`
- 기준 HEAD: `0f416389c602ed601aa6447238a520f007550ef0`
- commit / push / staging 미실행. PASS 2 사용자 직접 검수는 아직 수행하지 않음.

## 동작과 데이터

좌클릭의 새로운 입력으로 1→2→3→4타를 연결한다. 후속 입력은 공격 종료 직전 버퍼 구간에 한 타만 예약하며, 준비·활성·회복 시간을 건너뛰지 않는다. 회복 완료 후 초기화 시간이 지나면 1타부터 시작한다. 4타 후에는 새 콤보로 돌아간다. 피해 수신·피격 경직·사망·전투 종료·비활성화 시 콤보와 버퍼를 취소한다.

설정 위치: `unity/Assets/Shibuya/Data/Shibuya Combat Profile.asset`.

| 타격 | 피해 | 준비 / 활성 / 회복(초) | 수평 넉백 속도 | 피격 경직(초) | 모션 / 연출 강도 |
|---|---:|---|---:|---:|---|
| 1 | 20 | 0.10 / 0.08 / 0.22 | 6 | 0.25 | 오른 직선타 / 1.0 |
| 2 | 20 | 0.09 / 0.07 / 0.18 | 4 | 0.25 | 왼 직선타 / 1.0 |
| 3 | 20 | 0.10 / 0.07 / 0.18 | 4 | 0.25 | 오른 회전타 / 1.1 |
| 4 | 25 | 0.18 / 0.10 / 0.32 | 11 | 0.45 | 왼 마무리 강타 / 1.5 |

- 1타의 PASS 1 피해·판정·타이밍·넉백·경직 값을 보존했다.
- 타격별 판정 반경 0.85, 전방 거리 1.1, 높이 1.0을 각각 독립 편집할 수 있다.
- 넉백 방향은 `공격자에게서 멀어지는 방향` / `공격자가 바라보는 전방` 중 선택한다. 기본 1~3타는 전자, 4타는 후자다.
- 입력 버퍼 0.16초, 콤보 초기화 대기 0.60초. 초기화 시간은 공격 회복이 끝난 뒤부터 계산한다.
- 단일 공격의 기존 serialized 필드는 제거하고 자산을 4타 데이터로 이전했다. 중복된 두 설정 중 하나를 선택하는 숨은 경로는 없다.
- 사용자 필드는 한국어 `SerializedProperty` Inspector / 타격 PropertyDrawer로 표시한다. 모션·방향 enum도 한국어다. 데이터 복제본에서 SerializedProperty 편집과 Undo를 검증했다.
- `CharacterStatsProfile`은 기존처럼 최대 체력만 적용한다. 공격·방어 배율은 **현재 피해에 적용하지 않는다**. 공용 능력치/Health 코드는 변경하지 않았다.
- 게임플레이 판정은 Fighter, 시각 모션은 BlockVisual, 간단한 사운드/충돌 강조는 HitFeedback이 담당한다. 이동·중력·넉백의 소유자는 계속 Fighter 하나다.

## 변경 파일

수정:

- `unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaPlayerInput.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlockVisual.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaHitFeedback.cs`
- `unity/Assets/Shibuya/Data/Shibuya Combat Profile.asset`
- `unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass1Tests.cs` — 단일 공격 데이터 참조만 새 1타 데이터로 변경. 기존 회귀 assertion은 유지.

추가:

- `unity/Assets/Shibuya/Editor/ShibuyaPass2Tests.cs` 및 `.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass2TestRunner.cs` 및 `.meta`
- 이 보고서

신규 씬·프리팹·재질은 없다. 기존 씬/프리팹의 Profile 연결을 그대로 사용한다.

## 자동 검증

최종 실제 discovery/execution **7 PASS / 0 FAIL / 0 SKIP**. 이 중 5개는 실제 Play Mode 프레임에서 실행하는 UnityTest이며 수동 플레이 판정은 아니다.

| 테스트 | 결과 |
|---|---|
| `ShibuyaPass1Tests.RealPlayLoopMovementHitVictoryDefeatAndRestart` | PASS |
| `ShibuyaPass1Tests.SceneAssetsAndKoreanEditorsAreConnected` | PASS |
| `ShibuyaPass2Tests.BufferedFourStrikesUseIndependentDamageTimingAndKnockback` | PASS |
| `ShibuyaPass2Tests.FourStrikeDataKoreanLabelsAndSerializedUndo` | PASS |
| `ShibuyaPass2Tests.HeldInputSlowClicksTimeoutAndEarlyBufferExpiry` | PASS |
| `ShibuyaPass2Tests.HitDeathAndMatchFinishCancelComboAndBuffer` | PASS |
| `ShibuyaPass2Tests.RealInputComboHitsWithoutRepositioningBetweenStrikes` | PASS |

- 4타 순서·각 타격 독립 설정·같은 프레임 연타 시 단계 건너뛰기 방지·버퍼 한 칸·초기화·누르기 유지·피격/사망/승리 취소를 검증했다.
- 실제 입력 경로에서 전진하며 4회 눌러, 타격 사이 플레이어/대상 위치 재설정 없이 4회 적중→승리를 확인했다.
- 기존 PASS 1의 이동·달리기·점프/착지·자유 카메라·벽 충돌·벽 너머 공격 차단·다중 Collider 중복 방지·승패·재도전·재도전 후 객체 수/AudioClip 검사를 유지했다.

결과: `unity/Logs/ShibuyaPass2QA/results.xml`, `execution.jsonl`, `discovery.json`.

검증 도중의 문제는 성공으로 계산하지 않았다. 첫 Pipeline 비동기 collector에서는 완료 결과를 받지 못했고, 백그라운드 실행 중 프레임 정지도 발생했다. 기존 TestRunnerApi의 callback을 재등록하는 작은 기록기로 XML을 저장하고, 테스트 중에만 런타임 runInBackground를 활성화했다. 신규 테스트의 EnterPlayMode 전에 생성된 closure가 소실되는 실패는 시나리오를 Play Mode 진입 후 생성하도록 고쳤다. 이 문제를 게임플레이 코드 변경으로 우회하지 않았다.

최종 컴파일 실패 0, Shibuya 참조 shader 오류 0. 최종 Editor Console은 오류 0 / 경고 2 (`Deleting invalid font reference.`). Pipeline 누적 버퍼에는 중간 main-thread timeout 오류 1개가 남아 있으므로, 실행 전 기간 오류 0으로 표현하지 않는다.

종료 시 Play Mode OFF, ShibuyaCombatMVP의 isDirty=false. QA 전용 Profile 잔존 0, temporary impact AudioClip 잔존 0. 기존 재도전 리소스 검사도 PASS. Gojo/VFX·기존 Scene/Prefab·Packages·ProjectSettings·USER VERIFIED 문서는 시작 시 SHA-256과 동일하다.

## 직접 확인 방법

1. `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 열고 Play.
2. WASD로 더미에게 접근하고, 필요하면 전진하며 좌클릭 연타. 오른 주먹→왼 주먹→회전타→마무리 강타를 확인한다.
3. 입력을 멈추면 콤보가 초기화되고, 클릭 유지로 자동 연타되지 않는지 확인한다.
4. 초록 HP 감소 및 HP 0 승리→재도전을 확인한다. ShibuyaMatch Inspector의 `플레이어 패배 검증`으로 패배→재도전도 확인할 수 있다.
5. Profile의 `지상 4타 콤보`를 펼쳐 각 타격의 한글 항목을 조절한다.

속도감·무게감·음량·모션 품질·Inspector 실제 마우스 조작/Prefab Override 표시는 사용자 직접 검수 대기다. 현재는 튜닝 가능한 초기 밸런스이며 완성 애니메이션은 아니다. 공중/업다운 분기·대시·가드 등은 이번 PASS에서 구현하지 않았다. 다음 PASS는 손맛 확인 후 지상 콤보 분기를 작은 범위로 검토하는 것을 제안한다.
