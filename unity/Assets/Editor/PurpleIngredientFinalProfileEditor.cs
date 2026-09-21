using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientFinalProfile))]
    public sealed class PurpleIngredientFinalProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("리부트 2차 후보가 켜진 상태에서만 적용됩니다. 본체와 궤적·접촉 시점·2프레임 길이는 그대로 두고, 외곽 파열과 충돌 그래픽만 교체합니다. 새 시전부터 적용됩니다.",MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("candidateEnabled"),new GUIContent("짧은 에너지 파열·강한 충돌 후보 사용"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
