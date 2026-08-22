# Gate 4 — Final Regression

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Technique Presentation Request: USER VERIFIED
Gate 4D VFX Lifecycle Contract: USER VERIFIED
Gate 4E Animation Contract: USER VERIFIED
Gate 4F Audio Event Contract: USER VERIFIED
Prototype / Production Boundary Review: COMPLETE

Gate 4 Final Regression: USER TEST PENDING
Gate 4 Production Pipeline Extraction: NOT CLOSED YET
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

---

# 목적

Gate 4는 화면에 새 기능을 많이 추가한 단계가 아니라 Gate 1~3 prototype의 연결 구조를 바꾼 단계다.

따라서 마지막 한 세션에서 다음을 확인한다.

```text
1. 기존 Gojo/Sukuna Gameplay가 그대로인가?
2. Team/Match rule이 그대로인가?
3. HUD snapshot migration이 깨지지 않았는가?
4. Technique Presentation event가 중복/누락 없이 동작하는가?
5. VFX lifecycle migration 뒤 잔류/중복이 없는가?
6. Animation contract migration 뒤 기존 prototype pose가 유지되는가?
7. Audio event migration 뒤 중복/누락이 없는가?
```

---

# 실행

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

Unity compile 완료 후 `CombatMVP` Play.

먼저 Console의 빨간 오류가 없는지 확인한다.

---

# A. Gojo combat

```text
[ ] 이동 / Target Lock 정상
[ ] 기본공격 1 → 2 → 3 정상
[ ] hit / combo / finisher HUD 정상
[ ] 1/2/3 pose 정상
[ ] swing / hit / finisher sound 정상
[ ] SPACE Dodge 이동/무적/pose/sound 정상
[ ] Q 창 gameplay + VFX + camera + sound 정상
[ ] E 혁 gameplay + VFX + camera + sound 정상
[ ] 창 + 혁 이후 R 허식 자 사용 가능
[ ] 허식 자: 청/적 수렴 → 큰 보라 구체 이동
[ ] 허식 자 camera/flash/shake/FOV 정상
[ ] 허식 자 VFX 중복 없음
[ ] 허식 자 sound 1회, 중복/에코 없음
[ ] V 무량공처 준비/개방/Active 정상
[ ] 무량공처 presentation + sound 1회 정상
[ ] 종료 후 technique burnout 정상
```

---

# B. Player Team / Tag

```text
[ ] Player Team HUD Active/Reserve 정상
[ ] HP / CE 값 정상
[ ] T Tag → Sukuna 정상
[ ] Tag 후 Gojo HP / CE 보존
[ ] Sukuna HUD name/color/skill deck 정상
[ ] Tag entry pulse / presentation 정상
[ ] Action 중 잘못된 Tag 없음
```

---

# C. Sukuna combat

```text
[ ] 이동 / 기본공격 / Dodge 정상
[ ] Q 해 정상
[ ] E 팔 정상
[ ] 해 + 팔 이후 R 푸가 준비 정상
[ ] 영역 밖 적 1명 조건의 푸가 정상
[ ] 푸가 anticipation/release/impact camera/VFX 정상
[ ] 푸가 sound 중복 없음
[ ] V 복마어주자 준비/개방 정상
[ ] 복마어주자 domain presentation/sound 정상
[ ] 영역 필중 pulse 정상
[ ] 영역 안 R 강화 푸가 정상
[ ] 강화 푸가 광역 피해 정상
[ ] 강화 푸가 VFX/audio 이상한 이중 재생 없음
```

---

# D. Player KO rule

```text
[ ] 첫 Player Active KO → 살아있는 Reserve로 Auto Tag
[ ] 첫 KO에 최종 DEFEAT 처리 없음
[ ] Reserve가 마지막으로 KO될 때만 DEFEAT
```

Audio도 가능하면 첫 KO와 최종 패배를 구분해 확인한다.

---

# E. Opponent Team

F2로 `TRAINING · MULTI CURSE` ↔ `TEAM BATTLE` 전환.

```text
[ ] mode strip 정상
[ ] Team Battle 우측 Active / Reserve HUD 정상
[ ] 첫 Active Curse KO
[ ] RESERVE ENTRY 표시 정상
[ ] 새 Curse가 Active row로 이동
[ ] Target Lock 자동 승계
[ ] 첫 상대 KO에 VICTORY 없음
[ ] 마지막 상대 KO에만 VICTORY
[ ] Victory sound 정상
```

---

# F. Scene reload / lifecycle

```text
[ ] F2 scene reload 뒤 HUD 정상
[ ] F2 뒤 Camera presentation 정상
[ ] F2 뒤 Spatial VFX 정상
[ ] F2 뒤 Audio 정상
[ ] 종료된 VFX가 계속 남지 않음
[ ] 같은 VFX가 두 번 생성되지 않음
[ ] 카메라가 기술 종료 후 정상 시점으로 복귀
```

---

# G. Prototype debt — 실패로 보지 않는 항목

다음은 이미 의도적으로 기록된 prototype debt다.

```text
F1 Help overlay가 일부 HUD를 가림
실제 rigged model 없음
procedural pose 사용
IMGUI HUD
prototype city silhouette
production voice/music 없음
허식 자 visual/damage timing 불일치
```

이 항목 자체만으로 Gate 4 실패 판정하지 않는다.

---

# 통과 기준

위 핵심 회귀가 정상이고 Console 핵심 오류가 없으면:

```text
Gate 4 Final Regression: USER VERIFIED
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED
```

그리고 다음 단계는:

```text
Gate 5A — Third Character Stress Test
```

Gate 5A에서는 구조가 다른 세 번째 캐릭터를 넣어 Gate 4 contract가 실제로 복붙 없이 버티는지 검증한다.
