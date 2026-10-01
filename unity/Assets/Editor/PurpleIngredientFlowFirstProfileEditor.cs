using JJKGame.Player;
using UnityEditor;
using UnityEngine;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientFlowFirstProfile))]
    public sealed class PurpleIngredientFlowFirstProfileEditor : Editor
    {
        private static readonly string[] Fields={"candidateEnabled","blueFlowEnabled","redFlowEnabled","detailsEnabled","scatterEnabled","purpleAccentEnabled","flowCount","reach","brushWidth","flowBrightness","flowSpeed","groundStrength","orbCount","accentStrength"};
        private static readonly string[] Labels={"새 후보 활성화","아오 흡인 흐름","아카 압력 흐름","작은 구슬 및 섬광","융합 재료 산란","융합 보라색 파열","흐름 수","흐름 도달 거리","붓결 두께","흐름 밝기","흐름 속도","지면 흐름 세기","작은 구슬 수","융합 강조 세기"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("별도 탐색 후보이며 저장 기본 OFF입니다. 기존 Final2와 다른 후보를 덮어쓰지 않습니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
