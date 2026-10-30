using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaTrainingDefenseProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaTrainingDefenseInspector : UnityEditor.Editor
    {
        public static readonly Dictionary<string,string> Labels=new Dictionary<string,string>{
            {"defenseEnabled","방어 판단 사용"},{"observationInterval","실제 공격 관측 간격"},
            {"minimumReaction","최소 반응 지연"},{"maximumReaction","최대 반응 지연"},{"minimumDecisionInterval","다음 방어 판단 최소 간격"},
            {"minimumGuardHold","최소 가드 유지 시간"},{"maximumGuardHold","최대 가드 유지 시간"},
            {"guardWeight","종합 모드 가드 가중치"},{"dodgeWeight","종합 모드 회피 가중치"},{"parryWeight","종합 모드 패링 가중치"},{"noResponseWeight","종합 모드 무반응 가중치"},
            {"threatRange","공격 위협 관측 거리"},{"attackerFacingHalfAngle","공격자 방향 허용 반각"},{"dodgeBasis","AI 회피 방향 기준"},
            {"leftWeight","왼쪽 회피 가중치"},{"rightWeight","오른쪽 회피 가중치"},{"backWeight","후방 회피 가중치"},{"mixedAllowsOffense","종합 모드에서 기존 공격형 행동 허용"}
        };
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();var it=serializedObject.GetIterator();bool child=true;
            while(it.NextVisible(child)){child=false;using(new EditorGUI.DisabledScope(it.name=="m_Script"))EditorGUILayout.PropertyField(it,new GUIContent(it.name=="m_Script"?"스크립트":Labels[it.name]),true);}
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("현재 가능한 행동의 가중치 합으로 정규화합니다. 합계가 0/유효하지 않으면 무반응입니다. 회피 방향은 막힌 후보를 제외하고 정규화합니다. 고정 난수·시드는 공통 훈련 AI 프로필에서 공유하며 공격 난수와는 별도 흐름입니다. 패링 모드도 성공을 보장하지 않습니다. 새 일반 가드에도 기존 패링 창이 적용됩니다.",MessageType.Info);
        }
    }
}
