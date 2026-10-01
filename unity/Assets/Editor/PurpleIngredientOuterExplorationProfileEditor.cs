using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(JJKGame.Player.PurpleIngredientOuterExplorationProfile))]
    public sealed class PurpleIngredientOuterExplorationProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("candidateEnabled"), new GUIContent("탐색 후보 활성화"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("violence"), new GUIContent("폭주 강도"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("energyReach"), new GUIContent("외곽 도달 범위"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frontBackDepth"), new GUIContent("전후 깊이 중첩"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventRate"), new GUIContent("파열 이벤트 빈도"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
