using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaMovesetDefinition)), CanEditMultipleObjects]
    public sealed class ShibuyaMovesetInspector : ShibuyaInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("기본 4타는 전투 설정에 보존됩니다. 이 자산을 Fighter에 연결한 경우에만 사용합니다. 타격 발동 시간은 준비 시작이며 실제 판정은 발동+준비 시간입니다. 타격은 실제 활성 시작 순서로 입력하세요. 새 무브셋의 실제 전진은 이동 클립만 사용합니다.", MessageType.Info);
            if (!((ShibuyaMovesetDefinition)target).IsValid)
                EditorGUILayout.HelpBox("비어 있는 단계, 잘못된 시간순서 또는 이동 참조가 있습니다. 실행할 수 없습니다.", MessageType.Error);
        }
    }

    [CustomPropertyDrawer(typeof(ShibuyaMovesetStep))]
    [CustomPropertyDrawer(typeof(ShibuyaMovementDefinition))]
    [CustomPropertyDrawer(typeof(ShibuyaHitEvent))]
    public sealed class ShibuyaMovesetDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;
            var child = property.Copy(); var end = child.GetEndProperty();
            if (child.NextVisible(true)) do {
                if (SerializedProperty.EqualContents(child, end)) break;
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(child, true);
            } while (child.NextVisible(false));
            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
            if (property.isExpanded) {
                using (new EditorGUI.IndentLevelScope()) {
                    var child = property.Copy(); var end = child.GetEndProperty();
                    if (child.NextVisible(true)) do {
                        if (SerializedProperty.EqualContents(child, end)) break;
                        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                        position.height = EditorGUI.GetPropertyHeight(child, true);
                        EditorGUI.PropertyField(position, child, new GUIContent(ShibuyaInspector.KoreanLabel(child.name)), true);
                    } while (child.NextVisible(false));
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
