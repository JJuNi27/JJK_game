# 전투 오디오 설정

## 현재 상태

`PrototypeCombatAudio`는 외부 음원 파일이 없어도 전투가 완전히 무음이 되지 않도록 간단한 런타임 합성 효과음을 생성합니다.

현재 실제 전투 이벤트에 연결된 소리:

- Q 술식순전 「창」 시전과 첫 적중
- E 술식반전 「혁」 시전과 첫 적중
- R 허식 「자」 발동
- V 무량공처 성공 발동
- 스쿠나 V 복마어주자 전개
- 복마어주자 안 푸가 광역 폭발
- 기본 공격 1·2·3타 휘두름과 적중
- 플레이어 피격
- SPACE 회피
- 승리와 패배

고죠용 영역 음성과 스쿠나용 영역 음성은 분리되어 있습니다.

```text
PrototypeCombatAudio
- 고죠 창/혁/자/무량공처와 공통 전투 SFX

SukunaCombatAudio
- 스쿠나 복마어주자 음성/SFX
- 복마어주자 안 푸가 광역 폭발 SFX
```

따라서 `Gojo_Domain`을 넣어도 스쿠나 복마어주자에서 고죠 대사가 재생되지 않습니다.

전투 사운드 이벤트는 카메라 흔들림과 화면 플래시에도 함께 연결되어 있습니다. 회전 카메라는 사용하지 않고 위치 흔들림만 짧게 적용해 멀미 가능성을 낮춥니다.

## 개인 로컬 BGM·대사·효과음 덮어쓰기

아래 폴더는 `.gitignore`에 포함되어 GitHub에 올라가지 않습니다.

```text
unity/Assets/Resources/LocalAudio/
```

폴더가 없다면 직접 생성하고 아래 이름으로 오디오 파일을 넣습니다. Unity가 지원하는 `wav`, `ogg`, `mp3` 등을 사용할 수 있습니다.

```text
BGM.wav 또는 BGM.mp3

Gojo_Blue.wav
Gojo_Red.wav
Gojo_Purple.wav
Gojo_Domain.wav

Sukuna_Domain.wav
Sukuna_DomainSFX.wav
Sukuna_DomainFuga.wav

BasicSwing.wav
BasicHit.wav
BasicFinisher.wav
PlayerHit.wav
Dodge.wav
Victory.wav
Defeat.wav
```

확장자를 제외한 파일 이름이 정확히 일치해야 합니다.

- `BGM`: 전투 시작 시 반복 재생
- `Gojo_Blue`: Q 사용 시 대사
- `Gojo_Red`: E 사용 시 대사
- `Gojo_Purple`: 허식 자가 실제 발동할 때 대사
- `Gojo_Domain`: 무량공처 입력에 성공해 영역이 실제 전개될 때 대사
- `Sukuna_Domain`: 복마어주자가 실제 전개될 때 스쿠나 대사
- `Sukuna_DomainSFX`: 복마어주자 전개 효과음
- `Sukuna_DomainFuga`: 복마어주자 안 푸가가 영역 전체 폭발로 전환될 때 효과음
- `BasicSwing`: 기본 공격을 휘두를 때
- `BasicHit`: 기본 공격 1·2타와 3타의 공통 타격층
- `BasicFinisher`: 3타에만 추가로 겹치는 강한 마무리 충격음
- `PlayerHit`: 플레이어가 피해를 입었을 때
- `Dodge`: 회피가 시작될 때
- `Victory`: 모든 적을 쓰러뜨렸을 때
- `Defeat`: 플레이어가 쓰러졌을 때

로컬 음원이 없으면 각각의 런타임 합성 fallback 효과음을 사용합니다.

## 저장소 원칙

- 저작권이 있는 원본 BGM·성우 음성 파일은 저장소에 커밋하지 않습니다.
- 개인 테스트용 파일은 `LocalAudio`에만 둡니다.
- 배포 가능한 빌드를 만들 때는 직접 제작하거나 정식 사용 허가를 받은 음원으로 교체합니다.
- 코드와 씬은 오디오 파일이 없어도 오류 없이 실행되어야 합니다.
