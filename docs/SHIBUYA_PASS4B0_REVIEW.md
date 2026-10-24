# PASS 4B.0 — F 홀드 공통 주력 아우라

2026-10-24 / `feat/shibuya-combat-mvp` / HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`.

**LOCAL UNCOMMITTED / targeted 범위 CODEX VALIDATED / PENDING USER VISUAL REVIEW.**
F 입력으로 실제 주력 아우라가 재생된다. 시각 품질 USER VERIFIED/Production 승격은 하지 않는다. 기존 PASS 4A 미해결 18건은 별도로 남긴다.

## 첨부 레퍼런스 3장 확인과 방향 수정

처음에는 로컬 레퍼런스 폴더에서 해당 아우라 이미지를 찾지 못해 텍스트 기준으로 시작했다. 이후 사용자가 대화에 첨부한 **3장을 실제로 확인한 뒤** 구현 방향을 수정했다. 이미지를 런타임 텍스처로 복사하지 않았다.

- 첫 이미지: 주먹을 감싸는 반투명한 청록 에너지와 두껍고 굵기 불균일한 검은 경계, 세로 방향의 밝은 흐름. 몸/손은 충분히 드러난다.
- 두 번째: 몸 옆·뒤에 큰 백청색/시안 실루엣이 연결되고, 검은 외곽이 굴곡·갈라짐·떨어진 조각을 만든다. 작은 입자보다 큰 형태가 먼저 읽힌다.
- 세 번째: 청록 경계와 더 짙은 파란 내부의 깊이, 말리는 검은 끝, 캐릭터 윤곽의 푸른 빛이 중요하다.
- 정지 이미지이므로 정확한 속도·수명·회전 주기는 확인할 수 없다. 상승하며 굽고 찢어지는 모션은 형태를 바탕으로 한 제작 해석이다.

최초의 작은 독립 화염 + 독립 검은 조각은 푸른 불꽃과 검은 벌레를 붙인 모습이 될 위험이 있었다. 메인 주력과 먹물이 같은 경로/시드를 공유하도록 바꾸고, 큰 형태의 경계와 틈에서 검은색과 시안이 함께 나타나도록 했다. 이번 아우라에서는 검은 외곽 자체가 정체성이다. Purple의 과거 검은 stroke 문제를 이유로 이 특징을 제거하지 않았다.

## 렌더링 기술과 레이어

기존 PurpleTempest의 소유 Material/Mesh 해제 패턴과 전용 URP HLSL 사용 방식을 읽기 전용으로 참고했다. 기존 고죠 셰이더·머티리얼·프리팹을 참조해 수정하거나 복제 의존성을 만들지 않았다.

VFX Graph/새 패키지/Renderer Feature를 추가하지 않았다. **한 번 만드는 3D 볼륨 메시 + 전용 URP 셰이더 + 소량의 Particle System**을 선택했다. 셰이더 코드로 형태·역할을 제어하면서 기존 프로젝트의 렌더링/검증 방식과 맞출 수 있기 때문이다. 단일 billboard나 전체 캐릭터를 두르는 원통은 쓰지 않았다.

| 레이어 | 역할 및 구현 |
|---|---|
| MainAura | 큰 주력 형태 9개를 하나의 메시로 묶음. 두께 있는 닫힌 단면, 불균일한 목/팽창, 말리는 끝, GPU 국소 변형과 상승/교체. 파란 내부와 밝은 시안 명암 분리 |
| InkRuptures | 메인과 같은 씨앗·경로의 뒤쪽 hull을 별도로 렌더. 불균일한 확장과 서로 다른 erosion으로 검은 경계/틈을 만듦. 독립 검은 촉수나 일정한 링이 아님 |
| EnergyWisps | 작은 3D 흐름 9개를 별도 메시로 묶어 메인과 먹물 사이의 짧은 상승/굴곡을 보조 |
| BodyRim | 원본 메시를 공유하는 별도 Fresnel 셰이더 렌더러. 각 신체 Transform의 자식이라 기존 블록 동작을 따라감. 원본 Material/PropertyBlock은 쓰지 않음 |
| MicroParticles | 작은 밝은 입자/어두운 티끌. Local simulation, 최대 64개. 메인 에너지를 대체하지 않음 |
| 활성/해제 | 약 0.12초 상승, 0.24초 감쇠. F 해제 즉시 새 방출 중단, 잔여 표현 감쇠/정리. 큰 충격파·폭발 없음 |

BodyRim은 SkinnedMeshRenderer의 메시/bones/rootBone 공유 경로도 마련했지만, 이번 실제 검증 대상은 현재 블록 캐릭터다. 실제 고죠 스킨/애니메이션 연결을 검증한 것으로 확대하지 않는다. 전역 Bloom/Exposure/Light/카메라 및 캐릭터 원본 머티리얼은 변경하지 않았다.

## 입력·소유권·캐릭터별 설정

기존 ShibuyaPlayerInput에 `auraHeld`와 F 읽기만 추가했다. 플레이어 경로에서 공통 Controller를 연결하며 기존 씬/프리팹 연결은 변경하지 않았다.

- F 홀드 의도와 실제 시각 재생 가능 상태를 분리한다. 공격/이동/점프 입력을 아우라 때문에 거절하지 않는다.
- 기본 설정은 피격/다운/시네마틱 중 아우라를 시각적으로 억제한다. F를 계속 누르고 있으면 상태 회복 후 다시 재생한다. 사망/전투 종료/비활성에서는 즉시 정리한다.
- `SetSuppressed`와 새 홀드 시작 구분을 제공한다. 향후 W+F 대시가 우선권을 요청할 연결 지점이며 **이번에는 대시를 구현하지 않았다.** F 홀드 후 W 입력을 대시로 처리하지 않는다.
- 주력 게이지·회복/소모·버프·피해·무적·속도·전투 스탠스 변경 없음.
- 기본 청록, 핑크/초록 프리셋은 데이터다. 캐릭터 이름을 검사하는 분기 없음. 메인 색상에서 강조/윤곽/입자 색을 파생하며 사용자가 개별 보조색을 지정할 수 있다. 먹물 색은 독립 유지.
- 캐릭터마다 소유 Material 4개를 만들고 재사용한다. 같은 캐릭터 안에서는 레이어 간 공유해도 다른 캐릭터의 인스턴스와는 분리한다. 두 캐릭터 색상 변경 오염 검사 통과.

## 튜닝 위치와 사용법

1. 씬 `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 Play하고 화면을 클릭해 기존 조작을 활성화한다.
2. **F 누름/유지/해제**로 활성·루프·감쇠를 확인한다. WASD/Shift/Space/일반 체술/MMB를 함께 사용할 수 있다.
3. 기본 설정은 `Assets/Shibuya/Resources/Cursed Energy Aura.asset`. 크기/강도/높이/폭/흐름/불규칙성/페이드/각 레이어 사용·불투명도·강도/입자 밀도를 한국어 Inspector에서 조절한다.
4. 기본은 메인 색으로 보조색이 파생된다. **보조색 직접 지정**을 켜면 강조·몸 윤곽·입자 색을 따로 조절한다. 검은 먹물은 항상 개별 설정이다.
5. Play Mode의 플레이어에 있는 `CursedEnergyAuraController`의 **캐릭터 주력 아우라 설정**에 `VFX/CursedEnergyAura/Presets/Pink Aura.asset` 또는 `Green Aura.asset`을 지정할 수 있다. 실제 저장할 캐릭터 설정은 향후 해당 캐릭터 프리팹/바인딩에 이 프로필을 지정한다. 이번에 기존 프리팹을 저장하지 않았다.
6. 걷기/달리기/회전/점프, 지상 콤보, W 업→Space 추격→공중 3클릭, 피격 후 복구, F 해제 뒤 잔존을 직접 확인한다.

생성 메뉴는 `JJK Game → 시부야 PASS4B → 기본 주력 아우라 자산 만들기 (기존 값 보존)`. 이미 실행해 필요한 자산은 생성됐으며, 일반 플레이 전에 메뉴를 다시 실행할 필요는 없다. 생성기는 기존 자산을 덮지 않는다. 임시 소유 씬에서 프리팹을 작성하고 원래 저장된 씬으로 복귀한다.

## 변경 파일

이전 로컬 파일 중 추가 수정은 **2개뿐**이다.

- `unity/Assets/Shibuya/Scripts/ShibuyaPlayerInput.cs`: F 시각 입력 전달/종료 정리.
- `unity/Assets/Shibuya/Editor/ShibuyaPass3CTestRunner.cs`: 기존 안전 출력 경로 허용 목록에 `Logs/ShibuyaPass4B`만 추가. 과거 경로/기준/계측은 유지.

신규 코드와 필요한 `.meta`:

- `Scripts/CursedEnergyAuraProfile.cs`, `CursedEnergyAuraController.cs`, `CursedEnergyAuraVisual.cs`, `CursedEnergyAuraGeometry.cs`.
- `Editor/CursedEnergyAuraInspector.cs`, `CursedEnergyAuraAssets.cs`, `ShibuyaPass4B0Tests.cs`.

위 코드의 공통 루트는 `unity/Assets/Shibuya/`다. 신규 자산:

- `Resources/Cursed Energy Aura.asset` 및 `.meta`.
- `VFX/CursedEnergyAura/Cursed Energy Aura.prefab` 및 `.meta`.
- 같은 폴더의 `Energy.mat`, `Ink.mat`, `BodyRim.mat`, `Motes.mat` 및 `.meta`.
- `CursedEnergyAuraVolume.shader`, `CursedEnergyAuraRim.shader`, `CursedEnergyAuraMote.shader` 및 `.meta`.
- `Presets/Pink Aura.asset`, `Presets/Green Aura.asset` 및 `.meta`, 새 폴더 `.meta`, 이 보고서.

시작 스냅샷 183개 중 위 기존 2개만 변경, 나머지 181개 동일, 누락 0. 기존 전투 런타임·Data Asset·Scene·Prefab·GUID·Packages/ProjectSettings·고죠 VFX·USER VERIFIED 잠금값·기존 미커밋 작업을 보존했다. `preservation-check.json` 참조.

## 실제 시각 QA

최종 이미지 경로:

`unity/Logs/ShibuyaPass4B0QA/20261024-034500/render-and-movement-check/legacy/ShibuyaPass4B0QA/`

`00-no-F.png`, `01-activation.png`, `02-front-loop.png`, `03-side.png`, `04-back.png`, `05-high-angle.png`, `06-pink.png`, `07-green.png`, `08-released.png`.

Unity Play Mode에서 실제 카메라를 URP RenderRequest로 960×720 렌더했다. 바탕은 기존 밝은 시부야 블록아웃이며 조명/카메라 시스템의 저장값은 변경하지 않았다. 화면 캡처를 보지 않고 셰이더 파일 존재만으로 시각 검증했다고 한 것이 아니다. 정면·측면·하이 앵글·핑크 이미지를 직접 확인했다. OFF/해제 PNG는 SHA-256 동일하다.

최초 정상 렌더에서는 유리 잎/매끈한 표면과 회색 먹물 끝이 강했다. 불균일한 목과 말리는 끝, 국소 윤곽 변형, 더 선명한 먹물, 파란 내부 명암을 보완했다. 시각 체감의 최종 승인은 사용자가 한다.

## targeted 결과와 실패 원본

QA 루트: `unity/Logs/ShibuyaPass4B0QA/20261024-034500/`.

| 별도 실행 | PASS / FAIL / SKIP |
|---|---:|
| `visual-pilot/results.xml` | 1 / 1 / 0 |
| `visual-after-init-fix/results.xml` | 0 / 1 / 0 |
| `visual-after-mesh-fix/results.xml` | 0 / 1 / 0 |
| `visual-after-particle-fix/results.xml` | 1 / 0 / 0 |
| `final-targeted/results.xml` | 11 / 1 / 0 |
| `render-and-movement-check/results.xml` | 4 / 0 / 0 |

**21회 실행, 최신 고유 12케이스 = 12 PASS / 0 FAIL / 0 SKIP.** 새 아우라 8개 + 직접 영향받는 기존 4개다. 단일 12개 전체 성공 실행으로 표현하지 않는다. discovery/execution/XML/계측/최신 집계는 `validation-summary.json`에 보존했다. 전체 190개 재실행 없음.

최초 오류와 수정:

- MonoBehaviour 필드 초기화에서 native MaterialPropertyBlock을 만들던 오류 → 런타임 Bind 시점으로 이동.
- 메시 끝점 sin(π)의 작은 음수에 분수 지수를 적용해 NaN/AABB 오류 → 음수 오차를 0으로 clamp. 모든 생성 좌표의 유한성 검사 추가.
- Particle Velocity의 축별 커브 모드 불일치 → XYZ를 같은 TwoConstants 모드로 설정.
- 이동 검사에서 이미 더미 캡슐에 막힌 플레이어에게 추가 이동을 요구 → 열린 공간의 이동/회전/점프로 관측 fixture 수정. 실제 충돌·이동 값은 변경하지 않았다. 실패 당시 소스도 `final-targeted/test-source-before-clear-space-movement.txt`로 보존.
- 최초 QA 시작은 기존 기록기의 경로 허용 목록에 PASS4B가 없어 거부됐다. 테스트를 실행한 것으로 집계하지 않는다. 안전한 새 접두사만 추가했다.

이들은 서로 다른 실제 구현/검사 결함이었으며 환경 지연으로 돌리거나 LogAssert.Expect로 숨기지 않았다. 원본 실패 기록/이미지는 삭제하지 않았다.

통과 범위: F 활성·유지·해제, 반복 및 자원 재사용, 이동·회전·점프 부착, F 홀드 중 지상 피해 85, W 업→Space→공중 3클릭 5타/피해 62, 피격 억제 후 F 유지 복구, 사망/비활성 정리, 색상 오염 없음, 인스턴스 파괴 cleanup, 한국어 Inspector/Undo/셰이더. 기존 PASS 3C.1 실제 높이 유지, PASS 3D 시네마틱 복귀, PASS 4A.1 반대 시점 공격, PASS 4A.2.1 최고 Pitch 화면 복구도 통과했다.

## 성능·cleanup·Console

- F 토글 30회 + 12초 홀드: 캐릭터 하위 Transform 26개, 소유 Material 4개 / Mesh 3개로 동일. 최종 geometry 6,966 vertices. 미세 입자는 측정 시 7개, 시스템 상한 64개.
- 매 프레임 CPU 메시 재생성 없음. 역할별 메시를 최초 1회 만들고 GPU에서 변형한다. 이미지/텍스처/렌더 타깃을 런타임 아우라가 생성하지 않는다(QA 캡처만 임시 RT 사용).
- F OFF에서는 재사용할 4/3 자원을 의도적으로 보유하지만 Renderer/입자는 꺼진다. 인스턴스 파괴 후에는 소유 자원 반환을 검사했고, 최종 Play Mode 종료 후 전용 Material/Mesh **0/0** 확인.
- 최종 C# compile error 0 / warning 0, 셰이더 오류 검사 통과. 최종 현재 Console **error 0 / warning 1**(기존 font reference). 누적 버퍼 error 25 / warning 45에는 이전 개발 오류와 실패 이력이 남아 있다. 로그를 지우지 않았다.
- Pipeline 정상, Play Mode 종료, 실행 중 QA 없음, 미저장 씬 없음. 주요 신규/기존 설정 자산 4개 dirty=false 확인.
- AutoTick/프레임률/vSync/Play Unfocused 설정 변경 없음. 최종 targetFrameRate -1 / vSync 0 / Time.timeScale 1 / AutoTick SessionState 저장값 -1. Computer Use·포커스 강제 전환·패키지 변경·Git staging/commit/push 없음.
- 최종 Git: 동일 브랜치/HEAD, staged 0 / tracked unstaged 28 / untracked 91. 기존 여러 PASS의 미커밋 작업을 포함한 전체 수다. `git diff --check` 오류 없음(기존 LF→CRLF 안내만 있음). 이번 신규 Shibuya 파일 목록은 QA의 `new-files.json`에도 기록했다.

## 남은 시각적 한계

청록+파란 내부, 검은 경계, 신체 주변 깊이와 색상 교체는 읽힌다. 다만 레퍼런스처럼 넓게 하나로 이어지는 잉크/화염 실루엣에는 아직 차이가 있다. 일부 각도·프레임에서는 반복되는 덩어리/잎 형태나 검은 조각의 면이 보이고, 전경 에너지가 블록 캐릭터 팔/상체와 겹친다. 레퍼런스의 손 주변 압축된 주력과 섬세한 천/피부 반사광을 완성했다고 하지 않는다.

현재 모델은 블록형이며 실제 스킨·캐릭터별 부위 바인딩, 원거리 LOD, 다수 캐릭터 GPU overdraw/성능은 별도 검증이 필요하다. 12초 무누적 검사는 GPU 성능 벤치마크가 아니다. 정지 캡처만으로 모션 속도/손맛을 USER VERIFIED로 판단하지 않으며, 사용자 Play Mode에서 먼저 검수받는다.

## 2026-10-24 — 로컬 주력 영상 3종을 기준으로 한 실루엣 개선

**LOCAL / CODEX VALIDATED(기술 검증) / PENDING USER VISUAL REVIEW.** 아래는 후속 수정 기록이며 위의 최초 실행/실패 이력을 대체하지 않는다.

### 직접 확인한 레퍼런스와 해석 범위

`_local_refs/videos/주력이펙트.mp4`(30fps, 10.233초), `다양한 주력이펙트.mp4`(30fps, 3.4초), `주력이펙트 전투.mp4`(30fps, 19.167초)를 디코딩했다. 각 영상의 전체 구간 12개 표본과 8개 인접 프레임을 직접 이미지로 확인했다. 사람처럼 연속 재생을 시청했다는 뜻은 아니다. 추출 시점/프레임 번호/원본 메타데이터는 새 QA 경로에 보존했다.

- 첫 영상: 압축된 하단에서 위로 솟는 에너지, 긴 끝과 찢긴 검은 경계. 중심의 큰 형태는 유지하면서 약 33~233ms 인접 구간에서 윤곽과 내부 세로 흐름이 바뀐다. 매 프레임 전체 형상이 무작위로 교체되는 현상으로 해석하지 않았다.
- 두 번째: 단일 불꽃과 평면 소재 시연이 함께 있다. 가장자리 erosion/명암은 참고하되, 시연용 판의 배치를 캐릭터 오라 구조로 복사하지 않는다.
- 전투 영상: 이동/충돌 시 진행 방향으로 길어지는 흐름, 검은 경계와 색 에너지의 결합을 확인했다. 투사체 이동·충돌 폭발은 이번 F 홀드 아우라에 새 기능으로 추가하지 않았다.
- 앞서 제공된 애니 이미지 3장의 몸/손 주변 배치와 이번 소재 영상의 형태·움직임을 결합해 해석했다. 원본 제작자의 메시/셰이더 내부 구조를 확인한 것은 아니다.

### 원인과 유지/변경

기존 표현은 실제로 네 장의 billboard가 아니라 닫힌 메시 9개였다. 그러나 고정된 둘레, 넓은 옆면, 둥근 중간 단면과 긴 노출 시간이 겹쳐 벽처럼 읽혔다. 파티클 수를 늘리거나 카메라-facing 판을 추가해서 해결할 문제가 아니었다.

1. **메인 형상 변경:** 몸 주변 반경/발생 높이를 분산하고, 넓은 잎 단면을 압축된 뿌리와 길고 가늘어지는 꼬리로 변경. 단면을 길이 방향으로 비틀며 거의 원형의 두께를 유지한다. 메인 길이는 1.4~2.05m 범위의 생성 형상(프로필 높이 배율 적용 전)이며, 폭은 기존 .25~.4에서 .15~.23 범위로 바꿨다. 전체 프로필 크기·강도 값은 변경하지 않았다.
2. **경로 변경:** 같은 위치에 선 벽 전체를 돌리는 대신 불꽃이 보이지 않는 재생 경계에서 개별 경로 방향과 높이를 바꾼다. 꼬리의 굽힘과 위로 이류되는 밀도는 연속 진행한다. 카메라 방향을 사용하지 않는다.
3. **먹물 동기화 결함 수정:** 기존 geometry는 위치 seed는 같지만 수명 phase seed에 `role`을 사용해 에너지/먹물이 따로 상승했다. 동일 `family`로 통일했다. 자동 검사는 두 메시의 정점과 phase UV 일치를 확인한다.
4. **동일 표면의 먹물:** 공유 density/erosion 안에서 암부와 청록이 침범한다. 분리된 검은 후면 hull은 매우 얇고 grazing 경계만 보이도록 제한했다. 전역 ZWrite 변경이나 transparent architecture 교체는 없다. 실제 volume raymarch도 아니다.
5. **유지:** F 제어/전환/억제/풀링, 색상 프로필 3종, 한국어 Inspector, 몸 림, 미세 입자와 상한. 메인·먹물·wisps 3메시/소유 4머티리얼 구조와 CPU 메시 최초 1회 생성도 유지했다. 입력·전투·락온·공중전 코드는 이번 패스에서 수정하지 않았다.

수정 파일은 `CursedEnergyAuraGeometry.cs`, `CursedEnergyAuraVisual.cs`, `CursedEnergyAuraVolume.shader`, `ShibuyaPass4B0Tests.cs`, 이 보고서 5개다. 프로필/프리팹/씬/머티리얼 에셋/GUID는 변경하지 않았다. 폭·높이·속도·불규칙성 등 기존 한국어 튜닝은 계속 사용한다. 메시 세그먼트 수는 내부 topology mechanics, 구체적인 꼬리/이류 계수는 presentation tuning으로 향후 필요 시 데이터로 분리할 대상이며 이번에는 새 gameplay 하드코딩을 도입하지 않았다.

### 검증과 시각적 한계

QA: `unity/Logs/ShibuyaPass4BVisualQA/20261024-reference-polish/`.

- `visual-first/results.xml`: 별도 초기 시각 검사 **2 PASS / 0 FAIL**. 이 캡처를 보고 남아 있던 통통한 중간 단면과 유리 잎 인상을 한 번 더 보완했다.
- `final-targeted/results.xml`: 최종 코드 **단일 실행 9 PASS / 0 FAIL / 0 SKIP**. 이전 12개 결과와 합산해 전체 회귀를 수행했다고 표현하지 않는다. 전체 190개 재실행 없음.
- F 홀드/해제/피격 억제/사망, 실제 이동·점프·회전, 지상 피해85, 업→추격→공중5타 피해62, 프로필 인스턴스 분리, 셰이더 오류·한국어 필드·Undo, 토글30회+12초 홀드 무누적/파괴 cleanup, 청록/먹물 경로 일치 및 회전 캡처 검사 포함.
- 원속도 캡처: 고정 측면 약2초 후 약4초에 걸친 360도 회전. 173개 실제 렌더 프레임과 약5.968초 타임스탬프를 보존했다. MP4는 실제 프레임 간격으로 인코딩한 30fps 영상이며 일부 프레임을 복제한다(최대 측정 간격 약66.7ms). 모든 순간이 네이티브30fps로 샘플링됐다고 주장하지 않는다.
- 비교 이미지 `before-after-views.jpg`는 이전/현재 별도 실행의 정면·측면·상부다. 동일 시각/픽셀 일치 비교가 아니다. `final-adjacent-frames.jpg`는 현재 측면 인접 프레임, `final-orbit-sheet.jpg`는 현재 회전 구도 표본이다.
- 최종 C# compile error/warning 0/0, 셰이더 오류 검사 PASS. 현재 Console error0/warning1(기존 invalid font reference). 누적 보존 버퍼 error25/warning57에는 이전 개발 오류가 남아 있으며 삭제하지 않았다.
- Play Mode 종료 후 전용 Material/Mesh 0/0, dirty ScriptableObject 없음, Shibuya 씬 dirty=false. fps -1/vSync0/scale1/AutoTick SessionState -1 및 Play Unfocused2 유지. 이번 작업에서 설정값을 변경하지 않았다.

코덱스 시각 판단: 넓은 벽의 옆면과 떨어진 검은 조각이 줄었고, 긴 꼬리와 먹물의 동반 이동은 개선됐다. **레퍼런스 수준의 하나로 연결된 혼령 화염을 완성했다고 판단하지 않는다.** 일부 각도에는 개별 불꽃 군집, 매끈한 중간 면과 구멍이 난 잎 같은 인상이 남는다. 실제 캐릭터의 손/몸에 부위별로 바인딩한 아우라, 넓은 장면에서의 가독성/다인전 GPU 비용은 미검증이다. 사용자 Play Mode에서 F를 유지하며 360도 회전·이동·점프 후 해제를 검수해야 한다. 추가 기능·자동 후속 polish·USER VERIFIED 선언·commit/push 없이 정지한다.

## 2026-10-24 — 추가 피드백: 몸 기반 방출과 먹물 대비

**LOCAL / CODEX VALIDATED(기술) / PENDING USER VISUAL REVIEW.** 위의 주변 반경 기반 버전을 보완하는 후속 기록이다.

### 원인과 변경

이전 루트는 바디를 조회하지 않는 고정 주변 반경이었다. 셰이더의 수명별 상승 이동도 불꽃 뿌리까지 들어 올렸다. 따라서 닫힌 3D 메시여도 몸 밖의 독립 불꽃 벽처럼 보일 수 있었다.

- 원본 바디 Renderer의 local bounds를 합쳐 몸 영역을 구하고, 허리·가슴·등·양 어깨·양 팔·골반의 8개 목표점을 실제 Renderer bounds 내부로 투영한다. 원본 mesh/transform/머티리얼은 수정하지 않는다. 원본 렌더러가 없을 때만 CharacterController bounds로 fallback한다. 캐릭터 이름으로 규칙을 선택하지 않는다.
- 원본 바디 Transform과 로컬 점을 보관해 포즈를 따라간다. 시각 리그의 LateUpdate 이후 아우라 attachment만 갱신한다. 실제 전투 이동·충돌·포즈 소유권은 그대로다.
- 메인은 둘레의 독립 불꽃 9개에서 몸에 붙어 겹치는 굵은 흐름 5개로 변경했다. 별도 새 효과 타입을 추가하지 않고 기존 wisps는 팔/몸 부위의 작은 꼬리로 사용한다. 메인 흐름의 기반은 유지되고 디테일 꼬리만 완전히 생성/소멸한다. 메인의 겹치는 부분은 같은 body-space density field를 사용한다.
- 전체 불꽃을 위로 옮기지 않고 꼬리 길이 방향으로만 상승/굽힘을 적용한다. 프로필의 크기/폭/높이는 방출점 이후의 흐름에 적용하여 크기를 키워도 뿌리가 몸에서 떨어지지 않는다.
- 먹물 mask 기여를 약10% 늘리고 grazing 전환 범위를 조금 넓혔다. 색 에셋값이나 hull 팽창 폭을 늘리는 방식이 아니다. 청록과 같은 geometry/phase/erosion을 계속 공유한다.
- F 제어, 청록/핑크/초록 프로필, 한국어 Inspector, 기존 프리팹/씬/머티리얼 에셋과 GUID, 전투/락온/공중전 코드는 이번에 변경하지 않았다. 기존 폭·높이·흐름 속도·불규칙성·먹물 강도 튜닝을 유지한다. 부위 목표 비율은 현재 presentation 내부 상수이며 추후 캐릭터별 명시적 bone/anchor 설정을 만들 경우 데이터로 분리할 수 있다.

### 검증과 증거

QA 루트: `unity/Logs/ShibuyaPass4BBodyQA/20261024-body-emission/`.

- `targeted/results.xml`: **9 PASS / 1 FAIL / 0 SKIP**. 새 바디 검사는 위치/부착 assertion 이후 Vector3를 일반 JSON으로 기록하다 `normalized` 순환 참조가 발생해 실패했다. 런타임 실패로 보거나 무시하지 않았고 원본 XML·검사 소스·실행 로그를 보존했다.
- 기록을 x/y/z 숫자로 수정하고, 프로필 교체를 통해 실제 폭 배율까지 적용됐는지 검사를 보완했다. `body-record-recheck/results.xml`: **1 PASS / 0 FAIL**. 런타임을 변경하거나 테스트 허용 오차를 완화하지 않았다.
- **복수 실행의 최신 고유 10개 = 10 PASS / 0 FAIL / 0 SKIP.** 단일 10개 전체 성공 실행은 아니다. 기존 190개 전체 회귀는 재실행하지 않았다.
- 8개 방출점 모두 실제 바디 Renderer bounds 내부 확인. 테스트용 바디 부위를 0.2m 옮겼을 때 방출점도 약 **0.200000018m** 이동했다. 전체 크기1.5배와 폭1.4배가 실제 적용된 후에도 해당 방출점의 월드 위치 오차는0.005m 미만이었다.
- F 홀드 중 지상4타 피해85, 업→추격→공중5타 피해62, 이동/점프/회전, 피격 억제/복구/사망, 팔레트/인스턴스 분리, 한국어 필드/Undo/셰이더 오류, 반복/cleanup 검사 통과.
- 토글30회+12초 유지: Transform26 / Material4 / Mesh3 유지. 최종 정점4,390개, 측정 입자8개(상한64). Play 종료 후 소유 Material/Mesh0/0, dirty ScriptableObject 없음, 씬 dirty=false.
- C# compile error/warning0/0, 셰이더 오류 검사PASS. 최종 현재 Console error0/warning0. 누적 보존 버퍼 error25/warning69에는 이전 개발 오류와 기존 invalid-font 경고가 남아 있다. 원본 로그를 지우지 않았다.
- fps -1 / vSync0 / scale1 / AutoTick SessionState -1 / Play Unfocused2 불변. Computer Use나 포커스 전환 없음. 사용자 Play Mode가 종료됐다는 응답 및 실제 stopped/scene clean 확인 후 소스 수정을 시작했다.

`body-aura-original-speed.mp4`: 측면 고정 약2초+360도 회전 약4초. 실제 174프레임, 약5.976초 타임스탬프로30fps 인코딩. 최대 샘플 간격 약62.7ms로 일부 출력 프레임은 복제된다. `body-before-after.jpg`는 별도 실행의 이전/현재 정면·측면·상부 비교이며 동일 시점 픽셀 비교가 아니다. `body-orbit-sheet.jpg`는 현재 회전 표본이다.

코덱스 시각 판단: 몸 밖 빈 공간에서 시작하는 인상은 줄었고, 몸에 붙은 큰 흐름과 작은 꼬리의 구분이 생겼다. 현재 블록 모델에서는 상체·어깨 쪽 화염 비중이 높고 일부 넓은 면/갈라진 흐름은 남는다. 완전히 용접된 단일 유체 메시나 실제 volumetric rendering은 아니다. 스킨 모델의 bone/표면 변형 추종, 장비를 가진 모델의 bounds 선별, 다인전 GPU 비용은 이번 검사 범위 밖이다. F 홀드 중 이동/공격하며 몸에서 나오는지와 검은 경계의 강도를 사용자가 직접 검수해야 한다.

변경 파일은 geometry/visual/volume shader/아우라 검사/이 보고서의 5개다. 기존 사용자 미커밋 작업은 보존하며 staging/commit/push와 USER VERIFIED 선언 없이 멈춘다.
