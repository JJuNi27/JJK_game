# PASS 4B.3b — 주력 아우라 실루엣·크기 비교

> 2026-10-25 체크포인트 갱신: 통합 아우라의 시각적 방향은 사용자가 승인했고, 4B.4a에서 일반 F 기본 프로필에 적용한 뒤 최신 출력도 직접 확인했다. 아래는 개발 당시 비교/검증 기록이다. 1.7/2.7/3.0 등 실험 크기 곡선의 영구 채택까지 승인된 것은 아니다. 현재 저장 곡선은100→1.0 /150→1.5 /200→2.0이다.

2026-10-24 KST. **LOCAL / CODEX VALIDATED(targeted 기술 범위) / PENDING USER VISUAL REVIEW**.
브랜치 `feat/shibuya-combat-mvp`, HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`.
기존 PASS3D~4B.3a 미커밋 작업 위에서 진행했다. commit/push/staging은 하지 않았다.

## 범위와 기본값

기존 전신 주력장을 교체하지 않았다. **기존 Resources 기본 아우라와 Pink/Green 프로필 파일은 바이트 단위로 보존**했다. 4B.3b 통합 후보는 Play Mode 메뉴로만 선택하는 임시 복제 프로필이다. 1.7/2.7/3.0 곡선을 기존 기본값으로 승격하지 않았다.

handoff/current task는 10-22 PASS3D 시점이므로 실제 로컬 상태와 이번 요청을 기준으로 작업했다. 시작 시 staged 0, tracked unstaged 29, untracked 105였고 Unity는 stopped, Shibuya 씬 clean이었다. 변경 전 1,172개 소스/문서/데이터 파일의 SHA-256과 관련 소스 사본을 새 QA 폴더에 남겼다.

## 1. 실제 렌더 구조

- `MainAura`: 높이별 신체 bounds 단면을 연결한 단일 전신 외곽 carrier. Main/Interior는 **동일 셰이더 field 분기**의 기여이며 별도 Interior 메시가 아니다.
- `InkRuptures`: 같은 정점·위상을 쓰는 얇은 먹물 hull. MainAura 자체에도 침식 경계/암부 색 혼합이 있다.
- `EnergyWisps`: 실제 메시 표면의 대표점에 부착된 24개 보조 닫힌 불꽃. 사용자가 말한 Surface Details다.
- `BodyRim`, `MicroParticles`: 기존 신체 윤곽광과 미세 입자. 이번에 재설계하지 않았다.

기존 Main/Ink/Details 소유 메시 수는 3, 소유 재질은 4다. field carrier는 각각 2,009정점, Details는 3,120정점이다. 본/Animator나 게임플레이 이동을 새로 연결하지 않았다.

## 2. 레퍼런스 확인

로컬 `주력이펙트.mp4`, `다양한 주력이펙트.mp4`, `주력이펙트 전투.mp4`를 실제로 추출했다. 0.1초 간격 인접 프레임과 메타데이터를 QA 루트에 보존했다. 첫 영상 5.0~5.7초에서는 큰 화염 흐름과 작은 끝 조각의 교체가 구분된다. 청록 내부·먹물 경계의 동행, 불규칙한 가는 꼬리를 참고했다. 다양한 예시의 평면 panel을 캐릭터 geometry로 복제하지 않았다.

`주력이펙트(1).mp4`는 `_local_refs`에서 찾지 못했다. 확인하지 않은 영상의 움직임을 추정해 근거로 사용하지 않았다. 0.1초 표본은 미세 수명·정확한 제작 방식까지 확정하는 증거가 아니다.

## 3. A — Surface Details OFF

렌더 코드 수정 **전에** 전체/Details OFF/메인 단독/Details 단독을 촬영했다. OFF 시 팔·얼굴·허리 앞에 고정된 작은 뿔/조각이 사라지며 MainAura와 Interior는 유지됐다. 큰 어깨/머리 외곽의 넓은 면은 여전히 Main에서도 남으므로 원인을 전부 Details 탓으로 돌리지 않았다.

구현을 삭제하지 않고 기존 `wisps.enabled`를 독립 사용한다. 통합 실험만 OFF, 기존 기본 프로필은 ON을 보존한다. Inspector 표시를 ‘신체 표면 보조 불꽃 (Surface Details)’로 명확히 했다.

## 4. B — 먹물 경계와 넓은 암부 분리

`inkBoundaryScale`은 얇은 hull 간격, grazing 경계 폭·농도를 함께 강화한다. `inkInteriorArea`는 Main 안의 넓은 inkFlow 기여를 별도로 조절한다. 같은 변형/침식장을 사용하며 별도 검은 판을 만들지 않는다. Blend/ZWrite/Cull/렌더 순서는 보존했다.

독립 B 비교는 경계 1.00 / 1.15 / 1.30, 넓은 암부 1.00으로 고정했다. 통합 후보는 경계 1.30, 넓은 암부 0.80이다. 경계 선명도를 높이면서 고배율의 큰 검은 면을 완화하려는 **비교값**이며 최종 승인이 아니다. 기존 기본은 둘 다 1.00이다.

## 5. C — 분리되는 화염 끝 조각

Main carrier의 여러 높이(41개 row 중 5~33)에서 변형 crest를 선택한다. 비동기 seed/cycle로 최대 8개 작은 닫힌 화염 조각이 생겨 굽어 상승하고 축소·감쇠한다. 출생점을 월드 공간에 저장하므로 캐릭터가 이동해도 조각 전체가 캐릭터에 붙어 따라가지 않는다. world-space 곡선 drift를 기존 shader의 별도 mode로 처리한다.

기존 body-attached Details를 재사용하면 다시 부위별 뿔이 되므로, 작은 공유 메시 **1개(504정점/896삼각형)**를 추가했다. 청록/먹물 renderer 2개가 같은 메시와 기존 energy/ink 재질을 공유한다. 재질 추가와 새로운 ParticleSystem은 없다. 같은 수명·경로·실루엣 위에 먹물을 얹는다. ON/OFF 가능하며 기본 OFF다.

이는 실제 Main 메시의 끝을 topology로 절단하는 물리 시뮬레이션이 아니다. field crest에서 출발하는 작은 표현 조각이다. 화면상의 alpha 침식 끝과 항상 정확히 같은 픽셀에서 찢어지는 것은 보장하지 않는다. 기존 Outer가 주역이며 작은 조각은 보조다.

## 6. D — 고배율 퍼짐의 원인과 수정

수정 전 확인한 원인:

1. 신체 밖 carrier 여유 거리뿐 아니라 GPU의 normal 변위·crest·보조 불꽃 폭/굽힘도 동일 expansion으로 확대됐다.
2. 같은 48개 둘레 분할과 고정 UV window가 더 큰 공간을 차지해 긴 면이 오래 노출됐다.
3. 먹물 hull 확장과 넓은 inkFlow가 얇은 경계와 구분되지 않았다.
4. Interior의 가시성·밀도 값 자체가 문제라기보다, 외곽 변형과 면의 공간 크기가 함께 증가했다.

`preserveLocalFlameShape`는 최대 주력량 100의 국소 변형 크기를 기준으로 큰 배율의 변형/Details 폭이 계속 팽창하지 않도록 분리한다. 큰 field의 window도 고정 UV span 대신 body-space 길이 기준으로 전환해 넓은 면을 국소적으로 끊는다. 전체 motionSpeed/irregularity를 줄이거나 최대 배율을 낮추지 않았다.

`extraFieldReach`는 별도로 전신 바깥 여유 거리를 늘린다. 기존 thickness 성장에 `max(0,expansion-BaseExpansion) × 0.22 × extraFieldReach`를 더한다. 통합 후보는 0.65, 기존 기본은 0이다. 발밑·몸·CharacterController·실제 방출점은 확대하지 않는다. 높이별 전신 연결 geometry는 그대로이며 새로운 shell/벽 시스템을 추가하지 않았다.

## 7. E — 실제 화면 크기

960×720, 같은 카메라/자세에서 phase 0.8/1.4/2.0/2.6/3.2의 5개 표본을 비교했다. 메인 field/ink만 켜고 입자·Details·분리 조각·Rim을 제외해 작은 조각이 측정치를 부풀리지 않게 했다. aura 없는 body 영상과 12/255 초과 RGB 차이의 외곽 bounding box를 측정했다. 몸 기준 폭/높이는 실제 신체 Mesh 정점의 화면 투영이다. 이는 모든 카메라에서의 최대 envelope나 물리 부피 측정이 아니다.

| 설정 | 폭(px), 중앙값 | 몸 대비 폭 | 몸 대비 높이 |
|---|---:|---:|---:|
| 기존 100 ×1.00 | 218 | 2.202 | 1.224 |
| 기존 150 ×1.50 | 257 | 2.596 | 1.287 |
| 기존 200 ×2.00 | 299 | 3.020 | 1.358 |
| 기존 200 ×2.50 | 343 | 3.465 | 1.390 |
| 통합 후보 100 ×1.00 | 220 | 2.222 | 1.232 |
| 통합 후보 150 ×1.70 | 271 | 2.737 | 1.287 |
| 통합 후보 200 ×2.70 | 333 | 3.364 | 1.343 |
| 통합 후보 200 ×3.00 | 352 | 3.556 | 1.378 |

통합 1.7→2.7의 폭 중앙값 증가는 약22.9%, 2.7→3.0은 약5.7%다. 높이보다 폭 쪽으로 차이가 난다. 기존 2.5의 퍼진 실루엣보다 새 2.7이 항상 크지는 않으며, 기존 3.0보다 새 3.0의 폭·높이도 작다. 이것은 국소 늘어짐을 억제한 결과다. ‘무조건 더 커졌다’고 평가하지 않는다. phase별 범위가 겹치므로 매 프레임의 시각 크기 순서를 보장하지 않는다.

동일 배율 1/1.5/1.7/2/2.5/2.7/3의 기존/후보 각 5개 표본 70개를 원본 JSON으로 보존했다. E의 기존/후보 이미지 모두 Details OFF로 원인을 통제했으며, 원래 Details ON인 전체 기준은 A에서 별도로 비교한다.

## 8. Interior와 기존 기능 보존

내부 상승 가시성 0.65, 밀도 1.0, 기존 noise 상승 방향·내부 색 기여 계산을 유지했다. 별도 Interior emitter를 늘리지 않았다. 큰 field에서도 내부 상승 무늬는 남는다. 강화된 경계/coverage와 합성한 최종 인상은 사용자 검수가 필요하다.

F 입력/상태 Controller, CursedEnergyProfile.MaxEnergy 소유권, 현재 잔량과 크기 분리, 전투·락온·카메라·공중전 코드는 이번에 수정하지 않았다. 지상 피해20/20/20/25, Dummy1000/Player100, 기존 GUID·Gojo/VFX·Scene·Prefab·Packages·ProjectSettings·기존 dirty 문서는 변경 전 해시와 비교한다.

기존 기본 설정으로 촬영한 전체/Details OFF 각 5방향 **10장 모두 변경 전후 decoded RGB 차이 0픽셀**이었다. 이는 기본 경로의 정지 비교이며 모든 동작·환경에 대한 픽셀 동등성을 의미하지 않는다.

## 9. Inspector 및 사용자 미리보기

추가 한국어 SerializedProperty 항목:

- 고배율 국소 불꽃 형태 보존
- 증가 주력량의 추가 공간 확장
- 먹물 경계 두께·대비 배율
- 넓은 내부 먹물 면적 비중
- 화염 끝 분리 조각 사용
- 분리 조각 교체 속도

기존 크기 곡선/Interior/색상/레이어 설정과 Undo·Range 편집을 보존했다. 실험 프로필을 수정해도 원본 자산을 저장하지 않는다.

사용 방법:

1. `ShibuyaCombatMVP` Play Mode 진입.
2. `JJK Game → 시부야 PASS4B3b → 통합 후보 → 배율 1.00 / 1.70 / 2.70 / 3.00 미리보기 (Play Mode 전용)`.
3. F 홀드, 카메라 회전, 이동·공격·해제 비교. 선택된 임시 Profile Inspector에서 Details/먹물/분리 조각을 독립 비교할 수 있다.
4. `JJK Game → 시부야 PASS4B3b → 기존 프로필로 복귀` 또는 Play Mode 종료.

‘새 후보 곡선으로 비교’ 메뉴는 복제 프로필에 새 후보 곡선 100→1/150→1.7/200→2.7을 넣는다. **강제 배율 미리보기는 실제 최대 주력량을 변경하지 않는다.** 메뉴 종료·Domain Reload 시 임시 프로필을 정리한다.

## 10. 비교 자료

공통 QA: `unity/Logs/ShibuyaPass4B3bQA/20261024-silhouette/`.

`media/`:

- A: `A-details-front/side/diagonal/back/high.png`, `A-Details-AB.mp4`.
- B: `B-ink-front/side/diagonal/back/high/close.png`, `B-Ink15-AB.mp4`, `B-Ink30-AB.mp4`.
- C: `C-tips-*.png`, `C-Tips-AB.mp4`, `C-tip-close.mp4`.
- D: `D-local-shape-*.png`, `D-Shape-AB.mp4`.
- E: `E-growth-*.png`, `E-Integrated-AB.mp4`.
- 통합: `integrated-before-after-*.png`, `integrated-1.00-orbit.mp4`, `integrated-1.70-orbit.mp4`, **`integrated-2.70-complete-orbit.mp4`**, `integrated-3.00-orbit.mp4`.
- `integrated-moving.mp4`, `integrated-attack.mp4`, `integrated-release.mp4`, `high3-orbit-contact.png`.

비교 영상은 왼쪽 baseline/control, 오른쪽 candidate이며 각 화면에 개별 라벨을 표시했다. 원본의 실제 timestamp에 맞춰 30fps 출력으로 복제했으며 보간·슬로 재생은 하지 않았다. 촬영 source 간격은 `video-sampling.json`에 기록했다.

최초 2.7배 orbit 원본은 마지막 약1.6초의 Editor 프레임 공백으로 약216도까지만 촬영됐다. `final-targeted` 원본과 초기 파생 `integrated-2.70-orbit.mp4`는 그대로 남겼으며 **완전 회전 증거로 사용하지 않는다**. `orbit270-completion`에서 해당 촬영만 보완했고 완전360도/4.009초/최대 source 간격89.5ms를 확인했다. 위 complete 파일을 사용한다. 다른 orbit도 최대 약71ms의 capture 간격이 있으므로 실제 사용자 Play Mode의 연속 움직임을 대체하지 않는다.

## 11. 검증 및 실패 이력

최종 집계와 rendering 진단 결과는 QA 루트 `validation-summary.json`을 기준으로 한다. 여러 실행의 최신 고유 결과를 단일 전체 실행으로 표현하지 않는다.

**별도 실행들의 최신 고유 20 PASS / 0 FAIL / 0 SKIP**다. 전체 전투 회귀 190개 실행 결과가 아니다.

- `final-targeted/results.xml`: 분리 B/C, 화면 크기/D, 통합 원속도·이동·공격·해제, Inspector/자원 정리 **4 PASS / 0 FAIL / 0 SKIP**.
- `related-and-motion/results.xml`: 기존 F·표면 분포·연결 topology·이동/점프·기본85피해·공중5타·피격/사망·독립 재질·Undo·최대/현재 주력량·Interior, 중립 경로 재촬영, A~E 독립 원속도 **14 PASS / 0 FAIL / 0 SKIP**.
- `orbit270-completion/results.xml`: 불완전했던 회전 촬영만 보완 **1 PASS**.
- `fragment-overdraw-null-fixed/results.xml`: 실제 shader 변형·discard를 재사용하는 화면 fragment 겹침 진단 **1 PASS**.

최종 정적 검토에서 Main을 Inspector에서 껐을 때 비활성 분리 조각이 미준비 field를 조회할 수 있는 경계를 발견했다. 활성 tip renderer와 field fit이 있을 때만 출생점을 갱신하도록 제한했다. 기존 Main ON 시각 경로에는 변화가 없다. `final-layer-toggle`에서 기존 자원/Inspector 검사에 Main OFF→ON을 추가해 해당 케이스만 재검증하여 **1 PASS**를 확인했다. 이 중복 케이스를 새 고유 PASS로 합산하지 않는다. 총5개 실행의 최신 고유 결과가20개다.

최초 `before/results.xml` 1 FAIL은 실행 중 ParticleSystem의 seed를 바꾼 QA 코드 오류였다. StopEmittingAndClear 후 seed를 지정하도록 고쳤고 원본을 보존했다. QA 코드 작성 중 존재하지 않는 입력 필드 및 var 다중 선언의 C# 오류도 수정했으며 Console 이력에 남겼다. 게임플레이 기대값이나 승인값을 완화하지 않았다.

`fragment-overdraw/results.xml`의 최초 진단 1 FAIL은 설정하지 않은 material vector array를 빈 배열로 가정한 QA 도구의 NullReference였다. null 복사를 건너뛰도록 고쳐 별도 경로에서 같은 진단을 실행했다. 런타임 VFX·전투 코드나 기대값 수정은 없었다. 실패 XML과 Console 이력은 보존했다.

## 12. 성능·자원

통합 후보 소유 재질4/메시4, 반복F/이동12회 후 증가 없음, 파괴 후 소유 재질/메시0/0. 기본 경로는 메시3을 유지한다. 분리 조각을 켰다 끈 경우 작은 mesh 캐시는 visual 파괴까지 유지된다.

Details OFF는 기존 3,120정점/5,760삼각형의 제출을 끄고, 분리 조각은 공유504정점/896삼각형을 두 번 렌더한다. Rim/motes를 제외한 제출 vertex 수는 기본7,138→통합5,026이지만, 이것만으로 GPU 비용 감소를 주장하지 않는다. 확대된 field의 fragment 비용은 오히려 늘 수 있다.

움직이는 3배 후보의 field refit 12표본 중앙값2.085ms/최대2.589ms. 이는 CPU field 갱신 한 구간이며 전체 프레임 또는 GPU 시간은 아니다. 같은 사선 카메라의 미변형 carrier 삼각형 투영면적은 기존3배63,287→통합3배89,500 pixel-area로 약41.4% 증가했다. 이 수치 역시 실제 overdraw가 아니며 별도 fragment 진단과 구분한다.

추가 fragment 진단은 기존 volume shader를 메모리에서만 복제해 **동일한 vertex 변형·alpha clip을 통과한 fragment마다 white/alpha0.1**을 합성했다. Linear Float render target의 `C = 1 - 0.9^N`으로 층수를 읽었다. 진단용 재질/Shader를 파일로 저장하지 않았고 종료 시 제거했다. 원래 재질·카메라·레이어는 finally에서 복구했다.

| 사선 카메라 5개 phase | 덮인 픽셀 중앙값 | 덮인 픽셀당 층수 중앙값 | 최고 층수 | fragment-pixel 합 중앙값 |
|---|---:|---:|---:|---:|
| 기존1.0 (Details ON) | 30,942 | 1.300 | 7 | 39,872 |
| 기존2.0 (Details ON) | 50,971 | 1.515 | 8 | 78,329 |
| 기존2.5 (Details ON) | 61,628 | 1.608 | 9 | 103,246 |
| 기존3.0 (Details ON) | 73,403 | 1.696 | 9 | 127,161 |
| 통합2.7 (Details OFF, tips ON) | 59,846 | 1.218 | 6 | 72,619 |
| 통합3.0 (Details OFF, tips ON) | 65,246 | 1.224 | 6 | 79,066 |

후보에서는 Details 제거의 영향과 국소 coverage 변화가 함께 반영된다. 이 표만으로 개별 수정 하나의 성능 효과를 분리하지 않는다. body의 depth 가림을 제외한 보수적 wrapper 전용 진단이며 Rim/motes/UI는 제외했다. shader 실행 시간·GPU ms·다인전 성능을 측정한 것이 아니다. 과도한 겹침 폭증은 이 고정 조건에서는 관측하지 않았으나 다른 카메라/캐릭터/플랫폼까지 일반화하지 않는다.

## 13. 변경 파일

기존 로컬 파일6개 수정:

- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraProfile.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraField.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraGeometry.cs`
- `unity/Assets/Shibuya/Scripts/CursedEnergyAuraVisual.cs`
- `unity/Assets/Shibuya/Editor/CursedEnergyAuraInspector.cs`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/CursedEnergyAuraVolume.shader`

신규:

- `unity/Assets/Shibuya/Editor/CursedEnergyAuraSilhouettePreview.cs` 및 `.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4B3bTests.cs` 및 `.meta`
- 이 보고서. QA 원본/미디어/스크립트는 새 Logs 하위에만 저장했다.

## 14. 남은 시각·성능 한계

OFF로 작은 신체 부착 뿔은 줄었지만 Main의 큰 머리/어깨 꼬리까지 없어지는 것은 아니다. 일부 사선/측면에서는 여전히 넓은 반투명 면이 보인다. 실제 유체/볼륨이 아니라 닫힌 carrier 표면과 alpha/가림으로 만드는 표현이다. 작은 조각이 crest에서 출발해도 일부 phase에서는 잠깐 떨어져 떠 보일 수 있다.

레퍼런스의 자잘한 먹물 섬유·끝 침식에 비하면 현재 결과는 큰 단위 변형이 더 잘 읽힌다. ‘reference와 동일하다’, ‘천/면 인상이 완전히 해결됐다’고 평가하지 않는다. A~E와 고배율 회전 영상을 보고 Details OFF, 경계1.15/1.30, 새 곡선과 2.7/3.0 중 무엇을 기본으로 채택할지는 사용자가 선택해야 한다.

2.7/3.0은 더 넓은 후보이며 최종 기본값이 아니다. 3.0은 화면 가림과 fragment 비용의 증가를 함께 봐야 한다. 넓은 맵·다인전/여러 캐릭터 동시 사용·저사양 GPU의 성능은 검증하지 않았다. 기술 targeted 검증과 주관적 품질 승인은 구분하며 **USER VERIFIED는 사용자 직접 확인 후에만** 기록한다.

## 15. 컴파일·Console·Editor

최종 C# compile 성공, Volume/Rim/Mote shader message 0, 마지막 레이어 검사 후 현재 Editor Console error0 / warning0이다. 그 직전 점검에서는 `Deleting invalid font reference.` warning1을 관측했으며 버퍼에 보존돼 있다. Pipeline의 보존 버퍼에는 이번 수정 전의 UnityConnect/CrashReporter 및 과거·이번 QA 코드의 수정된 오류를 포함해 error10이 남아 있다. Console을 clear하거나 최초 실패를 지우지 않았다. 현재 error0과 전체 이력 error0을 혼동하지 않는다.

Unity ready / stopped, Shibuya 씬 clean, Assets의 dirty ScriptableObject0, 소유 material/mesh0/0, 진단 Shader 잔존0. 일부 Unity 내장·패키지·Gojo Shader의 메모리 dirty 플래그는 import 과정에서 관측했으나 저장/초기화하지 않았고 해당 파일의 디스크 해시는 변경 전과 같다.

AutoTick true/16ms, targetFrameRate -1, vSync0, Time.timeScale1을 유지했다. AutoTick/프레임률/Play Unfocused 설정을 변경하는 호출이나 Windows 직접 조작·포커스 요청은 하지 않았다. 카메라 출력용 API와 Play Mode 테스트를 사용했다. 상태 원본은 `final-editor-state.json`, 마지막 레이어 검사 후 확인은 `final-editor-state-after-layer-toggle.json`이다.

## 16. Git·보호 결과

변경 전 1,172개 파일 중1,166개는 SHA-256 동일하고 변경6개는 모두 위 아우라 구현 파일이다. 신규5개는 Editor 코드2개/해당 meta2개/이 보고서다. 기존 모든 meta GUID와 Scene/Prefab/Data Asset/Resources 기본·Pink·Green/Gojo/VFX/Packages/ProjectSettings/기존 dirty 문서는 동일하다. Dummy 최대HP1000/Player100을 재확인했다.

브랜치/HEAD 그대로, staged0 / tracked dirty29 / untracked files110. 시작 전부터 있던 tracked dirty29와 untracked105를 보존했다. 기존 QA에는 쓰지 않았고 신규 Logs 경로에만 증거를 추가했다. `preservation.json`, `before-hashes.json`, `source-before/`에 근거가 있다. staging/commit/push/merge/reset/clean/checkout/worktree 작업은 하지 않았다.
