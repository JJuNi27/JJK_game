using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientPolishProfile))]
    public sealed class PurpleIngredientPolishProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("무라사키 생성에 쓰는 아오/아카 재료 구체만 변경하는 비교 후보입니다. 새 시전부터 적용됩니다. 독립 기술과 본체·탄생 효과는 변경하지 않습니다.",MessageType.Info);
            var fields=new[]{"candidateEnabled","silhouetteBreakup","blueFlowWidth","blueFlowEmission","redFlowWidth","redFlowEmission","moteLength"};
            var labels=new[]{"재료 구체 후보 사용","외곽 미세 변형","아오 흡인 흐름 두께","아오 흡인 흐름 발광","아카 압력 흐름 두께","아카 압력 흐름 발광","짧은 입자 궤적 길이"};
            for(int i=0;i<fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i]),new GUIContent(labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
