# JJK Game 프로젝트 인수인계서

> 새 ChatGPT 대화에서는 `README.md`, 이 문서, `docs/DEVELOPMENT_ROADMAP.md`, `docs/CANON_SKILL_RULES.md`, `docs/ADAPTATION_REFERENCES.md`, `docs/PHANTOM_PARADE_REFERENCE.md`, `docs/SUKUNA_FIRST_VERSION_RESEARCH.md`, `docs/AUDIO_SETUP.md`, `unity/README.md`와 최신 코드를 먼저 읽는다.

이 문서는 **사용자 확인 완료**, **현재 판단**, **다음 구현**을 분리하는 단일 인수인계 기준이다.

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
- 공격 속성·무하한 방어: `NOW`, 사용자 확인 완료
- 고죠 시기별 버전 ID: `FOUNDATION_ONLY`
- 첫 스쿠나 버전 조사: `NOW`, 완료
- 스쿠나 Milestone 1 해·팔: `NOW`, 다음 코드 묶음
- 화로·복마어주자: 앞 단계 확인 뒤 `NOW 후보`
- 최종 3D 모델·정교한 애니메이션: `LATER`
- 신주쿠 고죠 번아웃 조기 회복: `LATER`

## 3. 원작·공식 게임 참고 원칙

1. 만화·공식 팬북·공식 애니메이션·극장판 설정 우선
2. `OFFICIAL_CONFIRMED`, `ADAPTATION_REFERENCE`, `GAME_ORIGINAL`, `INTERPRETATION_PENDING`, `LOCALIZATION_PENDING` 구분
3. 팬텀 퍼레이드는 모션·VFX·카메라·사운드·주력·상태 변화의 적극적인 참고자료
4. GameWith는 일본 서버 상세 수치와 일본 원문 조사처
5. 화면 명칭은 글로벌 한국어 클라이언트 우선
6. 공식 게임 원본 모델·애니메이션·텍스처·음원은 복사하지 않음
7. 같은 인물도 시기·육체·습득 기술이 다르면 별도 버전

핵심 문서:

```text
docs/PHANTOM_PARADE_REFERENCE.md
docs/SUKUNA_FIRST_VERSION_RESEARCH.md
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

## 5. 현재 Unity 고죠 조작

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

### 공통 피해 구조와 무하한

- `DamageContext` 기반 피해 전달 정상
- 일반 타격과 술식 피해 속성 정상
- 현대 고죠 무하한 자동 방어 정상
- 주령 A `NORMAL STRIKE`가 무하한에 막힘
- 차단 시 `INFINITY · BLOCKED`와 청색 파문 정상
- 주령 B `DOMAIN AMPLIFICATION`이 무하한 우회
- 우회 시 `INFINITY BYPASSED · DOMAIN AMPLIFICATION`과 실제 피해 정상
- SPACE 회피 무적이 영역전연 테스트 공격보다 먼저 적용됨
- 기존 Q/E/R/V, 번아웃, 사운드와 승패 회귀 없음

### 고죠 피해 마이그레이션

사용자가 2026-07-28 모두 정상이라고 확인했다.

- 창→혁 추가 12 피해가 `DamageContext.CursedTechnique` 사용
- 실제 피해가 들어갈 때만 추가 넉백·경직과 보너스 안내
- 허식 자 55 피해가 `DamageContext.CursedTechnique` 사용
- 무적·방어 시 넉백만 들어가는 문제 방지
- 락온, 무량공처, 번아웃, 무하한과 기존 사운드 회귀 없음

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

훈련용 주령:

```text
주령 A · NORMAL STRIKE · 무하한에 막힘
주령 B · DOMAIN AMPLIFICATION · 무하한 우회
```

주령 B는 정식 죠고·하나미 구현이 아니라 공통 판정 테스트용이다.

## 9. 첫 스쿠나 조사 완료

상세 문서:

```text
docs/SUKUNA_FIRST_VERSION_RESEARCH.md
```

선택:

```text
내부 ID: SukunaShibuyaYujiBody
시기·육체: 시부야 사변 · 이타도리 유지 육체 스쿠나
판정: NOW
```

비교한 팬텀 퍼레이드 버전:

```text
宿儺「本物の呪術」
- 복마어주자, 영역 후 스킬 변화, 가변 주력 참격 참고
- 첫 구현에는 복잡도가 높아 영역 단계 참고로 보류

両面宿儺「火力勝負」
- 해·팔·개, 최대 주력 160, 기술 소비와 준비 조건 참고
- 첫 전투 세트의 주 참고 버전
```

글로벌 한국어 카드 부제·세부 스킬 표기는 실제 클라이언트 캡처 확인 전까지 `LOCALIZATION_PENDING`이다.

### 주력 판단

기존 `SukunaVastReserve = 300`은 조사 전 임시값이었다.

첫 테스트 권장:

```text
최대 주력 160
시작 주력 95
Q 해 15
E 팔 45
```

고죠는 작은 소비의 효율형, 스쿠나는 큰 통과 큰 소비의 대용량형으로 차이를 만든다. 300부터 시작하면 첫 테스트에서 자원 선택 압박이 약해질 가능성이 커 160으로 시작한다.

## 10. 다음 코드 묶음 — 스쿠나 Milestone 1

범위:

```text
1. 고죠/스쿠나를 테스트할 최소 플레이어 전환 경로
2. SukunaShibuyaYujiBody 프로필
3. 최대 주력 160 / 시작 95
4. LMB 3단 체술
5. Q 해
   - 원거리 참격
   - 적 2명 이상이면 넓은 다중 대상형
   - 적 1명이면 집중 단일 대상형
   - 소비 15
6. E 팔
   - 짧은 근거리
   - 높은 단일 피해와 다중 히트 표현
   - 소비 45
7. 해·팔 사용 상태 기록
```

이번 묶음에 넣지 않음:

```text
R 화로
V 복마어주자
반전술식 자동 회복
다중 상태 저항
영역 후 스킬 변화
메구미 육체·헤이안 진신·세계참
완성형 모델·애니메이션·VFX·보이스
```

해·팔 확인 뒤 R 화로, 화로 확인 뒤 V 복마어주자를 순차 구현한다.

## 11. 알려진 한계

- 현재 무하한은 판정 프로토타입이며 거리 감속 시뮬레이션이 아님
- 주령 B는 정식 영역전연 캐릭터 구현이 아님
- 최종 캐릭터 모델과 실제 애니메이션 없음
- Primitive·LineRenderer 기반 임시 VFX
- 수동 카메라 조준 없음
- 주령은 단순 직선 추적
- 다른 고죠 버전 선택 UI 없음
- 스쿠나 글로벌 한국어 세부 명칭 일부 확인 대기

## 12. 새 채팅에서 Assistant가 먼저 할 일

1. 이 문서를 읽는다.
2. 고죠 DamageContext 마이그레이션까지 사용자 확인 완료임을 유지한다.
3. 첫 스쿠나 조사 결과와 160/95 판단을 유지한다.
4. 스쿠나 Milestone 1을 범위대로 구현한다.
5. 거대한 범용 캐릭터 프레임워크는 만들지 않는다.
6. 사용자 제안은 `DEVELOPMENT_ROADMAP` 기준으로 평가한다.
7. 작업 뒤 이 문서를 반드시 갱신한다.
