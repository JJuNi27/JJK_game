using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    // Explicit development setup only. Does not change any saved scene, input binding, or profile.
    public sealed class ShibuyaGrabTestWindow : EditorWindow
    {
        private static ShibuyaFighter player,dummy;
        private static int scenario;
        private static bool pending,armed,reacted;
        private static double setupAt;
        [MenuItem("JJK Game/시부야 PASS4D/잡기 검수 도구")]
        public static void Open()=>GetWindow<ShibuyaGrabTestWindow>("잡기 검수 (개발용)");
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Play Mode에서 상황을 선택하고 Game View를 클릭하세요. 2초 후 더미를 몸 전방에 배치합니다. 직접 우클릭 홀드+새 좌클릭으로 잡으세요. 패링/회피는 실제 입력·전투 경로를 사용하며 성공을 강제하지 않습니다. 더미 이동은 검수 준비에만 사용합니다.",MessageType.Info);
            using(new EditorGUI.DisabledScope(!EditorApplication.isPlaying)){
                string[] names={"일반 대상","일반 가드 대상","패링 창 대상","사거리 밖 (3m)","측면 회피 대상","후방 회피 대상","준비 중 더미 반격"};
                for(int i=0;i<names.Length;i++)if(GUILayout.Button(names[i]+" 준비 (2초 후)"))Schedule(i);
                if(GUILayout.Button("검수 중단 / 더미 입력 해제"))Cleanup();
            }
            EditorGUILayout.LabelField(pending?"예약 중 — Game View로 돌아가세요.":armed?"준비 완료 — 직접 잡기를 입력하세요.":"검수 대기");
            if(player!=null)EditorGUILayout.LabelField("개발용 결과",$"시도 {player.GrabAttemptCount} / 성공 {player.GrabSuccessCount} / 실패 {player.GrabFailureCount}");
        }
        private void OnDisable(){EditorApplication.update-=Tick;Cleanup();}
        private void OnEnable(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        private static void Schedule(int selected){Cleanup();scenario=selected;pending=true;setupAt=EditorApplication.timeSinceStartup+2;}
        private static void Place(ShibuyaFighter f,Vector3 position){var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=position;cc.enabled=true;Physics.SyncTransforms();}
        private static void Cleanup(){pending=armed=reacted=false;if(dummy!=null)dummy.SetGuardHeld(false);player=dummy=null;}
        private static void Tick()
        {
            if(!EditorApplication.isPlaying){Cleanup();return;}
            if(pending&&EditorApplication.timeSinceStartup>=setupAt){
                pending=false;var match=Object.FindFirstObjectByType<ShibuyaMatch>();if(match==null)return;
                player=match.Player;dummy=match.Dummy;
                if(player.IsGrabBusy||dummy.IsGrabBusy||player.Health.IsDead||dummy.Health.IsDead){Cleanup();return;}
                Place(dummy,player.transform.position+player.transform.forward*(scenario==3?3:1.4f));
                dummy.transform.rotation=Quaternion.LookRotation(-player.transform.forward);dummy.SetGuardHeld(false);
                if(scenario==1)dummy.SetGuardHeld(true);armed=true;
            }
            if(!armed)return;
            if(player==null||dummy==null||player.Health.IsDead||dummy.Health.IsDead||player.State==ShibuyaFighterState.Finished){Cleanup();return;}
            if(!reacted&&player.GrabPhase==ShibuyaGrabPhase.Startup){
                float at=scenario==2||scenario==4||scenario==5?.13f:0;
                if(player.GrabElapsed>=at){
                    reacted=true;
                    if(scenario==2)dummy.SetGuardHeld(true);
                    if(scenario==4||scenario==5)dummy.TryGroundDash(scenario==4?Vector2.right:Vector2.down,player.transform.forward,player.transform.right);
                    if(scenario==6)dummy.TryAttack();
                }
            }
            if(reacted&&!player.IsGrabBusy){dummy.SetGuardHeld(false);armed=false;}
        }
    }
}
