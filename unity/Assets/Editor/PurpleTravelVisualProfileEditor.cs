using System.Collections.Generic;
using JJKGame.Player;
using UnityEditor;
using UnityEngine;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleTravelVisualProfile))]
    public sealed class PurpleTravelVisualProfileEditor : Editor
    {
        private static readonly Dictionary<string,string> Labels=new()
        {
            ["travelRefinementEnabled"]="발사 이후 이동 표현 사용",["referenceSpeed"]="표현 기준 속도",["residueLifetime"]="공간 잔류 수명 (초)",
            ["partialInheritance"]="이동 파편 속도 상속",["residueRadiusLimit"]="잔류 거리 상한 (본체 반경 배율)",
            ["wakeLengthRadii"]="뒤쪽 웨이크 길이 (본체 반경 배율)",["wakeWidthRadii"]="웨이크 폭 반경 (본체 반경 배율)",
            ["wakeEmission"]="웨이크 발광",["releaseBoost"]="발사 순간 강화",["releasePeakSeconds"]="발사 강화 감쇠 시간 (초)",
            ["haloRearStretch"]="이동 아우라 뒤쪽 변형",["rearDistortion"]="뒤쪽 국소 왜곡 강조"
        };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();EditorGUILayout.HelpBox("Release 이후 외부 표현만 조절합니다. 본체·Charge·게임플레이·Terminal에는 적용하지 않습니다.",MessageType.Info);
            var p=serializedObject.GetIterator();bool children=true;
            while(p.NextVisible(children)){children=false;if(p.name=="m_Script")continue;EditorGUILayout.PropertyField(p,new GUIContent(Labels.TryGetValue(p.name,out var label)?label:p.displayName),true);}
            serializedObject.ApplyModifiedProperties();
        }
    }
}
