using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaBlackFlashProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaBlackFlashInspector : UnityEditor.Editor
    {
        public static readonly Dictionary<string,string> Labels=new Dictionary<string,string>{
            {"m_Script","스크립트"},{"effectEnabled","흑섬 본체 활성화"},{"scale","전체 크기"},{"intensity","전체 발광 강도"},
            {"coreSize","검은 충돌핵 크기"},{"coreExpansion","충돌핵 팽창 배율"},{"lightningColor","붉은 번개 색상"},
            {"branchWidth","주 가지 굵기"},{"branchLength","주 가지 길이"},{"branchCount","전체 줄기 수 (주·보조)"},{"forksPerBranch","줄기당 추가 분기 수"},
            {"forwardBias","타격 전방 방향 편향"},{"flashIntensity","최초 충격광 강도"},{"flashDuration","최초 충격광 시간 (초)"},
            {"coreDuration","충돌핵 시간 (초)"},{"branchDuration","큰 분기 시간 (초)"},{"pressureSize","충격파 최대 반경"},{"pressureSpeed","충격파 확장 속도"},
            {"smokeCount","연기 덩어리 수"},{"smokeSize","연기 크기"},{"residueDuration","잔류 시간 (초)"},{"duration","전체 수명 상한 (초)"},
            {"simultaneousLimit","동시 재생 최대 수"},{"inkShader","검은 주력 셰이더"},{"lightShader","접촉광·압력 셰이더"},
            {"primaryBranchCount","굵은 파열 주가지 수"},{"coreDensity","검은 핵의 중심 밀도"},
            {"ruptureGrowthTime","파열 가지 성장 시간 (초)"},{"silhouetteChange","파열 실루엣 변화 강도"},
            {"dischargeInterval","작은 방전 교체 간격 (초)"},{"residualDischargeTime","잔여 방전 시간 (초)"},
            {"smokeSpread","잔류 연기 분산 강도"}
        };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();var p=serializedObject.GetIterator();
            if(p.NextVisible(true))do{using(new EditorGUI.DisabledScope(p.name=="m_Script"))EditorGUILayout.PropertyField(p,new GUIContent(Labels.TryGetValue(p.name,out string label)?label:p.name),true);}while(p.NextVisible(false));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("실제 HP 감소가 확정된 흑섬만 재생합니다. 시간은 국소 히트스톱과 독립적인 실시간입니다. 전체 수명보다 긴 레이어는 상한에서 종료됩니다. 포화 시 가장 오래된 잔류를 교체하며 새 흑섬은 재생합니다. 화면 반전·카메라·추가 피해는 적용하지 않습니다.",MessageType.Info);
        }
    }
}
