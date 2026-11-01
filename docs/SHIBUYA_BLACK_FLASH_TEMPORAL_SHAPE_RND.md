# PASS 4H — Black Flash Temporal × Shape R&D

2026-11-01 / LOCAL UNCOMMITTED / `feat/shibuya-combat-mvp` / HEAD `891064067834bc26f3a1afc6a37b84bb5e1e79c1`.

**기술 CODEX VALIDATED(targeted) / 시각 USER VERIFIED 대기.** BASE/T/S/TS를 Editor 전용으로 제작·촬영했다. 실제 전투 REV1과 저장 프로필은 교체하지 않았다. 최종 targeted 실행은 **3 PASS / 0 FAIL / 0 SKIP**이며 영상은 앞선 정상 촬영을 재사용했다. Frame Debugger의 실제 draw 분석은 성공하지 못했다. 후보의 자동 채택·L 후속 구현·Git staging/commit/push는 하지 않는다.

## 1. 조사한 기존 구조

AGENTS, CURRENT_HANDOFF, CURRENT_TASK, USER_VERIFIED_SETTINGS, BLACK_FLASH_VFX, VFX_DIRECTION, UNITY_VALIDATION, H0/REV1/Visual R&D/Temporal Assessment 보고서를 확인하고 최신 로컬 `ShibuyaBlackFlashGeometry/Visual/Presentation/Profile`, Ink/Light Shader, `Shibuya Black Flash.asset`, 기존 QA/테스트 runner를 재사용했다. 원격 코드가 아닌 미커밋 REV1이 기준이다.

원본 레퍼런스는 `_local_refs/videos/blackflash_refs/로블_흑섬.mp4` / `주팬퍼_흑섬.mp4`이며 직전 독립 분석의 실제 원본 디코드와 native30fps 인접 표본을 재사용했다. Manji 약 1.533~2.167초 / 나나미 약 66.733~67.100초의 긴 획·넓은 파열면 교체·잔류 분리와 비교했다. 원작의 실제 렌더링 기술은 확인되지 않았다.

기존 REV1은 루트 Transform 0→1 확대가 아니라, shader에서 anchor 기준 정점 확대와 root-tip clip 공개를 겹친다. 줄기11개+지류22개=33개 획, 6각 닫힌 단면, 큰 줄기4개다. 주가지 정점 확대 약103ms, 마지막 지류 끝 공개 약142.9ms, 가지 noise clip 가능 시각 약190.82ms, 가지 renderer 수명280ms다. 검정과 붉은 rim/vein은 같은 Ink pass에 있다.

## 2. 사전 예측

구현 전에 `unity/Logs/ShibuyaPass4HTemporalShapeQA/20261101/predictions.md`에 기록했다.

| 조건 | 바꿀 변수 | 예측 | 예상 부작용 |
|---|---|---|---|
| BASE | 없음 | 연결된 가지가 자라고 유지되는 기준 | 기존 나무/거미 다리 인상 |
| T | 성장95→30ms | 길이가 늘어나는 구간 단축 | 같은33개 획이 더 오래 완성 상태로 보여 물체성이 남을 수 있음 |
| S | 가지 geometry의 연결/폭/윤곽 | Y자 접합 감소, 검은 면적 증가 | 두꺼운 칼날/판 인상으로 바뀔 수 있음 |
| TS | T와 S의 조합만 | 더 빨리 넓은 검은 파열이 등장 | T/S 단점도 그대로 남을 수 있음 |

이번에는 L을 구현하지 않는다. 비교 결과와 사용자 검수를 먼저 받는다.

## 3. BASE/T/S/TS의 실제 차이와 통제

모두 원본 프로필을 **메모리에서 복제**하고 기존 `ShibuyaBlackFlashVisual`과 생성기를 사용한다. 공통 QA Ink shader는 원본을 바탕으로 지류 지연 기준과 붉은 표현 진단만 분리했다. 전체 합성에서는 같은 알고리즘/재질 상태다. 별도 화면 반전·굴절·카메라 shake·새 연기·새 flipbook·음향을 추가하지 않았다.

### T — 지류 지연을 고정한 성장 변경

원본 shader는 `birth = uv.w × _Growth`여서 성장95→30ms만 바꾸면 지류의 시작 지연까지 바뀐다. 이번 명세의 ‘기타 시작 지연 유지’를 지키기 위해 QA shader에서 **`birth = uv.w × _BirthGrowth`**로 분리했다.

- BASE/S: `_Growth=.095`, `_BirthGrowth=.095`.
- T/TS: `_Growth=.030`, `_BirthGrowth=.095`.
- 따라서 지류의 기존 절대 지연23.75/47.50ms는 네 후보에서 동일하다. T가 모든 지류를30ms에 완성시키는 실험은 아니다.
- CPU vertex/triangle/UV0/seed 배열은 BASE와 T가 같고, S와 TS가 같다.

### S — 기존 메시에서 파열 획만 변경

핵·접촉광·압력·연기/잔여 방전은 원본과 같다. **BranchingRupture 메시만** 수정한다.

- 굵은4개 획은 기존 경로를 유지하며 넓은 축2.1배, 얇은 축0.75배로 단면을 바꾸고 불규칙 강도0.22를 적용한다.
- 가는 획은 기존 경로의 앞35%를 잘라 새로운 뿌리를 만들고, 폭1.6배의 독립 획/파편으로 재배치한다. 부모의 Y자 접합에서 바로 갈라지는 비중을 줄인다.
- 획33개·정점5,090개·triangle/UV0 시간·seed 데이터는 유지한다. **획 개수를 늘려 밀도를 올리지 않았다.**
- 최외곽 반경을 BASE와 맞추는 geometry fit은 약0.9999958이었다. CPU 최대 반경은 두 형상 모두3.047894m다. 공통 QA parent scale로 이를 명목 지름4m에 맞췄다. 실제 전투의 profile.scale은 건드리지 않았다.
- pixel area는 같은 값으로 강제하지 않았다. ‘범위를 비슷하게 두고 면적/연결 구조를 바꾸는’ 실험이므로 검은 점유 면적의 변화 자체가 관측 대상이다.

S는 하나의 구체적인 형상 수정이다. 모든 형태 기반 접근이나 고품질 작화의 최종 가능성을 대표하지 않는다. 닫힌 단면과 기존4개 주 경로가 남는 한계도 의도적으로 유지했다.

### 원본 BASE 일치 검증

같은 메시·카메라·시점에서 QA shader와 실제 production Ink shader를 교대로 렌더했다. 16.7/50/100/150/200/300ms의 **최대 RGBA 채널 차이0**이었다. 이는 선택한 측면·시드71·시점의 픽셀 비교이며 모든 기기·시점을 전수 보장하는 것은 아니다.

## 4. 동일 조건 촬영

실제 `ShibuyaCombatMVP` Play Mode에서 동일 더미 위치+1m, world+Z 방향, seed71, 명목 지름4m, 카메라 수평 거리6m/높이2m를 사용했다. 각 runtime frame에서 같은 age/카메라로 네 후보를 차례로 렌더하고 다른 후보는 숨겼다. 새로운 실제 공격/피해/Zone/주력 소비는 발생시키지 않았다.

| 구도 | 실제 고유 runtime frame | 평균 간격 | 최대 간격 | 길이 |
|---|---:|---:|---:|---:|
| 정면 | 61 | 16.663ms | 26.491ms | 1초 |
| 측면 | 60 | 16.823ms | 33.595ms | 1초 |
| 45도 | 61 | 16.661ms | 21.578ms | 1초 |
| 회전 | 240 | 16.670ms | 31.071ms | 4초 |

고정 영상은 첫0.1초 뒤에 효과를 시작한다. 회전은 **4초360도에 같은 효과4회**를 재생해 앞·옆·뒤에서 peak가 반복해서 보이게 했다. 이는 하나의 효과를4초 유지하거나 모든 각도에서 같은 age를 동시에 보여주는 방식이 아니다.

60fps 파일은 실제 기록 시각의 유지 시간을 사용해 만들었다. 필요한 프레임 반복은 있으나 광학 보간이나 원본에 없던 움직임 생성은 없다. 0.5배속은 같은 영상의 시간을2배로 늘렸다. 원본 게임 녹화는30fps이며 줌·화면 반전·캐릭터·배경이 달라 동일 조건 품질 benchmark는 아니다.

느린 파생 파일의 최초 인코딩은 말미1프레임이 짧은1.983/7.983초였다. 길이 검사로 발견해 `intermediate-slow/`에 보존하고 마지막 표본의 hold를 보완하여 최종2/8초,120/480프레임으로 맞췄다. raw 촬영은 변경하지 않았다.

## 5. 시간값 — 계산·실제 프레임·GPU 관측 구분

### 코드 계산값

| 항목 | BASE/S | T/TS |
|---|---:|---:|
| 코어 정점 확대 완료 | 95ms | 30ms |
| 주가지 끝 공개 | 95.40ms | 35.60ms |
| 가지 정점 확대 완료 | 103ms | 38ms |
| 첫 지류 끝 공개 | 119.15ms | 59.35ms |
| 마지막 지류 끝 공개 | 142.90ms | 83.10ms |
| 첫 noise clip 가능 — 코어/가지 | 129.49 / 190.82ms | 동일 |
| 코어/가지 renderer OFF | 190 / 280ms | 동일 |
| 잔여 방전/연기 종료 | 445 / 685ms | 동일 |
| 인스턴스 전체 상한 | 750ms | 동일 |

양의 길이 공개는8ms 이후, 첫/둘째 지류의 양의 길이 공개는31.75/55.50ms 이후가 계산상 시작이다. 다만 **f=0 cap은 `clip(0)`을 통과할 수 있어 시작 전 작은 뿌리 픽셀이 보일 수 있다.** 실제 isolated mask에서도0ms에 BASE/T10픽셀, S/TS15픽셀이 있었다. 따라서 ‘최초 픽셀’을 ‘긴 주가지/지류가 실질적으로 나타남’과 같은 것으로 기록하지 않는다. 원본 동작을 고치지 않고 이 제약을 남겼다.

### 실제 runtime sample 시각

측면 영상에서 각 계산 조건을 처음 넘긴 실제 sample age는 BASE 주가지 끝100.08ms / 확대 완료116.65ms / 마지막 지류149.83ms, T 주가지 끝·확대 완료49.65ms / 마지막 지류83.16ms였다. 이는 **실제로 촬영한 프레임의 시각**이며 GPU tip 하나를 추적해 직접 완료를 검출한 값은 아니다. 60fps 근처 촬영도 연속 시간을 정확히 대체하지 못한다.

첫 noise clip의 정확한 실제 픽셀 발생 시각, 개별 지류의 화면상 최초 출현과 표시 획 수는 계측하지 못했다. hash clip·작은 끝의 해상도·가림 때문에 전체 mask만으로 분리할 수 없다. 위 계산값을 해당 항목의 실측 완료로 표시하지 않는다.

### 실제 GPU 출력 — 주 파열 단독 mask

측면,480×360,배경 alpha0,QA layer만 렌더하여 alpha>12/255인 픽셀을 셌다. 캐릭터/배경 가림이 없는 **고정 shader age의 실제 렌더 결과**다. 시간곡선 계산값과 다르며 실제 전투의 최종 가시 면적과도 다르다.

| age | BASE | T | S | TS |
|---:|---:|---:|---:|---:|
| 0ms | 10 | 10 | 15 | 15 |
| 16.7ms | 29 | 119 | 51 | 190 |
| 33.3ms | 101 | 893 | 167 | 1,210 |
| 50ms | 270 | 1,212 | 370 | 1,598 |
| 83.3ms | 843 | 1,261 | 1,145 | 1,634 |
| 100ms | 1,164 | 1,244 | 1,556 | 1,635 |
| 150ms | 1,231 | 1,231 | 1,621 | 1,621 |
| 200ms | 1,189 | 1,189 | 1,551 | 1,551 |
| 300ms | 0 | 0 | 0 | 0 |

150ms의 외곽 bbox는 BASE/T123×98px, S/TS127×98px다. 약31.7% 면적 증가가 외곽 반경을 크게 키운 결과는 아니다. 그러나 이 수치가 폭발감/품질이31.7% 좋아졌다는 뜻은 아니다.

## 6. 레이어별 검수와 렌더 상태

QA 메뉴에서 전체/핵/주 파열/붉은 진단/잔류를 원속도 또는 고정 시각으로 볼 수 있다. 고정150ms 캡처도 네 후보에 남겼다.

- 핵만: 작은 중심과4개 굽힌 lobe가 보인다. 긴 가지 인상의 주요 소유자는 아니다. 닫힌 불투명 표면의 느낌은 남는다.
- 주 파열만: BASE/T는 연결된 뻗음, S/TS는 더 넓은 갈고리형 획과 분리 파편을 확인할 수 있다. 긴4개 주 경로가 남아 고체·거미 다리 인상을 완전히 없애지 못했다.
- 붉은 진단: rim/vein/작은 방전의 shader 성분을 분리한다. **별도 붉은 renderer의 draw가 아니다.** 정상 합성과 다른 alpha/clip을 쓰는 진단 모드다.
- 잔류만: 원본 메시 연기와 작은 방전이 네 후보에서 동일하다. ParticleSystem/새 flipbook으로 바꾸지 않았다.

코드·material에서 확인한 상태는 Ink queue2980, SrcAlpha/OneMinusSrcAlpha, core/branch ZWrite1/CullBack, smoke ZWrite0, Light queue3010/additive/ZWrite0이다. 검은 면과 붉은 테두리는 같은 pass다. 같은 queue의 실제 앞뒤 순서를 Frame Debugger로 확정한 것은 아니다.

## 7. Frame Debugger 실제 시도와 한계

`UnityEditorInternal.FrameDebuggerInternal.FrameDebuggerUtility`를 reflection으로 확인했고 backend는 locallySupported=true였다. 창을 열거나 포커스를 전환하지 않고 실제 `SetEnabled(true,0)`를 시도하고 view repaint/update 후 이벤트를 조회했다. 안전 watchdog과 finally에서 비활성화했다.

**결과: 이벤트0, 읽기 시점 enabled=false. 실제 draw 목록·render target 전후 캡처를 확보하지 못했다.** watchdog 복구가 작동했으며 GUI를 갖춘 정상 Frame Debugger 검수 성공이라고 하지 않는다. 이 시도만으로 D3D12가 지원되지 않는다거나 draw가 없다고 결론내릴 수 없다. 원인도 확정하지 않는다. 원본 기록은 `TS-frame-debugger.json`이다.

따라서 새 렌더링 근거는 Frame Debugger가 아니라 **실제 GPU layer mask와 레이어 단독 캡처**다. RenderDoc 설치·실행·그래픽 API 변경은 하지 않았다.

사용자가 수동 확인할 순서:

1. Play → `JJK Game → 시부야 PASS4H → 흑섬 시간·형상 R&D`.
2. BASE, 시각 고정0.15초, 전체 합성을 선택하고 재생한다. gameplay의 TimeScale은 그대로다.
3. `Window → Analysis → Frame Debugger`에서 해당 Game camera 이벤트를 확인한다. 코어/BranchingRupture/SmokeEmbers/Light의 draw 전후를 비교한다.
4. 검정과 빨강은 같은 draw이므로 이벤트만으로 색을 분리하려 하지 않는다. 색 성분은 QA의 붉은 진단으로, 깊이/정렬은 실제 이벤트에서 확인한다.
5. 0.05/0.10/0.20초와 측면 카메라에서도 반복한다. 단순 QA renderer toggle 이미지를 Frame Debugger 전후 캡처로 혼동하지 않는다.
6. 디버거를 끄고 QA 정리/창 닫기 후 기존 전투를 확인한다.

## 8. 사전 예측과 실제 결과

| 후보 | 관측된 결과 | 예측과 다른 점·한계 |
|---|---|---|
| T | 33~50ms에 훨씬 큰 파열이 보임. 150ms 이후 BASE와 mask/형상이 같음 | 성장 구간 단축은 명확하나 같은 큰 가지가 더 오래 보임. 나무 의미 인식을 제거했다고 할 수 없음 |
| S | peak 면적 증가, 일부 Y자 접합이 끊긴 조각으로 바뀜 | 기존4개 큰 굽은 획과 불투명 단면 때문에 갈고리/칼날 느낌이 남음. 형태 실험의 효과는 제한적 |
| TS | 초반에 S의 더 넓은 형태가 빠르게 드러남 | peak 이후 S와 동일. 두 개선이 합쳐졌다는 것은 확인되지만 최종 품질의 유의미한 우위는 미확정 |

이번 판단은 실제 runtime 영상의 시간순 프레임·0.05/0.15초 동일 조건 이미지·GPU 출력 대조에 근거한다. 사람이 원속도를 반복 시청한 것과 동등한 지각 평가, 통계적 유의성, 독립 블라인드 심사라고 표현하지 않는다. 원속도에서 어느 후보가 ‘주력 파열’로 읽히는지는 사용자 검수가 필요하다.

레퍼런스는 큰 면의 추가/해체와 긴 획·작은 방전이 섞인다. 이번 S는 연결부/단면만 바꿔 이 작화 교체까지 재현하지 않았다. 기존 조형과 높은 alpha/rim이 살아 있어 ‘나무가 빠르게 자라는 효과’에서 완전히 벗어났다는 성공 선언을 하지 않는다.

## 9. 질문별 결론과 다음 추천

| 질문 | 답 |
|---|---|
| Q1 시간만 바꾸면 줄어드는가? | 성장하는 구간은 수치·영상에서 줄었다. 완성 형상의 물체성은 남아 ‘나무 인식 해결’은 아님 |
| Q2 형상만의 차이는? | 크기 범위를 유지하면서 검은 면적/불규칙성 증가와 지류 연결 감소. 고체/갈고리 인상은 남음 |
| Q3 결합이 더 좋은가? | 초반의 빠른 면적 형성은 확인. 150ms 이후 TS=S여서 자동 최종 우위를 주장할 근거 부족 |
| Q4 불투명 고체 질감은? | 여전히 문제 후보. 같은 높은 alpha, 닫힌 단면, 오래 유지되는 주 경로와 rim이 남아 있음 |
| Q5 Frame Debugger의 새 근거는? | 없음. 실제 draw 확보 실패를 명시하며 GPU layer 출력과 구분 |
| Q6 다음 우선순위는? | 사용자 영상 평가 후 **큰 검은 본체의 윤곽 해체·부분 소거 시점과 붉은 표현을 분리하는 제한적 shader/layer-time 실험**을 우선 제안. 새 flipbook 전체 재설계는 보류 |

단순히 alpha를 내려 검은 주력의 존재감을 잃는 방향은 권하지 않는다. 큰 본체의 면적을 살린 채 같은 연결 실루엣이 오래 지속되는 구간을 어떻게 해체할지가 다음 질문이다. L이나 shader 수정은 이번에 자동 진행하지 않았다. 3D 자체를 실패로 확정하거나 TS를 본게임 기본값으로 채택하지 않는다.

## 10. 실제 영상과 이미지

QA root: `unity/Logs/ShibuyaPass4HTemporalShapeQA/20261101/`.

최종 재생 자료는 **`videos/`**다. 검증 결과27개 MP4와 비교 이미지13개가 있다.

- `comparison-side-1x.mp4`: 네 후보 측면 원속도 — 먼저 볼 자료.
- `comparison-front-1x.mp4`, `comparison-diagonal-1x.mp4`, `comparison-orbit-1x.mp4`.
- 각 `comparison-…-0.5x.mp4`: 같은 자료의 느린 비교.
- `BASE-vs-T-side-1x.mp4`, `BASE-vs-S-side-1x.mp4`, `BASE-vs-TS-side-1x.mp4`.
- `front/side/diagonal/orbit-0/1/2/3-1x.mp4`: 후보별 원본 구도 영상16개.
- `phase-0.0167/0.0333/0.0500/0.0833/0.1000/0.1500/0.2000/0.3000.png`: 동일 age 네 후보.
- `layers-0/1/2/3.png`: 후보별 전체/핵/주 파열/붉은 진단/잔류.
- `side-time-sequence.png`: 원속도 측면 영상의0.1~0.5초를30Hz로 나열한12열×4행. 정적 보조자료이며 시간 비교의 우선 자료는 MP4다.
- `video-verification.json`: 실제 파일·해상도·FPS·길이·프레임 수. 고정1x1초/60프레임, orbit1x4초/240프레임, 느린 비교는 각각2/8초다.

원시 촬영 PNG와 실제 frame/time은 `capture-native/legacy/ShibuyaPass4B3QA/TS-video/` 및 `TS-timing-*.json`이다. 고정 시각·단독 GPU mask·레이어 캡처는 `evidence-final/legacy/ShibuyaPass4B3QA/TS-stills/TS-gpu-masks/TS-layers` 아래다. 마지막 targeted 실행도 별도 결과를 보존했다.

## 11. 기술 검증과 실패 이력

| 실행 | 결과 | 해석 |
|---|---|---|
| initial | 2 PASS / 1 FAIL | 촬영56.61ms gap으로55ms 미만 기준 실패. 초기 PASS만으로 최종 완료 판단하지 않음 |
| capture-recheck | 0 PASS / 1 FAIL | 같은 기대값 재확인에서도58.33ms gap. 반복 재시도를 중단하고 촬영 구현 조사 |
| capture-native | 0 PASS / 1 FAIL | native buffer로 프레임별 managed readback 배열 할당 제거. 네 구도 촬영은 기준 통과했지만 뒤의 Vector3 JSON 직렬화가 실패 |
| evidence-final | 1 PASS | 촬영 원본 재사용 검증 후 telemetry/layer/FD 시도 완료 |
| final-targeted | **단일3 PASS / 0 FAIL / 0 SKIP** | 최종 완료 sentinel 포함한 변수 통제/픽셀 parity, 남은 증거,12회 정리 검사. 기존 촬영을 검증하여 재사용 |

최종3 PASS는5회 통과 수를 합산한 값이 아니다. 다만 **모든 최종 영상이 마지막 실행에서 새로 촬영됐다는 뜻도 아니다.** `TS-reused-capture.json`에 원본 출처와 검증 내용을 남겼다.

최초 촬영은4개 동시 readback에서 초당 약166MB의 managed byte 배열을 만들었다. 이를 촬영 전 확보한 native storage로 바꾸고 PNG 변환을 촬영 후로 유지했다. 이후 간격은 개선됐지만 이 비교만으로 두 최초 지연의 유일 원인이 GC였다고 확정하지 않는다. 기대값55ms는 낮추지 않았다.

계측 저장은 Vector3를 float 배열로 바꾸어 순환 참조를 제거했다. 이전 XML의 PASS 표기와 달리 factorial 증거 파일이 없던 점도 확인해 최종 도달 sentinel을 추가하고 세 targeted 검사를 다시 수행했다. 최종 parity 기록까지 존재하는 것을 확인했다.

중간 컴파일에서 JToken 확장 메서드 사용 오류 CS7036 두 건이 있었고 수정 후 컴파일 정상이다. Console 누적에 이 오류2건과 기존4F·폰트·InputManager 경고를 보존했다. 마지막 Editor ground truth 오류0/경고0, 새 QA shader message0이며 Console을 수동 삭제하지 않았다. 기존 대규모 전투 회귀나 PASS4A 과거18건을 해결 처리하지 않았다.

## 12. 리소스·상태

- 12회 후보 생성/모드 변경/해제 검사 후 소유 material/profile/root0.
- 종료 후 실제 `BlackFlash pooled` mesh0을 별도 조회했다.
- HP/주력/production VFX 수락 수/피해 observer 수 불변. 실제 피해 이벤트와 gameplay 계산 순서는 건드리지 않았다.
- 종료 시 Play stopped, scene clean, compiling false, TimeScale1, fps -1, runInBackground false, AutoTick true/16ms, Game View PlayUnfocused, Frame Debugger OFF. 임시 촬영 재사용 SessionState도 제거했다.
- native 촬영 buffer는 view 종료 시 dispose한다. GPU ms/장시간 누적/저사양 overdraw benchmark는 수행하지 않았다. 촬영·readback 비용을 본게임 VFX 성능으로 보고하지 않는다.

## 13. 추가 파일과 한국어 설정

기존 파일을 수정하지 않고 아래5개와 `.meta`5개를 추가했다.

1. `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShapeProfile.cs`: Editor 전용 설정과 SerializedProperty 한국어 CustomEditor.
2. `unity/Assets/Shibuya/Editor/Black Flash Temporal Shape QA.asset`: 성장시간0.03/주획 너비2.1/시작 절단0.35/파편 너비1.6/불규칙0.22.
3. `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShape.shader`: QA 지연 분리와 붉은 표현 진단.
4. `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShapePreview.cs`: BASE/T/S/TS·레이어·고정 age 검수창. 실제 전투에 Attach하지 않음.
5. `unity/Assets/Shibuya/Editor/ShibuyaPass4HTemporalShapeTests.cs`: targeted QA와 캡처·상태 기록.

이 보고서까지 **비무시 신규11개**다. 원본 runtime Profile/Geometry/Visual/Ink/Presentation/Match는 변경하지 않았다. 기존 R&D/REV1/평가 보고서와 handoff/task도 덮어쓰지 않았다. QA 원본과 인코딩 스크립트는 ignored Logs에 별도로 둔다.

## 14. 보호 확인

시작 baseline의 기존 소스·문서·자산 **1,377개 SHA-256 불변, 유실0**. 기존 관련 QA **5,127개 경로의 크기/mtime 불변**이다. QA 원본 검사는 이번에 새로 전수 SHA를 계산한 것과 다르다. 상세 `source-baseline.json`, `old-qa-inventory.json`, `preservation.json`.

실제 REV1과 저장값, F 아우라, 피해/치명/흑섬/Zone/반격/숫자, 카메라, Gojo, scene/prefab/Packages/ProjectSettings/기존 GUID, 사용자 `.slnx`를 보존했다. 기존 미커밋 작업을 초기화·이동·삭제하지 않았다.

## 15. Git

브랜치/HEAD 동일. 시작 staged0 / tracked dirty7 / untracked34. 이 작업의 신규11개가 더해져 최종 **staged0 / tracked dirty7 / untracked45**다. 기존 tracked 변경은 모두 시작 전부터 있던 내용이다. staging/commit/push/merge/reset/clean/checkout/worktree 생성은 수행하지 않았다.

## 16. 사용자 직접 검수

1. `videos/comparison-side-1x.mp4`를 먼저 보고 T가 ‘파열’인지 ‘더 빠른 나무 성장’인지 판단한다. 다음 S의 큰 갈고리/면이 나아졌는지 본다.
2. 45도/정면/회전 원속도를 본다. 후면 peak는 회전의 후속 반복에서 확인한다. 필요할 때만0.5배속과 phase 이미지를 본다.
3. Unity에서 Play → **JJK Game → 시부야 PASS4H → 흑섬 시간·형상 R&D**. 후보를 선택하고 `동일 시드71로 재생`을 누른다. `반복 재생`과 `레이어 표시`를 바꿔 볼 수 있다.
4. `시각 고정(검수용)`으로0.05/0.15/0.20초를 비교한다. 이는 QA VFX의 age만 고정하며 실제 전투·TimeScale을 정지시키지 않는다.
5. 창의 `실험 설정` Asset에서 한국어 Inspector 수치를 조절할 수 있다. 이번 결과를 재현하려면 위 초기값을 사용한다. 색/본게임 수치 설정이 아니다.
6. 창 닫기/`임시 자원 정리`/Play 종료로 QA를 정리한다. QA 메뉴를 사용하지 않는 실제 전투는 계속 기존 REV1이다.

시각적 최종안은 **USER VERIFIED 대기**로 남긴다. 이 보고 후 L·새 셰이더 방향·flipbook 재설계 등 후속 구현을 시작하지 않는다.
