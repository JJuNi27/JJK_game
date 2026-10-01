using JJKGame.Player;
using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIdentityCoupledProfile))]
    public sealed class PurpleIdentityCoupledProfileEditor : Editor
    {
        private static readonly string[] Fields={"candidateEnabled","coreRadiance","edgeBreakup","fissureContrast","energyChurn","frontInterruption","dischargeReach","dischargeEmission"};
        private static readonly string[] Labels={"질량-외곽 결합 후보 활성화","중심 광원 강도","외곽 국소 파열","어두운 균열 대비","발광 변화 강도","앞면 노출 단절","방전 도달 거리","방전 발광 강도"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. 완성 Purple 본체에만 적용합니다. Travel·종점 폭발·기존 후보는 유지됩니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
