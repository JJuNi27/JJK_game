# 현재 작업 — Fourth Polish 이후 사용자 검수 / 다음 분리 패스

상태: **이전 P0~P6 자동검증 완료 / 사용자 실플레이 피드백 반영 완료 / 다음 구현은 Sol → Astra 순서 권장**

## 이전 pass 공식 종료 상태

FourthPolishQA 기준:
- Full Unity regression: **25 / 25 통과**
- targeted validation: 최종 안정화 검사 통과
- Python: unittest 11 / 11, pytest 2 / 2 통과
- Blue / 기존 Unlimited Void blue-black nebula / 보호 대상 로컬 자산 보존
- commit / push / merge 없음

중요:
자동검증 결과보다 **사용자 실제 Play Mode 검수 결과가 우선**한다.

사용자 실제 검수 결과:
- Domain 종료 후 basic melee: **여전히 실패**
- 따라서 P0는 아직 해결되지 않음

---

# 다음 작업은 두 트랙으로 분리

## Track A — Sol 전달: 구조 / 한글화 / 실제 P0 수정

목표:
시각 결과를 가능한 한 변경하지 않고, 재사용 가능한 다중 캐릭터 구조와 사용자 조절성을 개선한다.

### A0. Domain 종료 후 평타 실제 미해결
사용자가 실제 CombatMVP에서 Domain 종료 후 평타가 여전히 나오지 않는 것을 확인함.

완료 조건:
Character Select
→ CombatMVP
→ Unlimited Void
→ Domain 종료
→ 실제 공격 입력
→ 실제 BasicAttack animation / pose / hit sequence가 정상 시작

규칙:
- technique burnout은 의도대로 유지 가능
- 평타/물리 공격만 복구
- 기존 자동 replay만으로 해결 주장 금지
- 가능하면 실제 input path와 scene transition을 그대로 재현
- 자동테스트는 사용자 실제 결과를 대신하지 않음

### A1. Inspector 사용자-facing 한글화
사용자가 직접 조절하는 Inspector/Data 표시를 가능한 한 한글화.

원칙:
- C# identifier / class / enum 이름: 영어 유지
- Header / Tooltip / Custom Inspector Label: 한글
- 과도한 CustomEditor 작성 금지
- 기존 SerializeField 연결을 깨지 않음

예:
- Walk Speed → 걷기 속도
- Run Speed → 달리기 속도
- Domain Active Duration → 영역 지속시간
- Blue/Red/Purple voice field도 사용자에게 읽기 쉽게 표시

자세한 기준:
`docs/architecture/COMBAT_DATA_DRIVEN.md`

### A2. VFXLab은 Gojo 전용 scene이 아님
매우 중요:
**Gojo는 첫 번째 production-quality 캐릭터일 뿐이며 게임의 주인공으로 고정된 캐릭터가 아니다.**

VFXLab의 장기 역할:
- Character Select
- 선택 캐릭터의 Animation / VFX / Audio / Technique / Domain preview
- 이후 Sukuna / Yuji / Yuta / Megumi / Maki 등 확장 가능

따라서 안전한 범위에서:
- Gojo-specific naming / wiring / inspector layout을 공통 구조로 분리
- 기존 Gojo 동작은 그대로 유지
- 지금 당장 Character Select 완전체를 만들 필요는 없음
- 다음 캐릭터가 들어와도 시스템을 다시 새로 만들 필요가 없도록 seam/interface/data 구조를 마련

### A3. Audio / Voice 구조 일반화
현재 `Blue Voice / Red Voice / Purple Voice / Domain Voice`처럼 Gojo 기술이 공통 컴포넌트에 직접 박혀 보이는 구조를 조사.

장기 목표 예:
- CharacterPresentationProfile
- TechniquePresentationProfile
- CombatAudioProfile / VoiceProfile

기술별:
- Voice
- Cast SFX
- Release SFX
- Travel SFX
- Impact SFX
- Timing / Beat hook

이번 Sol pass에서는:
- 기존 오디오 동작을 깨지 않는 최소 안전 일반화
- 실제 새 캐릭터용 asset 제작 금지
- 대규모 폴더 이동 금지
- binary asset 이동 금지

### A4. Data-driven 준비
이번 pass에서 전체 AbilityDefinition migration을 끝내려 하지 않는다.

가능한 범위:
- 기존 hardcoded / serialized tuning 값을 조사
- 사용자 조절값은 한글 Inspector로 노출
- 다음 Character/Ability/Profile 확장을 위한 구조적 seam만 마련

금지:
- 전체 전투 시스템 대규모 재작성
- Blue/Red/Purple 시각 결과 변경
- USER VERIFIED movement 변경

---

## Track B — Astra 전달: Strikeborn Impact Standard 기반 VFX enhancement

자세한 방향:
`docs/design/VFX_DIRECTION.md`

핵심 목표:
**팬텀 퍼레이드 + 원작 고증을 유지하면서, 실제 플레이의 연출 밀도/타격감/잔류감은 Strikeborn급 이상을 목표로 한다.**

"기술 본체 하나 + 펑"에서 끝내지 않는다.

공통 presentation 단계:
Anticipation
→ Charge
→ Release
→ Travel
→ Impact
→ Environment Reaction
→ Aftermath
→ Camera / Screen FX
→ Audio hook / Cleanup

### B0. 기존 Blue baseline 보호 + 2차 enhancement 허용
이전 Blue는 1차 완성 baseline으로 보호한다.

보존:
- 4-hit gameplay
- core / attraction / singularity
- 현재 debris 방향
- 현재 사용자가 만족한 기본 형태

이제 허용:
- Strikeborn-style impact density
- 환경 반응
- 마지막 collapse의 hit feel
- restrained camera/screen feedback
- residual spatial shimmer / aftermath
- 기존 visual identity를 망가뜨리지 않는 추가 레이어

즉 "갈아엎기"가 아니라 **baseline 위에 presentation layer 강화**.

### B1. Red — Strikeborn급 repulsion presentation
현재 gameplay/collision 구조는 보존.

VFX 목표:
- compressed anticipation
- 강한 release
- travel 중 공기/압력 반응
- impact flash
- irregular repulsive shockwave
- environment debris / dust
- 넓은 회백색 pressure vapor
- residual pressure aftermath

금지:
- fireball처럼 보이기
- cartoon smoke puff
- 충돌 후 빨간 projectile가 계속 생존

원작의 "척력" 정체성을 유지하면서 Strikeborn의 연출 밀도를 적용한다.

### B2. Hollow Purple
현재 좋은 formation / fusion은 강하게 보호.

실제 사용자 피드백:
**완성된 Purple이 발사될 때 약간 아래 방향으로 나감.**

수정:
- Gojo 중앙축에서 형성
- launch vector가 의도한 수평 정면축을 따르도록 실제 anchor/vector 원인 수정
- 단순 눈속임 offset으로 덮지 않음

2차 enhancement:
- release flash / compression
- stronger launch readability
- FOV / camera impulse는 과하지 않게
- travel distortion
- branching violet lightning
- environment reaction
- scar / residual electricity / spatial shimmer
- 기술이 지나간 공간에 결과가 남는 presentation

보존:
- fusion
- 0.32s hold baseline
- 현재 넓어진 scar width 관계

### B3. Cosmic Eye — 거의 완료, pulse 제거
현재 reference fidelity는 크게 개선됨.

사용자 만족:
"진짜 많이 비슷해졌다. 조금만 더"

새 요구:
- Eye 전체가 숨쉬듯/심장 뛰듯 scale/intensity pulse하는 움직임 제거
- pupil / 전체 iris silhouette는 안정적으로 유지
- cloud / rim / tail의 subtle flow는 유지 가능
- reference와 더 가까운 정적이고 압도적인 landmark 느낌

기존 Astra reference 분석 수치/구조 유지:
`docs/references/gojo/unlimited_void/COSMIC_EYE_REFERENCE_ANALYSIS.md`

### B4. Domain release ceiling/dome artifact 제거
현재 caster-centered world-space release 방향은 유지.

사용자 영상에서 release 중:
- 천장
- 돔 내부면
- shell / ceiling
처럼 읽히는 이상한 면이 나타남.

요구:
- 해당 artifact의 실제 원인 추적 후 제거
- infinite / orientation-ambiguous Unlimited Void 느낌 유지
- USER VERIFIED blue-black nebula background는 변경하지 않음
- participant restoration / camera handoff / cleanup 보존

---

# 공통 잠금 / 원칙

## 보호
- 사용자 검증 완료 locomotion
- Blue 1차 baseline
- Unlimited Void blue-black nebula background
- Purple formation/fusion
- Domain capture / barrier / isolated interior / restoration architecture

## VFX 품질 기준
앞으로 화려함을 임의로 줄이지 않는다.
실제 플레이에서:
- 타격 순간
- 환경 반응
- 화면/카메라 feedback
- aftermath
까지 포함해서 평가한다.

## 사용자 승인 전
- USER VERIFIED 표현 금지
- commit / push / merge 금지
