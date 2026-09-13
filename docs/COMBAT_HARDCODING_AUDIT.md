# Combat hard-coding audit

기준: 2026-09-13 local working tree. `A`는 이번 패스에서 migration, `B`는 Astra/presentation 또는 전용 후속 패스에서 migration, `C`는 알고리즘/런타임 상태로 코드에 유지한다.

| 영역 | 파일 | 기존 원본/값 | 상태 | 목표/결과 | 위험도 |
|---|---|---|---|---|---|
| Character Stats | `Core/Health.cs` | `maxHealth=100` SerializeField | A | `CharacterStatsProfile`; `Health`는 현재 HP/피해/회복/사망만 실행 | 중 |
| Arena Rule | `Core/Health.cs` | `dieBelowWorld=true`, `worldDeathY=-12` | B | `ArenaRules` 후보. 캐릭터 능력치에 넣지 않음 | 중 |
| Basic Attack 공통 | `Player/BasicAttack.cs` | radius 1.6, combo 0.9, display 0.75, hit combo 1.05 | A | `BasicAttackProfile` | 중 |
| Basic Attack 단계 | 같은 파일 | 피해 12/14/24, cooldown .24/.28/.52, knockback 4.5/6/11, stun .12/.17/.38, 3타 고정 분기 | A | `BasicAttackStep[]`; 배열 길이가 타수, finisher/tag 포함 | 높음 |
| Cursed Energy | `Core/CursedEnergyController.cs` | Standard/SixEyes/Sukuna/Yuta enum switch. Gojo 100/100/12/.8/.01/1 | A | `CursedEnergyProfile`; Gojo 런타임은 `CharacterCombatDefinition`에서 주입 | 높음 |
| Legacy CE API | 같은 파일 및 Sukuna controllers | enum `ApplyProfile` 호출 | C | 이전 Scene/Sukuna 미이관 보호 fallback. Gojo 소비자는 더 이상 호출하지 않음 | 중 |
| Blue Gameplay | `Player/GojoTechniqueController.cs` | .24, 10, 4.5, .95, .10, 8, 16, .42, 3.2, 16, offset 1.8 | A | `GojoTechniqueGameplayProfile.Blue` | 높음 |
| Red Gameplay | 같은 파일/`RedTechniqueProjectile.cs` | .30, 26, 42, 1.7, 18, 23, .52, 4.5, 24, spawn 1/.9 | A | `GojoTechniqueGameplayProfile.Red`; production defaults는 legacy/preview fallback | 높음 |
| Purple Gameplay | `Player/GojoTechniqueChainController.cs` | prep 8, cooldown 10, range 48, radius 3.2, damage 55, push 34, stun 1, cost 45 | A | `GojoTechniqueGameplayProfile.Purple` | 높음 |
| Purple presentation sync | 같은 파일 | visual .85, slack .08, merge .24, launch 1.6 | B | 승인된 formation/fusion 보호. `TechniquePresentationProfile` 후보 | 높음 |
| Blue→Red synergy | 같은 파일 | mark 2.2, bonus 12, push 28, stun .72, notice 1.15; HUD `BONUS +12` | A | `BlueRedSynergyData`; HUD가 실제 `bonusDamage`를 표시 | 중 |
| Domain input/gameplay | `Player/GojoDomainController.cs` | ready 3, R→L .65, release .90±.22, fail 1.2, active 10, victim stun 16, capture 30, cost 60 | A | `DomainGameplayProfile`; state machine/restore/gate는 불변 | 매우 높음 |
| Domain visual/interior | 같은 파일/`DomainPresentationSettings` | barrier diameter/interior radius/cinematic fields | C | 이미 gameplay capture radius와 독립된 presentation data | 높음 |
| Burnout | `Core/TechniqueBurnoutController.cs` | duration 5 | A | `BurnoutPolicyProfile` (종료 후 발생/기간/조기 복구 seam) | 높음 |
| Target Lock gameplay | `Player/TargetLockController.cs` | max distance 30 | A | `TargetingProfile` | 낮음 |
| Target Lock presentation | 같은 파일 | ring radius .95, height .08, rotation 110, width .09, gold color | B | `TargetingPresentationProfile` 후보 | 낮음 |
| Training Bot gameplay | `Enemy/CurseBotController.cs` | move 3.2, rotation 10, gravity -24, engagement 1.15/.42, range 1.7, damage 12, cooldown .9, windup .55, reach .35, damping 18 | A | `TrainingBotProfile`; CombatMVP bot에 Normal profile 연결 | 중 |
| Training Bot mode | 같은 파일 | `name.Contains("_B")`로 Domain Amplification | A | 이름 검사 제거; Normal/Domain Amplification profile로 명시 | 높음 |
| Bot ordering | `Enemy/PrototypeOpponentTeamController.cs` | `_B` 이름으로 reserve 정렬 | B | 팀 슬롯/roster data로 이동. 공격 정책을 정하지는 않음 | 중 |
| Gojo traits/variant | `Player/GojoVariantController.cs` | variant enum으로 Infinity/수동 burnout 복구 결정 | B | `CharacterCombatDefinition`에 Trait/Passive seam 추가; 현 gameplay 재작성은 보류 | 높음 |
| Character dispatch | `Player/PrototypeCharacterController.cs` | character switch로 component enable/disable | C | 현재 prototype lifecycle 실행 규칙. 수치/프로필 선택은 catalog로 이동; 장기 ability loadout executor 후보 | 높음 |
| Combat Camera | `Camera/SimpleCameraFollow.cs` | offset (0,6.5,-8.5), smooth 8, look 1.4, shake ceiling .75, focus .58/22, noise 24, FOV +10/-5.5 | B | `CombatCameraProfile` 후보. Astra 카메라 소유권 안정화 뒤 이동 | 높음 |
| Combat Feedback | `Core/ProductionCombatFeedbackDirector.cs` | 기술/phase switch의 flash/shake/hit-stop/FOV/focus/color/duration | B | `TechniquePresentationProfile`의 Anticipation/Release/Impact/Aftermath | 매우 높음 |
| Scene activation | 같은 파일 및 production bootstrap 계열 | `TargetSceneName="CombatMVP"` | B | scene capability/bootstrap asset 후보 | 중 |
| Infinity presentation | `Player/GojoInfinityDefense.cs` | feedback 1.15, ripple .32/1.05, cyan/orange colors | B | Infinity presentation profile 후보 | 중 |
| Runtime timings | 여러 controller | `Time.time`, epsilon .001, geometry segment 64, smoothing 식 | C | 상태/수치가 아닌 알고리즘 내부 상수 | 낮음 |

## Runtime data flow

`Resources/CombatData/Character Combat Catalog` → `CharacterCombatDefinition` → Stats / CE / Basic Attack / Gojo Techniques / Domain / Burnout / Targeting / Movement → 각 runtime controller.

Profile이 연결된 Gojo 경로에서는 Profile이 source of truth다. Controller의 기존 serialized 수치는 Profile이 없는 이전 Scene을 위한 legacy fallback으로만 남는다.

## 보호 확인

- Walk 3 / Run 14, Gojo 평타 12/14/24 및 timing/knockback/stun, Blue 4-hit 입력값, Red production 값, Purple formation/fusion sync 값, Domain ACTIVE state machine 및 CombatActionGate를 변경하지 않았다.
- Blue/Red/Purple VFX, Unlimited Void background, audio/model/FBX/texture/blend asset은 수정하지 않았다.
- `worldDeathY`는 CharacterStats로 옮기지 않았다.
