using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaQuickRecoveryTestWindow : EditorWindow
    {
        private static ShibuyaFighter player,attacker;
        private static ShibuyaMovesetDefinition original,temporary;
        private static GameObject wall;
        private static bool pending,armed,followed;
        private static double scheduledAt;
        private static int mode;
        [MenuItem("JJK Game/시부야 PASS4E/긴급 탈출 검수 도구")]
        public static void Open()=>GetWindow<ShibuyaQuickRecoveryTestWindow>("긴급 탈출 검수");
        private void OnEnable(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        private void OnDisable(){EditorApplication.update-=Tick;Cleanup();}
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Play Mode 전용. 예약 후 Game View를 클릭하고 다운을 기다리세요. 착지 후 0.45초 이전 Space는 무시됩니다. 0.45~0.85초에 직접 WASD+새 Space, 또는 아무 입력 없이 자연 기상을 확인하세요. 락온은 MMB로 직접 선택합니다. 검수용 실제 다운 공격은 HP 1 피해이며 저장 데이터는 변경하지 않습니다.",MessageType.Info);
            using(new EditorGUI.DisabledScope(!EditorApplication.isPlaying)){
                string[] names={"기본 다운 / 최소 대기·자연 기상·락온 검수","탈출 초반 실제 체술 (무적 구간)","탈출 후반 실제 체술 (무적 종료 후)","탈출 중 실제 잡기 시도","벽 근처 다운 (카메라를 기본 방향으로 두고 D+Space)"};
                for(int i=0;i<names.Length;i++)if(GUILayout.Button(names[i]+" — 2초 후 준비"))Schedule(i);
                if(GUILayout.Button("검수 중단 / 임시 공격·벽 정리"))Cleanup();
            }
            if(player!=null)EditorGUILayout.LabelField("개발용 상태",player.IsQuickRecovering?"방향 기상 중":player.CanRequestDownRecovery?"새 방향+Space 입력 가능":player.IsDowned?"다운 대기/기상 중":"정상 전투");
        }
        private static void Schedule(int selected){Cleanup();mode=selected;pending=true;scheduledAt=EditorApplication.timeSinceStartup+2;}
        private static void Place(ShibuyaFighter f,Vector3 p){var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=p;cc.enabled=true;Physics.SyncTransforms();}
        private static void RestoreAttack(){if(attacker!=null&&temporary!=null)attacker.SetMoveset(original);if(temporary!=null)Object.DestroyImmediate(temporary);temporary=null;}
        private static void Cleanup(){pending=armed=followed=false;RestoreAttack();if(attacker!=null)attacker.SetGuardHeld(false);if(wall!=null)Object.DestroyImmediate(wall);wall=null;player=attacker=null;original=null;}
        private static bool Attack(bool down)
        {
            RestoreAttack();
            Place(attacker,player.transform.position+player.transform.forward*1.4f);attacker.transform.rotation=Quaternion.LookRotation(-player.transform.forward);
            var step=down?JsonUtility.FromJson<ShibuyaMovesetStep>(JsonUtility.ToJson(attacker.Profile.finishers.downFinisher))
                :new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=JsonUtility.FromJson<ShibuyaStrikeDefinition>(JsonUtility.ToJson(attacker.Profile.strikes[0]))}}};
            foreach(var h in step.hits){h.strike.damage=1;h.strike.advanceDistance=0;h.strike.knockbackSpeed=0;h.strike.hitStopDuration=0;h.strike.startup=down?.1f:.01f;h.cinematic=null;}
            temporary=CreateInstance<ShibuyaMovesetDefinition>();temporary.name="PASS4E development hit (temporary)";temporary.steps=new[]{step};
            return attacker.SetMoveset(temporary)&&attacker.TryAttack();
        }
        private static void Tick()
        {
            if(!EditorApplication.isPlaying){Cleanup();return;}
            if(pending&&EditorApplication.timeSinceStartup>=scheduledAt){
                pending=false;var match=Object.FindFirstObjectByType<ShibuyaMatch>();if(match==null)return;
                player=match.Player;attacker=match.Dummy;original=attacker.Moveset;
                if(player.Health.IsDead||attacker.Health.IsDead||player.IsDowned||player.IsQuickRecovering||!Attack(true)){Cleanup();return;}
                if(mode==4){wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="PASS4E temporary validation wall";wall.transform.position=player.transform.position+new Vector3(1.2f,1,0);wall.transform.localScale=new Vector3(.2f,2,6);}
                armed=true;
            }
            if(!armed)return;
            if(player==null||attacker==null||player.Health.IsDead||attacker.Health.IsDead||player.State==ShibuyaFighterState.Finished){Cleanup();return;}
            if(temporary!=null&&attacker.ComboStep==0)RestoreAttack();
            if(!followed&&player.IsQuickRecovering&&player.QuickRecoveryElapsed>=(mode==2?.20f:.06f)){
                followed=true;
                if(mode==1||mode==2)Attack(false);
                if(mode==3){Place(attacker,player.transform.position+player.transform.forward*1.4f);attacker.transform.rotation=Quaternion.LookRotation(-player.transform.forward);attacker.SetGuardHeld(false);attacker.SetGuardHeld(true);attacker.TryGrab();}
            }
        }
    }
}
