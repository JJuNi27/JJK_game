using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientFinal2Profile))]
    public sealed class PurpleIngredientFinal2ProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("리부트 2차 + Final 후보가 켜진 상태에서 적용합니다. 본체·궤적·충돌 시간은 유지하고 외곽 입체 파열과 공간 반전만 교체합니다. 새 시전부터 적용됩니다.",MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("candidateEnabled"),new GUIContent("입체 에너지 파열·공간 반전 후보 사용"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
