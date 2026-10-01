using JJKGame.Player;
using UnityEditor;
using UnityEngine;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleConvergenceDiagnosticProfile))]
    public sealed class PurpleConvergenceDiagnosticProfileEditor:Editor
    {
        private static readonly string[] Fields={"candidateEnabled","flashFrames","peakFrames","flashStrength","radialRadius","hotCoreRadius","holdDuration","hitStopScale","catchupDuration","impulseDuration","impulseDistance","impulseRoll","pulseDuration","bloomIntensity","exposure"};
        private static readonly string[] Labels={"강도 진단 후보 활성화","섬광 전체 길이 (60Hz 프레임)","최대 섬광 길이 (60Hz 프레임)","방사형 섬광 세기","주변 광채 반경","고온 중심 반경","접촉 정지 시간 (실시간 초)","정지 중 시간 배율","기존 연출 따라잡기 시간","카메라 충격 길이","카메라 충격 거리","카메라 충격 기울기 (도)","화면 밝기 펄스 길이","블룸 최대 강도","노출 최대 보정 (스톱)"};
        public override void OnInspectorGUI(){serializedObject.Update();EditorGUILayout.HelpBox("기본 OFF. 기존 수렴 후보를 보존한 강한 진단 후보입니다. 사용자 시각 승인 전 Production에 적용하지 않습니다.",MessageType.Info);for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));serializedObject.ApplyModifiedProperties();}
    }
}
