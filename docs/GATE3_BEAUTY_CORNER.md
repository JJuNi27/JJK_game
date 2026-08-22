# Gate 3 — Beauty Corner

작성 기준일: 2026-08-23

## 현재 상태

```text
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED

Gate 3 Beauty Corner: FINAL REGRESSION
Gate 3A Combat Feel: USER VERIFIED
Gate 3B Prototype Character Presentation: USER VERIFIED
Gate 3C Signature Technique Presentation: USER VERIFIED
Gate 3D Arena + HUD Polish: FINAL REGRESSION
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

Gate 3 최종 회귀 절차:

```text
docs/GATE3_FINAL_REGRESSION.md
```

## Gate 3 목적

Gate 3은 게임 전체를 90% 완성하는 단계가 아니다.

대표 전투 범위에서:

```text
Combat Feel
Character Presentation
Signature Technique Presentation
Arena Mood
HUD Information Hierarchy
```

가 실제 플레이에서 함께 성립하는지 증명하는 단계다.

실제 모델/애니메이션/환경/UI 아트 자산은 이후 교체할 수 있지만, 연결 규칙과 표현 타이밍은 지금 검증한다.

---

# 3A — Combat Feel · USER VERIFIED

검증 완료:

```text
기본 3타 적중 Camera Shake
짧은 Hit Stop
Hit Flash
실제 Impact 위치 Procedural Burst
1타 < 2타 < 3타 FINISH 강도
허공/방어 시 잘못된 적중 피드백 방지
```

핵심 파일:

```text
unity/Assets/Scripts/Player/BasicAttack.cs
unity/Assets/Scripts/Core/PrototypeHitStopController.cs
unity/Assets/Scripts/Core/PrototypeHitImpactVfx.cs
unity/Assets/Scripts/Camera/SimpleCameraFollow.cs
```

---

# 3B — Prototype Character Presentation · USER VERIFIED

실제 리깅 모델/Animator가 아직 없기 때문에 최종 애니메이션 제작은 자산 의존 상태다.

현재 Prototype에서 검증한 것:

```text
고죠 / 스쿠나 Fighter별 기본 자세 차이
이동 lean
Dodge presentation
Tag entry presentation
Fighter Shell 유지 상태에서 Active Fighter 표현 교체 가능
```

현재 procedural 자세는 최종 애니메이션이 아니다.

실제 모델이 들어오면 Animator 기반 전용 모션으로 교체한다.

상세:

```text
docs/GATE3B_CHARACTER_PRESENTATION.md
```

---

# 3C — Signature Technique Presentation · USER VERIFIED

검증 완료:

```text
Signature Flash / Shake / Hit Stop
FOV Kick
Camera World Focus
Spatial Ring / Burst VFX
푸가 Release / Explosion emphasis
무량공처 / 복마어주자 anticipation + activation
```

허식 「자」 형태 교정:

```text
기존 긴 Purple LineRenderer Beam
→ 폐기

청색 + 적색 구체 수렴
→ 큰 보라 구체 생성
→ 전방 이동
```

상세:

```text
docs/GATE3C_SIGNATURE_TECHNIQUE_PRESENTATION.md
docs/GATE3C_PASS3_SPATIAL_VFX.md
docs/GATE3C_PASS4_CINEMATIC_FRAMING.md
```

현재 Prototype 제한:

```text
허식 자 Visual은 이동하는 구체
Damage는 기존 즉시 Capsule 판정
```

Damage timing과 Visual projectile 일치는 Gate 4 이후 production 구조에서 다룬다.

---

# 3D — Arena + HUD Polish · FINAL REGRESSION

상세:

```text
docs/GATE3D_ARENA_HUD_POLISH.md
```

Pass 1 Arena Mood Scaffold:

```text
USER VERIFIED

야간 도심 분위기
Fog
비충돌 건물 실루엣
Neon accent
Arena ring
Mood light
```

Pass 2 Team HUD Visual Integration:

```text
USER VERIFIED

Player Active / Reserve
Opponent Active / Reserve
HP / CE
Tag / KO / Reserve Entry
F2 Mode strip
```

Pass 3 Contextual Skill Deck:

```text
화면 표시 사용자 긍정 피드백 확인
최종 회귀에서 상태 전환까지 묶어서 검증 예정

고죠: Q 창 / E 혁 / R 허식 자 / V 무량공처
스쿠나: Q 해 / E 팔 / R 푸가 / V 복마어주자
```

---

# Gate 3 종료 조건

다음을 한 세션에서 확인하면 닫는다.

```text
1. Console 빨간 오류 없음
2. Player Tag / HP / CE / KO Auto Tag 회귀 없음
3. Opponent Team / Reserve Entry / Victory 회귀 없음
4. Target Lock 정상
5. 기본 Hit Feedback 정상
6. 고죠 대표 술식 + 무량공처 정상
7. 스쿠나 대표 술식 + 복마어주자 + 푸가 정상
8. 허식 자가 레이저가 아닌 구체형
9. Arena presentation 정상
10. HUD / Skill Deck가 Active Fighter와 Match State를 잘못 표시하지 않음
```

통과 시:

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: STARTED
```

---

# Gate 4로 넘길 것

Beauty Corner에서 실제 반복된 구조만 공통화한다.

```text
Character Asset Contract
Character Presentation Data
Animation Event Contract
Technique Presentation Profile
VFX Spawn / Follow / Stop lifecycle
Voice / SFX Event Contract
Camera Feedback Request
Hit Stop Request
HUD Fighter/Skill Data Binding
```

하지 않을 것:

```text
모든 캐릭터를 억지로 같은 Ability 구조에 넣는 거대 범용 시스템
지원 공격 / 합동기 / 3인 팀 완성형 확장
온라인 선행 구현
```

Gate 4 계획:

```text
docs/GATE4_PRODUCTION_PIPELINE_PLAN.md
```

## Codex 사용 기준

Gate 3의 작은 체감 튜닝은 직접 수정 비중이 높았다.

Gate 4부터는 여러 파일에 걸친 반복 구조 추출/이관이 주 작업이므로 Codex류 agent를 활용하기 좋은 구간이다.

단, 이 ChatGPT 대화 자체에서 별도 Codex 실행기가 노출되지 않은 경우 Codex를 실행했다고 가장하지 않는다. 사용자가 로컬 Unity AI Gateway / Codex 환경을 연결한 경우에는 해당 환경에서 Editor/코드 작업을 나누고, Git diff와 실제 Unity 검증을 기준으로 통합한다.
