using JJKGame.Player;
using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientElectricArcProfile))]
    public sealed class PurpleIngredientElectricArcProfileEditor : Editor
    {
        private static readonly (string Field, string Label)[] Fields =
        {
            ("candidateEnabled", "새 전기 아크 후보 활성화"),
            ("arcSlots", "동시 방전 슬롯"),
            ("eventRate", "방전 발생 빈도"),
            ("nearReach", "근거리 도달 거리"),
            ("farReach", "원거리 도달 거리"),
            ("lineWidth", "번개 선 두께"),
            ("brightness", "번개 밝기")
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Purple Ingredient 전기 아크 탐색 후보", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("기존 Final2와 탐색 후보는 보존됩니다. 저장 상태는 기본 OFF입니다.", MessageType.Info);
            foreach (var (field, label) in Fields)
                EditorGUILayout.PropertyField(serializedObject.FindProperty(field), new GUIContent(label));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
