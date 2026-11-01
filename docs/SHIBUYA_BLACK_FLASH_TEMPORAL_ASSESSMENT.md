# 흑섬 VFX 시간축·렌더링 독립 기술 검토

2026-11-01 / LOCAL / `feat/shibuya-combat-mvp` / HEAD `891064067834bc26f3a1afc6a37b84bb5e1e79c1`.

**분석 및 후속 실험 제안만 수행했다. REV1/A/B/C 교체, VFX·Shader·Data Asset·전투·카메라 수정, Unity Play Mode·테스트 실행, 도구 설치, Git staging/commit/push는 하지 않았다.** 새로 작성한 것은 이 보고서다. 기존 기술 PASS를 시각 합격이나 USER VERIFIED로 승격하지 않는다.

## 1. 실제 조사 범위와 판단 요약

**결론: 나뭇가지 인상은 ‘연결된 가는 입체 가지’라는 형상과 ‘그 가지가 뿌리부터 자라고 오래 유지되는’ 시간축, 불투명한 검정/붉은 rim 표현이 결합한 결과라는 설명이 가장 강하다. 시간축 문제의 존재는 코드로 확인되지만 기여도가 90%라는 주장은 입증되지 않았다.**

현재 제작 방식을 고정하고 **성장 시간 한 값만 바꾼 QA 비교**를 먼저 권한다. A/B/C 제작 방식 비교의 확대, VFX Graph 전환, 화면 반전·굴절 추가는 그 결과 이후에 판단한다.

읽은 자료:

- `AGENTS.md`, `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md`, `docs/locked/USER_VERIFIED_SETTINGS.md`의 현재 관련 지침.
- `docs/design/BLACK_FLASH_VFX.md`, `docs/design/VFX_DIRECTION.md`, `docs/architecture/CAMERA_PRESENTATION.md`, `docs/references/README.md`.
- `docs/SHIBUYA_PASS4H0_REVIEW.md`, `docs/SHIBUYA_PASS4H0_VISUAL_REV1_REVIEW.md`, `docs/SHIBUYA_BLACK_FLASH_VISUAL_RND.md`.
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlash{Geometry,Visual,Presentation,Profile}.cs`, `ShibuyaCombatDamageResult.cs`, `ShibuyaMatch.cs`와 관련 Fighter 연결.
- `unity/Assets/Shibuya/Shaders/ShibuyaBlackFlashInk.shader` / `ShibuyaBlackFlashLight.shader`, `Resources/Shibuya Black Flash.asset`.
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashRndPreview.cs` / `.shader`, 관련 R&D 테스트·촬영 기록.
- Unity/URP 버전·실행 중 API의 읽기 전용 조회, PC/Mobile Pipeline, Renderer, RenderGraph 설정과 로컬 URP 패키지 소스.

영상은 다음 실제 파일을 읽고 메타데이터 및 시점별 디코드 영상을 확인했다. 사람이 원속도 영상을 연속 시청한 것과 동등한 지각 검증이나 독립 블라인드 심사를 했다고 표현하지 않는다.

| 원본 | 실제 확인 | 이번 비교 구간 |
|---|---|---|
| `_local_refs/videos/blackflash_refs/로블_흑섬.mp4` | 1198×676, 30fps, 약 20.467초 | Manji 약 1.533~2.167초; 재추출 표본 1.533/1.633/1.700/1.767/1.833초 |
| `_local_refs/videos/blackflash_refs/주팬퍼_흑섬.mp4` | 1292×620, 30fps, 약 113.733초 | 나나미 약 66.733~67.100초; 재추출 표본 66.733/66.767/66.833/66.900/67.067초 |

기존 REV1의 `unity/Logs/ShibuyaPass4H0REV1QA/20261101/evidence-final/six-phases-side.png`, 같은 시드 전후 자료와 R&D `unity/Logs/ShibuyaBlackFlashVisualRndQA/20261101/evidence-final/`의 비교 이미지·영상 메타데이터·인접 프레임 자료를 대조했다. 이 분석에서 원본을 변환·덮어쓰거나 새 Unity 촬영을 하지 않았다. 재추출 프레임은 메모리에서만 조합했다.

**확신 구분:** 수식·설정·메시 구성은 확인 사실, 그것이 ‘나무’로 느껴지는 기여 순서는 근거가 강한 해석, 개선 정도와 최종 제작 방식은 미확정이다.

## 2. 현재 시간축과 수치

### 실제 사용 알고리즘

`ShibuyaBlackFlashVisual.Play` 27~33행은 루트 scale을 프로필의 **1.0으로 고정**한다. 현재 REV1에 ‘Transform scale 0→1 SmoothStep’이 있다는 설명은 틀리다. 메시 경로와 topology는 재생 시작 시 한 번 생성한다.

대신 `ShibuyaBlackFlashInk.shader`의 `Vert` 25~51행과 `Frag` 68~79행에서 **정점 확대와 길이 방향 공개를 동시에** 한다. 현재 저장 `_Growth = 0.095초`다.

```text
코어 정점 확대: 0.25 + 0.75 × saturate(age / 0.095)
코어 자식 Transform: 항상 1.65배
코어 추가 찢김: tear × 0.32 × burst² × (0.12 + 0.7f)

가지 정점 확대 g = saturate((age - 0.008) / 0.095)
anchor 기준 확대 S = 0.24 + 0.76g
가지 공개: clip(saturate((age - 0.008 - birth) / 0.095) - 0.92f)
주줄기 birth = 0, 두 지류 birth = 0.25×0.095 / 0.5×0.095
가지 추가 전방 변형: g² × f × 0.32 × 0.35
```

`f`는 메시 길이 좌표다. 따라서 작은 메시가 커지는 동시에, 그중에서도 뿌리 쪽 일부만 먼저 보인다. 이는 ‘완성된 파열이 순간 출현’하는 방식보다 **뻗어 나가는 줄기**를 강하게 설명한다.

주줄기 끝 공개는 약 **95.4ms**, 정점 확대 완료는 **103ms**, 두 지류 끝 공개는 **119.15 / 142.90ms**다. 공개 이후에도 작은 분기는 45ms 단위로 흔들림/표시 조합을 바꾸지만, 분기 연결 자체가 새로 생성되지는 않는다. 주가지는 연속 sin/cos 변형을 쓴다.

이 본체에 AnimationCurve는 없다. HLSL `lerp`, `saturate`, `clip`, sin/cos, 작은 분기의 `floor`가 핵심이다. `smoothstep`은 주로 rim과 연기 등장/시선각 alpha에 쓰이며 주가지 성장의 easing 함수가 아니다. 명시적인 Hold 상태도 없다.

### 요청 시점별 계산표

아래는 **현재 저장 프로필과 소스 수식에서 계산한 예측값**이다. 새 런타임 계측/GPU 실측값이 아니다. t=0은 `Play`의 시각 원점이며 원본 영상의 실제 HP 감소 시각과 동일하다고 가정하지 않는다.

`F=min(1,g/0.92)`는 주줄기에서 공개될 수 있는 길이 좌표 비율, `R=S×F`는 뻗음의 명목상 지표다. R은 굽은 경로·뿌리 위치·추가 변형·두께·noise clip·카메라 가림을 제외하므로 실제 월드 최대 거리나 화면 픽셀 비율이 아니다. 코어 배율도 추가 찢김을 제외한 자식 Transform×정점 배율이다.

| ms | 코어 배율 | 가지 정점 배율 S | 공개 F | 명목 뻗음 R | 접촉광 fade | 압력 호 배율 | 가지 alpha 상한* |
|---:|---:|---:|---:|---:|---:|---:|---:|
| 0 | 0.413 | 0.240 | 0.000 | 0.000 | 1.000 | OFF | 0.99 |
| 16.7 | 0.630 | 0.310 | 0.100 | 0.031 | 0.629 | OFF | 0.99 |
| 33.3 | 0.846 | 0.442 | 0.289 | 0.128 | 0.260 | 0.331 | 0.99 |
| 50 | 1.064 | 0.576 | 0.481 | 0.277 | OFF | 0.515 | 0.99 |
| 83.3 | 1.498 | 0.842 | 0.862 | 0.726 | OFF | 0.881 | 0.99 |
| 100 | 1.650 | 0.976 | 1.000 | 0.976 | OFF | 1.065 | 0.99 |
| 150 | 1.650 | 1.000 | 1.000 | 1.000 | OFF | 1.615 | 0.99 |
| 200 | OFF | 1.000 | 1.000 | 1.000 | OFF | 2.100 | 0.99 |
| 300 | OFF | OFF | OFF | OFF | OFF | OFF | OFF |

\* alpha 상한은 공개/소거를 통과한 주가지 픽셀의 값이다. 예를 들어 0ms에는 거의 공개되지 않았고 200ms에는 부분 소거가 진행될 수 있으므로 ‘거의 불투명한 완성 가지가 표의 모든 시각에 전부 존재한다’는 뜻이 아니다.

### 레이어별 시작·종료·소거

| 레이어 | 현재 시간 | 실제 변화 |
|---|---|---|
| 접촉광 | 0~45ms | 완성된 가는 흰 튜브 11개가 선형 fade. 화면 전체 impact frame 아님 |
| 코어 | 0~190ms | 95ms까지 정점 확대/찢김. alpha는 152ms까지 0.99, 이후 감소 |
| 가지 | 0~280ms | 주가지 약 103ms까지 확대, 지류 공개는 약 143ms까지. alpha는 224ms까지 0.99 |
| 압력 호 | 25~215.91ms | `min(2.1,0.24+11×localAge)` 확대 + 선형 fade. 194.09ms에 최대 배율 도달 |
| 연기 | 65~685ms | localAge 0~50ms 동안 등장, 이후 개별 drift/상승/팽창과 fade |
| 잔여 방전 | 65~445ms | 45ms마다 표시/윤곽 조합 변경, 개별 바깥 이동과 fade |
| 인스턴스 | 750ms | pool 비활성화 |

소거도 이름만으로 판단하면 안 된다. `dissolve=saturate((t-0.35)/0.65)`, `clip(n×0.24+0.75-dissolve)`, n∈[-1,1]이므로, 실제 첫 noise clip이 가능한 시점은 수명의 **68.15% 이후**다. 코어 약 **129.49ms**, 가지 약 **190.82ms**다. 수명 35%부터 바로 구멍이 뚫린다는 설명은 틀리다. 실제 표면 위치에서 구멍이 처음 생기는 정확한 시각은 n 분포에 따라 더 늦을 수 있다.

따라서 가지는 약 103ms에 다 뻗고 약 191ms 전까지 거의 잘리지 않는 구조다. 명시적인 Hold는 없지만 **완성된 동일 가지가 유지되는 시각 구간**은 이미 있다. 무조건 100~200ms Hold를 더하면 오히려 물체성이 길어질 수 있다.

## 3. 나뭇가지 인상의 원인과 렌더링 분해

### 근거가 강한 원인 순서

1. **고정된 연결 그래프 + 복합 성장.** `Geometry.Tube` 23~37행은 6각 단면과 양 끝 캡이 있는 닫힌 튜브다. 길이를 따라 반경이 `Lerp(1,0.012,Pow(f,0.72))`로 가늘어진다. `Build` 74~85행은 줄기 11개에 각각 지류 2개, 총 **33개 획**을 만든다. 굵은 것은 4개여도 연결된 가는 획은 많이 남는다. 이 경로가 100~143ms 동안 드러난다.
2. **낮은 실루엣 교체율과 늦은 소거.** 작은 획은 45ms 단위로 변하지만 굵은 주가지는 같은 중심선과 연결을 유지한다. 주가지 흔들림의 기본 최대 xy 성분은 0.055×0.32≈0.0176m로, 길이 약 1.91~2.93m의 주 경로에 비해 작다. ‘살아 있는 먹물 면의 교체’보다 같은 가지가 흔들리는 인상이 생길 조건이다. 이는 변형 성분별 비교이며 전체 이동량 실측은 아니다.
3. **불투명 검정 + 물체 외곽을 강조하는 rim.** `Ink.Frag` 78~89행은 거의 검은 내부와 시선각 기반 붉은 경계를 같은 패스에서 계산한다. 높은 alpha와 depth write가 덩어리/표면의 존재를 강화한다. 카메라를 돌리면 rim 밝기도 바뀐다. Geometry만큼 변해 보이는 밝기 변화가 실제 실루엣 교체는 아닐 수 있다.
4. **충돌핵보다 길고 가는 획이 먼저 인식되는 구도.** 정면에서는 공격자/피격자가 작은 중심을 가린다. 실제 카메라 거리·밝은 블록아웃 배경에서는 작은 검은 면보다 외곽의 긴 획이 더 쉽게 읽힌다. 중심의 질량과 주변 streak의 비율 문제이며 단순 크기 부족이라고 확정하지 않는다.

1~3의 구현 조건은 확인됐다. 각 원인의 지각 기여도, 순위 차이는 후속 동일 형상 비교 없이는 확정하지 않는다.

### 실제 renderer/material 연결

`Visual.Initialize` 16~24행: 인스턴스당 MeshFilter/MeshRenderer 5쌍. `Presentation.Configure` 29~35행: scene 소유 material 3개. 매 프레임 mesh topology를 만들지 않고 MPB의 age 등만 바꾼다. pool 재생 시작에는 seed 기반 CPU 생성/업로드가 있다. 종료 시 비활성화하며 소유자가 파괴될 때 mesh/material을 정리한다.

| renderer | 실제 geometry | 재질/패스 | 렌더링상 의미 |
|---|---|---|---|
| ContactLight | 가는 닫힌 튜브 11개 | Light, queue 3010, additive, ZWrite OFF, Cull OFF | 흰 월드 광선. 화면 반전 아님 |
| TornCore | 작은 닫힌 Mass + 굽힌 닫힌 lobe 4개 | Ink, queue 2980, alpha blend, ZWrite ON, Cull Back | 공간이 있는 핵이나 여전히 닫힌 표면 |
| BranchingRupture | 주·보조 11 + 지류 22 튜브 | 같은 Ink | 붉은 rim/vein은 별도 renderer가 아닌 동일 draw의 fragment 계산 |
| BrokenPressure | 기울어진 불완전 호 7개 | 같은 Light | 선/호 확대. 실제 배경 굴절 없음 |
| SmokeEmbers | 거친 닫힌 Mass 12 + 작은 방전 튜브 12 | Ink shader를 쓰는 smoke material, ZWrite OFF | ParticleSystem/flipbook 연기가 아닌 메시 proxy. 같은 draw 안에 연기/방전 분기 |

고정 queue상 Ink/Smoke는 2980, Light는 3010이다. 같은 queue 안의 서로 다른 renderer 순서는 카메라 거리 등 정렬 조건에 따라 달라진다. 코어/가지의 depth와 겹침이 연기·뒤쪽 발광을 가릴 가능성은 있지만, 실제 이상한 픽셀의 원인이라고 확정하려면 draw별 관측이 필요하다. queue만 보고 매번 고정된 5개 순서를 가정하지 않는다.

`RecalculateNormals`는 CPU 생성 시 한 번이다. 이후 GPU 변형에 맞는 법선 재계산은 없으므로 rim이 변형된 곡면과 어긋날 수 있다. 튜브의 tangent 기준축이 특정 기울기에서 바뀌는 코드도 있어 단면 비틀림 가능성이 있으나, 현재 주된 시각 결함으로 재현·확정하지 않았다.

**판정:** 3D이기 때문에 실패한 것이 아니다. 현재 선택한 ‘닫힌 가는 분기 형상 + 성장 + 유지 + 경계 발광’이 나무/거미 다리와 유사한 시공간 단서를 함께 제공한다.

## 4. Gemini 주장별 평가

| 제안 | 판단 / 기대 효과 | 한계·충돌 위험 | 비용 / 우선순위 |
|---|---|---|---|
| 시간 보간이 핵심 | 실제 정점 lerp와 root-tip 공개가 있어 부분 동의 | 루트 scale 0→1 설명은 REV1과 다름. 90% 기여도 근거 없음 | 현재 코드 분석으로 확인 / 높음 |
| Stepped/Constant | 본체의 급격한 윤곽 상태 교체에 유효할 수 있음. 작은 방전은 이미 stepped | 모든 레이어를 floor 처리하면 압력/연기까지 덜컥거림. 고정 topology는 남음 | QA만 적용 중간 / 성장 실험 다음 |
| 빠른 확장 + Hold | 첫 출현을 파열처럼 만드는 타당한 가설 | 현재도 완성 가지가 오래 유지됨. 0.1~0.2초 추가 Hold는 물체성·연속 흑섬 가림을 늘릴 수 있음 | 성장 시간 비교는 작음, 독립 Hold clock은 중간 / 성장 먼저 |
| 독립 레이어 시계 | 강하게 동의. 현재도 시작/수명이 일부 분리되어 있음 | 같은 `_Age`에 성장·소거·rim noise가 묶여 있어 본체만 멈추는 제어는 불완전 | 부분 확장 가능 / 실험 결과 이후 |
| 손그림 flipbook | 찢긴 면/모양 교체를 직접 저작하기 유리 | 작화 품질이 필요. 단일 camera-facing 평면의 부피 한계. 현재 B는 손그림 품질 대표가 아님 | 작화 비용 큼 / 즉시 전환 보류 |
| 3D+flipbook | 깊이와 저작성의 절충 가능 | C처럼 교차 인쇄면·alpha/depth 간섭이 남을 수 있음 | 중간~큼 / 현재 C 자동 채택 금지 |
| Distortion/Shader Graph | 배경 굴절과 파열의 공간 반응에 유효 | 구현 경로와 copy 시점 필요. 나뭇가지 형상·시간축 자체는 고치지 않음 | 중간 / 본체 진단 다음 |
| 화면 impact frame | 흑섬 충격을 강하게 보완 | 본체 결함을 가릴 수 있으며 H1 범위. 확률 확정 전 BF 전용 예고 금지 | 중간 / 별도 승인 후 |
| Ribbon/Trail | 실제로 이동하는 획이나 잔류에 적합 | 고정 충돌핵을 대체한다고 자동 해결되지 않음. 새 판/끈 인상 가능 | 중간 / 필요성 확인 후 |
| VFX Graph 전환 | 대량 입자·GPU authoring에는 선택지 | 에너지의 작화나 timing을 자동 해결하지 않음. 이번 원인 분리에 필요 없음 | 큼 / 현재 비추천 |
| 수치 기반 시간 QA | 강하게 동의 | 픽셀 수나 PASS만으로 주관적 파열감을 승인할 수 없음 | 낮음~중간 / 최우선 실험에 병행 |

‘메시 없이 impact frame+distortion’은 **화면 연출만의 기여도**를 보는 실험이다. 본체가 나무처럼 보이는 원인을 가리는 첫 실험으로는 변수가 너무 많다. 후속 H1에서 본체 ON/OFF와 화면 효과 ON/OFF를 분리할 때 유용하다.

## 5. ChatGPT 주장별 평가

- 실제 코드를 먼저 확인해야 한다는 반론은 맞다. 이번 확인으로 보간/성장이 실제 존재한다는 단계까지 전진했다. 이제 원인을 모른다는 말만 반복할 단계는 아니다.
- 모든 3D 메시가 고체처럼 보이는 것은 아니라는 반론도 맞다. 형상·alpha·소거·화면상 폭·motion identity가 함께 결정한다. 현재 A의 판 같은 인상은 3D 전체의 한계를 입증하지 않는다.
- 레이어별 독립 시간, screen effect와 본체 원인 분리는 타당하다. 다만 현재 이미 시작/수명 분리가 있으므로 ‘독립 시계가 전혀 없다’는 표현도 부정확하다.
- 시간축 우선 실험은 합리적이다. **처음부터 Pop/Hold/Dissolve/색/크기를 한꺼번에 바꾸지 않고 성장 값 하나부터** 해야 원인을 분리할 수 있다.
- BF 성공 확정 전 전용 예고를 금지하는 판단에 동의한다. 현재 `Presentation.Present` 40~50행은 Applied + BlackFlash + 유한한 양수 HP 감소 이후에만 재생한다. 일반 공격 준비와 성공 이후 표현은 분리해야 한다.

## 6. 두 분석에서 추가로 놓치기 쉬운 것

1. **곱으로 작용하는 성장과 늦은 실제 소거.** 함수 이름이나 프로필의 95ms만 읽으면 실제 지류 공개 143ms와 68.15% 소거 시작을 놓친다. ‘4개 주가지’ 뒤에는 29개 가는 획이 더 있다.
2. **R&D는 제작 기법을 단독 비교하지 않았다.** A/B/C는 형상·패턴·시계·alpha·시선 반응을 함께 바꿨다. B/C의 절차적 방사 그림이 꽃/별처럼 반복되는 것은 flipbook이라는 기술 전체의 한계와 다르다.
3. **회전 각도와 효과 나이의 결합.** 기존 1초 회전 영상에서 본체는 초반 약 0.3초만 존재한다. 뒷면 각도를 볼 때는 잔류만 남아 있으므로 ‘회전 영상이 있다’는 사실이 모든 각도 peak 검증을 의미하지 않는다. 고정 정면/측면/45도 영상은 있으나 후면 peak는 별도 확인이 필요하다.
4. **촬영 시계 문제와 실제 성장 문제는 별개다.** 초기 R&D의 0.24~0.57초 간격 자료는 첫 파열을 놓쳤다. 최종 `timing-final`은 이를 보완했지만, 그 사실이 현재 REV1 자체의 100ms 성장을 없애지는 않는다. 중간 영상으로 새 결론을 내리지 않는다.
5. **카메라 가림·rim 법선·depth write가 평면 크기와 무관하게 존재감을 바꾼다.** 정면만 보고 geometry나 alpha 부족으로 단정하지 않는다. 원본의 줌/반전도 본체 픽셀 점유 측정을 교란한다.
6. **랜덤 재현과 동시 발생.** QA seed71은 고정이지만 실제 재생 seed는 `DamageResultKey.GetHashCode()`다. 세션 사이에 같은 피해처럼 보여도 동일 seed라고 보장하지 않는다. 동시 제한 8을 넘으면 오래된 live instance를 재사용하므로 연속 흑섬의 개별 시각 보장도 후속 점검 대상이다. 이번 관찰의 직접 원인으로 입증된 것은 아니다.
7. **첫 재생 비용과 시계의 관계.** `PlayAt`이 넘긴 시작 시각 뒤에 `Play`가 5개 메시를 작성한다. 첫 사용/업로드/컴파일 지연으로 초기 visible sample을 놓칠 가능성은 있다. 현재 실제 전투에서 측정하지 않았으므로 발생했다고 단정하지 않는다. 후속 촬영은 mesh 준비 완료·실제 frame timestamp를 함께 기록해야 한다.
8. **지류 뿌리가 성장하는 부모를 따라가지 않는다.** `Build` 79~84행은 완성된 부모 경로의 `path[4]/path[8]`를 지류의 고정 anchor로 저장한다. shader는 부모와 지류를 각자의 anchor 기준으로 확대한다. 따라서 부모의 변형된 접합점과 지류의 시작점이 일치하도록 보장하지 않는다. 첫 지류의 공개가 가능해지는 약 31.75ms에 부모 확대 S는 0.43인데, 지류 뿌리는 이미 완성 부모 경로의 4/13 지점에 있다. 곡률·추가 변형을 제외한 접합점 차이만 약 `0.3077×(1−0.43)×길이≈0.175×길이`다. 실제로 떨어진 조각처럼 보이는 정도는 두께·가림·방전 hash clip에 따라 달라 **화면 결함으로 확정하지 않는다.** 다만 ‘부모에서 자연스럽게 갈라지는 성장’이라는 설명에는 이 제약을 포함해야 한다. 성장 단일값 실험은 이 불일치 구간도 짧게 만든다.

## 7. 레퍼런스 및 A/B/C와의 대조

직접 디코드한 로블 Manji 표본에는 길고 연결된 획과 함께 넓은 검은 면의 갑작스러운 추가, 붉은 독립 방전, 본체 이후 잔류가 있다. 주팬퍼 나나미는 초기 비슷한 윤곽이 잠시 유지되다가 큰 검은 파열면이 추가되고 나중에 조각이 흩어진다. **원본에도 선·분기·짧은 hold가 있으므로 전부 삭제할 이유는 없다.**

기존 관측에서 첫 검붉은 표본부터 큰 화면 점유까지 약 100~134ms가 걸린다. 이를 원작의 100ms 선형 mesh 성장으로 환산하면 안 된다. 카메라, 화면 반전, 공격 모션과 새 윤곽 추가가 섞였으며 원본 녹화는 30fps다. 반대로 ‘레퍼런스는 언제나 1프레임에 최대 크기’라고 단정할 수도 없다.

REV1 측면 시트는 20→50→90ms에 같은 연결 가지가 실제로 길어지는 모습이다. 계산표와 일치한다. 140ms에는 거의 같은 연결 실루엣이 크게 유지되고, 320ms 이후에는 잔여 방전/연기가 남는다. 원본은 큰 면의 추가·교체와 가는 획의 후행을 함께 써서 구조의 정체성이 덜 고정된다.

| 후보 | geometry/표현 | 시간 변화 | 현재 한계 |
|---|---|---|---|
| REV1 | 5 renderer, 약 5,090정점, 닫힌 코어/분기/연기 | 복합 성장 + 작은 획 45ms cadence | 긴 연결 가지가 추적 가능한 물체로 남음 |
| A | 7개 넓은 닫힌 곡면 조각, 공통 광/연기; 3 renderer, 442정점 | 15ms부터 본체, 30Hz 정점 흔들림/소거, 본체 약 300ms | 이미 펼쳐져 있어도 금속판/날개처럼 보임. 시간만이 유일 원인은 아니라는 반례 |
| B | camera-facing quad + 자체 atlas, 공통 광/연기; 3 renderer, 68정점 | 16장을 300ms에 배치. 슬롯 약 18.75ms, 의도적 개별 hold 없음 | 방사 패턴·스티커·저해상도 작화. ‘손그림 방식 실패’ 증거 아님 |
| C | 굽힌 13×13 격자 3장 + 3D 파편12, 공통 광/연기; 6 renderer, 619정점 | 같은 atlas, 면별 위상차·grazing fade | 겹친 인쇄면. alpha>0.05를 통과한 반투명 픽셀도 ZWrite하므로 뒤 레이어를 가릴 수 있음 |

B/C atlas는 768×768, 셀192px, 4×4 배열이다. topology/atlas를 매 프레임 새로 만드는 구조는 아니며 QA 생성 시 작성하고 Dispose에서 정리한다. A/B/C는 Editor 전용이다. 기존 전투의 REV1 이벤트 경로를 대체하지 않는다.

기존 최종 촬영 요약의 각 1초 59~60개 고유 runtime frame, 평균 간격 약 16.67~16.99ms, 최대 38.54ms를 확인했다. 이 최댓값은 짧은 1~2프레임 impact를 정밀 판정하기에는 여전히 주의가 필요하다. 30fps 원본의 60fps 인코딩은 프레임 반복이며 원작 60fps 정보를 복원한 것이 아니다.

## 8. 가장 먼저 할 최소 실험 1~2개

**이번 보고서에서는 실행하지 않는다. 사용자가 실험 방향을 승인한 후 QA 전용 인스턴스에서 수행할 제안이다. 원본 Data Asset과 실제 피해 경로는 그대로 둔다.**

### 실험 1 — 같은 REV1, 성장 시간만 95→30ms

- control: 기존 `ruptureGrowthTime=0.095`.
- candidate: 기존 프로필을 메모리에서 복제한 QA 설정만 `ruptureGrowthTime=0.030`.
- 기존 Profile/Visual에 이미 있는 범위와 API를 사용한다. 새 VFX 기법, 새 원화, 새 gameplay clock이 필요 없다. 영구 프로필 저장 없이 비교한다.
- geometry/seed71/색/폭/전체 크기/수명/소거/카메라/배경을 같게 둔다. 새 화면 반전·distortion·카메라 shake·음향은 추가하지 않는다. 이 필드는 코어/가지 성장과 분기 birth를 함께 조절하므로 이를 ‘주가지 한 요소만’ 바꾼 것으로 설명하지 않는다.

| 예측 | control 95ms | candidate 30ms |
|---|---:|---:|
| 코어 정점 확대 완료 | 95ms | 30ms |
| 주줄기 끝 공개 | 95.4ms | 35.6ms |
| 가지 정점 확대 완료 | 103ms | 38ms |
| 두 지류 끝 공개 | 119.15 / 142.90ms | 43.10 / 50.60ms |
| renderer 수명 | 코어190 / 가지280ms | 동일 |

현 clamp의 하한 때문에 ‘완전히 펼쳐진 가지가 반드시 60fps 첫 1~2프레임 안에 보인다’고 약속할 수는 없다. 약 38ms라면 60fps 표본에서는 약 3번째 프레임이다. 1프레임 pop을 위해 clamp와 shader를 즉시 바꾸는 것보다 기존 제어로 차이를 먼저 확인한다.

**예상:** 뿌리에서 서서히 자라는 구간은 줄어든다. 그러나 수명이 같아 완성 가지가 더 오래 남기 때문에, 나뭇가지/거미 다리 인상은 유지되거나 더 강해질 수도 있다. 이것도 실패를 숨길 일이 아닌 유용한 결과다.

**관측/판정:** 정면·측면·45도 각각의 원속도 고정 클립을 같은 seed/시각으로 촬영한다. 16.7/33.3/50/100/150ms의 길이 좌표 공개, 검은 실루엣, 분리되는 조각과 지속 동일 윤곽을 비교한다. runtime frame/time 간격도 보존한다. peak 크기를 같게 맞추되 매 프레임 픽셀 면적까지 같다고 주장하지 않는다.

- 기술 예측 실패: 실제 sample age/MPB를 맞췄는데 끝 공개가 예상대로 빨라지지 않으면 uniform 전달·clip·가림·프레임 누락부터 확인한다.
- 지각 가설 약화: 성장만 빨라졌고 사용자 원속도 평가에서 여전히 같은 나뭇가지라면 ‘시간축만으로 충분’ 가설을 기각한다. geometry/동일 윤곽 지속/rim을 다음 대상으로 올린다.
- 지각 가설 지지: 같은 형상인데 즉각적인 파열로 명백히 개선되면 시간축의 기여는 지지된다. 90% 또는 최종 시각 합격으로 확대하지 않는다.

### 실험 2 — 필요할 때만, 짧은 본체 수명 비교

실험 1의 빠른 성장 후보를 유지하고 **branchDuration 하나만 280→190ms**로 낮춘 QA 후보와 비교한다. 이는 최종 tuning 권고가 아니라 ‘완성 가지가 오래 유지되는 것’의 기여를 분리하는 실험이다. 코어/광/압력/연기/작은 잔여 방전은 그대로 둔다.

현재 수식대로라면 가지의 첫 noise clip 가능 시각은 190.82→129.49ms, alpha 감소 시작은 224→152ms가 된다. 성장 완료와 late fade가 분리되어 ‘빠른 출현 + 더 빠른 본체 해체’가 되는지 확인할 수 있다. 짧아진 본체가 단순히 눈에 안 보여 문제를 숨긴 것은 아닌지, 원속도에서 충분한 검은 질량/타격 방향이 남는지를 함께 본다.

사용자가 파열의 가독성이 사라졌다고 느끼거나 30fps에서 본체가 안정적으로 잡히지 않으면 채택하지 않는다. 이 실험도 나무 인상을 해결하지 못하면 다음은 **형상과 표면**이다. 새 flipbook부터 만드는 대신 같은 peak 자세에서 굵은 연결 획/가는 방전/핵을 분리해 어느 레이어가 물체성을 만드는지 확인한다.

0.1~0.2초 Hold는 첫 실험에서 넣지 않는다. 나중에 필요하면 성장/정점 morph 전용 clock만 짧게 유지하고 alpha·소거·연기·압력 clock은 계속 진행시킨다. 현재 `_Age` 전체를 고정하면 이들을 함께 얼리므로 독립 Hold 실험이 아니다. Time.timeScale이나 실제 전투 히트스톱은 변경하지 않는다.

## 9. 렌더링 도구와 Unity 6 URP distortion

### 현재 환경

읽기 전용 Unity API 응답: **Unity 6000.3.20f1 / Windows 11(10.0.26200) / Direct3D12 / NVIDIA GeForce RTX 5060 / PC_RPAsset**. Play=false, compiling=false였다. URP는 **17.3.0**이다. 새 컴파일·Console 무오류 검증을 실행한 것은 아니다.

PC pipeline은 Opaque Texture와 Depth Texture가 켜져 있다. Mobile asset은 둘 다 꺼져 있어 PC에서 가능한 경로가 모든 품질 설정에서도 준비됐다고 가정하면 안 된다. GlobalSettings의 실제 `RenderGraphSettings.m_EnableRenderCompatibilityMode=0`과 패키지 구현은 RenderGraph 경로를 지지한다. 오래된 `m_EnableRenderGraph:0` 직렬화 값 하나로 호환 모드라고 판단하지 않는다. 현재 PC Renderer에 확인된 feature는 SSAO이며 새 distortion feature는 없다.

### Distortion은 세 가지를 구분해야 한다

1. **자체 VFX UV/정점 왜곡:** 현재 Ink 패턴/정점을 흔드는 것. 배경 scene color를 굴절시키지 않는다. 새 opaque copy가 필수는 아니다.
2. **월드 국소 굴절:** 투명 표면에서 screen UV를 변형하여 `_CameraOpaqueTexture`/Scene Color를 샘플링한다. 현재 PC 설정은 필요한 opaque copy를 지원한다. 그러나 copy는 투명 오브젝트 이전이므로 그 뒤에 그려진 흑섬 자체까지 같은 이미지로 휘게 만들지는 못한다. 깊이 fade/캐릭터 경계/화면 가장자리 처리도 필요하다. [Shader Graph 17.3 Scene Color 공식 문서](https://docs.unity3d.com/Packages/com.unity.shadergraph@17.3/manual/Scene-Color-Node.html)
3. **투명 흑섬을 포함한 화면 굴절/반전:** 원하는 draw 이후의 camera color를 복사하고 마스크를 씌워 합성하는 별도 pass가 필요하다. 예컨대 Before Rendering Post Processing 위치와 **Fetch Color Buffer**를 쓰는 Full Screen Pass 또는 명시적 RenderGraph copy/pass다. 읽는 color와 쓰는 attachment를 무계획하게 동일하게 두지 않는다. [Full Screen Pass 공식 문서](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/renderer-features/renderer-feature-full-screen-pass.html)

특히 URP 17.3 로컬 패키지 `Runtime/RendererFeatures/FullScreenPassRendererFeature.cs` 220~248행은 fetch 시 `activeColorTexture`를 별도 texture로 Blit한 후 그 source를 사용한다. 따라서 **Requirements의 Color로 요구하는 opaque texture**와 **Fetch Color Buffer의 현재 주입 시점 color**를 혼동하지 않는다. source에 흑섬이 포함되는지는 실제 injection 시점과 camera stack에 따라 검증해야 한다. Shader Graph나 VFX Graph를 도입하는 것만으로 이 pass 구조가 생기지는 않는다.

후속 H1에서는 참가자/local camera 필터링, HUD overlay 포함 여부, 연속 BF 중첩 길이를 따로 검증해야 한다. 현재 모든 카메라에 renderer feature를 일괄 적용하는 설계는 권하지 않는다. 이번에는 설정이나 feature를 변경하지 않았다.

### Frame Debugger — 먼저 쓰면 유용하나 이번에는 실행하지 않음

내장 도구로 URP draw 이벤트와 그 시점의 누적 화면을 볼 수 있다. 시간에 따른 파열감 자체를 판정하는 도구는 아니다. 실제 프레임을 안정적으로 재현한 후 이벤트를 확인해야 한다. [Unity Frame Debugger 공식 문서](https://docs.unity3d.com/6000.0/Documentation/Manual/FrameDebugger.html)

후속 조사 절차:

1. 기존 QA 미리보기의 seed71, 측면, age 50/100/150/200ms를 각각 재현한다. 실시간 연속 재생을 계속 흘려 놓으면 짧은 draw가 사라져 목록이 바뀌므로 한 자세를 유지하는 QA 상태가 필요하다. 현재 Preview의 `Sample(age)` 재사용을 우선하고 실제 전투 clock을 멈추지 않는다.
2. `Window → Analysis → Frame Debugger`에서 해당 Game camera의 opaque 종료 지점, 코어/가지/연기 Ink draw, 나중의 Light draw 순서를 확인한다. SRP batching과 실제 이벤트 표기 때문에 정확히 renderer 수만큼만 이벤트가 나온다고 약속하지 않는다.
3. 코어/가지 draw 직전·직후를 비교하여 가려짐이 opaque 캐릭터 때문인지, 앞 Ink의 깊이 기록 때문인지 찾는다. material의 ZWrite/Blend/Cull과 MPB `_Age/_Life/_Growth`를 가능한 범위에서 확인한다.
4. 검정과 빨강은 **같은 fragment pass**이므로 두 색을 이벤트 하나씩 떼어 볼 수 없다. 이벤트를 단계적으로 표시하는 것과 특정 레이어만 단독으로 켜는 것도 다르다. 후자는 QA renderer toggle/색 분리 미리보기가 필요하다.
5. C는 투명 영역이 alpha clip으로 빠지는지, 살아남은 반투명 격자가 뒤쪽 격자를 가리는지 조사한다. 가지의 정적 silhouette와 depth artifact를 시간축 문제와 분리한다.
6. 결과에 camera/seed/age/renderer/material을 붙여 기존 같은 시점 PNG와 연결한다. 내장 도구가 시각 문제를 찾는다는 것과 GPU 정점 셰이더를 한 줄씩 분석한다는 것은 구분한다.

### RenderDoc — 현재 도입 보류, 특정 GPU 의문이 남을 때만

현재 **Windows+D3D12는 RenderDoc의 지원 조합**이다. Unity가 지원 API에서 실행되어야 한다는 조건도 충족하는 방향이다. 다만 해당 GPU/드라이버/Unity 버전에서 실제 capture 성공을 시험하지 않았으므로 ‘연결 검증 완료’는 아니다. D3D11로 바꿀 근거는 없다. [RenderDoc 공식 지원 표](https://github.com/baldurk/renderdoc), [Unity RenderDoc 통합 문서](https://docs.unity3d.com/6000.0/Documentation/Manual/RenderDocIntegration.html)

- 추가 가치: 실제 draw의 vertex 입력/출력, bound resources/uniform, depth/blend state 및 render target을 분석하여 ‘수식대로 변형됐는데 clip/가림 때문에 안 보이는가’를 확인할 수 있다. shader source/debug 정보의 가용성은 API·컴파일 정보에 따라 달라지므로 모든 D3D12 shader의 소스 단위 stepping을 보장하지 않는다.
- 적합한 시점: Frame Debugger로 설명되지 않는 소거·깊이·굽힌 격자 overlap이 남거나, 실제 GPU 출력과 계산 예측이 어긋날 때. 33/50/100/150ms 같은 미리보기 고정 시점을 각각 capture하고 같은 seed의 PNG/age 표와 묶는다. GPU capture를 시간 성능의 무교란 측정으로 쓰지 않는다.
- 위험: Unity의 Load RenderDoc는 **graphics device를 reload**한다. 미저장 scene/asset을 사용자가 정리한 뒤 별도 승인 하에 실행해야 한다. capture가 앱/도구 창을 띄울 수도 있으므로 기존 포커스 요구도 고려한다. 이번에는 설치/Load/capture/API 변경을 전혀 하지 않았다.
- 지금 불필요한 이유: 성장 수식, topology, 시계, alpha를 소스로 이미 추적할 수 있다. RenderDoc를 설치해도 같은 가지를 왜 ‘나무’로 느끼는지의 지각 인과를 자동 판정하지 않는다.

### REA — 이번 작업에서는 불필요

공식 저장소는 native binary/JavaScript/Electron/.NET/웹의 동작·구조를 소스 없이 조사하도록 에이전트와 분석 도구를 연결한다고 설명한다. MCP 등록과 workflow 설정도 수반한다. 우리는 원본 C#/HLSL·프로필·메시 생성기를 이미 보유하고 있다. 이를 다시 바이너리로 역공학해 얻을 핵심 정보가 없고 GPU frame 진단 도구의 대체도 아니다. **이번 VFX에는 도입을 추천하지 않는다.** [REA 공식 저장소](https://github.com/morluto/rea)

향후 자신이 소유/인가받은 소스 없는 플러그인의 의존성 등을 조사할 별도 필요가 생길 때 재검토할 수 있다. 이번에는 설치·MCP 등록·외부 앱/게임 분석·decompile을 하지 않았다.

## 10. 독립 결론 — Q1~Q6

| 질문 | 답 |
|---|---|
| Q1 주원인 | 고정된 닫힌 분기 형상을 두 단계로 성장시키고 높은 불투명도로 유지하는 조합. 단독 원인이나 기여도 수치는 미확정 |
| Q2 Gemini 시간 가설 일치? | 부분적으로 강하게 일치. 실제 root Transform 성장 설명은 틀리지만 shader 정점 성장+clip 공개가 확인됨. 90%/3D 자체 실패 주장은 수용 불가 |
| Q3 시간 우선 실험 합리적? | 예. 이미 존재하는 한 성장 설정부터 고정 형상 A/B. 처음부터 Hold·소거·screen effect를 묶지 않음 |
| Q4 A/B/C 비교 계속? | 현재는 보류. 결과를 보존하되 변수 통제부터. 현재 시제품 중 자동 채택 없음 |
| Q5 첫 작은 실험? | QA 메모리 복제 프로필에서 성장 95→30ms만 변경. 다음은 필요 시 가지 수명 280→190ms만 분리 비교 |
| Q6 놓친 문제? | 복합 성장·늦은 실제 clip·33개 획, 부모/지류 anchor 불일치, 법선/rim/depth, 촬영 시계, 회전각-효과나이 결합, 미숙한 절차 atlas와 제작 방식의 혼동 |

**권장 순서:** 성장 단일변수 → 필요 시 본체 지속 단일변수 → 각도별 geometry/표면 분리 → 그 후 작화/제작 방식 선택 → 별도 H1 화면 연출. 첫 두 단계에 새 패키지나 역공학 도구는 필요 없다.

‘성장 문제가 실제 존재한다’에는 높은 확신이 있다. ‘성장을 빠르게 하면 충분한 흑섬이 된다’에는 아직 확신이 없다. 나뭇가지 형상과 재질 인상이 남을 가능성이 크므로 사용자의 원속도 검수에서 채택 여부를 결정해야 한다.

## 11. 미확정 사항과 QA 상태

- 이번에는 Unity Play Mode/테스트/Frame Debugger/RenderDoc를 실행하지 않았다. **새 CODEX VALIDATED 구현 완료나 USER VERIFIED 판정은 없다.** 분석 수치와 기존 테스트 결과는 다르다.
- 기존 REV1 최신 고유14 PASS는 마지막3회 케이스별 집계, Visual R&D 최신 고유5 PASS는5회 케이스별 집계라는 기록을 유지한다. 합쳐서 새 단일 전체 PASS라고 하지 않는다. 과거 실패/불완전 영상도 보존한다.
- 실제 화면상 길이·opacity·렌더링 픽셀별 소거, 첫 hit GPU 지연, 동시 다수 흑섬의 GPU ms/overdraw는 이번 계산으로 실측되지 않았다.
- 원본 게임의 실제 mesh/flipbook/particle/시간 제어 내부는 모른다. 30fps 녹화의 반복 프레임을 원작의 정확한 authored hold로 단정하지 않는다.
- 원속도에서 어떤 후보가 ‘주력 파열’로 느껴지는지, 어느 정도의 잠깐 유지가 좋은지는 사용자 검수가 필요하다. 정지 프레임 유사도/정점 수/자동 PASS로 대신하지 않는다.

## 12. Git와 기존 작업 보호

시작: 브랜치/HEAD는 위와 같고 staged0 / tracked dirty7 / untracked33이었다. 기존 변경은 `.gitignore`, handoff/task/reference 문서, 기존 TestRunner/Match 연결, 사용자 `unity/unity.slnx`와 미커밋 H0/REV1/R&D 파일들이다. 이번에 이를 수정하거나 stage하지 않았다.

분석 중 기존 dirty/untracked 파일40개의 SHA-256 baseline을 잡아 보고서 작성 후 재대조했고 **40개 모두 불변, 유실0**이었다. 이 검사는 기존 미커밋 파일 보존 확인이며 전체 ignored QA·원본의 새 전수 해시 검사라고 확대하지 않는다. QA/레퍼런스는 읽기만 했고 변환 파일을 그 위에 기록하지 않았다.

이번 추가 파일은 이 보고서1개다. 최종 확인 상태는 **staged0 / tracked dirty7 / untracked34**, 브랜치/HEAD 동일이다. `git diff --check` 오류는 없었고 기존 Match 파일에 대한 LF→CRLF 경고만 있었다. 기존 scene/prefab/Data Asset/Gojo/F aura/Packages/ProjectSettings/`.meta`는 편집하지 않았다. fetch/pull/merge/checkout/reset/clean/commit/push와 새 worktree 생성도 없다.

실제 수정 방향은 사용자 검토 후 별도 요청에서 결정한다. 이 문서는 실험 설계까지이며 구현을 시작하지 않는다.
