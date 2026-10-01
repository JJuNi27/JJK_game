using JJKGame.Player;
using UnityEditor;

namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientExpandedDischargeProfile))]
    public sealed class PurpleIngredientExpandedDischargeProfileEditor : Editor
    {
        private static readonly string[] Labels={"새 후보 활성화", "대형 방전 수", "중형 방전 수", "국소 방전 수", "대형 발생 빈도", "중형 발생 빈도", "국소 발생 빈도", "대형 최소 수명", "대형 최대 수명", "중형 최소 수명", "중형 최대 수명", "국소 최소 수명", "국소 최대 수명", "대형 도달 배율", "중형 도달 배율", "국소 도달 배율", "공간 깊이", "발광 강도", "대형 연결 재구성", "중형 연결 재구성", "국소 연결 재구성"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Purple Ingredient 확장 방전 후보", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("현재 HYBRID 방전은 보존됩니다. 이 후보는 별도 toggle로만 선택됩니다.", MessageType.Info);
            var iterator=serializedObject.GetIterator();bool enter=true;int index=0;
            while(iterator.NextVisible(enter))
            {
                enter=false;if(iterator.propertyPath=="m_Script")continue;
                EditorGUILayout.PropertyField(iterator,new UnityEngine.GUIContent(index<Labels.Length?Labels[index]:iterator.displayName),true);index++;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
