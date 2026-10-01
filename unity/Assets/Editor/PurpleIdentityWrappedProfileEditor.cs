using JJKGame.Player;
using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIdentityWrappedProfile))]
    public sealed class PurpleIdentityWrappedProfileEditor : Editor
    {
        private static readonly string[] Fields={"candidateEnabled","edgeMotionScale","rimGain","plumeGain","reach","eventRate","sparkGain","arcGain"};
        private static readonly string[] Labels={"외곽 에너지 후보 활성화","구체 가장자리 움직임","경계 발광 강도","외곽 파열 강도","파열 도달 거리","파열 발생 빈도","불꽃 발광 배율","번개 발광 배율"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. 구체의 크기와 이동 경로는 유지하고, 충전과 이동 중의 외곽 에너지만 강화합니다. 종점 폭발은 기존 표현을 사용합니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)
                EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
