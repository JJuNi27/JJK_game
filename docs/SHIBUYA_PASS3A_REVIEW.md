# Shibuya PASS 3A — common moveset engine

Base: `740a6ffdb8f6ef83606266a57aa2beaf128ef34e`, branch `feat/shibuya-combat-mvp`.

## USER VERIFIED checkpoint — 2026-10-18

사용자가 Unity Play Mode에서 직접 확인한 범위:
- 좌클릭 한 번으로 여러 순차 타격이 실행됨.
- 넉백으로 이동 중인 상대에게 후속 평타 무브셋으로 접근 가능.
- PASS3A 메뉴에서 기존 4타 무브셋으로 정상 복귀 가능.
- 신규 예제의 전체 동작이 의도에 맞음.

위 범위는 **USER VERIFIED**다. 충돌·무적·피격 취소·무한 경직 방지·저프레임 이벤트 순서 등 자동 검사로 확인한 경계 조건은 **CODEX VALIDATED**이며, 사용자 직접 검증으로 확대하지 않는다.

## Ownership and compatibility

- `ShibuyaFighter` remains the sole CharacterController movement owner. Health/DamageContext and existing hit presentation are reused.
- A null **캐릭터 무브셋** reference uses the existing Combat Profile four strikes through the common event scheduler. The default scene and Combat Profile asset are not migrated or overwritten.
- Optional `ShibuyaMovesetDefinition` contains input stages, ordered hit events and actual movement clips. One accepted click advances one stage, independently of event count.
- Each hit has its own strike data, dedup set, timing, damage, collision volume, stun, knockback and presentation. Skipped active windows are visited once in order. A local hit-stop defers remaining events without advancing the fighter clock.
- Explicit movesets use movement clips as their movement data owner; the legacy per-strike advance fields are hidden in their Inspector. Default four-strike advance remains unchanged.
- Actual movement clips support bounded forward displacement and last-hit-target approach. Approach uses current target position, collider clearance and bounded velocity prediction, with finite distance/time/turn. Damage requires arrival and normal hit checks. No teleport, swap or aerial pursuit is implemented.
- Knockback supports the existing velocity/damping policy and opt-in distance/time requests. Both consume attempted displacement through the same collision owner; walls reduce actual travel and do not bank movement.

## Confirmed connections — approved policy 1

- Only an accepted successor press while the previously hit defender is still stunned can reserve a confirmed connection. A later press never re-stuns a recovered defender.
- Internal events within one authored input can reserve the same finite connection after a valid hit.
- Ordinary stun and source-owned connection reservation are separate. Reservation does not grant damage, invulnerability, targeting outside the hitbox or wall penetration.
- Korean Combat Profile controls: **유효 예약 입력의 확정 연계 사용**, **확정 연계 후속 입력 허용 시간**, **한 연결의 최대 추가 구속 시간**, **한 콤보의 최대 연결 보장 시간**.
- Default successor allowance is 0.24s; the existing 0.16s input-buffer asset value remains intact. Normal 4/5/6/8/10-click-per-second rhythms have a reservation opportunity before first-hit stun expires. This describes input opportunities, not measured user hand feel.
- Missed/late input, interruption, death, battle end, target failure and finite expiry release reservation. No new C escape, guard, parry or immunity system is introduced.
- Authored gaps beyond configured caps are deliberately not guaranteed. Increasing a moveset's reach does not silently increase its lock budget.

## Opt-in examples and manual review

1. Open `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`, enter Play Mode. Default four-strike gameplay is still selected.
2. Menu **JJK Game → 시부야 PASS3A → Play Mode 멀티 히트 예제**: one click produces 7 and 11 damage, two distinct arm motions and event windows.
3. Menu **Play Mode 넉백 후 접근 예제**: first click requests a 4m victim knockback; timely second click runs a bounded approach and arrival-gated 23-damage finisher. Start within normal first-hit range. A wall, lost/escaped/invulnerable target must prevent the follow-up.
4. Menu **Play Mode 기존 4타 복귀** restores the legacy data path. Menu assignments are runtime-only. Stop Play Mode to discard them; do not save test selection into the scene.
5. New assets: `Assets/Shibuya/Data/PASS3A Multi Hit.asset` and `PASS3A Knockback Approach.asset`. Inspector exposes stage/hit arrays, timing, per-event strike data, movement bounds/arrival requirements and knockback policy. Standard SerializedProperty editing/Undo is retained.
6. Check hand feel separately: default four-click advance, directional reactions and heavy local stop; then multi-hit motion rhythm and approach facing/arrival. Automated Play Mode tests do not replace user approval.
7. Edit step/event/movement array structure outside Play Mode. For live selection use the runtime-only menu above. The default scene contains no moveset reference.

## Validation

**CODEX VALIDATED; the user-reviewed example behaviors above are USER VERIFIED.** Actual UTF discovery/execution is recorded by `ShibuyaPass3ATestRunner` under `unity/Logs/ShibuyaPass3AQA/`; process exit alone is not a PASS. The counts below describe the prior implementation validation, not a new test run during checkpoint preparation.

Initial targeted run: **11 PASS / 0 FAIL**. Initial results preserved in `targeted-initial/` before regression. Existing overwritten QA metrics/images were copied into `pre-regression-evidence/` before running old tests.

Full regression: **41 PASS / 0 FAIL / 0 SKIP**, including the original 25 tests and 16 new tests. Evidence preserved in `full-regression-41/`.

Final follow-up: a narrow correction makes explicit multi-hit movesets track the most recent applied hit target rather than retaining the stage's first victim. Legacy four-strike target selection is unchanged. The new approach example's first recovery is authored at 0.18s so its reserved follow-up fits the finite hold cap; this does not change the default character data. A two-victim regression and an uninterrupted approach-link assertion were added. Final PASS3A-only results are recorded in the root `results.xml` / `execution.jsonl` after this follow-up.

Final per-case results: **42 unique PASS / 0 unresolved FAIL / 0 SKIP** = original **25** + new **17**. This is **not one 42-case run**:
- `full-regression-41/results.xml`: 41/41 passed before the narrow explicit-moveset follow-up.
- `targeted-17-diagnostic/results.xml`: 16 passed, 1 two-victim fixture failure. The fixture inserted the second victim while the first event was still active, legitimately producing a third hit rather than the expected two.
- Root `results.xml`: the corrected two-victim fixture alone **1 PASS / 0 FAIL**. The second victim is now inserted after the first active window. No damage rule was weakened.
- `verification-summary.json` maps every case to its latest result and source XML; prior failures remain available, including the single-test diagnostic reproduction.

Final Pipeline audit: compilation failed **false**, Console errors **0**, missing scripts **0**, Time.timeScale **1**, opt-in scene references **0**. Scene, default combat profile, Dummy Stats and Player Stats are not dirty; Play Mode is stopped. Historical `Deleting invalid font reference.` Editor reload warnings occurred; they were not repaired or concealed in this PASS. The final console ground-truth sample had 0 errors/0 warnings, while the session history retained those warnings.

Measured in the completed full run:
- Unobstructed attacker forward movement: **0.30m**; independent victim distance knockback: **4m**, with reduced actual travel against a wall and no stored burst afterward.
- Approach scenario: attacker travel approximately **4.10m**. The victim's roughly **5.19m** total includes the initial 4m request and the later strong finisher; it is not the first-hit knockback alone.
- Single-input multi-hit: **7 → 11** damage, event order **0 → 1**, one accepted input; multiple victim colliders do not duplicate either event.
- Slow-frame scheduling is tested by advancing the real runtime scheduler across complete active windows in Play Mode, not by claiming a sustained low-FPS rendering benchmark.

## Changed files

Modified existing files (only these four):
- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlockVisual.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs`

New files, with Unity-generated `.meta` companions for code and assets:
- `unity/Assets/Shibuya/Scripts/ShibuyaMovesetDefinition.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaMovesetRuntime.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaMovesetInspector.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaMovesetExamples.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3ATests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3ATestRunner.cs`
- `unity/Assets/Shibuya/Data/PASS3A Multi Hit.asset`
- `unity/Assets/Shibuya/Data/PASS3A Knockback Approach.asset`
- This review document.

Comparison against the pre-work SHA-256 inventory of 1,007 existing files found only the four modifications above and no missing files. Existing Scene/Prefab/default Data Assets, locked documents and Gojo/VFX files remain untouched. Dummy HP **1000**, player HP **100**, default four-hit damage **20/20/20/25** are preserved.

## Combat Data management menu audit — 2026-10-18

사용자가 실수로 실행한 메뉴: `JJK Game → Combat Data → Create Or Repair Default Profiles`.

실제 구현은 `Assets/Editor/CombatDataMigrationUtility.cs`의 `CreateOrRepairDefaultProfiles()`다. Shibuya 예제 선택/복귀 메뉴와는 별개다.

- 출력 대상은 `Assets/Characters/Gojo/Data`, `Assets/Characters/Common/Data`, `Assets/Resources/CombatData`다. `Create<T>()`는 기존 자산이 있으면 재사용하고, 없을 때만 새 자산을 만든다.
- 단, 메뉴는 완전한 읽기 전용이 아니다. Domain Amplification 봇 모드, Gojo 정의의 식별자/표시명/프로필 참조/이동 3·14/Traits/Passives, 카탈로그의 Gojo 단일 항목을 다시 지정한다. 기존 Basic Attack steps가 비어 있으면 기본 3단계로 초기화한다.
- 마지막의 `AssetDatabase.SaveAssets()`는 당시 dirty인 다른 자산도 저장할 수 있다. 따라서 화면 메시지만으로 사용자 설정 보존을 가정하지 않았다.
- 현재 파일을 PASS3A 시작 전 1,007개 파일의 SHA-256과 다시 대조한 결과, 예상 Shibuya 코드 4개 외 변경·누락은 없었다. 기존 Gojo/공통 프로필·카탈로그·GUID·참조와 기존 Shibuya Scene/기본 Data/Packages/ProjectSettings는 byte 단위로 보존됐다.
- 새 PASS3A 예제 두 개는 현재 factory 데이터와 일치하고 유효하다. 각 GUID는 중복되지 않으며 `ShibuyaMovesetDefinition` 스크립트 참조가 정상이다. 기본 Scene의 opt-in 참조는 0이다.
- 메뉴 출력 폴더에 추가 untracked/ignored 자산이 없고, 전체 Git untracked 목록도 원래 계획한 PASS3A 파일과 일치한다. Unity 캐시/임시 파일의 개별 생성 시점은 이 메뉴의 영향으로 단정하지 않으며 커밋하지 않는다.
- Pipeline 조회에서 열린 Scene dirty=false, Play Mode=false, 로드된 persistent ScriptableObject dirty 목록은 비어 있었다. 임의 저장·폐기·복구는 수행하지 않았다.

결론: 현재 증거에서 관리 메뉴의 **예상 밖 영속 데이터 변경은 발견되지 않았다**. PASS3A 파일만 체크포인트 대상으로 삼는다. 메뉴 자체의 덮어쓰기 가능성을 없애는 리팩터링은 이번 작업에 포함하지 않는다.

## Reproducing automated validation

기존 결과를 보존한 뒤 저장된 Scene/종료된 Play Mode에서 실행한다. 메뉴 `JJK Game → 시부야 PASS3A → 콤보 및 PASS1 회귀 검증`의 현재 `Run()`은 PASS3A 17개만 선택한다.

기존 PASS1~2.3을 포함한 42개 전체 discovery/execution을 다시 수행하려면 Pipeline C# 평가에서 다음을 실행한다. 명령줄 인용 문제를 피하려면 이 snippet을 임시 `.cs` 파일로 작성해 `eval_file --file <absolute-path>`로 호출한다.

```csharp
JJKGame.Shibuya.EditorTools.ShibuyaPass3ATestRunner.RunFiltered("ShibuyaPass");
return true;
```

`discovery.json`, `execution.jsonl`, `results.xml`의 실제 테스트 수·실행·실패를 확인한다. 현재 체크포인트는 재테스트 없이 기존 XML과 사용자 검수를 기록한다. 재생성 가능한 QA Logs/이미지/캐시/임시 snippet은 Git 대상에서 제외한다.

## Limits and next PASS

- Horizontal approach only; no teleport/swap/root-motion animation authoring, upper/down branches, aerial chase, Q/E/R or multiplayer.
- Existing CharacterStatsProfile attack/defense multipliers remain unused in the damage calculation.
- The block-rig presentation adapter selects the current event's existing motion; simultaneous independent limb animations are not added.
- Highly sparse authoring is allowed to release a defender at the explicit hold cap. No input or tracking policy can guarantee a hit after legal escape, immunity or world obstruction.
- PASS 3B should follow user play review and separately specify W/S finisher selection and aerial state ownership; those features are not implemented here.

This checkpoint records the scoped PASS3A USER VERIFIED review above while preserving the PASS2–2.3 baseline. PASS3B and all other new gameplay features require a separate user request.
