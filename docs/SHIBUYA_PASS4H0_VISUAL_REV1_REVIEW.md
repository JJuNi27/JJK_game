# PASS 4H.0 시각 개선 REV1 — 방향성 있는 주력 파열

2026-11-01 / `feat/shibuya-combat-mvp` / HEAD `891064067834bc26f3a1afc6a37b84bb5e1e79c1`.

**LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기.** 기술 검증과 시각 합격을 구분한다. 측면의 방향성·성장·잔류 분리는 개선됐으나 정면에서는 공격자가 중심을 가리고, 큰 가지는 아직 각진 검은 조각처럼 읽힐 수 있다. 레퍼런스 수준의 최종 품질이나 모든 시각 목표 달성을 선언하지 않는다. H1 화면 반전/카메라/새 히트스톱은 구현하지 않았다.

## 1. 시작 상태와 실제 원인

기존 H0의 미커밋 구현과 문서를 이어서 사용했다. 시작 Git은 tracked dirty7/untracked23/staged0. `.slnx`는 기존 사용자 변경이다. Unity가 사용자 Play Mode였으므로 사용자에게 종료를 요청했고, 종료 응답 후 Play stopped/scene clean을 확인한 다음 수정했다. AutoTick true/16ms, fps -1, TimeScale1, runInBackground false, PlayUnfocused2가 기준이다.

기존 Geometry는 핵의 닫힌 질량6개가 겹치고, 주가지11개와 분기가 중심 주변을 촘촘하게 둘렀다. Visual은 완성된 가지 전체를 .3→1로 확대했다. Ink의 붉은 Fresnel이 형태의 테두리로 먼저 읽혔고, 연기와 잔여 방전은 같은 Transform의 확대·상승을 공유했다. 이 조합이 검붉은 가시 공/단단한 물체라는 인상을 만들었다.

로블 Manji1.633초와 주팬퍼 나나미66.900초 원본 프레임을 다시 추출하고, 기존 인접 프레임 시트 및 사용자 `흑섬 1차 영상.mp4` 분석을 재사용했다. 참고 영상에는 크고 작은 검붉은 가지의 차이, 접촉 후 빠른 실루엣 변화, 본체 이후의 방전·연기가 보인다. 참고 영상의 카메라/화면 반전/배경과 현재 블록아웃은 다르므로 같은 구도·조건의 품질 비교라고 주장하지 않는다.

## 2. 외부 개발 자료와 적용 판단

| 자료 | 직접 확인한 내용 | 이번 적용 |
|---|---|---|
|[VFX Mesh Lab](https://github.com/PudinKiller/VFXMeshLab)|Unity6 URP **Editor** 메시 저작 도구. Arc/Ribbon, taper/twist/bend/noise, UV·vertex data 및 일반 Mesh 출력. 런타임 애니메이션이 자동 제공되는 도구는 아님|비대칭 곡면·빈 공간·메시 데이터 저작 원칙만 참고. 현재 절차적 메시가 있어 설치 이득이 작음|
|[VFX Essential Shader](https://github.com/excifroge/VFX-Essential-Shader)|Mask/dissolve/UV distortion/vertex offset/rim 모듈. README는 shader_feature variant 수와 성능 최적화가 보장되지 않는다고 명시|자체 Ink shader의 정점 변형·부분 소거·발광 선으로 해결. 범용 shader/keyword 집합 도입 안 함|
|[Lightning Bolt System](https://github.com/sonereraslan/unity-lightning-system)|Generator의 경로/분기 구조와 Animator의 Progression→Peak→Regression, pooled LineRenderer 구조 확인|주·보조 분기와 성장/소멸의 분리만 참고. 독립적인 GPU 변형 구현. LineRenderer/Point Light/Camera Shake 이식 안 함|

세 저장소의 실제 [Mesh Lab MIT](https://github.com/PudinKiller/VFXMeshLab/blob/main/LICENSE.md), [Essential MIT](https://github.com/excifroge/VFX-Essential-Shader/blob/main/LICENSE.md), [Lightning MIT](https://github.com/sonereraslan/unity-lightning-system/blob/main/LICENSE) 문서를 확인했다. **외부 소스 파일·코드·자산을 복사하거나 수정해 포함하지 않았다.** 패키지 설치/Packages 변경도 없다. 위 링크는 기술 참고 출처이며, 프로젝트 전체를 통합했다는 뜻이 아니다.

## 3. 메시와 시간 구조

기존 5개 pooled mesh와 scene 소유3재질, 피해 observer를 그대로 유지했다. 전체 크기1/가지 길이2.25/기존 프로필 값은 보존했다.

- **접촉광:** 굵은 쐐기를 가늘고 긴 입체 접촉광으로 수정했다. 기존 .045초/강도5 유지.
- **핵:** 작은 압축핵과4개의 휘어진 파열면으로 구성한다. 닫힌 구6개의 겹침을 제거하고 파열면 사이에 빈 공간을 둔다. 새 중심 밀도 .42는 검정을 투명하게만 만드는 값이 아니라 중심 Geometry 비율이다. 핵의 기존 .19초 수명과 팽창1.65 유지.
- **주가지:** 기존 줄기 예산11개 중4개를 굵은 주가지, 나머지를 작은 방전으로 구분한다. 주가지는 전방으로 기울면서 횡방향으로도 퍼진다. 3D 단면·깊이를 유지하고, 무작정 전체 크기를 키우지 않았다.
- **성장:** UV0에 길이 좌표/종류/줄기 시드/분기 지연, UV1에 root anchor를 저장한다. GPU에서 가지 뿌리부터 순차 공개하고, 위치별 굽힘·밀림·실루엣 변형을 적용한다. 핵도 파열 단계에 따라 위치별로 벌어지고 소거된다. Transform 전체 확대만으로 단계를 표현하지 않는다.
- **작은 번개:** .045초 간격으로 작은 분기의 위치·형태·표시 조합을 바꾼다. 주가지의 부드러운 움직임과 다른 cadence를 사용한다. 검은 가지 표면의 붉은 선과 작은 독립 방전이 함께 보이게 했다.
- **잔류:** .065초 이후 smoke/잔여 방전이 시작한다. 연기마다 크기·속도·밀도를 달리하고 앞/바깥 방향 이동 후 상승시킨다. 잔여 방전은 .38초 후 종료하고 연기는 기존 .62초 동안 유지한다. 따라서 절대 시각 약 .445초까지 방전, 약 .685초까지 연기가 남을 수 있다(전체 수명 .75초 상한).
- **압력:** 기존 불완전3D 호를 재사용하고 폭·굴곡·불균일성을 조정했다. 실제 화면 굴절 구현은 아니다.

메시 topology/경로 scratch/List는 pooled instance에 보존한다. 매 프레임 Mesh/Material/경로 배열을 재생성하지 않는다. seed별 Geometry는 재생 시작 시 작성하고 매 프레임 변형은 shader가 수행한다. Shader keyword/variant 축은 추가하지 않았다. 변형 후 법선은 원래 법선의 근사이므로 가까운 rim 품질에는 한계가 있다.

## 4. 방향과 전투 연결

`ShibuyaCombatDamageResult`에는 WorldPoint는 있으나 실제 타격 limb 축/접촉 법선은 없다. 기존 H0의 **공격자 가슴 근처→WorldPoint**, 길이가 거의0이면 attacker.forward/world.forward fallback을 그대로 사용한다. 이 방향을 수락 순간 고정하고 local +Z를 파열 전방으로 삼는다. 카메라 기준으로 본체를 이동시키거나 정밀 타격 방향이라고 설명하지 않는다.

**Presentation/Match/피해 이벤트 코드는 이번 REV1에서 변경하지 않았다.** 실제 Applied+BlackFlash+유한한 양수 HP 감소만 재생한다. action/event/target 중복 제거, 숫자OFF 독립, 마지막 사망 타격, 가드/회피/Health 무적/IDamageGuard 차단 경로를 유지했다. 카메라·확률·피해·주력·Zone·패링·반격·콤보를 바꾸지 않았다. 실제 Gojo 무하한 씬 실행은 이번에도 수행하지 않았으며 IDamageGuard 대역 검증과 구분한다.

## 5. 한국어 Inspector / 저장 데이터

기존 사용자 값은 모두 유지하고 다음7개만 추가했다.

| 한국어 항목 | 필드 | 초기값 |
|---|---|---:|
|굵은 파열 주가지 수|primaryBranchCount|4|
|검은 핵의 중심 밀도|coreDensity|.42|
|파열 가지 성장 시간|ruptureGrowthTime|.095초|
|파열 실루엣 변화 강도|silhouetteChange|.32|
|작은 방전 교체 간격|dischargeInterval|.045초|
|잔여 방전 시간|residualDischargeTime|.38초|
|잔류 연기 분산 강도|smokeSpread|1.8|

기존 branchCount11은 전체 줄기 예산이고 primaryBranchCount4는 그 안의 굵은 줄기 수다. 혼동 방지를 위해 한국어 label을 `전체 줄기 수 (주·보조)`로 명시했다. 기존 SerializedProperty/Undo/다중 선택/참조 편집을 유지했다. 구 H0 Profile/Shader/Geometry/Visual/Inspector/데이터와 문서 사본은 아래 QA root의 `before/`에 보관한다. 별도의 영구 중복 프로필은 만들지 않았다.

## 6. 검증 결과와 실패 이력

QA root: `unity/Logs/ShibuyaPass4H0REV1QA/20261101/`. 이전 H0 QA는 덮어쓰지 않았다.

| 실행 경로 | 결과 | 설명 |
|---|---|---|
|before-capture|2 PASS|수정 전 같은 시드/나이 다각도·실제 피해 영상|
|first-shape|2 PASS / 1 FAIL|잔여 방전 끝 삼각형의 cross.sqrMagnitude 8.842e-17로 기존 >1e-16 기준 미달|
|final-targeted|단일14 PASS / 0 FAIL / 0 SKIP|끝단 최소 반경 .001로 실제 Geometry 보완. 기존 기대값 유지. 신규3 + H0기존8 + 선택 기존 회귀3|
|capture-final|2 PASS|게임 프레임당1회 녹화 수집, Inspector label 확인|
|front-coverage-final|5 PASS|정면 가림에 대한 횡방향/핵 파열면 보완 후 데이터2·캡처3만 재실행|

**최종 최신 고유14 PASS / 0 FAIL / 0 SKIP는 마지막3개 실행의 케이스별 최신 결과다.** 중간14개 단일 실행 후 Geometry 보완이 있었으므로 이를 최종 전체 단일 실행 성공이라고 하지 않는다. fullname별 최신 XML 출처는 `latest-unique-results.json`에 기록했다. 최초 실패 XML/trace를 보존했다. 전체 과거 전투 회귀를 다시 돌리지 않았고 PASS4A 과거18건을 해결 처리하지 않는다.

실제 피해 결과8개(일반20/치명30/흑섬50/Zone 이후 흑섬55×5), 흑섬6개에 VFX수락6개. 같은 결과 재전달 추가0. 숫자OFF 독립, Guarded/Health 무적/회피/특수 방어/허공 출력0, 마지막1 HP 사망 타격 출력1을 확인했다. 기존 반격 흑섬·홀드 반복 방지, 혼합4타 숫자 전달, F 아우라 현재 주력량 독립·본인 HUD 검사도 통과했다. 신규 검사는 actual primary vertex의 +Z 편향·3D 분포, 시드 재현성과 Unity gameplay RNG 비간섭, GPU용 데이터 유효성, 120회 Sample 후 CPU Mesh ID/vertices 보존을 확인한다. 실제 GPU 변화는 캡처와 함께 판단하며 CPU 불변만으로 시각 품질을 증명하지 않는다.

녹화 도구의 최초 async 수집은 Editor test coroutine이 같은 runtime frame에서 여러 번 재개되어351장/고유 시각59개였다. 이는 공격 중복이 아니다. 원본을 보존하고 Time.frameCount가 달라졌을 때만 수집하도록 QA 도구를 수정했다. 원본 테스트 기대값이나 게임 시간을 바꾸지 않았다. 잘못된 셸 작업 경로로 새 QA 파일 작성1회가 실패했으며 기존 소스는 영향 없었고 올바른 경로로 작성했다.

## 7. 실제 Play Mode 이미지 / 원속도 영상

최종 근거는 위 QA root의 **`evidence-final/`**이다. 중간 `evidence/`와 기존 실패/촬영도 보존한다.

- `before-after-same-seed.png`: 같은 시드71/전투 거리/포즈/좌표, 나이 .02/.09/.21/.43초, 상단 H0·하단 REV1.
- `side-before-after.png`: 같은 조건의 측면 .09/.21초.
- `six-phases-side.png`: .02/.05/.09/.14/.32/.43초. 접촉→형성→성장/peak→중심 소거→잔여 방전·연기.
- `rev1-angles.png`: 전투 정면/근접/측면/45도/하이앵글 peak.
- `references-and-rev1-v2.png`: 실제 원본 Manji1.633초·나나미66.900초·REV1 측면 .09초. 다른 게임/카메라/Presentation 조건임을 전제한다.
- `before-after-actual-original-speed.mp4`: 실제 일반/치명/흑섬 검사 중 흑섬 구간의 수정 전후 병렬 영상. 각각 별도 실제 실행의 피해 입력이라 event seed·최초 피해 프레임은 완전히 같지 않다.
- `rev1-moving-original-speed-60fps.mp4`, `rev1-moving-original-speed-30fps.mp4`: 동일 시드 무피해 preview, 원속도 카메라 회전. 실제 피해 경로 검증 영상과 구분한다.

최종 async는1.05초 동안 **서로 다른 runtime frame63개**, 평균 간격16.784ms/중앙16.671ms/최대29.708ms다. 약59.6fps의 실측 표본이며 고정60fps 캡처 보장은 아니다. 실제 unscaled 시각으로 프레임 유지 시간을 작성해60/30fps 파일로 내보냈다. 보간/슬로모션/TimeScale 변경은 없다. 이전 방식의 실제 피해 영상은 수정 전36장/후39장이며 최대 표본 간격 약62.7/59.6ms다. 카메라 RenderRequest 출력이므로 IMGUI 숫자/HUD와 오디오는 영상에 포함되지 않는다. 원본 PNG·timestamps·gameFrames·ffconcat·capture-metrics.json을 보존한다.

## 8. 성능 / Console / 환경

기본1개는 **5,090정점/5메시**, scene당3공유재질이다(구 H0 6,252정점). 기본8개 활성 상한에서40렌더러/약40,720정점이다. 30회 preview 포화 검사는3슬롯/15메시/잔류 교체27회, 피해/주력/Zone 변경0. 30회 생성·작성 측정45.47ms, 평균 약1.52ms/회이며 통제된 GPU benchmark는 아니다. GC API0 반환은 무할당 증거가 아니며 재생 시작 시 Random 등 managed 할당은 존재한다. 매 프레임 Geometry/Material 할당 경로는 없다. 실제 GPU ms/overdraw/저사양/장시간 다수 흑섬은 미측정이다.

종료 후 소유 Mesh0/Material0/observer0. C# 컴파일 오류0, 두 shader message0. 최종 Editor ground truth는 오류0/경고1이며 마지막 경고는 기존 `Deleting invalid font reference.`다. **누적 세션 오류0이라고 보고하지 않는다.** `console-buffer-complete.json`/`console-classification.json`에는 구 H0부터 누적된 도구 오류61건(script recompile10/recompile_status50/editor status1)이 남아 있다. 최신 CLI의 구형 Pipeline 명령 fallback이며 실제 컴파일은 완료됐다. 누적 경고67건에는 기존 InputManager1/폰트49/기존4F 테스트15/혼합 Shader 줄바꿈2가 포함된다. Shader 줄바꿈은 LF로 통일 후 재임포트하여 shader message0을 확인했다. 패키지 업그레이드나 Console 수동 삭제는 하지 않았다. 잘못된 최상위 CLI console 명령 응답도 별도 JSON에 보존한다.

AutoTick/PlayUnfocused는 변경하지 않았고 fps는 fixture가60으로 설정했다가 -1로 복구했다. TimeScale1/runInBackground false, Play stopped, scene clean. Computer Use나 포커스 강제 전환은 사용하지 않았다.

## 9. 변경 파일 / 보호

이번 REV1에서 기존 H0 대비 수정한 파일:

- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashGeometry.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashVisual.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashProfile.cs`
- `unity/Assets/Shibuya/Shaders/ShibuyaBlackFlashInk.shader`
- `unity/Assets/Shibuya/Resources/Shibuya Black Flash.asset`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashInspector.cs`

신규: `unity/Assets/Shibuya/Editor/ShibuyaPass4H0Rev1Tests.cs`와 `.meta`, 이 보고서. Handoff/Current Task에는 최신 상태를 덧붙이고 기존 이력을 보존한다. 이전 H0의 Match/QA runner/.gitignore/레퍼런스 등록/설계 문서는 기존 로컬 상태 그대로다. 기존 메타 GUID와 Profile의 기존 필드 값도 유지한다.

시작 baseline은 tracked+nonignored1,366개 파일의 경로/크기/SHA-256이다. `baseline.json`과 `before/` 원본 사본을 보존한다. 최종 비교는 기존8개 승인 경로만 변경/나머지1,358개 불변/유실0/예상 밖 변경0, 기존 Profile 필드 변경0, 신규 test GUID 중복0이다. `.slnx`, 잠금값, 전투 계산·카메라·Gojo/F aura, Scene/Prefab/Packages/ProjectSettings는 해시 불변이다. 로컬 레퍼런스 원본과 QA 원본을 수정·삭제·덮어쓰지 않았다.

REV1 자체는 기존6개 렌더/프로필 파일 수정, 테스트+.meta 신규2개, 보고서 신규1개, handoff/current task 수정2개로 총11개 경로다. **최종 Git 전체는 기존 H0까지 포함해 tracked dirty7/untracked26/staged0**이고 브랜치/HEAD는 시작과 같다. `git diff --check` 통과. 상세 경로·해시 검증은 `preservation-final.json`을 따른다. staging/commit/push는 없다.

## 10. 남은 시각 한계 / 사용자 검수

측면에서는 성게형 덩어리보다 전방으로 뻗는 파열과 작은 방전의 차이가 잘 보인다. 중심이 사라진 뒤 연기가 독립적으로 퍼지는 것도 확인했다. 그러나 **정면에서는 공격자 가림이 여전히 크고, 구 H0보다 검은 면의 화면 점유가 작게 느껴질 수 있다.** 횡방향과 접촉부 파열면을 보완했지만 완전히 해결했다고 하지 않는다. 큰 가지의 기하학적 면, 얇은 분기의 철사 인상, 개별 연기 질량의 둥근 경계도 일부 남는다. 압력은 실제 굴절이 아니며 배경·화면 반전·음향이 합쳐진 레퍼런스의 전체 충격과 차이가 있다. H1을 추가하면 자동으로 이 형태 문제가 해결된다고 가정하지 않는다.

Play → **JJK Game/시부야 PASS4H/흑섬 VFX 미리보기 (무피해)**에서 seed71로 정면/측면/근접/회전 중 비교한다. 실제 피해는 **JJK Game/시부야 PASS4G/공격 등급·반격 검수**에서 QA 흑섬 자격을 허용한 후 LMB로 확인한다. RNG 교체는 다음1타 확정 예약 기능이 아니다. Resources의 `Shibuya Black Flash.asset`에서 한국어 설정을 편집할 수 있다.

사용자는 검은 핵이 다시 공처럼 뭉치는지, 주가지와 작은 방전이 구별되는지, 원속도에서 파열·잔류가 읽히는지, 특히 정면의 타격 존재감이 충분한지 확인해야 한다. 최종 시각 판정은 USER VERIFIED 대기다. staging/commit/push 및 후속 PASS는 수행하지 않는다.
