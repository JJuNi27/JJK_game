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
    public sealed class PurpleIngredientCasterPressureValidation
    {
        static int Materials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("PurpleIngredient"));
        static int Meshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleIngredient"));
        [Test] public void DefaultOffShaderAndKoreanInspector()
        {
            var p=PurpleIngredientCasterPressureProfile.Current;Assert.That(p,Is.Not.Null);Assert.That(p.candidateEnabled,Is.False);
            var s=Resources.Load<Shader>("VFX/PurpleIngredientCasterPressure");Assert.That(s,Is.Not.Null);Assert.That(s.isSupported,Is.True);
            Assert.That(ShaderUtil.GetShaderMessages(s).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
            var e=Editor.CreateEditor(p);try{Assert.That(e.GetType().Name,Is.EqualTo("PurpleIngredientCasterPressureProfileEditor"));}finally{Object.DestroyImmediate(e);}
        }
        [Test] public void CasterAnchorReleaseAndEndedPressure()
        {
            var host=new GameObject("CasterPressureValidation");var owner=new GameObject("CasterPressureOwner");
            try
            {
                owner.transform.position=new Vector3(10,1,20);var v=host.AddComponent<PurpleIngredientCasterPressure>();
                v.Configure(owner.transform,Vector3.right,1.1f,2,5,PurpleIngredientCasterPressureProfile.Current);
                v.Sample(.6f,new Vector3(40,2,40),new Vector3(-40,2,-40));Assert.That(v.Anchor,Is.EqualTo(owner.transform.position));Assert.That(v.Visible,Is.True);
                var mesh=host.GetComponentInChildren<MeshFilter>().sharedMesh;
                Assert.That(mesh.bounds.size.x,Is.GreaterThan(5));Assert.That(mesh.bounds.size.z,Is.GreaterThan(5));
                foreach(var x in mesh.vertices)Assert.That(float.IsNaN(x.x)||float.IsInfinity(x.x)||float.IsNaN(x.y)||float.IsInfinity(x.y)||float.IsNaN(x.z)||float.IsInfinity(x.z),Is.False);
                v.Sample(4.99f,Vector3.zero,Vector3.zero);Assert.That(v.ForwardWeight,Is.GreaterThan(.1f));
                v.Sample(5.1f,Vector3.zero,Vector3.zero);var anchor=v.Anchor;Assert.That(v.Phase,Is.EqualTo("Release"));Assert.That(v.ForwardWeight,Is.GreaterThan(.5f));
                owner.transform.position+=Vector3.forward*100;v.Sample(5.2f,Vector3.zero,Vector3.zero);Assert.That(v.Anchor,Is.EqualTo(anchor),"Charge pressure never follows caster/projectile after release");
                v.Sample(5.5f,Vector3.zero,Vector3.zero);Assert.That(v.Phase,Is.EqualTo("Ended"));Assert.That(v.Visible,Is.False);Assert.That(v.ChargeWeight,Is.Zero);
            }
            finally{Object.DestroyImmediate(host);Object.DestroyImmediate(owner);}
        }
        [Test] public void CanonicalIngredientAndPurplePosesUnaffected()
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Canonical Production construction is Play Mode only; run PlayCanonical entry.");
            var profile=PurpleIngredientCasterPressureProfile.Current;bool old=profile.candidateEnabled;
            var owner=new GameObject("CasterPressureOwner");var a=new GameObject("CanonicalBaseline");var b=new GameObject("CanonicalCandidate");
            try
            {
                owner.transform.position=new Vector3(0,1,0);
                profile.candidateEnabled=false;var baseline=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(a.transform,owner.transform,0,null,false);
                profile.candidateEnabled=true;var candidate=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(b.transform,owner.transform,0,null,false);
                Assert.That(a.GetComponentInChildren<PurpleIngredientCasterPressure>(),Is.Null);Assert.That(b.GetComponentInChildren<PurpleIngredientCasterPressure>(),Is.Not.Null);
                foreach(float t in new[]{.35f,.65f,.95f,1.35f,1.444f,1.6f,2.2f,3.5f,5.01f,5.2f,5.5f,6f})
                {
                    baseline.Update(t,1/60f);candidate.Update(t,1/60f);
                    foreach(string name in new[]{"HollowPurpleBlueOrbRoot","HollowPurpleRedOrbRoot","HollowPurpleDenseBody"})
                    {
                        var x=a.transform.Find("HollowPurpleCanonicalOrbSequence/"+name);var y=b.transform.Find("HollowPurpleCanonicalOrbSequence/"+name);
                        Assert.That(y.position,Is.EqualTo(x.position));Assert.That(y.localScale,Is.EqualTo(x.localScale));Assert.That(y.gameObject.activeSelf,Is.EqualTo(x.gameObject.activeSelf));
                    }
                }
                var wind=b.GetComponentInChildren<PurpleIngredientCasterPressure>();Assert.That(wind.Visible,Is.False);
                Assert.That(wind.Anchor,Is.EqualTo(owner.transform.position));
                var projectile=b.transform.Find("HollowPurpleCanonicalOrbSequence/HollowPurpleDenseBody");Assert.That(Vector3.Distance(wind.Anchor,projectile.position),Is.GreaterThan(10));
            }
            finally{profile.candidateEnabled=old;Object.DestroyImmediate(a);Object.DestroyImmediate(b);Object.DestroyImmediate(owner);}
        }
        [Test] public void CancelAndRepeatedOwnedResourcesCleanup()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();int m=Materials(),mesh=Meshes();
            for(int i=0;i<4;i++)
            {
                var owner=new GameObject("CasterPressureCleanupOwner");var go=new GameObject("CasterPressureCleanup");
                try
                {
                    var v=go.AddComponent<PurpleIngredientCasterPressure>();v.Configure(owner.transform,Vector3.forward,1.1f,2,5,PurpleIngredientCasterPressureProfile.Current);
                    v.Sample(i==0?.65f:i==1?1.4f:i==2?5.1f:5.5f,Vector3.left,Vector3.right);
                    v.Dispose();v.Dispose();Assert.That(v.Visible,Is.False);
                }
                finally{Object.DestroyImmediate(go);Object.DestroyImmediate(owner);EditorUtility.UnloadUnusedAssetsImmediate();}
                Assert.That(Materials(),Is.EqualTo(m));Assert.That(Meshes(),Is.EqualTo(mesh));
            }
        }
        public static string Run()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit Mode assertions required");
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurpleIngredientCasterPressureQA"));Directory.CreateDirectory(output);
            var report=new XElement("test-run");int passed=0,failed=0;
            foreach(object tests in new object[]{new PurpleIngredientCasterPressureValidation(),new PurpleIngredientPairPolishValidation()})
            foreach(var method in tests.GetType().GetMethods().Where(m=>m.GetCustomAttribute<TestAttribute>()!=null && m.Name!=nameof(CanonicalIngredientAndPurplePosesUnaffected)))
            {
                var item=new XElement("test-case",new XAttribute("name",tests.GetType().Name+"."+method.Name));
                try{EditorUtility.UnloadUnusedAssetsImmediate();method.Invoke(tests,null);item.SetAttributeValue("result","Passed");passed++;}
                catch(Exception e){item.SetAttributeValue("result","Failed");item.Add(new XElement("failure",(e.InnerException??e).ToString()));failed++;}
                report.Add(item);
            }
            report.SetAttributeValue("total",passed+failed);report.SetAttributeValue("passed",passed);report.SetAttributeValue("failed",failed);report.SetAttributeValue("harness","Explicit NUnit assertion invocation, not Unity Test Runner discovery");
            new XDocument(report).Save(Path.Combine(output,"targeted.xml"));if(failed>0)throw new Exception(report.ToString());return passed+" PASS / 0 FAIL; actual assertions executed; targeted.xml saved";
        }
        public static string PlayCanonical()
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Play Mode required");
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurpleIngredientCasterPressureQA/play-canonical.xml"));
            var report=new XElement("test-run",new XAttribute("total",1),new XAttribute("harness","Explicit NUnit assertion invocation in Play Mode"));
            var item=new XElement("test-case",new XAttribute("name","PurpleIngredientCasterPressureValidation.CanonicalIngredientAndPurplePosesUnaffected"));
            try{new PurpleIngredientCasterPressureValidation().CanonicalIngredientAndPurplePosesUnaffected();item.SetAttributeValue("result","Passed");report.SetAttributeValue("passed",1);report.SetAttributeValue("failed",0);report.Add(item);new XDocument(report).Save(output);return "Play canonical: 1 PASS / 0 FAIL; play-canonical.xml saved";}
            catch(Exception e){item.SetAttributeValue("result","Failed");item.Add(new XElement("failure",e.ToString()));report.SetAttributeValue("passed",0);report.SetAttributeValue("failed",1);report.Add(item);new XDocument(report).Save(output);throw;}
        }
    }
}
