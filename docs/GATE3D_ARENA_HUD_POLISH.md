# Gate 3D — Representative Arena + HUD Polish

작성 기준일: 2026-08-23

## 상태

```text
Gate 3A Combat Feel: USER VERIFIED
Gate 3B Prototype Character Presentation: USER VERIFIED
Gate 3C Signature Technique Presentation: USER VERIFIED

Gate 3D Representative Arena + HUD Polish: FINAL REGRESSION
Gate 3D Pass 1 · Arena Mood Scaffold: USER VERIFIED
Gate 3D Pass 2 · Team HUD Visual Integration: USER VERIFIED
Gate 3D Pass 3 · Contextual Skill Deck HUD: USER FEEDBACK · VISUAL OK / FINAL REGRESSION PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Gate 3D 목적

Gate 3D는 전투 규칙을 바꾸지 않고 Beauty Corner의 화면 정보와 대표 전장 분위기를 정리하는 단계다.

```text
1. 대표 전장 분위기
2. 캐릭터/술식이 읽히는 배경 명암
3. HUD와 월드 연출의 시각적 통합
4. Gate 1~3 핵심 기능 최종 회귀
```

실제 최종 맵/모델/UI 자산이 아직 없으므로 현재 구현은 `GAME_ORIGINAL prototype presentation`이다.

---

# Pass 1 — Arena Mood Scaffold · USER VERIFIED

파일:

```text
unity/Assets/Scripts/Core/PrototypeBeautyArenaPresentation.cs
```

`CombatMVP`에서만 동작한다.

```text
- 야간 Ambient / Linear Fog
- 플레이 영역 밖 비충돌 도심 실루엣
- 청색 / 적색 / 주황 Neon accent
- 바닥 Arena ring / Street lane accent
- 차가운 조명 + 따뜻한 조명
```

건물/장식 Collider는 의도적으로 제거했다. 현재 건물은 배경 실루엣이므로 통과 가능이 정상이다.

2026-08-22 사용자 확인:

```text
건물 실루엣 정상
맵 링 정상
장식 건물 통과 정상
```

---

# Pass 2 — Team HUD Visual Integration · USER VERIFIED

파일:

```text
unity/Assets/Scripts/Player/PrototypePlayerTeamController.cs
unity/Assets/Scripts/Enemy/PrototypeOpponentTeamController.cs
```

유지 정보:

```text
Active / Reserve
HP / CE
Tag Ready / Cooldown / Action Lock
KO
Opponent Reserve Entry
F2 Encounter Mode
```

기능은 그대로 두고 Player/Opponent HUD를 야간 Beauty Corner에 맞는 전투 HUD plate로 정리했다.

2026-08-22 사용자 확인:

```text
HUD 변경 정상
Tag / HP / CE / Opponent Team 표시 정상
```

---

# Pass 3 — Contextual Skill Deck HUD

파일:

```text
unity/Assets/Scripts/Core/PrototypeSkillDeckHud.cs
```

현재 Active Fighter의 대표 술식을 우하단에서 표시한다.

고죠:

```text
Q 창
E 혁
R 허식 자
V 무량공처
```

스쿠나:

```text
Q 해
E 팔
R 푸가
V 복마어주자
```

상태 표현:

```text
READY
DODGE
CASTING
DOMAIN INPUT
DOMAIN ACTIVE
TECHNIQUE BURNOUT
DISABLED
```

Q/E/R/V 입력 순간 해당 chip이 짧게 강조된다. 사용할 수 없는 상태에서는 chip이 흐려진다.

사용자 첫 인상:

```text
"좋긴하네 나중에 점차적으로 좋아질거 같은데?"
```

따라서 화면 표시 자체는 확인됐지만, Tag / Domain / Burnout / 입력 pulse까지 한 번에 보는 최종 회귀를 아직 수행하지 않았으므로 완전한 USER VERIFIED는 보류한다.

---

# Gate 3D Final Regression

상세 절차:

```text
docs/GATE3_FINAL_REGRESSION.md
```

이번 검증에서 새 기능을 추가하지 않는다.

목표는 지금까지 검증한 것들이 한 플레이 세션 안에서 서로 깨지지 않는지 확인하는 것이다.

통과 기준:

```text
- Console 빨간 오류 없음
- Gojo / Sukuna Tag 및 HP/CE 보존 정상
- 기본 3타 Hit Stop / Flash / Impact VFX 정상
- 허식 자가 레이저가 아니라 청/적 합류 → 큰 보라 구체 형태
- 푸가 발사/폭발 표현 정상
- 무량공처 / 복마어주자 표현과 Camera Focus 정상
- 야간 Arena / 비충돌 건물 / Ring 정상
- Player / Opponent Team HUD 정상
- Skill Deck가 Active Fighter와 함께 교체되고 입력/상태 표현 정상
- Team Battle 첫 KO 후 Reserve Entry + Target Lock 승계 정상
- 마지막 상대 KO에만 VICTORY
- 마지막 Player KO에만 DEFEAT
```

이 묶음이 통과하면:

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
```

로 닫고 Gate 4 Production Pipeline Extraction으로 넘어간다.

## 바꾸지 않은 것

Gate 3D Presentation 작업은 다음 Gameplay를 변경하지 않는다.

```text
Input Binding
Damage
HP / CE 규칙
Cooldown
Cast Time
Domain Rule
Tag
Target Lock
Match Result
```

## 최종 자산 관련 주의

Gate 3 완료는 `최종 그래픽 완성`을 의미하지 않는다.

아직 자산 의존 작업:

```text
실제 리깅 고죠/스쿠나 모델
Animator 기반 이동/공격/회피/술식 애니메이션
최종 Environment Model / Texture / Decal / Post Process
Canvas/TMP 기반 완성형 HUD
실제 Voice / SFX / Music 자산
```

Gate 3에서 증명하는 것은 이 자산들을 연결할 `전투 감각 / 표현 타이밍 / 정보 구조`다.
