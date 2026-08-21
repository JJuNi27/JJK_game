# Gate 2 — Team Match Architecture

작성 기준일: 2026-08-21

이 문서는 `주술사 되어보기`의 Team Match Architecture 진행 상태와 테스트 기준을 기록한다.

## 이전 Gate 상태

Gate 1 Core Combat은 사용자 확인 완료 상태다.

```text
현대 고죠
- 이동 / 기본 3타 / 회피
- 창 / 혁 / 허식 자
- 무량공처
- 무하한
- 영역전연 우회
- 술식 번아웃

시부야 스쿠나 · 이타도리 육체
- 해
- 팔
- 영역 밖 푸가
- 복마어주자
- 복마어주자 안 광역 푸가
- 스쿠나 전용 영역 사운드 경로
```

두 주령이 한 위치에 겹쳐 한 마리처럼 보이던 훈련장 AI 문제도 engagement slot으로 수정했고 사용자가 정상 확인했다.

## Gate 2를 나누는 이유

최종 추천 방향은 소규모 팀 기반 3D 아레나 격투지만, 현재 훈련장의 두 주령을 바로 Active/Reserve로 바꾸면 스쿠나의 다중 해·복마어주자·영역 푸가 회귀 테스트 환경이 사라진다.

따라서 Gate 2를 단계적으로 진행한다.

```text
Gate 2A
플레이어 팀 Active / Reserve 검증

Gate 2B
상대 팀 구조 대칭화 여부 검토
훈련용 다중 봇 환경과 실제 Team Battle 환경 분리 검토
```

## Gate 2A 목표

현재 팀:

```text
GOJO SATORU · 현대 · 교사
RYOMEN SUKUNA · 시부야 사변
```

현재 임시 교대 입력:

```text
T · TAG
```

`T`는 최종 조작 확정이 아닌 `GAME_ORIGINAL` 프로토타입 입력이다.

### Acceptance Criteria

```text
1. 장면 재시작 없이 고죠 ↔ 스쿠나 교대
2. Active 한 명 + Reserve 한 명
3. 각 캐릭터 HP 독립 저장
4. 각 캐릭터 CE 독립 저장
5. Reserve 상태에서는 저장된 HP/CE가 변하지 않음
6. 첫 KO 시 살아 있는 Reserve 자동 입장
7. 마지막 팀원 KO 시에만 DEFEAT
8. KO된 캐릭터로 수동 교대 불가
9. 술식/영역/회피 등 Action 중 수동 교대 불가
10. 교대 시 기본 3타 콤보 상태 초기화
11. 카메라와 Target Lock은 현재 Player shell을 계속 추적
12. 기존 고죠/스쿠나 기술과 두 주령 훈련 환경 유지
```

## 구현 방식

이번 단계에서는 두 캐릭터 GameObject를 동시에 생성하지 않는다.

기존 Player GameObject를 `Fighter Shell`처럼 유지하고, 활성 캐릭터의 구성 요소와 외형을 전환한다.

```text
Player shell
- CharacterController
- Health
- CursedEnergyController
- BasicAttack
- TargetLockController
- Camera target

PrototypeCharacterController
- 현재 캐릭터 기술/방어/영역/외형 활성화

PrototypePlayerTeamController
- 고죠 상태 snapshot
- 스쿠나 상태 snapshot
- Active / Reserve
- 수동 Tag
- KO Auto Tag
```

이 방식의 장점:

```text
- 기존 카메라 재연결 불필요
- 기존 Target Lock 재연결 불필요
- 기존 이동/공격 shell 재사용
- HP/CE 상태 보존 구조를 먼저 검증 가능
- 현재 캐릭터별 기술 컴포넌트 내부 쿨타임/준비 상태를 유지 가능
```

이번 단계가 성공한 뒤 실제 모델/애니메이션 파이프라인을 만들면서 `Fighter Entity`를 별도 오브젝트로 분리할 필요가 있는지 판단한다.

거대한 범용 Character/Ability framework를 지금 만들지는 않는다.

## 변경 파일

새 파일:

```text
unity/Assets/Scripts/Player/PrototypePlayerTeamController.cs
```

수정:

```text
unity/Assets/Scripts/Core/CombatInputBindings.cs
unity/Assets/Scripts/Core/Health.cs
unity/Assets/Scripts/Core/CursedEnergyController.cs
unity/Assets/Scripts/Player/BasicAttack.cs
unity/Assets/Scripts/Player/PrototypeCharacterController.cs
```

## 상태 Snapshot

캐릭터가 교대할 때 현재 Active 상태를 저장한다.

```text
CharacterId
Initialized
Current HP
Current CE
KO 여부
```

처음 한 번도 등장하지 않은 Reserve는 최초 등장 시 해당 캐릭터의 기존 시작값을 사용한다.

현재 예:

```text
현대 고죠
HP 100
CE 100

시부야 스쿠나
HP 100
CE 95
```

한 번 등장한 뒤에는 저장된 값을 그대로 복구한다.

### 현재 Reserve 회복 규칙

`GAME_ORIGINAL`

- Reserve에 있는 동안 HP 자동회복 없음.
- Reserve에 있는 동안 CE 자동회복도 없음.
- 즉 교대 순간 저장된 수치가 그대로 정지한다.

이 규칙은 최종 확정이 아니며 추후 원작/주팬퍼 캐릭터별 회복 패시브와 Team Battle 규칙을 설계할 때 재검토한다.

## KO Auto Tag

`Health.ReceiveDamage()`는 실제 HP가 0이 된 뒤 `DamageResolved`를 보내고, 그 다음 최종적으로 여전히 죽어 있으면 `Died`를 보낸다.

Gate 2A에서는 `PrototypePlayerTeamController`가 이 사이에 개입한다.

```text
Active HP 0
→ DamageResolved
→ Active snapshot = KO
→ 살아 있는 Reserve 확인
→ Reserve 캐릭터 적용
→ Reserve HP/CE 복구
→ Health가 더 이상 0이 아님
→ Died 발생하지 않음
```

Reserve도 이미 KO라면 복구하지 않는다.

```text
마지막 Active HP 0
→ DamageResolved
→ 살아 있는 Reserve 없음
→ Health 0 유지
→ Died
→ 기존 MatchController DEFEAT
```

낙사 `Kill()`도 `Environmental` DamageResolved를 발생시키도록 연결해 같은 팀 상태 경로를 사용한다.

단, 현재 Player shell 자체의 위치는 유지하므로 한 캐릭터가 맵 아래로 떨어져 KO되면 새 Reserve도 같은 위치에서 등장한다. 다음 프레임에 다시 낙사할 가능성이 있다. 실제 Team Spawn/Entry 연출을 만들 때 안전 위치 복귀를 추가해야 한다.

## 교대 제한

수동 T Tag는 현재 `CombatActionState.Normal`일 때만 가능하다.

따라서 아래 중에는 수동 교대를 막는다.

```text
회피 중
술식 시전 중
영역 입력 중
영역 활성 중
사망 상태
```

수동 교대:

```text
쿨타임 약 1.25초
교대 직후 무적 약 0.30초
```

KO 자동 교대:

```text
교대 직후 무적 약 0.80초
```

모두 현재 `GAME_ORIGINAL` 검증 수치다.

## UI

현재 Team prototype HUD를 별도로 표시한다.

```text
ACTIVE · 현재 캐릭터 HP/CE
TEAM · T TAG · READY / cooldown / ACTION LOCK
ACTIVE row
RESERVE row
KO 표시
```

기존 MatchController/캐릭터 HUD가 아직 완전히 Team-aware로 통합된 것은 아니다. 이번 2A에서는 Team state correctness를 먼저 검증한다. HUD 통합은 2A 정상 확인 뒤 정리한다.

## 사용자 테스트 순서

```text
1. git pull origin master
2. Unity Console 빨간 오류 없는지 확인
3. 게임 시작

[기본 교대]
4. 시작 캐릭터 고죠 확인
5. T
→ 장면 리로드 없이 바로 스쿠나로 변경
→ 스쿠나 외형/HUD/Q/E/R/V 확인
6. 다시 T
→ 장면 리로드 없이 고죠 복귀
→ 무하한/창/혁/자/무량공처 사용 가능

[HP 보존]
7. 고죠가 영역전연 주령에게 일부 피해를 받음
8. T → 스쿠나
9. T → 고죠
→ 이전에 깎인 고죠 HP 그대로인지

[CE 보존]
10. 스쿠나에서 Q 또는 E 사용해 CE 소비
11. T → 고죠
12. T → 스쿠나
→ 스쿠나 CE가 시작값 95로 리필되지 않고 이전 값으로 돌아오는지

[Action Lock]
13. 술식 시전 중 / 회피 중 / 영역 중 T
→ 교대되지 않는지

[첫 KO Auto Tag]
14. 고죠 또는 스쿠나 한 명을 적 공격으로 HP 0까지 둠
→ DEFEAT가 뜨지 않아야 함
→ 살아 있는 Reserve가 자동으로 Active가 되어야 함
→ Team HUD에서 전 캐릭터가 KO 표시

[최종 KO]
15. 남은 캐릭터도 HP 0
→ 그때 DEFEAT

[기존 전투 회귀]
16. TAB 두 적 타깃 가능
17. Curse A / Curse B 둘 다 보임
18. 고죠 무하한과 영역전연 우회 정상
19. 스쿠나 해/팔/푸가/복마어주자/영역 푸가 정상
```

## 사용자 테스트 진행 기록

2026-08-21:

```text
[USER VERIFIED]
- 게임 시작 후 고죠 → T → 스쿠나 교대 정상
- 장면 재시작 없이 캐릭터가 변경됨
- 다시 T로 교대하는 기본 Tag 동작 정상
- 고죠가 피해를 받은 뒤 스쿠나로 교대했다가 복귀해도 기존 HP가 그대로 보존됨
- 캐릭터별 HP snapshot 저장/복원 동작 정상

[아직 확인 필요]
- CE 독립 보존
- Reserve CE 상태 수치 정지
- Action Lock
- 첫 KO Auto Tag
- 최종 KO 시에만 DEFEAT
- KO 캐릭터 수동 교대 차단
- 교대 시 콤보 초기화
- 카메라/Target Lock 유지
- 기존 고죠/스쿠나 기술 회귀 여부
```

기본 교대와 HP 보존 Acceptance Criteria는 사용자 확인 완료했지만 Gate 2A 전체를 완료로 판정하지 않는다. 남은 CE 보존·KO·Action Lock·회귀 테스트까지 통과해야 한다.

## 테스트 대기 상태

```text
Gate 1 Core Combat: USER VERIFIED
적 겹침 수정: USER VERIFIED
Gate 2A Basic Tag: USER VERIFIED
Gate 2A HP Preservation: USER VERIFIED
Gate 2A Team Runtime: USER TEST IN PROGRESS
```

Assistant는 Unity를 직접 컴파일했다고 주장하지 않는다.

## Gate 2A 이후

정상 확인 시 다음 순서:

```text
1. Gate 2A 사용자 확인 완료 기록
2. Legacy HUD를 Team-aware HUD로 정리
3. 실제 Team Match에서 상대편도 Active/Reserve가 필요한 구조 설계
4. 훈련용 다중 Curse 모드와 Team Battle 모드 분리 여부 결정
5. Gate 2B 최소 상대 팀 구현
6. 카메라/Target/KO Entry 검증
7. 이후 Beauty Corner
```

Beauty Corner 전에는 지원공격, 합동 궁극기, 3인 팀, 편성 비용, 온라인을 넣지 않는다.
