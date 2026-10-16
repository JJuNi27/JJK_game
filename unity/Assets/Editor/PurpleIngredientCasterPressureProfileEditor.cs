using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientCasterPressureProfile))]
    public sealed class PurpleIngredientCasterPressureProfileEditor : Editor
    {
        static readonly string[] Fields={"candidateEnabled","stormRadius","stormHeight","flowWidth","opacity","crestEmission","stormSpeed","ingredientCoupling","releaseSeconds","forwardSweep"};
        static readonly string[] Labels={"시전자 풍압 후보 활성화","시전자 주변 범위 (미터)","상체·측면 흐름 높이","넓은 공기 흐름 폭","풍압 밀도","부분 바람 능선 발광","차징 폭풍 속도","아오·아카 국소 영향","발사 후 소멸 시간","발사 방향 쓸림 거리"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. 풍압의 중심은 재료 구체가 아니라 시전자입니다. PairPolish 동작은 그대로 두고, 발사 후 시전자 자리에서 소멸합니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
