# JJK Game 프로젝트 인수인계서

> 새 ChatGPT 대화에서는 `README.md`, 이 문서, `docs/DEVELOPMENT_ROADMAP.md`, `docs/CANON_SKILL_RULES.md`, `docs/ADAPTATION_REFERENCES.md`, `docs/PHANTOM_PARADE_REFERENCE.md`, `docs/AUDIO_SETUP.md`, `unity/README.md`와 최신 코드를 먼저 읽는다.

이 문서는 **사용자 확인 완료**, **원격 테스트 전**, **다음 우선순위**를 분리하는 단일 인수인계 기준이다.

## 1. 환경과 작업 방식

- 프로젝트: **주술사 되어보기**
- 사용자: 박민준
- 저장소: `JJuNi27/JJK_game`
- 브랜치: `master`
- 로컬: `D:\GitHub\JJK_game`
- Unity 프로젝트: `D:\GitHub\JJK_game\unity`
- Unity 6.3 LTS `6000.3.20f1`, URP

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

작업 루프:

```text
Assistant가 master 수정
→ 사용자가 pull
→ 사용자 Unity에서 실행
→ 화면·오류·조작감 전달
→ 문제 수정과 다음 계획 작업을 함께 진행
```

Assistant는 Unity를 직접 실행·컴파일했다고 주장하지 않는다.

## 2. 기술 리드 원칙

사용자의 제안을 무조건 구현하지 않는다. 현재 단계와 비용을 평가하고 다음 중 하나로 설명한다.

```text
NOW             지금 구현해야 하는 핵심
FOUNDATION_ONLY 기반만 지금 만들고 콘텐츠는 보류
LATER           좋은 아이디어지만 후반이 효율적
REJECT          방향·원작·구조와 맞지 않아 채택하지 않음
```

세부 기준: `docs/DEVELOPMENT_ROADMAP.md`

현재 판단:

- 팬텀 퍼레이드·GameWith 조사 절차: `NOW`
- HUD 압축: `NOW`, 완료
- 공격 속성·무하한 방어: `NOW`, 사용자 확인 완료
- 고죠 시기별 버전 ID: `FOUNDATION_ONLY`
- 최종 3D 모델·정교한 애니메이션: `LATER`
- 신주쿠 번아웃 조기 회복: `LATER`, 신주쿠 버전 전용

## 3. 원작·공식 게임 참고 원칙

1. 만화·공식 팬북·공식 애니메이션·극장판 설정 우선
2. `OFFICIAL_CONFIRMED`, `ADAPTATION_REFERENCE`, `GAME_ORIGINAL`, `INTERPRETATION_PENDING`, `LOCALIZATION_PENDING` 구분
3. 팬텀 퍼레이드는 모션·VFX·카메라·사운드·주력·상태 변화의 적극적인 참고자료
4. GameWith는 일본 서버 상세 수치와 일본 원문 조사처
5. 화면 명칭은 글로벌 한국어 클라이언트 우선
6. 공식 게임 원본 모델·애니메이션·텍스처·음원은 복사하지 않음
7. 같은 인물도 시기·육체·습득 기술이 다르면 별도 버전

조사 문서:

```text
docs/PHANTOM_PARADE_REFERENCE.md
docs/ADAPTATION_REFERENCES.md
docs/CANON_SKILL_RULES.md
```

## 4. Python/Pygame 확인 완료

```text
고죠 · 무량공처
스쿠나(이타도리 몸) · 복마어주자
후시구로 메구미 · 감합암예정
옷코츠 유타(본편 2학년) · 진안상애
```

일본어 Vosk 음성인식과 장인 입력을 사용자가 확인했다. 현재 개발 우선순위는 Unity 전투다.

## 5. Unity 조작

```text
WASD 이동
SPACE 회피
TAB 타깃 전환
LMB 3단 기본 공격
Q 술식순전 「창」
E 술식반전 「혁」
R 허식 「자」
V 영역 입력 시작
X 영역 입력 취소
ENTER 승패 뒤 재시작
F1 조작 도움말 열기/닫기
```

## 6. 사용자 확인 완료 상태

### 전투·맵·적

- WASD 이동, 기본 공격 1→2→3타
- 공격 동작 연계와 실제 적중 콤보 분리
- 주령 추적, 공격 예고, 회피 무적
- 약 65×65 맵, 안전벽, 낙사 처리
- 주령 두 마리, 개별 HP, 전체 처치 승리
- TAB 락온 전환과 락온 우선 조준

### 현대 고죠 술식

- 지속 수렴장 형태의 창
- 이동·관통 발사체 형태의 혁
- 창→혁 적중 보너스
- Q/E 사용 자체로 허식 자 준비
- 허식 자 락온 우선 조준
- 무량공처 장인 입력과 영역 행동 정지
- 영역 종료 후 약 5초 술식 번아웃

### 주력·행동 상태

- `SIX EYES · 육안 효율`
- Q/E/R/V 실제 소비 각각 1
- 주력 자동 회복
- 회피·술식 시전·영역 입력 행동 충돌 차단
- 번아웃 중 Q/E/R/V 차단
- 번아웃 중 이동·기본 공격·회피 허용
- 시간 경과 후 술식 재사용

### HUD·외형·사운드

- 압축 HUD와 F1 조작 도움말
- 흰 머리·안대·검은 복장의 로우폴리 고죠 프록시
- 프록시는 최종 모델이 아님
- 기본 공격·피격·회피·창·혁·자·무량공처·승패 사운드
- 카메라 위치 흔들림과 화면 플래시
- 무량공처 출렁이는 임시 효과

### 공통 피해 구조와 무하한 — 사용자 확인 완료

- `DamageContext` 기반 피해 전달 정상
- 일반 타격과 술식 피해 속성 정상
- 현대 고죠 무하한 자동 방어 정상
- 주령 A `NORMAL STRIKE`가 무하한에 막힘
- 차단 시 `INFINITY · BLOCKED`와 청색 파문 정상
- 주령 B `DOMAIN AMPLIFICATION`이 무하한 우회
- 우회 시 `INFINITY BYPASSED · DOMAIN AMPLIFICATION`과 실제 피해 정상
- SPACE 회피 무적이 영역전연 테스트 공격보다 먼저 적용됨
- 기존 Q/E/R/V, 번아웃, 사운드와 승패 회귀 없음

## 7. 고죠 버전 기반

```text
HiddenInventoryPreAwakening  회옥·옥절 각성 전
HiddenInventoryAwakened     회옥·옥절 각성 후
ModernTeacher               현대 일반 고죠, 현재 선택
ShinjukuShowdown            신주쿠 결전 고죠
```

- 현대 일반 고죠: 안대
- 회옥·옥절: 선글라스 외형 기반
- 신주쿠 결전: 눈 노출 외형 기반
- `TryRestoreTechniqueEarly()`는 신주쿠 결전만 성공
- 현재 조기 회복 입력은 연결하지 않음

## 8. 공통 피해와 무하한 구조

핵심 파일:

```text
unity/Assets/Scripts/Core/DamageContext.cs
unity/Assets/Scripts/Core/Health.cs
unity/Assets/Scripts/Player/GojoInfinityDefense.cs
unity/Assets/Scripts/Enemy/CurseBotController.cs
```

현재 전달 방식:

```text
Unspecified
PhysicalStrike
CursedTechnique
DomainSureHit
Environmental
```

현재 우회 속성:

```text
DomainAmplification
TechniqueNullification
IgnoresInfinity
Unblockable
```

처리 순서:

```text
사망·유효 피해 확인
→ 회피 무적
→ IDamageGuard
→ 실제 HP 감소
```

현재 무하한 적용 버전:

```text
ModernTeacher
ShinjukuShowdown
```

훈련용 주령 역할:

```text
주령 A
- NORMAL STRIKE
- 주황색 공격 예고
- 무하한에 막힘

주령 B
- DOMAIN AMPLIFICATION 테스트 타격
- 보라색 공격 예고
- 무하한 우회
```

이것은 정식 죠고·하나미의 영역전연 구현이 아니라 공통 판정 검증용이다.

## 9. 현재 원격 저장소의 새 작업 — 사용자 테스트 필요

### 허식 자·창→혁 추가 피해 마이그레이션

수정 파일:

```text
unity/Assets/Scripts/Player/GojoTechniqueChainController.cs
```

변경 내용:

- 창→혁 보너스 12 피해가 `TakeDamage(float)` 대신 `DamageContext` 사용
- 전달 방식은 `CursedTechnique`
- 공격 이름은 `BLUE → RED CHAIN`
- 피해가 실제 적용될 때만 강화 넉백·경직과 보너스 안내 표시
- 허식 「자」 55 피해가 `DamageContext` 사용
- 전달 방식은 `CursedTechnique`
- 공격 이름은 `HOLLOW PURPLE · 허식 「자」`
- 방어되거나 무적이면 넉백·경직도 적용하지 않음

이번 변경은 현재 주령에게 방어기가 없어서 체감상 기존과 같아야 한다. 목적은 향후 방어·무효화 캐릭터에게 일관된 판정을 제공하는 것이다.

### 테스트 순서

```text
1. git pull 후 Console 빨간 오류 확인
2. Q를 주령에게 적중시켜 BLUE MARK 생성
3. 제한시간 안에 E 적중
   → BLUE → RED · BONUS +12 표시
   → 기존 추가 피해·강화 넉백 유지
4. 허공에 Q/E 사용 후 R
   → 허식 자가 기존처럼 발동
   → 주령에게 55 피해와 넉백 유지
5. R 락온 우선 조준 유지
6. 무하한 A 차단·B 영역전연 우회 유지
7. V 무량공처·번아웃·사운드·승패 유지
```

## 10. 알려진 한계

- 현재 무하한은 판정 프로토타입이며 거리 감속 시뮬레이션이 아님
- 주령 B는 정식 캐릭터의 영역전연 모션·제약을 구현하지 않음
- 최종 캐릭터 모델과 실제 애니메이션 없음
- Primitive·LineRenderer 기반 임시 VFX
- 수동 카메라 조준 없음
- 주령은 단순 직선 추적
- 다른 고죠 버전 선택 UI 없음

## 11. 다음 우선순위

이번 마이그레이션이 정상이라면:

```text
1. 고죠 DamageContext 마이그레이션 완료 상태로 기록
2. 스쿠나 첫 Unity 버전 조사표 작성
   - 어느 시기·육체를 첫 버전으로 할지
   - 원작 기술 조건
   - 팬텀 퍼레이드·GameWith 스킬 구성
   - 글로벌 한국어 명칭
   - 주력 총량과 소비
   - 해·팔·화로·복마어주자 조건
3. 조사표를 보고 구현 범위 평가
4. 필요한 최소 스쿠나 기본 전투만 구현
5. 고죠와 스쿠나에서 실제로 반복된 부분만 공통 구조로 추출
```

최종 모델·정교한 애니메이션·완성형 사운드는 최소 두 캐릭터의 전투 상태 구조가 안정된 뒤 진행한다.

## 12. 새 채팅에서 Assistant가 먼저 할 일

1. 이 문서를 읽는다.
2. 무하한·영역전연 테스트는 이미 정상 확인됐음을 유지한다.
3. 사용자가 9번 마이그레이션을 pull·테스트했는지 확인한다.
4. 정상이라면 스쿠나 조사표를 작성하고 즉시 구현할지 판단한다.
5. 사용자 제안은 `DEVELOPMENT_ROADMAP` 기준으로 평가한다.
6. 작업 뒤 이 문서를 갱신한다.
