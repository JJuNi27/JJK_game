# PASS 4H.0 — 공통 흑섬 월드 VFX 본체

## 범위와 기준

2026-11-01, `feat/shibuya-combat-mvp`, 기준 HEAD `891064067834bc26f3a1afc6a37b84bb5e1e79c1`. 기존 로컬 레퍼런스 분석 문서4개와 사용자 `unity/unity.slnx` 변경을 보존하며 이어서 구현했다. 이번 범위는 공통 월드 VFX와 Applied 피해 이벤트의 읽기 전용 연동이다. 화면 반전·새 카메라 shake·추가 hit-stop·음향·Zone VFX는 후속 PASS다. 일반 흑섬마다 짧은 화면 반전을 넣는 최종 합의는 유지된다.

**CODEX VALIDATED(targeted) / USER VERIFIED 대기**. 최신 고유11 PASS / 0 FAIL / 0 SKIP는6회 실행의 케이스별 최신 집계다. 신규8개와 기존 선택 회귀3개이며 단일11개 전체 실행이라고 표현하지 않는다. 시각 품질은 사용자의 직접 검수 대기다.

## 구현과 레퍼런스 대응

로블 Manji 약00:00.9~00:02.7, 주팬퍼 나나미01:06.6~01:07.067 인접 프레임 시트를 다시 직접 확인했다. 상세 관측은 `design/BLACK_FLASH_VFX.md`에 남겨 두었다. 원본 자산/코드는 추출하지 않았다.

| 층 | 구현 | 레퍼런스와의 관계 / 한계 |
|---|---|---|
|접촉광|서로 다른3D 축의 짧은 흰 가시, additive unlit|접촉을 즉시 강조. 화면 반전은 아님. 긴 프레임에서는 짧은 peak를 놓칠 수 있음|
|검은 충돌핵|불규칙한 닫힌 메시와 서로 겹치는 작은 파열 질량, 빠른 팽창 후 노이즈 소거|검정 면적과 붉은 경계를 같은 표면에 표현. 초기 촬영에서 붉은 면이 과해 Fresnel 범위/깊이/뒷면 처리를 보완|
|검붉은 가지|6면 입체 tube, 불규칙 경로·굵기·길이·앞뒤 방향과 추가 분기|시점에 맞춘 카드가 아니며 서로 다른 깊이를 갖는다. 애니메이션 원화의 손그림 분기와 동일한 수준의 형태 제어는 아님|
|압력|기울기·깊이·길이가 다른 끊긴 호7개가 빠르게 팽창|완전한 원형 disc 대신 불완전한 압력 인상. 실제 화면 굴절/공간 refraction은 미구현|
|잔류|별도 alpha 재질의 검은 불규칙 질량과 작은 방전, 상승/분리/소거|주황 불꽃 없음. 실제 유체 연기 시뮬레이션은 아니며 가까이 보면 메시 조각의 질감이 남을 수 있음|

검은 핵/가지에는 additive를 사용하지 않는다. `SrcAlpha OneMinusSrcAlpha`와 깊이 기록을 사용해 검정이 배경에 사라지는 문제 및 뒷면의 과도한 붉은 겹침을 줄였다. 연기는 같은 shader의 깊이 기록OFF 별도 공유 재질이다. 광/압력만 additive다. 기존 F 아우라 shader/geometry/profile과 독립적이다.

## 실제 피해 연동

`ShibuyaMatch.Start`에서 `ShibuyaBlackFlashPresentation.Attach(this)` 한 줄로 scene-local observer를 붙인다. Scene/Prefab 직렬화는 변경하지 않는다. 실제 `ShibuyaCombatDamageResults.Resolved`에서 다음 조건을 모두 검사한다.

- `DamageResolution.Applied`.
- `ShibuyaAttackGrade.BlackFlash`.
- 유한한 실제 `HealthLoss > 0`.
- 유한한 `WorldPoint`, 동일 scene.

개별 action/event/target의 `ShibuyaDamageResultKey`를 중복 방지에 사용한다. 다른 타격의 흑섬은 별도로 수락한다. 숫자 표시 옵션과 카메라 유무에 의존하지 않는다. 실제 HP가0이 되어 Match가 먼저 StopCombat해도 이미 성립한 결과는 재생한다. 확률 재추첨, 피해, 주력 보상, Zone 스택을 VFX가 호출하지 않는다.

좌표는 결과의 `WorldPoint`이며 정밀 메시 접촉점은 아니다. 방향은 결과 수신 시 공격자 가슴 근처→타격점으로 계산해 동결하는 시각적 근사다. 실제 접촉 법선이나 기술별 모션 축이라고 설명하지 않는다. 기존 `HitConfirmed`를 두 번째 발생 경로로 쓰지 않아 중복되지 않는다. 결과는 기존 hit-stop 이전 같은 프레임에 전달되고 본체는 unscaled clock으로 진행한다. 기존 소리/hit-stop/카메라 소유권은 변경하지 않는다.

## 데이터와 한국어 Inspector

`Assets/Shibuya/Resources/Shibuya Black Flash.asset`를 일반 전투에서 로드한다. QA 메뉴 선택이 필수인 구조가 아니다. `ShibuyaBlackFlashInspector`는 SerializedProperty/한국어 GUIContent/Undo/다중 선택을 사용한다.

| 설정 | 시작값 |
|---|---|
|전체 크기 / 발광 강도|1 / 1|
|검은 핵 크기 / 팽창|0.62 / 1.65|
|붉은 색(HDR)|3.2 / 0.012 / 0.025|
|주 가지 굵기 / 길이 / 개수|0.14 / 2.25 / 11|
|가지당 추가 분기 / 전방 편향|2 / 0.32|
|흰 접촉광 강도 / 시간|5 / 0.045초|
|핵 / 가지 시간|0.19 / 0.28초|
|압력 반경 / 확장 속도|2.1 / 11|
|연기 수 / 크기|12 / 0.24|
|잔류 / 전체 수명 상한|0.62 / 0.75초|
|동시 재생 상한|8 (Inspector1~16)|

값은 USER VERIFIED 전 시작 튜닝값이다. 광/연기/압력은 지연과 개별 수명을 가지며 전체 상한을 넘으면 정리한다. 매 프레임 새 오브젝트/재질/메시를 만들지 않는다. 인스턴스당5개 메시, scene당 핵/연기/광3개 공유 재질을 소유한다. 새 결과는 비활성 슬롯을 재사용하고 포화 시 가장 오래된 잔류를 교체해 새 타격 peak를 출력한다. `RecycledLiveCount`로 이 교체를 구분한다. 중복 키 집합은 Match 수명 동안 유지하고 Match 파괴 시 해제한다.

## 실행 이력과 근거

원본은 `unity/Logs/ShibuyaPass4H0QA/20261101/`이며 덮어쓰지 않는다.

| 실행 | 범위 | 결과 / 이유 |
|---|---|---|
|`initial`|신규7개|2 PASS / 5 FAIL. MaterialPropertyBlock을 MonoBehaviour 필드 초기화에서 만들면 안 되는 실제 초기화 오류와 정리 검사 실패|
|`initialization-fixed`|실패했던 런타임5개|4 PASS / 1 FAIL. Initialize에서 생성하도록 수정 후 피해 연동·차단/치명타·실제 촬영 통과. 정리 검사는 파괴 관측 시점 조사로 분리|
|`final-targeted`|변경 관련4개+기존 회귀3개|단일7 PASS / 0 FAIL / 0 SKIP|
|`smoke-final`|연기 geometry/shader와 실제 촬영|단일4 PASS / 0 FAIL / 0 SKIP|
|`contact-final`|접촉광·압력 외곽 보완 촬영|단일2 PASS / 0 FAIL / 0 SKIP|
|`blockers-final`|실제 측면 회피/IDamageGuard/무효 감소량|단일1 PASS / 0 FAIL / 0 SKIP|

초기 신규 테스트의 잘못된 타격 필드 `delayBefore`를 실제 `startTime`으로 수정한 컴파일 실패, CLI inline 문자열 전달 실패도 각각 원본 JSON에 보존했다. 초기 Component.light 이름 가림 경고도 이름을 분리해 수정했다. 테스트 기대값이나 전투 승인값을 낮추지 않았다. 리소스 검사에서는 Editor coroutine의 단순 `yield null` 대신 실제 runtime Destroy 완료와 이후 시간을 기다린 뒤 동일한0개 기준을 확인한다. 수정 전 정리 검사에서 재질2개가 관측되었고 완료 대기 후0개다. 최초 초기화 오류가 섞인 실행의 모든 관측 차이를 별도 원인까지 확정한 것은 아니다.

실제 경로 계측:

- 일반20 / 치명30 / 흑섬50 HP 감소. 앞의2개는 흑섬 VFX0, 흑섬만1개.
- 숫자OFF에서 이후5타 흑섬도 각각55 HP 감소/새 이벤트로 재생. Zone 버프에 따른 기존55를 유지하며 VFX가 계산하지 않는다. 이 검사에서 실제 피해 결과8개, 유효 흑섬6개, VFX수락6개, 같은 결과 재전송 추가0.
- 가드/Health 무적/허공은 VFX0, 마지막1 HP를 소진한 흑섬은 Match 종료 후에도1개 재생하고 수명 종료 후0개.
- 측면 C 회피의 실제 `IsGroundDashEvading` 구간에 ReceiveStrike→Commit을 통과시켜 Guarded/감소0/VFX0 확인. 기존 IDamageGuard 대역도 Guarded/0이다. **실제 Gojo 무하한 씬 실행은 아니다.** 0/음수/NaN/무한 HP 감소 결과도 직접 observer에 전달해 거부했다.
- 기존4타 혼합 등급의 숫자4개/흑섬 보상2회, 패링 반격·홀드 반복 방지, 현재 주력량과 F 아우라 크기 독립·본인 HUD 회귀3개 통과.

`latest-unique-results.json`에11개 fullname별 최신 출처를 기록했다. 각 XML과 최초 FAIL은 그대로다. 현재 결과를 과거 전체 회귀 성공으로 확대하지 않는다.

### 실제 화면 근거

최종 경로(위 QA root 기준):

- `evidence-final/stages-and-angles.png`: 정면 전투거리0.02/0.09/0.21/0.43초, 측면/하이앵글 peak 비교.
- `contact-final/legacy/ShibuyaPass4B3QA/4H0-views/`: 근접/전투거리/45도/측면/하이앵글, 각4개 시점과 어두운 배경 격리 캡처.
- `evidence-final/normal-critical-blackflash-original-speed.mp4`: 일반/치명/흑섬3열,1.1초.
- `evidence-final/normal-critical-blackflash-sequence-original-speed.mp4`: 위3개를 순서대로 재생,3.3초.
- `evidence-final/grade0-original-speed.mp4`, `grade1-original-speed.mp4`, `grade2-original-speed.mp4`: 개별960×720 영상.
- `evidence-final/capture-metrics.json`, 원본 `contact-final/legacy/ShibuyaPass4B3QA/4H0-video-timestamps.json`: 실제 프레임 시각/피해/VFX수락 추적.

동일 시드71·동일 나이의 다각도 캡처는 무피해 형태 비교이며 실제 피해 입력과 이동 카메라 영상과 구분한다. 각 영상은 실제 unscaled 타임스탬프 간격으로 프레임을 유지해60fps 파일로 출력했다. **실제60fps 캡처나 보간 영상이 아니다.** 마지막 촬영은 일반31/치명23/흑섬29개 원본 프레임이다. 경계 프레임의 이전 dt에는225.6/165.6/354.1ms 지연이 포함돼 있으므로 매끄러운60fps로 보고하지 않는다. 흑섬 수락은0.0614초 표본이며 이후0.1233초 표본까지 약62ms 간격이었다. 짧은 flash의 모든 중간 형상을 기록한 것은 아니다.

카메라 RenderRequest 캡처는 IMGUI HUD/숫자·오디오를 포함하지 않는다. 숫자 전달/가시성은 별도 기존4G3 테스트로 확인한다. 최종 PNG의 검은 핵·붉은 입체 분기, 접촉광, 압력 호, 잔류를 직접 확인했다. 근접에서 핵은 각진 파열 질감이 강하고 연기는 아직 여러 질량의 묶음으로 읽힐 수 있다. 전투 정면에서는 공격자가 핵/광을 가린다. 압력은 검붉은 본체보다 약하게 읽히는 각도가 있다. 레퍼런스의 잉크 원화·실제 공간 굴절·후속 화면 강조까지 완성한 품질이라고 선언하지 않는다. 어두운 격리 캡처는 실제 야간/Bloom on-off 회귀와 다르며 그 비교는 미검증이다.

### 성능·정리·Console

기본1개는6,252정점/5메시이며3슬롯 포화 검사에서15메시를 재사용했다. 무피해30회에서 신규 피해·보상·Zone0, 활성3개, 오래된 잔류 교체27회.30회 동기 생성/재작성은48.94ms(평균 약1.63ms/회)였다. GC API가0을 반환했지만 branch 경로 배열 등 이벤트별 managed 할당은 코드에 존재하므로 **무할당 증거로 해석하지 않는다**. 매 프레임 새 Mesh/Material/GameObject는 생성하지 않는다.8개 동시 활성에서 최대40 renderer와 약50,016정점이며 실제 GPU/overdraw/ms·저사양/장시간 테스트는 미측정이다. 촬영 readback 비용을 게임 GPU 성능으로 간주하지 않는다.

실제 Destroy 후 소유Material/Mesh0, 구독0, Play 종료 후에도0을 확인했다. enable/disable3회에서 구독 증감이 정확했고 새 Match마다 scene 소유자가 재생성된다. 파괴 시 child pool도 정리한다. 중복 키는 Match 수명 동안 유지하므로 무한 지속하는 미래 자유 난전에서는 메모리 예산을 별도로 검토해야 한다.

최종 컴파일 오류0, shader message0. 마지막 Editor Console 현재0 error/0 warning/3 log다. **누적 도구 세션 오류0은 아니다.** `console-buffer-final.json`에는 최신 CLI가 구형 Pipeline 명령 `script recompile/recompile_status`를 탐색한 호환 fallback 오류32건, warn30건(Input Manager1·폰트23·기존4F 테스트 경고6), log20건이 남아 있다. CLI는 기존 명령으로 fallback해 실제 컴파일을 마쳤다. 패키지 업그레이드는 하지 않았다. 초기 런타임/테스트 컴파일 실패는 별도 원본에 남는다. 과거 PASS4A18건·이전 폰트/타임아웃 이력은 해결 처리하지 않는다.

종료 상태는 Play stopped/scene clean/컴파일 false/TimeScale1/fps -1/runInBackground false/AutoTick true16ms/PlayUnfocused(2)로 시작 기준과 같다. 포커스 전환·화면 직접 조작·임시 AutoTick 변경 없이 수행했다.

## 사용자 직접 검수

1. `ShibuyaCombatMVP` Play. 실제 VFX 프로필은 Project의 위 Resources asset에서 한국어 Inspector로 조절한다.
2. `JJK Game → 시부야 PASS4G → 공격 등급·반격 검수`에서 현재 플레이어를 연결하고 QA용 흑섬 자격을 허용한다. 기존 추첨 버튼은 이후 공격용 RNG 교체이며 다음 한 타를 예약하는 기능이 아니다.
3. 실제 적중으로 일반/치명/흑섬을 비교하고 숫자OFF에서도 흑섬 본체가 나오는지 본다. 정상 가드·무적·허공에서는 성공 본체가 없어야 한다.
4. `JJK Game → 시부야 PASS4H → 흑섬 VFX 미리보기 (무피해)`로 같은 시드를 반복해 근접/전투 거리/측면/하이앵글의 핵·분기·잔류를 본다. 이 메뉴는 피해·주력·Zone을 만들지 않으며 실제 연동 검사를 대체하지 않는다.
5. 검은 질량이 먼저 읽히는지, 붉은 가지가 케이블처럼 보이지 않는지, 연기가 검은 조각으로 느껴지는지, 다음 전투를 과하게 가리는지 확인한다. 화면 반전이 없는 것은 H0 범위이며 H1에서 연결할 합의다.

## 변경 범위

신규: Profile/Geometry/Visual/Presentation4개 런타임, 한국어 Inspector/무피해 검수창/targeted 테스트3개 Editor 파일, Ink/Light shader2개, Resources profile1개와 필요한 .meta/새 Shaders 폴더 .meta.

기존 수정: Match의 observer 부착1줄, QA runner의 H출력 경로 허용1곳, `.gitignore`의 공유 `_local_refs/` 제외, 이 보고서와 handoff/current task/설계 문서의 최신 상태 기록. 기존 레퍼런스 등록 문서는 이전 로컬 변경을 유지한다. 사용자 `.slnx`는 포함·복구·수정하지 않는다. 기존 전투 계산·카메라·Gojo/F aura·Scene/Prefab/기존 Data Asset/Packages/ProjectSettings/잠금 문서는 수정 대상이 아니다.

정적 검증: 신규10개 asset의 .meta 누락0, 새 GUID 중복0, Resources profile 참조 누락0, `git diff --check` 통과. baseline33,515개에서 파일 유실0. 기존 로그32,169개 중 Unity가 자동 갱신한 AssetImportWorker/shader compiler 로그7개를 제외한 QA/기록은 해시 불변이다. 이7개는 테스트 원본 XML/trace/영상이 아니며 직접 수정하지 않았다. 승인된 수정 경로를 제외한 기존 비로그1,340개는 마지막 해시 비교에서 불변이며 `.slnx`/레퍼런스 원본/잠금값/Gojo/F aura/Scene/Prefab/기존 Data/Packages/ProjectSettings를 포함한다.

최종 Git은 tracked dirty7개(기존 `.slnx` 포함), untracked23개(기존 설계 문서 포함), staged0이다. HEAD/브랜치 그대로이며 원격은 작업 시작 조회에서 같은 SHA였다. 새 Shared `_local_refs/` ignore를 확인했고 영상 추적0이다. QA는 ignored Logs 아래 보존한다. 문서·기술 검증을 마치고 **CODEX VALIDATED(targeted) / USER VERIFIED 대기**로 멈춘다. staging/commit/push와 후속 PASS를 수행하지 않았다.
