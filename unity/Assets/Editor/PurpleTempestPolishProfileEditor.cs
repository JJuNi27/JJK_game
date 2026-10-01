using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleTempestPolishProfile))]
    public sealed class PurpleTempestPolishProfileEditor : Editor
    {
        static readonly string[] Fields={"candidateEnabled","windReach","windWidth","windSpeed","windOpacity","releaseWindSeconds","travelWindStrength","brightWidth","brightEmission","darkWidth","darkEmission","stormSpeed","dischargeReach","spatialCoupling"};
        static readonly string[] Labels={"폭풍 폴리시 후보 활성화","차징 풍압 범위 (구 반경 배수)","차징 풍압 폭","풍압 회전 속도","풍압 불투명도","발사 위치 풍압 소멸 시간 (초)","비행 방향 풍압 강도","네온 방전 폭","네온 방전 발광","검보라 주력 폭","검보라 주력 발광","폭풍 변화 속도","방전 범위","세 층 공간 연결 강도"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. 기존 Tempest보다 우선하는 별도 후보입니다. 구체는 유지하며, 차징 풍압은 발사 위치에 남아 소멸하고 비행 풍압은 진행 방향을 따릅니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
