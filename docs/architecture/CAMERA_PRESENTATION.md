# 카메라 Presentation / Domain 시네마틱 소유권

## 소유권 원칙
Normal combat camera와 매 프레임 싸우지 않는다.
임시 cinematic override / shot-director layer를 사용한다.

`Normal Combat Camera`
→ `Domain Cinematic Override`
→ temporary shots
→ complete / cancel
→ normal camera state 정확히 복구

## 기존 Presentation 소유권 보존
프로젝트에는 이미 다음과 같은 presentation feedback 소유 구조가 있다.
- shake
- FOV
- focus
- flash
- hit-stop 관련 feedback

불필요하게 중복 시스템을 만들지 않는다.

## 복구 요구사항
완료 또는 중단 시 다음을 정확히 복구:
- FOV
- follow target
- camera target
- camera mode/state
- temporary transform 변경

영구적인 transform drift 금지.

## Full cinematic 조건
유효한 enemy가 최소 1명 capture된 경우에만 victim 중심 full cinematic을 사용한다.
Enemy가 없으면 더 짧은 Gojo/domain-only sequence.

## Primary cinematic victim
결정적 우선순위:
1. 기존 lock-on target이 captured + valid
2. 가장 가까운 captured opponent
3. 안정적인 deterministic fallback

이 선택은 presentation 전용이다.
Gameplay 효과는 captured enemy 전체에 적용된다.

## 의도한 Shot language
- Hand / seal detail
- Blindfold / side-face close-up hook
- Six Eyes extreme close-up hook
- Gojo hero front shot
- Barrier closure wide
- 기존 Purple tunnel abduction
- victim reaction close-up
- Domain arrival / reveal

authored animation이 없는데 blindfold mesh 파괴 애니메이션을 가짜로 만들지 않는다.
facial rig가 없다면 억지 표정 애니메이션 대신 stunned pose / head orientation / camera framing을 사용한다.

## 톤
Unlimited Void 카메라는:
- 차분함
- 불길함
- 압도감
- 서두르지 않는 호흡

을 가져야 한다.

frantic hyper-cut pacing은 피한다.
Blindfold / Six Eyes / domain seal animation과 voice가 실제로 준비되면 최종 timing을 다시 다듬는다.

## Domain exit
center-outward dissolve / barrier-release를 사용할 경우:
- dissolve 중 camera ownership이 안정적이어야 한다.
- 안전한 handoff 지점에서 normal combat camera로 복귀한다.
- participant를 복구한 뒤 camera가 isolated interior에 남아 있으면 안 된다.

## VFXLab 안전
VFXLab manual/orbit camera control은 preview 뒤에도 살아 있어야 한다.
반복 preview 후에도 preview camera가 정확히 복구되어야 한다.
