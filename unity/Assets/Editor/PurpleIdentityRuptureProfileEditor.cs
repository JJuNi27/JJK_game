using JJKGame.Player;
using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIdentityRuptureProfile))]
    public sealed class PurpleIdentityRuptureProfileEditor : Editor
    {
        private static readonly string[] Fields={"candidateEnabled","coreRadiance","edgeBreakup","fissureContrast","energyChurn","frontOpening","tearWidth","dischargeReach","dischargeEmission"};
        private static readonly string[] Labels={"수렴 파열 후보 활성화","중심 광원 강도","외곽 국소 파열","어두운 균열 대비","발광 변화 강도","앞면 열림 강도","파열 통로 너비","방출 도달 거리","방출 발광 강도"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. 활성화 시 Purple Charge 본체에만 적용하며 Travel, 종점, 기존 후보를 보호합니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
