# PASS 4G.3 — 연속 피해 숫자 누락 수정 (2026-10-31)

> 체크포인트 보충(2026-10-31): 사용자가 누락 수정 후 기본 콤보의 타격별 숫자 출력과 기본 표시 동작을 MVP 수준 USER VERIFIED로 승인했다. 공중/극단적 다단·투영/수명·문구 겹침 등의 자동 검증을 사용자 승인으로 확대하지 않는다. 정확한 저장 범위는 `docs/SHIBUYA_PASS4G_CHECKPOINT.md`. 아래 LOCAL UNCOMMITTED/검수 대기/실패 이력은 수정 당시 기록이다.

LOCAL UNCOMMITTED. 브랜치 `feat/shibuya-combat-mvp`, HEAD `ce6626ba6148c34e9ea4b98024a47556f9645854`. 사용자 재검수 대기. 기존 PASS4G.0~4G.3 미커밋 작업과 원본 QA를 보존하며, staging/commit/push하지 않는다.

## 확인된 원인과 최소 수정

`ShibuyaFighter.ResolveHits` → `PrepareGradedDamage`에 전달한 `sequenceToken`은 **콤보 전체/공중 체공 관계의 식별자**다. 개별 클릭의 식별자가 아니다. 지상 각 단계 첫 타격은 `resolvingEvent=0`을 사용한다. `ShibuyaAttackGradeRuntime.CommitGradedDamage`의 중복 방지 키 `(Sequence, Event, Target)`가 따라서 정상적인 2~4타를 1타의 재전송으로 판단했다. Health에서는 이미 HP가 차감됐지만, 이후 등급 commit과 `ShibuyaCombatDamageResults.Resolved` 발행이 생략됐다.

수정 전 실제 기본 4타: HP 감소 `20/20/20/25`, HP `915`, accepted action `1/2/3/4`, combo token `1/1/1/1`, event `0/0/0/0`. **HP 감소4회 / 결과 수신1회 / 표시 생성1회 / 등급 commit1회**. 해당 실행 동안 플레이어·피격자·표시 컴포넌트 ID와 구독 수는 일정했다. 표시 슬롯에 도달하기 전 누락된 것이므로 숫자의 수명·크기·밝기를 바꾸어 해결할 문제가 아니었다.

런타임 수정은 `ShibuyaFighter.cs`의 위 호출 인자를 `AcceptedAttackCount`로 바꾸고 이유를 주석으로 명시한 것뿐이다. 이 값은 지상 단계·공중 클릭·반격 시작마다 증가한다. 서로 다른 accepted action을 구분하면서 같은 action/event/target의 재전송은 계속 차단한다. 멀티 타깃은 target, 다단 타격은 event로 구분한다. 확정 연계·추격·체공의 원래 `sequenceToken`은 변경하지 않았다.

중요한 부수 영향: 이 키는 표시 전용이 아니라 기존 등급 commit과 공유돼, 정상 후속 흑섬의 성공 집계·보상도 함께 누락될 수 있었다. 동일 버그를 바로잡아 각 유효 타격이 원래 규칙대로 commit된다. **확률, 배율, 보상량, Zone 수치나 반격 동작을 재조정한 것이 아니다.** 예를 들어 정상 후속 흑섬이 이제 Zone을 갱신하므로 이후 타격의 실제 피해는 기존 Zone 정책에 따라 달라질 수 있다.

## 전달·수명·투영 조사

- Health의 실제 감소량은 차감 순간 out 값으로 포착되고, ReceiveStrike → grade commit → immutable result bus → 로컬 카메라의 `ShibuyaDamageNumbers.Present`로 전달된다. 이번 수정에서 Health/표시 컴포넌트/검수 창은 변경하지 않았다.
- 표시 구독은 OnEnable/Configure에서 중복 없이 생성되고 OnDisable에서 해제된다. QA 창이 이 구독을 소유하지 않는다.
- 기본32슬롯, 수명0.85초, 타깃별5개 위치 분산과 오래된 슬롯 교체 정책을 유지했다. 고정 슬롯에 새 결과를 넣는 횟수를 `Presented`에서 별도로 집계했다.
- 실제 화면 좌표·alpha·문구·타깃 로컬 앵커 추종과 OnGUI 포함 PNG를 함께 확인했다. 카메라 뒤/화면 밖은 기존 정책대로 숨긴다. 카메라 복귀 시 수명이 남은 같은 숫자가 다시 보이지만 새 표시 이벤트는 발생하지 않는다.
- 표시 age는 unscaled time이다. 국소 히트스톱이 피해 결과를 다시 발행하거나 팝업을 추가 생성하지 않는다. OFF는 기존 슬롯을 비우고 이후 표시 생성을 억제하며, 피해·Zone·주력·히트스톱은 유지한다.
- 가드/패링/C 회피/Health 무적/공통 특수방어/죽은 대상/0 피해는 HP 감소·정상 성공 숫자를 만들지 않는 기존 targeted 검사를 재사용해 확인했다. 실제 고죠 무하한 장면을 실행했다는 뜻은 아니다.

## QA 창 Missing과의 관계

`ShibuyaCombatGradeReview.Bind/Restore`는 런타임 프로필 사본을 관리하지만, 씬 재시작 때 예전 `actor` 참조를 자동으로 현재 플레이어로 교체하지 않는다. 파괴된 Unity 객체가 ObjectField에서 Missing처럼 남을 수 있다. 숨겨진 테스트용 창으로 이전 플레이어 연결 → 정상4타 → 실제 전투 종료 → 재시작을 재현했다. **창은 이전 destroyed actor를 가진 상태인데도 새 플레이어의 HP20 감소 / 결과1 / 생성1이 정상**이었다. 재연결도 확인했다.

따라서 이 수명 경로는 이번 콤보 중복 키 결함과 구분된다. 사용자 영상의 바로 그 Missing 순간이 어느 수명 전환 때문인지는 원본 영상을 이번 요청에서 제공받지 않아 확정하지 않는다. 창 자동 재연결 UX를 범위 밖에서 추가하지 않았다.

## 실제 비교 결과

원본 루트: `unity/Logs/ShibuyaPass4G3DeliveryQA/20261031-missing-numbers/`.

| 조건 | 실제 HP 감소 횟수 | 결과 수신 | 표시 생성 | 관측 |
|---|---:|---:|---:|---|
| 수정 전 기본4타 |4|1|1|915 HP, 2~4타 누락 재현 |
| 수정 후 기본4타 |4|4|4|915 HP, 각 신규 숫자 화면 가시성 true |
| 카메라 이동+넉백 |4|4|4|신규 숫자4개 모두 투영/alpha 정상 |
| 일반→치명→흑섬→흑섬 |4|4|4|20/30/50/68.75, 흑섬2·스택2·주력120 |
| 중간 한 타만 OFF |4|4|3|OFF된2타만 의도적 미생성, 나머지 action1/3/4 |
| 2단계×2이벤트 |4|4|4|event0/1/0/1, action1/1/2/2, 중복 없음 |
| 공중3클릭 부분만 |5|5|5|지상4타까지 포함하면9/9/9, HP853 |
| 카메라 뒤→복귀 |1|1|1|뒤에서는 숨김, 복귀 후 같은 표시 재노출 |
| 재시작 후 새 캐릭터 |1|1|1|QA 창의 이전 actor는 Missing 상태 |

등급 혼합4타 마지막68.75는 기존 `25 × Zone1.1 × 흑섬2.5`다. 숫자 때문에 피해를 재계산하지 않았다. 색상·문구·기본 피해20/20/20/25·캐릭터 데이터는 보존했다.

### 비교 이미지와 원속도 영상

- 수정 전 2타 직후: `before-fix-trace/legacy/ShibuyaPass4B3QA/four-step-hit-2.png` — HP960, 숫자1개.
- 수정 후 같은 조건: `after-fix-four-step/legacy/ShibuyaPass4B3QA/four-step-hit-2.png` — HP960, 분리된 숫자2개. 1~4타 개별 PNG도 같은 디렉터리에 있다.
- 원속도: `after-fix-delivery-cases/legacy/ShibuyaPass4B3QA/four-step-original-speed.mp4`. 실제 OnGUI 포함78장과 실시간 timestamp로 약2.82초를 구성했다. 출력60fps는 시간 재배치이며 네이티브60fps 촬영/보간이 아니다. 원본1075×500 화면 오른쪽1px padding으로1076×500 인코딩, 크기/수명 변경 없음. concat·timestamp·인코딩 로그·원본 PNG 보존.
- 동일 복수 검사 실행 내 공통 `four-step-hit-*` 파일명은 뒤 검사 캡처로 교체될 수 있었다. **수정 전후 근거는 위 독립 실행 디렉터리의 PNG**를 사용한다. 혼합 등급/이동별 수치 근거는 각 이름의 JSON을 사용한다. 이후 새 실행에서는 테스트 이름 하위 폴더로 PNG를 분리하도록 QA 저장 경로만 보완했다.

## 실패 이력과 검증 공백

기존 PASS4G.3의33 PASS는 당시 고유 케이스의 복수 실행 집계다. 그 검사는 한 accepted action 안의 다단 타격과 기존 콤보 HP를 확인했지만, **서로 다른 콤보 단계의 HP 감소 횟수와 표시 횟수를 대조하지 않아 이번 결함을 놓쳤다.** 이전 통과를 취소하거나 현재 성공으로 덮어쓰지 않는다.

이번 `before-fix-trace`의 의도된 4대1 FAIL, 최초 새 기록기의 Rect 직렬화 실패, 공중/재시작 QA 실패 XML을 모두 남긴다. 새 fixture의 completion witness는 본문이 끝나지 않은 검사를 PASS로 집계하지 않는다. 실패 뒤 같은 실행에서 후속 coroutine이 완료되지 않은 사례도 별도로 보존한다.

재시작 검사 초기 실패는 `Finished=false`에서 RestartBattle을 호출한 테스트 구성 오류였다. 실제 종료 뒤 재시작하도록 수정해 독립 실행 PASS를 확인했다. 공중 첫 클릭 타임아웃 계측에서는 chase 도착 후 `AcceptedAttackCount=4` 그대로, HP/수신/생성도4 그대로였다. Editor coroutine이 한 Player Update 사이에 여러 번 진행해 테스트의 한-yield 클릭이 입력 소비 전에 켜졌다 꺼진 경로다. 입력 접수·해제 소비를 실제로 기다리도록 새 QA만 보완한다. 원래 타격 기대 개수·게임플레이 상한·시간·데이터를 완화하지 않는다.

`air-real-input-delivery`의 실제 입력 소비 계측 실행은 PASS다. accepted action5/6/7, 최종 confirmed hit9, 공중 부분만 HP 감소5 / 결과5 / 생성5를 확인했다. 첫 실패 기록을 삭제하지 않는다. 새 입력 검사는 클릭 유지로 자동 공격을 추가하지 않고, 각 에지가 한 번 접수된 뒤 해제가 처리됐는지 확인한다.

### 최종 targeted 결과

**CODEX VALIDATED(targeted) / USER VERIFIED 대기. 최신 고유13 PASS / 0 FAIL / 0 SKIP는6회 실행의 케이스별 최신 결과**이며 단일13개 전체 실행이 아니다. 기존33개에 더해 전체46개가 재검증됐다고 표현하지 않는다.

- `after-fix-four-step`:1 PASS.
- `after-fix-delivery-cases`:5 PASS/2 FAIL. 실패2건은 아래 독립 실행으로 대체하되 원본 유지.
- `multistep-isolated`:1 PASS.
- `window-isolated`:1 PASS.
- `air-real-input-delivery`:1 PASS.
- `existing-targeted-regression`:단일4 PASS. 실제5이벤트·추가 Collider/재전송·방어/무적/특수방어·OFF 동등성·반격 흑섬/hold 중복 방지.

`latest-unique-results.json`은 각 테스트의 결과 XML 출처와 모든 실행의 PASS/FAIL/SKIP를 별도로 보존한다. 위 성공 출처 외 초기 실패 실행도 삭제하지 않는다. 이 기술 검증은 USER VERIFIED나 장시간/멀티플레이 보증이 아니다.

## 컴파일·Console·보존

컴파일 정상. 마지막 QA 직후 Console error0/warning0이었으나, PNG 저장 경로 보완 후 최종 재컴파일에서는 **Editor Console error0/warning5**다. 기존4F 테스트의 CS0108 1건/CS0252 2건과 `Deleting invalid font reference.` 2건이다. 새 테스트/런타임의 컴파일 오류는 없다. 세션 도구 버퍼에는 **Pipeline main-thread 5000ms timeout 오류5건**도 남아 있다. 도구 timeout 후 실제 검사는 실행되는 경우가 있어 파일/실행 상태를 확인한 뒤에만 다음 검사를 시작했다. 따라서 세션 전체 오류0이라고 주장하지 않는다. Console/QA 실패 기록을 지워 성공으로 만들지 않았다.

Play stopped, 씬 clean, fps -1, Time.timeScale1, 실제 AutoTick true/16ms, Game View PlayUnfocused. 이번에 AutoTick/화면 포커스 정책을 바꾸지 않았다. 테스트 fixture의 임시60fps는 종료 시 -1로 복구했다. 표시 observer0/component0. 초기 실패 QA에서 남긴 비표시 EditorWindow1개만 instance ID·parent 없음·파괴된 actor를 확인하고 정리했으며 사용자의 표시 중인 검수 창은 보존했다.

기존1339개 작업 파일의 시작 SHA-256과 비교해 기존 파일 변경은 `ShibuyaFighter.cs`와 보고 문서3개뿐이며 누락0임을 확인했다. 기존4G/4G.3 QA437개도 SHA-256 일치/누락0이다. 새 QA는 별도 디렉터리에만 저장했다. 기존 Data Asset/Scene/Prefab/Packages/ProjectSettings/Gojo/VFX/아우라/잠금 문서/`.slnx`/기존 .meta는 작업 시작 바이트 그대로다. `preservation-final.json`, `editor-final.json`, `console-final.json`에 증거를 남긴다.

이번 파일 범위는 `ShibuyaFighter.cs`, 신규 `ShibuyaPass4G3DeliveryTests.cs`와 .meta, 이 보고서, 기존4G.3 보고서의 정정 안내, CURRENT_HANDOFF/CURRENT_TASK의 최신 수정 기록이다. 표시 컴포넌트나 기존 Inspector/자산 설정은 수정하지 않는다.

최종 Git은 시작과 같은 브랜치/HEAD, tracked dirty34 / untracked50 / staged0이다(시작34/47/0, 신규 테스트/.meta/보고서3파일 증가). commit/push/reset/clean/worktree 없음. 초기 FAIL/XML은 새 루트에 전부 유지한다. 남은 시각 한계로 다단 흑섬의 큰 문구나 결과 패널과의 부분 겹침은 가능하며, 숫자 누락 수정과 별개로 사용자 재검수 대상이다.

## 사용자 재검수

`ShibuyaCombatMVP` Play → 기존 메뉴 `JJK Game/시부야 PASS4G/공격 등급·반격 검수` → 현재 플레이어 연결 → 일반 추첨 → 정상 거리에서 LMB4회. 각 HP 감소에 새 숫자가 별도 위치로 나타나는지 확인한다. 같은 방법으로 W+4타 → Space → 공중 LMB3회(2+2+1)를 확인한다. 이동하며 공격하고 카메라를 돌려 본다. 숫자 OFF에서 HP만 감소하고, 재ON 후 새 피해만 표시되는지 확인한다. 흑섬 검수는 기존 QA 허용/추첨 정책을 그대로 따른다. 가드·회피·특수방어는 실제 HP가 줄지 않으면 성공 숫자가 없어야 한다.

화면 밖 숫자 숨김, 32슬롯 포화 시 오래된 표시 교체, 수명 만료는 기존 의도적 정책이다. 지형 뒤 별도 숫자 occlusion raycast와 정식 설정 저장 메뉴는 이번에 추가하지 않았다.
