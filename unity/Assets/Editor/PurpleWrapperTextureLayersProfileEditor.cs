using UnityEditor;
using UnityEngine;
using JJKGame.Player;
[CustomEditor(typeof(PurpleWrapperTextureLayersProfile))]
public sealed class PurpleWrapperTextureLayersProfileEditor : Editor
{
    static readonly string[] Fields={"candidateEnabled","shellReach","brushWidth","brushCycle","curveCycle","windOpacity","windEmission","releaseWindSeconds","travelWindStrength","darkDepth","darkWidth","darkEmission","brightWidth","brightEmission","arcCycle","stormSpeed","layerOcclusion","spatialCoupling"};
    static readonly string[] Labels={"실험 후보 사용","외곽 반경 비율","브러시 폭","브러시 교체 주기","곡선 교체 주기","풍압 불투명도","풍압 발광","발사 잔류 풍압 시간","이동 풍압 강도","암부 깊이","암부 폭","암부 보라 발광","번개 폭","번개 발광","번개 교체 주기","전체 움직임 속도","레이어 가림 강도","공간 연결 강도"};
    public override void OnInspectorGUI()
    {
        serializedObject.Update();EditorGUILayout.HelpBox("저장 기본 OFF. 발사체 주변 wrapper만 실험하며 구체·게임플레이·기존 후보를 유지합니다.",MessageType.Info);
        for(int i=0;i<Fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]),new GUIContent(Labels[i]),true);
        serializedObject.ApplyModifiedProperties();
    }
}
