using UnityEditor;
using UnityEngine;
using JJKGame.Player;
namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleIngredientReboot2Profile))]
    public sealed class PurpleIngredientReboot2ProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("리부트의 어두운 본체는 유지하고 외곽 에너지·융합 수직 궤적·첫 접촉의 국소 흑백 충격만 바꾸는 후보입니다. 새 시전부터 적용되며 기존 후보보다 우선합니다.",MessageType.Info);
            var fields=new[]{"candidateEnabled","energyReach","bandWidth","collapseSpeed","pressureSpeed","ruptureDuration","fusionVerticalArc","collisionFrames","collisionContrast"};
            var labels=new[]{"리부트 2차 후보 사용","외곽 에너지 반경","찢어진 압력층 두께","아오 수축 속도","아카 팽창 속도","짧은 방전 지속 시간","융합 수직 굴곡 높이","충돌 프레임 수 (60Hz)","충돌 흑백 대비"};
            for(int i=0;i<fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i]),new GUIContent(labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
