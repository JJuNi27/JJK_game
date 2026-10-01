using JJKGame.Player;
using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    [CustomEditor(typeof(PurpleExplosionTurbulenceProfile))]
    public sealed class PurpleExplosionTurbulenceProfileEditor : Editor
    {
        private static readonly string[] Fields = { "candidateEnabled", "energyGain", "flowSpeed",
            "silhouetteBreakup", "domainWarp", "darkDepth", "coreRadiance", "coreRadius", "density" };
        private static readonly string[] Labels = { "폭발 본체 후보 활성화", "에너지 강도 배율", "난류 흐름 속도",
            "비대칭 실루엣 파열", "내부 흐름 뒤틀림", "암부 깊이", "고온 중심 발광", "고온 중심 범위", "질량 밀도" };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("기본 OFF. Purple 형성/완성 hold 본체에만 적용하며 Travel·종점 폭발·외부 번개·수렴 연출은 보존합니다.", MessageType.Info);
            for (int i = 0; i < Fields.Length; i++)
                EditorGUILayout.PropertyField(serializedObject.FindProperty(Fields[i]), new GUIContent(Labels[i]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
