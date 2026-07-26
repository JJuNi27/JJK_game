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
- HUD 압축: `NOW`
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

### 공통 피해 구조와 무하한 — 2026-07-27 사용자 확인 완료

- `DamageContext` 기반 피해 전달 정상
- 일반 타격과 술식 피해 속성 정상
- 현대 고죠 무하한 자동 방어 정상
- 주령 A `NORMAL STRIKE`가 무하한에 막힘
- 차단 시 `INFINITY · BLOCKED` 표시와 청색 파문 정상
- 주령 B `DOMAIN AMPLIFICATION`이 무하한을 우회함
- 우회 시 `INFINITY BYPASSED · DOMAIN AMPLIFICATION` 표시와 실제 피해 정상
- SPACE 회피 무적이 영역전연 테스트 공격보다 먼저 적용됨
- 기존 고죠 Q/E/R/V, 번아웃, 사운드와 승패의 회귀 문제 없음

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

이것은 죠고·하나미의 정식 영역전연 구현이 아니라 공통 판정 검증용이다.

## 9. 알려진 한계

- 현재 무하한은 판정 프로토타입이며 거리 감속 시뮬레이션이 아님
- 주령 B는 정식 캐릭터의 영역전연 모션·제약을 구현하지 않음
- 허식 자와 창→혁 추가 피해는 `DamageContext`로 아직 마이그레이션 전
- 최종 캐릭터 모델과 실제 애니메이션 없음
- Primitive·LineRenderer 기반 임시 VFX
- 수동 카메라 조준 없음
- 주령은 단순 직선 추적
- 다른 고죠 버전 선택 UI 없음

## 10. 다음 우선순위

다음 작업에서는 거대한 범용 시스템을 더 만들지 않는다.

```text
1. 허식 자와 창→혁 추가 피해를 DamageContext로 마이그레이션
2. 고죠 전투 회귀 확인
3. 스쿠나 프로토타입 조사표 작성
   - 원작 조건
   - 팬텀 퍼레이드·GameWith 스킬 구성
   - 글로벌 한국어 명칭
   - 주력 총량과 소비
   - 해·팔·화로·복마어주자 조건
4. 조사표 검토 뒤 스쿠나 기본 전투 구현 여부 판단
5. 고죠와 스쿠나에서 실제로 반복되는 부분만 공통 구조로 추출
```

최종 모델·정교한 애니메이션·완성형 사운드는 최소 두 캐릭터의 전투 상태 구조가 안정된 뒤 진행한다.

## 11. 새 채팅에서 Assistant가 먼저 할 일

1. 이 문서를 읽는다.
2. 무하한·영역전연 테스트는 이미 사용자가 정상 확인했다는 점을 유지한다.
3. 허식 자·창→혁 추가 피해 마이그레이션부터 진행한다.
4. 그다음 스쿠나 조사표를 작성하고 즉시 구현할지 다시 판단한다.
5. 사용자 제안은 `DEVELOPMENT_ROADMAP` 기준으로 평가한다.
6. 작업 후 이 문서를 반드시 갱신한다.
