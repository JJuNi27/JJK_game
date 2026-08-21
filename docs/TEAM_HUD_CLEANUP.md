# Team-aware HUD Cleanup

작성 기준일: 2026-08-21

이 문서는 Gate 2A 사용자 검증 완료 뒤 진행한 HUD 정리 상태와 사용자 테스트 기준을 기록한다.

## 상태

```text
Gate 2A Team Runtime: USER VERIFIED
Team-aware HUD Cleanup: REMOTE IMPLEMENTED / USER TEST PENDING
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

2026-08-21 첫 HUD 정리 사용자 캡처:

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

위 겹침 문제는 `Move team HUD clear of technique cards` 변경으로 원격 수정했다. 실제 Unity 최종 확인은 아직 필요하다.

## 사용자 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류가 없는지 확인
3. 게임 시작

[고죠 Active]
4. 좌측 상단에 ACTIVE 고죠 HP/CE 패널이 한 번만 보이는지
5. DODGE와 Q/E/R 기술 카드가 정상인지
6. TEAM 패널이 기술 카드 아래쪽 빈 공간에 독립적으로 보이고 어떤 패널과도 겹치지 않는지
7. TEAM ACTIVE 행 HP/CE가 피해/주력 소비 시 실시간으로 변하는지
8. 하단 중앙에 고죠 무량공처 상태 패널이 정상인지
9. F1 도움말에 고죠 Q/E/R/V와 T 팀 교대가 표시되는지

[스쿠나 Active]
10. T로 스쿠나 교대
11. 좌측 상단 PLAYER/ACTIVE 패널이 중복되지 않고 하나만 보이는지
12. TEAM 패널이 스쿠나 Q/E/R 기술 카드와 겹치지 않는지
13. 고죠 영역 패널이 사라지고 스쿠나 복마어주자 패널만 보이는지
14. F1 도움말이 Q 해 / E 팔 / R 푸가 / V 복마어주자 기준인지
15. TEAM ACTIVE 행 HP/CE가 실시간으로 변하는지
16. RESERVE 고죠 HP/CE가 저장값 그대로인지

[회귀]
17. T 교대 정상
18. TAB Target Lock 정상
19. 회피/기본공격/술식 정상
20. 첫 KO Auto Tag와 최종 DEFEAT 정상
```

## 완료 기준

위 항목을 사용자가 실제 Unity에서 확인하면:

```text
Team-aware HUD Cleanup: USER VERIFIED
```

로 변경한다.

그 다음 Gate 2B에서 상대 팀 Active/Reserve 구조와 훈련용 다중 Curse 모드 분리를 설계한다.
