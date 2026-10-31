using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaAerialCombatDefinition)), CanEditMultipleObjects]
    public sealed class ShibuyaAerialCombatInspector : ShibuyaInspector
    {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("일반 점프와 어퍼컷 추격 후 LMB는 같은 공중 체술을 사용합니다. 공격 중 체공하며, 새 클릭 연계는 입력 창과 실시간 총 체공 상한 안에서만 가능합니다. 도착만으로 공격하지 않습니다. 피격자 높이는 유효 적중·경직이 이어질 때만 유지합니다. 체공 종료 후 현재 위치에서 자연 낙하합니다.",MessageType.Info);
            if(!((ShibuyaAerialCombatDefinition)target).IsValid) EditorGUILayout.HelpBox("추격 수치 또는 공중 체술 이벤트가 유효하지 않습니다. 공중 체술의 피격 결과는 일반 피격만 허용합니다.",MessageType.Error);
        }
    }
    [CustomEditor(typeof(ShibuyaMovesetDefinition)), CanEditMultipleObjects]
    public sealed class ShibuyaMovesetInspector : ShibuyaInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("기본 4타는 전투 설정에 보존됩니다. 이 자산을 Fighter에 연결한 경우에만 사용합니다. 타격 발동 시간은 준비 시작이며 실제 판정은 발동+준비 시간입니다. 타격은 실제 활성 시작 순서로 입력하세요. 작성 전진은 이동 클립, 자동 접근 보정은 공통/타격별 추적 설정을 사용합니다. 도착 필수 접근과 자동 보정 예산은 중복되지 않습니다.", MessageType.Info);
            if (!((ShibuyaMovesetDefinition)target).IsValid)
                EditorGUILayout.HelpBox("비어 있는 단계, 잘못된 시간순서 또는 이동 참조가 있습니다. 실행할 수 없습니다.", MessageType.Error);
        }
    }

    [CustomEditor(typeof(ShibuyaFinisherSet)), CanEditMultipleObjects]
    public sealed class ShibuyaFinisherSetInspector : ShibuyaInspector
    {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("기본 마무리는 기존 4타 설정을 사용합니다. 이 자산은 어퍼컷과 다운의 대체 마지막 단계만 소유합니다. 입력 선택은 클릭 접수 시 고정되며, 자연 기상은 최소 다운 시간보다 빠르지 않습니다.", MessageType.Info);
            EditorGUILayout.HelpBox("띄우기의 기본 수평 운동은 공중 상태의 발사 거리·시간·낙하 속도가 소유합니다. 이 경우 감쇠 넉백 속도는 사용하지 않아 감춥니다. 거리·시간 넉백을 별도로 작성하면 추가 이동 예산으로 적용됩니다.", MessageType.Info);
            if (!((ShibuyaFinisherSet)target).IsValid) EditorGUILayout.HelpBox("타격/이동/상태 설정이 유효하지 않습니다.", MessageType.Error);
        }
    }

    [CustomPropertyDrawer(typeof(ShibuyaMovesetStep))]
    [CustomPropertyDrawer(typeof(ShibuyaCinematicShot))]
    [CustomPropertyDrawer(typeof(ShibuyaMovementDefinition))]
    [CustomPropertyDrawer(typeof(ShibuyaHitEvent))]
    [CustomPropertyDrawer(typeof(ShibuyaAirSettings))]
    [CustomPropertyDrawer(typeof(ShibuyaDownSettings))]
    [CustomPropertyDrawer(typeof(ShibuyaTargetingSettings))]
    [CustomPropertyDrawer(typeof(ShibuyaAttackMetadata))]
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
