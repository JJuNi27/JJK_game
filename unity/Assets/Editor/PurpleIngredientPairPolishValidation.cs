using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using JJKGame.Player;
using Object=UnityEngine.Object;
namespace JJKGame.EditorTools
{
 public sealed class PurpleIngredientPairPolishValidation
 {
  static string Output=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurpleIngredientPairPolishQA"));
  static int Materials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("PurpleIngredient"));
  static int Meshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleIngredient"));
  static void Select(bool pair)
  {
   PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
   PurpleIngredientFinal2Profile.Current.candidateEnabled=true;PurpleIngredientFlowFirstProfile.Current.candidateEnabled=true;
   PurpleIngredientPairPolishProfile.Current.candidateEnabled=pair;
  }
  static bool[] Save()=>new[]{PurpleIngredientReboot2Profile.Current.candidateEnabled,PurpleIngredientFinalProfile.Current.candidateEnabled,PurpleIngredientFinal2Profile.Current.candidateEnabled,PurpleIngredientFlowFirstProfile.Current.candidateEnabled,PurpleIngredientPairPolishProfile.Current.candidateEnabled};
  static void Restore(bool[] s)
  {
   PurpleIngredientReboot2Profile.Current.candidateEnabled=s[0];PurpleIngredientFinalProfile.Current.candidateEnabled=s[1];PurpleIngredientFinal2Profile.Current.candidateEnabled=s[2];PurpleIngredientFlowFirstProfile.Current.candidateEnabled=s[3];PurpleIngredientPairPolishProfile.Current.candidateEnabled=s[4];
  }
  [Test] public void DefaultOffShadersAndKoreanInspector()
  {
   Assert.That(PurpleIngredientPairPolishProfile.Current,Is.Not.Null);Assert.That(PurpleIngredientPairPolishProfile.Current.candidateEnabled,Is.False);
   foreach(string name in new[]{"PurpleIngredientPairBrush","PurpleIngredientFlowSprite","HollowPurpleIngredientReboot"})
   {
    var shader=Resources.Load<Shader>("VFX/"+name);Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);
    Assert.That(ShaderUtil.GetShaderMessages(shader).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
   }
   var inspector=Editor.CreateEditor(PurpleIngredientPairPolishProfile.Current);try{Assert.That(inspector.GetType().Name,Is.EqualTo("PurpleIngredientPairPolishProfileEditor"));}finally{Object.DestroyImmediate(inspector);}
  }
  [Test] public void ForcePathsArriveAndPush()
  {
   var saved=Save();GameObject root=null;
   try
   {
    Select(true);root=new GameObject("PairValidationForcePaths");
    foreach(bool red in new[]{false,true})
    {
     var obj=new GameObject(red?"Red":"Blue");obj.transform.SetParent(root.transform);var ingredient=obj.AddComponent<PurpleFusionIngredient>();ingredient.Configure(red);
     var volume=obj.GetComponent<PurpleIngredientPairPolishVolume>();Assert.That(volume,Is.Not.Null);
     var line=obj.transform.Find("PairForceFlow_0").GetComponent<LineRenderer>();
     ingredient.Render(.35f,0,Vector3.right*4);float first=line.GetPosition(19).magnitude;
     ingredient.Render(.40f,0,Vector3.right*4);float second=line.GetPosition(19).magnitude;
     Assert.That(red?second-first:first-second,Is.GreaterThan(.001f),"Blue must converge; Red must push outward during the active phase");
     float closest=100,furthest=0;
     for(int i=0;i<180;i++)
     {
      ingredient.Render(i/120f,0,Vector3.right*4);foreach(var pos in volume.OrbWorldPositions){float radius=obj.transform.InverseTransformPoint(pos).magnitude;closest=Mathf.Min(closest,radius);furthest=Mathf.Max(furthest,radius);}
      foreach(var renderer in obj.GetComponentsInChildren<LineRenderer>())for(int k=0;k<renderer.positionCount;k++){var p=renderer.GetPosition(k);Assert.That(float.IsNaN(p.x)||float.IsInfinity(p.x),Is.False);}
     }
     Assert.That(furthest,Is.GreaterThan(PurpleIngredientFlowFirstProfile.Current.reach),"Strong candidate keeps enlarged force reach");
     if(!red)Assert.That(closest,Is.LessThan(.6f),"Motes must reach inside the body, not fade outside it");
    }
   }
   finally{if(root!=null)Object.DestroyImmediate(root);Restore(saved);}
  }
  [Test] public void ExistingBodyAndOppositionRelationship()
  {
   var saved=Save();GameObject a=null,b=null;
   try
   {
    Select(false);a=new GameObject("PairValidationBaseline");var original=a.AddComponent<PurpleFusionIngredient>();original.Configure(false);original.Render(.8f,.3f,Vector3.right*4);
    Assert.That(a.GetComponent<PurpleIngredientPairPolishVolume>(),Is.Null);Assert.That(a.GetComponent<PurpleIngredientFlowFirstVolume>(),Is.Not.Null);
    Select(true);b=new GameObject("PairValidationCandidate");var next=b.AddComponent<PurpleFusionIngredient>();next.Configure(false);next.Render(.8f,.3f,Vector3.right*4);
    var bodyA=a.transform.Find("BlueAttractionMass").GetComponent<Renderer>();var bodyB=b.transform.Find("BlueAttractionMass").GetComponent<Renderer>();
    Assert.That(bodyA.sharedMaterial.shader,Is.EqualTo(bodyB.sharedMaterial.shader));Assert.That(bodyA.transform.localScale,Is.EqualTo(bodyB.transform.localScale));
    foreach(string prop in new[]{"_PhaseTime","_Fusion","_Polarity","_Breakup","_Emission"})Assert.That(bodyA.sharedMaterial.GetFloat(prop),Is.EqualTo(bodyB.sharedMaterial.GetFloat(prop)));
    var path=b.transform.Find("PairForceFlow_0").GetComponent<LineRenderer>();var first=path.GetPosition(19);next.Render(.8f,.3f,Vector3.left*4);
    Assert.That(Vector3.Distance(first,path.GetPosition(19)),Is.GreaterThan(.01f),"Opposite ingredient must affect flow, not just be passed unused");
   }
   finally{if(a!=null)Object.DestroyImmediate(a);if(b!=null)Object.DestroyImmediate(b);Restore(saved);}
  }
  [Test] public void RepeatedMaterialAndMeshCleanup()
  {
   EditorUtility.UnloadUnusedAssetsImmediate();var saved=Save();int m=Materials(),mesh=Meshes();
   try
   {
    Select(true);
    for(int n=0;n<3;n++)
    {
     var root=new GameObject("PairValidationCleanup");try
     {
      foreach(bool red in new[]{false,true}){var go=new GameObject(red?"Red":"Blue");go.transform.SetParent(root.transform);var v=go.AddComponent<PurpleFusionIngredient>();v.Configure(red);for(int i=0;i<150;i++)v.Render(i/60f,Mathf.Clamp01((i/60f-1.1f)/.9f),Vector3.right*4);}
     }finally{Object.DestroyImmediate(root);EditorUtility.UnloadUnusedAssetsImmediate();}
     Assert.That(Materials(),Is.EqualTo(m));Assert.That(Meshes(),Is.EqualTo(mesh));
    }
   }finally{Restore(saved);}
  }
  [Test] public void ProtectedFlowFirstDefaultAndCleanup()
  {
   var tests=new PurpleIngredientFlowFirstTests();tests.SaveProfiles();try{tests.CandidateIsSeparateDefaultOffAndReleasesMaterials();}finally{tests.RestoreProfiles();}
  }
  [Test] public void ProtectedFlowFirstDirectionsAndFusion()
  {
   var tests=new PurpleIngredientFlowFirstTests();tests.SaveProfiles();try{tests.OppositeFlowDirectionsFusionScatterAndRepeatedCleanup();}finally{tests.RestoreProfiles();}
  }
  public static string Run()
  {
   if(Application.isPlaying)throw new InvalidOperationException("Run targeted assertions in Edit Mode before capture");
   Directory.CreateDirectory(Output);var tests=new PurpleIngredientPairPolishValidation();var report=new XElement("test-run");int passed=0,failed=0;
   foreach(var method in typeof(PurpleIngredientPairPolishValidation).GetMethods().Where(m=>m.GetCustomAttribute<TestAttribute>()!=null))
   {
    var item=new XElement("test-case",new XAttribute("name",method.Name));
    try{EditorUtility.UnloadUnusedAssetsImmediate();method.Invoke(tests,null);item.SetAttributeValue("result","Passed");passed++;}
    catch(Exception e){item.SetAttributeValue("result","Failed");item.Add(new XElement("failure",(e.InnerException??e).ToString()));failed++;}
    report.Add(item);
   }
   report.SetAttributeValue("total",passed+failed);report.SetAttributeValue("passed",passed);report.SetAttributeValue("failed",failed);report.SetAttributeValue("harness","Explicit existing NUnit assertion invocation; not Unity Test Runner discovery");
   new XDocument(report).Save(Path.Combine(Output,"targeted.xml"));
   if(failed>0)throw new Exception(report.ToString());return passed+" PASS / 0 FAIL; actual NUnit assertion methods executed; targeted.xml generated";
  }
 }
}
