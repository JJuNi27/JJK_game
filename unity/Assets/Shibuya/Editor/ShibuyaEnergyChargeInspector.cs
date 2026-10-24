using JJKGame.Core;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaEnergyCharge))]
    public sealed class ShibuyaEnergyChargeInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var charge = (ShibuyaEnergyCharge)target;
            var fighter = charge.GetComponent<ShibuyaFighter>();
            EditorGUILayout.HelpBox("수치는 캐릭터의 전투 프로필 → 주력 능력치 Data Asset에서 설정합니다. 현재 주력은 아우라 크기에 영향을 주지 않습니다.", MessageType.Info);
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.ObjectField("주력 능력치 원본", fighter.Profile != null ? fighter.Profile.cursedEnergy : null, typeof(CursedEnergyProfile), false);
                if (charge.Resource != null) {
                    EditorGUILayout.FloatField("현재 주력량", charge.Resource.CurrentEnergy);
                    EditorGUILayout.FloatField("최대 주력량", charge.Resource.MaxEnergy);
                }
                EditorGUILayout.Toggle("충전 중", charge.IsCharging);
                EditorGUILayout.FloatField("연속 홀드 시간 (초)", charge.HoldDuration);
            }
            if (Application.isPlaying) Repaint();
        }
    }
}
