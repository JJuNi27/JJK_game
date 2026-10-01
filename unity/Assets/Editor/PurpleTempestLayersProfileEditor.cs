using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleTempestLayersProfile))]
    public sealed class PurpleTempestLayersProfileEditor:Editor
    {
        static readonly string[] Fields={"candidateEnabled","windReach","windWidth","windSpeed","windOpacity","brightWidth","brightEmission","darkWidth","darkEmission","stormSpeed","dischargeReach"};
        static readonly string[] Labels={"다층 폭풍 후보 활성화","풍압 범위 (구 반경 배수)","풍압 폭","풍압 회전 속도","풍압 불투명도","밝은 방전 폭","밝은 방전 발광","어두운 주력 폭","어두운 주력 발광","폭풍 변화 속도","방전 범위"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. 본체와 타이밍은 유지하며, 하단 풍압과 밝고 어두운 방전이 겹치는 후보입니다. PressureStorm과 함께 켜면 이 후보가 우선합니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
