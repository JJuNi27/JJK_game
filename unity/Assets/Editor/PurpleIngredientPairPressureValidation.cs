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
 public sealed class PurpleIngredientPairPressureValidation
 {
  static string Output=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurpleIngredientPairPressureQA"));
  static int Materials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("PurpleIngredient"));
  static int Meshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleIngredient"));
  static bool[] Save()=>new[]{PurpleIngredientReboot2Profile.Current.candidateEnabled,PurpleIngredientFinalProfile.Current.candidateEnabled,PurpleIngredientFinal2Profile.Current.candidateEnabled,PurpleIngredientFlowFirstProfile.Current.candidateEnabled,PurpleIngredientPairPolishProfile.Current.candidateEnabled,PurpleIngredientPairPressureProfile.Current.candidateEnabled};
  static void Restore(bool[] s)
  {
   PurpleIngredientReboot2Profile.Current.candidateEnabled=s[0];PurpleIngredientFinalProfile.Current.candidateEnabled=s[1];PurpleIngredientFinal2Profile.Current.candidateEnabled=s[2];PurpleIngredientFlowFirstProfile.Current.candidateEnabled=s[3];PurpleIngredientPairPolishProfile.Current.candidateEnabled=s[4];PurpleIngredientPairPressureProfile.Current.candidateEnabled=s[5];
  }
  static void Select(bool pressure)
  {
   PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;PurpleIngredientFinal2Profile.Current.candidateEnabled=true;PurpleIngredientFlowFirstProfile.Current.candidateEnabled=true;PurpleIngredientPairPolishProfile.Current.candidateEnabled=true;PurpleIngredientPairPressureProfile.Current.candidateEnabled=pressure;
  }
  [Test] public void DefaultOffShaderAndInspector()
  {
   var p=PurpleIngredientPairPressureProfile.Current;Assert.That(p,Is.Not.Null);Assert.That(p.candidateEnabled,Is.False);
   var shader=Resources.Load<Shader>("VFX/PurpleIngredientPairPressure");Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);
   Assert.That(ShaderUtil.GetShaderMessages(shader).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
   var editor=Editor.CreateEditor(p);try{Assert.That(editor.GetType().Name,Is.EqualTo("PurpleIngredientPairPressureProfileEditor"));}finally{Object.DestroyImmediate(editor);}
  }
  [Test] public void PairPolishPathsBodyAndArrivalsRemainUnchanged()
  {
   var saved=Save();GameObject a=null,b=null;
   try
   {
    foreach(bool red in new[]{false,true})
    {
     Select(false);a=new GameObject("PressureValidationBaseline");var baseline=a.AddComponent<PurpleFusionIngredient>();baseline.Configure(red);
     Assert.That(a.GetComponent<PurpleIngredientPairPressureVolume>(),Is.Null);
     Select(true);b=new GameObject("PressureValidationCandidate");var candidate=b.AddComponent<PurpleFusionIngredient>();candidate.Configure(red);
     var volume=b.GetComponent<PurpleIngredientPairPressureVolume>();Assert.That(volume,Is.Not.Null);
     foreach(float clock in new[]{.35f,.4f,.65f,.95f,1.2f})
     {
      baseline.Render(clock,.25f,Vector3.right*4);candidate.Render(clock,.25f,Vector3.right*4);
      var x=a.GetComponent<PurpleIngredientPairPolishVolume>();var y=b.GetComponent<PurpleIngredientPairPolishVolume>();
      Assert.That(x.OrbWorldPositions,Is.EqualTo(y.OrbWorldPositions),"Macro pressure cannot retime or change absorption/mote paths");
      for(int i=0;i<12;i++)for(int j=0;j<20;j++)Assert.That(a.transform.Find("PairForceFlow_"+i).GetComponent<LineRenderer>().GetPosition(j),Is.EqualTo(b.transform.Find("PairForceFlow_"+i).GetComponent<LineRenderer>().GetPosition(j)));
     }
     string body=red?"RedRepulsionMass":"BlueAttractionMass";
     var matA=a.transform.Find(body).GetComponent<Renderer>().sharedMaterial;var matB=b.transform.Find(body).GetComponent<Renderer>().sharedMaterial;
     Assert.That(matA.shader,Is.EqualTo(matB.shader));Assert.That(a.transform.Find(body).localScale,Is.EqualTo(b.transform.Find(body).localScale));
     foreach(string prop in new[]{"_PhaseTime","_Fusion","_Polarity","_Breakup","_Emission"})Assert.That(matA.GetFloat(prop),Is.EqualTo(matB.GetFloat(prop)));
     volume.Sample(.35f,0,Vector3.right*4);float first=volume.LeadingRadius;volume.Sample(.4f,0,Vector3.right*4);float second=volume.LeadingRadius;
     Assert.That(red?second-first:first-second,Is.GreaterThan(.001f),"Macro pressure must follow the opposed force direction");
     for(int frame=0;frame<120;frame++)
     {
      volume.Sample(frame/60f,0,Vector3.right*4);
      foreach(var filter in b.GetComponentsInChildren<MeshFilter>().Where(m=>m.name.StartsWith("MacroPressure_")))
       foreach(var v in filter.sharedMesh.vertices)Assert.That(float.IsNaN(v.x)||float.IsInfinity(v.x)||float.IsNaN(v.y)||float.IsInfinity(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.z),Is.False);
     }
     Assert.That(b.GetComponentsInChildren<MeshFilter>().Count(m=>m.name.StartsWith("MacroPressure_")),Is.EqualTo(4));
     volume.Sample(1,.8f,Vector3.right*4);Assert.That(b.GetComponentsInChildren<MeshRenderer>().Where(m=>m.name.StartsWith("MacroPressure_")).All(m=>!m.enabled),Is.True,"Pressure must end before completed Purple hold");
     Object.DestroyImmediate(a);Object.DestroyImmediate(b);a=b=null;EditorUtility.UnloadUnusedAssetsImmediate();
    }
   }
   finally{if(a!=null)Object.DestroyImmediate(a);if(b!=null)Object.DestroyImmediate(b);Restore(saved);}
  }
  [Test] public void RepeatedPressureMaterialMeshCleanup()
  {
   EditorUtility.UnloadUnusedAssetsImmediate();var saved=Save();int m=Materials(),mesh=Meshes();
   try
   {
    Select(true);
    for(int n=0;n<3;n++)
    {
     var root=new GameObject("PressureValidationCleanup");try
     {
      foreach(bool red in new[]{false,true}){var go=new GameObject(red?"Red":"Blue");go.transform.SetParent(root.transform);var v=go.AddComponent<PurpleFusionIngredient>();v.Configure(red);for(int i=0;i<150;i++)v.Render(i/60f,Mathf.Clamp01((i/60f-1.1f)/.9f),Vector3.right*4);}
     }finally{Object.DestroyImmediate(root);EditorUtility.UnloadUnusedAssetsImmediate();}
     Assert.That(Materials(),Is.EqualTo(m));Assert.That(Meshes(),Is.EqualTo(mesh));
    }
   }finally{Restore(saved);}
  }
  public static string Run()
  {
   if(Application.isPlaying)throw new InvalidOperationException("Run assertions in Edit Mode");
   Directory.CreateDirectory(Output);var report=new XElement("test-run");int passed=0,failed=0;
   foreach(object tests in new object[]{new PurpleIngredientPairPressureValidation(),new PurpleIngredientPairPolishValidation()})
    foreach(var method in tests.GetType().GetMethods().Where(m=>m.GetCustomAttribute<TestAttribute>()!=null))
    {
     var item=new XElement("test-case",new XAttribute("name",tests.GetType().Name+"."+method.Name));
     try{EditorUtility.UnloadUnusedAssetsImmediate();method.Invoke(tests,null);item.SetAttributeValue("result","Passed");passed++;}
     catch(Exception e){item.SetAttributeValue("result","Failed");item.Add(new XElement("failure",(e.InnerException??e).ToString()));failed++;}
     report.Add(item);
    }
   report.SetAttributeValue("total",passed+failed);report.SetAttributeValue("passed",passed);report.SetAttributeValue("failed",failed);report.SetAttributeValue("harness","Explicit NUnit assertion invocation, not Unity Test Runner discovery");
   new XDocument(report).Save(Path.Combine(Output,"targeted.xml"));
   if(failed>0)throw new Exception(report.ToString());return passed+" PASS / 0 FAIL; actual assertions executed; targeted.xml saved";
  }
 }
}
