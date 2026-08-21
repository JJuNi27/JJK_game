# Team-aware HUD Cleanup

작성 기준일: 2026-08-21

이 문서는 Gate 2A 사용자 검증 완료 뒤 진행한 HUD 정리 상태와 사용자 테스트 기준을 기록한다.

## 상태

```text
Gate 2A Team Runtime: USER VERIFIED
Team-aware HUD Cleanup: USER VERIFIED
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 작업 목적

Gate 2A에서는 팀 상태 정확성을 먼저 검증하기 위해 새 `PrototypePlayerTeamController` HUD를 기존 HUD 위에 추가했다.

그 결과 팀 모드에서는 다음 UI 책임이 중복될 수 있었다.

```text
MatchController
- 기존 PLAYER HP/CE 패널
- 고죠 영역 패널
- F1 조작 도움말

PrototypeCharacterController
- 스쿠나 PLAYER HP/CE 패널
- 스쿠나 영역 패널
- 스쿠나 F1 도움말

PrototypePlayerTeamController
- ACTIVE HP/CE 패널
- ACTIVE / RESERVE 팀 패널
```

이번 작업은 Beauty Corner가 아니다. 최종 디자인을 만드는 것이 아니라 팀전에서 HUD 정보가 한 번만, 현재 Active 캐릭터 기준으로 정확히 보이도록 책임을 정리한다.

## 변경 내용

### MatchController

- Team HUD가 활성화되어 있으면 기존 PLAYER HP/CE 패널을 그리지 않는다.
- 현재 Active가 스쿠나면 고죠 영역 패널을 그리지 않는다.
- F1 도움말을 현재 Active 캐릭터 기준으로 표시한다.
- 팀 모드에서는 도움말에 `T 팀 교대`를 표시한다.
- 적 HP, 적 수, 회피 상태, 기본공격 콤보, 적 공격 경고, 승패 UI는 기존대로 유지한다.

### PrototypeCharacterController

- 팀 모드에서는 기존 스쿠나 PLAYER HP/CE 패널을 그리지 않는다.
- 팀 모드에서는 스쿠나 전용 중복 F1 도움말을 그리지 않는다.
- 스쿠나가 Active일 때 스쿠나 영역 상태 패널은 계속 담당한다.
- 비팀/레거시 모드의 1/2 캐릭터 전환 HUD는 유지한다.

### PrototypePlayerTeamController

- ACTIVE HP/CE를 팀 모드의 단일 플레이어 패널로 사용한다.
- ACTIVE 행의 HP/CE는 snapshot이 아니라 현재 실시간 값을 표시한다.
- RESERVE 행은 저장된 snapshot 값을 표시한다.
- `READY / cooldown / ACTION LOCK / RESERVE KO` 상태는 유지한다.
- 첫 HUD 정리에서는 TEAM 패널을 좌측 상단 y=108에 배치했으나, 실제 사용자 캡처에서 고죠/스쿠나 Q/E/R 기술 카드와 겹치는 것이 확인됐다.
- 수정 후 TEAM 패널은 좌측 하단의 빈 공간을 사용하며 화면 높이에 따라 `Screen.height - 200` 부근으로 이동한다.
- 500px 높이 Game View 기준 약 y=300에 배치되어 상단 기술 카드와 하단 영역전개 패널 사이 공간을 사용한다.

## 사용자 시각 확인 기록

### 2026-08-21 첫 HUD 정리 캡처

```text
[정상 확인]
- 고죠 Active HP/CE 패널 중복 없음
- 스쿠나 Active HP/CE 패널 중복 없음
- 고죠 ↔ 스쿠나 교대 시 패널 색상/이름 전환 정상
- Active HP 실시간 감소 표시 정상
- Active CE 실시간 소비 표시 정상
- 고죠 Active일 때 고죠 영역 패널 표시
- 스쿠나 Active일 때 고죠 영역 패널이 사라지고 복마어주자 패널 표시

[수정 필요 확인]
- TEAM 패널이 Q/E/R 기술 카드 뒤에 겹쳐 ACTIVE/RESERVE 정보 가독성 저하
```

위 겹침 문제는 `Move team HUD clear of technique cards` 변경으로 수정했다.

### 2026-08-21 수정 후 사용자 캡처

```text
[USER VERIFIED]
- 고죠 Active 화면에서 TEAM 패널이 Q/E/R 기술 카드와 완전히 분리됨
- 스쿠나 Active 화면에서도 TEAM 패널이 Q/E/R 기술 카드와 겹치지 않음
- TEAM 패널이 좌측 중하단 빈 공간에 독립적으로 표시됨
- 고죠 Active/Reserve 행 표시 정상
- 스쿠나 Active/Reserve 행 표시 정상
- 고죠 ↔ 스쿠나 교대 시 ACTIVE 이름/색상 전환 정상
- Active HP/CE와 Reserve snapshot 정보 표시 정상
- 고죠 영역 상태 패널과 스쿠나 복마어주자 패널 전환 정상
- 화면 하단 영역 패널과 TEAM 패널도 겹치지 않음
```

스쿠나 캡처의 CE가 시작값보다 높게 보이는 것은 기존 스쿠나 주력 프로필의 초당 회복이 진행된 결과이며 HUD 배치 문제와 무관하다.

## 회귀 상태

Gate 2A 완료 시 이미 다음 전투 회귀 항목을 사용자 확인했다.

```text
T 교대 정상
TAB Target Lock 정상
회피/기본공격/술식 정상
첫 KO Auto Tag 정상
최종 팀원 KO 시 DEFEAT 정상
```

따라서 이번 HUD 수정에서는 시각 배치 회귀를 중심으로 재확인했고 정상 통과했다.

## 완료 판정

```text
Team-aware HUD Cleanup: USER VERIFIED
```

다음 작업:

```text
Gate 2B Opponent Team Architecture
- 실제 Team Battle용 상대 Active / Reserve 구조 설계
- 기존 두 Curse 동시 훈련 환경은 별도 Training 용도로 보존
- 최소 상대 팀 runtime을 먼저 검증
```
