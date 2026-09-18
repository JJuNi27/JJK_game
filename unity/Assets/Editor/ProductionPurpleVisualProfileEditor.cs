using System.Collections.Generic;
using JJKGame.Player;
using UnityEditor;
using UnityEngine;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(ProductionPurpleVisualProfile))]
    public sealed class ProductionPurpleVisualProfileEditor : Editor
    {
        private static readonly Dictionary<string,string> Labels=new()
        {
            ["useIntegratedCandidate"]="D2-R3 + OuterR2 통합 사용",["bodyEnabled"]="본체 표시",["outerEnabled"]="외부 효과 표시",
            ["body"]="확정된 D2-R3 본체",["outer"]="확정된 OuterR2 외부 효과",
            ["shellEmission"]="표면 발광",["shellDensity"]="표면 밀도",["internalEnergy"]="내부 에너지",["inwardSpeed"]="내향 흐름 속도",
            ["turbulenceSpeed"]="난류 속도",["arcRadius"]="내부 방전 두께",["plasmaScale"]="플라즈마 구조 크기",["arcLifetime"]="번개 수명 (초)",
            ["outerReach"]="내부 방전 외향 범위",["instability"]="불안정한 발광",["voidContrast"]="어두운 틈 대비",
            ["haloEnabled"]="아우라 표시",["lightningEnabled"]="외향 번개 표시",["debrisEnabled"]="파편 표시",["distortionEnabled"]="국소 왜곡 표시",
            ["residualSeconds"]="이동 잔류 시간 (초)",["velocityInheritance"]="투사체 속도 상속 비율",["maxResidualRadius"]="잔류 거리 제한 (본체 반경 배율)",
            ["haloRatio"]="아우라 지름 배율",["haloIntensity"]="아우라 밝기",["sparkCount"]="작은 파편 최대 수",["sparkSpeed"]="파편 외향 속도",
            ["sparkEmission"]="파편 발광",["heroFragments"]="주요 파편 수",["heroFragmentScale"]="주요 파편 크기",["arcReach"]="외향 번개 도달 배율",
            ["arcWidth"]="외향 번개 두께",["arcEmission"]="외향 번개 발광",["distortionRatio"]="왜곡장 지름 배율",["distortionStrength"]="국소 왜곡 강도"
        };
        private static GUIContent Label(SerializedProperty p)=>new(Labels.TryGetValue(p.name,out var label)?label:p.displayName);
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("게임플레이 수치는 변경하지 않습니다. 통합 사용을 끄면 보존된 이전 Production 표현으로 돌아갑니다. 생성된 시퀀스에는 재시전 시 적용됩니다.",MessageType.Info);
            foreach(string field in new[]{"useIntegratedCandidate","bodyEnabled","outerEnabled"})
            {var p=serializedObject.FindProperty(field);EditorGUILayout.PropertyField(p,Label(p));}
            foreach(string field in new[]{"body","outer"})
            {
                var group=serializedObject.FindProperty(field);group.isExpanded=EditorGUILayout.Foldout(group.isExpanded,Label(group),true);
                if(!group.isExpanded)continue;
                using(new EditorGUI.IndentLevelScope())
                {
                    var end=group.GetEndProperty();var p=group.Copy();p.NextVisible(true);
                    while(!SerializedProperty.EqualContents(p,end)){EditorGUILayout.PropertyField(p,Label(p),true);if(!p.NextVisible(false))break;}
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
