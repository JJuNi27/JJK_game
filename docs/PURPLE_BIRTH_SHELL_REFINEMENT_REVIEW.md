# Purple Birth — shell만 최소 수정

2026-09-19 · **CODEX VALIDATED / PENDING USER VISUAL REVIEW**

사용자 지시에 따라 Outer Polish와 Birth의 compression / flash / debris / electric accent를 유지했다. 이번 runtime 변경은 `unity/Assets/Resources/VFX/PurpleFusionBirth.shader` **한 파일의 shell 수식뿐**이다. Profile, C#, 본체, Outer, canonical timing, gameplay, camera는 수정하지 않았다. 후보 toggle의 저장 상태도 기존 OFF 그대로다.

## 변경

- shell만 기존 0.28초에서 **최대 0.11초**로 단축했다. 즉 약 2.11초에 shell 출력이 0이 된다. Birth 전체 duration을 바꾸지 않았으므로 flash 및 0.28초 파편 동작은 유지된다.
- 완전한 구형 표면 대신 world-space 방향별 sector와 기존 3D noise의 교집합으로 넓은 빈 구간을 만들었다.
- 구체 접선에서 빛을 증폭하던 항을 제거하고 가장자리를 약화했다. 연속된 비눗방울 테두리가 남지 않게 했다.
- 팽창 범위는 본체 반경 기준 1.02→2.14에서 1.02→1.88로 줄이고 빠르게 감쇠한다. 새 효과·재질·renderer는 추가하지 않았다.

## 실제 화면

Side/Caster에서 기존 2.1~2.2초의 큰 반투명 dome이 사라졌다. 2.033초 전후에는 방향별로 끊어진 짧은 충격 흔적이 보이고, 2.067초에는 대부분 감쇠한다. 2.2초에는 shell이 없다. 2.0초 flash peak와 이후 electric fragments는 그대로 보인다.

기존보다 보호막 인상은 크게 줄었으며 이번 제한된 목표에 부합한다고 판단한다. 다만 팽창 자체는 여전히 구면 기반이므로 **초기 한두 프레임의 부분 곡선까지 없애지는 않았다**. 짧아진 shell은 freeze frame보다 연속 영상에서 평가해야 하며, 주된 탄생 강조는 여전히 flash와 전기 파편이다. 최종 시각 승인은 사용자에게 남긴다.

## 비교 자료

- [영상 모음](../unity/Logs/PurpleBirthShellQA/Review/index.html)
- [Side: Baseline / Outer / Outer+Birth](../unity/Logs/PurpleBirthShellQA/Review/Side_Baseline_Outer_Birth.mp4)
- [Caster: Baseline / Outer / Outer+Birth](../unity/Logs/PurpleBirthShellQA/Review/Caster_Baseline_Outer_Birth.mp4)
- [Side: 이전 shell / 수정 shell](../unity/Logs/PurpleBirthShellQA/Review/Side_Shell_Before_After.mp4)
- [Caster: 이전 shell / 수정 shell](../unity/Logs/PurpleBirthShellQA/Review/Caster_Shell_Before_After.mp4)
- [Side still](../unity/Logs/PurpleBirthShellQA/Review/Side_Shell_Comparison.jpg), [Caster still](../unity/Logs/PurpleBirthShellQA/Review/Caster_Shell_Comparison.jpg)

3열 비교는 동일 조건 0~5.83초, 30fps, 175프레임이다. Charge→fusion/birth→hold→release→early travel을 포함한다. Shell 전후 영상은 1.55~2.55초를 같은 시간으로 맞췄다. Still 열 순서: Outer only / Previous Birth / Refined Birth.

원본 렌더: `unity/Logs/PurpleFinalPolishQA/Final/20260918_150235_362/`. 기존 렌더와 QA 자료는 보존했다.

## 최소 검증

- `PurpleFinalPolishTests.FullComparison`: **1 PASS / 0 FAIL**.
- C# compile error **0**, shader error **0**. QA 프로젝트의 C# 파일은 현재 working tree와 모두 동일함을 확인했다.
- Baseline / Outer / OuterBirth 각각 Side/Caster spawn/render/dispose, owned material cleanup, camera position/FOV restore 및 관련 shader 확인을 재사용했다.
- gameplay·timing·C# 변경이 없어 이전에 통과한 gameplay regression은 반복하지 않았다. Full regression 없음.
- 작업 전 파일 hash와 비교했을 때, 문서 작성 전 변경된 기존 파일은 `PurpleFusionBirth.shader` 하나뿐이다. Flash 수식과 Birth C# source hash는 유지한다. 관련 없는 dirty 파일도 그대로다.
- 별도 실행 간 raw 비교에는 작은 캐릭터 pose 차이가 있다. 전체 화면이 픽셀 단위로 동일하다고 주장하지 않는다. 동일 실행에서 Birth OFF/ON의 hold/release/travel 지정 프레임 차이는 0이다.
- renderer/material/particle 수 증가 없음. Shell 계산은 0.11초 이후 건너뛴다. GPU ms는 측정하지 않았다.

증거: `unity/Logs/AstraCombatQA/purple-birth-shell-final-01.log`, 동일 이름 `.xml`, `unity/Logs/PurpleBirthShellQA/before_hashes.json`, `comparison_metrics.json`, `preservation_final.json`. 수정 전 shader는 `PurpleFusionBirth_before.shader`로 보관했다.

이번 변경 파일: shader 1개, 이 보고서, `CURRENT_HANDOFF.md`, `active/CURRENT_TASK.md`. QA media/script는 `unity/Logs/` local-only. Staging/commit/push/merge 및 reset/clean/restore는 실행하지 않았다.
