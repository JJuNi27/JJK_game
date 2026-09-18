# D2-R3 + OuterR2 — Production Integration 최종 보고

2026-09-18 결과 정리 · 최종 Unity 실행 2026-09-16

**LOCAL / CODEX VALIDATED / PENDING USER VISUAL REVIEW**

중단 후 `purple-integration-targeted-final.xml/.log`, `lifecycle.json`과 실제 working tree를 먼저 확인했다. 최종 targeted는 **3 PASS / 0 FAIL**, C#·shader 오류 **0**이었다. 이번 재개에서는 runtime·profile·shader·test를 추가 수정하지 않았고 Unity 재실행, 전체 regression, 새 Play Mode 캡처도 하지 않았다. 저장된 최종 RGB24를 별도 `Final/`에 인코딩하고 보고서와 보호 검증만 정리했다.

## 1. Production integration 완료 여부

**통합 구현 완료.** 기존 Hollow Purple canonical sequence에 확정된 D2-R3 Body + OuterR2를 연결했다. 새 `ProductionPurpleVisualProfile`에서 통합 사용이 켜져 있다. 시각 R&D는 종료했으며 새 색상·표면·Halo 방향을 추가하지 않았다. 최종 시각 승인은 사용자에게 남아 있다. 고정 Observer 영상은 가림 때문에 판독 불가이므로 모든 관점의 시각 QA 완료라고 주장하지 않는다.

## 2. D2-R3 Body 적용 위치

`HollowPurpleDenseBody`의 기존 fusion 말기 등장 → completed charge → travel에 적용한다. `PurpleEnergyBody.Configure(..., true)`를 canonical sequence에서만 명시적으로 호출한다. Production adapter는 확정 R3의 core/source·plasma·internal arc 계산과 shader를 사용한다. Production의 기존 visual diameter와 transform scale에 맞추고, world-space arc 가림 반경도 scale을 반영한다. Gameplay radius와 시각 반경은 합치지 않았다.

## 3. OuterR2 적용 위치

같은 charge/travel body의 소유 자식으로 Halo / outward lightning / debris / Hero fragment / local distortion을 생성한다. shader는 확정 exploration asset을 읽기 전용으로 사용하고 Production 설정은 독립 asset으로 복사했다. Body/Outer 및 Halo/Lightning/Debris/Distortion을 한국어 Inspector에서 분리할 수 있다. 기존 긴 wake와 기존 fragment/trail renderer는 통합 후보에서 중복 생성하지 않는다.

## 4. Terminal explosion 보존

초기 검사에서 공용 `PurpleEnergyBody`가 terminal compression core에도 새 후보를 적용한 문제를 발견했다. 이를 **명시적 opt-in**으로 제한한 수정이 최종 tree에 적용되어 있다. `PurpleTerminalBurst`는 여전히 인자 없는 `compression.Configure()`를 사용하므로 기존 표현을 생성한다. 파일 자체도 시작 시점과 동일하다.

최종 `ActualCombatNoHitMaxRange`와 반복 lifecycle 검사는 terminal이 존재하는 동안 활성 `ProductionPurpleVisual`이 남지 않는 것을 확인했다. 최종 6.73초 Travel 프레임도 새 분홍 body가 기존 어두운 compression core로 바뀌는 것을 보여 준다. 기존 폭발의 색·형태·타이밍은 재설계하지 않았다.

## 5. Charge 결과

기존 Blue/Red summon·fusion 약 2초, completed charge 약 3초를 유지한다. Magenta mass, fused white-out core, 어두운 틈과 chunky surface가 Production 장면에서도 읽힌다. 기존 Production보다 밝고 큰 aura가 보인다. Caster silhouette는 확인한 화면에서 남아 있지만, 구체 뒤의 적과 공간 일부는 큰 본체에 가려진다.

## 6. Release 결과

기존 impact sequence와 약 5.133초 release timing을 유지했다. charge/travel은 같은 body/outer instance와 이어지는 flow clock을 사용해 발사 시 다시 생성하지 않는다. 기존 pressure front·camera follow-through는 유지했다. 발사 순간의 강한 백색 프레임은 기존 impact 연출이다.

## 7. Travel 결과

구형 mass와 irregular aura가 유지되며, 길어진 혜성 꼬리는 추가하지 않았다. 기존 바닥 scar는 유지했다. 측면 추적 영상에서 이동과 간헐적인 파편·아크가 읽힌다. 경로의 건물에 의해 가려지는 구간이 있다. 기존 collision policy를 바꾸지 않았으므로 건물 관통/가림 문제를 이번 통합으로 해결했다고 주장하지 않는다.

## 8. Debris / velocity inheritance

새 입자는 결정적 preview clock 계산을 유지하면서 projectile 속도 반대 방향의 짧은 위치 지연을 받는다. 설정은 잔류 시간 **0.09초**, 속도 상속 **0.35**, 잔류 거리 상한 **본체 반경×0.55**다. Unity PS의 단순 world-space 방출로 바꾼 방식은 아니며, 이동 관성을 계산해 local particle 위치에 반영한다.

검사에서 정지 상태와 이동 상태의 입자 위치가 진행 반대 방향으로 달라지는 것을 확인했다. 관측 최대 잔류 **1.755m**. Release age를 함께 제한하여 발사 첫 프레임부터 긴 지연이 생기지 않는다. 본체에 완전히 접착된 위치 계산은 아니지만, 짧은 잔광의 체감 강도는 실제 화면에서 절제되어 있다.

## 9–10. First-target hit / Behind-target damage

기존 실제 CombatMVP 검사 `AstraCombatVfxTests.PurpleRealCombatCastAndInterruptedPreviewRestore`는 **1/1 PASS**였다. 첫 target HP **100→45**, 뒤 target HP **100 유지**, 첫 target 위치의 terminal 생성, camera restore를 검증했다.

이 실제 발동 검사는 terminal 적용 범위를 제한하기 **전** 실행한 결과다. 이후 피해/충돌/종료 event 코드는 수정하지 않았으며 최종 수정은 body visual 선택 범위에 한정된다. 최신 재개 요청에 따라 실제 발동 검사를 이유 없이 반복하지 않았다. 저장된 초기 first-hit terminal 이미지는 최종 terminal 시각 근거로 사용하지 않는다.

## 11. Max-range terminal

최종 `ActualCombatNoHitMaxRange`는 실제 CombatMVP의 `ActivatePurple`·production runner 경로로 무적중 발동을 진행했다. 기존 release origin + 방향×range에서 terminal 생성, 새 body/outer 비활성, 잔류 종료 후 cleanup을 확인했다. **PASS**.

## 12. Cancel / cleanup / camera restore

최종 반복 검사는 caster 비활성화 시 sequence 종료·Dispose와 material/mesh 회수를 확인했다. 기존 실제 CombatMVP + VFXLab 중단 검사는 카메라 lease·위치·회전·FOV 복구와 preview 정리를 통과했다. 카메라/Domain 일반 구조를 수정하지 않았다. `lifecycle.json` 자체가 camera나 first-hit의 전체 결과를 담는 것은 아니며 해당 내용은 별도 XML과 테스트 assertion을 근거로 한다.

## 13. Repeated cast cleanup / 성능 sanity

[lifecycle.json](../unity/Logs/PurpleProductionIntegrationQA/lifecycle.json):

| 지표 | 최종 결과 |
|---|---:|
| 반복 sequence | 5회 |
| 관측 최대 particle | 68 |
| 최대 잔류 거리 | 1.755m |
| 반복 후 소유 material 증가 | 0 |
| 반복 후 소유 mesh 증가 | 0 |
| 계측한 travel sampling 루프의 managed allocation | 0 bytes |

Charge에서 생성된 body/outer가 travel에 재사용되고, terminal/cancel 후 소유 자원 증가가 없었다. 계측 allocation 0은 해당 테스트 루프의 결과이며 전체 게임의 allocation 0을 뜻하지 않는다. GPU frame time, 다양한 기기에서의 성능, 장시간 전투 최적화까지 검증한 것은 아니다. Body raymarch와 Halo 80-step sampling 비용은 남은 성능 평가 항목이다.

## 14. Caster / Observer / 배경 가독성

- **Caster:** bright mass·core·aura와 캐릭터 silhouette가 읽힌다. 큰 charge가 전방 일부를 가린다. 발사 후 원거리에서는 파편/가는 아크가 약해진다.
- **측면 추적 외부 시점:** forced cinematic 없이 charge/travel 형상과 공간 이동을 확인할 수 있다. 약 6초 부근 건물 가림이 있다.
- **고정 Observer:** 영상은 생성/인코딩됐지만 장면 가림 때문에 Purple을 판독할 수 없다. 이 영상은 성공적인 Observer readability 근거에서 **제외**한다. 원본은 보존하며 재촬영하지 않았다.
- **Bright/Dark:** 같은 Production 장면에 일시적인 밝은/어두운 배경판을 둔 still에서 본체·negative space가 남는다. 밝은 배경에서 Halo/미세 파편 대비는 떨어진다. 이는 완성된 밝은 맵/어두운 맵 전체의 플레이 검증은 아니다.
- **Distortion:** 기존 최종 shader와 Production world radius를 연결했다. 국소 왜곡은 본체 바깥이며, 단색 배경에서는 판독이 약하다. 이번 재개에서 별도 On/Off 캡처를 반복하지 않았다.

## 15–18. 최종 targeted tests / PASS·FAIL / 오류

최종 실행 이름: **`JJKGame.EditorTools.ProductionPurpleIntegrationTests`**

| 테스트 | 결과 |
|---|---|
| `ActualCombatNoHitMaxRange` | PASS |
| `ProductionSequenceRenderReview` | PASS |
| `RepeatedLifecycleAndTravelSpace` | PASS |

**최종 3 PASS / 0 FAIL / 0 skipped · C# compile error 0 · shader error 0.**

[최종 XML](../unity/Logs/AstraCombatQA/purple-integration-targeted-final.xml), [최종 log](../unity/Logs/AstraCombatQA/purple-integration-targeted-final.log).

별도 실제 CombatMVP 검사: [purple-integration-real-first.xml](../unity/Logs/AstraCombatQA/purple-integration-real-first.xml) **1 PASS / 0 FAIL**. 초기 targeted의 terminal 오적용으로 인한 2 FAIL은 수정 후 최종 3/3으로 해소됐다. 최종 render test의 PASS는 실행/생성/정리와 shader 검사의 통과이며 모든 영상 구도의 시각 합격을 뜻하지 않는다. 이번 재개에서는 테스트를 재실행하지 않았다.

## 19. 결과 영상 / 이미지

- [최종 비교 페이지](../unity/Logs/PurpleProductionIntegrationQA/Final/comparison.html)
- [Caster Production 7초](../unity/Logs/PurpleProductionIntegrationQA/Final/Caster_Production_7s.mp4)
- [측면 Observer / Travel 추적 7초](../unity/Logs/PurpleProductionIntegrationQA/Final/Travel_Production_7s.mp4)
- [기존 Production ↔ 통합 Charge / Bright / Dark](../unity/Logs/PurpleProductionIntegrationQA/Final/Production_charge_comparison.jpg)
- [Caster 시퀀스](../unity/Logs/PurpleProductionIntegrationQA/Final/Caster_sequence.jpg), [Travel 시퀀스·기존 terminal 복귀](../unity/Logs/PurpleProductionIntegrationQA/Final/Travel_sequence.jpg)
- [최종 max-range terminal 프레임](../unity/Logs/PurpleProductionIntegrationQA/Final/MaxRange_terminal.jpg)
- [고정 Observer 판독 불가 자료](../unity/Logs/PurpleProductionIntegrationQA/Final/Observer_sequence.jpg) — 가림 때문에 시각 승인 근거에서 제외.

최종 원본: `unity/Logs/PurpleProductionIntegrationQA/Render/20260916_093155_441`. 960×540 / 30fps / 0…7초 양 끝 포함 **211프레임**. 최종 media 3개 decode 확인. 초기 원본/인코딩은 별도로 남겨두고 `Final/`을 만들었다. 영상은 실제 CombatMVP scene에서 canonical production sequence를 샘플링한 QA 렌더이며, 실제 입력·피해 coroutine 검사는 별도 Play Mode 결과다.

## 20. 수정 Production 파일 / 신규 파일 / 보호

기존 Production 파일은 **2개만 변경**:

- [PurpleEnergyBody.cs](../unity/Assets/Scripts/Player/PurpleEnergyBody.cs): charge/travel에서 명시적으로 요청할 때만 새 adapter 선택. 기본 호출은 기존 표현 유지.
- [PrototypeHollowPurplePresentationRuntime.cs](../unity/Assets/Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs): 새 조합 선택·projectile velocity 전달, 통합 후보의 중복 legacy wake/particle 생성 방지.

신규 **7개 + 각 metadata 7개**:

- [ProductionPurpleBodyRuntime.cs](../unity/Assets/Scripts/Player/ProductionPurpleBodyRuntime.cs)
- [ProductionPurpleOuterRuntime.cs](../unity/Assets/Scripts/Player/ProductionPurpleOuterRuntime.cs)
- [ProductionPurpleVisual.cs](../unity/Assets/Scripts/Player/ProductionPurpleVisual.cs)
- [ProductionPurpleVisualProfile.cs](../unity/Assets/Scripts/Player/ProductionPurpleVisualProfile.cs)
- [ProductionPurpleVisualProfile.asset](../unity/Assets/Resources/VFX/ProductionPurpleVisualProfile.asset)
- [ProductionPurpleVisualProfileEditor.cs](../unity/Assets/Editor/ProductionPurpleVisualProfileEditor.cs)
- [ProductionPurpleIntegrationTests.cs](../unity/Assets/Editor/ProductionPurpleIntegrationTests.cs)

기존 Production 표현 코드는 남아 있으며 Profile의 통합 사용을 끈 뒤 다음 시전에서 선택할 수 있다. 수정 전 Production/3차 baseline 파일 **49개**는 `unity/Logs/PurpleProductionIntegrationQA/FrozenBefore/`에 원래 상대 경로와 SHA-256으로 보존했다. 현재 실행 파일 2개에는 통합 변경이 있으므로 현재 tree 전체가 이전 baseline과 동일하다고 표현하지 않는다.

보호 snapshot **797개** 중 허용된 Production 2개와 handoff/task 문서 2개 외 **793개 동일**. 기존 exploration·OuterR2/R1 결과·Purple_UserPrototype/Scene·local art·gameplay profile·Package/ProjectSettings는 그대로다. [최종 audit](../unity/Logs/PurpleProductionIntegrationQA/final-audit.json).

## 21. 남은 리스크 Top 3

1. **Observer 검수 범위:** 고정 Observer 카메라는 가림으로 판독 불가다. 측면 추적에서는 보이지만 모든 실제 전투 시점의 가독성 승인을 대신하지 않는다.
2. **GPU 비용:** 반복 particle/material/mesh·managed allocation sanity는 통과했으나 큰 화면 면적의 body/Halo raymarch 성능은 기기별 frame-time 검증이 남아 있다.
3. **환경·거리 대비:** 밝은 배경에서 Halo와 미세 방전/잔광이 약해지고, 원거리/건물 가림에서 세부가 사라진다. 기존 terminal의 어두운 압축 표현과 밝은 새 body 사이 대비 변화도 사용자 확인 대상이다.

## 22. 현재 Production 사용 가능 여부

**로컬 Production build candidate로 사용·플레이 검수 가능하다.** 통합 코드, 최종 lifecycle과 무적중 종료 검사, 기존 실제 first-hit/camera 검사가 확보됐다. 다만 사용자 visual approval 및 위 Observer/GPU 리스크 확인 전 최종 출시 품질로 확정하지 않는다. 새 polish 없이 현재 후보를 유지한다.

## 23. Git status

`feat/gojo-blue-screen-distortion` · 기존 dirty 변경 포함 **tracked modified 20 / untracked 191** · index 비어 있음. HEAD `a9c6ccf9c09b2effdd559e83e865296e45f600b2` 유지. [전체 git status -sb](../unity/Logs/PurpleProductionIntegrationQA/final-git-status.txt).

commit / push / merge / reset / clean / restore를 실행하지 않았다. 이번 재개에서 문서와 QA 정리 외 소스 수정은 없다. **사용자 visual approval 대기.**
