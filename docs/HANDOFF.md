# JJK Game 프로젝트 인수인계서

> 새 ChatGPT 대화에서는 `README.md`, 이 문서, `docs/CANON_SKILL_RULES.md`, `docs/ADAPTATION_REFERENCES.md`, `docs/AUDIO_SETUP.md`, `unity/README.md`와 최신 코드를 먼저 읽는다.

이 문서는 **사용자 확인 완료**, **원격 테스트 전**, **다음 작업**을 분리하는 단일 인수인계 기준이다.

## 1. 환경과 작업 방식

- 프로젝트: **주술사 되어보기**
- 사용자: 박민준
- 저장소: `JJuNi27/JJK_game`
- 브랜치: `master`
- 로컬: `D:\GitHub\JJK_game`
- Unity: `D:\GitHub\JJK_game\unity`
- Unity 6.3 LTS `6000.3.20f1`, URP

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

Assistant가 원격 `master` 수정 → 사용자가 pull → Unity 실행 확인 → 문제 수정과 다음 작업을 함께 진행한다. Assistant는 Unity를 직접 실행했다고 주장하지 않는다.

## 2. 제작 원칙

1. 만화·공식 팬북·공식 애니메이션·극장판 설정을 우선한다.
2. `OFFICIAL_CONFIRMED`, `ADAPTATION_REFERENCE`, `GAME_ORIGINAL`, `INTERPRETATION_PENDING`을 구분한다.
3. 팬텀 퍼레이드는 원작을 게임으로 번역한 모션·VFX·사운드·자원 참고자료로 사용한다.
4. 같은 인물도 시기·육체·습득 기술이 다르면 별도 캐릭터 버전으로 분리한다.
5. 공식 게임 원본 자산을 복사하지 않고 프로젝트용으로 새로 제작한다.
6. 연결된 2~3개 작업은 묶되 테스트 항목은 분리한다.
7. 작업 묶음 뒤 이 문서를 반드시 갱신한다.

## 3. Python/Pygame 완료 상태

사용자가 다음 네 캐릭터의 영역전개 입력과 일본어 Vosk 음성인식을 확인했다.

```text
고죠 · 무량공처
스쿠나(이타도리 몸) · 복마어주자
후시구로 메구미 · 감합암예정
옷코츠 유타(본편 2학년) · 진안상애
```

현재 우선순위는 Unity 전투다.

## 4. Unity 조작

```text
WASD 이동
SPACE 회피
TAB 타깃 전환
LMB 3단 기본 공격
Q 창
E 혁
R 허식 자
V 영역 입력 시작
X 영역 입력 취소
ENTER 승패 뒤 재시작
F1 조작 도움말 열기/닫기
```

## 5. 사용자가 정상 확인한 상태

### 기본 전투와 맵

- 이동, 3단 공격, 적중 콤보
- 주령 추적·공격 예고·회피 무적
- HP, 승패, 재시작
- 약 65×65 맵과 안전벽, 낙사 처리
- 주령 두 마리, 개별 HP, 전체 처치 승리
- TAB 락온 전환

### 고죠 술식

- 지속 수렴장 형태의 창
- 이동·관통 발사체 형태의 혁
- 창→혁 적중 보너스
- Q/E 사용 자체로 허식 자 준비
- 허식 자 락온 우선 조준
- 무량공처 장인 입력과 영역 행동 정지
- 영역 종료 후 약 5초 술식 번아웃

### 주력과 행동 상태

사용자가 최신 묶음까지 모두 정상이라고 확인했다.

- 고죠 HUD `SIX EYES · 육안 효율`
- Q/E/R/V 실제 소비 각각 1
- 주력 자동 회복
- 회피·술식 시전·영역 입력 중 행동 충돌 차단
- 번아웃 중 Q/E/R/V 차단
- 번아웃 중 이동·기본 공격·회피 허용
- 번아웃 종료 후 술식 재사용

### 사운드·화면 피드백

- 기본 공격, 피격, 회피, 창·혁·자·무량공처, 승패 사운드
- 카메라 위치 흔들림과 화면 플래시
- 무량공처 출렁이는 임시 효과
- 3타 전용 저음 충격음 보강

## 6. 현재 원격 저장소의 새 작업 — 사용자 테스트 필요

### 6-1. 고죠 시기별 버전 기반

새 파일:

```text
unity/Assets/Scripts/Player/GojoVariantController.cs
```

버전 ID:

```text
HiddenInventoryPreAwakening  회옥·옥절 각성 전
HiddenInventoryAwakened     회옥·옥절 각성 후
ModernTeacher               현대 일반 고죠
ShinjukuShowdown            신주쿠 결전 고죠
```

현재 선택은 `ModernTeacher`다.

- 현대 일반 고죠는 안대 외형
- 회옥·옥절은 둥근 선글라스 외형 기반
- 신주쿠 결전은 눈 노출 외형 기반
- 번아웃 조기 회복 함수 `TryRestoreTechniqueEarly()`는 `ShinjukuShowdown`만 성공
- 현재 조기 회복 입력은 연결하지 않음

### 6-2. 압축 HUD

- 상단 체력 패널과 적 체력 패널 축소
- 주력 바를 플레이어 패널 안으로 통합
- Q/E/R과 BLUE MARK 패널 폭·높이 축소
- 영역 패널을 한 줄 상태창으로 축소
- 기존 상시 조작 설명 제거
- `F1`을 누를 때만 조작 도움말 표시
- 공격 연계와 회피 경고도 더 작은 크기로 변경

핵심 파일:

```text
unity/Assets/Scripts/Core/MatchController.cs
unity/Assets/Scripts/Core/CursedEnergyController.cs
unity/Assets/Scripts/Player/GojoTechniqueController.cs
unity/Assets/Scripts/Player/GojoTechniqueChainController.cs
unity/Assets/Scripts/Core/TechniqueBurnoutController.cs
```

### 6-3. 로우폴리 고죠 프록시

새 파일:

```text
unity/Assets/Scripts/Player/GojoPrototypeAvatar.cs
```

- 기존 파란 캡슐 Renderer를 숨김
- 충돌·이동용 CharacterController는 유지
- 흰 머리, 안대, 검은 고전복, 머리·몸·팔·다리 형태를 런타임 Primitive로 생성
- 이동 중 팔과 다리에 간단한 보행 흔들림 적용
- 버전 프로필에 따라 선글라스·안대·눈 외형 전환 기반
- 외부 에셋이나 장면 재생성 불필요

이것은 최종 모델이 아니라 전투 가독성과 외형 확인용 자체 제작 프록시다.

## 7. 이번 테스트 순서

```text
1. Unity Console에 빨간 컴파일 오류가 없는지
2. 파란 캡슐 대신 흰 머리·안대·검은 복장의 사람형 고죠가 보이는지
3. 이동하면 팔·다리가 가볍게 움직이는지
4. 플레이어 제목에 현대 · 교사 버전이 표시되는지
5. 체력·주력·적 체력 패널이 이전보다 작아졌는지
6. Q/E/R 패널이 왼쪽에 작게 정렬되는지
7. 하단 영역 상태창이 이전보다 얇아졌는지
8. F1을 누를 때만 조작 도움말이 열리고 다시 누르면 닫히는지
9. 기존 Q/E/R/V, 번아웃, 사운드, 승패가 정상인지
```

## 8. 실제 모델 제작 경로

권장:

```text
VRoid Studio 또는 Blender로 프로젝트용 모델 제작
→ VRM/FBX 내보내기
→ Unity 가져오기
→ Mixamo 또는 Humanoid 애니메이션 리타기팅
→ 팬텀 퍼레이드·애니를 참고해 프로젝트용 모션과 VFX 제작
```

Unity Asset Store·Sketchfab 모델은 개별 라이선스와 저작자 표시를 확인한다. 다운로드 가능한 고죠 팬 모델은 캐릭터 권리까지 허가된 것이 아닐 수 있어 기준 모델로 자동 채택하지 않는다.

## 9. 알려진 한계

- 로우폴리 프록시는 최종 캐릭터 모델이 아님
- 실제 공격 애니메이션 없음
- Primitive/LineRenderer 기반 임시 VFX
- 수동 카메라 조준 없음
- 주령은 단순 직선 추적
- 현재 다른 고죠 버전 선택 UI 없음
- 신주쿠 조기 회복 입력 없음
- 무하한 상시 방어와 예외 공격 규칙 없음

## 10. 다음 우선순위

새 작업이 정상이라면:

```text
캐릭터 외형·애니메이션 기반
- Humanoid 모델 교체 슬롯
- Idle/Run/Attack 애니메이션 상태 머신
- 기술별 시전 애니메이션 이벤트

고죠 정체성
- 현대 일반 고죠의 무하한 상시 방어
- 무하한을 무시하는 공격 타입 기반

그 다음
- 캐릭터 버전 선택 UI
- 회옥·옥절 버전 상세 스킬 확정
- 신주쿠 결전 버전 전용 고급 운용
```

## 11. 새 채팅에서 Assistant가 먼저 할 일

1. 이 문서를 읽는다.
2. 사용자가 6번 작업을 pull·테스트했는지 확인한다.
3. 오류가 있으면 고치면서 다음 작업도 진행한다.
4. 원작은 `CANON_SKILL_RULES`, 공식 게임 참고는 `ADAPTATION_REFERENCES`를 확인한다.
5. 작업 후 이 문서를 갱신한다.
