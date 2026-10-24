# PASS 4B.4a — 승인 아우라의 일반 F 기본 적용

> 2026-10-25 사용자 최종 확인: **아우라 시각 방향 / QA 메뉴 없는 일반 F 최신 출력 / F 충전 동작은 USER VERIFIED**. 사용자가 현재 시부야 구현의 Git 체크포인트·일반 push를 승인했다. 아래는 이 승인 이전의 기술 검증 기록이며 테스트 세부를 USER VERIFIED로 확대하지 않는다. 저장 곡선100→1.0 /150→1.5 /200→2.0 유지, 4.0 최종 채택 없음, 신체 강화 사용자 결정으로 보류.

2026-10-25 KST. **LOCAL / CODEX VALIDATED / USER VERIFIED 대기(일반 F 적용 검수)**.
브랜치 `feat/shibuya-combat-mvp`, HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24` 유지.
기존 4B.3b/3c 시각 구성에 대한 사용자 승인을 구현 기준으로 삼았다. 새 크기 배율의 최종 승인을 의미하지 않는다.

## 1. 실제 원인

`ShibuyaPlayerInput.Start`가 생성하는 `CursedEnergyAuraController.Awake`는 `Resources.Load<CursedEnergyAuraProfile>("Cursed Energy Aura")`를 읽는다. 이 저장 프로필에는 과거 설정이 남아 있었다.

반면 `CursedEnergyAuraSilhouettePreview.ShowProfile`은 저장 프로필을 복제한 DontSave 객체에 Candidate를 적용하고 Configure로 교체한다. 종료/Domain Reload 시 원래 프로필로 복귀하며 저장하지 않는다. `CursedEnergyAuraEndPreview.Candidate`는 같은 3b 묶음에 상하 연결을 더한다. 따라서 **미리보기를 승인해도 일반 F 기본 프로필에는 전달되지 않았던 것**이 원인이다. F 입력/자원/HUD가 구형 효과를 강제 생성한 문제는 아니었다.

## 2. 정식 저장한 구성

기존 `CursedEnergyAuraEndPreview.Candidate`를 그대로 적용해 아래3개 Data Asset만 저장했다. 각 프로필의 GUID·색·프리팹·크기 곡선·나머지 수치는 보존했다.

- `unity/Assets/Shibuya/Resources/Cursed Energy Aura.asset`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/Presets/Pink Aura.asset`
- `unity/Assets/Shibuya/VFX/CursedEnergyAura/Presets/Green Aura.asset`

| 설정 | 이전 | 정식 기본 |
|---|---:|---:|
| Surface Details (`wisps.enabled`) | ON | OFF |
| 고배율 국소 불꽃 형태 보존 | OFF | ON |
| 증가 주력량의 추가 공간 확장 | 0 | 0.65 |
| 먹물 경계 두께·대비 배율 | 1.00 | 1.30 |
| 넓은 내부 먹물 면적 비중 | 1.00 | 0.80 |
| 화염 끝 분리 조각 | OFF | ON |
| 머리 위 흐름 연결 | 0 | 1 |
| 발밑 흐름 연결 | 0 | 1 |

Interior 가시성0.65 / 밀도1.0, 분리 조각 교체 속도0.7, Main/Ink 강도·색·불규칙성·속도 등은 승인 후보가 사용하던 기존값 그대로다. 새로운 효과/geometry/shader를 만들지 않았다. Surface Details 데이터는 삭제하지 않고 표시만 OFF다.

직렬화 전후 JSON 비교에서 **세 프로필 모두 위8개 항목만 의미상 변경**됐다. 신규 추가 필드의 중립값도 저장되어 있으나 다른 설정값을 재튜닝하지 않았다. 클래스의 중립 기본값은 레거시 해석을 위해 유지했고, `CursedEnergyAuraAssets.Create`가 새 기본 자산을 처음 작성할 때만 기존 Candidate로 승인 구성을 저장하도록 했다. 이미 존재하는 사용자 프로필을 자동 수리/덮어쓰기 하지 않는다. 일반 런타임은 Editor/미리보기 코드에 의존하지 않고 저장 Data Asset만 읽는다.

## 3. 크기·색·기존 자원 보존

저장 곡선과 제한을 변경하지 않았다: 기준 최대 주력100, 100→1.0 / 150→1.5 / 200→2.0, 최소0.5 / 저장 최대3.0. 3b 별도1.7/2.7 후보 곡선이나 3c의2.7/3.0/3.5/4.0 중 하나를 영구 채택하지 않았다.

기존 3c 메뉴의 강제4.0 / 최대200 기준 CurveAt200 실험과 런타임 임시 clamp4는 그대로 유지한다. Inspector의 곡선과 최대 제한도 계속 조절할 수 있다. 이번 비교는 **양쪽에 같은 기존 저장 곡선**을 써서, 강제3.0/4.0 메뉴와 기본1.0/2.0을 섞어 비교하지 않았다.

청록 mainColor(0.02,0.75,1), Pink(1,0.035,0.36), Green(0.08,1,0.24)와 각 보조색·먹물색은 동일하다. PASS4B.4 충전 Profile/Controller/Input/Match/HUD/소비·회복/중단 로직은 바이트 단위로 보존했다. 잔여 주력에 따른 크기 변화는 추가하지 않았다.

구버전 원본 YAML·meta·SHA-256은 **`docs/archive/CURSED_ENERGY_AURA_PRE_4B4A.json`**에 작은 영구 백업으로 남겼다. 약11KB이며 Unity에 import되는 중복 프리셋이 아니다. QA 폴더에도 변경 전 원본과 전체 직렬화 JSON을 보존했다. 복구 시에는 사용자 승인 후 해당3개 프로필만 대상으로 삼고 기존 GUID를 유지한다.

## 4. 실제 Play Mode 및 시각 비교

QA 루트: `unity/Logs/ShibuyaPass4B4aQA/20261025-default/`.

- QA 메뉴/Configure/임시 프로필 없이 실제 Play 진입 → 일반 F → 해제 확인.
- 별도 새 Play 진입에서도 저장 기본 프로필이 그대로 로드됨을 확인.
- 일반 F와 기존 승인 Candidate를 같은 씬·카메라·캐릭터·포즈·위상2.0·입자 seed38421·최대 주력량으로 비교. 비교 시 미세 입자는 양쪽 모두 정지/clear하여 불규칙 입자 차이를 통제했다.
- 청록100/150/200, Pink200, Green200 × 정면/측면/45도/후면/하이앵글/상단 = **30쌍 / 60장, decoded RGB 차이 모두0픽셀**.
- 메인 field 정점도 같은 조건에서 동일했다. 이는 통제한 정지 비교이며 모든 실제 움직임·환경의 픽셀 동등성을 주장하지 않는다. 렌더 코드와 움직임 로직은 변경하지 않았다.

`media/` 비교 이미지(LEFT 승인 미리보기 / RIGHT 일반 F):

- `Cursed-Energy-Aura-200-front-comparison.png`
- `Cursed-Energy-Aura-200-high-comparison.png`
- `Cursed-Energy-Aura-200-top-comparison.png`
- `Pink-Aura-200-front-comparison.png`
- `Green-Aura-200-front-comparison.png`

원본 이미지와 차이 수치: `direct-entry-recheck/legacy/ShibuyaPass4B3QA/4B4a/`, `4B4a-render-equivalence.json`.
메뉴 없는 일반 F/HUD 캡처: `direct-entry-recheck/legacy/ShibuyaPass4B4aQA/normal-F-hud.png`.

## 5. 테스트 결과와 실패 이력

| 별도 실행 | PASS | FAIL | SKIP |
|---|---:|---:|---:|
| `targeted/results.xml` | 5 | 2 | 0 |
| `focused-recheck/results.xml` | 0 | 3 | 0 |
| `direct-entry-recheck/results.xml` | 3 | 0 | 0 |

**현행8개 고유 케이스의 최신 결과: 8 PASS / 0 FAIL / 0 SKIP. 단일 전체 성공 실행이 아니다.**

최초5 PASS: 세 저장 프로필의 승인 구성 일치, 4B.3c 크기 실험, 색상/F 반복/리소스 정리, 4B.4의100/150/200 자원 상태·회복·이벤트, 최대치 홀드·빠른 반복·중단.
최종3 PASS: 독립적인 일반 F 첫 진입, 별도 재진입, 동일 조건 렌더 비교.

실패 기록은 삭제하지 않았다. 최초에는 한 EditMode 검사에서 Play Mode를 두 번 오가다가 두 번째 진입이 frame1에서 멈춰180초 timeout이 났고, 비교 준비에서는 NullReference가 났다. 후속 재확인에서도 중첩 EnterPlayMode와 빈 추가 UnitySetUp을 포함한 검사 구성에서 재개 후 지역변수가 null/0인 기록이 확인됐다. 당시 비교 상태는 path=null / maximum=0 / held=false / envelope=0이었다.

기존 검증에서 사용하던 **각 public UnityTest가 직접 Enter/Exit를 소유하는 형태**로 진입 검사를 두 케이스로 분리하고, 빈 추가 UnitySetUp을 제거했다. 동일 검증 조건과 엄격한 RGB0 차이 기대값은 유지했다. 이후 두 진입과 실제30쌍 비교가 통과했으며 런타임 코드는 수정하지 않았다. 검사 실행/재개 구성 문제로 판단하지만 Unity Test Framework 내부의 정확한 원인까지 확정하지 않는다. 교체된 복합 진입 테스트의 실패도 역사적 결과로 남긴다. 첫/두 번째 검사 소스 사본도 QA 폴더에 보존했다.

## 6. 리소스·Console·환경

12회 F 반복과 세 저장 색상 전환에서 같은 Visual 재사용, 소유 Material4 / Mesh4 유지. 네 번째 메시1개는 이미 승인 후보에 있던 분리 화염 공유 메시이며 새로 추가한 시스템이 아니다. F 해제 후 렌더 비활성/파티클0, Visual 파괴 후 소유 Material0 / Mesh0을 확인했다.

최종 C# compile 성공. shader 소스와 Material 자산을 변경하지 않았으며 비교 렌더에서 shader 오류 없음. 현재 Console error0 / warning1(`Deleting invalid font reference.`). 세션 보존 버퍼의 과거 error5는 이전4B.4 테스트 작성 중 컴파일 오류 이력이며 지우지 않았다. 이번 검사 실패 XML과 Console 상태를 구분한다.

최종 Play stopped / 씬 clean / QA 비활성 / targetFrameRate -1 / vSync0 / Time.timeScale1 / AutoTick true·16ms. AutoTick과 Play Unfocused 설정을 변경하지 않았다. 정체 상태에서 background player loop를 한 번 요청했으며 영구 설정이나 포커스 변경은 하지 않았다. 테스트용 프레임률은 원래 값으로 복구됐다.

## 7. 변경 범위와 사용자 확인

변경한 기존5개: 위 Data Asset3개, `unity/Assets/Shibuya/Editor/CursedEnergyAuraAssets.cs`, `unity/Assets/Shibuya/Editor/ShibuyaPass4B3cTests.cs`.
기존3c 테스트는 저장 기본 상하 연결의 기대값만 승인 승격에 맞춰0→1로 갱신했다. 크기 곡선/제한/현재 주력 독립성 기준은 유지했다.

신규4개: `unity/Assets/Shibuya/Editor/ShibuyaPass4B4aTests.cs`와 `.meta`, 이 보고서, 위 단일 archive JSON.
기존 한글 Inspector를 그대로 사용하며 새 설정 필드는 없다.

시작1,189개 파일 중 기존5개만 변경, 1,184개 동일, 삭제0. 기존 미커밋 전투·카메라·Gojo/VFX·USER VERIFIED·Scene/Prefab/Packages/ProjectSettings·모든 기존 meta·4B.4 자원/HUD 파일을 보존했다. staging/commit/push/merge/reset/clean/checkout/worktree 작업 없음.

직접 확인:

1. 기존 `ShibuyaCombatMVP`를 열고 Play. **QA 메뉴를 누르지 말고 F만 홀드**한다.
2. Details 뿔 대신 전신 주력장·강한 먹물 경계·Interior·분리 화염·상하 연결이 나타나는지 정면/옆/뒤/하이앵글에서 본다.
3. 기본 최대100에서는 배율1.0이다. 이전 강제4.0 미리보기보다 작은 것이 이번 구성 승격의 오류를 뜻하지 않는다. 최종 크기 정책은 별도 사용자 선택 사항이다.
4. 런타임 `ShibuyaEnergyCharge` 메뉴로 현재 주력을 비우고 F 충전/HUD를 확인한다. 회복해도 크기가 변하지 않아야 한다.
5. F 해제와 Play 종료 후 재진입에서도 같은 최신 기본 구성이 나오는지 확인한다.

이번 패스는 승인 후보의 정식 연결 작업이며 외형을 추가 polish하지 않았다. 시각 최종 승인·새 크기 정책·다인전 성능은 사용자 검수/후속 범위로 남긴다.
