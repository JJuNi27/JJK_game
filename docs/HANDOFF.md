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

사용자의 제안을 무조건 구현하지 않는다. 먼저 현재 단계와 비용을 평가하고 다음 중 하나로 설명한다.

```text
NOW             지금 구현해야 하는 핵심
FOUNDATION_ONLY 기반만 지금 만들고 콘텐츠는 보류
LATER           좋은 아이디어지만 후반이 효율적
REJECT          방향·원작·구조와 맞지 않아 채택하지 않음
```

세부 기준: `docs/DEVELOPMENT_ROADMAP.md`

현재 판단:

- 팬텀 퍼레이드·GameWith 조사 절차: `NOW`
- HUD 압축: `NOW`, 테스트 시야 개선에 필요
- 고죠 시기별 버전 ID: `FOUNDATION_ONLY`
- 최종 3D 모델·정교한 애니메이션: `LATER`
- 신주쿠 번아웃 조기 회복: `LATER`, 신주쿠 버전 전용
- 공격 속성·무하한 방어: `NOW`

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
- 파란 캡슐 대신 흰 머리·안대·검은 복장의 로우폴리 고죠 프록시
- 프록시는 사용자가 정상 표시를 확인했으며 최종 모델이 아님
- 기본 공격·피격·회피·창·혁·자·무량공처·승패 사운드
- 카메라 위치 흔들림과 화면 플래시
- 무량공처 출렁이는 임시 효과

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

## 8. 현재 원격 저장소의 새 작업 — 사용자 테스트 필요

### 8-1. 공통 피해 구조

새 파일:

```text
unity/Assets/Scripts/Core/DamageContext.cs
```

`DamageContext`가 다음을 함께 전달한다.

```text
피해량
공격자
전달 방식
방어 우회 속성
공격 이름
접촉 지점
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

`Health.ReceiveDamage()`는 회피 무적을 먼저 검사하고 이후 `IDamageGuard`를 거쳐 피해를 적용한다. 기존 `TakeDamage(float)`는 호환 경로로 유지한다.

기본 공격·창·혁과 훈련용 주령 공격은 새 `DamageContext`로 전환됐다. 허식 자와 창→혁 추가 피해는 아직 호환 경로를 사용하며 이후 방어 대상 캐릭터가 생기기 전에 마이그레이션한다.

### 8-2. 현대 고죠 무하한

새 파일:

```text
unity/Assets/Scripts/Player/GojoInfinityDefense.cs
```

현재 적용 버전:

```text
ModernTeacher
ShinjukuShowdown
```

현재 동작:

- 일반 물리 타격·일반 술식은 피해 차단
- 체력 감소 없음
- `INFINITY · BLOCKED` HUD 표시
- 고죠 앞에 청색 파문 표시
- 영역 필중·영역전연·술식 무효화·명시적 특수 우회는 통과
- 통과 시 `INFINITY BYPASSED`와 사유 표시
- 낙사는 직접 사망 처리라 무하한과 분리

회옥·옥절 버전의 자동화 수준은 해당 버전 구현 때 다시 확정한다.

### 8-3. 훈련용 주령 판정 분리

`CurseBotController`는 정식 원작 캐릭터가 아니라 전투 규칙 검증용 봇이다.

```text
주령 A / 원본 CurseBot
- NORMAL STRIKE
- 주황색 공격 예고
- 무하한에 막혀야 함

주령 B / CurseBot_B
- DOMAIN AMPLIFICATION 테스트 타격
- 보라색 공격 예고
- 무하한을 우회해 12 피해를 줘야 함
```

이것은 죠고·하나미의 정식 영역전연 구현이 아니라 공통 판정 검증용이다.

## 9. 이번 테스트 순서

```text
1. git pull 후 Console 빨간 오류 확인
2. HUD에 INFINITY · 무하한 자동 방어 표시
3. 주황색 예고의 일반 주령 A 공격을 일부러 맞기
   → 청색 파문과 BLOCKED
   → HP 100 유지
4. 보라색 예고의 주령 B 공격을 일부러 맞기
   → INFINITY BYPASSED · DOMAIN AMPLIFICATION
   → HP 12 감소
5. SPACE 회피 중 B 공격
   → 회피 무적으로 피해 없음
6. Q/E/R/V, 번아웃, 사운드와 승패 회귀 확인
7. 기본 공격·창·혁이 주령에게 기존 피해를 주는지 확인
```

## 10. 알려진 한계

- 현재 무하한은 전투 판정 프로토타입이며 거리 감속 시뮬레이션이 아님
- 주령 B는 정식 캐릭터의 영역전연 모션·제약을 구현하지 않음
- 허식 자와 창→혁 추가 피해는 새 피해 구조로 아직 마이그레이션 전
- 최종 캐릭터 모델과 실제 애니메이션 없음
- Primitive·LineRenderer 기반 임시 VFX
- 수동 카메라 조준 없음
- 주령은 단순 직선 추적
- 다른 고죠 버전 선택 UI 없음

## 11. 다음 우선순위

이번 작업이 정상이라면 즉시 거대한 범용 시스템을 더 만들지 않는다.

다음 판단 단계:

```text
1. 무하한·피해 구조의 회귀 오류 수정
2. 허식 자·연계 피해를 DamageContext로 마이그레이션
3. 스쿠나 프로토타입 조사표 작성
4. 스쿠나의 해·팔 기본 전투와 대용량 주력 프로필 구현
5. 고죠와 스쿠나에서 실제로 반복되는 부분만 공통 캐릭터 구조로 추출
```

최종 모델·정교한 애니메이션·완성형 사운드는 최소 두 캐릭터의 전투 상태 구조가 안정된 뒤 진행한다.

## 12. 새 채팅에서 Assistant가 먼저 할 일

1. 이 문서를 읽는다.
2. 사용자가 8번 작업을 pull·테스트했는지 확인한다.
3. 오류가 있으면 고치면서 계획된 다음 작업을 진행한다.
4. 사용자 제안은 `DEVELOPMENT_ROADMAP` 기준으로 NOW/FOUNDATION_ONLY/LATER/REJECT 판단한다.
5. 작업 후 이 문서를 반드시 갱신한다.
