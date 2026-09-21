using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientRebootProfile))]
    public sealed class PurpleIngredientRebootProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("어두운 주력 질량 + 아오 수축층 / 아카 압력층 후보입니다. 기존 후보보다 우선하며 새 시전부터 적용됩니다. 완성된 무라사키와 생성 경로·크기·시간은 변경하지 않습니다.",MessageType.Info);
            var fields=new[]{"candidateEnabled","silhouetteBreakup","crackEmission","bandReach","bandWidth","blueCollapseSpeed","redPressureSpeed","supportingFlow"};
            var labels=new[]{"질량 방향 후보 사용","외곽 불안정도","균열 발광 강도","압력층 도달 반경","압력층 두께","아오 수축 속도","아카 팽창 속도","보조 흐름 존재감"};
            for(int i=0;i<fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i]),new GUIContent(labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
