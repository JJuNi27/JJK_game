# New Chat Bootstrap — JJK_game

새 ChatGPT 채팅에서 이 프로젝트를 바로 이어가기 위한 시작 지시문.

아래 블록을 새 채팅 첫 메시지로 붙여넣는다.

---

## Copy/Paste Prompt

나는 GitHub 저장소 `JJuNi27/JJK_game`에서 Unity 6 URP 기반 주술회전 3D 아레나 액션 게임을 개발 중이야.

이전 채팅이 길어져 새 채팅으로 넘어왔다.
내가 이전 내용을 다시 설명하게 하지 말고, 먼저 GitHub connector를 사용해서 아래 문서를 직접 읽고 현재 상태를 복구해줘.

반드시 읽을 문서:
1. `docs/CURRENT_HANDOFF.md`
2. `docs/DEVELOPMENT_ROADMAP.md`
3. `docs/GATE5B_FIRST_PRODUCTION_SLICE.md`
4. `docs/JJK_VFX_REFERENCE_GOJO.md`
5. `docs/JJK_VFX_EXTERNAL_IMPLEMENTATION_RESEARCH.md`

그리고 최신 `master` commit도 확인해.

현재 큰 단계는:
`Gate 5B First Production-Quality Vertical Slice`

현재 집중 작업은 고죠 VFX이고:
- Pass 8: USER VISUAL REVIEW FAILED / REWORK REQUIRED
- Pass 8R-1: USER VISUAL PARTIAL / DETAIL REWORK REQUIRED
- Pass 8R-2A: Gojo Blue Production VFX Benchmark를 구현했고 Unity 실제 검수 단계까지 왔음
- Blue는 아직 USER VERIFIED가 아님
- Red로 넘어가기 전에 Blue production benchmark를 합격시켜야 함

Blue에는 이미:
- custom URP procedural energy shader
- deep body / Fresnel shell / outer shell
- inward spiral particles
- broken convergence arcs
- impact collapse
- future distortion source hook

이 구현되어 있음.

Unity 실제 테스트에서 발생했던 오류:
- Particle orbital curve mode 불일치
- playing 상태에서 ParticleSystem duration 변경
- HLSL shader 인자 `point` 예약어 충돌

은 이미 master에서 수정되었으므로 같은 문제를 다시 처음부터 해결하려 하지 마.

외부 VFX 조사도 완료했으니
`docs/JJK_VFX_EXTERNAL_IMPLEMENTATION_RESEARCH.md`
내용을 반드시 기준에 포함해.

외부 reference 역할:
- starkim1999 = 원작 정체성 + inward/outward motion + distortion + Domain transition
- Zepwlert = 매우 많은 visual layer + screen effects/distortion benchmark
- Floppiii = particle textures/anime shader + cinematic set-piece benchmark
- Swammy UE5 JJK fan game = 실제 3D action-game readability/integration benchmark
- Deadlyxrebel Hollow Purple = debris/material reaction/stage transition 아이디어

우리 목표는 이 사람들의 asset을 복사하는 게 아니라 구현 원리를 우리 Unity 6 URP 구조에 맞게 재구성하는 것.

현재 Blue에서 가장 유력한 다음 개선 순서:
1. 실제 screen distortion proof
2. custom particle texture/mask
3. visual-only debris/dust suction
4. presentation 내부 timing stagger
5. 사용자 화면 기반 art tuning

중요한 개발 규칙:
- gameplay damage/CE/cooldown/range/hitbox/pull/push/stun/timing을 VFX 때문에 바꾸지 말 것
- Purple 기존 timing `0.24 / 0.78 / 18m` 보존
- Domain gameplay state/radius/duration/input 보존
- Pass 7의 camera/FOV/focus/flash/hit-stop ownership 침범 금지
- 큰 수정은 Codex에게 맡기고, 작은 1~2파일 수정은 직접 처리 가능
- 큰 작업은 feature branch → commit/push → GitHub 실제 diff 검토 → merge → Unity 사용자 테스트 순서
- Codex static build 성공을 Unity Play Mode 성공이라고 말하지 말 것
- 사용자 검수 전에는 USER VERIFIED로 닫지 말 것
- 사용자에게 전문 용어를 쓸 때는 짧게 뜻도 설명할 것
- 가능/불가능을 정확히 말하고 근거 없이 추측하지 말 것

새 채팅에서는 문서를 읽고:
1. 현재 master와 상태를 5~10줄로 요약하고
2. 내가 지금 당장 해야 할 다음 행동을 하나만 제시하고
3. 필요한 경우 Codex에 보낼 다음 프롬프트를 기존 handoff와 충돌 없게 작성해줘.

참고:
무량공처 장인 인식(MediaPipe/RandomForest) 관련 별도 문서는 다른 실험/MVP 흐름이다.
내가 명시적으로 연결하라고 하지 않는 한 현재 Unity `JJK_game` 전투 VFX 작업과 섞지 마.

---

## Handoff policy

- 새 중요 결정이 생기면 `docs/CURRENT_HANDOFF.md` 갱신
- 새 reference 조사 결과는 별도 docs 문서 또는 기존 reference 문서에 기록
- 사용자의 시각 판정을 status에 반영
- 다음 새 채팅에서도 이 bootstrap + CURRENT_HANDOFF만으로 재개 가능해야 함
