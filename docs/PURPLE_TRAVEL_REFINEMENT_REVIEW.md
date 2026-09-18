# Purple Release → Travel refinement

2026-09-18 — **LOCAL / CODEX VALIDATED / PENDING USER VISUAL REVIEW**.

기존 D2-R3 + OuterR2 Production 통합 위에서 Release 이후 외부 이동 표현만 추가했다. 본체 shader/core/surface, Charge OuterR2, gameplay, canonical sequence, camera, terminal explosion은 수정하지 않았다. Production 로컬 검수 후보이며 사용자 시각 승인을 기다린다.

실제 참고 파일은 `_local_refs/videos/고죠 무라사키 날리는 장면.mp4`다. 약 5.9~6.4초의 발사에서 보이는 짧은 공간 잔류, 넓은 마젠타 wake, 뒤쪽으로 흩어지는 외부 에너지를 참고했다. 레퍼런스를 Unity Asset으로 복사하지 않았다.

## 결과와 구현 범위

| 항목 | 구현 및 실제 확인 |
|---|---|
| 1. World-space residue | 현재 직선·등속 경로에서 `-velocity × min(particle age, release age, 0.14s)`를 local 좌표로 역변환해 발생 공간의 이동을 상쇄한다. PS 전체를 World space로 전환하지 않았다. |
| 2. Local / partial inheritance | 모든 PS의 simulation space는 Local 유지. 외향 lightning, 작은 spark 슬롯의 1/3, Hero fragment 1개 슬롯은 짧은 world anchor를 사용한다. 나머지는 15% velocity inheritance에 해당하는 잔류 보상이다. 후자는 0.14초 뒤 보상 거리가 고정되어 본체와 함께 이동한다. 내부 R3 arc는 그대로다. |
| 3. Release Burst | Release 이후 0.035초에 걸쳐 반응을 켜고, `1 + 0.8 × exp(-releaseAge / 0.18)`로 wake/outer deformation peak를 감쇠한다. 초기 큰 뒤쪽 aura가 이후 줄어든다. 기존 flash·흑백 impact·release timing은 그대로다. 별도 gameplay burst는 없다. |
| 4. Travel Wake | 뒤쪽에만 존재하는 3D additive plasma volume. 월드 좌표 noise와 불규칙한 밀도 절단으로 짧은 덩어리가 바뀐다. 72회 raymarch, 화면 중심 본체 차폐, scene depth 차폐를 사용한다. 얇은 ribbon이나 긴 legacy wake를 다시 켜지 않았다. |
| 5. 길이 / 수명 | 본체 뒤 표면부터 최대 **1.35R**, 전체 폭의 이론적 상한 **2.24R**. 현재 정상 travel의 R≈3.44m에서 길이 약 **4.64m**, 폭 상한 약 **7.70m**. 실제 보이는 밀도는 이 상한 안에서 끊어진다. 잔류 particle은 최대 **0.14초**, 관측 지연 거리 **4.199781m**. Wake는 고정 수명 particle이 아니라 이동하는 유한 volume이며, 30m/s에서 길이 통과 시간은 약 **0.155초**다. terminal/cancel 시 함께 제거된다. |
| 6. Halo stretch | 원래 Charge shader는 유지하고 travel 전용 복사 shader만 전환한다. 이동 방향 앞쪽은 압축하고 뒤쪽 반경·흐름·두께를 비대칭으로 변화시킨다. Body Transform을 길게 늘리지 않았다. 정지 시 기존 shader로 돌아간다. |
| 7. Sphere identity | 실제 비교에서 코어와 구형 본체 유지. Charge On/Off RGB 차이 **0**. Travel 46프레임의 화면 중심 반경 55px 영역, 총 435,390 pixel sample에서 변경 **0**. 이는 지정한 중앙 영역 검사이며 전체 본체 픽셀 동일성을 주장하는 수치는 아니다. |
| 8. Distortion | travel 전용 distortion에 뒤쪽 가중치를 추가했다. 기존 본체 제외 mask와 국소 범위는 유지. 격자 배경 가장자리 굴절은 보이지만 다른 outer 변화에 비해 약하다. 본체 중앙 흐림이나 전체 화면 왜곡은 관찰되지 않았다. |

잔류 계산은 현재 승인된 직선·등속 경로를 전제로 한다. 급격한 방향 전환·가감속의 실제 birth transform history를 저장하는 범용 시스템은 아니다. 속도 0에서는 travel 반응이 꺼지고 낮은 속도에서는 wake/stretch가 비례해 약해진다.

## 실제 화면 평가

**Caster:** Charge에서 기존 크기와 밀도를 유지하고, release 후 뒤쪽 에너지가 잠깐 넓어졌다가 빠르게 줄어든다. 본체가 멀어지면서 화면 점유율이 감소한다. 기존 impact의 화면 whiteout은 유지했다. 정면에 가까운 시점에서는 wake가 본체 뒤에 겹쳐 측면보다 덜 분리되며, 원거리에서는 잔류 방전의 개별 움직임이 약하다.

**Side / Observer:** 이전의 둥근 outer를 그대로 이동시키는 인상이 줄었다. 뒤쪽에 넓고 짧은 마젠타 덩어리와 빠르게 교체되는 파편·방전이 보인다. 본체보다 긴 혜성 꼬리로 바뀌지 않았다. 연속 프레임에서 전방 이동과 후방 에너지 교체가 구분된다. 다만 모든 lightning이 화면에서 명백하게 고정된 월드 잔상으로 읽히는 수준은 아니다. 짧은 수명·본체 차폐·관측 각도 때문에 보이는 정도가 다르다.

레퍼런스에 가까워진 부분은 **발사 직후의 넓은 후방 에너지, 짧은 마젠타 잔류, 둥근 본체와 비대칭 outer의 대비**다. 레퍼런스의 화면 전체로 뻗는 강한 방전·공간 폭주감과는 여전히 차이가 있다. 이번 작업에서 본체나 기존 camera를 확대해 그 차이를 메우지는 않았다.

새롭게 늘어난 비용/시각 부담은 후방 마젠타 화면 점유율과 volume shader 비용이다. 첫 wake 보정에서 드러난 규칙적인 raymarch 줄무늬는 샘플 증가와 jitter로 완화했다. 일부 장면에서는 여전히 작은 입자감과 부드러운 핑크 연무가 남는다.

**남은 리스크 Top 3**

1. Wake의 plasma 경계는 개선됐지만, 밝은 마젠타가 겹치는 프레임에서는 구름/연무 인상이 남고 미세한 샘플 질감이 보일 수 있다.
2. 원거리 Caster, 밝거나 복잡한 도시 배경에서 개별 residue·distortion 가독성은 제한적이다. 이번 새 영상은 동일 조건을 위한 임시 격자 무대이며 실제 도시 전체 시야 QA를 대체하지 않는다.
3. Wake 72-step volume와 넓어진 halo 투영 범위의 **GPU frame-time은 미측정**이다. 관측 입자 최대 116은 lightning mesh segment도 포함하므로 이전의 다른 시점 샘플 수 68과 직접 성능 비교하면 안 된다. 급격한 가감속 경로도 이번 범위가 아니다.

실제 화면 기준으로는 기존 이동 표현보다 개선되어 **로컬 Production 검수 후보로 유지할 가치가 있다**. 사용자 영상 확인 전 추가 polish나 출시 품질 승인을 진행하지 않는다.

## Targeted validation

최종 유효 결과는 서로 다른 실행의 최신 테스트 결과를 합쳐 **2 PASS / 0 FAIL**이다. 하나의 최종 XML에서 2개가 실행됐다는 의미는 아니다.

| 테스트 | 최종 근거 | 결과 |
|---|---|---|
| `PurpleTravelRefinementTests.TravelTargetsRangeAndRepeatedCleanup` | `unity/Logs/AstraCombatQA/purple-travel-refinement-01.xml` | PASS |
| `PurpleTravelRefinementTests.ChargeLockAndTravelRender` | `unity/Logs/AstraCombatQA/purple-travel-render-final-03.xml` | PASS |

- 새 C# compile error **0**, 새 shader error **0**. 새 3개 shader supported 및 ShaderUtil error 검사 PASS.
- 실제 damage queue + canonical sequence: spawn/travel, 첫 target HP **45**, 뒤 target HP **100**, first-hit terminal, 무적중 max-range terminal, travel cancel PASS.
- 3회 lifecycle 종료 후 production material 증가 **0**, production visual 잔존 **0**. terminal에 ProductionOuterRuntime 없음, legacy PurpleEnergyBody 유지 확인.
- Charge 픽셀 동일성 및 최종 영상 생성 PASS. camera 소스는 변경하지 않았고 기존 integration의 camera restore 증거를 보존했다. 이번에 새로운 camera restore 전용 assertion을 추가한 것은 아니다.
- 최초 실행은 lifecycle PASS / render FAIL이었다. 두 번째 캡처에서 scene controller가 actor 위치를 바꾸어 동일 구도가 깨졌다. QA 시작 위치를 매 pass 고정한 뒤 Charge RGB 차이 0으로 PASS. runtime Charge 수정으로 해결하지 않았다.
- 이후 wake 경계 및 sampling artifact 변경 시 **렌더 테스트만** 재실행했다. 중간 `purple-travel-render-final.log`는 Package Manager IPC 실패로 테스트 미실행이며 test FAIL 수에 합산하지 않았다.
- Full regression과 기존 ProductionIntegrationTests 재실행 없음. Unity still은 비교/실패 진단에 필요한 소수만 생성하고 motion은 RGB24 스트림으로 저장했다. 원래 exploration/이전 QA 폴더는 덮어쓰지 않았다.

## 결과물

- [비교 페이지](../unity/Logs/PurpleTravelRefinementQA/Final/comparison.html)
- [Caster Charge → Release → Travel, 4.03초](../unity/Logs/PurpleTravelRefinementQA/Final/Caster_Charge_Release_Travel_4s.mp4)
- [Side 이전 / 이후, 2.03초](../unity/Logs/PurpleTravelRefinementQA/Final/Side_Before_After_2s.mp4)
- [Side 개선 원본, 2.03초](../unity/Logs/PurpleTravelRefinementQA/Final/Side_Travel_2s.mp4)
- [동일 시간 비교 이미지](../unity/Logs/PurpleTravelRefinementQA/Final/Side_comparison.jpg)
- [Release still](../unity/Logs/PurpleTravelRefinementQA/Final/Release.png), [Travel still](../unity/Logs/PurpleTravelRefinementQA/Final/Travel.png), [Charge still](../unity/Logs/PurpleTravelRefinementQA/Final/Charge_After.png)
- [최종 연속 프레임](../unity/Logs/PurpleTravelRefinementQA/Final/Side_After_sequence.jpg)
- [lifecycle](../unity/Logs/PurpleTravelRefinementQA/lifecycle.json), [픽셀·오류 수](../unity/Logs/PurpleTravelRefinementQA/final-validation.json), [보호 파일 비교](../unity/Logs/PurpleTravelRefinementQA/preservation.json)

최종 raw render: `unity/Logs/PurpleTravelRefinementQA/Render/20260918_032307_981/`. 960×540 / 30fps, Side 61 frames × 2, Caster 121 frames. 비교 MP4는 1920×540. 소리 없는 시각 검수 자료다.

## 이번 수정 파일과 보존

기존 runtime 수정은 **`unity/Assets/Scripts/Player/ProductionPurpleOuterRuntime.cs` 1개**다. 시작 시 해당 파일도 기존 integration 작업으로 untracked 상태였으므로 단순 `git diff`에 모든 task delta가 표시되지는 않는다. task 시작본은 `unity/Logs/PurpleTravelRefinementQA/ProductionPurpleOuterRuntime.before.cs.txt`에 보존했다.

신규 7개 파일과 각각의 `.meta`:

- `unity/Assets/Scripts/Player/PurpleTravelVisualProfile.cs`
- `unity/Assets/Editor/PurpleTravelVisualProfileEditor.cs` — 한글 Inspector
- `unity/Assets/Resources/VFX/PurpleTravelVisualProfile.asset`
- `unity/Assets/Resources/VFX/PurpleTravelWake.shader`
- `unity/Assets/Resources/VFX/PurpleTravelHalo.shader`
- `unity/Assets/Resources/VFX/PurpleTravelDistortion.shader`
- `unity/Assets/Editor/PurpleTravelRefinementTests.cs`

문서: 이 보고서 신규, `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md` 갱신. 로컬 QA 산출물은 `unity/Logs/PurpleTravelRefinementQA/`에만 정리했다.

시작 snapshot 757개 중 허용된 runtime 1개와 상태 문서 2개를 제외한 **754개 동일 / 누락 0**. D2-R3/R2/R1/OuterR2 exploration, production body/terminal/canonical sequence/profile, Purple_UserPrototype, local art, Scene, Package/ProjectSettings, 이전 integration 영상·archive를 보존했다.

Git: `feat/gojo-blue-screen-distortion`, HEAD `a9c6ccf9c09b2effdd559e83e865296e45f600b2`. 최종 tracked modified **20**, untracked **206**, staged **0**. 시작 dirty tree를 포함한 수치다. 전체 `git status -sb`는 [git-status-final.txt](../unity/Logs/PurpleTravelRefinementQA/git-status-final.txt)에 저장했다. commit / push / merge / reset / clean / restore 실행 없음.

전체 `git diff --check`는 보호된 기존 `VFXLab.unity` 로컬 변경의 trailing whitespace 8곳을 보고했다. 해당 Scene hash는 task 시작과 동일하며 이 작업에서 정리하지 않았다. 이번 상태 문서의 diff whitespace 검사는 통과했다.
