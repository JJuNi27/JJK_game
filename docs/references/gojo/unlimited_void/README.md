# Unlimited Void 레퍼런스

이 폴더는 현재 무량공처 비주얼 참고자료용이다.

- `interior_anime_ref_01.png`
  - blue-black cosmic interior의 원작 분위기 기준
  - 전경 캐릭터는 무시
  - background / Domain environment만 참고

- `interior_concept_ref_01.png`
  - blue nebula / cosmic depth / White Blood 배치 아이디어용
  - 의도적으로 과장된 생성 컨셉
  - 정확한 최종 목표가 아님

- `cosmic_eye_ref_01.png`
  - 중앙 cosmic-eye의 **직접적인 형태/비율/스케일 기준**
  - foreground character는 무시
  - eye 자체는 자유롭게 재해석하지 말고 가능한 한 reference와 가깝게 맞춘다.

- `COSMIC_EYE_REFERENCE_ANALYSIS.md`
  - Astra가 reference와 현재 구현을 직접 비교한 분석 문서
  - 다음 Cosmic Eye 구현 전에 반드시 읽는다.
  - reference 비율, 명암 구조, 색, tail, 현재 구현 차이, 구현 Top 5가 정리되어 있다.

Binary media는 기본적으로 Git에서 무시되지만 Codex는 로컬 workspace에서 직접 확인할 수 있다.

## 매우 중요 — interior_concept_ref_01.png
이 이미지는 사용자가 ChatGPT에게 원하는 분위기를 설명해서 **일부러 과장되게 생성한 컨셉 이미지**다.

최종 정답 이미지가 아니며 그대로 복제하지 않는다.

참고:
- blue / blue-white nebula language
- cosmic depth
- layered space
- richer environment 속 White Blood 배치

전체 분위기/원작성은 `interior_anime_ref_01.png`를 우선.

최종 interior는:
**더 어둡고 절제된 anime reference**
와
**과장된 concept richness**
사이.

## 매우 중요 — cosmic_eye_ref_01.png
이 reference는 concept image와 다르게,
**eye 자체는 다음 pass에서 직접적인 목표 reference로 사용한다.**

Astra 분석에서 특히 중요하게 확인된 것:
- Eye outer radius 1.0 기준 pupil 약 **0.43**
- main iris 약 **0.45~0.83**
- strongest bright rim 약 **0.84~0.89**
- outer corona 약 **0.90~1.00**
- rightward tail은 center에서 약 **2.6~2.9**까지 보이는 핵심 실루엣
- pupil은 안정적인 pitch-black
- pupil과 bright rim 사이에 **어두운 여백 + 큰 비정형 cloud**
- warm ivory / pale gold-orange accent는 제거하지 않음
- 촘촘한 반복 선보다 큰 cloud mass가 우선
- tail은 본체 cloud에서 자연스럽게 이어져야 함

현재처럼 "영감을 받아 다른 블랙홀 효과를 만든다"보다,
**reference의 eye 구조를 실제 목표로 보고 충실하게 재현하는 쪽을 우선한다.**

세부 내용은 `COSMIC_EYE_REFERENCE_ANALYSIS.md` 참고.
