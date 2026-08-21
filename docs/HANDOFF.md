# JJK Game 프로젝트 인수인계서

작성 기준일: 2026-08-21

> 새 ChatGPT 대화에서는 이 문서와 `docs/PROJECT_DIRECTION.md`, `docs/ORIGINAL_FIDELITY_POLICY.md`, `docs/CANON_SKILL_RULES.md`, `docs/DEVELOPMENT_ROADMAP.md`, `docs/PHANTOM_PARADE_REFERENCE.md`, `docs/SUKUNA_FIRST_VERSION_RESEARCH.md`, `docs/AUDIO_SETUP.md`, `unity/README.md`, 최신 코드를 먼저 읽는다.

이 문서는 현재 사용자 확인 상태, 게임 방향, 작업 방식과 다음 우선순위를 기록하는 단일 인수인계 기준이다.

## 1. 환경과 작업 방식

```text
프로젝트: 주술사 되어보기
저장소: JJuNi27/JJK_game
브랜치: master
로컬: D:\GitHub\JJK_game
Unity: D:\GitHub\JJK_game\unity
Unity 6.3 LTS 6000.3.20f1 / URP
```

사용자 pull:

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

기본 흐름:

```text
Assistant가 조사/설계
→ 작은 변경은 직접 master 수정
→ 큰 구조 변경은 Codex 사용 검토
→ Diff/코드 검토
→ 사용자가 git pull
→ 사용자 Unity에서 실제 테스트
→ 결과를 HANDOFF에 기록
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 2. 개발 리드 원칙

사용자의 아이디어를 무조건 구현하지 않는다.

```text
NOW
FOUNDATION_ONLY
LATER
REJECT
```

현재 개발 방법:

```text
Risk-Driven Vertical Slice Production
리스크 우선 버티컬 슬라이스 개발
```

Gate:

```text
Gate 0 Vision Lock
Gate 1 Core Combat Proof
Gate 2 Match Architecture Proof
Gate 3 Beauty Corner
Gate 4 Production Pipeline Extraction
Gate 5 Third Character Stress Test
Gate 6 Production
```

## 3. 게임 방향

현재 추천 본체:

```text
주술회전 캐릭터의 원작 규칙을 최대한 살린
Character-Authentic 3D Arena Action Fighter

기본: 1v1
확장: 2~3인 팀/교대전
후속: 4~8인 Incident Battle
```

참고 방향:

```text
NARUTO STORM 4
- 팀 편성/교대

DRAGON BALL: Sparking! ZERO
- 원작 강함/비대칭 캐릭터성
- 스펙터클/파괴

Jujutsu Shenanigans
- 타격감
- 파괴 가능한 전장
- 후속 난전 감각
```

메인으로 하지 않음:

```text
20~50인 상시 FFA
오픈월드 MMO/RPG
초기부터 완전 동적 파괴
```

## 4. 원작/주팬퍼 정책 — 확정

상세: `docs/ORIGINAL_FIDELITY_POLICY.md`

현재 가장 중요한 결정:

```text
밸런스보다 원작 구현 우선
```

소스 활용 순서:

```text
1. 만화 완결 범위 + 공식 팬북/애니/극장판/공식 설정
2. 주술회전 팬텀 퍼레이드의 전체 캐릭터 게임 설계
3. 글로벌 한국어 공식 명칭
4. 실시간 격투게임용 GAME_ORIGINAL 보완
5. 경쟁 밸런스
```

주팬퍼는 단순 번역 참고가 아니다.

다음 요소를 적극적으로 본다.

```text
스킬 구성
패시브/오토 스킬
최대/시작 주력
주력 소비/회복
타격 수
대상 수
선행 조건
영역 전후 스킬 변화
상태/저항/회복
VFX/SFX/보이스/카메라
```

단, 주팬퍼는 턴제이므로:

```text
턴제 설계
→ 그 효과가 표현하려는 캐릭터 특성 해석
→ 실시간 3D 격투 규칙으로 재설계
```

예: `턴 시작 시 피해 +20%` 같은 순수 턴제 계수는 그대로 가져오지 않는다. 반대로 영역 후 스킬 변화, 반전술식, 고유 저항, 해/팔→푸가 준비처럼 캐릭터성을 표현하는 구조는 적극적으로 옮긴다.

같은 캐릭터의 시대/육체/전투 방식 차이는 별도 캐릭터로 분리한다.

예:

```text
고죠
- 회옥·옥절 각성 전
- 회옥·옥절 각성 후
- 현대 교사
- 신주쿠 결전

스쿠나
- 이타도리 육체
- 메구미 육체
- 헤이안 진신/신주쿠
```

사용자 검수 글로벌 한국어:

```text
스쿠나 R = 푸가
내부 ID = Fuga
```

## 5. 입력 인식 방향

기존 Python 장인/음성 인식은 폐기하지 않는다.

현재 추천:

```text
Standard Input Mode
- 키/패드

Immersion Input Mode
- 카메라 장인 인식
- 음성 인식
```

두 방식은 나중에 같은 Gameplay Command를 발생시키도록 한다.

현재 스쿠나 V 단발 입력은 복마어주자 로직 테스트용 임시 입력이다.

## 6. Unity 공통 전투 — 사용자 확인 완료

```text
WASD 이동
SPACE 회피
TAB 타깃
LMB 기본 3단 공격
F1 도움말
ENTER 승패 뒤 재시작
```

공통 피해 구조:

```text
DamageDeliveryType
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

피해 판정 순서:

```text
회피 무적
→ 활성 IDamageGuard
→ HP 감소
```

## 7. 현대 고죠 — 사용자 확인 완료

현재 버전:

```text
ModernTeacher
```

기술:

```text
Q 창
E 혁
R 허식 자
V 무량공처
```

완료:

- 지속 수렴장 창
- 이동/관통 혁
- 창→혁 보너스
- 허식 자
- 무량공처 장인 입력
- 영역 종료 후 약 5초 술식 번아웃
- 육안 효율 주력 프로필
- 무하한 자동 방어
- 일반 타격 차단
- 영역전연 우회
- DomainSureHit 우회 구조
- 회피 무적 우선

## 8. 시부야 스쿠나 — Core Combat 사용자 확인 완료

현재 버전:

```text
SukunaShibuyaYujiBody
시부야 사변 · 이타도리 유지 육체
```

주력:

```text
최대 160
시작 95
초당 회복 4
```

기술:

```text
Q 해
- 적 1명 집중 4히트
- 다중 적 2히트
- CE 15

E 팔
- 근거리 단일 7히트
- CE 45

R 푸가 · 영역 밖
- 해/팔 사용 후 준비
- 살아 있는 적 1명 제약
- 약 24m GAME_ORIGINAL 테스트 거리
- 추적 화염탄
- CE 35
- 피해 78

V 복마어주자
- V 단발 임시 입력
- 개방형 영역
- 약 30m GAME_ORIGINAL 범위
- 8회 반복 DomainSureHit 참격
- CE 80
```

2026-08-21 사용자 추가 확인 완료:

```text
스쿠나 전용 복마어주자 오디오 분리 ✅
영역 밖 기존 푸가 회귀 정상 ✅
복마어주자 안 광역 푸가 정상 ✅
고죠 복귀 정상 ✅
```

### 영역 안 푸가 현재 규칙

```text
해 + 팔 준비 필요
복마어주자 활성 중 R
적 1명 제한 해제
영역 밖 24m 제한 미적용
영역 내 적 모두 광역 폭발 피해
CE 35
성공 시 해/팔 준비 소모
```

영역 안 푸가는 현재 `CursedTechnique`이며 `DomainSureHit`으로 만들지 않았다.

이유:

```text
영역 안에서 푸가가 광역 열압식 폭발로 사용되는 것은 반영
하지만 푸가 자체가 자동 필중/무하한 우회인지까지는 단정하지 않음
```

`INTERPRETATION_PENDING` 유지.

복마어주자 참격 자체가 해/팔 준비를 자동으로 충족하는지도 아직 자동화하지 않았다.

## 9. 스쿠나 전용 오디오

파일:

```text
unity/Assets/Scripts/Player/SukunaCombatAudio.cs
```

로컬 경로:

```text
unity/Assets/Resources/LocalAudio/
```

지원 파일명:

```text
Sukuna_Domain
Sukuna_DomainSFX
Sukuna_DomainFuga
```

파일이 없어도 스쿠나 전용 합성 fallback이 재생된다.

## 10. 2026-08-21 적 2마리 겹침 버그 — 수정 원격 반영 / 사용자 테스트 필요

사용자 보고:

```text
적이 두 마리 있어야 하는데 가끔 화면에는 한 마리만 보임
예전부터 간헐적으로 존재
```

조사 결과:

`MatchController`는 실제로 기본 적 + 런타임 clone을 서로 다른 위치에 배치한다.

하지만 `CurseBotController`의 기존 AI는 모든 적이:

```text
각자 스폰
→ 플레이어의 정확히 같은 중심점 추적
→ 가까워지며 서로 같은 위치/카메라 선상에 겹칠 수 있음
```

구조였다.

수정 파일:

```text
unity/Assets/Scripts/Enemy/CurseBotController.cs
```

새 동작:

```text
Start에서 현재 활성 CurseBot 목록 확인
→ 이름 기준으로 안정적으로 슬롯 배정
→ 플레이어 주변 원형 engagement slot 계산
→ 각 봇이 자기 슬롯으로 접근
→ 충분히 자리 잡은 뒤 공격
```

현재 두 마리면 서로 약 180도 반대 자리로 접근한다.

목적:

- 모델이 한 위치에 겹쳐 한 마리처럼 보이는 현상 방지
- 두 적의 공격 텔레그래프 가독성 개선
- 나중에 테스트 적 수가 늘어도 원형으로 자리를 분산

이 변경은 사용자 Unity 테스트 전이다.

테스트:

```text
1. git pull
2. Unity Console 빨간 오류 확인
3. 전투 재시작을 여러 번 반복
4. 적 두 마리가 처음부터 실제로 보이는지
5. 플레이어에게 접근한 뒤에도 두 모델이 겹쳐 한 마리처럼 보이지 않는지
6. Curse A 일반 공격 / Curse B 영역전연 공격이 둘 다 정상인지
7. TAB 타깃 전환으로 두 적 모두 선택 가능한지
```

## 11. Gate 상태

```text
Gate 0 Vision Lock: 충분히 진행됨
Gate 1 Core Combat Proof: 고죠/스쿠나 기능 기준 완료
Gate 1 잔여: 적 겹침 버그 회귀 확인만 필요
Gate 2 Match Architecture Proof: 다음 큰 작업
```

## 12. 다음 우선순위

적 겹침 수정이 정상이라면 Team Match Architecture로 넘어간다.

여기부터는 여러 파일에 걸친 구조 변경이므로 Codex 사용을 적극 검토한다.

먼저 코드베이스 전체를 분석하고 Acceptance Criteria를 확정한 뒤 구현한다.

최소 목표:

```text
Solo 1v1 유지
2v2 Team Prototype
Active Fighter
Reserve Fighter
전투 중 교대
각 캐릭터 HP/CE 독립 보존
KO 시 다음 캐릭터 입장
팀 HUD
교대 후 Target Lock/Camera 정상 재연결
```

초기에는 넣지 않음:

```text
지원 공격
합동 궁극기
완성형 캐릭터 선택 화면
3인 팀 최종 UI
온라인 네트워크
```

현재 `1/2 키 → 장면 재시작` 구조는 Gate 2에서 교체 대상이다.

## 13. Gate 2 뒤 — Beauty Corner

Team Match 최소 구조가 검증된 뒤 작은 범위를 거의 완성품 품질로 만든다.

목표 예:

```text
대표 전투장 한 구역
실제 고죠 모델
실제 스쿠나 모델
이동/공격/회피 애니메이션
허식 자
푸가
무량공처
복마어주자
대표 VFX/SFX/보이스
카메라/히트스톱/타격감
```

게임 전체를 90% 만드는 것이 아니라 작은 한 조각을 약 90% 품질로 만든다.

이 과정에서 실제로 반복되는 구조만:

```text
Character Asset Contract
Animation Event Contract
Technique Presentation Profile
VFX lifecycle
Voice/SFX event
Camera feedback
Hit stop
```

형태로 추출한다.

거대한 범용 Ability System을 미리 만들지 않는다.

## 14. Codex 사용 기준

직접 수정 적합:

```text
작은 버그
1~3개 파일
조건/수치
작은 기능
문서
```

Codex 적합:

```text
Team/Match Architecture
여러 파일 리팩터링
Player와 Character 생명주기 분리
반복 코드 마이그레이션
테스트 추가
전체 코드베이스 탐색이 필요한 변경
```

Codex 절차:

```text
1. Assistant가 설계/원작 규칙/Acceptance Criteria 확정
2. Codex에 구체적 작업 명세
3. Codex 변경 Diff 검토
4. 사용자 Unity 테스트
5. HANDOFF 갱신
```

게임 디자인/원작 판정은 Codex에게 맡기지 않는다.

## 15. 새 채팅에서 먼저 할 일

1. 이 문서를 읽는다.
2. 고죠 Core Combat은 사용자 확인 완료로 유지한다.
3. 스쿠나 해/팔/영역 밖 푸가/복마어주자/영역 안 푸가/전용 영역 오디오는 사용자 확인 완료로 유지한다.
4. 적 2마리 engagement slot 수정이 사용자 테스트됐는지 확인한다.
5. 정상이라면 Gate 2 Team Match Architecture 설계로 이동한다.
6. Gate 2는 Codex 사용을 적극 검토한다.
7. 주팬퍼를 단순 번역이 아니라 전체 게임 설계 참고자료로 사용한다.
8. 밸런스보다 원작 재현을 우선한다.
9. 작업 뒤 이 문서를 갱신한다.
