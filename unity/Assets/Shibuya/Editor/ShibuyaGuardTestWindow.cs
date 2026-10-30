using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    // Editor only, opt-in. Creates no saved scene, profile, input binding or AI.
    public sealed class ShibuyaGuardTestWindow : EditorWindow
    {
        private static double scheduledAt;
        private static float angle,cost;
        private static bool unblockable;
        private static bool pending;
        private static ShibuyaFighter attacker;
        private static ShibuyaMovesetDefinition original,temporary;
        private static bool practiceLoop;
        private static ShibuyaParryPracticeCue cue;
        [MenuItem("JJK Game/시부야 PASS4D/가드 피격 검수 도구")]
        public static void Open()=>GetWindow<ShibuyaGuardTestWindow>("가드 검수 (개발용)");
        private void OnDisable(){pending=practiceLoop=false;if(cue!=null)Object.Destroy(cue);}
        [InitializeOnLoadMethod] private static void Bind(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Play Mode 전용입니다. 버튼 후 Game View를 클릭해 입력을 잡고, 새 우클릭을 공격 직전에 누르면 패링, 일찍 누르고 유지하면 일반 가드입니다. 연습 모드는 3초 예고 후 실제 타격을 반복합니다. 가드 수치는 Fighter Inspector에서만 확인합니다.",MessageType.Info);
            using(new EditorGUI.DisabledScope(!EditorApplication.isPlaying)) {
                if(GUILayout.Button(practiceLoop?"패링 반복 연습 중단":"패링 반복 연습 시작 (3초 예고)")) {
                    practiceLoop=!practiceLoop;
                    if(practiceLoop)Schedule(0,16,false);else {pending=false;if(cue!=null)Object.Destroy(cue);}
                }
            }
            using(new EditorGUI.DisabledScope(!EditorApplication.isPlaying||pending||temporary!=null)) {
                if(GUILayout.Button("2초 후 정면 일반 체술"))Schedule(0,16,false);
                if(GUILayout.Button("2초 후 측면 일반 체술"))Schedule(90,16,false);
                if(GUILayout.Button("2초 후 후방 일반 체술"))Schedule(180,16,false);
                if(GUILayout.Button("2초 후 가드 파괴 강타 (HP 피해 25)"))Schedule(0,120,false);
                if(GUILayout.Button("2초 후 방어 불가 일반 체술"))Schedule(0,16,true);
            }
            if(pending)EditorGUILayout.LabelField("공격 예약 중 — Game View로 돌아가세요.");
        }
        private static void Schedule(float direction,float guardDamage,bool bypass){
            angle=direction;cost=guardDamage;unblockable=bypass;scheduledAt=EditorApplication.timeSinceStartup+(practiceLoop?3:2);pending=true;
            var match=Object.FindFirstObjectByType<ShibuyaMatch>();
            if(match!=null){cue=match.Player.GetComponent<ShibuyaParryPracticeCue>()??match.Player.gameObject.AddComponent<ShibuyaParryPracticeCue>();}
        }
        private static void Cleanup()
        {
            if(attacker!=null)attacker.SetMoveset(original);
            if(temporary!=null)Object.DestroyImmediate(temporary);temporary=null;attacker=null;original=null;
        }
        private static void Tick()
        {
            if(!EditorApplication.isPlaying){pending=practiceLoop=false;if(temporary!=null)Cleanup();return;}
            if(temporary!=null&&attacker==null){Cleanup();pending=practiceLoop=false;return;}
            if(cue!=null&&cue.CombatEnded){
                pending=practiceLoop=false;Object.Destroy(cue);
                if(temporary!=null&&(attacker==null||attacker.ComboStep==0))Cleanup();return;
            }
            if(cue==null&&practiceLoop){pending=practiceLoop=false;}
            if(cue!=null)cue.secondsUntilHit=pending?Mathf.Max(0,(float)(scheduledAt-EditorApplication.timeSinceStartup))+(practiceLoop?.3f:.1f)
                :attacker!=null&&attacker.CurrentStrike!=null?Mathf.Max(0,attacker.CurrentStrike.Startup-attacker.AttackElapsed):-1;
            if(temporary!=null&&attacker!=null&&attacker.ComboStep==0){Cleanup();if(practiceLoop)Schedule(0,16,false);return;}
            if(!pending||EditorApplication.timeSinceStartup<scheduledAt)return;pending=false;
            var match=Object.FindFirstObjectByType<ShibuyaMatch>();if(match==null)return;
            attacker=match.Dummy;var victim=match.Player;
            if(attacker.ComboStep!=0||attacker.Health.IsDead||victim.Health.IsDead){attacker=null;practiceLoop=false;return;}
            Vector3 offset=Quaternion.AngleAxis(angle,Vector3.up)*victim.transform.forward*1.4f;
            var cc=attacker.GetComponent<CharacterController>();cc.enabled=false;attacker.transform.position=victim.transform.position+offset;cc.enabled=true;
            attacker.transform.rotation=Quaternion.LookRotation(-offset);Physics.SyncTransforms();
            original=attacker.Moveset;temporary=CreateInstance<ShibuyaMovesetDefinition>();temporary.name="PASS4D development strike (temporary)";
            var strike=JsonUtility.FromJson<ShibuyaStrikeDefinition>(JsonUtility.ToJson(attacker.Profile.strikes[0]));
            strike.damage=cost>=100?25:20;strike.guardDamage=cost;strike.guardable=!unblockable;strike.advanceDistance=0;
            strike.parryable=!unblockable;
            if(practiceLoop)strike.startup=.3f;
            temporary.steps=new[]{new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=strike}}}};
            if(!attacker.SetMoveset(temporary)||!attacker.TryAttack())Cleanup();
        }
    }
}
