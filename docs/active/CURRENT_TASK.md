# 현재 작업 — Purple Tempest polish 시각 refinement

작성 기준: **2026-10-02**

Branch: `feat/gojo-blue-screen-distortion`

Purple code checkpoint: `22cea8ed60ecafdf83ccc76860cd6bd87f7c4df4`

Checkpoint message: `chore(vfx): checkpoint Purple tempest candidates pending visual review`

상태: **CODEX VALIDATED / PENDING USER VISUAL REVIEW / NOT USER VERIFIED**

## 현재 핵심

현재 최신 후보는 **`PurpleTempestPolish`**다.

- saved default OFF
- Production/기존 후보/구체 본체 보존
- historical full-image equality는 **INCONCLUSIVE**
- targeted validation은 checked scope에서 **7 PASS / 0 FAIL**
- C#/shader/final console error 0
- cleanup material/mesh 0/0
- A/B의 위치·크기·활성 상태 385 sample 동일
- terminal pixel diff 0
- full regression / real gameplay damage test는 이 pass에서 재실행하지 않음

자세한 기술 보고서:
- `docs/PURPLE_TEMPEST_POLISH_REVIEW.md`

## 현재 visual 판단 — 사용자 피드백

이 평가는 USER VERIFIED 승격이 아니다.

### 전체

**CLOSER. 방향은 맞지만 아직 부족.**

### Sphere / body

**KEEP.**

Purple body는 안정적인 보라 구체가 맞다. 구체 자체를 irregular/jelly 형태로 다시 찢는 방향으로 돌아가지 않는다.

목표는:

> Stable Purple sphere + surrounding space unable to withstand it.

### Dark cursed energy / 검보라 주력

이번 polish에서 이전의 `보라 굵은 띠 + 일정한 검은 stroke` 인상은 줄었다.

하지만 trade-off로:
- 검보라의 무게감이 너무 약해짐
- 작은 검보라 조각/연기 조각처럼 흩어지는 순간이 있음
- 먼 시점에서 lethal mass가 약해질 수 있음

다음 방향:
- 검정 외곽선을 다시 두르지 않는다.
- dark layer의 공간 점유/두께/무게감은 다시 키운다.
- 검정은 stroke가 아니라 내부 암부, 먹힌 영역, 찢어진 면, 깊이로 존재한다.
- 선/국수로 돌아가지 않으면서 굵은 cursed-energy mass가 되도록 한다.

### Wind / pressure

이번 pass의 중요한 성과:
- Charge 회오리가 projectile에 그대로 붙어서 Travel하는 문제를 줄임
- Charge와 Travel의 역할을 분리함

아직 남은 문제:
- 흰/회백 풍압이 깨끗한 원호/리본처럼 보임
- 공간 자체가 압력으로 밀려나는 질량감이 부족함
- Charge → Release → Travel 전환이 하나의 흐름이 변형되는 것보다 효과 교체처럼 보일 수 있음

다음 방향:

```text
Charge/Fusion
- 캐릭터/구체 하단/주변 공간을 크게 휘감는 white/gray hurricane pressure

Release
- 회전 흐름이 압축/파열되며 진행 방향으로 전환

Travel
- 동일 cyclone 유지 금지
- directional pressure / lateral shear / wake 언어
```

새로운 travel effect 종류를 무조건 추가하라는 뜻은 아니다. 기존 요소를 먼저 재구성한다.

### Neon discharge

현재 세 outer layer 중 가장 가능성이 높다.

**KEEP + POLISH.**

다음 방향:
- 기존 white + neon purple/pink 정체성 유지
- 모든 arc가 같은 길이/굵기/방사형이 되지 않게 함
- 일부가 body bloom/dark current/wind에 묻혔다 다시 드러나게 함
- front/back depth와 spatial overlap 강화
- 너무 긴 clean cable처럼 보이는 arc는 줄임

## 현재 가장 중요한 문제

세 layer의 존재 여부가 아니라 **관계**다.

이미 발생 주기와 움직임 속도 차이는 어느 정도 구현돼 있다.

다음에는:
- 같은 공간에서 서로 가림
- 다른 layer를 침범
- 밝기/암부가 서로 연결
- 일부가 묻혔다 나타남
- Charge/Release/Travel에서 역할이 바뀜

이 필요하다.

`풍압 ON + 검보라 ON + 네온 ON`처럼 세 시스템이 분리되어 읽히지 않고 하나의 Purple storm으로 보여야 한다.

## 다음 구현 우선순위

1. **Dark cursed energy 무게감 복구**
   - stroke/closed noodle 없이 더 크고 무거운 mass
2. **Wind visual quality + Release transition**
   - 깨끗한 리본 감소
   - Charge 흐름이 발사 순간 방향을 바꾸는 연결감
3. **Neon과 두 layer의 중첩/가림/연결 polish**
4. Bright Side/Caster + 필요 시 distant Observer에서 재검토

새 시스템을 무작정 추가하기 전에 현재 `PurpleTempestPolish`의 세 layer를 polish한다.

## QA 환경

Purple 시각 QA 기본:
- 밝은 neutral 환경
- pure white 배경 금지
- daylight-like lighting
- QA 환경 때문에 Purple emission/bloom/color를 별도로 보정하지 않음
- Side/Caster를 기본 비교
- 큰 맵에서의 위협감을 보기 위한 distant/Observer를 보조 사용

목표 원거리 인상:

> "와 맞으면 뒤질뻔 했겠네;; 저게 뭐냐.."

안전성과 가독성이 유지된다면 지나치게 소심하게 시작하지 않는다. 큰 wrapper/space occupation을 허용하고 사용자 review에서 줄이는 쪽을 선호한다.

## Reference 해석 잠금

- Purple **body는 sphere가 맞다**.
- 레퍼런스에서 edge가 불규칙해 보이는 주된 이유는 외부 storm/energy가 겹치고 가리기 때문이다.
- sphere를 jelly/deformed body로 만드는 방식으로 해결하지 않는다.
- bright discharge와 dark cursed-energy shape는 역할이 다르지만 spatially overlap한다.
- dark shape를 전부 lightning으로 해석하지 않는다.
- white/gray lower wind는 purple ribbon이 아니다.
- 모든 layer를 같은 방향으로 계속 orbit시키지 않는다.

최종 움직임/presentation authority는 현재 프로젝트의 Gojo Hollow Purple 발사 영상 reference를 따른다. repository의 reference media가 local-only일 수 있으므로 작업 전에 실제 로컬 reference를 확인한다.

## 보호 범위

- 기존 USER VERIFIED 값: `docs/locked/USER_VERIFIED_SETTINGS.md`
- Production Purple / 이전 후보
- sphere/body identity 및 크기/궤적
- fusion/contact/release timing
- gameplay semantics
- camera architecture
- terminal explosion
- 사용자 Scene / Animator / FBX / LocalModels / LocalAudio
- unrelated dirty working tree

후보의 technical validation을 USER VERIFIED로 표현하지 않는다.

## Git 상태 주의

2026-10-02 checkpoint에서 Purple 관련 파일만 selective commit/push했다.

원래 로컬에는 checkpoint에서 제외한 tracked/untracked 작업이 남아 있었다. 이후 작업자는 실제 working tree를 먼저 확인하며 이 파일들을 reset/clean/restore하지 않는다. `git add .` 금지.

## Future queue — 현재 작업과 섞지 않음

Gojo 장기 완성 계획:
- `docs/GOJO_FINALIZATION_PLAN.md`

캐릭터 모델/리깅/평타/양산 문제를 해결하기 위한 zero-budget 3D pipeline:
- `docs/architecture/CHARACTER_PRODUCTION_PIPELINE.md`

각성/blindfold/model pipeline 구현은 현재 Purple 작업과 동시에 시작하지 않는다.
