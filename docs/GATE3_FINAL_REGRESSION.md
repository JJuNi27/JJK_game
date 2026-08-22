# Gate 3 — Final Beauty Corner Regression

작성 기준일: 2026-08-23

## 목적

Gate 3A~3D에서 개별적으로 확인한 기능을 한 번의 플레이 세션에서 다시 연결 확인한다.

이 테스트는 새 기능 검증이 아니라 `회귀 없음`을 확인하는 Gate 3 종료 테스트다.

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 시작

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

Unity에서:

```text
1. Console 빨간 오류가 없는지 확인
2. CombatMVP Play 시작
3. 가능하면 Play 시작 직후부터 아래 순서대로 진행
```

---

## A. 기본 화면 / Arena

확인:

```text
[ ] 야간 도심 분위기 표시
[ ] 배경 건물 실루엣 표시
[ ] Arena ring 표시
[ ] 배경 건물은 통과 가능
[ ] Player / Team HUD 겹침 없음
[ ] 우하단 Skill Deck 표시
```

건물 통과는 현재 Prototype 설계상 정상이다.

---

## B. 고죠 Active

Skill Deck:

```text
[ ] Q 창
[ ] E 혁
[ ] R 허식 자
[ ] V 무량공처
[ ] Q/E/R/V 입력 시 해당 chip 짧은 pulse
```

기본 전투:

```text
[ ] 기본 3타 정상
[ ] 적중 시 Shake / Flash / 짧은 Hit Stop
[ ] 실제 맞은 위치 Impact Burst
[ ] 3타 FINISH가 1/2타보다 강하게 읽힘
[ ] 허공 공격에는 Hit feedback 없음
[ ] SPACE Dodge 정상
[ ] TAB Target Lock 정상
```

허식 자:

```text
[ ] 기존 긴 보라 레이저가 보이지 않음
[ ] 청색 + 적색 구체가 합쳐짐
[ ] 큰 보라 구체가 전방으로 이동
[ ] Flash / Shake / FOV / Camera Focus가 정상 복귀
```

무량공처:

```text
[ ] V 준비 상태 정상
[ ] 기존 영역 입력 흐름 정상
[ ] Domain 개방 표현 정상
[ ] Camera Focus 이후 플레이어 중심 시점 복귀
[ ] 영역 종료/번아웃 흐름 정상
[ ] Skill Deck 상태가 DOMAIN INPUT / DOMAIN ACTIVE / TECHNIQUE BURNOUT 상황을 읽을 수 있음
```

---

## C. Player Tag → 스쿠나

```text
[ ] T Tag 정상
[ ] Active Fighter HUD가 스쿠나로 변경
[ ] Skill Deck가 즉시 스쿠나 기술로 변경
[ ] 고죠 HP/CE 저장
[ ] 스쿠나 HP/CE 로드/유지
[ ] Tag 후 기본 공격 sequence 초기화
```

Skill Deck:

```text
[ ] Q 해
[ ] E 팔
[ ] R 푸가
[ ] V 복마어주자
```

스쿠나 전투:

```text
[ ] Q 해 정상
[ ] E 팔 정상
[ ] 푸가 준비 조건 정상
[ ] 영역 밖 푸가 발사 정상
[ ] 푸가 폭발 위치 Camera Focus 정상
[ ] 복마어주자 Casting / Active 표현 정상
[ ] 영역 안 광역 푸가 정상
```

---

## D. Player Team 상태 보존

```text
[ ] 스쿠나 → 고죠 재Tag 시 고죠 HP 복원
[ ] 고죠 CE 복원
[ ] Reserve 중 HP/CE가 임의로 변하지 않음
[ ] Action Lock 중 Tag 차단
[ ] KO된 캐릭터로 수동 Tag 불가
[ ] 첫 Player KO에서 살아있는 Reserve로 Auto Tag
[ ] 첫 KO에는 DEFEAT 없음
[ ] 마지막 Player KO에만 DEFEAT
```

---

## E. Opponent Team

F2로 Team Battle 진입 후 확인:

```text
[ ] OPPONENT TEAM HUD 정상
[ ] 상대는 한 번에 Active 한 명만 전장에 존재
[ ] 첫 상대 KO 뒤 Reserve Entry
[ ] 첫 상대 KO에는 VICTORY 없음
[ ] 새 Reserve 등장 시 Target Lock 자동 승계
[ ] 마지막 상대 KO에만 VICTORY
```

---

## F. 종료 판정

다음이 모두 만족되면 Gate 3 종료 가능:

```text
Console 빨간 오류 없음
핵심 Gameplay 회귀 없음
Gate 3 Presentation이 서로 충돌하지 않음
카메라가 기술 후 이상한 방향에 남지 않음
HUD가 Active Fighter / Match State를 잘못 표시하지 않음
```

사용자 최종 확인 문구 예:

```text
Gate 3 회귀 정상
```

그 확인 이후 문서 상태를:

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: STARTED
```

로 변경한다.

## Gate 4로 넘길 Prototype 부채

Gate 3 종료 시 일부러 남기는 Prototype 성격:

```text
- IMGUI 기반 HUD
- Runtime procedural Arena scaffold
- Runtime procedural fighter avatar/pose
- Prototype Signature VFX controller 계층
- 허식 자 시각 구체와 실제 Damage timing 불일치
- 캐릭터별 코드에 섞여 있는 Presentation 연결
```

이 항목들은 Gate 4에서 공통 계약을 추출할 때 정리한다.
