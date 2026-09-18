# New Chat Bootstrap — JJK_game

아래 블록을 새 ChatGPT 채팅의 첫 메시지로 붙여넣는다.

---

나는 GitHub 저장소 `JJuNi27/JJK_game`에서 Unity 6 URP 기반 주술회전 3D 캐릭터 액션 격투게임을 개발 중이야.

현재 branch는 `feat/gojo-blue-screen-distortion`이고, 2026-09-18 Production Purple code checkpoint는 `d2ee74e8c20da795c500971c393ac097b22a2e5a`다. master만 보고 상태를 추정하지 마라.

먼저 아래 순서로 읽고 상태를 복구해줘.

1. `AGENTS.md`
2. `docs/design/GAME_VISION.md`
3. `docs/CURRENT_HANDOFF.md`
4. `docs/active/CURRENT_TASK.md`
5. `docs/locked/USER_VERIFIED_SETTINGS.md`
6. `docs/design/VFX_DIRECTION.md`
7. `docs/architecture/GOJO_VFX.md`
8. 작업에 따라 `docs/architecture/CAMERA_PRESENTATION.md`, `DOMAIN_SYSTEM.md`, `COMBAT_DATA_DRIVEN.md`

최신 handoff/current task/locked settings가 오래된 roadmap·Gate 기록보다 우선한다. 같은 PC라면 먼저 `git status -sb`와 `git log -3 --oneline`을 확인하고, REMOTE·LOCAL-ONLY·CODEX VALIDATED·USER VERIFIED를 구분해라.

프로젝트 방향은 팬텀 퍼레이드의 기술 연출과 애니/만화 원작의 술식·영역·패시브 규칙을 최대한 고증하고, Strikeborn급 이상의 타격감·presentation density·environment reaction·aftermath를 지향하는 넓은 도시형 1:1 캐릭터 액션 격투게임이다. Gojo는 첫 production-quality 캐릭터일 뿐 고정 주인공이 아니다.

현재 Production Purple 후보는 **D2-R3 Body + OuterR2 + Release→Travel refinement**다. D2-R3/OuterR2는 charge와 travel에 적용되고 terminal explosion은 기존 Production 표현을 유지한다. Travel은 짧은 world-space residue, 굵고 짧은 magenta wake, release peak, rear-biased outer response를 사용하며 Body의 구형 정체성을 유지한다.

Production integration은 targeted 3 PASS / 0 FAIL, Travel refinement는 2 PASS / 0 FAIL, C#·shader error 0, Charge pixel difference 0이다. First-hit 종료, 뒤 target 피해 차단, no-hit max range terminal, cancel/cleanup/camera restore, repeated cleanup을 유지한다. 고정 observer 영상은 장면 가림으로 시각 합격 근거에서 제외했다.

이 상태는 **CODEX VALIDATED / USER VISUAL APPROVAL PENDING**이다. USER VERIFIED로 올리지 마라. 다음 즉시 작업은 최신 Travel refinement에 대한 Gemini feedback과 사용자/ChatGPT 영상 검수, Unity Play Mode 최종 시각 확인이다. 사용자가 Purple을 승인하기 전 새 Blue/Red/Domain visual track을 임의로 시작하지 마라.

Purple lineage는 A/B/C exploration → dark Hybrid D → bright D2 → R1 surface/internal 분리 → R2 depth/hairball 문제 → R3 chunky plasma/fused core → OuterR1 shield 문제 → OuterR2 irregular corona → reference-driven Travel refinement다.

영구 규칙:

- 코드는 HOW, Data Asset은 WHAT. 사용자-facing Inspector/Data UI는 한글이 기본.
- USER VERIFIED 값은 `docs/locked/USER_VERIFIED_SETTINGS.md`를 따르고 되돌리지 않는다.
- 큰 미적 선택이 여러 개면 2~4개 후보를 제시하고 사용자 선택을 기다린다. 사용자는 Art/VFX Director, AI는 production translation/implementation/validation 역할이다.
- 자동 테스트는 사용자 Play Mode 시각 승인을 대신하지 않는다.
- `Purple_UserPrototype`, 사용자 Scene/Animator/FBX, `unity/Assets/LocalModels/`, `unity/Assets/Resources/LocalAudio/`, `.blend`, local reference와 QA output을 보존한다.
- `_local_refs/videos/`의 reference는 local-only이며 Git에 올리지 않는다.
- reset/clean/restore와 dirty tree의 `git add .`를 사용하지 않는다. 사용자 승인 없이 commit/push/merge하지 않는다.

향후 Domain 아이디어는 barrier size의 Inspector/Data 조절, destruction 보류, Gojo hand sign/blindfold lowering와 trapped opponent reaction cinematic camera다. 현재 Purple 승인 작업과 섞지 않는다.

첫 응답에서는 현재 REMOTE와 LOCAL-ONLY 상태를 5~10줄로 요약하고, 보호값과 validation 수준을 구분하고, immediate next action 하나만 제시해라.

---

중요 결정이 바뀌면 `docs/CURRENT_HANDOFF.md`, 실행 작업은 `docs/active/CURRENT_TASK.md`, 실제 사용자 검증만 `docs/locked/USER_VERIFIED_SETTINGS.md`에 반영한다.
