using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(CasterPressure3DFlowProfile))]
    public sealed class CasterPressure3DFlowProfileEditor : Editor
    {
        static readonly string[] Fields={"candidateEnabled","path","breadth","depth","density","flowSpeed","crestEmission","releaseSeconds","forwardSweep"};
        static readonly string[] Labels={"단일 입체 풍압 후보 활성화","시전자 기준 흐름 경로","외부 압력 폭","앞뒤 깊이","압력 밀도","흐름 이동 속도","국소 능선 발광","발사 후 소멸 시간","전방 쓸림 거리"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. 전체 폭풍이 아닌 한 흐름의 입체 표현 실험입니다. 기존 후보를 대체하지 않습니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]),true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
