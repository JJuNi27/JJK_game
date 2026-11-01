# PASS 4H — Art-Directed Black Flash L2/L3 시제품

**LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / 시각 USER VERIFIED 대기.**

브랜치 `feat/shibuya-combat-mvp`, HEAD `891064067834bc26f3a1afc6a37b84bb5e1e79c1`.

기존 실제 전투 REV1을 보존하고 **Editor QA 전용 L2 검은 본체 + L3 붉은 외곽**을 새로 만들었다. 가지 성장 알고리즘과33획을 새 본체에 사용하지 않았다. 새 원화4장의 교체, 고정 곡면2장과 작은 중심 볼륨, 실제 알파 경계에 연결된 별도 붉은 발광을 사용한다. L0/L1/L4/L5/L6·전투 이벤트·카메라 효과는 추가하지 않았다.

최종 `validated/results.xml`은 **단일 실행3 PASS / 0 FAIL / 0 SKIP**다. 해당 실행에서 최종 재생 순서의 영상도 새로 촬영했다. 중간 실패와 이전 출력은 보존했다. 이 수치는 최종 시각 합격을 뜻하지 않는다.

## 1. 실제 시각 목표 확인

예상 경로 `_local_refs/images/blackflash/BLACK_FLASH_VISUAL_TARGET.png`에는 파일이 없었다. 사용자 안내 후 **`_local_refs/videos/blackflash_refs/흑섬_레이어.png`**를 찾아 직접 확인했다. 참조 이미지 원본을 Unity Asset으로 복사하거나 패널/배경을 잘라 텍스처로 사용하지 않았다.

이미지에서 읽은 핵심:

- L2는 넓고 불규칙한 검은 면이 주역이고 일부 긴 날카로운 획이 비대칭으로 뻗는다. 같은 폭의 가는 가지 묶음과 다르다.
- L3는 검은 형상에 **붙은 붉은 외곽**이 중요하다. 전체가 균일한 네온 선이 되는 대신 부분 강조와 독립 방전이 섞인다.
- 검은 질량과 붉은 발광은 렌더링 역할을 분리한다. 검은색까지 additive로 만들지 않는다.
- 그림의 흰 섬광·압력·연기·배경·캐릭터는 이번 구현 범위가 아니다.

`로블_흑섬.mp4`, `주팬퍼_흑섬.mp4`의 기존 실제 디코드/인접 프레임 분석도 재사용했다. Manji 약1.533~2.167초, 나나미 약66.733~67.100초에서 보인 짧은 윤곽 유지, 큰 면의 추가/교체와 후반 해체를 시간 구성의 참고로 삼았다. 원본30fps 녹화나 정적 인포그래픽의 시간표를 엔진의 확정 정답으로 취급하지 않았다.

AGENTS, CURRENT_HANDOFF, CURRENT_TASK, USER_VERIFIED_SETTINGS, BLACK_FLASH_VFX, Visual R&D, Temporal Assessment, Temporal×Shape R&D와 UNITY_VALIDATION을 읽고 현재 로컬 구현을 확인했다.

## 2. 새 키 포즈 원화 제작

`imagegen`으로 **새로운 투명 RGBA 키 포즈 아틀라스**를 제작했다. 생성형 이미지이며 사람이 손으로 그렸다고 설명하지 않는다. 참조 이미지의 캐릭터/배경/문자를 복제하지 않았다.

첫 생성은 네 방향 대칭이 강해 별/나비처럼 읽힐 가능성이 있었다. 한 번의 방향 수정으로 왼쪽의 무거운 검은 면과 서로 길이가 다른 대각 획, 큰 빈틈과 비대칭 조각을 강화한 두 번째 원화를 선택했다. 두 생성본 모두 `generated-source/`에 남겼다. 새 원화 자체의 USER VERIFIED 승인은 아직 없다.

선택한 원화는 **1254×1254,2×2 배열,셀627×627**이다. 직접 알파를 확인했고 가짜 체크무늬나 배경을 검정으로 마스킹한 이미지가 아니다. 색상 대신 실제 알파를 이용해 검은 본체를 그린다.

재생 순서는 원화 셀 **1→0→2→3**으로 선택했다. 더 큰 검은 질량을 가진 원화를 두 번째 peak에 배치하기 위한 편집이며 Data Asset에서 조절 가능하다.

| 재생 포즈 | 원화 셀 | alpha>200/255 픽셀 | alpha<5/255 픽셀 | 의미 |
|---|---:|---:|---:|---|
| 최초 파열 | 1 | 52,159 | 327,464 | 이미 넓게 나타나는 비대칭 검은 파열 |
| 최대 파열 | 0 | 58,386 | 320,220 | 더 큰 질량과 긴 대각 획 |
| 윤곽 변화 | 2 | 55,497 | 322,228 | 중심이 찢어지고 큰 면이 분리됨 |
| 해체 | 3 | 30,117 | 351,187 | 중심이 비고 떨어진 획/조각만 남음 |

이는 원화 셀의 실제 픽셀 측정이다. 게임 화면의 가림이나 투영 면적과 같지 않다. 해체 포즈는 peak의 약51.6% 검은 면적이며, 정확히 몇 개의 물리 파편이 생성됐다는 뜻도 아니다.

개별 `pose-0/1/2/3-RGBA.png`는 새 아틀라스에서 원화 픽셀을 그대로 내보낸 이미지다. 인포그래픽을 잘라 만든 것이 아니다. 밝은 배경 비교 이미지는 검수용 합성일 뿐, 게임 Asset의 투명 배경을 바꾸지 않았다.

## 3. L2 구현

새 `ShibuyaBlackFlashArtPreview`는 전투 bootstrap이나 피해 이벤트에 구독하지 않는 Editor 전용 객체다.

- 기존 가지 생성기를 사용하지 않고 새 원화를 그리는 **고정 곡면2장**을90도로 배치한다. 카메라 방향으로 계속 회전하는 billboard는 아니다.
- 주 캔버스5m,측면 보조층0.86배,곡면 깊이0.45m다. 캔버스 여백이 있어 실제 검은 외곽의 지름이 항상5m라는 뜻은 아니다.
- 작은 닫힌 접촉 볼륨을 중심에 두어 두 면의 깊이 연결을 보조한다. 큰 닫힌 판/33개 튜브를 본체로 쓰지 않는다.
- 거의 edge-on인 면은 면 전체의 시선각으로 숨긴다. 최초 캡처에서 곡면의 일부 법선만 살아 가느다란 인쇄 줄이 남는 현상을 보고 보완했다.
- L2는 alpha-mask 기반 검정,일반 alpha 합성,depth write ON이다. 검정에 additive를 적용하지 않는다.

핵심 실루엣은 원화4장이 결정한다. 작은 noise/무작위 정점으로 본체 모양을 생성하지 않는다. 전체 Transform이 시간에 따라0→1로 커지거나 뿌리부터 끝을 공개하지 않는다.

## 4. L3 구현

L3는 L2 곡면과 **같은 Mesh/UV/원화 프레임/후반 UV 이동**을 사용한다. 알파의 이웃 표본에서 경계를 계산하므로 검정과 상관없는 별도 테두리 그림을 띄우는 구조가 아니다.

- 선명 경계 기본2.4 원화 픽셀,잔광7픽셀.
- HDR red `(5,0.012,0.008)`.
- 외곽 발광 분포0.72와 위치·시각에 따른 폭/밝기 변화로 균일한 네온 외곽을 피한다.
- L3는 별도 material/renderer의 additive 합성,depth write OFF다. L2의 깊이와 작은 depth bias를 사용한다.
- 독립 붉은 방전2개는 짧은 직접 작성 경로다. 별도 pressure ray fan이나 Y자 분기망은 만들지 않았다. 현재 이 경로의 위치 움직임은 제한적이며, 외곽의 밝기/폭 변화가 주요 동작이다.

L3 단독 모드는 검은 mask의 경계를 그리되 검은 면을 렌더하지 않는다. 본체와의 결합은 L2+L3 모드에서 확인한다. 이 shader는 scene-color distortion이나 화면 반전을 하지 않는다.

## 5. 실제 시간 구성

| age | 포즈/동작 |
|---|---|
| 0~35ms | 최초 검은 파열이 바로 나타남 |
| 35~105ms | 최대 파열 포즈 유지,붉은 외곽의 국소 밝기/폭 변화 |
| 105~190ms | 찢어진 윤곽 포즈로 교체 |
| 190~300ms | 해체 포즈,조각이 바깥쪽으로 이동하는 UV 변형 |
| 255~300ms | 마지막 약45ms fade |
| 300ms 이후 | 새 L2/L3 전체 비활성화 |

전체0.3초와 포즈 종료 시각,원화 순서는 새 QA Data Asset에서 조절한다. 뒤의 UV 변형은 최대0.055셀 정도의 바깥 이동이며, 실제 physics 파편/연기 시스템은 아니다. 동일한 한 이미지를 확대·축소한 결과가 아니다.

4포즈를 교체하는 방식이어서 일부 구간은 step/pop처럼 느껴질 수 있다. 고밀도 연속 작화를 완성했다고 하지 않는다. 전투 히트스톱이나 TimeScale을 변경하지 않았다.

## 6. QA 위치·구도와 REV1 비교 조건

실제 `ShibuyaCombatMVP` Play Mode에서 공격자/더미를 같은 위치에 놓고, 더미의1m 높이에서 공격자 쪽 캡슐 반경만큼 옮긴 **근사 접촉점**에 재생했다. limb 접촉을 실제 공격 이벤트에서 검출한 것은 아니다. 새로운 피해나 판정 연동도 없다.

카메라 수평 거리6m,높이 오프셋2m,정면/측면/45도/360도 회전을 사용했다. 원속도 촬영은 매 runtime frame의 실제 unscaled 시각으로 age를 계산하고 같은 프레임에서 REV1/L2/L2+L3/L3를 차례로 렌더했다.

REV1 비교는 기존 R&D 도구의 **명목 지름4m 정규화 QA 재생**이다. 실제 전투 profile.scale은 그대로다. 새 아트의5m 캔버스와 여백이 있어 pixel area나 실제 외곽을 완전히 같게 정규화한 성능 실험은 아니다. 새 결과의 차이는 면/연결/원화 교체에서도 확인되며 크기 증가만으로 성공 판정하지 않는다.

## 7. 원속도 영상과 이미지

모든 새 QA: `unity/Logs/ShibuyaPass4HArtDirectedQA/run-001/`.

최종 재생 자료는 **`videos/`**이며29개 MP4와16개 PNG 검수 이미지를 확인했다. 주요 파일:

- **`REV1-vs-L2L3-side-1x-loop.mp4`**: 기존 REV1 대 새 L2/L3,측면 원속도3회 반복. 먼저 볼 자료.
- **`L2L3-four-views-1x.mp4`**: 정면/측면/45도/회전4분할,원속도4초.
- `L2-side-1x.mp4`, `L2-front-1x.mp4`, `L2-diagonal-1x.mp4`, `L2-orbit-1x.mp4`: 검은 본체만.
- `L2L3-side/front/diagonal/orbit-1x.mp4`: 본체+붉은 외곽.
- `L3-side/front/diagonal/orbit-1x.mp4`: 붉은 외곽/방전만.
- `comparison-front/side/diagonal/orbit-1x.mp4`와 각 `-0.5x.mp4`: 네 모드 동시 비교.
- `keyposes-on-light.png`: 실제 새 원화4포즈의 밝은 배경 검수.
- `L2-time-silhouettes.png`: 동일 측면 구도에서16~290ms의 L2 변화.
- `peak-views.png`: 전투 장면의 정면/측면/45도/후면 근처 peak 비교.
- `phase-0.016/0.033/0.060/0.100/0.140/0.190/0.235/0.290/0.320.png`.

개별 투명 키 포즈는 `validated/legacy/ShibuyaPass4B3QA/Art-keyposes/pose-0~3-RGBA.png`다. 최종 raw PNG/runtime time은 같은 root의 `Art-video/`, `Art-timing-*.json`에 있다. `video-verification.json`은 인코딩 파일의 존재·해상도·FPS·길이·프레임 수다.

| 구도 | 실제 source frame | 평균 간격 | 최대 간격 |
|---|---:|---:|---:|
| 정면1초 | 60 | 16.674ms | 20.132ms |
| 측면1초 | 60 | 16.865ms | 33.272ms |
| 45도1초 | 60 | 16.780ms | 26.462ms |
| 회전4초 | 237 | 16.905ms | 48.405ms |

60fps 최종 파일은 실제 sample 간 유지 시간을 사용했고 부족한 구간은 표본을 반복한다. 광학 보간이나 새 동작 프레임을 생성하지 않았다. 회전은4초360도 동안 효과를4회 재생해 서로 다른 방향에서 peak를 보게 했다. 하나의 효과가4초 유지되는 것은 아니다. 최초 포즈도 고정 영상마다2~3개의 실제 sample에 포함됐다.

원본 레퍼런스는30fps 녹화에 카메라/화면 반전/완성 캐릭터·음향이 섞여 있다. 본체만 시험한 새 자료와 전체 임팩트를 동일 조건으로 비교하지 않는다. AI 검수는 시간순 실제 프레임과 메타데이터 대조이며 사람의 연속 실시간 지각·독립 블라인드 심사를 대신하지 않는다.

## 8. 각도별 가독성과 남는 시각 한계

peak age60ms,480×360,QA layer만 투명 배경에 렌더한 L2의 alpha>12/255 픽셀:

| yaw | 픽셀 |
|---:|---:|
| 0° | 10,198 |
| 45° | 10,145 |
| 90° | 7,373 |
| 135° | 9,724 |
| 180° | 9,571 |
| 225° | 12,004 |
| 270° | 7,227 |
| 315° | 9,519 |

이8개 수평 각도에서 완전히 사라지는 경우는 없었다. 측면에서 약해지고 대각에서 겹친 면적이 증가하는 차이는 분명하다. 캐릭터/배경 가림을 제외한 수치이므로 실제 전투 화면의 최종 가시 픽셀과 같지 않다. 최고 하이앵글·모든 높이를 검증한 것도 아니다.

**이번 결과에 대한 판단:** 기존 REV1의 가는 연결 가지보다 넓은 검은 찢김과 해체의 차이는 명확하다. 그러나 완전한3D 볼륨은 아니며45도에서는 두 곡면의 교차/겹침이 인쇄된 면처럼 보일 수 있다. 카메라를 돌릴 때 원화의 방향성이 바뀌어 보이고, pose 전환에서 팝감이 남을 수 있다. 중앙은 캐릭터에 가려지고 아래쪽 획은 지면 depth에 가려진다.

붉은 외곽은 검정에 연결됐지만 작은 해상도에서는 선이 가늘어지거나 반짝일 수 있다. 독립 방전2개의 위치 움직임도 아직 단순하다. 원본 수준의 최종 흑섬·연속 작화·영화적 타격감을 완성했다는 평가는 하지 않는다. **사용자가 본체의 정체성 자체에 만족하는지 먼저 검수해야 한다.**

## 9. 기술 검증과 실패 보존

| 실행 | 결과 | 의미 |
|---|---|---|
| keyposes | 1 PASS | 초기 원화 alpha/서로 다른 포즈/레이어 공유와 이미지 출력 |
| final-targeted | 단일3 PASS | 첫 재생 순서의 촬영·8방향·정리. 이후 원화 순서를 조정했으므로 최종 시각 근거는 아님 |
| authored-order-final | 1 PASS / 2 FAIL | 측면 촬영60.532ms gap,키 포즈 검사 최종 도달 확인 실패. 완료로 처리하지 않음 |
| **validated** | **단일3 PASS / 0 FAIL / 0 SKIP** | 최종 원화 순서·실제 촬영·8방향·키 포즈·12회 반복 정리 검증 |

중간 실패 XML과 촬영을 모두 보존했다. 마지막3 PASS를 여러 실행의 통과 수 합산으로 만들지 않았다. 기존 전투 전체 회귀를 불필요하게 반복하지 않았고 PASS4A 과거18건도 해결 처리하지 않는다.

보완 사항:

- 처음 새 PNG를 AssetDatabase 반영 전에 조회해 null 도구 응답이 있었고, 초기 importer shape가 Cubemap으로 잡힌 것도 확인했다. **새 PNG에 한해 Texture2D/입력 alpha/NPOT 유지/압축 없음**을 명시해 정상화했다. 프로젝트 설정이나 기존 texture는 수정하지 않았다.
- 곡면의 per-pixel 법선으로 grazing을 판단하면 옆으로 선 면이 검은 줄로 남아, 면 전체의 기준 방향으로 숨기는 방식으로 보완했다.
- 키 포즈 검사의 숨긴 객체 조회를 includeInactive=true로 명시하고 시작 yield/진행 stage 진단을 보완했다. 최초 최종도달 실패의 모든 내부 경위를 확정했다고 하지는 않는다.
- native readback storage와 PNG 후처리를 사용한다. 추가로 촬영 전 준비 단계에서 managed 정리를 수행하고3개 runtime frame을 기다려 기록 시계를 시작했다. 녹화 품질 기준55ms 미만은 낮추지 않았다. 최초 지연의 단일 원인을 GC라고 확정하지 않는다.

최종 컴파일 오류0,새 shader message0. 마지막 Editor Console ground truth 오류0/경고1(기존 폰트 경고)이다. 누적 세션의 과거 Temporal QA 컴파일 오류2건과 기존 경고는 지우지 않았다. `console-buffer.json`과 `console-status.json`에 구분해 보존했다.

## 10. 리소스·시스템 보호

12회 반복 생성/샘플/정리 후 소유 Mesh/Material/Profile 각각0. 새 본체는 고유 mesh4개/정점604개/renderer6개다. L2/L3는 같은 두 곡면 mesh를 공유하고 material은 검정·붉은색2개다. per-frame geometry/texture 생성은 하지 않는다.

1254² RGBA32 아틀라스는 압축·mipmap 없음/readable이며 Unity runtime memory 조회는 **12,779,024bytes(약12.2MiB)**였다. CPU readable 사본을 포함한 개발 설정이므로 배포용 최적화 완료로 설명하지 않는다. 실제 GPU ms/overdraw/저사양/대량 동시 재생 benchmark는 미실행이다. 촬영용 대용량 native buffer/readback 비용도 본게임 비용과 구분한다.

HP/주력/production VFX 수락 수/피해 observer 수는 QA 전후 불변이었다. 초기화/피해 이벤트/확률/치명/Zone/반격/숫자/카메라 controller에 연결하지 않았다. 기존 scene/prefab/Packages/ProjectSettings를 저장하지 않았다.

종료 상태: Play stopped,scene clean,compiling false,TimeScale1,fps -1,runInBackground false,AutoTick true/16ms,Game View PlayUnfocused. Computer Use/마우스 조작/강제 포커스 전환/RenderDoc 설치는 하지 않았다.

시작 baseline의 **소스·문서·참조1,391개 SHA-256 불변/유실0**, 기존 관련 QA **7,588개 경로 크기·mtime 불변**이다. QA 검사는 새로운 전수 SHA 검사라고 확대하지 않는다. 원본 레퍼런스 PNG/영상과 기존 `.slnx`도 보존했다. 상세 `baseline.json`, `old-qa.json`, `preservation.json`.

## 11. 추가한 파일과 Inspector

기존 파일을 덮어쓰지 않고 다음6개와 각 `.meta`를 추가했다.

1. `unity/Assets/Shibuya/Editor/BlackFlashArtAtlas.png`: 새4포즈 투명 원화.
2. `unity/Assets/Shibuya/Editor/Black Flash Art Directed QA.asset`: QA 전용 설정.
3. `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashArtProfile.cs`: 설정/한국어 SerializedProperty CustomEditor.
4. `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashArt.shader`: L2 alpha mask와 L3 additive contour.
5. `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashArtPreview.cs`: 독립 미리보기·레이어/REV1 선택·정리.
6. `unity/Assets/Shibuya/Editor/ShibuyaPass4HArtDirectedTests.cs`: targeted 검사/촬영.

이 보고서 포함 신규13개다. 원화·크기·곡면 깊이·측면 보조층 크기·포즈 종료 시각·전체 길이·원화 순서·붉은 HDR 색·경계/잔광 폭·발광 분포·독립 방전 사용을 한국어 Inspector에서 조절한다. 실제 전투의 Profile/Geometry/Shader는 수정하지 않았다.

Git은 같은 브랜치/HEAD,staged0이다. 시작 tracked dirty7/untracked45에서 신규13개가 추가된 **tracked dirty7/untracked58**이며 기존 tracked 내용은 시작 전부터 있던 그대로다. QA/원본은 Git에 추가하지 않았다. staging/commit/push/reset/clean/merge/checkout/worktree 생성 없음.

## 12. 사용자 직접 검수 방법과 종료

1. `videos/REV1-vs-L2L3-side-1x-loop.mp4`를 원속도로 먼저 본다. 검은 질량의 넓이·찢김·해체가 기존 가지와 다른지 판단한다.
2. `L2L3-four-views-1x.mp4`와 `L2-side-1x.mp4`를 본다. 붉은 발광을 빼도 본체가 흑섬의 파열로 읽히는지가 중요하다.
3. `keyposes-on-light.png`와 `L2-time-silhouettes.png`로 디자인과 시간 변화를 확인한다. 필요할 때0.5배속을 본다.
4. Unity Play → **JJK Game → 시부야 PASS4H → 흑섬 아트 디렉션 L2·L3**.
5. `L2 검은 본체만`/`L3 붉은 외곽·방전만`/`L2+L3`/`기존 REV1 비교`를 선택하고 `접촉 위치에서 재생`을 누른다. 반복 또는 검수 시각 고정으로0.02/0.06/0.14/0.235초를 확인한다. 카메라는 기존 자유 조작을 그대로 쓴다.
6. 창을 닫거나 `임시 자원 정리`/Play 종료로 QA를 정리한다. 일반 게임의 흑섬은 계속 기존 REV1이다.

**실제 게임 채택과 시각 품질은 USER VERIFIED 대기다.** 본체가 여전히 평면 인쇄물이나 뾰족한 판으로 느껴진다면 그 피드백을 기준으로 다음 방향을 결정해야 한다. 이번에는 L2/L3 시제품과 증거 제공에서 멈추며 다른 레이어나 전투 연동을 자동 진행하지 않는다.
