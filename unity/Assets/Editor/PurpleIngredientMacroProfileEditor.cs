using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientMacroProfile))]
    public sealed class PurpleIngredientMacroProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("무라사키 재료 구체의 매크로 공격성 비교 후보입니다. 기존 후보보다 우선하며 새 시전부터 적용됩니다. 생성 위치·크기·시간과 완성된 무라사키는 유지합니다.",MessageType.Info);
            var fields=new[]{"candidateEnabled","silhouetteBreakup","flowReach","blueSpeed","blueWidth","blueEmission","redSpeed","redWidth","redEmission","trailSpan"};
            var labels=new[]{"매크로 후보 사용","표면 불안정도","외부 흐름 도달 반경","아오 흡인 속도","아오 흡인 두께","아오 흡인 발광","아카 분출 속도","아카 압력 두께","아카 압력 발광","궤적 잔상 길이"};
            for(int i=0;i<fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i]),new GUIContent(labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
