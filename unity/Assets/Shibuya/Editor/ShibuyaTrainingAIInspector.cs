using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaTrainingAIProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaTrainingAIProfileInspector : UnityEditor.Editor
    {
        public static readonly Dictionary<string,string> Labels=new Dictionary<string,string>{
            {"trainingAllowed","훈련 AI 사용 허용 (자동 ON 아님)"},{"detectionRange","탐색 시작 거리"},
            {"approachStartDistance","접근 시작 거리"},{"approachStopDistance","접근 종료 거리"},
            {"decisionInterval","거리·경로 판단 갱신 간격"},{"facingTolerance","공격 전 몸 방향 허용각"},
            {"reactionDelay","정렬 후 공격 반응 지연"},{"minimumRest","공격 묶음 후 최소 휴식"},{"maximumRest","공격 묶음 후 최대 휴식"},
            {"oneHitWeight","1타 종료 가중치"},{"twoHitWeight","2타 종료 가중치"},{"threeHitWeight","3타 종료 가중치"},{"fourHitWeight","4타 종료 가중치"},
            {"useRunning","접근 시 달리기 사용"},{"maximumAttackHeightDifference","지상 공격 허용 높이 차"},
            {"useFixedSeed","QA 고정 난수 사용"},{"randomSeed","QA 난수 시드 (다음 ON부터 적용)"}
        };
        public override void OnInspectorGUI(){
            serializedObject.UpdateIfRequiredOrScript();var it=serializedObject.GetIterator();bool child=true;
            while(it.NextVisible(child)){child=false;using(new EditorGUI.DisabledScope(it.name=="m_Script"))EditorGUILayout.PropertyField(it,new GUIContent(it.name=="m_Script"?"스크립트":Labels[it.name]),true);}
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("새 Play Mode에서는 항상 수동 더미입니다. 사용 허용은 런타임 ON과 다릅니다. 가중치 합을 기준으로1~4타 묶음을 고릅니다. 기존 공격·피해·버퍼·이동 설정은 Fighter의 Data Asset을 사용합니다.",MessageType.Info);
        }
    }
    [CustomEditor(typeof(ShibuyaTrainingAI)),CanEditMultipleObjects]
    public sealed class ShibuyaTrainingAIInspector : UnityEditor.Editor
    {
        public static string StateLabel(ShibuyaTrainingAIState state)
        {
            switch(state){
                case ShibuyaTrainingAIState.Idle:return "대기/탐색";
                case ShibuyaTrainingAIState.Approach:return "접근 중";
                case ShibuyaTrainingAIState.FaceTarget:return "방향 정렬/반응 대기";
                case ShibuyaTrainingAIState.Attack:return "공격 묶음 진행 중";
                case ShibuyaTrainingAIState.Recovery:return "묶음 후 휴식";
                case ShibuyaTrainingAIState.UnableToAct:return "기존 전투 상태로 행동 불가";
                case ShibuyaTrainingAIState.Blocked:return "직선 경로 막힘";
                default:return "AI OFF";
            }
        }
        public override void OnInspectorGUI(){
            serializedObject.UpdateIfRequiredOrScript();
            using(new EditorGUI.DisabledScope(true))EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"),new GUIContent("스크립트"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("profile"),new GUIContent("훈련 AI 판단 설정"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"),new GUIContent("훈련 상대"));serializedObject.ApplyModifiedProperties();
            if(Application.isPlaying&&targets.Length==1){var ai=(ShibuyaTrainingAI)target;bool on=EditorGUILayout.Toggle("실제 런타임 AI ON",ai.Running);if(on!=ai.Running)ai.SetTrainingEnabled(on);DrawRuntime(ai);}
        }
        public static void DrawRuntime(ShibuyaTrainingAI ai)
        {
            using(new EditorGUI.DisabledScope(true)){
                EditorGUILayout.TextField("현재 판단 상태",StateLabel(ai.State));EditorGUILayout.TextField("최근 판단/거절 사유",ai.LastReason);
                EditorGUILayout.FloatField("타깃까지 거리",ai.TargetDistance);EditorGUILayout.FloatField("다음 공격까지 대기",ai.NextAttackRemaining);
                EditorGUILayout.IntField("공격 요청 횟수",ai.AttackRequestCount);EditorGUILayout.IntField("실제 시작한 타격 단계",ai.StartedAttackCount);
                EditorGUILayout.IntField("시작한 공격 묶음",ai.GroupCount);EditorGUILayout.IntField("완료한 공격 묶음",ai.CompletedGroupCount);
                EditorGUILayout.IntField("중단한 공격 묶음",ai.InterruptedGroupCount);EditorGUILayout.IntField("거절된 공격 요청",ai.RejectedRequestCount);
                EditorGUILayout.IntField("이번 묶음 목표 타수",ai.CurrentGroupLength);EditorGUILayout.IntField("이번 묶음 실제 시작 타수",ai.CurrentGroupStartedSteps);
                EditorGUILayout.IntField("현재 Fighter 공격 단계",ai.Owner!=null?ai.Owner.ComboStep:0);
            }
        }
    }
}
