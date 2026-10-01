using JJKGame.Player;
using UnityEditor;
using UnityEngine;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurplePressureStormProfile))]
    public sealed class PurplePressureStormProfileEditor:Editor
    {
        private static readonly string[] Fields={"candidateEnabled","flowLanes","pressureReach","flowWidth","flowSpeed","pressureOpacity","pressureEmission","lightningWidth","lightningEmission","dischargeReach"};
        private static readonly string[] Labels={"풍압 폭풍 후보 활성화","압력 흐름 수","압력장 범위 (구 반경 배수)","흐름 폭","흐름 속도","압력층 불투명도","압력층 발광","번개 폭 (구 반경 배수)","번개 발광","경계 방전 도달 거리"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. 현재 구체는 그대로 두고 외곽 풍압과 경계 방전을 교체합니다. 비교 기준은 Wrapped 후보이며, 이 프로필은 본체 설정을 변경하지 않습니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
