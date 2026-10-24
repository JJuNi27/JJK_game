# PASS 3D + 공중 접촉 개선 — 체크포인트 제안

> 역사 문서: 아래는2026-10-22 당시44개 제안과 미승인 상태를 보존한 기록이다. 2026-10-25 사용자가 PASS4B.4a까지의 의존성을 포함한 통합 체크포인트·일반 push를 승인했다. 현재 범위/승인은 CURRENT_HANDOFF.md와 active/CURRENT_TASK.md를 따른다. 아래44개를 현재 전체 staging 목록으로 사용하지 않는다.

작성: 2026-10-22 / `feat/shibuya-combat-mvp`
기준 HEAD: `42833ebfec9a176056ae983096c41c1ef4d8fb24`
**준비만 완료 / staging·commit·push 별도 승인 대기.**

## 승인과 보류

사용자가 직접 플레이하여 승인한 기능은 W 어퍼컷 → Space 무피해 접촉·자연 낙하, 추격 후 공중 평타 3클릭, PASS 3D 시네마틱 발동·카메라 복귀의 세 가지다. **LOCAL USER VERIFIED — 기능 범위 한정**으로 기록한다.

캐릭터별 세부 모션, 카메라 구도·연출 품질은 아직 미흡하며 승인에서 제외한다. 자동 검사만 통과한 충돌·무적·저프레임·외부 간섭 등의 경계 및 네트워크 동작을 사용자 승인으로 확대하지 않는다.

- 기존 회귀 135 + 시네마틱 18: **단일 실행 153 PASS / 0 FAIL / 0 SKIP**.
- 신규 공중 접촉·이동: 별도 복수 실행의 최신 고유 **18 PASS**.
- 원본 XML의 정확한 이름 집합과 최신 18개 각 결과를 이번에 읽기 전용으로 재확인했다. 테스트는 재실행하지 않았다.
- 마지막 Console 상태는 error 1(Pipeline 환경 복구 요청 시간 초과 로그), warning 1(기존 font reference 경고)이다. 오류 0 또는 시각 품질 승인으로 바꾸지 않는다.

## 포함 제안 — 정확히 44개

기존 수정 25개 + 신규 19개다. 아래 목록 전체가 하나의 기능 체크포인트 제안이며 아직 index에 올리지 않았다. 기존 테스트 11개의 변경은 실행 결과 출력 경로만 분리한 것이지만, 원본 QA 보존 및 동일한 검증 재현에 필요하므로 포함한다.

### 런타임 코드 및 신규 .meta — 16개

참가자 한정 시네마틱·카메라, 유효 피격 연결, 공중 접촉·이동 소유권과 데이터 정의.

| 상태 | 정확한 경로 |
|---|---|
| 수정 | `unity/Assets/Shibuya/Scripts/ShibuyaAerialCombatDefinition.cs` |
| 수정 | `unity/Assets/Shibuya/Scripts/ShibuyaAerialCombatRuntime.cs` |
| 신규 | `unity/Assets/Shibuya/Scripts/ShibuyaCinematicCameraRuntime.cs` |
| 신규 | `unity/Assets/Shibuya/Scripts/ShibuyaCinematicCameraRuntime.cs.meta` |
| 신규 | `unity/Assets/Shibuya/Scripts/ShibuyaCinematicFighterRuntime.cs` |
| 신규 | `unity/Assets/Shibuya/Scripts/ShibuyaCinematicFighterRuntime.cs.meta` |
| 신규 | `unity/Assets/Shibuya/Scripts/ShibuyaCinematicProfile.cs` |
| 신규 | `unity/Assets/Shibuya/Scripts/ShibuyaCinematicProfile.cs.meta` |
| 신규 | `unity/Assets/Shibuya/Scripts/ShibuyaCinematicSession.cs` |
| 신규 | `unity/Assets/Shibuya/Scripts/ShibuyaCinematicSession.cs.meta` |
| 수정 | `unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs` |
| 수정 | `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs` |
| 수정 | `unity/Assets/Shibuya/Scripts/ShibuyaHitStateRuntime.cs` |
| 수정 | `unity/Assets/Shibuya/Scripts/ShibuyaMovesetDefinition.cs` |
| 수정 | `unity/Assets/Shibuya/Scripts/ShibuyaMovesetRuntime.cs` |
| 수정 | `unity/Assets/Shibuya/Scripts/ShibuyaOrbitCamera.cs` |

### Editor·테스트 및 신규 .meta — 20개

한글 Inspector, 기본 OFF Play Mode 예제, 재현 가능한 검사 및 QA 원본 보호용 출력 경로 분리.

| 상태 | 정확한 경로 |
|---|---|
| 신규 | `unity/Assets/Shibuya/Editor/ShibuyaCinematicExample.cs` |
| 신규 | `unity/Assets/Shibuya/Editor/ShibuyaCinematicExample.cs.meta` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaMovesetInspector.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass1Tests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass21Tests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass22Tests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass23Tests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass2Tests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass3ATests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass3B1Tests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass3BTests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass3CAirCountTests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass3CHoverTests.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass3CTestRunner.cs` |
| 수정 | `unity/Assets/Shibuya/Editor/ShibuyaPass3CTests.cs` |
| 신규 | `unity/Assets/Shibuya/Editor/ShibuyaPass3DAirContactTests.cs` |
| 신규 | `unity/Assets/Shibuya/Editor/ShibuyaPass3DAirContactTests.cs.meta` |
| 신규 | `unity/Assets/Shibuya/Editor/ShibuyaPass3DTests.cs` |
| 신규 | `unity/Assets/Shibuya/Editor/ShibuyaPass3DTests.cs.meta` |

### Data Asset 및 필요한 .meta — 3개

새 접촉 시간과 기본 OFF 시네마틱 예제. 기존 피해·발사·체공 값은 보존.

| 상태 | 정확한 경로 |
|---|---|
| 수정 | `unity/Assets/Shibuya/Data/Shibuya Aerial Combat.asset` |
| 신규 | `unity/Assets/Shibuya/Data/Shibuya Cinematic Example.asset` |
| 신규 | `unity/Assets/Shibuya/Data/Shibuya Cinematic Example.asset.meta` |

### 문서 — 5개

사용자 기능 승인·시각적 보류, 최신 인수인계·현재 작업, 재현/실패 이력과 체크포인트 목록.

| 상태 | 정확한 경로 |
|---|---|
| 수정 | `docs/active/CURRENT_TASK.md` |
| 수정 | `docs/CURRENT_HANDOFF.md` |
| 신규 | `docs/SHIBUYA_AIR_CONTACT_REVIEW.md` |
| 신규 | `docs/SHIBUYA_PASS3D_CHECKPOINT_PLAN.md` |
| 신규 | `docs/SHIBUYA_PASS3D_REVIEW.md` |

## 제외·보존

| 대상 | 처리와 이유 |
|---|---|
| `unity/Logs/ShibuyaPass3DQA/**` | 원래 실패 XML·중간 실행·계측·포커스 백업 포함 전부 로컬 보존, 커밋 제외 |
| `unity/Logs/ShibuyaPass3DAirContactQA/**` | 최신 153개 XML·신규 검사·프레임 trace 포함 로컬 보존, 커밋 제외 |
| `unity/Logs/ShibuyaAirContactQA/**` | 사전/최종 해시 감사·환경 복구·임시 스크립트 포함 로컬 보존, 커밋 제외 |
| 그 밖의 QA Logs·이미지·영상·캐시·임시 결과·Unity Library/Temp/UserSettings | 재생성 결과 및 개인 Editor 상태. 추가하거나 삭제하지 않음 |
| `AGENTS.md`, `docs/locked/USER_VERIFIED_SETTINGS.md` | 이번에 변경하지 않았으며 체크포인트에 새 변경으로 포함하지 않음 |
| 기존 Gojo/VFX·Scene·Prefab·Packages·ProjectSettings·기존 .meta | 이번 체크포인트에 변경 없음. 보호하고 그대로 유지 |
| LocalModels·LocalAudio·레퍼런스 등 로컬 전용 자산 | Git에 새로 추가하지 않음. 로컬 보존 |

현재 Git에 보이는 수정/신규 파일 중 제외할 무관한 dirty 파일은 발견하지 않았다. ignored QA는 목록에 보이지 않아도 명시적으로 제외한다. GitHub에는 위 문서의 결과·경로와 재현 코드가 저장될 제안이며, **로컬 원본 XML/trace까지 업로드되는 제안이 아니다**.

## 보호 확인과 제한

- 시네마틱 예제 저장 기본 OFF(`cinematicEnabled: 0`), 공중 접촉 시간 0.18초 확인.
- 이전 변경 감사에서 지상 피해 20/20/20/25, 공중 피해 12+13, 체공 상한 1.2초, Dummy HP 1000/Player HP 100, 기존 발사 설정 및 GUID 보존 확인. 이번 작업은 문서만 수정했다.
- 이번 문서 정리 전후 비문서 dirty 파일 39개와 관련 QA 파일 472개의 SHA-256을 비교해 전부 동일함을 확인했다. 코드·데이터·테스트·원본 QA 변경 0이다.
- 과거 사거리 실패의 PASS 3D 유발 여부는 INCONCLUSIVE 유지. 기능 승인으로 과거 원인을 소급 확정하지 않는다.
- 검증/승인 시점의 기록이다. 미래 커밋 직전에 현재 diff와 사용자 작업 충돌을 다시 확인해야 한다. 미저장 Unity 씬은 이번 문서 작업에서 조회·저장·폐기하지 않았다.

## 후속 승인 시 제안

하나의 체크포인트 커밋으로 위 44개만 개별 staging → staged diff 검토 → 같은 브랜치에 commit → 일반 push를 제안한다. **이 순서는 아직 실행 승인받지 않았다.** 목록이나 HEAD가 달라지면 다시 검토한다.

제안 메시지: `feat(shibuya): checkpoint finisher cinematics and aerial contact`

준비 완료 시 확인한 Git 상태: staged 0 / unstaged 25 / untracked 19, HEAD 동일. 현재 단계는 사용자 목록 검토에서 종료한다.

다음 개발 후보는 **지상 방향 대시·회피**다. 별도 범위·입력·무적 정책 승인 전에는 구현하지 않는다. 캐릭터별 모션·시네마틱 구도/연출 품질 폴리싱도 별도 보류다.
