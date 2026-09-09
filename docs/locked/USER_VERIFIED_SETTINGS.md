# 사용자 검증 완료 설정 — 명시적 요청 없이는 변경 금지

이 문서는 사용자가 Unity / Blender에서 직접 확인한 값과 동작을 기록한다.
자동 테스트만 통과한 결과보다 우선한다.

## 고죠 이동

### Movement Profile
- Walk Speed: **3**
- Run Speed: **14**

### Animator Blend Tree
| Motion | Threshold | Time Scale |
|---|---:|---:|
| Gojo_Idle | 0 | 1.00 |
| Gojo_Walk | 3 | 1.15 |
| Gojo_Run | 14 | 1.00 |

### 입력
- 일반 이동: Walk
- Shift + 이동: Run
- Space: Dodge / Evade

### 사용자 검증 완료 전환 흐름
`Idle → Walk → Run → Walk → Idle`

## 보호된 애니메이션 Asset
- Gojo_Idle
- Gojo_Walk
- Gojo_Run
- Gojo_Walk_Base (backup)

관련 없는 VFX 작업 때문에 locomotion이나 위 애니메이션 관계를 변경하지 않는다.

## 보호된 Gameplay / Presentation 기준
작업에서 명시적으로 변경을 요구하지 않는 한 다음을 보존한다.
- 현재 Animator 설정
- evade 동작
- Gojo Blue의 기존 **4-hit gameplay**
- 현재 성공적인 Purple formation / fusion 기반
- 현재 성공적인 Unlimited Void 보라색 선 터널
- 현재 카메라 피드백 소유 구조
- 현재 Domain capture / barrier / isolated interior / participant restoration 구조

## 상태 용어
항상 아래 용어를 같은 의미로 사용한다.
- **사용자 검증 완료 (USER VERIFIED)**: 사용자가 Unity / Blender에서 직접 확인함.
- **코덱스 검증 완료 (CODEX VALIDATED)**: compile/test/자동 preview는 통과했지만 사용자의 시각 승인은 아직 없음.
- **사용자 시각 검토 대기 (PENDING USER VISUAL REVIEW)**: 기술적으로 구현됐지만 사용자의 Play Mode 판단이 필요함.
