# JJK Game 프로젝트 인수인계서

> 새 ChatGPT 대화에서는 `README.md`, 이 문서, `docs/PROJECT_DIRECTION.md`, `docs/DEVELOPMENT_ROADMAP.md`, `docs/ORIGINAL_FIDELITY_POLICY.md`, `docs/CANON_SKILL_RULES.md`, `docs/PHANTOM_PARADE_REFERENCE.md`, `docs/SUKUNA_FIRST_VERSION_RESEARCH.md`, `docs/AUDIO_SETUP.md`, `unity/README.md`와 최신 코드를 먼저 읽는다.

작성 기준일: 2026-08-21

이 문서는 **사용자 확인 완료**, **현재 원격 구현/테스트 대기**, **게임 방향**, **개발 방법**, **다음 우선순위**를 분리하는 단일 인수인계 기준이다.

## 1. 환경과 작업 방식

- 프로젝트: **주술사 되어보기**
- 저장소: `JJuNi27/JJK_game`
- 브랜치: `master`
- 로컬: `D:\GitHub\JJK_game`
- Unity 프로젝트: `D:\GitHub\JJK_game\unity`
- Unity 6.3 LTS `6000.3.20f1`, URP

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

기본 흐름:

```text
Assistant가 원작/주팬퍼/구조 조사 및 설계
→ 작은 작업은 master 직접 수정
→ 큰 구조 변경은 필요할 때 Codex 사용
→ Diff/코드 검토
→ 사용자가 git pull
→ 사용자 Unity에서 실제 실행 테스트
→ 결과를 HANDOFF에 기록
```

Assistant는 Unity를 직접 컴파일했다고 주장하지 않는다.

## 2. 기술 리드 원칙

```text
NOW             현재 구조에 필요해 바로 구현
FOUNDATION_ONLY 기반만 준비하고 콘텐츠는 보류
LATER           좋은 아이디어지만 후반이 효율적
REJECT          프로젝트 방향·원작·구조와 맞지 않음
```

사용자의 제안을 무조건 구현하지 않는다. 원작/게임 방향/재작업 위험을 보고 판단한다.

## 3. 원작 재현 정책 — 확정

상세: `docs/ORIGINAL_FIDELITY_POLICY.md`

현재 우선순위:

```text
1. 원작에서 실제로 어떻게 싸우는가
2. 주술회전 팬텀 퍼레이드의 캐릭터 게임 설계
3. 주팬퍼 턴제 구조를 실시간 3D 격투 규칙으로 변환
4. 필요한 GAME_ORIGINAL 보완
5. 경쟁 밸런스
```

중요:

- 주팬퍼는 **단순 번역/명칭 참고자료가 아님**.
- 스킬 구성, 패시브, 최대/시작 주력, 소비/회복, 상태 변화, 타격 수, 대상 수, 선행 조건, 영역 전후 변화, 반격/회복/저항, VFX/SFX/보이스까지 전부 적극 참고한다.
- 단 주팬퍼는 턴제이므로 숫자/턴 문구를 그대로 복사하지 않고 의도를 실시간 격투 규칙으로 변환한다.
- 원작과 충돌하면 원작 우선.
- 밸런스 때문에 고죠/스쿠나 같은 강캐의 핵심 능력을 약화하지 않는다.
- 만화 완결 범위 전체 사용 가능.
- 같은 인물도 시기/육체/습득 기술이 다르면 별도 플레이 버전으로 분리한다.

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

사용자 검수 완료:

```text
스쿠나 R 표시명: 푸가
내부 ID: Fuga
```

## 4. 현재 게임 방향 — 잠정 추천안

상세: `docs/PROJECT_DIRECTION.md`

```text
캐릭터 원작성을 최우선으로 한
3D 아레나 액션 격투게임
```

참고 방향:

```text
NARUTO STORM 4
- 소규모 팀
- 전투 중 캐릭터 교대

DRAGON BALL: Sparking! ZERO
- 캐릭터 강함/개성의 비대칭성
- 캐릭터가 된 느낌
- 강한 기술의 스펙터클/환경 반응

Jujutsu Shenanigans
- 즉각적인 타격감
- 파괴 전장
- 후속 사건전의 난전 감각
```

메인 후보:

```text
Solo 1v1
+ 2~3인 Team Battle
+ 후속 Incident Battle 4~8인
```

20~50인 상시 FFA나 오픈월드 MMO/RPG는 현재 메인 방향이 아니다.

## 5. 개발 방법 — Risk-Driven Vertical Slice

```text
Gate 0 Vision Lock
→ 게임 방향/원작 설계 원칙

Gate 1 Core Combat Proof
→ 고죠/스쿠나 핵심 전투

Gate 2 Match Architecture Proof
→ Solo + 2v2 Active/Reserve/교대

Gate 3 Beauty Corner
→ 작은 대표 구간을 거의 완성품 품질로 제작

Gate 4 Production Pipeline Extraction
→ 실제 반복된 모델/애니/VFX/오디오 규격만 공통화

Gate 5 Third Character Stress Test
→ 메구미/유타처럼 구조가 다른 캐릭터로 검증

Gate 6 Production
→ 캐릭터/맵/온라인/사건전 확장
```

현재는 **Gate 1 마무리** 단계다.

## 6. Codex 사용 기준

Codex를 무조건 사용하지 않는다.

직접 수정:

```text
- 1~3개 중심 파일의 명확한 기능/버그
- 작은 조건/수치
- 문서
```

Codex 적합:

```text
- 여러 파일에 걸친 Team/Match 구조 리팩터링
- Player / Fighter / Team 생명주기 분리
- 반복 코드 마이그레이션
- 테스트 추가
- 전체 코드베이스 탐색이 필요한 구조 변경
```

이번 `스쿠나 영역 사운드 + 영역 안 푸가`는 Assistant가 직접 구현했다.

## 7. 현재 Unity 공통 조작

```text
WASD 이동
SPACE 회피
TAB 타깃 전환
LMB 3단 기본 공격
F1 도움말
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

스쿠나:

```text
Q 해
E 팔
R 푸가
V 복마어주자
```

스쿠나 V 단발은 핵심 영역 로직 검증용 임시 입력이다.

## 8. 사용자 확인 완료 — 현대 고죠

- 이동/3단 공격/회피
- 창 지속 수렴장
- 혁 이동·관통 발사체
- 창→혁 보너스
- 허식 자
- 무량공처 장인 입력
- 영역 후 약 5초 술식 번아웃
- 육안 효율형 주력
- 현대 고죠 무하한 자동 방어
- 일반 타격 차단
- 영역전연 우회
- 회피 무적 우선
- `DamageContext` 마이그레이션

공통 피해:

```text
DamageDeliveryType
PhysicalStrike
CursedTechnique
DomainSureHit
Environmental

DamageTraits
DomainAmplification
TechniqueNullification
IgnoresInfinity
Unblockable
```

## 9. 사용자 확인 완료 — 시부야 스쿠나 기존 기능

버전:

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
- 다중 적 2히트
- 단일 적 집중 4히트
- CE 15

E 팔
- 근거리 단일 7히트
- CE 45

R 푸가 · 영역 밖
- 해/팔 사용 후 준비
- 살아 있는 적 정확히 1명
- 현재 약 24m GAME_ORIGINAL 거리 조건
- 추적 화염탄
- CE 35
- 피해 78

V 복마어주자
- V 단발 임시 입력
- 개방형 사당/지면 범위선
- 반경 약 30m GAME_ORIGINAL
- 8회 반복 DomainSureHit 참격
- CE 80
```

사용자가 해/팔/영역 밖 푸가/복마어주자 모두 정상 확인했다.

## 10. 2026-08-21 새 원격 구현 — 사용자 테스트 필요

### A. 스쿠나 영역 오디오 완전 분리

새 파일:

```text
unity/Assets/Scripts/Player/SukunaCombatAudio.cs
```

복마어주자는 더 이상 `PrototypeCombatAudio.PlayDomain()`을 사용하지 않는다.

```text
Sukuna_Domain
- 복마어주자 음성

Sukuna_DomainSFX
- 복마어주자 전개 효과음

Sukuna_DomainFuga
- 영역 내 푸가 광역 폭발 효과음
```

경로:

```text
unity/Assets/Resources/LocalAudio/
```

로컬 파일이 없어도 합성 fallback SFX가 재생된다.

### B. 복마어주자 안 푸가 광역 전환

원작/주팬퍼 재확인:

- 푸가는 해·팔 준비 뒤 사용.
- 원작 259화 설명상 영역 밖에서는 다수 대상 사용 제한.
- 복마어주자 안에서는 분쇄된 물질에 폭발성 주력을 입혀 영역 범위 전체를 열압식 폭발처럼 활용.
- 주팬퍼 `[화력 승부]`는 해 15 / 팔 45 / 푸가 선행조건을 게임 구조로 표현.

새 파일:

```text
unity/Assets/Scripts/Player/SukunaDomainFugaBlast.cs
```

수정 파일:

```text
unity/Assets/Scripts/Player/SukunaTechniqueController.cs
unity/Assets/Scripts/Player/SukunaDomainController.cs
```

현재 구현 규칙:

```text
영역 밖
- 기존 단일 푸가 그대로 유지
- 적 1명 조건 유지
- 약 24m 거리 조건 유지

복마어주자 안
- 해 + 팔 준비는 현재 동일하게 필요
- R 사용 가능
- 영역 밖의 적 1명 제한 해제
- 24m 제한을 영역 내 푸가에는 적용하지 않음
- 화염탄 속도 약 1.55배 GAME_ORIGINAL
- 화염탄이 터지면 영역 중심 기준 반경 전체에 폭발
- 영역 안 살아 있는 적 모두 피해 78
- CE 35
- 성공 시 해/팔 준비 소모
```

중요 판정:

```text
영역 내 푸가 피해 타입 = CursedTechnique
```

현재는 `DomainSureHit`으로 만들지 않았다. 푸가의 열압식 폭발이 영역 안에서 광역화되는 것은 확인되지만, **푸가 자체가 무하한을 자동 우회하는 필중인지까지 단정하지 않는다.** 이 부분은 `INTERPRETATION_PENDING`으로 유지한다.

### C. 현재 영역 내 푸가 준비 방식의 한계

현재 프로토타입은 여전히:

```text
Q 해 사용
E 팔 사용
→ 푸가 준비
```

를 요구한다.

복마어주자의 참격 자체가 해/팔 준비를 자동으로 충족시키는지까지는 이번 변경에서 자동화하지 않았다. 원작의 영역 내 조리/분쇄 과정을 더 정확히 모델링할 때 다시 검토한다.

## 11. 새 기능 테스트 순서

```text
1. git pull
2. Unity Console 빨간 오류 없는지 확인
3. 숫자 2 → 스쿠나

[영역 오디오]
4. V 복마어주자
→ 고죠 영역 음성이 나오지 않는지
→ Sukuna_Domain 파일이 있으면 그 음성이 재생되는지
→ 파일 없으면 스쿠나 전용 fallback SFX인지

[영역 밖 푸가 회귀]
5. 적 2명일 때 Q/E 후 R
→ 여전히 차단
6. 적 1명일 때 Q/E 후 R
→ 기존 단일 푸가 정상

[영역 안 푸가]
7. 적 2명 살아 있는 새 전투에서 Q와 E 사용
8. CE를 충분히 회복시킨 뒤 V 복마어주자
9. 영역 활성 중 R
→ HUD에 `영역 내 광역 2명` 표시 확인
→ 푸가 발사
→ 영역 중심에서 큰 주황/적색 폭발
→ 영역 안 적 둘 다 피해
→ 해/팔 준비 초기화

10. 숫자 1 → 고죠
→ 창/혁/자/무량공처/무하한 회귀 없는지
```

참고: Q15 + E45 + V80 + R35이므로 연속 입력하면 주력이 부족하다. Q/E 뒤 CE를 충분히 회복시켜 V를 쓰면 테스트 가능하다.

## 12. 현재 알려진 한계

- Unity 직접 컴파일 전이므로 새 코드 사용자 테스트 필요
- 영역 내 푸가의 `CursedTechnique` vs `DomainSureHit` 판정은 추가 원작 검토 대상
- 복마어주자 자체의 참격이 푸가 준비를 자동 충족하는 구조 미구현
- 푸가 약 24m 조건은 영역 밖 GAME_ORIGINAL이며 추후 재검토
- 영역 후 최대 주력 증가/회복/스킬 변화 미구현
- 영역 충돌/상쇄/덮어쓰기 미구현
- Python 장인/음성 Unity 연결 미구현
- 현재 캐릭터 선택은 `1/2 키 → 장면 재시작`
- 실제 고죠/스쿠나 모델/완성 VFX/애니메이션 미적용

## 13. 다음 우선순위

새 기능이 정상이라면 Gate 1을 마감한다.

```text
1. 새 영역 오디오/영역 푸가 회귀 오류 수정
2. 고죠/스쿠나 전체 회귀 테스트 완료 기록
3. 스쿠나 Core Combat Gate 1 완료 선언
```

그 다음 게임 방향을 팀 아레나 추천안으로 유지한다면:

```text
4. 기존 Player/Character/Match 구조 전체 분석
5. Team Match Acceptance Criteria 확정
6. 여기부터 Codex 사용 적극 검토
7. 2v2 Active / Reserve 최소 구조
8. 교대 시 HP/CE 보존
9. KO 후 다음 캐릭터 입장
10. Target Lock / Camera / HUD 전환 검증
11. 그 뒤 Beauty Corner 시작
```

Beauty Corner 목표:

```text
대표 맵 한 구역
실제 고죠/스쿠나 모델
이동/기본공격/회피 애니메이션
허식 자 / 푸가
무량공처 / 복마어주자
대표 VFX/SFX/보이스
카메라/히트스톱/타격감
```

## 14. 새 채팅에서 Assistant가 먼저 할 일

1. 이 문서를 읽는다.
2. 고죠 기존 전투와 스쿠나 해/팔/영역 밖 푸가/복마어주자는 사용자 확인 완료임을 유지한다.
3. 10번의 새 `스쿠나 영역 오디오 분리 + 영역 내 푸가`가 사용자 테스트됐는지 확인한다.
4. 오류가 있으면 새 기능만 우선 수정한다.
5. 정상이라면 Gate 1 마감 후 Team Match Architecture로 이동한다.
6. Team/Match 대규모 리팩터링은 Codex를 적극 검토한다.
7. 주팬퍼는 단순 번역이 아니라 전체 캐릭터 게임 설계 참고자료로 계속 사용한다.
8. 밸런스보다 원작 재현을 우선한다.
9. 작업 후 반드시 이 문서를 갱신한다.
