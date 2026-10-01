using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(JJKGame.Player.PurpleIngredientHybridProfile))]
    public sealed class PurpleIngredientHybridProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("candidateEnabled"),new GUIContent("Hybrid 후보 활성화"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventRate"),new GUIContent("방전 발생 속도"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reachMultiplier"),new GUIContent("방전 도달 범위"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("depth"),new GUIContent("앞뒤 공간 깊이"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventIntensity"),new GUIContent("방전 폭주 강도"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
