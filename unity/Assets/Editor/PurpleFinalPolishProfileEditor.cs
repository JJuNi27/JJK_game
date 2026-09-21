using System.Collections.Generic;
using JJKGame.Player;
using UnityEditor;
using UnityEngine;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleFinalPolishProfile))]
    public sealed class PurpleFinalPolishProfileEditor : Editor
    {
        private static readonly Dictionary<string,string> Labels=new()
        {
            ["outerPolishEnabled"]="외부 폴리시 후보 사용",["fusionBirthEnabled"]="합체 탄생 강조 후보 사용",
            ["coronaContrast"]="코로나 대비",["compressionStrength"]="합체 압축 강조",["birthStrength"]="탄생 강조 밝기",["birthDuration"]="탄생 여운 (초)",
            ["haloEnabled"]="아우라 표시",["lightningEnabled"]="외향 번개 표시",["debrisEnabled"]="파편 표시",["distortionEnabled"]="국소 왜곡 표시",
            ["residualSeconds"]="이동 잔류 시간 (초)",["velocityInheritance"]="속도 상속 비율",["maxResidualRadius"]="잔류 거리 제한 (반경 배율)",
            ["haloRatio"]="아우라 지름 배율",["haloIntensity"]="아우라 밝기",["sparkCount"]="작은 파편 최대 수",["sparkSpeed"]="파편 외향 속도",
            ["sparkEmission"]="파편 발광",["heroFragments"]="주요 파편 수",["heroFragmentScale"]="주요 파편 크기",["arcReach"]="외향 번개 도달 배율",
            ["arcLifetime"]="번개 수명 (초)",["arcWidth"]="외향 번개 두께",["arcEmission"]="외향 번개 발광",["distortionRatio"]="왜곡장 지름 배율",["distortionStrength"]="국소 왜곡 강도"
        };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본값은 두 후보 모두 끔입니다. 새 시전부터 적용됩니다. 원본 Production 자산과 본체는 보존됩니다. 사용자 시각 승인 대기 중입니다.",MessageType.Info);
            foreach(var field in new[]{"outerPolishEnabled","fusionBirthEnabled","coronaContrast","compressionStrength","birthStrength","birthDuration"})
                EditorGUILayout.PropertyField(serializedObject.FindProperty(field),new GUIContent(Labels[field]));
            var group=serializedObject.FindProperty("outer");group.isExpanded=EditorGUILayout.Foldout(group.isExpanded,"외부 효과 후보 설정",true);
            if(group.isExpanded)
            using(new EditorGUI.IndentLevelScope())
            {
                var end=group.GetEndProperty();var p=group.Copy();p.NextVisible(true);
                while(!SerializedProperty.EqualContents(p,end))
                {
                    // Travel/geometry limits remain identical to baseline in this comparison.
                    bool locked=p.name=="residualSeconds" || p.name=="velocityInheritance" || p.name=="maxResidualRadius";
                    using(new EditorGUI.DisabledScope(locked))EditorGUILayout.PropertyField(p,new GUIContent(Labels[p.name]),true);
                    if(!p.NextVisible(false))break;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
