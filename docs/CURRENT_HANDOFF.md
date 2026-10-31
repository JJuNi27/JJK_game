# JJK_game — Current Project Handoff

## 최신 체크포인트 — PASS4G 사용자 승인 / MVP 수준 (2026-10-31)

사용자가 Unity Play Mode에서 **치명타·흑섬 발동, Zone 진입·스택·버프 적용, 패링 이후 반격과 흑섬 연계, 데미지 숫자 표시, 누락 수정 후 기본 콤보의 타격별 숫자 출력**을 직접 확인하고 MVP 수준으로 승인했다. 이 범위만 USER VERIFIED다. 흑섬 VFX·Zone 전용 연출·캐릭터별 반격 모션과 최종 전투 밸런스의 승인이 아니다.

공격 분류의 모든 예외·확률 통계 경계·다수 대상/극단적 다단·반격 취소/사망/벽·특수 방어/무하한 전체 회귀·장시간 자원/성능·화면 밖 숫자·다단 흑섬 문구 겹침은 자동 검증/미검증 한계를 그대로 유지한다. CODEX VALIDATED를 사용자 직접 승인으로 확대하지 않는다. 현재 캐릭터 흑섬 기본OFF/QA 명시 허용 정책도 그대로다.

이번 문서가 포함되는 체크포인트는 `feat/shibuya-combat-mvp`의 `ce6626ba6148c34e9ea4b98024a47556f9645854` 다음 커밋이다. 정확한 SHA는 이 문서를 포함한 Git 이력과 완료 보고에서 확인한다. 아래 LOCAL UNCOMMITTED·검수 대기·옛 HEAD는 각 구현 당시 기록이며 삭제하지 않는다. 이번 작업은 관련 파일의 선택적 commit/push와 문서 정리만 수행한다.

기존 근거를 재사용했다. **G0~G2 최신 고유58 PASS는5회 실행 이력(마지막 targeted27 PASS), G3 최신 고유33 PASS는6회 실행 이력(선택 회귀 단일14 PASS), 누락 수정 최신 고유13 PASS는6개 최신 PASS 출처 실행**이다. 서로 다른 범위·중복 케이스가 있으므로 합계104개의 단일 전체 회귀 성공으로 표현하지 않는다. 최초 FAIL/XML/trace·폰트 경고·Pipeline 타임아웃을 보존하고 PASS4A 과거 미해결18건을 해결 처리하지 않는다. 이번 Git 저장에서 Unity 실행/재컴파일/테스트를 추가하지 않는다. 상세 파일 목록·제외·정적 검증은 `docs/SHIBUYA_PASS4G_CHECKPOINT.md`.

다음 개발 후보는 **흑섬 VFX 및 Zone 시각 피드백**이며 별도 승인 후 시작한다. **특수 방어·술식 중화 공통 시스템**은 향후 논의할 초안이다. 무하한 상시 방어(차단 시 피해·피격 모션·경직 없음), 영역전연 자격 패시브/선택 태세 및 활성 중 생득술식 제한·주력 소모, 간이영역의 영역 필중 대응, 신카게류 발도의 조건부 반응, 천역모·흑승의 장비 기반 무효화/교란을 논의할 수 있다. 확정 적중과 특수 방어 관통은 별개이며 `대상 추적 → 적중 판정 → 특수 방어 → 실제 피해·피격·보상` 경계를 검토한다. 이는 확정 명세나 구현 완료 기록이 아니다. 이번 저장 후 추가 기능을 시작하지 않는다.

## 최신 로컬 — PASS4G.3 연속 피해 숫자 누락 수정 (2026-10-31)

**LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**. HEAD `ce6626ba6148c34e9ea4b98024a47556f9645854` 유지. 아래33 PASS는 수정 전 당시 이력이다. `ResolveHits`가 콤보 전체 sequenceToken을 등급/결과 중복 키로 전달해 정상 후속 타격의 commit/숫자가 생략됐다. 개별 `AcceptedAttackCount`로 호출 인자만 수정했다. 피해4회/결과1/생성1 →4/4/4, 공중 부분5/5/5. 같은 이벤트 재전송은 계속 차단한다. 같은 버그로 억제되던 정상 흑섬 보상 commit도 복구되지만 확률·배율·보상량·Zone 데이터는 조정하지 않았다.

최신 고유13 PASS/0 FAIL/0 SKIP는6회 실행 집계(신규9+기존 targeted4), 단일 전체 실행 아님. 초기 새 QA 입력/재시작/직렬화 실패를 보존했다. 공중 QA 클릭은 Editor continuation과 실제 Player Update를 구분해 입력 소비를 관측하도록 보완했다. QA 창의 Missing 참조와 독립적으로 재시작 후 새 표시가 작동함을 확인했다. 상세 `docs/SHIBUYA_PASS4G3_DELIVERY_REVIEW.md`, 원본 `unity/Logs/ShibuyaPass4G3DeliveryQA/20261031-missing-numbers/`, `latest-unique-results.json`.

기존 QA437개·사용자 데이터·아우라·Gojo/VFX·Scene/Prefab·Packages/ProjectSettings·잠금 문서·`.slnx` 보존. 최종 컴파일 정상/Editor Console0 error·5 warning(기존4F 테스트 경고3+폰트 경고2), 도구 세션에는 Pipeline timeout5건 이력이 있어 전체 세션 오류0은 아니다. Play stopped/씬 clean/fps -1/TimeScale1/AutoTick true16ms/PlayUnfocused/표시 observer0. 표시 기본32슬롯·0.85초·색상·문구·화면 밖 숨김 정책 불변. Git tracked dirty34/untracked50/staged0. 사용자 재검수 전 승격/commit/push하지 않는다.

## 최신 로컬 — PASS4G.3 데미지 표시 / 사용자 검수 대기 (2026-10-31)

**LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**. 브랜치 `feat/shibuya-combat-mvp`, HEAD `ce6626ba6148c34e9ea4b98024a47556f9645854` 유지. 아래4G.0~4G.2와4F 기록은 이전 시점이며 삭제하지 않는다.

실제 HP 차감 순간을 읽기 전용 out 값으로 캡처해 회복/중첩 피해/사망 callback과 분리했다. 기존 Health API·방어 판정·알림 순서는 유지한다. 시부야 표시 결과는 이미 확정된 일반/치명/흑섬과 실제 HP 감소를 전달하며 재추첨·추가 피해·보상을 만들지 않는다. 일반 흰색/치명 금색·CRITICAL!/흑섬 붉은 숫자+검은 외곽·흑섬!, 작은 Zone 보조 문구, 다단 개별 표기,32개 재사용 슬롯을 제공한다. 기존 카메라별 IMGUI stack과 호환하고 Camera/LockOn/HUD·Scene/Prefab을 교체하지 않았다.

Play에서 기본ON. 기존 `JJK Game/시부야 PASS4G/공격 등급·반격 검수` → 현재 플레이어 연결 → 숫자 ON/OFF. OFF는 현재 표시를 즉시 비우며 이후 피해·흑섬·Zone·주력·hit-stop·사운드/VFX는 계속 진행한다. QA 추첨 버튼은 이후 타격용 RNG 교체이며 다음 한 타 예약이 아니다. 능력 기본OFF·반격 확정 치명 같은 원래 규칙은 우회하지 않는다. 설정은 새 `Assets/Shibuya/Resources/Shibuya Damage Numbers.asset`의 한글 Inspector다. 정식 설정 저장/메뉴·고급 흑섬 VFX는 미구현이다.

**최신 고유33 PASS/0 FAIL/0 SKIP는6회 실행 집계**(신규19+선택 회귀14)다. 관련 회귀는 단일14 PASS, 최종12 HP/OFF 재촬영2 PASS. 초기 폰트 이름 변경에 따른 렌더 결함·반복 경고와 QA 입력/Destroy 관측 실패를 원본 로그/XML/PNG로 보존했다. 수정 후 실제 숫자·한글,12 HP 표시, ON/OFF 동일 피해·보상을 재확인했다. 기존4G.0~4G.2의58개를 이번33개에 더하지 않으며 PASS4A 과거18건도 해결 처리하지 않는다.

최종 컴파일 정상/Editor Console error0·font warning2. 초기 신규 font family 경고3340회는 수정 이후 추가0, 현재2개는 원래도 있던 `Deleting invalid font reference.` 유형이지만 모두 기존 것이라고 단정하지 않는다. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused, 표시 observer/component 잔류0. 기존 Data/Meta/Gojo/VFX/아우라/씬/프리팹/패키지/설정/잠금 문서/`.slnx`와 이전4G QA164개를 보존했다. Git tracked dirty34/untracked47/staged0, commit/push 없음.

보고서 `docs/SHIBUYA_PASS4G3_REVIEW.md`, 원본 `unity/Logs/ShibuyaPass4G3QA/20261031-damage-display/`. 최종 영상은 `disposal-and-spacing-check/legacy/ShibuyaPass4B3QA/4G3-damage-overlay-original-speed-padded.mp4`. 시각 크기·다단 밀도·마무리 결과 패널과의 순간 겹침은 사용자 검수 대기다. 추가 PASS를 시작하지 않는다.

## 최신 로컬 — PASS4G.0~4G.2 구현 / 사용자 검수 대기 (2026-10-31)

브랜치 `feat/shibuya-combat-mvp`, 작업 시작 로컬·원격 HEAD `ce6626ba6148c34e9ea4b98024a47556f9645854`. 아래4F 체크포인트는 이전 시점이다. 이번 변경은 LOCAL UNCOMMITTED이며 staging/commit/push하지 않는다. 기존 dirty `unity/unity.slnx`는 바이트 그대로 보존한다.

공격 분류(도달/피해 성격/출처/타격별 등급 허용), 시부야 주력200, 치명타25%·1.5배, 흑섬2.5배와 Applied-only 보상, Zone12초/대전 수명 스택, 패링 후 새 LMB 반격을 추가했다. 기존 캐릭터 흑섬 능력은 기본OFF다. 치명타가 흑섬으로 올라가며1.5×2.5로 중복 곱하지 않는다. 일반 최종 흑섬 상한15%/Zone20%를 조건부 추첨에 환산한다. 반격만 치명타100%·흑섬 조건부50%의 예외다.

명시 승인된 기존 수치 변경은 시부야 주력100→200과 패링 공격자 경직0.35→0.5초다. F 기본12/초·소비 배율1·패링 밀려남1.2m·성공자 자세0.1초·기본 피해20/20/20/25·HP1000/100을 보존한다. 캐릭터 아우라 기준 용량200으로 이전100과 동일 크기를 유지하며 공유 VFX Profile/Curve/Shader/Geometry를 바꾸지 않는다. 동일 시점 PNG·정점이 일치했다. Zone은 실제 비용과 F 회복 경로에 연결되며 신체 강화 패시브·주력 대쉬·각성·고급 흑섬 VFX는 구현하지 않았다.

완료 상태는 **CODEX VALIDATED(targeted) / USER VERIFIED 대기**다. 4G.0 최초10 PASS/1 FAIL(새 데이터 검사 배열 접근), 테스트 수정 후 해당1 PASS. 4G.1 최초 단일24 PASS, 4G.2 최초 단일12 PASS, 최종 targeted 단일27 PASS/0 FAIL/0 SKIP. **최신 고유58 PASS/0 FAIL/0 SKIP는5회 실행 집계**이며 단일58개 실행이 아니다. G1 최신 고유25, G2 최신 고유16이며 기존 회귀와 G0를 중복 합산하지 않는다. 실행별 XML·수정한 테스트 컴파일 CS0535 1건·최초 실패는 `unity/Logs/ShibuyaPass4GQA/20261031-combat-grades/`에 보존했다. 과거 PASS4A 미해결18건은 해결 처리하지 않는다.

최종 컴파일 정상, Editor Console error0/font warning1, Pipeline 세션에는 수정한 테스트 컴파일 error1이 남아 있다. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused, 새 QA 프로필·소유 Mesh/Material 잔류0. 기존 자산/Scene/Prefab/Packages/ProjectSettings/GUID·잠금 문서는 불변이다. 최종 검사/한계는 각 PASS 보고서를 우선한다. 원속도 반격 영상은 `final-targeted/legacy/ShibuyaPass4B3QA/4G2-parry-counter-original-speed-60fps.mp4`다.

검수: Play → `JJK Game/시부야 PASS4G/공격 등급·반격 검수`. QA 흑섬 허용/시드/강제 추첨은 런타임 사본만 수정한다. 패링 연습은 기존 `JJK Game/시부야 PASS4D/가드 피격 검수 도구`에서 시작한다. 패링 후0.5초 안의 새 LMB가 반격이며 입력하지 않으면 자동 공격하지 않는다. 벽·특수 무적·다른 시네마틱·사망·종료는 예약 반격의 명시적 취소 예외다. 캐릭터별 최종 모션/카메라/VFX 품질은 미승인이다.

보고서: `SHIBUYA_PASS4G0_REVIEW.md`, `SHIBUYA_PASS4G1_REVIEW.md`, `SHIBUYA_PASS4G2_REVIEW.md`. 표준 명칭은 `design/COMBAT_ACTION_VOCABULARY.md`. 새 기능을 USER VERIFIED로 승격하지 않는다. 기존 USER VERIFIED 문서는 과거 승인 기록을 유지한다.

## PASS4F.1 체크포인트 — USER VERIFIED / MVP 임시 수용 (2026-10-31)

브랜치 `feat/shibuya-combat-mvp`. 이전 공격형 AI 체크포인트는 `9568855d806c29ad66375ac470a6d69c3482205c`다. 이번 문서가 포함된 새 방어형 AI 체크포인트의 SHA는 현재 브랜치 Git 이력에서 확인한다. 이번 작업은 관련 파일의 선택적 commit/push와 문서 정리만 수행하며 게임플레이·AI·밸런스는 수정하지 않는다.

사용자는 직접 플레이한 뒤 움직임이 다소 어색하지만 AI는 추후 개선하기로 하고 **현재 MVP 수준을 임시 수용**했다. USER VERIFIED 범위는 **AI 모드 선택·기본 동작, 공격형/방어형 AI의 현재 기본 구조, 현재 구현을 보존하고 다음 작업으로 넘어가는 결정**이다. 자연스러운 이동, 공격·방어 전환의 완성도, 대전 난이도·재미, 패링 성공률/반응 정확도 최종 밸런스, 종합 모드 공격 적극성, 캐릭터별 성향, 장시간 안정성과 전체 경계 조건은 최종 승인하지 않았다. 자동 검증 세부 행동을 사용자 직접 승인으로 확대하지 않는다.

**검수 UX:** AI는 기본 OFF이며 모드 선택과 ON/OFF는 별도다. Play → `JJK Game/시부야 PASS4F/모드`에서 선택 → 기존 `공격형 훈련 AI ON·OFF`로 실제 활성화한다. 이름과 무관하게 선택한 모드로 동작한다. `훈련 AI 현재 상태·설정`에서 실제 ON/Running과 선택 모드를 구분해 확인한다. 초기 “AI가 움직이지 않는다” 보고를 모드 선택만으로 자동 활성화된 것으로 해석하지 않으며, 확인되지 않은 고장 원인을 단정하지 않는다. 상태 창을 닫으면 연결 AI OFF, 새 Play/재시작도 기본 OFF다.

기존 검증을 재사용했다. PASS4F.1 **최신 고유39 PASS/0 FAIL/0 SKIP는6회 실행 집계**(신규23+선택 회귀16)이며 단일 전체 실행 성공이 아니다. 최초 실패·버퍼 만료 진단·QA 입력 시점 수정·증거 미완료6건의 별도 재검증을 원본 XML/trace/영상과 함께 유지한다. 마지막 구현 검증 기록은 컴파일 정상/Editor Console error0·폰트warning1이다. 이는 이번 Git 작업에서 새로 실행한 Unity 검사 결과가 아니다.

PASS4F.0의 최신 고유34 PASS(7회 실행), 이전 도구/QA 오류1928건(CLI 명령 호환19+탐색 콜백 반복1909), PASS4F.1 QA 코루틴 CS0161 수정 이력1건을 구분해 보존한다. PASS4A 과거 미해결18건은 해결로 소급하지 않는다. 기존 QA565개를 포함한 로컬 원본과 실패 기록은 삭제·변경하지 않는다. `.slnx` 자동 순서 변경은 staging 없이 로컬에 보존하고 QA/미디어/캐시/로컬 모델·오디오·레퍼런스는 커밋에서 제외한다.

### 후속 AI 개선 목록 — 이번에는 구현하지 않음

1. 접근·거리 조절의 자연스러움
2. 공격과 방어 사이 행동 전환
3. 종합 모드의 과도한 수동성
4. 패링·가드·회피 반응 타이밍
5. 반복적이고 기계적인 행동 패턴
6. 캐릭터별 전투 성향과 난이도
7. 장애물 회피 및 고급 이동
8. 공격 예고 가독성과 실제 전투 밸런스

위 목록은 후속 과제이며 신규 구현 승인이 아니다. 기존 공격형 AI와 고죠/VFX·승인 아우라·카메라·전투 Data Asset·사용자 조절값을 보존한다. 저장 후 추가 기능은 시작하지 않는다. 상세 구현·실행별 결과는 `docs/SHIBUYA_PASS4F1_REVIEW.md`를 따른다. 아래 LOCAL UNCOMMITTED/USER VERIFIED 대기 표기는 이전 구현 당시 기록이다.

## 최신 로컬 — PASS4F.1 방어형 훈련 AI / 사용자 검수 대기

2026-10-30. `feat/shibuya-combat-mvp`, 로컬·원격 체크포인트 HEAD `9568855d806c29ad66375ac470a6d69c3482205c`. PASS4F.0은 사용자 승인 범위를 문서화해20개 파일로 일반 commit/push했고 실제 원격 HEAD 일치를 확인했다. 현재 PASS4F.1은 **LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**이며 staging·commit·push하지 않았다.

공격형/가드 연습/회피 연습/패링 연습/종합 모드를 제공한다. 현재 실제 공격 관측→0.16~0.30초 반응→가능 행동의 가중치 선택→기존 SetGuardHeld/TryGroundDash 실행이다. 미래 입력/예약이나 플레이어 마우스·키를 읽지 않는다. 진행 중 정상 콤보와 기존 상태 소유권이 우선한다. 공격형의 기존 경로·수치는 보존했다. 새 방어 설정은 별도 한국어 Defense Profile이며 기존 AI Profile이 참조한다.

최신 고유 **39 PASS/0 FAIL/0 SKIP(신규23+선택 회귀16), 6회 실행 집계**다. 단일39개 전체 실행 성공이 아니다. QA 공격자의 첫 예약 경계에서 발생한 후속 입력 만료를 계측하고 QA 입력만 허용 창 중간으로 옮겼다. 원래 게임플레이 값·기대값은 유지했다. 실패 뒤 PASS였으나 증거가 없던6건은 본문 완료 기록을 붙여 별도6 PASS를 확인했으며 원본 XML을 보존한다. PASS4A 과거 미해결18건은 해결로 변경하지 않는다.

현재 컴파일 정상/Editor Console error0·warning1(font). 이번 Pipeline 세션 error1은 수정한 테스트 코루틴 CS0161이며, 이전4F.0 도구/QA1928건(CLI19+탐색1909)과 구분한다. 재컴파일 중 Pipeline 연결 실패1회는 검사가 미시작임을 확인하고 연결 복구 뒤1회 시작했다. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused, 런타임 AI0 및 Shibuya Mesh·Material0/0이다.

메뉴: Play → `JJK Game/시부야 PASS4F/모드`에서 모드 선택 → 기존 `공격형 훈련 AI ON·OFF`로 ON. 모드 선택만으로 자동 ON하지 않는다. `훈련 AI 현재 상태·설정`에서 모드·마지막 선택·실제 결과·카운터를 확인한다. 가드 체력은 HUD에 표시하지 않는다. 새 Play/재시작은 기존처럼 AI OFF다.

짧은 기본 공격에는 반응이 늦어 맞을 수 있고 패링 모드도 성공을 보장하지 않는다. 종합 모드는 계속 압박받으면 수동적이며, 영상에서도1패링/5반응 실패를 보였다. 무입력 구간에는 기존 공격이 가능함을 별도 검사했다. 자동 난이도 튜닝이나 강제 반격을 추가하지 않는다. 최종 손맛·난이도는 사용자 직접 검수 대상이다.

보고서 `docs/SHIBUYA_PASS4F1_REVIEW.md`, 원본 `unity/Logs/ShibuyaPass4F1QA/20261030-defense-ai/`, 원속도7개 `media/`. 기존4F.0 QA565개·.slnx 바이트·Gojo/VFX·아우라·카메라·전투값·Scene/Prefab/Packages/ProjectSettings/기존 GUID를 보존했다. 아래는 이전 승인/개발 시점의 기록이다.

## PASS4F.0 사용자 승인 체크포인트 — 2026-10-30

브랜치 `feat/shibuya-combat-mvp`, 이전 기준 `0cbaed7ea07a254a252e09e36d3072a7913da678`. 이번 문서를 포함하는 체크포인트 SHA는 Git 이력에서 확인한다.

사용자가 공격형 AI의 **플레이어 접근, 방향 정렬, 기존 Fighter의 1~4타 공격, 공격 후 휴식·재접근, 기본 조작감**을 직접 플레이하고 승인했다. 이 범위는 USER VERIFIED다. 장애물 우회, 장시간 안정성, 전체 경계 조건, 최종 난이도는 승인 범위가 아니다.

기본 OFF, 10m 탐색, 2.3→1.55m 접근, 20도 정렬, 반응0.20초, 묶음1~4타 가중치25/35/25/15, 휴식0.9~1.5초다. 기존 Fighter가 이동·회전·충돌·피해를 소유한다. OFF는 AI 소유 후속 예약만 철회하고 현재 타격·피격은 강제로 취소하지 않는다. 새 Play/재시작은 수동 더미다.

자동 검증은 **최신 고유34 PASS/0 FAIL/0 SKIP(신규18+선택 회귀16), 7회 실행 집계**다. 단일 전체34개 실행이 아니다. 최초 버퍼 만료 경계 결함과 재검증, 촬영 중 실제 사망에 따른 준비 실패, QA 탐색 오류를 원본 XML/trace/영상/로그와 함께 보존했다. PASS4A 과거 미해결18건은 그대로다.

이전 Pipeline 누적 오류1928건은 CLI 명령 호환19건(`script recompile`2, `script recompile_status`17)과 테스트 discovery 콜백 예외 반복1909건이다. 게임플레이/컴파일 오류가 아니며, 미시작 탐색은 성공 검사 수에 넣지 않았다. 마지막 Editor Console error0/warning1(font), Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused/런타임 Shibuya Mesh·Material0/0 기록을 재사용했다. 미확인 장시간 안정성을 해결 완료로 확대하지 않는다.

보고서 `docs/SHIBUYA_PASS4F0_REVIEW.md`, 로컬 원본 `unity/Logs/ShibuyaPass4F0QA/20261030-training-ai/`. Play 메뉴는 `JJK Game/시부야 PASS4F/공격형 훈련 AI ON·OFF`, 상태 창은 같은 경로의 `훈련 AI 현재 상태·설정`이다. 상태 창을 닫으면 연결 AI OFF다.

기존 Gojo/VFX·아우라·카메라·전투값·Scene/Prefab/Packages/ProjectSettings와 사용자 `.slnx` 변경을 보존한다. `.slnx`, QA 원본, 미디어와 로컬 자산은 이번 커밋에서 제외한다. PASS4F.1 방어형 AI는 이 체크포인트 push 확인 후 별도 LOCAL UNCOMMITTED로 구현하며 사용자 승인은 아직 없다. 아래는 이전 시점의 기록이다.

## 현재 — PASS4D.2 + PASS4E USER VERIFIED 체크포인트 / 훈련 AI 설계 합의 대기

2026-10-30. 브랜치 `feat/shibuya-combat-mvp`. 이전 원격 기준은 `bd5acdcb22c35002317a438a2b0fd28986b53607`이며, 이 문서가 포함된 새 저장 커밋의 SHA는 현재 브랜치 Git 이력에서 확인한다. 이번 요청은 관련 파일의 선택적 commit/push와 문서 정리만 수행한다.

사용자 직접 승인: **잡기 입력·조작감/성공·실패 기본 동작/기존 전투 연계**, **다운 후 방향 기상 기본 기능·방향 입력·조작**. 잡기15 HP/1.4m는 공통 MVP 초기값이지 모든 캐릭터의 고정 규칙이 아니다. 캐릭터별 잡기 성공 연출, 후속 타격 횟수·피해, 밀쳐내기/던지기, 시네마틱 여부, 짧은 즉시 실행형은 후속 설계 대상이다. 잡기 최종 모션·카메라와 기상 최종 애니메이션/VFX, 자동 검사만 한 극단적 경계 조건은 USER VERIFIED로 확대하지 않는다. 정확한 승인 범위는 locked/USER_VERIFIED_SETTINGS의 이번 승인 절을 따른다.

PASS4D.2 최신 고유41 PASS/0 FAIL/0 SKIP는4회 실행, PASS4E 최신 고유37 PASS/0 FAIL/0 SKIP는7회 실행의 케이스별 집계다. 기존 XML을 확인하고 재사용했으며 이번 저장을 위해 Unity를 실행하거나 테스트를 반복하지 않았다. 최초 실패·컴파일 수정·플랫폼 중단 보완·API 응답 시간 초과를 보존한다. 초기 쿨타임0 관측의 근본 원인은 미확정이며 이후 계측/재검증 통과와 구분한다. PASS4A 과거 미해결18건은 그대로다.

기존 PASS4C.2/4D.1 승인, 임시 MVP 가드 원통, 가드 체력 HUD 비공개, F 아우라·카메라·고죠 자산과 잠금값을 유지한다. QA 원본/영상/이미지/XML/trace, 로컬 모델·오디오·레퍼런스, Library/Temp/Cache는 로컬 보존이며 이번 커밋 대상이 아니다. `unity/unity.slnx`는 기존2개 프로젝트 순서만 바뀐 자동 생성 변경으로, 기능에 필요하지 않아 staging하지 않고 그대로 보존한다.

**다음 후보는 훈련 AI 설계**다. 사용자와 행동 규칙을 합의한 뒤 별도 PASS로 진행한다. 이번 체크포인트에서 훈련 AI나 신규 전투 기능을 구현하지 않는다. 아래는 이전 구현·검증 당시의 기록이며 LOCAL UNCOMMITTED/검수 대기/Console 상태를 현재 체크포인트 승인과 혼동하지 않는다.

## 최신 로컬 — PASS4E 방향 긴급 탈출 / 사용자 검수 대기

2026-10-30. `feat/shibuya-combat-mvp`, HEAD `bd5acdcb22c35002317a438a2b0fd28986b53607` 유지. PASS4D.2와 새 PASS4E는 LOCAL UNCOMMITTED이며 staging·commit·push 없음. 사용자는 이번 요청에서 **잡기 구조·조작감 정상**을 확인했다. 잡기 캐릭터별 연출·피해 구성·밀려남·모션의 최종 승인은 포함하지 않는다. 아래4D.2 검수 대기 표시는 이전 작성 시점이다.

PASS4E는 CODEX VALIDATED(targeted) / USER VERIFIED 대기. 실제 다운 착지 후 기존 minimumDownTime0.45초부터 자연 기상 전 새 WASD+Space로 카메라 기준 방향 탈출을 한다. 기존 자연 기상0.85초·기상0.22초는 유지했다. 새 Quick Recovery Data는2.5m/0.30초, 지면에서0.04~0.18초 회피, 종료·중단 후 쿨타임1.0초다. 조기/홀드/단독 Space는 저장하지 않으며 점프·공중 추격에 중복 전달하지 않는다. 지상에서 시작한 유한 수평 동작은 가장자리에서도 기존 중력과 함께 마치며, 공중 시작/체공/지면 스냅은 없다. 무적 밖의 피격·잡기·시네마틱·사망·포커스 상실 등은 취소한다.

최신 고유37 PASS(신규20+선택 회귀17)는 7회 실행의 케이스별 집계다. 단일 전체 성공 실행이 아니다. 초기 입력 Up 관측/쿨타임 관측 실패와 플랫폼 중단 문제·검증의 중간 높이 가정을 구분해 원본 XML과 계측을 남겼다. 플랫폼 중력 인계를 보완했고, 초기 쿨타임0 관측의 정확한 원인은 미확정으로 유지한다. PASS4A 과거18건은 별도 미해결 이력이다.

보고서 `docs/SHIBUYA_PASS4E_REVIEW.md`, 원본 `unity/Logs/ShibuyaPass4EQA/20261030-quick-recovery/`, 원속도3비교 `media/`. 최종 Editor Console error0/warning0, Pipeline 누적 error4(과거 컴파일2+API 시작 응답 timeout2)는 별도다. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused, runtime Mesh/Material0/0. 기존 Grab QA55개 해시, 승인 다운/대시/카메라/아우라·씬·GUID·고죠 자산 보존. Unity가 unity.slnx 두 Project 줄 순서만 자동 변경한 것은 별도 보존했다.

직접 검수: Play → `JJK Game/시부야 PASS4E/긴급 탈출 검수 도구` → 실제 다운 공격 예약 → Game View에서 직접 WASD+새 Space. 조기 홀드/무입력 자연 기상, 락온/고각, 벽·체술·잡기를 비교한다. 이후 추가 기능이나 Git 저장을 자동 진행하지 않는다. 아래는 이전 기록이다.

## 최신 로컬 — PASS4D.2 공통 잡기 MVP / 사용자 검수 대기

2026-10-30. 실제 HEAD `bd5acdcb22c35002317a438a2b0fd28986b53607`, `feat/shibuya-combat-mvp`. PASS4C~4D.1 체크포인트는 일반 push 완료 상태이며 아래1202e940 표기는 이전 기준이다. 이번 잡기는 LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기이고 staging·commit·push하지 않았다.

실제 가드 중 새 LMB로 잡기, 동시 RMB+LMB 잡기 우선, 패링 성공 후 RMB 유지 LMB는 기존 평타 반격이다. 전방1.55m/반각50도, 준비0.18+유효0.08초, 짧은 접촉0.10초 후 피해15/밀려남1.4m/경직0.40초, 성공 회복0.25·실패 회복0.50초다. `Shibuya Grab.asset`과 한국어 Inspector로 조절한다. 가드·패링만 제한적으로 우회하고 대시 무적·Infinity·Health 판정을 유지한다. 강제 위치 스냅/별도 이동 모터/시네마틱/주력 소모 없음. 기존 승인 카메라·대시·패링·아우라 설정·피해·HP·GUID·씬은 보존했다.

최신 고유41 PASS(신규23+선택 회귀18)는 4회 실행 집계이지 단일 전체 성공 실행이 아니다. 원본 실행13/0, 7/1, 18/0, 5/0을 보존했다. 실패1은 QA Vector3 JSON 순환 기록으로 좌표 배열로 수정 후 통과했다. 테스트 컴파일 오류2건도 수정·원본 보존. 최종 Editor Console error0/warning1(font), Pipeline 버퍼의 과거 error2는 별도다. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused 유지, runtime Mesh/Material0/0. 기존 PASS4A18건을 해결로 소급하지 않는다.

보고서 `docs/SHIBUYA_PASS4D2_REVIEW.md`, 원본 `unity/Logs/ShibuyaPass4D2QA/20261030-grab/`, 원속도3비교 `media-final/`. 직접 확인: Play → `JJK Game/시부야 PASS4D/잡기 검수 도구` → 상황 선택 → Game View 복귀 → 직접 RMB 유지+새 LMB. 최종 손 맞춤/캐릭터 모션/VFX·조작감은 사용자 검수 대기다. 반전술식·새 후속 기능은 시작하지 않는다. 아래는 이전 체크포인트/구현 당시 기록이다.

작성 기준: **2026-10-30 KST**
저장소: `JJuNi27/JJK_game` · 브랜치: `feat/shibuya-combat-mvp`
USER VERIFIED 체크포인트: `1202e940176daec9fbb0b8027702f600b2f677d4` (PASS 4B.4a까지, 10-25 일반 push 완료).
위 커밋은 이번 PASS4C~4D.1 체크포인트의 이전 기준이다. 이 문서가 포함된 새 체크포인트의 SHA는 현재 브랜치 Git 이력에서 확인한다. QA 원본/미디어와 로컬 모델·오디오·레퍼런스는 별도 로컬 보존이며 GitHub에 포함된 것으로 해석하지 않는다.

## 현재 — PASS4C~4D.1 체크포인트 / 후속 요청 대기

2026-10-30 사용자가 체크포인트 저장을 승인했다. PASS4C 지상 대시,4C.1 거리·쿨타임 튜닝,4C.2 카메라 기준 방향 통일,4D.0 가드·비공개 체력·파괴·임시 방어막,4D.1 패링·취소/반동·연습 도구와 필요한 코드/데이터/meta/테스트/보고서를 함께 보존한다.

USER VERIFIED는 **4C.2 방향 대시·조작감**, **4D.1 패링 구조·조작감**이다. 가드 원통은 임시 MVP 수용이며 최종 방어 VFX 승인이 아니다. 패링 최종 VFX와 캐릭터별 모션도 후속 작업이다. 가드 전체·자동 경계 검사까지 사용자 수동 검증으로 확대하지 않는다. 정확한 승인 범위는 `docs/locked/USER_VERIFIED_SETTINGS.md`의 이번 승인 절을 따른다.

기존 검증 결과를 재사용했으며 체크포인트를 위해 Unity를 실행하거나 전체 회귀를 다시 돌리지 않았다. 최신4D.1 고유30 PASS는 복수 실행 집계이고 단일 전체 성공 실행이 아니다. 최초 실패 XML/원인과 PASS4A 과거 미해결18건을 유지한다. 대용량 QA/Library/Temp/로컬 전용 자산은 커밋 대상에서 제외한다.

저장 후 추가 구현은 하지 않는다. **잡기는 다음 PASS에서 별도 승인 후 진행**한다. 패링/가드 최종 연출, 반전술식, 신체 강화 등도 자동 착수하지 않는다.

아래는 각 PASS 구현·자동 검증 당시의 기록이다. 당시의 LOCAL UNCOMMITTED/검수 대기/Console 상태를 최신 승인과 혼동하지 않는다.

## 0. 현재 — PASS 4D.1 타이밍 패링 MVP 검수 대기

우클릭 새 가드 진입 시0.12초 패링 창, 시작 사이0.50초 제한, 몸 정면120도. 실패/쿨타임은 일반 가드 유지. 성공은 가드를 종료하고0.10초 동작 후 수동 후속 행동 선택(자동 반격/홀드 재가드 없음), 공격자에게 가짜 HP 피해 없이0.35초 경직/1.2m 거리 넉백/현재·예약 공격 취소. Core Health 수정 없이 Guarded+전용 ParrySucceeded 이벤트로 구분한다. 가드/패링 가능 플래그는 독립이다. 기존 방어막/F 아우라/카메라/승인 대시 수치 보존.

LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기. 최신 고유30 PASS는 복수 실행 집계이며 단일30개 실행이 아니다. 최초 순변위 측정 실패는 준비 전진0.52m를 분리한 실제 반동1.20000017m로 확인했으며 원본을 보존했다. 보고서 `docs/SHIBUYA_PASS4D1_REVIEW.md`, QA/원속도3비교 `unity/Logs/ShibuyaPass4D1QA/20261030-parry/`. 기존 PASS4D 검수 메뉴의 반복 패링 연습/실제 공격 예고를 사용한다. 현재 Console error0/warning1, Play stopped/씬 clean/fps -1/Time.timeScale1/패링 리소스0. commit/push 없음.

PASS4C.2는 사용자 직접 승인. PASS4D.0 전체는 CODEX VALIDATED, 원통 외형만 임시 MVP 수용이며 새 패링의 승인으로 확대하지 않는다. 잡기/반전술식/주력 회복 보너스/신체 강화는 미구현이다. 아래는 이전4D.0 기록이다.

PASS4C.2 카메라 기준 대시 방향·속도는 사용자 직접 승인(LOCAL UNCOMMITTED)이다. 새 PASS4D.0은 우클릭 홀드/전방120도/독립 가드 체력100/회복1.2초 후20/s/파괴0.8초/타격별 가드 피해16·16·18·35와 별도 위험 방어막을 구현했다. 게임 HUD에는 가드 잔량을 표시하지 않는다. 기존 Health 방어 확장 재사용, 이동은 Fighter 단독 소유. F 아우라/카메라/대시 수치 보존. CODEX VALIDATED(targeted) / USER VERIFIED 대기. 최신 고유33 PASS는 복수 실행 집계이며 단일33개 성공 실행이 아니다. 원본 실패/테스트 관측 오류/수정한 프로필 생명주기 예외를 모두 보존했다.

상세·실제 피격 검수 메뉴·영상: `docs/SHIBUYA_PASS4D_REVIEW.md`. QA `unity/Logs/ShibuyaPass4DQA/20261030-guard/`. 메뉴 `JJK Game/시부야 PASS4D/가드 피격 검수 도구`는 Play Mode에서만2초 지연 실제 더미 공격을 제공한다. 현재 Console error0/warning1, 보존 버퍼 error6의 원인은 보고서에 구분했다. Play stopped/씬 clean/fps -1/Time.timeScale1. 패링/잡기/반전술식/신체 강화는 미구현. commit/push 없음.

아래는 PASS4C.2 구현 당시 기록이며 사용자 승인은 위 최신 기록을 따른다.

최신 PASS4C.2: 락온·비락온 모두 시작 시 카메라의 수평 WASD 방향을 저장해 대시한다. 타깃 위치는 이동 벡터에 사용하지 않으며 기존 몸 바라보기는 별도 유지한다. PASS4C.1 거리/지속시간/쿨타임/무적/Curve, 락온 선택·공격·카메라 코드는 보존했다. 단일 targeted20 PASS/0 FAIL/0 SKIP. 보고서·원속도5영상: `docs/SHIBUYA_PASS4C2_REVIEW.md`. LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기. 사용자는4C.1 거리·속도·쿨타임/비락온 방향에 만족한다고 피드백했지만 새4C.2 방향은 직접 검수 전이다. 최종 Play stopped/씬 clean/fps -1, 현재 Console error0/warning1(font), 이전 timeout은 버퍼 보존.

아래 PASS4C.1/4C 수치·검증·Console은 당시의 이력이다.

최신 PASS4C.1: 전방5.25m, 좌우3.60m, 후방3.30m, 종료/중단 후 쿨타임0.45s. 지속시간0.22/0.18/0.20s와 무적0.04~0.14s, 속도Curve·방향·락온·카메라·전투 런타임은 보존했다. 실제 Data Asset 네 값만 변경. 단일16 PASS/0 FAIL/0 SKIP와 별도 원거리 촬영1 PASS. 상세/원속도 자료: `docs/SHIBUYA_PASS4C1_REVIEW.md`. CODEX VALIDATED(targeted) / USER VERIFIED 대기 / LOCAL UNCOMMITTED. 아래4C 수치와20케이스·Console 기록은 이전 실행 이력이다.

CODEX VALIDATED(targeted 범위) / USER VERIFIED 대기. C KeyDown 방향 스텝, 기존 Fighter 단일 motor, 시작 방향 스냅샷, 짧은 일반 체술 회피 guard, 한국어 공통 Data Asset을 구현했다. 전방3.5m/0.22s, 측면2.4m/0.18s, 후방2.2m/0.20s, 측면·후방0.04~0.14s 회피, 종료 후0.65s 공통 쿨타임은 **실험값**이다. 전방 성분 대각선은 무적 없다.

공격/경직/다운/공중/추격/히트스톱/시네마틱에서는 시작하지 않는다. F 홀드 중 C를 차단하고, 진행 중 새 F는 대시를 종료해 기존 충전을 우선한다. 주력 소모/공격 캔슬/공중 대시/신체 강화는 없다. 기존 승인된 아우라·크기 곡선·충전/HUD·카메라/전투값을 보존했다.

최신 고유20케이스 PASS(복수 실행, 미디어 포함). 최초 시네마틱 API 반환형 검사 오류1건을 수정했고 원본 XML을 보존했다. 과거 PASS4A 회귀18건 전체 해결을 선언하지 않는다. 상세·원속도 영상·파일 목록: `docs/SHIBUYA_PASS4C_REVIEW.md`. 코드/설정 commit·push는 이번 작업에서 실행하지 않았다. 아래 USER VERIFIED 기록은 기존 승인 기능이며 새로운 C 기능까지 승인한 것이 아니다.

최종 Console은 error1/warning1. 오류는 검증 종료 후 Pipeline 복합 상태 조회의5초 timeout이며 작은 조회로 연결/Play 종료·씬 clean·프레임률 -1 복구를 확인했다. 테스트 중 게임 예외와 구분하고 로그를 지우지 않았다. 아래4B.4a의 Console 기록은 당시의 별도 시점이다.

## 1. 게임 방향과 보호

Unity 6 URP 기반 3D 자유 이동 격투게임. 현재는 ShibuyaCombatMVP의 블록 플레이어/더미로 1:1 공통 전투를 검증한다. 장기 목표는 넓은 도시, 캐릭터 선택, 독립 무브셋·술식·영역전개, 자유 난전·다인전·멀티플레이다.

Gojo는 첫 캐릭터이며 전용 VFX·모델·애니메이션·승인값은 동결 보존한다. 과거 Gojo 백업 기준은 `backup/gojo-vfx-wip-20261017`의 `540272a`; 모든 로컬 QA가 백업에 포함됐다고 가정하지 않는다. 과거 Purple 인수인계는 `6655f19` 시점 문서 및 Purple 보고서를 참조한다.

실제 로컬 작업 상태 → 최신 handoff/current task → locked settings → 관련 설계/보고서 순으로 확인한다. 사용자-facing Inspector는 한국어, 코드는 HOW/Data Asset은 WHAT. D:\JJK_game 하나를 사용하며 사용자 승인 없이 새 worktree·reset/clean/덮어쓰기/commit/push를 하지 않는다.

## 2. 체크포인트 구현 범위

- 기존 PASS 1~3C: 자유 이동/점프/카메라, 기본4타, W 업/S 다운, 제한 추적·정렬·방향 피격, 국소 히트스톱, 데이터 기반 무브셋, Space 공중 추격, 공중 LMB 2+2+1과 유한 체공.
- PASS 3D/공중 접촉: 유효 마무리 적중 후 참가자2명 세션·외부 간섭 차단·참가자 카메라, 무피해 추격 도착·자연 낙하, 새 피격의 이동 소유권.
- PASS 3C.1: 공중 후속 입력 유예 동안 피격자 높이 유지 관계. 거리·시간·피격 소유권 경계를 유지하며 전체18건 회귀를 해결했다고 선언하지 않는다.
- PASS 4A/4A.1/4A.2/4A.2.1: MMB 선택/해제, 신규50m/유지60m·시야 상실1.5초 유예, 락온 공격 방향, 3D 조준 제공, 수동 카메라 우선·시네마틱 소유권·최고 pitch 화면 밖 복구.
- PASS 4B~4B.3c: 공통 전신 혼령 아우라, 청록/핑크/초록, 먹물/Interior/분리 화염, 국소 형태 보존과 상하 연결, 최대 주력량 기반 크기.
- PASS 4B.4: 기존 CursedEnergyProfile/Controller 재사용, F 홀드 충전·현재 주력·소비/회복 API·변경 이벤트, 본인 파란 HUD, 최대치에서도 홀드 시간, 정상 해제/중단 이벤트 구분.
- PASS 4B.4a: 승인 후보를 저장 기본·Pink·Green에 직렬화. **QA 메뉴 없이 일반 F에서 최신 표현**.

## 3. USER VERIFIED 범위

기존 PASS 1~3C의 직접 승인 범위를 유지한다. 2026-10-22 직접 승인: W 어퍼컷→Space 무피해 접촉·자연 낙하, 추격 후 공중3클릭, 시네마틱 발동·카메라 복귀. 캐릭터 세부 모션과 시네마틱 구도·연출 품질은 승인에서 제외한다.

PASS 4A의 기본 선택/거리/시야 해제, PASS 4A.1 공격 방향 전환의 속도·자연스러움은 사용자가 직접 확인한 범위다. 자동 검사만 통과한 카메라 경계·모든 해상도·다인전 품질을 승인으로 확대하지 않는다.

**2026-10-25 현재 요청에서 사용자가 확인·승인한 항목:**
- 전신 혼령 주력 아우라의 시각적 방향.
- 별도 QA 메뉴 없이 일반 F 홀드에서 최신 아우라가 출력됨.
- 실제 F 홀드 주력 충전 동작.
- 현재까지의 관련 시부야 구현을 하나의 Git 체크포인트로 저장.

극단적 충돌·무적·입력·저프레임·리소스 경계, 픽셀 비교 등 자동 검증 세부는 CODEX VALIDATED로 구분한다.

## 4. 저장 설정과 미구현 사항

- 기본 지상 피해20/20/20/25, Dummy HP1000, Player HP100 유지.
- 주력 최대치 단일 원천: Shibuya Combat Profile → Shibuya Cursed Energy.MaxEnergy. 현재 저장 최대100, 시작 비율1, 홀드 충전 가능, 초당12. 기존 Gojo의 자동 회복은 별도 기본 모드로 보존.
- 아우라 저장 곡선 **100→1.0 / 150→1.5 / 200→2.0**, 최소0.5/최대3.0. 현재 잔량은 크기에 영향 없음.
- Details OFF, 국소 형태 보존 ON, 추가 확장0.65, 먹물 경계1.30/내부 비중0.80, 분리 화염 ON, 상하 연결1/1. Interior0.65/밀도1, 분리 교체속도0.7 유지.
- **2.7/3.0/3.5/4.0은 비교용 실험이며 최종 기본 배율 미선택.** 기존4.0 임시 미리보기/Inspector 조절 경로 유지.
- 시네마틱 예제는 저장 기본 OFF.
- **신체 강화 패시브는 사용자 결정으로 보류.** 준비 상태·임팩트 프레임·F 해제 버프·강화 아우라 지속·공격/이속 강화·W+F 대시는 구현하지 않음. 홀드/해제 API만 존재.

## 5. 검증 근거와 남은 이력

| 범위 | 기록 |
|---|---|
| PASS3D 기존 회귀135 + 시네마틱18 | 단일153 PASS / 0 FAIL / 0 SKIP, `unity/Logs/ShibuyaPass3DAirContactQA/20261022/final-regression-153/results.xml` |
| 공중 접촉·이동 신규 | 별도 복수 실행의 최신 고유18 PASS |
| PASS4A | 최초190개171 PASS/19 FAIL, 후속 조사 후 기존 회귀18건 미해결 기록 유지 |
| PASS4B.4 | 별도 실행9 PASS + 6 PASS, 관련2건 재확인. 최신 고유15 PASS |
| PASS4B.4a | 최초5 PASS/2 FAIL, 후속0 PASS/3 FAIL, 검사 구성 보완 후3 PASS. 현행 최신 고유8 PASS, 폐기된 복합 재진입 timeout도 원본 보존 |
| 일반F/승인미리보기 | 같은 설정/위상/카메라의 정지30쌍 RGB 차이0, 전체 움직임 픽셀 보증 아님 |

마지막4B.4a compile 성공, Console 현재 error0/warning1(기존 font reference). 과거 오류·실패 XML은 보존했다. 이번 체크포인트는 파일/참조/이전 증거 검토이며 전체 회귀를 다시 실행하지 않는다.

체크포인트 staged `git diff --check`는 기존 Unity 생성 빈 YAML/meta 필드와 테스트 마지막 빈 줄 때문에 형식 경고60건(24파일)을 냈다. 의미상 문제/충돌 표식은 없고, 원본 자산 바이트 보존을 위해 공백을 자동 정리하지 않았다. 해당2종 공백 규칙만 명령 단위로 제외한 추가 검사는 통과했으며 Git 설정은 변경하지 않았다.

**과거 PASS4A의18건은 해결됐다고 취급하지 않는다.** 후속 targeted PASS로 전체 원인을 소급 확정하지 않는다. 과거 공중 사거리 실패의 PASS3D 유발 여부도 INCONCLUSIVE 유지. 상세: `SHIBUYA_PASS4A_REVIEW.md`, 각 후속 PASS 보고서.

검증 보고서는 Git에 포함하지만 Logs의 원본 XML/trace/이미지/영상은 로컬 전용이다. Library/Temp/UserSettings/LocalModels/LocalAudio/레퍼런스도 제외·보존한다. 아우라 레거시 설정은 `docs/archive/CURSED_ENERGY_AURA_PRE_4B4A.json`에 별도 보존한다.

## 6. 재현과 후속 작업

씬: `unity/Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`.
- 일반 F: 메뉴 없이 홀드→최신 아우라/충전/본인 HUD, 해제→중단. 기본 시작은 최대치이므로 런타임 ShibuyaEnergyCharge 메뉴의 현재 주력 비우기로 회복을 확인할 수 있다.
- W+4타→새 Space→도착 후 새 LMB3회. 클릭이 없으면 접촉 후 자연 낙하.
- 시네마틱: `JJK Game → 시부야 PASS3D → 시네마틱 예제 ON (Play Mode 전용)`; OFF 또는 Play 종료로 해제.
- 아우라4배 비교: `JJK Game → 시부야 PASS4B3c`의 임시 배율/200기준 곡선 메뉴. 사용 후 저장 프로필 복귀.

현재 지상 방향 대시·회피 PASS4C는 로컬 구현 후 사용자 검수 대기다. 신체 강화는 계속 보류한다. 가드/패링/잡기 등 다음 기능, 캐릭터 모션/시네마틱 구도, 과거18건 QA 원인, 다인전·성능·복잡한 맵 경계는 별도 사용자 지시를 기다린다. 자동으로 구현하거나 승인 범위를 늘리지 않는다.
