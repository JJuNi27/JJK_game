# Hollow Purple 3차 baseline — 최종 검증 기록

2026-09-15 KST. **LOCAL / CODEX VALIDATED / PENDING USER VISUAL REVIEW**.

사용자의 마지막 지시에 따라 현재 3차 구현을 고정했다. 마무리 요청 이후 Purple 본체·셰이더·카메라·게임플레이 코드를 추가 수정하지 않았으며, 진행 중이던 최종 regression 결과 확인, 기존 렌더 QA, 보고서 정리만 수행했다. 이 기록은 1·2차 보고서의 이전 타이밍보다 우선한다.

1. **기존 결과에서 가장 큰 문제**

   빠른 창·혁 융합과 짧은 hold, 작은 소환 구체, 성운처럼 번지는 Purple 윤곽, 같은 그림의 반전에 가까운 임팩트, 종점 연출 부재였다. 기존 레이어·카메라 lease·발사 origin·상흔 구조 위에 3차를 구현했다.

2. **Blue summon 변경**

   Purple 소환용 Blue만 확대했다. 3D 표면 난류, 외부에서 중심으로 휘어 들어오는 나선, 안쪽으로 이동하는 입자를 사용한다. 독립 기술 Blue의 기존 로컬 변경은 그대로 보존했다.

3. **Red summon 변경**

   Purple 소환용 Red에 작은 백색 코어, 진한 적색 질량, 바깥으로 퍼지는 끊어진 압력 arc와 파편 흐름을 적용했다. Blue와 반대 방향의 운동을 유지한다. 독립 기술 Red는 이 패스에서 수정하지 않았다.

4. **2초 fusion timeline**

   0–0.45초 Blue 단독 → 0.45–1.10초 Red 등장·대치 → 1.10–2.00초 융합이다. 서로 다른 곡선 흐름과 연결 방전, 보라색 오염, 두 소환체의 수축을 거쳐 완성 위치에 Purple이 생긴다. VFXLab의 기존 0.62초 준비 구간은 이 공통 시퀀스 이전이다.

5. **Purple body / silhouette 변경**

   기존 volume 적분 셰이더를 유지하면서 구형 질량의 경계 변위를 줄이고 표면 난류를 분리했다. 어두운 magenta/deep violet 질량과 작은 백색 코어, 구 표면을 따라가는 리본, 앞뒤 깊이가 다른 방전을 사용한다. 전역 bloom이나 사용자 Scene을 바꾸지 않았다.

6. **3초 charge timeline**

   완성 후 0–0.8초 안정화, 0.8–1.8초 방전 상승, 1.8–2.6초 밀도·왜곡·카메라 압축, 2.6–3.0초 일부 운동과 발광 억제다. 기본 공통 시계의 2–5초에 해당한다. 한글 Inspector에서 소환·융합·충전 시간과 소환체/종점 크기를 조절할 수 있다.

7. **Multi-beat impact frame sequence**

   5.000–5.1333초, 60fps 기준 8프레임이다. 각 2프레임씩 가로 절단·속도선 → 중심을 옮긴 흑백 방사형 → Purple과 추상적인 시전자 실루엣 → 백색·보라 노출 컷으로 전환한다. 동일 그림을 반전하는 구조가 아니다. 프레임 도약 시에도 짧은 연출 창을 확보하고 이후 overlay를 정리한다.

8. **Caster camera choreography**

   소환 대각 구도 → 융합 시 낮고 가까운 구도 → 충전 중 느린 이동·후반 push-in → 임팩트 동기 충격 → 발사 follow-through → 원래 위치·회전·FOV 복귀다. 기존 `DomainCameraOverride` lease를 재사용한다. 소유권을 잃으면 Purple의 화면 연출을 종료하며 다른 연출의 lease를 해제하지 않는다.

9. **Observer view 대응**

   실제 follow/preview 대상이 시전자일 때만 자동으로 caster camera를 선택한다. 관련 없는 MainCamera에는 강제 이동·FOV·fullscreen overlay가 없다. 명시적인 observer 경로도 지원한다. 관찰자는 소환·충전·발사·종점의 world VFX를 본다.

10. **Release**

    5.1333초에 완성 위치에서 수평으로 발사한다. 이전 `ReleaseOrigin`과 초기 궤적 연속성을 보존했다. 피해 예약과 presentation은 같은 시작 시각 및 발사 시간을 사용한다. 새 캐릭터 모션은 만들지 않았다.

11. **Travel**

    완성된 구체의 크기·코어·표면 흐름·방전·공간 왜곡을 유지한다. 기존 불규칙한 경로 상흔과 짧은 trail을 남긴다. 기본 48m 이동 시간은 1.6초다.

12. **Terminal hit / range explosion**

    첫 적중 또는 최대사거리에서 본체가 압축된 뒤 백색·보라 core, 불규칙한 방사형 방전, 끊어진 압력 shell, 공간 왜곡으로 파열된다. 실제 표면이 있으면 짧은 먼지와 파편이 나온다. 지면이 없는 최대사거리에서는 허공에 지면 파편을 만들지 않는다. 폭발 및 잔류는 시퀀스가 소유하고 취소/종료 때 정리한다.

13. **Gameplay 값 변경 여부**

    **사용자가 명시적으로 선택한 규칙 변경:** 첫 적중에서 본체가 종료되고 뒤쪽 적의 피해도 중단된다. 기존 관통 규칙을 유지한 상태가 아니다. 대상은 기존 시전 시 corridor snapshot에서 가까운 순서로 처리하며, 살아 있는 첫 접촉에서 종료한다. 방어 판정으로 피해가 적용되지 않아도 접촉에서 본체가 종료된다. 새로운 실시간 projectile collision 또는 추가 광역 폭발 피해는 도입하지 않았다.

    damage **55**, radius **3.2**, range **48**, push **34**, stun **1**, energy **45**와 gameplay Data Asset은 보존했다. 소환/충전 취소 후 예약 피해가 남지 않도록 coroutine을 정리한다. 적중 이벤트는 해당 시전의 시작 시각으로 구분하며 Dispose 시 구독을 제거한다.

14. **수정 파일**

    아래는 **3차 전체 패스**의 구현/검증 변경 범위다. 마지막 마무리 요청 이후의 변경은 이 문서와 handoff/current task, QA 산출물뿐이다.

    | 범위 | 파일 (`unity/Assets/` 기준) |
    |---|---|
    | 설정·한글 Inspector | `Scripts/Player/GojoPolishSettings.cs`, `Resources/VFX/GojoPolishSettings.asset`, `Editor/KoreanCombatInspectorEditors.cs` |
    | 시퀀스·본체·화면 연출 | `Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs`, `PurpleEnergyBody.cs`, `PurpleReleasePresentation.cs` |
    | 첫 접촉 종료 | `Scripts/Player/GojoTechniqueChainController.cs` |
    | 카메라 소유자 확인 | `Scripts/Camera/DomainCameraOverride.cs`, `SimpleCameraFollow.cs`, `Scripts/Dev/VFXLab/VfxLabOrbitCamera.cs` |
    | Lab 단계 표시 | `Scripts/Dev/VFXLab/VfxLabPreviewSequence.cs` |
    | 기존 Purple 셰이더 수정 | `Resources/VFX/HollowPurpleVolume.shader`, `HollowPurpleImpact.shader` |
    | 새 helper | `Scripts/Player/PurpleFusionIngredient.cs`, `PurpleTerminalBurst.cs` 및 각각 `.meta` |
    | 새 shader | `Resources/VFX/HollowPurpleIngredient.shader`, `HollowPurpleDust.shader` 및 각각 `.meta` |
    | 검증 | `Editor/AstraCombatVfxTests.cs` |
    | 문서 | `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md`, 이 보고서 |

    3차 이전부터 dirty였던 Scene, ProjectSettings, 독립 Blue/Red, 기타 테스트·문서·모델 작업은 이 목록에 포함하지 않는다. 전체 dirty tree를 이번 패스가 만든 변경으로 취급하지 않는다.

15. **새 shader / helper 여부**

    이 패스는 위의 helper 2개와 shader 2개를 추가했다. 기존 Purple volume/filament/impact/scar와 EnergyBody/ReleasePresentation/TravelAftermath 기반을 유지했다. 레퍼런스 동영상이나 사용자 texture/model/audio를 런타임 Asset으로 복사하지 않았다.

16. **Compile / test**

    최종 `purple-third-final.xml`: **37/37 PASS, 실패 0, skip 0**. 로그의 C# 및 shader compile error **0**. 열린 사용자 Editor와 분리한 `unity/Logs/PolishQA/Project`에서 Unity 6000.3.20f1로 실행했다.

    범위는 동일 시전자 반복 실행·구독 제거, no-target/lock 방향, 첫 적중 및 뒤쪽 피해 차단, 최대사거리 종점, 소환/충전 취소와 지연 피해 정리, origin 연속성, 카메라 복귀·소유권 이전, overlay·terminal·material 정리, Purple scar, 공유 camera release/beat clock, Combat Data/한글 Inspector다. 프로젝트 전체의 모든 테스트를 실행했다는 뜻은 아니다.

    중간 실제 전투 검사 1건은 기존 CombatMVP 적이 시험 표적보다 앞에 있어 후방 표적의 피해 기대값이 맞지 않았다. 테스트에서 두 시험 표적을 분리한 뒤 재검증했다. 최종 실행은 전부 통과했다. 1차 이력의 기존 audio `skill1.voice` 연결 검사 실패는 이번 범위가 아니며 수정하지 않았다.

    [최종 테스트 XML](../unity/Logs/AstraCombatQA/purple-third-final.xml) · [Unity 로그](../unity/Logs/AstraCombatQA/purple-third-final.log)

17. **Visual QA**

    VFXLab과 CombatMVP에서 실제 Unity PNG **305장**을 생성했다. 0–5.5초는 0.1초 간격, 임팩트는 1/60초 간격으로 샘플링하고 종점 추가 프레임을 확인했다. 주요 caster/observer/terminal contact sheet와 원본 대표 프레임을 직접 검토했다. 창 단독→혁 대치→Purple 완성, 구형 윤곽, 네 임팩트 구도, 이동 중 본체 유지, 종점 압축·파열·잔류를 확인했다. 실제 첫 적중 장면에서는 지면 먼지와 파편도 확인했다.

    [시전자 단계 모음](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/PurpleThird_Caster_Stages.jpg) · [관찰자 단계 모음](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/PurpleThird_Observer_Stages.jpg) · [종점 모음](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/PurpleThird_Terminal.jpg)

    70프레임씩 연속으로 검토한 전체 시퀀스 시트: [VFXLab 시전자](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/PurpleThird_Dense_VFXLab_Caster.jpg), [VFXLab 관찰자](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/PurpleThird_Dense_VFXLab_Observer.jpg), [CombatMVP 시전자](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/PurpleThird_Dense_CombatMVP_Caster.jpg), [CombatMVP 관찰자](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/PurpleThird_Dense_CombatMVP_Observer.jpg).

    [시전자 샘플 재생](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/PurpleThird_Caster_sampled.mp4) · [관찰자 샘플 재생](../unity/Logs/AstraCombatQA/CODEX_PREVIEW/PurpleThird_Observer_sampled.mp4)

    MP4는 10Hz 캡처와 60Hz 임팩트 원본 프레임을 원래 시퀀스 시간대로 배치한 **샘플 재생 자료**다. 실시간 60fps 플레이 녹화나 성능 측정 결과가 아니다. 이번 마무리에서 새로운 시각 조정을 하지 않았다.

18. **USER VERIFIED가 필요한 부분**

    정상 플레이에서의 3초 충전 긴장감, 임팩트 강도, Purple의 질량감, 카메라 구도, 종점 폭발의 만족도는 사용자 Play Mode 확인이 필요하다. 자동 테스트와 CODEX frame review만으로 production quality 또는 USER VERIFIED를 선언하지 않는다. 현재 구현을 다음 비교의 고정 baseline으로 남긴다.

19. **Git status / baseline 보존**

    branch `feat/gojo-blue-screen-distortion`, LOCAL HEAD `a9c6ccf9c09b2effdd559e83e865296e45f600b2`. cached origin ref와 동일하며 이번 세션에서 remote 조회/fetch를 하지 않았다. **tracked modified 20개 / untracked 41개 / staged 0개**이며, 기존 사용자 변경을 포함한 dirty tree다. 전체 상태는 [최종 git 상태](../unity/Logs/AstraCombatQA/purple-third-git-status.txt), baseline 해시는 [고정 manifest](../unity/Logs/AstraCombatQA/purple-third-frozen-baseline.json)에서 확인한다.

    3차 시작 스냅샷에서 이번 수정 범위를 제외한 보호 파일 **49/49 해시가 일치**한다. 사용자 Scene·기존 독립 Blue/Red·모델/오디오 관련 파일·gameplay Data·레퍼런스 원본을 보존했다. 고정 manifest의 Assets 49개도 마무리 중 변경되지 않았으며, 현재 구현은 최종 테스트 사본과 일치한다. `.unity`에 남아 있는 기존 trailing whitespace는 정리하지 않았다.

20. **Commit / push 없음 확인**

    `reset`, `clean`, `restore`, 덮어쓰기 checkout, staging, commit, push, merge를 실행하지 않았다. 이 baseline은 **LOCAL working tree와 검증 기록**이며 Git checkpoint commit은 아니다.
