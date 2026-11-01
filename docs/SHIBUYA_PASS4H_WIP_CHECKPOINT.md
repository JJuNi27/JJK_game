# PASS 4H WIP 체크포인트 — 시각 최종 승인 아님

2026-11-02 / `feat/shibuya-combat-mvp`.
이 문서를 포함한 WIP 커밋은 기존 PASS4G 체크포인트 `891064067834bc26f3a1afc6a37b84bb5e1e79c1` 이후의 PASS4H 개발 상태를 보존한다. 실제 커밋 SHA와 원격 반영은 Git 이력 및 작업 완료 보고로 확인한다.

## 현재 동작과 승인 구분

- **실제 전투 흑섬은 현재 REV1**이다. Applied + BlackFlash + 양수 HP 감소 결과에 연결된 기존 H0/REV1 런타임을 보존한다.
- A/B/C, BASE/T/S/TS, 새 Art-Directed L2/L3는 Editor QA 시제품이다. 본게임에 자동 채택하거나 전투 REV1을 교체하지 않았다.
- 흑섬 VFX의 최종 형태·시각 품질은 **USER VERIFIED가 아니다**. 사용자 피드백상 이전 후보의 차이가 작고 품질이 부족했으며, 새 L2/L3도 최종 선택 전이다.
- 기존 PASS4G USER VERIFIED 범위, 확률·피해·Zone·반격·숫자·F 아우라·카메라·Gojo·잠금값은 변경하지 않는다. 이번 저장을 위해 새 기능이나 Unity 검증을 실행하지 않았다.
- 이번 WIP 저장 승인은 시각적 승인과 별개다. 이전 보고서의 LOCAL UNCOMMITTED/commit 미수행 문장은 당시 이력으로 보존한다.

## 재사용한 기술 검증 근거

| 범위 | 기존 결과 | 해석 |
|---|---|---|
| H0 | 최신 고유11 PASS | 6회 실행 집계; 단일 실행 아님 |
| REV1 | 최신 고유14 PASS | 마지막3회 케이스별 최신 집계 |
| Visual R&D A/B/C | 최신 고유5 PASS | 5회 실행 집계; 시각 최종안 선정 아님 |
| Temporal×Shape | 최종 targeted 단일3 PASS | 앞선 정상 촬영을 검증해 재사용 |
| Art-Directed L2/L3 | 최종 `validated` 단일3 PASS/0 FAIL/0 SKIP | 최종 순서 촬영 포함; 시각 USER VERIFIED 아님 |

위 숫자를 하나의 전체 회귀 성공으로 합산하지 않는다. 원본 XML·실패·촬영 이력은 로컬 Logs에 남는다. Frame Debugger의 실제 draw 확보 실패, pose 전환 팝감·2.5D 겹침·측면 약화·GPU 성능 미측정 등의 한계도 각 보고서대로 유지한다. PASS4A 과거 미해결18건을 해결 처리하지 않는다.

## 저장 범위

흑섬 런타임/Resources 프로필/셰이더, QA 미리보기/테스트/한국어 Inspector, 실제 새 L2/L3 PNG/프로필과 .meta, 관련 설계·검증 보고서 및 연결에 필요한 Match/TestRunner 변경을 포함한다.

`docs/archives/BLACK_FLASH_H0_PRE_REV1_SOURCES.zip`은 기존 ignored QA의 `ShibuyaPass4H0REV1QA/20261101/before/`에서 읽은 **초기 H0 Unity 소스·설정17개**와 SHA-256 manifest만 담은 작은 복원용 사본이다. 영상/이미지/로그/캐시가 없다. 원래 QA 사본을 변경하지 않았다. 현재 Git의 기본 구현은 REV1이며 이 아카이브를 자동으로 live Assets 위에 풀지 않는다. 모든 과거 중간 수정이 독립적인 Git 버전으로 존재한다는 뜻은 아니다.

아래 **66개 파일**만 선택적으로 저장한다.

- `.gitignore`
- `docs/CURRENT_HANDOFF.md`
- `docs/SHIBUYA_BLACK_FLASH_ART_DIRECTED_CORE_REVIEW.md`
- `docs/SHIBUYA_BLACK_FLASH_TEMPORAL_ASSESSMENT.md`
- `docs/SHIBUYA_BLACK_FLASH_TEMPORAL_SHAPE_RND.md`
- `docs/SHIBUYA_BLACK_FLASH_VISUAL_RND.md`
- `docs/SHIBUYA_PASS4H0_REVIEW.md`
- `docs/SHIBUYA_PASS4H0_VISUAL_REV1_REVIEW.md`
- `docs/SHIBUYA_PASS4H_WIP_CHECKPOINT.md`
- `docs/active/CURRENT_TASK.md`
- `docs/archives/BLACK_FLASH_H0_PRE_REV1_SOURCES.zip`
- `docs/design/BLACK_FLASH_VFX.md`
- `docs/references/README.md`
- `unity/Assets/Shibuya/Editor/Black Flash Art Directed QA.asset`
- `unity/Assets/Shibuya/Editor/Black Flash Art Directed QA.asset.meta`
- `unity/Assets/Shibuya/Editor/Black Flash Temporal Shape QA.asset`
- `unity/Assets/Shibuya/Editor/Black Flash Temporal Shape QA.asset.meta`
- `unity/Assets/Shibuya/Editor/BlackFlashArtAtlas.png`
- `unity/Assets/Shibuya/Editor/BlackFlashArtAtlas.png.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashArt.shader`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashArt.shader.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashArtPreview.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashArtPreview.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashArtProfile.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashArtProfile.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashInspector.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashInspector.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashReview.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashReview.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashRnd.shader`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashRnd.shader.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashRndPreview.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashRndPreview.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShape.shader`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShape.shader.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShapePreview.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShapePreview.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShapeProfile.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShapeProfile.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3CTestRunner.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4H0Rev1Tests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4H0Rev1Tests.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4H0Tests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4H0Tests.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4HArtDirectedTests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4HArtDirectedTests.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4HTemporalShapeTests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4HTemporalShapeTests.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4HVisualRndTests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4HVisualRndTests.cs.meta`
- `unity/Assets/Shibuya/Resources/Shibuya Black Flash.asset`
- `unity/Assets/Shibuya/Resources/Shibuya Black Flash.asset.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashGeometry.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashGeometry.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashPresentation.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashPresentation.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashProfile.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashProfile.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashVisual.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlackFlashVisual.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaMatch.cs`
- `unity/Assets/Shibuya/Shaders.meta`
- `unity/Assets/Shibuya/Shaders/ShibuyaBlackFlashInk.shader`
- `unity/Assets/Shibuya/Shaders/ShibuyaBlackFlashInk.shader.meta`
- `unity/Assets/Shibuya/Shaders/ShibuyaBlackFlashLight.shader`
- `unity/Assets/Shibuya/Shaders/ShibuyaBlackFlashLight.shader.meta`

## 제외하고 로컬 보존

- `unity/unity.slnx`: 기존 사용자 로컬 프로젝트 순서 변경을 보존하고 staging하지 않는다.
- `_local_refs/videos/blackflash_refs/`의 원본 MP4·`흑섬_레이어.png`: Git에 추가하지 않는다.
- `unity/Logs/`의 QA XML/trace/영상/이미지/생성 원화 중간본·보존 검사·실패 원본: 로컬 유지. 위의 작은 소스 복원 사본만 별도 보존한다.
- LocalModels/LocalAudio, Library/Temp/Cache/빌드 출력은 추가하지 않는다.
- 새 게임 원화는 `unity/Assets/Shibuya/Editor/BlackFlashArtAtlas.png`이며 참조 인포그래픽을 잘라 만든 이미지가 아니다. 해당 PNG와 importer .meta는 복원에 필요해 저장한다.

최종 staging 전 .meta/GUID/직렬화 참조와 문자열 에셋 경로, 파일 크기·비밀정보 패턴을 검사한다. QA 원본과 .slnx 보존은 별도 로컬 audit baseline으로 재확인한다. 기록 위치는 `unity/Logs/ShibuyaPass4HCheckpointQA/20261102/`이며 Git에 넣지 않는다.

## 복원·검수

현재 구현의 출발점은 이 WIP 커밋이다. Unity 원본 scene/prefab 및 Packages/ProjectSettings는 이번 커밋에서 바꾸지 않는다. 최신 새 시제품 설명은 [Art-Directed L2/L3 보고서](SHIBUYA_BLACK_FLASH_ART_DIRECTED_CORE_REVIEW.md), 앞선 실험은 [Temporal×Shape 보고서](SHIBUYA_BLACK_FLASH_TEMPORAL_SHAPE_RND.md)와 [Visual R&D 보고서](SHIBUYA_BLACK_FLASH_VISUAL_RND.md)를 따른다.

QA 메뉴는 `JJK Game → 시부야 PASS4H` 아래에 있으며 새 시제품은 `흑섬 아트 디렉션 L2·L3`다. 로컬 QA 미디어/원본 레퍼런스는 clone으로 내려오지 않는다. 코드·실제 아틀라스·설정은 저장하되 영상·실패 원본까지 GitHub에 백업한 것으로 오해하지 않는다.

다음 단계는 사용자가 요청한 **별도 통합 인수인계 문서 업로드**다. 이 체크포인트 이후 새 흑섬 구현·시제품 채택·USER VERIFIED 승격·다른 PASS를 시작하지 않는다.
