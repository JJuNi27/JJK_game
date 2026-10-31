# PASS 4G.3 — 전투 데미지 표시·치명타·흑섬 시각화

> 2026-10-31 체크포인트 보충: 사용자는 숫자 누락 수정 후 데미지 숫자와 기본 콤보의 타격별 출력을 MVP 수준 USER VERIFIED로 승인했다. 화면 밖 정책·극단적 다단·문구 겹침·최종 시각 품질 전체를 승인한 것은 아니다. `docs/SHIBUYA_PASS4G_CHECKPOINT.md` 참고. 아래 검수 대기·최초33 PASS·FAIL은 각각 당시 이력으로 보존한다.

> **2026-10-31 사용자 피드백 후 정정:** 아래33 PASS는 당시 기록이다. 콤보 전체 식별자를 개별 타격 거래 키로 사용해 정상 후속 단계가 중복 처리되는 결함을 추가 재현했다. HP4회/표시1회 → 수정 후4회/4회. 개별 accepted action 식별자로 최소 수정했으며 추가 targeted 최신 고유13 PASS는6회 실행 집계다. 공중 부분5/5, OFF·차단·재전송·재시작도 확인했다. 상세 원인/원본 실패/비교 자료는 `docs/SHIBUYA_PASS4G3_DELIVERY_REVIEW.md`. 기존 기록은 삭제하지 않는다. USER VERIFIED 대기.

> 최종 상태: **CODEX VALIDATED(targeted) / USER VERIFIED 대기**. 최신 고유33 PASS/0 FAIL/0 SKIP는6회 실행 집계다. 단일 전체33개 실행 결과가 아니다.

2026-10-31. LOCAL UNCOMMITTED / 사용자 Play Mode 검수 대기. 시작 브랜치 `feat/shibuya-combat-mvp`, 로컬 HEAD `ce6626ba6148c34e9ea4b98024a47556f9645854`. 시작 상태는 tracked dirty31/untracked34/staged0으로 이전4G.0~4G.2 작업과 일치했다. 기존4G QA164개와 기존 tracked/untracked1326개 파일의 SHA-256을 별도 보존했다. 기존 PASS 보고서·잠금 문서를 변경하지 않는다.

## 실제 피해와 표시의 경계

- 기존 `Health.ReceiveDamage(context)` API는 유지한다. 읽기 전용 `out appliedHealthLoss` 오버로드만 추가해 **HP를 차감하는 바로 그 순간**, HealthChanged/DamageResolved/Died 알림 전에 감소량을 캡처한다. 기존 피해·방어 판정과 콜백 순서는 동일하고 HP 차감은1회다. 알림 도중 회복/중첩 공격/환경 사망이 발생해도 다른 변화가 이 공격의 숫자에 섞이지 않는다.
- `ReceiveStrike`와 실제 Fighter 타격 경로가 해당 측정값을 기존 등급 commit에 전달한다. 성공 보상·확률·Zone 계산은 원래 코드가 담당한다. 과거 commit API의 호환 fallback은 보존하되, 표시 이벤트는 **측정값이 전달된 거래만** 발행한다. Applied라는 열거값만 보고 예상 피해를 표시하지 않는다.
- `ShibuyaCombatDamageResult`는 공격자/대상, 실제 HP 감소, 이미 확정된 등급, 위치, 시퀀스/이벤트/대상/행동 종류, 실제 회복 주력, Zone 진입/갱신의 immutable snapshot이다. Grab도 같은 측정 오버로드로 실제 피해15를 일반 숫자로 전달한다. 환경 Kill 자체에 숫자를 합성하지 않는다.
- publisher는 기존 등급 commit의 대전 수명 중복 키를 유지한다. 표시에도 bounded recent key 이력과 슬롯이 있어 반복 콜백을 방어한다. strike/grab 키를 구분한다. observer 예외는 전투 거래를 중단시키지 않도록 격리한다.
- Guarded/Invulnerable/Invalid/TargetDead,0·음수·NaN/Infinity 및 잘못된 표시 위치는 숫자/성공 문구를 만들지 않는다. 일반 가드·패링·실제 C 회피 무적·Health 무적·별도 IDamageGuard 차단을 검사한다. 특수 방어는 공통 guard 대역 검사이며 실제 고죠 무하한/VFX 장면을 새로 실행했다는 뜻은 아니다. 기존 무하한 규칙과 자산은 불변이다.

## UI·옵션·설정

기존 MVP UI와 같은 **카메라별 IMGUI overlay**를 사용한다. 기존 HUD·Canvas·Camera/LockOn 구조를 교체하지 않는다. PlayerInput 시작 경로에서 로컬 OrbitCamera에 별도 표시 컴포넌트를 연결하므로 씬/프리팹 수정이 없다. 타격 위치의 대상 로컬 앵커를 따라 머리·상체 근처에 표시하며 WorldToViewportPoint.z/화면 범위를 검증한다. 카메라 뒤/화면 밖은 숨기고 중앙 고정/반전 표시를 하지 않는다.

고정 배열 슬롯 재사용, 기본32개/상한128개. 같은 대상의 연속 타격은 사용량이 적은5개 위치와 보조 행에 분산한다. 꽉 차면 가장 오래된 표시를 교체하며 타격을 합산하지 않는다. OFF는 즉시 모든 슬롯을 비우고 새 UI 객체·폰트를 만들지 않는다. 재ON은 이후 새 거래만 표시한다. `ShibuyaDamageDisplayOptions` 인스턴스/변경 이벤트와 Configure 주입 경계로 미래 설정 저장/메뉴를 연결할 수 있다. PlayerPrefs나 정식 설정 메뉴는 이번에 만들지 않았다.

새 Resources 자산 `Assets/Shibuya/Resources/Shibuya Damage Numbers.asset`의 한글 SerializedProperty Inspector에서 조절한다:

| 항목 | 기본값 |
|---|---|
| 표시 | ON |
| 일반 | 흰색,32, 확대1.15배 |
| 치명타 | 금색(1,.78,.12),40, 확대1.35배, `CRITICAL!` |
| 흑섬 | 붉은색(1,.12,.20)+검은 외곽,50, 확대1.6배, `흑섬!` |
| 수명/상승 |0.85초/65px, 시작 확대 곡선·후반 fade 곡선 |
| 위치 |월드(0,.65,0), 화면(0,-8), 연속 간격112px |
| 외곽 |어두운 먹색,2px |
| Zone 보조 |별도 작은 문구, 주 숫자의0.42배, 옅은 청백색 |
| 폰트 |기존 프로젝트와 같은 OS Malgun Gothic, 자산 폰트도 지정 가능 |

주력 보조 숫자도 실제 회복량을 읽는다. 최대 주력이라+0이면+10을 거짓 표시하지 않는다. 정수는 정수로, 소수는 float round-trip precision으로 표시한다. 정수 근처의 작은 float 오차만 정리하며 매우 작은 양수도0으로 지우지 않는다. 글꼴·색상·크기·수명·이동·곡선·문구·동시 개수·외곽을 모두 한국어 Inspector에서 편집하고 Undo/기존 편집을 유지한다.

## 기존 QA 버튼의 실제 기능

메뉴는 기존 `JJK Game/시부야 PASS4G/공격 등급·반격 검수`를 확장했다. 새 버튼으로 기존 RNG 동작을 교체하지 않았다.

- `일반 추첨`: 이후 매 타격의 RNG에0.99999를 공급한다. 보통 일반 공격이 되지만 확정 치명타 반격 같은100% 규칙은 여전히 치명타다.
- `치명타 추첨`: 타격 시작마다0/0.99999 순서를 공급한다. 치명타 자격/확률이 있어야 하며 흑섬 추첨은 실패시킨다.
- `흑섬 추첨`:0/0을 공급하므로 **자격·양수 확률이 있는 경우에만** 흑섬이다. 캐릭터 기본 흑섬OFF 정책을 우회하지 않는다. QA 전용 허용을 명시적으로 켜야 한다.
- 다음 한 타 예약이 아니라 이후 타격용 RNG 교체다. 고정 시드를 다시 적용하거나 창을 닫으면 종료한다. 창의 런타임 Grade Profile 사본만 수정하며 실제 자산 확률을 저장하지 않는다.

추가 항목은 데미지 표시 ON/OFF, 마지막 실제 표시 피해/등급, 성공·차단 사유, 현재/누적 표시 개수, 표시 설정 자산 선택이다. 숫자 OFF는 일반 전투 VFX/주력 아우라/타격음/히트스톱/Zone을 끄지 않는다.

## 검증과 최초 실패 보존

루트: `unity/Logs/ShibuyaPass4G3QA/20261031-damage-display/`. 각 실행은 별도 results.xml/execution.jsonl이며 이전 실행을 덮어쓰지 않았다.

- `initial-targeted`:19개 중17 PASS/2 FAIL. 실제 수치·다단·OFF·초과 피해 등은 통과했지만 방어 검사에서 패링 복귀0.1초 중 대시를 요청했고, 정리 검사에서는2회 Editor continuation만으로 Destroy 완료를 가정했다.
- **이미지 직접 확인에서 별도 렌더 결함 발견:** 동적 OS Font.name을 진단용 이름으로 바꾸면서 FontEngine이 없는 family를 조회했다. 초기 자동 수치 PASS를 시각 성공으로 취급하지 않았다. 잘못된 글자/반복 경고를 `editor-initial-run.log`, 초기 PNG/XML과 함께 보존했다. 해당 원본에는 폰트 face 경고3340회와 family 조회 메시지3340회가 있다. Font.name 변경을 제거한 재촬영에서 실제 숫자·한글이 정상적으로 보였다.
- `font-and-failures-recheck`:4개 중3 PASS/1 FAIL. 패링 뒤 대시는 기존 복귀가 끝난 후 요청하도록 QA 입력만 수정했다. 폰트 이름은 정상화했지만 정리 완료 관측은 계속 실패했다.
- `font-lifecycle-probe`: 동일 정리1 FAIL. 계측은 fontDestroyed=false와 numberDestroyed=false를 보여 줬다. font 자산을 강제 삭제하거나 기대값을 완화하지 않았다.
- `disposal-and-spacing-check`:3 PASS. 실제 게임 프레임 종료의 Destroy 완료를 유한1초 내 기다린 후 같은 폰트 instance ID의 해제와 observer0을 확인했다. 기록은 프레임120→121,180개 제출/2개 슬롯/Transform99개 불변이다. Editor continuation과 게임 프레임을 혼동했던 QA 문제다. 숫자 겹침 간격과 작은 Zone 문구도 재촬영했다.

실제 결과:20/30/50/55 표시, 남은 HP12에서는50 대신12. HealthChanged에서10 회복한 상황도 해당 타격20을 정확히 표시. 실제5히트는20/50/33/55/22이며, 첫 흑섬 뒤 Zone의 치명타이므로 세 번째가33이다. 복수 대상 개별 표기·추가 Collider/receipt replay 중복 방지·생명주기·카메라 뒤/밖 숨김·fade·표시 옵션 반복12회·일반 잡기15를 확인했다. ON/OFF 비교는 같은 RNG draw2회/동일 피해50/흑섬1/스택1/주력110/동일 국소 hit-stop이며, 다시ON 후 새 Zone 타격55만 표시했다.

## 시각 증거와 사용자 확인

`final-related-regression/results.xml`은 **단일14 PASS/0 FAIL/0 SKIP**다. 기본/업/다운·공중3입력5히트·반격 치명타/흑섬·잡기·카메라 기준8방향 대시·방향 기상·실제 Zone 소비/충전/만료·F 아우라 동일성·훈련 AI·카메라 시네마틱 복귀·사망/재시작을 확인했다. 기존4G.0~4G.2의58개 전체를 다시 실행하거나 그 숫자를 이번 결과에 더하지 않았다. PASS4A 과거 미해결18건도 해결로 소급하지 않는다.

`final-overkill-and-toggle-visual`은2 PASS로, 폰트 수정 후12 HP 마무리와 OFF/재ON 이미지를 추가했다. 초기19개(10개 데이터+9개 Play)와 기존 회귀14개의 **최신 고유33 PASS**를6회 실행에서 확인했다. 최초17/2, 재검3/1, 원인 계측0/1, 정리/간격3/0, 관련 회귀14/0, 최종 시각2/0을 보존한다. `latest-unique-results.json`이 각 케이스의 최신 출처다.

정상화된 등급별 PNG: `font-and-failures-recheck/legacy/ShibuyaPass4B3QA/4G3-grade-1.png`~`4G3-grade-4.png`. 최종 간격의 다단 PNG: `disposal-and-spacing-check/legacy/ShibuyaPass4B3QA/4G3-five-hits.png`. 최종12 HP 및 OFF/재ON PNG: `final-overkill-and-toggle-visual/legacy/ShibuyaPass4B3QA/4G3-overkill-twelve.png`, `4G3-display-off.png`, `4G3-display-on-again.png`. 초기 결함 PNG는 삭제하지 않고 실패 이력으로 남긴다.

최종 원속도 영상: `disposal-and-spacing-check/legacy/ShibuyaPass4B3QA/4G3-damage-overlay-original-speed-padded.mp4`. 실제 OnGUI 포함 캡처68장을 실시간 타임스탬프로60fps에 배치한 것이며 네이티브60fps 촬영/모션 보간이 아니다. Game View 너비1075의 홀수 크기로 첫 H.264 인코딩이 실패했고 오른쪽1px padding만 추가해1076으로 인코딩했다. 픽셀 크기를 줄이거나 게임 화면 설정을 바꾸지 않았다. 실패 출력과 인코딩 기록도 보존했다.

Play → 기존 등급 검수 창 → 현재 플레이어 연결 → 일반/치명타/흑섬 추첨을 선택하고 LMB. 흑섬은 QA 전용 허용을 켠다. 숫자 OFF 시 기존 숫자가 즉시 사라지고 HP·주력·Zone은 계속 변하는지 확인한다. 재ON은 새 공격만 표시해야 한다. 최종 시각 크기·흑섬 검붉은 강조·다단 밀도는 사용자 직접 검수 대상이다. 고급 흑섬 번개/새 모션/시네마틱은 만들지 않았다.

## 남은 한계

표시는 로컬 카메라의 관찰용 MVP다. 네트워크 동기화·정식 옵션 메뉴·선택 영구 저장·전역 UI 레이아웃 최적화는 미구현이다.32개를 넘으면 오래된 표시를 버리고, 극단적인 연속 공격에서 모든 숫자를 끝까지 유지하지 않는다. 표시는 HitPoint 기반이며 지형 뒤 숫자의 별도 occlusion raycast는 하지 않는다. 미래 설정 어댑터가 Options를 주입하면 되지만 설정 메뉴 자체가 완성된 것은 아니다. 동적 폰트의 Unity TextCore 캐시/장시간 GPU 메모리 프로파일링은 별도로 수행하지 않았다.

흑섬의 순간 확대와 다단 문구는 일반 숫자보다 화면 점유가 크다. 마무리 직후 결과 패널 근처에 잠깐 겹칠 수 있어 최종 크기/밀도·승리 UI 관계는 사용자 직접 검수 대상으로 남긴다. 기존 HUD를 옮기거나 카메라를 바꿔 숨기지 않았다.

## 최종 컴파일·Console·보존·파일

모든 컴파일 요청은 정상 종료했다. 최종 Editor Console **error0/warning2**이며 경고는 `Deleting invalid font reference.`다. 이전에도 있던 경고 유형이지만 숫자 표시도 동적 폰트를 사용하므로 두 경고를 전부 기존 것이라고 단정하지 않는다. 이번 초기에 추가된 잘못된 family의 경고3340회/조회 메시지3340회는 수정 이후 추가 발생0임을 initial/final Editor.log 비교로 확인했다. 단순히 Console을 지워0으로 보고하지 않았다.

최종 Play stopped/scene clean/compiling false/test inactive, fps -1/Time.timeScale1/runInBackground false, AutoTick true16ms/PlayUnfocused. 표시 컴포넌트와 결과 observer 잔류0, 새 Inspector 누락 한글 label0. 풀 검사는180개 제출에도 Transform 개수99로 불변이며2개 슬롯만 재사용했고, 실제 소유 Font instance 해제를 확인했다. 팝업은 Mesh/Material/Canvas를 직접 생성하지 않는다. 관련 환경은 `environment-final.json`, 보호 및 전체 파일 목록은 `final-protection-and-inventory.json`이다.

기존 파일 수정9개: `unity/Assets/Scripts/Core/Health.cs`; Shibuya의 `Scripts/ShibuyaAttackGradeRuntime.cs`, `ShibuyaFighter.cs`, `ShibuyaGrabRuntime.cs`, `ShibuyaGuardRuntime.cs`, `ShibuyaPlayerInput.cs`; `Editor/ShibuyaCombatGradeReview.cs`; `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md`.

신규13개(meta 포함): Shibuya의 `Scripts/ShibuyaCombatDamageResult.cs`, `ShibuyaDamageNumberProfile.cs`, `ShibuyaDamageNumbers.cs`; `Editor/ShibuyaDamageNumberInspector.cs`, `ShibuyaPass4G3Tests.cs`와 각 `.meta`; `Resources/Shibuya Damage Numbers.asset`와 `.meta`; 본 보고서다.

브랜치/HEAD는 그대로 `feat/shibuya-combat-mvp` / `ce6626ba6148c34e9ea4b98024a47556f9645854`. 최종 Git은 tracked dirty34/untracked47/staged0(기존4G 작업 포함). 기존 `.slnx`,4G QA164개, 기존 Data Asset·Meta GUID·Scene/Prefab/Packages/ProjectSettings·고죠/VFX·승인 아우라·잠금 문서는 시작 해시와 동일하다. 기존 보고서와 실패 XML도 보존했다. git diff --check 통과, staging/commit/push 없음.
