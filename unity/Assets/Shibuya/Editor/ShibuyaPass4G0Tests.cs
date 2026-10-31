using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4G0DataTests
    {
        [Test] public void ClassificationUpgradeSafeDefaultsKoreanUndoAndProtectedValues()
        {
            Assert.That(ShibuyaAttackClassificationMigration.Upgrade(false),Is.Empty);
            var p=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            Assert.That(p.strikes.Select(s=>s.damage),Is.EqualTo(new[]{20f,20,20,25}));Assert.That(p.cursedEnergy.MaxEnergy,Is.EqualTo(200));Assert.That(p.auraCapacityReference,Is.EqualTo(200));
            foreach(string name in ShibuyaAttackClassificationMigration.AssetNames){
                var a=AssetDatabase.LoadMainAssetAtPath(ShibuyaSceneBuilder.Root+"/Data/"+name+".asset");var so=new SerializedObject(a);var it=so.GetIterator();int count=0;
                while(it.NextVisible(true))if(it.type==nameof(ShibuyaStrikeDefinition)&&!it.isArray){
                    var m=it.FindPropertyRelative("attack");Assert.That(m.FindPropertyRelative("reach").enumValueIndex,Is.EqualTo(1));Assert.That(m.FindPropertyRelative("nature").enumValueIndex,Is.EqualTo(1));Assert.That(m.FindPropertyRelative("origin").enumValueIndex,Is.EqualTo(1));Assert.That(m.FindPropertyRelative("blackFlashAllowed").boolValue,Is.True);count++;
                }
                if(a is ShibuyaGrabProfile g){Assert.That(g.attack.blackFlashAllowed||g.attack.criticalAllowed,Is.False);Assert.That(g.damage,Is.EqualTo(15));}else Assert.That(count,Is.GreaterThan(0));
            }
            var unknown=new ShibuyaStrikeDefinition();Assert.That(unknown.attack.CanBlackFlash||unknown.attack.criticalAllowed,Is.False);
            foreach(var f in typeof(ShibuyaAttackMetadata).GetFields())Assert.That(ShibuyaInspector.KoreanLabel(f.Name),Is.Not.EqualTo(f.Name));
            var copy=UnityEngine.Object.Instantiate(p);try{
                var so=new SerializedObject(copy);Undo.IncrementCurrentGroup();so.FindProperty("strikes").GetArrayElementAtIndex(0).FindPropertyRelative("attack").FindPropertyRelative("blackFlashAllowed").boolValue=false;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(copy.strikes[0].attack.blackFlashAllowed,Is.True);
            }finally{Undo.ClearUndo(copy);UnityEngine.Object.DestroyImmediate(copy);}
        }
    }
    public sealed class ShibuyaPass4G0Tests : ShibuyaPass4CFixture
    {
        [UnityTest] public IEnumerator Energy200ChargeCostHudAndRepeatedF()
        {
            var charge=input.EnergyCharge;var energy=charge.Resource;Assert.That(energy.CurrentEnergy,Is.EqualTo(200));Assert.That(energy.MaxEnergy,Is.EqualTo(200));
            Assert.That(energy.TrySpend(20),Is.True);Assert.That(energy.CurrentEnergy,Is.EqualTo(180));Assert.That(energy.Normalized,Is.EqualTo(.9f));energy.SetCurrentEnergy(100);
            frame.auraHeld=true;yield return Until(()=>charge.IsCharging);yield return Wait(.2f);float duration=charge.HoldDuration,restored=energy.CurrentEnergy-100;
            Assert.That(restored/duration,Is.EqualTo(12).Within(.08f));frame.auraHeld=false;yield return Until(()=>!charge.IsCharging);Assert.That(charge.HoldDuration,Is.Zero);
            for(int i=0;i<3;i++){frame.auraHeld=true;yield return Wait(.05f);frame.auraHeld=false;yield return Wait(.05f);}
            energy.SetCurrentEnergy(200);frame.auraHeld=true;yield return Wait(.12f);Assert.That(energy.CurrentEnergy,Is.EqualTo(200));Assert.That(charge.HoldDuration,Is.GreaterThan(.05f));
            Record("4G0-energy",new{maximum=energy.MaxEnergy,duration,restored,rate=restored/duration,costMultiplier=energy.Profile.CostMultiplier});
        }
        [UnityTest] public IEnumerator Matched100And200AuraImageGeometryAndFade()
        {
            var reserve=Clone(combat.cursedEnergy);combat.cursedEnergy=reserve;var so=new SerializedObject(reserve);so.FindProperty("maxEnergy").floatValue=100;so.ApplyModifiedPropertiesWithoutUndo();combat.auraCapacityReference=100;
            frame.auraHeld=true;yield return Wait(.5f);Assert.That(aura.SizeMultiplier,Is.EqualTo(1).Within(.00001f));Pose(new Vector3(4.5f,2.2f,-4.5f));
            Action fixedTime=()=>{foreach(var m in aura.Visual.GetComponentsInChildren<Renderer>().SelectMany(x=>x.sharedMaterials).Where(x=>x!=null&&x.HasProperty("_AuraTime")).Distinct())m.SetFloat("_AuraTime",2);};
            var field=aura.Visual.transform.Find("MainAura").GetComponent<MeshFilter>().sharedMesh;var before=field.vertices;float expansion=aura.Visual.Expansion;
            fixedTime();Capture("4G0-aura-100");
            so.FindProperty("maxEnergy").floatValue=200;so.ApplyModifiedPropertiesWithoutUndo();combat.auraCapacityReference=200;
            typeof(CursedEnergyAuraController).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(aura,null);fixedTime();Capture("4G0-aura-200");
            Assert.That(aura.SizeMultiplier,Is.EqualTo(1).Within(.00001f));Assert.That(aura.Visual.Expansion,Is.EqualTo(expansion));Assert.That(field.vertices,Is.EqualTo(before));
            var a=System.IO.File.ReadAllBytes(System.IO.Path.Combine(Root,"4G0-aura-100.png"));var b=System.IO.File.ReadAllBytes(System.IO.Path.Combine(Root,"4G0-aura-200.png"));Assert.That(a,Is.EqualTo(b),"matched time/pose aura pixels must not enlarge or change");
            frame.auraHeld=false;yield return Wait(settings.fadeOut+.1f);Assert.That(aura.Envelope,Is.Zero);frame.auraHeld=true;yield return Wait(settings.fadeIn+.1f);Assert.That(aura.Envelope,Is.EqualTo(1));
            Record("4G0-aura-identity",new{bytesIdentical=true,multiplier=aura.SizeMultiplier,expansion,vertices=before.Length,settings.fadeIn,settings.fadeOut});
        }
    }
}
