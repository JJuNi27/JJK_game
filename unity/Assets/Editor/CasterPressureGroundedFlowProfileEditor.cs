using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(CasterPressureGroundedFlowProfile))]
    public sealed class CasterPressureGroundedFlowProfileEditor : Editor
    {
        static readonly string[] Fields={"candidateEnabled","path","breadth","depth","density","broadSeconds","innerSeconds","crestSeconds","crestEmission","fusionDensityBoost","releaseSeconds","forwardSweep","fallbackFootOffset"};
        static readonly string[] Labels={"하체 풍압 후보 활성화","발 높이 기준 흐름 경로","넓은 압력 폭","압력 앞뒤 깊이","기본 밀도","큰 흐름 교체 주기","내부 압축 교체 주기","국소 능선 교체 주기","능선 발광","융합 구간 밀도 강화","발사 후 소멸 시간","전방 쓸림 거리","콜라이더가 없을 때 발 높이 보정"};
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. 이전 ONE FLOW는 보존합니다. 경로 높이는 캐릭터 중심이 아니라 발 높이 기준입니다.",MessageType.Info);
            for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]),true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
