# Hollow Purple presentation reauthor — LOCAL / CODEX 검증

## 2차 사용자 피드백 반영 — 2026-09-14, 아래 1차 기록보다 우선

이번 피드백은 흑백 프레임의 짧은 인지 시간, 빠른 buildup, 밝은 성운 형태의 본체였다. **Purple만 수정**했으며 기존 카메라/임팩트 그래픽/레이어 구조와 발사 origin을 유지했다.

| 순서 | sequence 시간 | 내용 |
|---|---|---|
| 창 단독 등장 | 0.00–0.28초 | Blue만 먼저 나타나 수렴 |
| 혁 등장·대치 | 0.28–0.52초 | Red가 나중에 커지며 두 힘이 분리되어 대치 |
| 융합 | 0.52–0.82초 | 기존 융합 궤적과 완성 위치로 결합, 카메라 측면 전환 |
| 자 완성 hold | 0.82–1.14초 | 기존 0.32초 유지, 밀도와 마지막 압축 |
| 임팩트 | 1.14–1.2067초 | 60fps 기준 4프레임: 검정 진입 1 → 반전 최대 대비 2 → 보라/백색 복귀 1 |
| 발사 | 1.2067초부터 | 완료 위치에서 직진, camera follow-through와 복귀 |

- 단계 시간과 임팩트 3–5프레임 선택을 `GojoPolishSettings`와 한국어 Inspector에 노출했다. VFXLab 단계 표시도 한국어로 맞췄다. VFXLab의 기존 0.62초 준비는 위 공통 sequence 이전이다.
- 전용 volume의 외층 밝기·haze·리본 발광을 낮추고 흡수 밀도를 높였다. 기본 density 1.15→1.8, emission 1→0.65, haze 0.45→0.06. 큰 soft cloud를 억제하고 불규칙한 경계와 진한 magenta/violet 질량을 만든다. 전역 bloom/다른 기술의 후처리 설정은 바꾸지 않고 Purple의 발광량을 줄였다.
- 높은 밀도에 코어가 가려지는 첫 렌더를 확인한 후, 작은 비정형 백색 중심만 강한 radiance로 보정했다. 외층을 함께 밝히지 않는다. 적분 32회와 고정 샘플 위치로 pixel grain도 줄였다. 분기 방전과 깊이 구조는 유지한다.
- damage/radius/range/push/stun/energy 수치는 유지한다. **적중 시점은 늘어난 발사 준비와 함께 늦춘다.** 피해 coroutine과 VFX가 같은 `PurpleReleaseTime`을 사용한다. 별도 C# merge 지연 필드를 제거해 두 시계가 어긋나지 않도록 했다.
- 보호된 formation/hold 지침 중 이번 사용자가 명시적으로 요청한 등장 순서·융합 속도를 변경했다. 0.32초 완성 hold와 완성 위치/발사 축은 보존했다.
- 첫 집중 검증 `purple-second-01.xml`: **5/5 PASS**. 최종 `purple-second-final.xml`: **33/33 PASS**, C# 및 shader compile error 0. Blue만 먼저 등장/Red 이후 등장, 4프레임의 `진입 1 → 반전 2 → 복귀 1`, hold/release 경로, 피해 지연, 실제 CombatMVP 시전, VFXLab 취소, camera/material cleanup, scar 및 공유 Data/한글 Inspector를 검증했다. 2차 렌더를 직접 확인했다. 기존 오디오 연결 검사 실패는 1차 이력으로 남으며 이번 범위에서 수정하지 않았다.

렌더: [어두운 hold 측면](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/VFXLab_PurpleSecond_0.980_side.png), [낮은 각도 release](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/VFXLab_PurpleSecond_1.270_low.png), [창/혁 대치](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/VFXLab_PurpleSecond_0.380_shot.png). 기존 1차 `PurpleProduction` 캡처는 비교용으로 남아 있다. 2차 파일 접두사는 `PurpleSecond`다.

**LOCAL / CODEX 검증 / PENDING USER VISUAL REVIEW**. 사용자가 2차 수정의 단계감·프레임 인지·질량감을 평가하기 전까지 USER VERIFIED로 변경하지 않는다. commit/push 없음.

## 1차 구현 및 검증 이력

2026-09-14. 사용자의 기존 결과 평가는 **4/10, PROTOTYPE BASELINE**이다. 아래 변경의 최종 시각 등급은 아직 **PENDING USER VISUAL REVIEW**다. Blue/Red의 이전 로컬 변경은 보존하고, 이번 재작업은 Purple에 집중했다.

1. **이전 품질의 실제 원인**

   기존 Unity 렌더에서 매끈한 동심 구체, 불투명한 분홍 표면과 분리된 흰 원, 거의 같은 성격의 가는 번개가 두드러졌다. hold 내내 비슷한 밝기였고 방출의 화면 문법이 약했다. 상흔은 일정한 폭의 LineRenderer를 반복해 길게 이어 붙였으며, vertex alpha가 지원되지 않는 재질에서는 fade 표현도 불명확했다.

   [이전 측면 렌더](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/VFXLab_Purple_0.560.png) / [새 측면 hold](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/VFXLab_PurpleProduction_0.450_side.png)

2. **세 레퍼런스에서 가져온 원칙**

   로컬 영상을 직접 디코딩해 확인했다. 원본 미디어를 수정·이동하거나 런타임 Asset으로 가져오지 않았다.

   | 독립 레퍼런스 | 직접 확인한 구간 | 적용한 원칙 |
   |---|---|---|
   | `gojo_mod_reference.mp4` | 24–29.6초, 39–47초 | Blue/Red의 반대 운동, 합성 후 큰 에너지 질량, 근접과 원경의 크기 대비, 짧은 이동 잔류 |
   | `strikeborn_reference.mp4` | 13.43–14.2초, 45.0–46.6초 | 짧은 흑백·색 대비 전환, 절정 후 광량 회수, 표면 반응과 잔흔으로 충격 전달 |
   | `purple_selfdestruct_reference.mp4` | 1.23–1.62초, 6.4–9.0초, 17.73–18.50초의 개별 프레임, 이후 19–22초 | 좁은 백색 중심과 다층 보라 질량, 전경 번개, 카메라 거리 변화, 붓질 형태의 반전 프레임 |

   추가 영상은 **30 fps / 715프레임 / 23.833초**다. 특히 프레임 **536–545 (17.867–18.167초)**에서 긴 백색 횡방향 도형 → 붉은 반전 → 보라 방사 구성 → 백색 실루엣 전환을 확인했다. 레퍼런스의 자폭 반경·장시간 whiteout·캐릭터 클로즈업은 일반 전투 Purple에 그대로 이식하지 않았다. 색과 화면 자산은 복사하지 않고 자체 절차 그래픽을 작성했다.

3. **임팩트 프레임 구현**

   `HollowPurpleImpact.shader`가 비대칭 코어 실루엣, 끊어진 윤곽, 굵기가 달라지는 방사선과 횡방향 절개를 생성한다. 카메라에 연결된 ScreenSpaceCamera Canvas가 방출 지점을 투영한다. 기본 50 ms 창에서 검정/백색 → 반전 → 백색·보라 섬광의 3단계를 사용한다. 60 Hz 기준 약 3프레임이며 실제 프레임 수는 실행 주기에 따른다. 화면 전체를 장시간 필터링하지 않는다. 방출 시점을 한 번에 건너뛴 hitch에는 첫 방출 관측에서 짧은 창을 시작해 그래픽 누락을 막는다.

   [검정 프레임](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/VFXLab_PurpleProduction_0.561_shot.png) / [반전 프레임](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/VFXLab_PurpleProduction_0.579_shot.png)

4. **카메라 choreography**

   새 Camera를 만들지 않는다. 기존 `DomainCameraOverride.Acquire/Pose/Release`를 사용한다. 기존 formation 이후 fusion 중 옆으로 이동하고 hold에서 낮은 시점을 만든다. 마지막 짧은 push-in/FOV 압축 → 방출 impulse/FOV 확대 → 약 0.65초 follow-through와 복귀다. Domain 등 다른 소유자가 있으면 점유하거나 화면 그래픽을 끼워 넣지 않는다. 원래 target, orbit 설정은 변경하지 않는다. 취소·비활성화·사망·파괴에서 lease와 overlay를 정리한다.

5. **core / mid / outer / haze**

   `HollowPurpleVolume.shader`는 카메라 광선을 볼륨 내부로 28회 샘플링한다. 조밀한 white-hot 중심, noise/flow로 휘는 magenta 질량, 구멍이 있는 violet/blue 외층, 낮은 밀도의 넓은 haze를 깊이에 따라 합성한다. 매끈한 색 구체와 여러 동심 껍질을 제거했다. 배경 왜곡에는 기존 localized distortion 셰이더를 재사용하며 그 파일의 사용자 변경은 그대로 보존했다.

6. **lightning / ribbon 개선**

   서로 다른 기울기와 길이를 가진 동적 리본 mesh 7개, 최대 24개의 주/분기 방전, 작은 streak 18개를 제한된 수로 생성한다. 주 번개와 분기 번개는 실제 접점에서 이어진다. 폭, 깊이, 발광, 생멸 주기가 다르다. 리본은 곡면 띠이고 전기는 꺾이는 궤적이어서 같은 선 패턴의 반복을 피한다. 공유 전용 filament 재질이 vertex alpha와 가장자리 감쇠를 실제로 반영한다.

7. **hold와 release**

   merge **0.24초**, hold **0.32초**는 유지한다. hold 초반 난류 → 밀도 증가 → 마지막 약 35–64 ms의 흐름·광량 억제 → 방출 peak로 구성했다. 방출 직후 약 33 ms 동안 에너지 내부 흐름만 고정한다. 투사체 위치·피해 시계·Time.timeScale을 멈추거나 새 gameplay delay를 넣지 않는다. 기존 feedback director의 합성 시작 시점 Purple flash/FOV/hit-stop을 제거해 정확한 방출 시점의 단일 소유자로 정리했다.

8. **travel**

   방출 spike는 지수 감쇠하며 내부 flow, 휘어진 띠, 분기 방전과 길어진 국소 왜곡이 움직임을 이어간다. 이미 수정된 완성 hold 위치에서 그대로 직진한다. 초기 holdOffset 제거로 아래로 꺾이거나, 이동 거리의 10%를 순간이동하는 동작을 되살리지 않았다.

   [실제 CombatMVP 시전 중 비행](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/CombatMVP_Purple_REAL_travel.png)

9. **scar**

   일정한 폭의 긴 선 대신 최대 64개의 bounded patch를 사용한다. 실제 진행 거리만큼 차례로 생성하고 RaycastNonAlloc으로 표면을 찾는다. 각 구간은 폭·좌우 위치·길이가 다르고 전용 셰이더가 가장자리·불투명도·밝은 파편을 끊는다. 어두운 중심, 불규칙한 보라 틈, 개별 발생 시점에 따른 소멸을 사용한다. 폭의 최대 범위는 현재 visible diameter와 기존 `purpleScarWidthMultiplier`를 따른다.

   [가까이 본 지면 상흔](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/CombatMVP_PurpleProduction_1.100_scar.png)

10. **aftermath**

    본체가 사라지면 volume, 리본, 본체 번개를 끈다. 기존 **1.8초** 잔류 설정 안에서 구간별 상흔과 최대 8개의 드문 낮은 방전이 소멸한다. 잔류 방전은 본체보다 어둡고 느리며 별도 impact나 피해를 만들지 않는다. 균일한 전체 경로 fade 대신 먼저 생긴 부분부터 다르게 사라진다.

11. **gameplay 보호**

    이번 Purple 재authoring에서 gameplay 숫자는 변경하지 않았다. radius **3.2**, damage **55**, range **48**, hit stun **1**, push **34**, energy cost **45**, launch duration **1.6** 및 기존 Profile ownership을 보존했다. 앞선 pass의 수평 방향/공통 ReleaseOrigin 수정도 유지한다. 게임플레이 Data Asset을 포함한 보호 파일 28개의 SHA-256은 작업 시작 기록과 동일하다.

12. **이번 Purple에서 변경한 파일**

    - `Assets/Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs`: 레이어/timeline/취소 통합, 기존 발사 origin 보존.
    - `Assets/Scripts/Player/PurpleTravelAftermath.cs`: 상흔과 저밀도 잔류 재authoring.
    - `Assets/Scripts/Player/GojoPolishSettings.cs`, `Assets/Resources/VFX/GojoPolishSettings.asset`: 전용 art/camera 조절값.
    - `Assets/Editor/KoreanCombatInspectorEditors.cs`: 모든 새 조절값에 명시적 한국어 SerializedProperty label.
    - `Assets/Scripts/Core/ProductionCombatFeedbackDirector.cs`: 합성 시작의 중복 Purple feedback 제거. 앞선 Blue 변경 보존.
    - `Assets/Editor/AstraCombatVfxTests.cs`: 실제 피해/반복/취소/리소스/렌더 검증.
    - `Assets/Editor/GojoFourthPolishTests.cs`: 기존 scar width 검증을 새 불규칙 patch 표현에 맞게 갱신.
    - 이 보고서와 `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md`의 최신 상태 기록. 아래 신규 helper/shader와 `.meta`.

    경로는 `unity/` 기준이다. `GojoTechniqueChainController`, Blue/Red 구현 및 VFXLab Blue 타이밍 수정은 앞선 pass의 작업이며 이번 Purple 집중 중 추가 수정하지 않았다.

13. **새 shader / material / helper**

    - `PurpleEnergyBody.cs`, `PurpleReleasePresentation.cs`.
    - `HollowPurpleVolume.shader`, `HollowPurpleFilament.shader`, `HollowPurpleImpact.shader`, `HollowPurpleScar.shader`.
    - 새 패키지, 외부 texture, 모델 또는 reference asset 이식 없음. 전용 재질과 mesh는 sequence 생성 시 만들고 정리 시 파괴한다. 별도 영구 `.mat` Asset은 만들지 않았다.
    - 사용자 조절값은 GojoPolishSettings에 한국어로 노출한다. 잔여 시간 곡선/기본 shot 거리·높이/절차 패턴 상수는 presentation helper에 남아 있는 **B: PRESENTATION tuning**이다. gameplay migration 누락으로 분류하지 않는다. Mesh topology, 수치 보호 clamp는 **C: KEEP IN CODE**다.

14. **compile / test 결과**

    사용자가 연 Unity를 건드리지 않고 `unity/Logs/PolishQA/Project` 복사본에서 Unity **6000.3.20f1 / URP**로 실행했다. C# compile error 0, 신규 shader compile error 0.

    - `purple-02.xml`: Purple 집중 4/4 PASS.
    - `purple-03.xml`: 렌더 PASS, 실제 전투 피해/정리는 정상. VFXLab 복구 기대값이 Begin 이전 위치를 사용해 실패했고, 기존 앵커 이동 이후 lease 획득 위치를 기준으로 검사하도록 수정했다.
    - `purple-final-regression.xml`: Purple + 공유 Data/Inspector/beat 회귀 **52/53 PASS**. Purple 5개 검사와 shared Purple gameplay 소비 검사 모두 PASS.
    - 실패 1개: 기존 `TrackAInspectorProfileTests.GojoProfileIsEditModeAssetAndPreservesSceneValues`, line 168. `Gojo Combat Audio Profile.asset`의 `skill1.voice`는 **HEAD 및 작업 시작부터 fileID: 0**인데 기존 로컬 검사는 Gojo_Blue.ogg 연결을 기대한다. Purple 변경으로 생긴 차이가 아니며 이 범위에서 오디오 Asset/사용자 테스트를 수정하지 않았다.
    - `purple-release-hitch.xml`: 방출 hitch 누락 방지 / camera 취소·재질 정리 / 실제 CombatMVP 시전 / VFXLab preview 취소 **2/2 PASS**. 최종 코드 기준.

    로그/XML: `unity/Logs/AstraCombatQA/`. 전 프로젝트의 모든 Domain/Blue/Red 테스트를 통과했다고 주장하지 않는다. 이전 pass의 Red surface fixture 실패와 Red 링 시각 개선도 이번 범위에서 완료한 것으로 취급하지 않는다.

15. **visual inspection**

    VFXLab/CombatMVP에서 단계별 52장과 실제 CombatMVP 시전 2장을 생성했다. front/side/low, choreography 카메라, travel와 scar를 직접 확인했다. 단계별 캡처는 Unity Play Mode의 sequence 시계를 수동 샘플링한 실제 3D 렌더이고, REAL 표시는 production ActivatePurple 경로를 시간 진행시키며 캡처한 결과다.

    첫 결과의 과도한 분홍 뭉침을 발견해 중간층 밀도/발광과 백색 중심의 투과를 조정했다. 리본 끝점의 소수 승 계산이 NaN을 만들던 문제를 실제 assertion으로 발견하고 clamp로 수정했다. 이후 생성 경고와 resource cleanup 검사가 통과했다. 기존의 매끈한 껍질, 흰 원, 선 묶음과는 실루엣과 화면 전환이 명확히 달라졌다. 이것은 Codex의 렌더 관찰이며 최종 사용자 시각 승인이 아니다. GPU 프레임 시간/목표 하드웨어 성능 인증은 하지 않았다.

16. **사용자가 Unity에서 확인할 장면**

    VFXLab에서 Hollow Purple을 연속 재생한다. formation → hold 마지막 억제 → 짧은 흑백 반전 → release를 정상 속도로 먼저 본다. 카메라 옆/정면/낮은 각도는 한국어 설정의 `자 짧은 카메라 연출 사용`을 잠시 끄고 수동 orbit으로 비교할 수 있다. 검토 후 원래 true 설정으로 복구한다.

    CombatMVP에서 가까운 적 락온, 멀리 있는 적 락온, 락온 없는 시전을 비교한다. 보이는 origin과 피해 경로의 높이, camera follow-through, 진행 방향 및 상흔을 확인한다. VFXLab에서는 hold 중 취소하고 다음 기술을 실행해 화면 그래픽과 카메라가 남지 않는지 본다.

17. **아직 필요한 USER VERIFIED**

    기존 4/10에서 충분히 큰 시각 도약인지, 50 ms 그래픽의 강도, 내부 질량/번개의 밀도, 카메라 움직임의 플레이 적합성, scar의 형태와 여운. Blue/Red의 추가 품질 작업과 전체 전투 VFX 최종 방향은 완료 승인으로 바꾸지 않는다.

18. **Git 상태**

    LOCAL working tree에만 반영했다. 브랜치 `feat/gojo-blue-screen-distortion`, 기준 HEAD `a9c6ccf9c09b2effdd559e83e865296e45f600b2`. cached origin ref는 같은 commit이며 새 fetch로 remote 최신 상태를 확인한 것은 아니다. index에 이번 변경을 올리지 않았다. 기존 dirty Scene/ProjectSettings/Blue distortion, 사용자 문서/스크립트/테스트와 이전 Blue/Red 수정은 보존했다. `git diff --check`의 기존 VFXLab Scene 공백 경고는 사용자 Scene을 고쳐 없애지 않았다. 이번 코드/설정/Editor 대상 검사는 통과했다.

19. **금지 작업 미실행**

    reset / clean / restore / 덮어쓰기 checkout / git add . / commit / push / merge를 실행하지 않았다. 사용자 Scene·Animator·FBX·LocalModels·LocalAudio·레퍼런스 원본을 변경하지 않았다.
