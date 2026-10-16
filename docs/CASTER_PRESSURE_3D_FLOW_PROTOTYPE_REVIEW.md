# CasterPressure3DFlowPrototype — ONE FLOW

2026-10-06 KST. **CODEX VALIDATED (아래 검사 범위) / PENDING USER VISUAL REVIEW / NOT USER VERIFIED**. 시각 목표는 부분 달성이다. **전체 적용 보류**. 기본 OFF, 기존 10개 풍압과 모든 후보 보존. Commit/push/merge 없음.

## Change Contract

- Target gap: 바닥에 눕힌 종이/리본이 회전하는 인상.
- Allowed scope: 대표 흐름 하나의 열린 입체 곡면·내부 압력·국소 능선, 별도 profile/shader/runtime와 opt-in lifecycle hook, Side/Caster QA.
- Must preserve: 기존 CasterPressure 및 모든 후보, PairPolish 동작·크기·경로·접촉/융합 timing, Purple 전체, gameplay·camera·USER VERIFIED·unrelated dirty 작업.
- Expected visible change: 낮은 전방→측면 상승→후방을 지나는 실제 3D 경로, 서로 다른 깊이/곡률/밀도가 한 흐름으로 읽힘.
- Failure condition: 넓은 흰 판/호스/평행 종이 복제, 수평 원운동이 주역, crest가 전체 실루엣을 정의함.
- Required evidence: 밝은 중성 Side/Caster, 원속도 짧은 A/B, 인접 프레임, 경로·release·resource·보호 검사.

## 표현과 동작

열린 비대칭 곡면 단면을 시전자 기준 spline 경로에 따라 배치했다. 닫힌 tube, cross-plane, billboard가 아니다. Outer/Inner/Crest 각각 mesh/material/renderer 하나, 총 2,379 vertices와 4,320 triangles. 실제 volumetric rendering은 아니다.

- Outer: 넓은 열린 곡면으로 공간 점유와 soft 회백색 밀도.
- Inner: 더 작은 압축 영역이지만 Outer를 축소 복제하지 않는다. 중심이 옆/높이 방향으로 비껴 있고 반대 곡률, 다른 phase/밀도 이동 속도를 사용한다.
- Crest: 같은 경로의 좁고 부분적인 밝은 능선. 독립 원운동을 하지 않는다. 일부 프레임에서는 여전히 길게 보여 부차적인 역할이 완전히 증명되지는 않았다.

Profile 경로는 local y 0.12→3.25m, z +2.5→-3.1m, x -4→+1.55→-2.1m를 통과한다. 밀도는 경로 방향으로 이동하고 폭/압축도 변한다. 경로 자체를 caster 주위 원궤도로 회전시키지 않는다. 앞뒤 위치가 있는 곡면과 offset interior라는 점은 기존 단일 얇은 strip보다 입체적이지만, 그것만으로 화면상의 volume impression을 보장하지 않는다.

Charge는 caster 위치를 따라가고 release 시 anchor를 고정한다. 기존 전방 쓸림과 0.32s fade를 재사용하며 projectile 위치/속도는 입력받지 않는다. 새 camera FX, Bloom/exposure 변경 없음. QA의 기존 공통 ConvergenceDiagnostic 및 Purple wrapper는 양쪽 동일하다.

## 실제 화면 평가

| 근거 | 관찰 | 반대 근거 / confidence |
|---|---|---|
| Side 0.35–1.35s | 수평 바닥 원호 비중은 줄고, 흐름이 구체 전경을 지나 상부·후방으로 이어진다. | 0.65/1.35s에 밝은 내부 곡면이 넓은 흰 swish/띠로 읽힌다. 전경 가림이 Blue 쪽에 치우쳐 caster를 감싸는 풍압이라는 인상은 약함. medium. |
| Caster 0.633–0.667s | Side와 다른 화면 위치/중첩으로 깊이가 있는 상승 경로가 나타난다. | 내부의 S자 흰 곡면이 여전히 sheet처럼 보인다. 캐릭터 앞·뒤를 통과한다는 인상보다 ingredient 사이 위쪽의 흰 흐름으로 읽히기도 한다. medium. |
| Caster 1.333–1.367s | Blue/Red 앞에 서로 다른 부분의 압력이 겹친다. | 층의 parallax와 하나의 공기 질량이라는 인상이 충분히 명확하지 않다. bright section/crest가 별도 띠처럼 읽힐 수 있음. medium. |
| Release 5.0–5.55s | anchor 고정, 전방 쓸림, 0.32s 안에 소멸. | 큰 공통 Purple wrapper가 전환을 가려 새 흐름 자체의 시각 검증은 제한됨. lifecycle high / visible quality low-medium. |

Visual similarity / Art direction: **MIXED / MIXED**. 기하학적 상승·깊이는 확보했지만 종이/띠 문제를 충분히 해결했다고 할 수 없다. 하나의 흐름만 비교하므로 baseline 10개와 전체 풍압 밀도/위압감을 동등 비교하지 않는다. **전체 storm 확장은 보류**한다.

AI 평가는 실제 렌더의 시간 순서/인접 프레임에 근거한다. 원속도 영상을 제공했으나 사람의 실시간 체감 검수를 대신했다고 주장하지 않는다. 렌더 이후 추가 visual polish나 전체 확장 없음.

## 검증

- C# compile error 0. 새 shader supported / error 0.
- **10 PASS / 0 FAIL**: 새 OFF/shader/Inspector, 높이·깊이 경로/anchor/release, 반복 cancel/resource 3개; 기존 PairPolish 6개; 실제 Play Mode canonical pose 보호 1개.
- 기존 custom harness에서 NUnit assertions를 명시적으로 실행했고 XML을 생성했다. Unity Test Runner discovery로 표현하지 않는다. [Edit targeted XML](../unity/Logs/CasterPressure3DFlowPrototypeQA/targeted.xml), [Play canonical XML](../unity/Logs/CasterPressure3DFlowPrototypeQA/play-canonical.xml).
- Side/Caster 각각 145 sample의 ingredient/Purple 위치·크기·활성 상태, contact, 공통 flash/hold/Bloom/exposure 기록 동일. anchor는 `(512,1,512)` 하나. 접촉 약 1.44418s 유지.
- Side 7.0s terminal pixel diff 0. **Caster terminal INCONCLUSIVE**: max RGB diff 164, changed pixels 1,505. Live actor/HUD 등 변수가 남아 원인을 확정하지 않으며 PASS로 승격하지 않는다.
- 캡처 후 ingredient materials/meshes 0/0, 기존/새 풍압 root 0/0, 후보 OFF. Play 종료 후 VFXLab Edit Mode/Scene dirty false/background false 복구. Scene asset 저장 없음.
- Console 현재 오류 0 / 이번 작업 새 오류 0. Console 전체 이력에는 이전 작업 오류 9건이 남는다. 이번 compile에는 기존 renderer member hiding 및 기존 capture unused-variable 경고가 재기록됐다. 기존 오류를 삭제하거나 전체 이력을 0이라고 보고하지 않는다.
- 시작 hash 기준 기존 파일 변경은 `PrototypeHollowPurplePresentationRuntime.cs`의 새 opt-in hook뿐이다. 기존 CasterPressure/profile/shader/capture/보고서와 모든 다른 dirty 파일 보존. Staged 0.
- Full regression/GPU 성능 측정/Observer 캡처 없음. CPU mesh/normal 갱신과 3개 transparent renderer의 비용은 전체 확장 전 고려해야 한다.

## 산출물

폴더: `D:\JJK_game\unity\Logs\CasterPressure3DFlowPrototypeQA\AB\20261005_175041_970` (UTC 식별자).

LEFT **BASELINE = 기존 CasterPressure 10개 흐름**, RIGHT **CANDIDATE = ONE FLOW만**. PairPolish/Purple/환경/카메라는 동일. 1920×540, 60fps, 145 frames / 2.416667s.

영상은 연속 전체 시전이 아니다. **차징 0.000–1.833s → 발사 5.000–5.550s 발췌를 붙였고**, 각 구간은 원속도이며 전환 라벨을 표시한다.

- [Side_AB_Focused.mp4](../unity/Logs/CasterPressure3DFlowPrototypeQA/AB/20261005_175041_970/Side_AB_Focused.mp4)
- [Caster_AB_Focused.mp4](../unity/Logs/CasterPressure3DFlowPrototypeQA/AB/20261005_175041_970/Caster_AB_Focused.mp4)
- 각 시점의 `Charge_Adjacent.jpg`, `Release_Adjacent.jpg`, `Flow_Sequence.jpg`.
- [비교 검증 JSON](../unity/Logs/CasterPressure3DFlowPrototypeQA/AB/20261005_175041_970/comparison_validation.json).

## 변경 파일

- 새 Player: `CasterPressure3DFlowProfile.cs`, `CasterPressure3DFlowPrototype.cs`.
- 새 Resources/VFX: `CasterPressure3DFlow.shader`, `CasterPressure3DFlowProfile.asset`.
- 새 Editor: `CasterPressure3DFlowProfileEditor.cs`, `CasterPressure3DFlowCapture.cs`, `CasterPressure3DFlowValidation.cs`.
- 위 7개 Unity 파일의 `.meta`.
- 기존 `PrototypeHollowPurplePresentationRuntime.cs`: bind/sample/dispose 연결 10줄 추가. 작업 전의 기존 caster pressure hook과 unrelated diff 보존.
- 이 보고서. QA 스크립트/로그/영상은 ignored `unity/Logs/CasterPressure3DFlowPrototypeQA` 안에만 저장.

USER VERIFIED / Production 승격 없음. 전체 적용 보류, 사용자 영상 검수 대기.
