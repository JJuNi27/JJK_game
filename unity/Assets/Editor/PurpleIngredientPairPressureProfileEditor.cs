using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientPairPressureProfile))]
    public sealed class PurpleIngredientPairPressureProfileEditor : Editor
    {
        static readonly string[] Fields={"candidateEnabled","pressureReach","pressureWidth","pressureDepth","density","diffuseEmission","crestEmission","blueSpan","redSpread","gapBend","erosion"};
        static readonly string[] Labels={"대형 풍압 후보 활성화","풍압 범위 (구체 반경 기준)","풍압 덩어리 폭","풍압 덩어리 깊이","풍압 밀도","회청·회적색 압력 발광","부분 선두 강조 발광","아오 수렴 흐름 길이","아카 압력 펼침","재료 사이 풍압 굴절","외곽 분해 강도"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. Final2 + FlowFirst + PairPolish를 유지하고 넓은 풍압만 추가합니다. 기존 운동·구체·융합 시간은 변경하지 않습니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
