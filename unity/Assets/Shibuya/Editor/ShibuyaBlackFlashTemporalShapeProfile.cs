using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    // Editor-only experiment settings. Never referenced by a combat profile or scene.
    public sealed class ShibuyaBlackFlashTemporalShapeProfile : ScriptableObject
    {
        [Range(.03f,.2f)] public float fastGrowth=.03f;
        [Range(1,3)] public float mainWidth=2.1f;
        [Range(.1f,.6f)] public float disconnectedStart=.35f;
        [Range(1,2.5f)] public float fragmentWidth=1.6f;
        [Range(0,.4f)] public float edgeIrregularity=.22f;
    }
    [CustomEditor(typeof(ShibuyaBlackFlashTemporalShapeProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaBlackFlashTemporalShapeInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            string[] fields={"fastGrowth","mainWidth","disconnectedStart","fragmentWidth","edgeIrregularity"};
            string[] labels={"T·TS 성장 시간 (초)","S·TS 주 파열 획 너비 배율","S·TS 작은 획 시작 절단 비율","S·TS 분리 파편 너비 배율","S·TS 외곽 불규칙 강도"};
            EditorGUILayout.HelpBox("QA 전용입니다. 실제 REV1과 지류의 절대 시작 지연은 변경하지 않습니다.",MessageType.Info);
            for(int i=0;i<fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i]),new GUIContent(labels[i]),true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
