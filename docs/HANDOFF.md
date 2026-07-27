# JJK Game 프로젝트 인수인계서

> 새 ChatGPT 대화에서는 `README.md`, 이 문서, `docs/DEVELOPMENT_ROADMAP.md`, `docs/CANON_SKILL_RULES.md`, `docs/ADAPTATION_REFERENCES.md`, `docs/PHANTOM_PARADE_REFERENCE.md`, `docs/SUKUNA_FIRST_VERSION_RESEARCH.md`, `docs/AUDIO_SETUP.md`, `unity/README.md`와 최신 코드를 먼저 읽는다.

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

Assistant가 `master`를 수정하고, 사용자가 pull 후 Unity에서 실행해 확인한다. Assistant는 Unity를 직접 컴파일했다고 주장하지 않는다.

## 2. 기술 리드 원칙

사용자의 제안을 무조건 구현하지 않는다.

```text
NOW             현재 구조에 필요해 바로 구현
FOUNDATION_ONLY 기반만 준비하고 콘텐츠는 보류
LATER           좋은 아이디어지만 후반이 효율적
REJECT          프로젝트 방향·원작·구조와 맞지 않음
```

세부 기준: `docs/DEVELOPMENT_ROADMAP.md`

현재 판단:

- 팬텀 퍼레이드·GameWith 조사: `NOW`
- 글로벌 한국어 명칭 사용자 검수: `NOW`
- 고죠 공통 피해·무하한: 완료
- 첫 스쿠나 Milestone 1: 원격 구현 완료, 테스트 대기
- 푸가: 해·팔 검증 뒤 `NOW 후보`
- 복마어주자: 푸가 검증 뒤 `NOW 후보`
- 완성형 모델·애니메이션·보이스: `LATER`

## 3. 원작·공식 게임 참고 원칙

1. 만화·공식 팬북·공식 애니메이션·극장판 설정 우선
2. 팬텀 퍼레이드는 자원·스킬 구성·모션·VFX·카메라·사운드의 적극적인 참고자료
3. GameWith는 일본 서버 상세 수치와 일본 원문 조사처
4. UI 명칭은 글로벌 한국어 클라이언트 우선
5. 사용자가 실제 글로벌 클라이언트 기준으로 검수한 명칭은 `USER_VERIFIED_GLOBAL_KO`
6. 원본 게임 자산은 복사하지 않고 프로젝트용으로 새로 제작
7. 같은 인물도 시기·육체·습득 기술이 다르면 별도 버전

사용자 검수 완료:

```text
스쿠나 R 표시명: 푸가
내부 ID: Fuga
```

`화로`는 원리 설명에만 사용하며 UI에는 `푸가`를 사용한다.

## 4. Python/Pygame 확인 완료

```text
고죠 · 무량공처
스쿠나(이타도리 몸) · 복마어주자
후시구로 메구미 · 감합암예정
옷코츠 유타(본편 2학년) · 진안상애
```

일본어 Vosk 음성인식과 장인 입력을 사용자가 확인했다. 현재 우선순위는 Unity 전투다.

## 5. Unity 공통 조작

```text
WASD 이동
SPACE 회피
TAB 타깃 전환
LMB 3단 기본 공격
F1 조작 도움말
ENTER 승패 뒤 재시작

1 현대 고죠 선택 후 장면 재시작
2 시부야 스쿠나 선택 후 장면 재시작
```

고죠:

```text
Q 창
E 혁
R 허식 자
V 무량공처 입력
X 영역 입력 취소
```

스쿠나 Milestone 1:

```text
Q 해
E 팔
R 푸가 · 현재 잠김
V 복마어주자 · 현재 잠김
```

## 6. 사용자 확인 완료 상태

### 공통 전투

- 이동, 3단 기본 공격과 적중 콤보
- 주령 추적·공격 예고·회피 무적
- TAB 락온과 락온 우선 조준
- 주령 두 마리, 개별 HP, 전체 처치 승리
- 약 65×65 맵, 안전벽과 낙사
- 압축 HUD, 승패와 재시작
- 기본 공격·피격·회피·기술·영역·승패 사운드

### 현대 고죠

- 지속 수렴장 형태의 창
- 이동·관통 발사체 형태의 혁
- 창→혁 적중 보너스
- Q/E 사용 자체로 허식 자 준비
- 허식 자 락온 조준
- 무량공처 장인 입력
- 영역 종료 후 약 5초 술식 번아웃
- 육안 효율 프로필, Q/E/R/V 실제 소비 각각 1
- 현대 고죠 무하한 자동 방어
- 일반 공격 차단, 영역전연 우회, 회피 무적 우선순위
- 허식 자와 창→혁 추가 피해의 `DamageContext` 마이그레이션

### 공통 피해 구조

```text
DamageDeliveryType
- Unspecified
- PhysicalStrike
- CursedTechnique
- DomainSureHit
- Environmental

DamageTraits
- DomainAmplification
- TechniqueNullification
- IgnoresInfinity
- Unblockable
```

처리 순서:

```text
사망·유효 피해
→ 회피 무적
→ 활성화된 IDamageGuard
→ 실제 HP 감소
```

비활성화된 방어 컴포넌트는 캐릭터 전환 뒤 피해 판정에 참여하지 않는다.

## 7. 고죠 버전 기반

```text
HiddenInventoryPreAwakening  회옥·옥절 각성 전
HiddenInventoryAwakened     회옥·옥절 각성 후
ModernTeacher               현대 일반 고죠
ShinjukuShowdown            신주쿠 결전 고죠
```

현재 고죠 선택은 `ModernTeacher`다. 번아웃 조기 회복은 신주쿠 결전 버전에만 예약되어 있고 입력은 연결하지 않았다.

## 8. 첫 스쿠나 선택

상세 문서: `docs/SUKUNA_FIRST_VERSION_RESEARCH.md`

```text
내부 ID: SukunaShibuyaYujiBody
시기·육체: 시부야 사변 · 이타도리 유지 육체 스쿠나
주 참고: 팬텀 퍼레이드 両面宿儺「火力勝負」
영역 참고: 宿儺「本物の呪術」
```

헤이안 진신, 메구미 육체, 세계참과 신주쿠 고급 운용은 보류한다.

## 9. 현재 원격 저장소의 새 작업 — 사용자 테스트 필요

### 9-1. 최소 캐릭터 전환

새 파일:

```text
unity/Assets/Scripts/Player/PrototypeCharacterController.cs
```

동작:

```text
1 키 → 현대 고죠 선택 → 현재 장면 재시작
2 키 → 시부야 스쿠나 선택 → 현재 장면 재시작
```

거대한 캐릭터 선택 시스템을 만들지 않고 정적 선택값과 장면 재시작만 사용한다.

스쿠나 선택 시:

- 고죠 창·혁·자·영역·무하한·번아웃 컴포넌트 비활성화
- 고죠 로우폴리 외형 숨김
- 스쿠나 기술·외형 활성화
- 스쿠나 전용 HUD가 기존 고죠 제목과 영역 패널을 덮어 표시

### 9-2. 시부야 스쿠나 주력

`CursedEnergyProfileId.SukunaShibuyaReserve`

```text
최대 주력 160
시작 주력 95
초당 회복 4
사용 뒤 회복 지연 1.2초
Q 해 소비 15
E 팔 소비 45
```

기존 `SukunaVastReserve = 300`은 미래 버전 후보로 남겨두며 이번 테스트에 사용하지 않는다.

### 9-3. Q 해

새 파일:

```text
unity/Assets/Scripts/Player/SukunaTechniqueController.cs
```

현재 동작:

```text
살아 있는 적 1명
→ 집중 단일 대상 4히트
→ 타격당 8, 총 32

살아 있는 적 2명 이상
→ 전방 넓은 다중 대상 2히트
→ 타격당 10, 대상별 총 20
```

- 사거리 약 18m
- 락온 대상 우선
- 소비 15, 쿨타임 약 3초
- `DamageContext.CursedTechnique`
- 해 사용 상태 기록

### 9-4. E 팔

```text
근거리 약 3.4m
단일 대상 7히트
타격당 8, 총 56
마지막 타격 강화 넉백·경직
소비 45, 쿨타임 약 5초
DamageContext.CursedTechnique
팔 사용 상태 기록
```

### 9-5. 푸가와 복마어주자 잠금

- Q 해와 E 팔을 사용하면 HUD의 준비 카운트가 각각 1/1로 변함
- 둘 다 사용하면 `R · 푸가 · 준비 완료 · Milestone 2에서 구현`
- R을 눌러도 아직 발동하지 않음
- V는 `복마어주자 · 현재 잠김`
- 해·팔 검증 전에 상위 기술을 섞지 않음

### 9-6. 스쿠나 임시 외형

새 파일:

```text
unity/Assets/Scripts/Player/SukunaPrototypeAvatar.cs
```

- 분홍색 머리
- 얼굴 문양과 붉은 눈
- 어두운 적색 복장
- 이동 시 간단한 팔다리 움직임
- 최종 모델이 아닌 캐릭터 식별용 Primitive 프록시

## 10. 이번 테스트 순서

```text
1. git pull 후 Unity Console 빨간 오류 확인

2. 기본 고죠 상태에서 기존 Q/E/R/V와 무하한 확인

3. 숫자 2 입력
→ 장면이 재시작되는지
→ 스쿠나 외형과 RYOMEN SUKUNA · 시부야 사변 HUD
→ CE 95 / 160
→ 고죠 Q/E/R 패널과 무하한 HUD가 사라지는지

4. 스쿠나로 주령 B 영역전연 공격을 맞기
→ 무하한으로 막히지 않고 일반 피해를 받는지

5. 주령 두 마리가 살아 있을 때 Q 해
→ 두 대상을 향한 넓은 2히트 참격
→ CE 95 → 80

6. 주령 한 마리를 처치한 뒤 Q 해
→ 남은 한 대상에게 집중 4히트

7. E 팔
→ 약 3.4m 안에서 7히트와 마지막 넉백
→ CE 45 소비
→ 멀리서 사용하면 빗나가지만 소비·사용 기록은 남는지

8. Q/E 사용 후 R HUD
→ 푸가 준비 완료 표시
→ R을 눌러도 아직 발동하지 않는지

9. V
→ 복마어주자가 발동하지 않는지

10. 숫자 1 입력
→ 고죠로 장면 재시작
→ 기존 무하한·창·혁·자·무량공처가 정상인지
```

## 11. 알려진 한계

- 스쿠나 해·팔 명칭은 사용자가 계속 글로벌 한국어 클라이언트 기준으로 검토
- 푸가와 복마어주자 미구현
- 해의 넓은 참격은 현재 전방 원뿔 판정
- 참격 VFX와 사운드는 임시 LineRenderer·공용 효과음
- 스쿠나 외형은 최종 모델이 아님
- 현재 캐릭터 전환은 장면 재시작 방식
- 주령 AI는 단순 직선 추적
- 수동 카메라 조준 없음

## 12. 다음 우선순위

Milestone 1이 정상이라면:

```text
1. 전환·주력·해·팔 회귀 오류 수정
2. 해 단일/다중 판정과 팔 거리·자원 압박 평가
3. R 푸가 원작 밑준비 조건 재검토
4. 영역 밖 살아 있는 적 한 명 제약과 푸가 구현
5. 푸가 확인 뒤 V 복마어주자 구현
6. 고죠와 스쿠나에서 실제로 반복된 부분만 공통 구조로 추출
```

완성형 모델·애니메이션·보이스는 최소 두 캐릭터의 기술·영역 상태가 안정된 뒤 진행한다.

## 13. 새 채팅에서 Assistant가 먼저 할 일

1. 이 문서를 읽는다.
2. 고죠 전투는 이미 사용자 확인 완료임을 유지한다.
3. 사용자가 9번 스쿠나 Milestone 1을 테스트했는지 확인한다.
4. 오류가 있으면 수정하면서 다음 계획 작업을 진행한다.
5. 정상이라면 푸가 구현 전 원작 조건을 다시 조사한다.
6. 작업 뒤 이 문서를 반드시 갱신한다.
