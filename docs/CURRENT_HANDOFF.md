# Current Project Handoff

작성 기준: **2026-10-02**

Branch: `feat/gojo-blue-screen-distortion`

Purple implementation checkpoint: `22cea8ed60ecafdf83ccc76860cd6bd87f7c4df4`

Commit message: `chore(vfx): checkpoint Purple tempest candidates pending visual review`

현재 상태: **CODEX VALIDATED / PENDING USER VISUAL REVIEW / NOT USER VERIFIED**

> 이 파일은 최신 상태 복구용 요약이다. 9월의 긴 후보 이력은 Git history와 각 `*_REVIEW.md`에 남아 있으므로 여기서는 현재 판단과 다음 행동을 우선한다.

## 1. 프로젝트 방향

Unity 6 URP 기반 넓은 도시형 **1:1 3D 액션 격투**.

- Gojo는 첫 production-quality 캐릭터일 뿐 고정 주인공이 아님
- 원작/팬텀 퍼레이드 계열의 캐릭터 정체성을 살리되 게임 가독성과 타격감을 우선
- 강한 VFX, camera/screen feedback, audio, environment reaction, aftermath까지 포함
- Data-driven: **코드는 HOW, Data Asset은 WHAT**
- 사용자-facing Inspector/Data 설정은 한국어 표시가 기본

전체 작업 규칙은 `AGENTS.md`를 따른다.

## 2. 현재 최우선 — Hollow Purple

현재 최신 후보:

- `PurpleTempestPolish`
- 기본 OFF
- Production/이전 후보 보존
- sphere/body identity 보존

기술 보고서:
- `docs/PURPLE_TEMPEST_POLISH_REVIEW.md`

실행 지침:
- `docs/active/CURRENT_TASK.md`

### 현재 사용자 판단

- 전체 방향: **CLOSER**
- Sphere/body: **KEEP**
- Neon discharge: **KEEP + POLISH**
- Charge/Travel wind 분리: 유효한 진전
- Wind visual quality: 아직 부족
- Dark cursed energy: 검은 국수/stroke 인상은 줄었지만 무게감도 너무 약해짐
- 승격/Production/USER VERIFIED 아님

### 중요한 Purple 해석 잠금

**Purple body는 sphere가 맞다.**

레퍼런스에서 불규칙하게 보이는 것은 sphere 자체를 jelly처럼 찢어서가 아니라 surrounding storm/energy가 edge를 가리고 침범하기 때문이다.

따라서 앞으로:
- body를 다시 과도하게 deform하지 않는다.
- stable sphere는 유지한다.
- wrapper/pressure/bright discharge/dark cursed energy로 위협감을 만든다.

목표 개념:

> **Stable purple sphere + surrounding space unable to withstand it.**

### 다음 Purple 우선순위

1. Dark cursed energy의 무게감 복구
   - 일정한 black outline 금지
   - closed noodle/띠로 돌아가지 않음
   - black은 내부 암부/먹힌 영역/깊이/찢어진 면으로 존재
2. Wind visual polish
   - Charge의 white/gray hurricane
   - Release에서 압축/파열/방향 전환
   - Travel에서 동일 cyclone을 그대로 들고 가지 않음
3. Neon을 보존하면서 다른 두 layer와 spatial overlap/occlusion 강화

새 VFX 종류를 계속 추가하기보다 현재 세 layer가 하나의 폭풍으로 읽히게 만드는 것이 핵심이다.

## 3. 최신 Purple technical checkpoint

`22cea8ed60ecafdf83ccc76860cd6bd87f7c4df4`

- Purple 관련 193개 파일 selective checkpoint
- unrelated tracked/untracked local work는 제외/보존
- `PurpleTempestLayers` / `PurpleTempestPolish` 및 필요한 Purple 의존 코드·자산·QA 문서 포함
- standalone Blue/Red, VFXLab Scene, Packages/ProjectSettings, 기타 로컬 작업은 checkpoint에서 제외

`PurpleTempestPolish` checked scope:
- targeted 7 PASS / 0 FAIL
- C#/shader/final console error 0
- cleanup material/mesh 0/0
- A/B pose/scale/active 385 samples 동일
- terminal pixel diff 0
- historical full-image comparison: **INCONCLUSIVE**
- full regression / real gameplay damage test는 해당 pass에서 재실행하지 않음

Technical PASS는 visual approval과 다르다.

## 4. QA 원칙

Purple 기본 visual QA는 **밝은 neutral 환경**.

- pure white 배경 금지
- daylight-like light
- QA 환경 때문에 Purple 자체 색/emission/bloom을 별도 보정하지 않음
- Side/Caster 기본
- distant Observer는 큰 맵에서 위협감 검증용 보조

원거리 목표 감정:

> "와 맞으면 뒤질뻔 했겠네;; 저게 뭐냐.."

실제 플레이에서는 효과가 예상보다 약하게 보일 수 있으므로, 안전성과 가독성이 유지된다면 지나치게 소심하게 시작하지 않는다.

## 5. 레퍼런스 해석

Final moving/presentation authority는 Gojo Hollow Purple 발사 reference 영상.

핵심:
- Blue: inward suction/vortex/pressure
- Red: outward pressure/repulsion
- Fusion/Charge: white/gray hurricane-like pressure가 하단/주변 공간을 휘감음
- Purple bright layer: white-hot + neon purple/pink discharge
- Purple dark layer: deep purple/near-black cursed-energy mass/tear/boundary
- bright와 dark는 concentric ring처럼 깨끗이 분리되지 않고 서로 overlap
- 모든 요소가 같은 방향으로 constant orbit하지 않음
- 작은 cross flare는 accent이며 핵심 구조보다 후순위

Reference media는 local-only일 수 있으므로 관련 구현 전 실제 로컬 파일을 확인한다.

## 6. 보호된 기준

다음은 사용자 요청 없이 변경하지 않는다.

- `docs/locked/USER_VERIFIED_SETTINGS.md`
- Gojo movement USER VERIFIED 값
- Unlimited Void gameplay/verified visual 기준
- 기존 Gojo Blue verified baseline
- 보호된 Purple production/gameplay semantics
- corrected release origin/axis
- 사용자 Scene / Animator / FBX / LocalModels / LocalAudio
- unrelated dirty working tree

자동화/테스트/Codex는 `USER VERIFIED`를 만들 수 없다.

## 7. Git 안전

원래 로컬 working tree는 의도적으로 unrelated dirty work가 존재한다.

- `git reset --hard` 금지
- `git clean` 금지
- 사용자 작업을 덮는 checkout/restore 금지
- dirty tree에서 `git add .` 금지
- 수정 전 `git status` 확인
- commit/push는 사용자 승인 후 selective staging

2026-10-02 Purple checkpoint는 위 원칙으로 selective push 완료.

## 8. Gojo 장기 완성 계획

새 문서:
- `docs/GOJO_FINALIZATION_PLAN.md`

큰 방향:

```text
Purple refinement
→ Blue final rework
→ Red final rework
→ Skill presentation / audio / camera polish
→ Gojo combat motion 정리
→ Awakening
```

Awakening 계획:
- 기본 상태 `R = 각성`
- 컨셉 대사: "진심으로 가볼까"
- 손으로 blindfold를 실제로 아래로 내리는 Hero Animation
- Six Eyes reveal
- 각성 후 `R = Hollow Purple`
- 각성 Q/E는 Blue/Red를 이용한 타격/콤보형 기술 후보

현재는 future plan이며 Purple과 동시에 구현하지 않는다.

## 9. 캐릭터 모델/모션 생산 문제

새 문서:
- `docs/architecture/CHARACTER_PRODUCTION_PIPELINE.md`

사용자 조건:
- 취미 프로젝트
- 외주/유료 제작 계획 없음
- 3D 격투 방향 유지

핵심 전략:

> **캐릭터를 매번 새로 만드는 대신 Character Production Pipeline을 만든다.**

구성:
- Human Base 계열
- Unity Humanoid 공통 규격
- shared locomotion/hit/down
- Fist / Sword archetype base motion
- 캐릭터별 평타/기술은 base에서 변형
- Hero animation은 정말 중요한 캐릭터 순간에만 투자
- 공통 toon/material/style rule로 모델 출처 차이를 줄임

예상 분류:
- Human/hand: Gojo, Sukuna, Megumi, Choso
- Human/weapon: Yuta
- Large/monster: Mahoraga 별도 계열

Gojo 완료 후 곧바로 Yuta 전체를 만들기보다 `Yuta Dummy`/human dummy로 retarget + sword pipeline을 먼저 검증하는 것이 권장된다.

## 10. 모델/모션 현실적 원칙

- 캐릭터별 평타 모션이 완전히 사라지는 것은 아니다.
- 공통화 목적은 **0부터 만드는 일을 줄이는 것**이다.
- 모델은 장기적으로 라이선스/스타일이 명확한 공통 human base 계열을 검토한다.
- VRoid/Blender/free mocap/Mixamo 계열은 후보이며 production 적용 전 검증한다.
- 현재 Gojo 모델은 이 계획 때문에 교체하지 않는다.
- 랜덤한 추출 모델을 캐릭터마다 그대로 최종 사용해 스타일을 맞추는 방식을 장기 표준으로 삼지 않는다.

## 11. Blindfold lowering 현실적 구현 방향

단순 mesh ON/OFF가 아닌 실제 lowering을 목표로 할 수 있다.

권장:
- separate skinned blindfold mesh
- blindfold bones and/or Blender shape keys
- hand animation과 deformation 동기화
- 얼굴 곡면 통과 보정
- camera framing/hand occlusion로 작은 clipping 숨김

초기 목표에서 full cloth simulation / finger-level cloth physics는 제외한다.

자세한 내용은 `docs/GOJO_FINALIZATION_PLAN.md` 참고.

## 12. AI 작업 분담

권장 routing:

- **Astra High**: reference interpretation, 구조적 VFX 판단, 원인 불명 시각 문제, architecture ambiguity
- **Sol**: 방향이 확정된 구현, polish, Git-safe execution/review
- bounded task가 충분히 명확할 때 더 가벼운 실행 모델 사용 가능

구조적 애매함이 생기면 억지 구현보다 `ASTRA ESCALATION`으로 멈춘다.

## 13. 새 세션 시작 시 읽을 순서

1. `AGENTS.md`
2. `docs/CURRENT_HANDOFF.md`
3. `docs/active/CURRENT_TASK.md`
4. `docs/locked/USER_VERIFIED_SETTINGS.md`
5. Purple이면 `docs/PURPLE_TEMPEST_POLISH_REVIEW.md`
6. Gojo 장기 계획이면 `docs/GOJO_FINALIZATION_PLAN.md`
7. 모델/리깅/캐릭터 생산이면 `docs/architecture/CHARACTER_PRODUCTION_PIPELINE.md`
8. 관련 architecture/design 문서

## 즉시 다음 행동

사용자가 Purple 작업을 계속하라고 요청하면 `PurpleTempestPolish`를 기반으로 **dark cursed energy 무게감 → wind transition/quality → neon overlap** 순서로 진행한다.

사용자 승인 없이:
- 후보 기본 ON
- Production 승격
- USER VERIFIED 선언
- 각성/모델 파이프라인 동시 착수
- unrelated local cleanup

을 하지 않는다.
