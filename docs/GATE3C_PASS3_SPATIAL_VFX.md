# Gate 3C Pass 3 — Spatial Signature VFX

작성 기준일: 2026-08-22

## 상태

```text
Gate 3C Pass 1 · Signature Activation Feedback: USER VERIFIED
Gate 3C Pass 2A · Anticipation + Release/Impact Timing: USER FEEDBACK · TOO SUBTLE
Gate 3C Pass 2B · Readability Tuning + FOV Kick: USER VERIFIED
Gate 3C Pass 3 · Spatial Signature VFX: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Pass 2B 사용자 확인

2026-08-22 사용자 실제 Unity 테스트에서 FOV Kick과 강화된 화면 피드백에 대해:

```text
오 확실히 표가 난다 화면 이펙트
```

라고 확인했다.

따라서 Pass 2B는 USER VERIFIED로 기록한다.

## Pass 3 목적

화면 전체 Flash/FOV만으로 끝내지 않고 술식이 실제 월드 공간을 점유하는 느낌을 추가한다.

새 파일:

```text
unity/Assets/Scripts/Player/PrototypeSignatureSpatialVfx.cs
unity/Assets/Scripts/Player/PrototypeSignatureSpatialVfxController.cs
```

둘 다 최종 자산이 아닌 Beauty Corner placeholder다.

## 현재 공간 VFX

```text
무량공처 준비
→ 고죠 주변 청색 이중 에너지 링 확장

복마어주자 Casting
→ 스쿠나 주변 적색 이중 에너지 링 확장

허식 자 Release
→ 캐릭터 전방에서 보라색 공간 Burst

푸가 Release
→ Projectile 생성 위치에서 주황/적색 Burst

푸가 Explosion
→ 실제 폭발 위치에서 더 큰 공간 Ring Burst
→ 복마어주자 내부 증폭 푸가는 더 크게 표시

무량공처 Active
→ 캐릭터 중심에서 큰 청색 개방 Ring

복마어주자 Active
→ DomainCenter 중심에서 큰 적색 개방 Ring
```

Ring과 Light는 `Time.unscaledTime` 기준으로 움직여 짧은 Hit Stop 중에도 연출이 읽히도록 했다.

## 바꾸지 않은 것

```text
Damage
CE Cost
Cooldown
Range
Cast Time
Domain Duration
Sure Hit
푸가 준비 조건
허식 자 준비 조건
Tag
Target Lock
```

## 빠른 사용자 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류 확인

3. 고죠 V
→ 캐릭터 주변에서 청색 링이 실제 월드 공간으로 퍼지는지

4. 무량공처 성공
→ Active 순간 더 큰 청색 개방 링

5. 허식 자
→ 발사 시작점 주변에 보라 공간 Burst

6. T → 스쿠나
7. 복마어주자 V
→ Casting 때 적색 링
→ Active 때 큰 적색 개방 링

8. 푸가
→ 발사 위치 Burst
→ 실제 폭발 위치에 큰 Ring Burst
```

성공 기준은 화면 Tint만 보이는 것이 아니라 `어디서 힘이 모이고 어디서 터졌는지`가 월드 공간에서 읽히는 것이다.
