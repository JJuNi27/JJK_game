# 사운드 / 음성 / Presentation Beat 아키텍처

이 문서는 앞으로 평타, 회피, Blue, Red, Purple, Unlimited Void 등 모든 기술의 사운드를 붙일 때 공통 기준으로 사용한다.

## 핵심 결론
**짧은 행동/타격 SFX는 이벤트별로 분리하고, 연속된 음성 문장은 하나의 clip으로 유지한다.**
애니메이션과 사운드 중 하나를 다른 하나의 절대 기준으로 삼기보다,
공통된 **Beat / Timeline 이벤트**에 둘 다 동기화한다.

## 왜 이렇게 하는가
예: 평타 3타
- 주먹 1
- 주먹 2
- 발차기 1

이 세 타격을 하나의 긴 SFX 파일로 묶으면:
- animation timing을 바꿀 때 오디오 전체를 다시 편집해야 함
- 각 hit frame에 맞춘 세밀한 조절이 어려움
- hit confirm / miss / cancel에 대응하기 어려움

따라서 각 타격은 독립 event가 더 적합하다.

## 권장 Combat SFX 구조

### Basic Attack 3-hit 예
Beat / Hit 1:
- punch whoosh
- impact SFX

Beat / Hit 2:
- punch whoosh
- impact SFX

Beat / Hit 3:
- kick whoosh
- impact SFX

각 event는 Inspector/Data에서:
- clip
- volume
- pitch
- delay / beat offset
등을 조절할 수 있는 방향을 선호.

## Voice 구조
연속된 하나의 문장/대사는 불필요하게 잘게 자르지 않는다.

예:
- "영역전개, 무량공처" 같은 연속 음성
- 하나의 voice clip으로 유지 가능

대신:
- voice clip 시작
- hand-seal beat
- White Blood group beat
- barrier close
- tunnel
같은 presentation event들을 공통 timeline에 맞춘다.

## 공통 Beat Clock
현재 프로젝트의 `PresentationBeatClock` 같은 공통 timing layer를 활용하는 방향을 선호한다.

예:
`Beat 1 → animation pose + SFX`
`Beat 2 → next strike + SFX`
`Beat 3 → kick + SFX`

Unlimited Void 예:
`Voice Start`
→ `White Blood Group 1`
→ `pause`
→ `White Blood Group 2 sub-beat A/B`
→ `pause`
→ `White Blood Group 3`

## 중요한 설계 원칙
- animation time을 audio file에 hard-bind하지 않는다.
- audio 전체를 animation clip 길이에 hard-bind하지 않는다.
- 둘 다 조절 가능한 beat/event timing을 공유한다.
- hit/miss/cancel 상태에 대응할 수 있도록 짧은 combat SFX는 분리.
- long voice line은 필요 이상으로 잘게 자르지 않는다.
- 사용자 튜닝을 위해 Inspector/Data-driven 값을 노출한다.

## 앞으로의 방향
사용자는 최종적으로:
- dodge sound
- basic attack sound
- Red
- Purple
- Unlimited Void
- voice
를 추가할 예정.

따라서 지금부터 새 기능은 가능하면 이 공통 timing 구조와 호환되게 만든다.
