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
2. `docs/GATE5B_FIRST_PRODUCTION_SLICE.md`
3. `docs/DEVELOPMENT_ROADMAP.md`
4. `docs/JJK_VFX_REFERENCE_GOJO.md`
5. `docs/JJK_VFX_EXTERNAL_IMPLEMENTATION_RESEARCH.md`

현재 작업 branch는 우선 `feat/gojo-blue-screen-distortion`를 확인한다.
master만 보고 현재 작업 상태를 추정하지 말 것.

현재 큰 단계:
`Gate 5B First Production-Quality Vertical Slice`

현재 확정 상태:
- Pass 1~7: USER VERIFIED
- Pass 8: USER VISUAL REVIEW FAILED / REWORK REQUIRED
- Pass 8R-1: USER VISUAL PARTIAL / DETAIL REWORK REQUIRED
- Pass 8R-2A Gojo Blue Production VFX Benchmark: USER VERIFIED / CLOSED
- Gojo Blue 4-hit gameplay: accepted as part of Blue slice
- Gojo Blue voice playback: USER VERIFIED LOCALLY
- Gojo adult blindfold model + Humanoid pipeline: USER VERIFIED LOCALLY
- Gojo custom Idle animation: USER VERIFIED LOCALLY
- 다음 작업: Run locomotion

중요: Blue는 더 이상 USER VISUAL TEST PENDING이 아니다.
사용자가 최종적으로 합격 판정을 했으므로 Blue를 다시 pending으로 되돌리지 말 것.

현재 사용자 지시 작업 순서:
1. Blue 완성 — DONE
2. Blue voice — DONE
3. Gojo model + animation integration — CURRENT
4. Run locomotion — NEXT
5. 필요 시 Walk/locomotion 보완
6. Blue casting motion + hand/anchor integration
7. Basic Attack 1 / 2 / Finisher
8. 그 뒤 Red

Gojo animation pipeline에서 매우 중요한 해결 사항:
- 원본 Mixamo `Gojo_Blindfold_RiggedAvatar`와 Blender-export animation FBX를 섞었을 때
  `Transform 'Armature' not found in HumanDescription` hierarchy mismatch가 발생했다.
- 별도 Humanoid Avatar를 만들면 손목/짝다리 포즈가 Unity retarget 과정에서 달라졌다.
- 해결은 Blender에서 Mesh + Armature를 animation 없이 `Gojo_Blender_Master.fbx`로 export하고,
  Unity에서 `Gojo_Blender_MasterAvatar`를 기준 Avatar로 만든 것이다.
- 이후 같은 Blender Armature에서 export한 animation FBX는
  `Humanoid → Copy From Other Avatar → Gojo_Blender_MasterAvatar`를 사용한다.
- VFXLab actual model도 `Gojo_Blender_Master`를 사용한다.
- Animator는 `Gojo_Animator`, Avatar는 `Gojo_Blender_MasterAvatar`, Apply Root Motion은 OFF.
- 이 구조로 바꾼 뒤 Blender의 손목/짝다리 포즈가 Unity에서도 정상 재생됨을 사용자가 직접 확인했다.

현재 custom Idle:
- Blender Action: `Gojo_Idle`
- 30 fps
- frame 1 / 30 / 60
- 양손 주머니
- 한쪽 다리 체중 이동
- Head/Neck 중심의 아주 작은 breathing loop
- Unity 실제 재생 USER VERIFIED LOCALLY

모델/음성 저작권성 자산은 local-only 정책.
- `unity/Assets/Resources/LocalAudio/`는 gitignore됨
- `사운드 모음/`도 gitignore됨
- `unity/Assets/LocalModels/`은 원격 .gitignore에 아직 없을 수 있으므로 다음 commit 전에 반드시 실제 `.gitignore`와 `git status`를 확인하고, 모델/텍스처가 공개 GitHub에 올라가지 않게 처리할 것.

매우 중요:
현재 Gojo model / textures / Animator / Blender-export FBX / VFXLab scene 변경은 사용자 PC 로컬에만 있을 수 있다.
GitHub에 이미 존재한다고 가정하지 말고, 다음 Git 작업을 시작하기 전에 사용자 로컬 `git status` / diff를 확인한다.
REMOTE IMPLEMENTED와 USER VERIFIED LOCALLY를 구분한다.

Blue local voice:
- `unity/Assets/Resources/LocalAudio/Gojo_Blue.ogg`
- Unity에서 실제 재생 사용자 확인 완료
- GitHub에는 올리지 않는 local asset

Unity Editor에서 반복되는:
- `MissingReferenceException: m_Targets of GameObjectInspector...`
- `SerializedObjectNotCreatableException: Object at index 0 is null`

은 현재 작업을 막지 않는 Unity Editor Inspector stale-reference 계열로 취급 중이다.
stack trace가 프로젝트 `Assets/Scripts`를 가리키거나 실제 기능을 막기 전에는 우선순위를 올리지 않는다.

중요한 개발 규칙:
- Purple 기존 timing `0.24 / 0.78 / 18m` 보존
- Domain gameplay state/radius/duration/input 보존
- Pass 7 camera/FOV/focus/flash/hit-stop ownership 침범 금지
- 큰 작업은 feature branch → commit/push → GitHub 실제 diff 검토 → 사용자 Unity test 순서
- master merge는 사용자 승인 전 하지 말 것
- Codex static build 성공을 Unity Play Mode 성공이라고 말하지 말 것
- 사용자 검수 전에는 USER VERIFIED로 닫지 말 것
- 반대로 사용자가 이미 USER VERIFIED한 상태를 다시 pending으로 되돌리지 말 것
- 전문 용어는 짧게 뜻도 설명할 것
- 가능/불가능을 정확히 말하고 근거 없이 추측하지 말 것

새 채팅에서는 문서를 읽고:
1. 현재 branch/remote 상태와 local-only handoff를 5~10줄로 요약하고
2. 사용자가 다음에 해야 할 행동은 Run locomotion 작업 하나부터 제시하고
3. 필요하면 Mixamo In Place Run → Blender 보정 → 같은 Master Avatar pipeline → Unity 검증 순서로 안내해줘.

참고:
무량공처 장인 인식(MediaPipe/RandomForest) 관련 별도 문서는 다른 실험/MVP 흐름이다.
내가 명시적으로 연결하라고 하지 않는 한 현재 Unity `JJK_game` 전투/VFX/animation 작업과 섞지 마.

---

## Handoff policy

- 새 중요 결정이 생기면 `docs/CURRENT_HANDOFF.md` 갱신
- 사용자의 시각 판정을 status에 즉시 반영
- REMOTE / LOCAL-ONLY / USER VERIFIED 상태를 명확히 구분
- local proprietary/copyright-sensitive assets가 GitHub에 올라가지 않게 확인
- 다음 새 채팅에서도 이 bootstrap + CURRENT_HANDOFF만으로 재개 가능해야 함
