using System;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    public static class ShibuyaAerialDataBuilder
    {
        public const string Path="Assets/Shibuya/Data/Shibuya Aerial Combat.asset";
        public static void CreateAndConnect()
        {
            if(EditorApplication.isPlaying || UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Play Mode 종료 및 씬 저장 상태 확인이 필요합니다.");
            var profile=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>("Assets/Shibuya/Data/Shibuya Combat Profile.asset");
            var data=AssetDatabase.LoadAssetAtPath<ShibuyaAerialCombatDefinition>(Path);
            if(EditorUtility.IsDirty(profile) || (data!=null && EditorUtility.IsDirty(data)))
                throw new InvalidOperationException("미저장 전투 설정을 먼저 보존하세요.");
            if(profile.aerialCombat!=null && profile.aerialCombat!=data) throw new InvalidOperationException("기존 공중 설정을 덮어쓸 수 없습니다.");
            if(data==null) {data=Create();AssetDatabase.CreateAsset(data,Path);}
            var serialized=new SerializedObject(profile);serialized.FindProperty("aerialCombat").objectReferenceValue=data;
            serialized.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(profile);
        }
        public static ShibuyaAerialCombatDefinition Create()
        {
            var data=ScriptableObject.CreateInstance<ShibuyaAerialCombatDefinition>();
            data.aerialAttack=new ShibuyaMovesetStep {hits=new[]{Hit(0,12,ShibuyaStrikeMotion.RightJab),Hit(.16f,13,ShibuyaStrikeMotion.LeftJab)}};
            return data;
        }
        private static ShibuyaHitEvent Hit(float time,float damage,ShibuyaStrikeMotion motion) => new ShibuyaHitEvent {
            startTime=time,strike=new ShibuyaStrikeDefinition {damage=damage,startup=.08f,activeTime=.08f,recovery=.14f,
                attackForwardOffset=.95f,attackRadius=.9f,attackHeight=1f,hitStun=.16f,knockbackSpeed=1.2f,
                alignVictimOnHit=false,motion=motion,hitStopDuration=.035f,soundPitch=.95f,advanceDistance=0}
        };
    }
}
