# 현재 작업 — Production Purple 사용자 시각 승인 대기

작성 기준: **2026-09-18**

Branch: `feat/gojo-blue-screen-distortion`

Remote code checkpoint: `d2ee74e8c20da795c500971c393ac097b22a2e5a`

상태: **CODEX VALIDATED / USER VISUAL APPROVAL PENDING**

현재 immediate task는 새 Purple 기능 구현이 아니다. Production 후보는 **D2-R3 Body + OuterR2 + Release→Travel refinement**이며, charge/travel에만 적용된다. Terminal explosion은 기존 Production visual을 유지한다.

## 다음 순서

1. 최신 Travel refinement 영상에 대한 Gemini feedback을 받는다.
2. 사용자와 ChatGPT가 실제 Caster/Side 영상을 검수한다.
3. Unity Play Mode에서 최종 visual을 확인한다.
4. 승인되면 Purple visual R&D를 종료한다.
5. 이후 사용자 선택에 따라 Blue distortion/particle/environment reaction, Red pressure/range/residual pressure, Domain visual/camera 중 다음 track으로 이동한다.

Purple 승인 전에는 새 visual track을 임의로 시작하지 않는다.

## 검증 기준

- Production integration: **3 PASS / 0 FAIL**, C#·shader error 0.
- Travel refinement: **2 PASS / 0 FAIL**, C#·shader error 0.
- Charge pixel difference 0.
- first target hit 종료, behind-target damage 없음, no-hit max-range terminal, cancel/cleanup/camera restore, repeated lifecycle cleanup 유지.
- Caster/Side readability 확인. 고정 observer는 장면 가림 때문에 시각 합격 근거에서 제외.
- 자동 테스트와 Codex 렌더 검토는 사용자 Play Mode 승인과 같지 않다.

## 보호 범위

- D2-R3 Body, OuterR2 Charge, gameplay 수치와 semantics, canonical timing, camera architecture, terminal explosion.
- `Purple_UserPrototype`, 사용자 Scene/Animator/FBX, LocalModels, LocalAudio, local reference, QA output.
- USER VERIFIED 설정은 `docs/locked/USER_VERIFIED_SETTINGS.md`를 따른다.

결과 문서:

- `docs/PURPLE_PRODUCTION_INTEGRATION_REVIEW.md`
- `docs/PURPLE_TRAVEL_REFINEMENT_REVIEW.md`
- `docs/PURPLE_D2_R3_REVIEW.md`
- `docs/PURPLE_D2_R3_OUTER_R2_REVIEW.md`

## Domain future queue

Barrier 크기의 Inspector/Data 조절, barrier destruction 보류, Gojo hand sign/blindfold lowering와 trapped opponent reaction cinematic camera 후보를 기록만 한다. Purple 승인 작업과 섞지 않는다.
