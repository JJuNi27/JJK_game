using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaGuardProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaGuardInspector : UnityEditor.Editor
    {
        public static readonly Dictionary<string,string> Labels=new Dictionary<string,string>{
            {"parry","우클릭 타이밍 패링 설정"},
            {"guardEnabled","공통 가드 사용"},{"shieldShader","전용 방어막 셰이더"},{"maximumGuard","최대 가드 체력"},
            {"halfAngle","정면 방어 반각 (도)"},{"movementMultiplier","가드 중 걷기 속도 배율"},
            {"recoveryDelay","가드 종료 후 회복 대기 (초)"},{"recoveryPerSecond","초당 가드 체력 회복"},{"breakLockDuration","가드 파괴 경직·재가드 잠금 (초)"},
            {"fallbackGuardDamage","타격 데이터 없는 일반 체술의 가드 피해"},{"stableThreshold","안정 상태의 최소 체력 비율"},{"criticalThreshold","위험 상태의 최대 체력 비율"},
            {"stableColor","안정 방어막 색·투명도"},{"strainedColor","중간 방어막 색·투명도"},{"criticalColor","위험 방어막 색·투명도"},
            {"impactStrength","방어 충돌 흔들림·파동 강도"},{"criticalInstability","위험 상태 깜빡임·흔들림"},{"crackStrength","위험 상태 균열 강도"},
            {"impactDuration","방어 충돌 반응 시간"},{"breakVisualDuration","파괴 효과 지속시간"},{"breakVisualStrength","파괴 효과 확산 강도"},
            {"shieldRadius","방어막 수평 크기"},{"shieldHeight","방어막 높이"},{"shieldCenterHeight","방어막 중심 높이"},
            {"guardArmAngle","블록 캐릭터 방어 팔 각도"},{"breakLeanAngle","가드 파괴 자세 기울기"}
        };
        public override void OnInspectorGUI(){
            serializedObject.UpdateIfRequiredOrScript();var it=serializedObject.GetIterator();bool child=true;
            while(it.NextVisible(child)){child=false;using(new EditorGUI.DisabledScope(it.name=="m_Script"))
                EditorGUILayout.PropertyField(it,new GUIContent(it.name=="m_Script"?"스크립트":Labels[it.name]),true);}
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("가드 체력은 실제 게임 HUD에 표시하지 않습니다. 위험 구간은 안정 기준보다 작게 설정하세요. F 주력 아우라와 별도 표현입니다.",MessageType.Info);
        }
    }
}
