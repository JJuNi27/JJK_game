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

Assistant가 `master`를 수정하고 사용자가 pull 후 Unity에서 실행해 확인한다. Assistant는 Unity를 직접 컴파일했다고 주장하지 않는다.

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
- 고죠 공통 피해·무하한: 사용자 확인 완료
- 스쿠나 Milestone 1 해·팔: 사용자 확인 완료
- 스쿠나 Milestone 2 푸가: 사용자 확인 완료
- 스쿠나 Milestone 3 복마어주자: 사용자 확인 완료
- 스쿠나 전용 영역 사운드 분리: 다음 소규모 정리 작업
- 영역 안 푸가 광역 전환: 다음 핵심 작업 `NOW`
- 완성형 모델·애니메이션·보이스: `LATER`

## 3. 원작·공식 게임 참고 원칙

1. 만화·공식 팬북·공식 애니메이션·극장판 설정 우선
2. 팬텀 퍼레이드는 자원·스킬 구성·모션·VFX·카메라·사운드의 적극적인 참고자료
3. GameWith는 일본 서버 상세 수치와 일본 원문 조사처
4. UI 명칭은 글로벌 한국어 클라이언트 우선
5. 사용자가 글로벌 클라이언트 기준으로 검수한 명칭은 `USER_VERIFIED_GLOBAL_KO`
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

스쿠나:

```text
Q 해
E 팔
R 푸가
V 복마어주자
```

현재 스쿠나 V는 핵심 영역 로직 검증용 단발 임시 입력이다. 최종적으로 Python 장인·음성 인식 계층과 연결할 예정이며, 현재는 V를 한 번 누르면 약 0.65초 뒤 전개된다.

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

### 시부야 스쿠나 Milestone 1 — 사용자 확인 완료

- 숫자 1·2 캐릭터 전환과 장면 재시작
- 스쿠나 외형과 전용 HUD
- 최대 주력 160 / 시작 95
- Q 해: 다중 적 2히트, 단일 적 집중 4히트
- E 팔: 근거리 단일 7히트와 마지막 넉백
- 해·팔 사용 상태 기록
- 스쿠나 선택 시 고죠 무하한과 술식 비활성화
- 숫자 1로 고죠 복귀 시 기존 전투 정상

### 시부야 스쿠나 Milestone 2·3 — 사용자 확인 완료

사용자가 다음을 정상 확인했다.

- Q 해와 E 팔 사용 후 R 푸가 준비
- 영역 밖 살아 있는 적 한 명일 때 푸가 발동
- 추적 화염탄, 폭발, 피해와 해·팔 준비 소모
- 적이 두 명일 때 푸가 차단
- V 단발 입력 후 복마어주자 전개
- 밀폐 벽 없이 사당과 지면 범위선이 나타나는 개방형 표현
- 범위 안 두 적에게 반복 필중 참격 적용
- 영역 종료와 쿨타임 복귀
- 현재 V 단발 방식이 임시 디버그 입력이라는 점을 사용자와 합의

## 7. 캐릭터 버전 기반

고죠:

```text
HiddenInventoryPreAwakening  회옥·옥절 각성 전
HiddenInventoryAwakened     회옥·옥절 각성 후
ModernTeacher               현대 일반 고죠
ShinjukuShowdown            신주쿠 결전 고죠
```

현재 고죠 선택은 `ModernTeacher`다. 번아웃 조기 회복은 신주쿠 결전 버전에만 예약되어 있고 입력은 연결하지 않았다.

첫 스쿠나:

```text
내부 ID: SukunaShibuyaYujiBody
시기·육체: 시부야 사변 · 이타도리 유지 육체 스쿠나
주 참고: 팬텀 퍼레이드 両面宿儺「火力勝負」
영역 참고: 宿儺「本物の呪術」
```

헤이안 진신, 메구미 육체, 세계참과 신주쿠 고급 운용은 보류한다.

## 8. 푸가 현재 상태

핵심 파일:

```text
unity/Assets/Scripts/Player/SukunaTechniqueController.cs
unity/Assets/Scripts/Player/SukunaFugaProjectile.cs
```

발동 조건:

```text
해 사용 1/1
팔 사용 1/1
영역 밖 살아 있는 적 정확히 1명
대상 거리 약 24m 이하
주력 35 이상
쿨타임 종료
```

프로토타입 수치:

```text
준비 약 0.52초
추적 발사체 속도 약 18
단일 대상 피해 78
넉백 24
경직 1.15초
쿨타임 약 11초
```

- 피해는 `DamageContext.CursedTechnique`
- 무하한을 자체적으로 우회하지 않음
- 영역 밖 폭발은 지정된 한 대상에게만 피해 적용
- 성공 발동 시작 시 해·팔 준비 상태 소모
- 약 24m 거리 조건은 원작 조건이 아닌 조정 가능한 `GAME_ORIGINAL` 값
- 사용자는 거리 조건을 당장은 유지하고 추후 상세 조사·밸런싱 때 재검토하기로 함

## 9. 복마어주자 현재 상태 — 사용자 확인 완료

핵심 파일:

```text
unity/Assets/Scripts/Player/SukunaDomainController.cs
unity/Assets/Scripts/Player/SukunaMalevolentShrineVisual.cs
unity/Assets/Scripts/Core/CombatActionGate.cs
unity/Assets/Scripts/Player/ThirdPersonPlayerController.cs
unity/Assets/Scripts/Player/PrototypeCharacterController.cs
```

### 입력과 상태

- 스쿠나 선택 상태에서 V 한 번 입력
- 약 0.65초 준비 후 전개
- V 직접 입력은 향후 Python 장인·음성 인식 연결 전까지의 임시 `GAME_ORIGINAL` 입력
- 준비 중 이동속도 약 35%
- 준비·전개 중 Q/E/R, 기본 공격과 새 회피 차단
- 전개 중 일반 이동 허용

### 개방형 영역 시각

- 장벽 벽이나 밀폐 구체를 생성하지 않음
- 발동 위치에 고정된 Primitive 사당 생성
- 지면에 외곽·중간·내부 범위선과 방사형 선 표시
- 적마다 교차 참격 LineRenderer 표시
- 영역 종료 시 약 0.55초 동안 페이드아웃

### 영역 수치

```text
주력 소비 80
준비 약 0.65초
반경 약 30m
지속 약 3.2초
필중 참격 8회
회당 피해 12
참격 간격 약 0.38초
미세 경직 0.10초
쿨타임 약 18초
```

- 현재 전투장이 약 65×65이므로 원작의 약 200m 범위를 그대로 쓰지 않음
- 30m는 다중 적·가독성 테스트 값
- 팬텀 퍼레이드의 적 전체 8히트 표현을 실시간 반복 참격으로 번역

### 피해 규칙

- `DamageDeliveryType.DomainSureHit`
- 기존 무하한 우회 구조와 연결
- `Health`의 현재 처리 순서상 회피 무적이 먼저 적용됨
- 현재 플레이어가 스쿠나이고 적이 훈련 봇이라 무하한 우회를 화면에서 직접 비교하는 PvP 테스트는 아직 없음
- 코드 경로는 기존 `DamageContext.BypassesInfinity` 규칙을 사용

## 10. 이번 단계에서 제외한 것

```text
영역 안 푸가 광역 전환
영역 사용 후 최대 주력 증가·대량 회복
영역 후 Q/E/R 변화
영역 충돌·상쇄·덮어쓰기
완성형 사당·환경 파괴
스쿠나 전용 영역 보이스·사운드 믹싱
```

현재 영역 발동음은 공통 프로토타입 영역 효과음을 재사용한다. 로컬 `Gojo_Domain` 음성이 등록된 환경에서는 잘못된 음성이 재생될 가능성이 있어 전용 사운드 분리가 다음 정리 후보다.

## 11. 알려진 한계

- 현재 복마어주자는 V 직접 입력이며 장인 입력이 아님
- 사당과 참격은 Primitive·LineRenderer 임시 VFX
- 영역 필중과 회피 무적의 실제 게임 디자인은 추후 조정 가능
- 무하한 적을 상대로 한 복마어주자 직접 화면 테스트 없음
- 영역 전용 스쿠나 보이스·사운드 미분리
- 영역 안 푸가 광역형 미구현
- 영역 후 스킬 변화·주력 증가·회복 미구현
- 현재 캐릭터 전환은 장면 재시작 방식
- 스쿠나 외형은 최종 모델이 아님

## 12. 다음 우선순위

```text
1. 스쿠나 전용 영역 사운드 분리
2. 영역 안 푸가 광역 전환의 정확한 조건 재조사
3. 영역 활성 중에는 푸가의 적 1명 제한을 해제하고 범위형 폭발로 전환
4. 영역 밖/안 푸가의 대상·피해·주력·준비 상태 소모 비교
5. 회귀 테스트 뒤 영역 후 상태 변화나 공통 영역 구조 추출 여부 판단
```

영역 안 푸가를 구현할 때도 팬텀 퍼레이드 한 유닛의 수치를 그대로 복제하지 않고, 원작의 영역 내 광역 운용 원리와 현재 실시간 전투 구조를 먼저 대조한다.

최종 모델·정교한 애니메이션·완성형 사운드는 최소 두 캐릭터의 기술·영역 상태가 안정된 뒤 진행한다.

## 13. 새 채팅에서 Assistant가 먼저 할 일

1. 이 문서를 읽는다.
2. 고죠 전투와 스쿠나 해·팔·푸가·복마어주자는 사용자 확인 완료임을 유지한다.
3. 스쿠나 전용 영역 사운드 분리부터 짧게 처리한다.
4. 그다음 영역 안 푸가 광역 전환 조건을 조사·구현한다.
5. 푸가 거리 24m는 원작 조건으로 설명하지 않고 추후 검토 대상으로 유지한다.
6. 현재 V 단발 입력은 임시 디버그 입력으로 유지한다.
7. 작업 뒤 이 문서를 반드시 갱신한다.
