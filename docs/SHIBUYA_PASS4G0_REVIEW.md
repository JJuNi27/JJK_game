# PASS 4G.0 — 공격 분류와 시부야 주력 200

> 2026-10-31 체크포인트 보충: PASS4G 핵심 동작은 사용자 MVP 승인을 받았다. 공격 분류의 모든 예외·주력/아우라 정적 비교까지 사용자 직접 검증으로 확대하지 않는다. 정확한 승인 범위·파일 목록은 `docs/SHIBUYA_PASS4G_CHECKPOINT.md`. 아래 LOCAL UNCOMMITTED/검수 대기는 구현 당시 기록이다.

2026-10-31. LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기.
기준 로컬·원격 `ce6626ba6148c34e9ea4b98024a47556f9645854`, 브랜치 `feat/shibuya-combat-mvp`.

## 구현

- 독립적인 reach/nature/origin 및 치명타·흑섬·직접 복합 타격 허용 메타데이터. 미지정 신규 공격은 등급 비허용. Core DamageContext 의미 불변.
- 명시한 기존 6개 자산의 13개 메타데이터만 업그레이드: 기본4, 업/다운2, 공중 이벤트2, 기존 PASS3A 예제4, 잡기1. 잡기는 other/치명타·흑섬 비허용이며 피해15 유지. 공중의 3단계는 기존 2개 타격 템플릿을 재사용하므로 이벤트 정의와 실행 횟수를 혼동하지 않는다.
- Shibuya Cursed Energy만 MaxEnergy/StartingEnergy 100→200. F 기본12/초, 소비 배율1, 시작 비율1 유지.
- 캐릭터 전투 프로필에 `auraCapacityReference=200` 추가. VFX가 읽는 용량은 `MaxEnergy * VFX.ReferenceMaxEnergy / CharacterReference`이므로 이전100/100과 새200/200이 같다. 다른 프로필의0은 기존 VFX 기준 fallback. 공유 VFX Curve·색상·Shader·Geometry·프리셋은 변경하지 않는다.
- 실제 Inspector의 메타데이터 및 캐릭터 기준 용량은 한국어 SerializedProperty 편집. Undo 검증.
- 표준 명칭: `docs/design/COMBAT_ACTION_VOCABULARY.md`.

## 검증과 실패 이력

원본 루트 `unity/Logs/ShibuyaPass4GQA/20261031-combat-grades/`.

1. `g0-validation/results.xml`: **11개 중10 PASS/1 FAIL**. 지상 기본·W/S·공중5히트·카메라 기준8방향 대시·패링·잡기·AI 기존 검사8개와 자원/아우라 검사2개 통과.
2. 실패는 새 데이터 검사가 SerializedProperty 배열 컨테이너를 단일 strike로 취급한 NullReference. 테스트에 `!it.isArray`만 추가. 런타임·기대값 수정 없음.
3. `g0-data-recheck/results.xml`: 해당1개 **1 PASS/0 FAIL**. 최신 고유11 PASS는 2회 실행 집계이며 단일 전체11 PASS라고 하지 않는다.

`g0-validation/legacy/ShibuyaPass4B3QA/4G0-energy.json`: 주력200, 비용20→현재180, HUD 입력 비율.9, F 회복12/초 및 최대치 홀드·재입력 확인.
같은 폴더에서 동일 프레임/포즈/시점의 `4G0-aura-100.png`와 `4G0-aura-200.png`는 **PNG 바이트 동일**, 메시 정점·전신 Expansion 동일. F 해제/재진입 fade도 통과. 시각 품질에 대한 사용자 최종 승인은 별도다.

`classification-before.json`/`classification-after.json`에서 메타데이터를 제외한 기존 직렬화 값이 동일함을 확인한 뒤 용량·캐릭터 기준값만 변경했다. `repository-before-sha256.json`은 초기 tracked 파일과 기존 dirty `.slnx` 바이트의 비교 기준이다. 기존 QA 삭제·변경 없음. staging/commit/push 없음.

Unity가 기존 예제 자산에서 생략됐던 필드의 현재 기본값(guard/targeting/air/down 등)을 명시적으로 직렬화하고 일부 필드 순서를 바꿨다. 기존 런타임 의미가 같은지는 JSON 비교로 확인했으며, diff 줄 수를 모두 신규 밸런스 변경으로 해석하지 않는다. 기존 `.meta` GUID는 유지했다. 최종 `final-targeted`에서도 아우라 동일성1 PASS를 재확인했다.

HUD 연결은 기존 `ShibuyaMatch.DrawEnergy`의 PlayerEnergy.Normalized/CurrentEnergy/MaxEnergy 직접 참조를 읽기 전용 확인했다. 자원 비율과 최대치 변경을 검사했으며, 위 카메라 PNG는 Overlay/OnGUI HUD의 픽셀 비교 자료가 아니다. HUD 디자인은 변경하지 않았다.
