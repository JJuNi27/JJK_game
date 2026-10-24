# PASS 4B.3c — 상·하단 흐름 연결 및 최대200의 4.0배 실험

> 2026-10-25 체크포인트 갱신: 이 시각 구성과 상하 연결을 포함한 승인 후보가 4B.4a의 저장 기본에 전달됐고 사용자가 일반 F 최신 출력을 확인했다. 아래 PENDING 표시는 당시 실험 기록이며 현재의 시각 방향 승인을 취소하지 않는다. 2.7/3.0/3.5/4.0의 최종 기본 배율은 여전히 미선택이다. 크기 곡선/제한의 실험과 표현 구성의 승격은 별개다.

2026-10-24 KST. **LOCAL / CODEX VALIDATED(targeted 기술 범위) / USER VISUAL REVIEW 대기**.
브랜치 `feat/shibuya-combat-mvp`, HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`.

사용자는 이번 요청에서 기존 **4B.3b 전신 혼령 아우라의 전체 디자인을 승인**했다. 그 외형을 기준으로 끝부분과 크기 실험만 보완했다. 이번 4B.3c 연결/4배 후보의 최종 시각 승인까지 받은 것은 아니다. 저장된 기본·Pink·Green 프로필 및 기존 4B.3b 후보 메뉴를 보존했고 4.0을 기본으로 채택하지 않았다.

## 1. 조사 및 실제 원인

AGENTS, handoff/current task/locked, 4B.3a/3b 보고서, 실제 Field/Visual/Geometry/Shader/Profile, 기존 A~E 설정을 확인했다. handoff/current task는 10-22 시점이므로 최신 로컬 미커밋 구현과 구분했다. 기존 2.7 complete/3.0 원속도 MP4를 probe하고 인접 시점들을 직접 추출했다. 시작 시 Unity stopped, 씬 clean, 테스트 비실행, staged0 / tracked dirty29 / untracked110이었다.

`CursedEnergyAuraField.CreateMesh`는 높이41 × 둘레49(중복 seam 포함)의 단면 사이만 연결했다. 상·하단을 가로지르는 삼각형은 없었다. 상단 원래 body radius를 줄이더라도 마지막에 확장 thickness를 더해 큰 끝 단면이 남았다. shader의 field end mask는 UV 상단0.84~1과 하단0~0.035를 감쇠했다. 따라서 **geometry의 큰 열린 단면과 끝 가시성 억제**가 함께 작동했다. 큰 배율일수록 수평 margin은 늘고 이 구조가 그대로 남으므로 더 큰 개구부로 보였다.

기존 4B.3b 후보의 실제 CPU 끝 단면 폭:

| 배율 | 상단 폭 | 하단 폭 | 최하단 local Y |
|---|---:|---:|---:|
| 2.7 | 1.19075m | 1.81954m | 0.053778m |
| 3.0 | 1.35266m | 1.97911m | 0.053778m |

상단은 하이앵글에서 큰 빈 공간이 확인됐다. 하단은 지상에서 지면/몸에 가려지므로 코드만으로 지상 시각 결함을 단정하지 않았다. 활성 캐릭터를 공중 정지 포즈에 두고 아래에서 촬영해 개구부가 보이는 것을 확인했다. 이 정지 포즈 캡처와 실제 점프 검증은 별개다.

## 2. 상단 연결

새 `upperFieldConnection`은 기본0, 실험1이다. 기존 메시의 UV0.8보다 높은 끝 단면만 안쪽으로 굽혀 마지막 단면을 지름0.028m까지 좁힌다. 높이별 흐름과 작은 비대칭 높이 변화가 이어진다. 기존 advected noise, cut, cyan/ink, crest를 그대로 쓰고 새 끝부분에만 같은 density의 erosion을 조금 강화했다.

위로 모인 흐름을 바로 지워버리지 않도록 상단 감쇠 시작을0.84→0.98로 옮긴다. 원판/삼각형 fan/별도 cap renderer/반구 메시를 추가하지 않았다. **엄밀히는 지름2.8cm의 작은 열린 단면이 남으며 물리적인 밀폐가 목표가 아니다.** 투명·침식된 흐름으로 이어져 보이게 하는 방식이다. 머리와 몸의 가독성을 위해 모든 픽셀을 불투명하게 덮지 않는다.

## 3. 하단 연결

새 `lowerFieldConnection` 역시 기본0, 실험1이다. 기존 최하단10%의 단면을 안쪽으로 모아 같은 작은 끝 단면으로 수렴시킨다. 원형 장판이나 지면 충격 효과는 추가하지 않았다. CPU 최저Y는 그대로이며, 연결된 하단의 GPU 변위와 ink hull이 이 최저 높이 아래로 내려가지 않도록 제한했다.

지상과 공중 모두 actor-local 몸 외곽 계산을 사용한다. 바닥에 고정된 별도 효과가 아니다. 실제 점프 중 몸의 월드 높이가 증가해도 field의 local 최저 높이는 유지됐다. 기존 다리/발 및 CharacterController 크기·이동 소유권은 바꾸지 않았다.

## 4. 기존 외형 보존 근거

메인 carrier는 **2,009정점 / 3,840삼각형** 그대로이며 인덱스 배열도 동일하다. 새로운 mesh/material/particle system이 없다. 연결 후보의 중간 row5~31은 기존 후보와 비교해 **정점 좌표 변화0 / 법선 변화0**이었다. 끝 연결이 영향을 주는 가장자리 법선과 상·하단 표현은 의도적으로 달라진다.

연결값0인 중립 경로도 수정 전과 비교했다. 2.7배7장에서는 decoded RGB 차이0, 3.0배7장에서는4~202픽셀의 작은 차이가 남았다(960×720 중 최대 약0.03%). 원인을 특정하지 않았으며 모든 기존 렌더가 픽셀 단위로 완전히 동일하다고 주장하지 않는다. 원본은 `legacy-neutral-render-comparison.json`이다.

새 메인 geometry 최소 삼각형 면적은 약0.000041883m², 뒤집힌 면0, 비유한 좌표0이었다. 이 검사는 수정한 메인 field에 대한 것으로 기존 모든 별도 detail/tip mesh topology를 새로 인증한 것은 아니다.

추가로4.0배 자체도 확인했다. 최소 삼각형 면적 약0.000049761m², 뒤집힌 면0, 정점2,009/삼각형3,840이었다. 4배 미리보기 진입→원래 프로필 복귀와 실제 최대 주력량200 보존도 같은 검사에서 통과했다.

Surface Details OFF, 국소 형태 보존 ON, extraFieldReach0.65, ink 경계1.30/넓은 암부0.80, detached tips ON의 4B.3b 통합 설정을 그대로 재사용한다. Interior 가시성0.65/밀도1, 색상, 움직임 속도/irregularity, 분리 조각 경로·수명은 변경하지 않았다. Blend/ZWrite/Cull 및 카메라/전투 규칙도 그대로다.

## 5. 200 지점 곡선과 4배 미리보기

엔진의 기존 `MaxEnergy / referenceMaxEnergy → AnimationCurve → clamp → smoothing` 경로를 사용한다. **MaxEnergy400을 만들지 않았다.** QA는 복제 전투/주력 데이터의 실제 최대치200으로 실행했다. 원래 Data Asset은 수정하지 않았다.

`CurveAt200`은 원본 곡선을 복제해200 지점과 이후 key값에 차이를 적용한다. 이후 key도 같은 차이를 더해200 직후 크기가 줄어드는 것을 피한다. 100/150 key와 이전 구간은 보존한다. 현재 저장 프로필의100→1.0/150→1.5와 기존 4B.3b 별도 후보 곡선의150→1.7을 혼동하지 않는다. 기존 후보 곡선/메뉴는 그대로 남았다.

실험 프로필의 최대 배율만4로 올린다. 저장 프로필의 최대 제한3과100/150/200 곡선은 보존했다. 4보다 큰 curve값이 입력되면 실험 clamp4가 작동하는 것도 확인했다. 잔여 주력량200/10/0/175를 바꿔도 4배 field 좌표와 크기는 동일했다.

## 6. 실제 화면 크기

동일 캐릭터·카메라·정지 자세, phase0.8/1.4/2.0/2.6/3.2, 동일 deterministic seed로 비교했다. 960×720에서 body-only 영상과12/255 초과 RGB 차이의 외곽 bbox를 측정했다. 측정 시 Main/Ink만 켜 작은 분리 조각·Rim·미세 입자가 크기를 부풀리지 않게 했다. 몸 기준 크기는 실제 body mesh의 화면 투영이다.

| 최대 주력량 | 실험 배율 | 폭 중앙값 | 높이 중앙값 | 몸 대비 폭/높이 |
|---:|---:|---:|---:|---:|
| 200 | 2.70 | 333px | 340px | 3.371 / 1.341 |
| 200 | 3.00 | 352px | 348px | 3.563 / 1.373 |
| 200 | 3.50 | 389px | 361px | 3.938 / 1.424 |
| 200 | 4.00 | 429px | 378px | 4.343 / 1.491 |

4.0은2.7 대비 폭 약28.8%/높이11.2%, 3.0 대비 폭21.9%/높이8.6% 증가했다. 20개 phase 표본에서 화면 clipping은 없었다. 모든 시점/플랫폼에서 같은 bbox 크기와 가독성을 보장하는 수치는 아니다.

4.0에서도 국소 deformation 크기는 4B.3b의 기준 규모에 머문다. 몸 밖 범위를 늘렸고 개별 불꽃 폭·굽힘을4배로 늘리지 않았다. 다만 커진 carrier 위에 기존 넓은 면이 더 크게 읽히는 프레임은 남는다. ‘4.0에서 형태 문제가 완전히 없다’고 평가하지 않는다.

## 7. Inspector·메뉴

기존 SerializedProperty/Undo 편집에 추가한 한국어 항목은 두 개다.

- 머리 위 주력 흐름 연결
- 발밑 주력 흐름 연결

두 값 모두 기존 자산에서는0이며, 실험 프로필에서 독립 비교할 수 있다.

Play Mode에서:

1. `JJK Game → 시부야 PASS4B3c → 연결 후보 강제 배율 미리보기 → 2.70 / 3.00 / 3.50 / 4.00 (실제 주력량 불변)`.
2. F 홀드, 카메라 회전·하이앵글, 이동·점프·공격·해제 확인. 이 메뉴는 시각 배율 강제 미리보기이며 실제 최대 주력량을 바꾸지 않는다.
3. 실제 최대 주력량에 따라 새 곡선을 보고 싶으면 `200 기준 곡선 실험` 메뉴를 사용한다. 실제 최대치가100이면 여전히100 크기다.
4. `비교 → 기존 4B3b 3.00` 또는 `기존 저장 프로필로 복귀`로 비교한다. Play Mode 종료/Domain Reload 시 임시 복제 프로필을 정리한다.

기존 4B3b 미리보기의 복구 소유권을 공유해 두 메뉴가 임시 프로필을 서로 덮어 쓰거나 원본 자산을 저장하지 않도록 했다. 캐릭터의 전투 Profile/HP/실제 MaxEnergy는 미리보기 메뉴에서 바꾸지 않는다.

## 8. 시각 증거

QA 루트: `unity/Logs/ShibuyaPass4B3cQA/20261024-end-connection/`.

`media/`:

- `ends-2.70-*.png`, `ends-3.00-*.png`: 왼쪽 기존4B.3b, 오른쪽 연결 후보. front/side/diagonal/back/high/top/close/under.
- `four-sizes-*.png`: 같은7개 방향에서2.7/3.0/3.5/4.0 나란히 비교.
- `size-2.70-orbit.mp4`, `size-3.00-orbit.mp4`, `size-3.50-orbit.mp4`, `size-4.00-orbit.mp4`: 각각 원속도360도 전체 회전.
- `four-moving.mp4`, `four-jump.mp4`, `four-release.mp4`: 4배 이동/점프/해제.
- `four-orbit-contact.png`: 4배 회전 인접 시점/여러 방향.

각 orbit은 실제 source time 약4.01~4.04초, 93~98프레임, 최대 source 간격 약85~88ms였다. 실제 timestamp대로30fps 출력에 프레임을 복제했고 보간/슬로 재생은 하지 않았다. 마지막 프레임 유지로 container는 약4.07~4.13초다. 사용자 실제 Play Mode의 연속 프레임 검수를 대체하지 않는다. `video-sampling.json` 및 원본 timestamp에 근거를 남겼다.

## 9. targeted 결과와 최초 실패

**별도 실행의 최신 고유10 PASS / 0 FAIL / 0 SKIP**. 단일10개 전체 실행 또는 대규모 전투 회귀 통과라는 뜻이 아니다.

| 실행 | 단일 실행 결과 | 비고 |
|---|---|---|
| `targeted` | 2 PASS / 2 FAIL | geometry/curve PASS, 최초 orbit/점프 관측 FAIL |
| `end-and-jump-check` | 2 PASS / 1 FAIL | 유효한 전후 이미지·geometry PASS, 점프는 상승했으나 지연 파괴 자원 검사 FAIL |
| `remaining-targeted` | 6 PASS / 0 FAIL | orbit4개/이동/점프/해제, 색상/자원, overdraw, 기존 지상85피해·공중5타·Interior 검사 |
| `four-geometry-final` | 1 PASS / 0 FAIL | 4배 자체 geometry 및 미리보기 복귀 |

최신 case별 집계/원본 XML은 `validation-summary.json` 및 각 실행 폴더에 있다. 기존 QA와 실패 원본을 삭제하거나 덮어쓰지 않았다.

실패/무효 자료 처리:

- 최초 orbit 첫 프레임 뒤1.6349초 공백을 확인했다. 촬영 워밍업을 구간 밖에 두고 테스트 Play Mode만 targetFrameRate60으로 실행한 뒤 같은 최대 공백 기준0.25초를 통과했다. 자동 검사 기준을 완화하지 않았다. 최초 stall의 내부 원인을 shader/GC 등 특정 요인으로 확정하지 않는다.
- 최초 점프 관측Y는0.03m였다. 한 프레임 pulse 대신 실제 update를 통과하도록0.05초 입력을 유지한 후1.0587m 상승을 확인했다. 원본에는 정확한 입력 소비 telemetry가 없으므로 최초 실패 원인을 단정하지 않는다. 게임플레이 점프/중력 값을 바꾸지 않았다.
- `Destroy` 후 두 coroutine yield만으로 자원0을 즉시 기대한 검사를 Unity의 지연 파괴 완료를 기다리는0.1초 뒤 관측으로 고쳤다. 기대값0/0은 그대로이며 최종 실제 material/mesh0/0이었다. 무한 대기/재시도는 없다.
- 최초 `before`의 아래쪽 이미지는 actor를 꺼 오라까지 해제한 무효 조건이었다. 첫 `targeted`의 아래쪽 이미지도 긴 프레임 중 자유낙하가 섞인 잘못된 카메라 조건으로 최종 비교에서 제외했다. 최종 `end-and-jump-check`는 활성 actor를 같은 프레임의 정지 포즈로 옮겨 촬영한 아래쪽 이미지다. 실제 점프 검증과 구분한다.

## 10. 리소스·성능 위험

기존4B.3b 통합과 동일하게 재질4/메시4이며12회 F 반복/이동/색상 변경 후 증가가 없었다. 해제 후 visible renderer/particle0, 파괴 후 소유 material/mesh0/0을 Play Mode에서 확인했다. 메인 geometry 정점/삼각형 수는 증가하지 않았다.

4배 field CPU refit 12표본 중앙값1.959ms/최대2.368ms. 전체 CPU 프레임이나 GPU ms가 아니다.

동일 vertex 변형·alpha discard를 재사용한 임시 메모리 shader에서 surviving fragment마다 white alpha0.1을 합성하고 Linear Float의 `C=1-0.9^N`으로 겹침을 측정했다. body depth를 제외한 보수적 wrapper 진단이며 Rim/motes/UI는 제외했다. 임시 shader/material을 저장하지 않았고 finally에서 원래 렌더 설정을 복구했다.

| 사선 카메라5개 phase | covered pixel 중앙값 | 평균 층수 중앙값 | 최고 층수 | fragment-pixel 합 중앙값 |
|---|---:|---:|---:|---:|
| 기존4B.3b 3.0 | 65,234 | 1.222 | 6 | 78,985 |
| 연결2.7 | 58,191 | 1.395 | 8 | 76,636 |
| 연결3.0 | 64,205 | 1.394 | 7 | 89,903 |
| 연결3.5 | 72,145 | 1.441 | 7 | 105,435 |
| 연결4.0 | 87,920 | 1.446 | 7 | 132,150 |

상·하단의 실제 coverage가 추가되어 같은3.0에서도 fragment 합이 약13.8% 늘었다. 4.0은 기존4B.3b 3.0보다 약67.3% 많다. ‘메시 수가 같으므로 성능도 동일’하다고 주장하지 않는다. 실제 GPU 시간, 넓은 맵/다수 캐릭터/저사양 기기는 미검증이다.

## 11. 컴파일·Console·환경

C# 최종 compile 성공. 모든 비교 렌더와 동일 shader 코드의 겹침 진단에서 shader compile 실패는 검출되지 않았다. runtime VFX 예외를 숨기지 않았다.

Pipeline에서는 이번 작업 중 상태 JSON 공유 위반1건과 `/api/exec`의 main-thread5초 timeout4건이 발생했다. 일부 무거운 최종 상태 조회도 timeout으로 끝났으므로 전역 Resources 최종 재스캔을 완료했다고 주장하지 않는다. 마지막4배 검사 시작 호출도 응답은 timeout이었지만 discovery/원본 XML로 실제 실행·완료를 확인해 중복 시작하지 않았다.

단순 조회에서는 Play stopped / 씬 clean / frameRate -1 / Time.timeScale1을 확인했다. 마지막 검사 전에는 현재 Console error1이 남았고, 마지막 검사 후의 ground truth는 error0 / warning0이었다. 보존 버퍼에는 이전10건을 포함한 error15가 남아 있다. **오류 없는 세션이었다고 보고하지 않는다.** 원본 오류와 count는 `final-editor-state.json`, 마지막 상태는 `final-editor-state-after-four.json`에 남긴다. Console을 수동 clear하지 않았으며 패키지를 수정하거나 연결을 재설치하지 않았다.

AutoTick은 확인 당시 true/16ms였고 변경하는 호출은 없었다. QA에서만 설정한 targetFrameRate60은 teardown 후 원래 -1로 복구된 것을 확인했다. vSync/Time.timeScale/Play Unfocused/Packages/ProjectSettings는 변경하지 않았다. Windows 직접 조작·포커스 요청도 하지 않았다.

## 12. 변경 범위·Git·남은 검수

기존 로컬 파일6개:

- `Scripts/CursedEnergyAuraField.cs`
- `Scripts/CursedEnergyAuraProfile.cs`
- `Scripts/CursedEnergyAuraVisual.cs`
- `Editor/CursedEnergyAuraInspector.cs`
- `Editor/CursedEnergyAuraSilhouettePreview.cs`
- `VFX/CursedEnergyAura/CursedEnergyAuraVolume.shader`

신규: `Editor/CursedEnergyAuraEndPreview.cs`, `Editor/ShibuyaPass4B3cTests.cs`와 각meta, 이 보고서. 위 경로는 `unity/Assets/Shibuya/` 기준이다. 기존 Geometry.cs/분리 조각 코드, Controller/F 입력, 기존 Scene/Prefab/전투 코드/Data Asset/Gojo/VFX/승인값/GUID는 그대로다.

시작1,177개 파일의 SHA-256 중1,171개가 동일하며 변경6개는 위 목록뿐이다. 새 파일5개가 추가됐다. 원본 프로필/HP1000·100/기존 피해20·20·20·25, 기존 dirty 문서와 작업을 보존했다. 최종 staged0 / tracked dirty29 / untracked115, 브랜치/HEAD 그대로이며 commit/push/merge/reset/clean/checkout/worktree 작업을 하지 않았다.

남은 직접 검수 포인트:

- 연결은 개선됐으나 일부 정면/사선에서 상단이 후드/둥근 덮개, 하단이 굵은 테두리처럼 읽힐 여지가 있다. ‘캡처럼 보이는 문제가 모든 프레임에서 해결됐다’고 평가하지 않는다. 두 연결 강도를 Inspector에서 독립 비교할 수 있다.
- 4.0은 범위가 커지지만 기존 큰 반투명 면도 더 크게 보이고, 캐릭터를 가리는 양과 렌더 비용이 증가한다. 국소 흐물거림 증폭은 막았으나 모든 면/천 인상이 사라진 것은 아니다.
- transparency/phase에 따라 머리나 바닥이 일부 드러난다. 작은 물리 개구부도 남긴다. 불투명 뚜껑으로 가리지 않았다.
- 카메라 각도별 상·하단 연결 품질, 움직이는 몸과의 연결,3.5/4.0 채택 여부는 사용자 Play Mode 승인 대기다. 기존 전체 디자인의 사용자 승인을 새 후보의 USER VERIFIED로 확대하지 않는다.
