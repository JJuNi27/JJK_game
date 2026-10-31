using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    public static class ShibuyaAttackClassificationMigration
    {
        public static readonly string[] AssetNames={"Shibuya Combat Profile","Shibuya Ground Finishers","Shibuya Aerial Combat","PASS3A Knockback Approach","PASS3A Multi Hit","Shibuya Grab"};
        public static string Upgrade(bool apply)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Play Mode 종료 후 분류 업그레이드를 실행하세요.");
            var changes=new List<string>();
            foreach(string name in AssetNames){
                string path=ShibuyaSceneBuilder.Root+"/Data/"+name+".asset";var asset=AssetDatabase.LoadMainAssetAtPath(path);
                if(asset==null)throw new InvalidOperationException(path);
                var so=new SerializedObject(asset);var it=so.GetIterator();
                bool missingGrab=asset is ShibuyaGrabProfile&&!System.IO.File.ReadAllText(path).Contains("  attack:");
                while(it.NextVisible(true)){
                    if(it.type!=nameof(ShibuyaStrikeDefinition)&&it.propertyPath!="attack")continue;
                    var m=it.type==nameof(ShibuyaStrikeDefinition)?it.FindPropertyRelative("attack"):it;
                    if(m==null||m.FindPropertyRelative("reach")==null)continue;
                    var reach=m.FindPropertyRelative("reach");
                    if(reach.enumValueIndex!=0&&!missingGrab)continue;
                    changes.Add(path+":"+m.propertyPath);
                    if(!apply)continue;
                    reach.enumValueIndex=(int)ShibuyaAttackReach.Melee;
                    m.FindPropertyRelative("nature").enumValueIndex=(int)ShibuyaDamageNature.Taijutsu;
                    m.FindPropertyRelative("origin").enumValueIndex=(int)(asset is ShibuyaGrabProfile?ShibuyaAttackOrigin.Other:ShibuyaAttackOrigin.Basic);
                    m.FindPropertyRelative("criticalAllowed").boolValue=!(asset is ShibuyaGrabProfile);
                    m.FindPropertyRelative("blackFlashAllowed").boolValue=!(asset is ShibuyaGrabProfile);
                    m.FindPropertyRelative("directHybridHit").boolValue=false;
                }
                if(apply){so.ApplyModifiedPropertiesWithoutUndo();if(missingGrab)EditorUtility.SetDirty(asset);AssetDatabase.SaveAssetIfDirty(asset);}
            }
            return string.Join("\n",changes);
        }
    }
}
