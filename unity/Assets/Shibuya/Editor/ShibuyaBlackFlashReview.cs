using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaBlackFlashReview : EditorWindow
    {
        int seed=71;Vector3 offset=new Vector3(0,1,0);ShibuyaBlackFlashPresentation presentation;
        [MenuItem("JJK Game/시부야 PASS4H/흑섬 VFX 미리보기 (무피해)")]
        public static void Open()=>GetWindow<ShibuyaBlackFlashReview>("흑섬 본체 검수");
        void OnGUI()
        {
            EditorGUILayout.HelpBox("Play Mode 전용 무피해 미리보기입니다. 주력·Zone·피해·확률을 변경하지 않습니다. 실제 연동은 PASS4G 공격 등급·반격 검수에서 별도로 확인하세요.",MessageType.Info);
            if(!Application.isPlaying){EditorGUILayout.LabelField("ShibuyaCombatMVP를 Play 해 주세요.");return;}
            if(presentation==null)presentation=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();
            if(presentation==null)return;
            EditorGUILayout.ObjectField("흑섬 설정",presentation.Profile,typeof(ShibuyaBlackFlashProfile),false);
            seed=EditorGUILayout.IntField("형태 시드",seed);offset=EditorGUILayout.Vector3Field("더미 기준 위치 보정",offset);
            if(GUILayout.Button("더미 위치에 흑섬 본체 재생 (무피해)")){
                var match=presentation.GetComponent<ShibuyaMatch>();if(match!=null) presentation.Preview(match.Dummy.transform.position+offset,match.Dummy.transform.position-match.Player.transform.position,seed);
            }
            EditorGUILayout.LabelField("실제 유효 흑섬 / 중복 거부",presentation.AcceptedCount+" / "+presentation.DuplicateCount);
            EditorGUILayout.LabelField("활성 / 풀 / 잔류 교체",presentation.ActiveCount+" / "+presentation.Capacity+" / "+presentation.RecycledLiveCount);
        }
    }
}
