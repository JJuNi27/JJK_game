using JJKGame.Player;
using UnityEditor;
using UnityEngine;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleConvergenceImpactProfile))]
    public sealed class PurpleConvergenceImpactProfileEditor:Editor
    {
        private static readonly string[] Fields={"candidateEnabled","flashFrames","flashStrength","hitStopDuration","hitStopScale","impulseDuration","impulseDistance","impulseRoll"};
        private static readonly string[] Labels={"수렴 임팩트 후보 활성화","섬광 길이 (60Hz 프레임)","흰색 섬광 세기","정지 시간 (실시간 초)","정지 중 시간 배율","카메라 타격 길이","카메라 타격 거리","카메라 기울기 (도)"};
        public override void OnInspectorGUI(){serializedObject.Update();EditorGUILayout.HelpBox("기본 OFF. 첫 접촉에서만 실행하는 탐색 후보입니다. 본체와 기존 타이밍 설정은 바꾸지 않습니다.",MessageType.Info);for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));serializedObject.ApplyModifiedProperties();}
    }
}
