using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaBlackFlashArtProfile : ScriptableObject
    {
        public Texture2D atlas;
        public int[] poseOrder={1,0,2,3};
        [Min(.5f)] public float worldSize=5;
        [Range(0,1)] public float curvature=.45f;
        [Range(.5f,1)] public float sideScale=.86f;
        [Min(.001f)] public float firstEnd=.035f,peakEnd=.105f,tearEnd=.19f,duration=.3f;
        [ColorUsage(false,true)] public Color red=new Color(5,.012f,.008f,1);
        [Range(1,8)] public float edgePixels=2.4f;
        [Range(3,16)] public float haloPixels=7;
        [Range(0,1)] public float edgeCoverage=.72f;
        public bool discharges=true;
    }
    [CustomEditor(typeof(ShibuyaBlackFlashArtProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaBlackFlashArtInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            string[] fields={"atlas","worldSize","curvature","sideScale","firstEnd","peakEnd","tearEnd","duration","red","edgePixels","haloPixels","edgeCoverage","discharges"};
            string[] labels={"직접 제작한 파열 키 포즈 아틀라스","본체 캔버스 크기 (m)","곡면 깊이 (m)","측면 보조층 크기","최초 파열 종료 (초)","최대 파열 종료 (초)","윤곽 변화 종료 (초)","전체 길이 (초)","붉은 외곽 HDR 색상","선명한 붉은 경계 폭 (픽셀)","붉은 잔광 폭 (픽셀)","외곽 발광 분포","독립 붉은 방전 사용"};
            EditorGUILayout.HelpBox("L2/L3 QA 전용입니다. 실제 전투 REV1과 흑섬 판정에는 연결되지 않습니다.",MessageType.Info);
            for(int i=0;i<fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i]),new GUIContent(labels[i]),true);
            var order=serializedObject.FindProperty("poseOrder");if(order.arraySize!=4)order.arraySize=4;
            string[] poses={"최초 파열 원화 번호","최대 파열 원화 번호","윤곽 변화 원화 번호","해체 원화 번호"};
            for(int i=0;i<4;i++)EditorGUILayout.IntSlider(order.GetArrayElementAtIndex(i),0,3,new GUIContent(poses[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
