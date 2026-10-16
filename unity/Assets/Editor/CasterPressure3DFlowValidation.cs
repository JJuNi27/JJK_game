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
    public sealed class CasterPressure3DFlowValidation
    {
        static int Materials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("PurpleIngredient3DFlow_"));
        static int Meshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleIngredient3DFlow_"));
        [Test] public void DefaultOffShaderAndInspector()
        {
            var p=CasterPressure3DFlowProfile.Current;Assert.That(p,Is.Not.Null);Assert.That(p.candidateEnabled,Is.False);
            var s=Resources.Load<Shader>("VFX/CasterPressure3DFlow");Assert.That(s,Is.Not.Null);Assert.That(s.isSupported,Is.True);
            Assert.That(ShaderUtil.GetShaderMessages(s).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
            var e=Editor.CreateEditor(p);try{Assert.That(e.GetType().Name,Is.EqualTo("CasterPressure3DFlowProfileEditor"));}finally{Object.DestroyImmediate(e);}
        }
        [Test] public void SpaceTraversalReleaseAndNoProjectileFollowing()
        {
            var owner=new GameObject("Flow3DOwner");var go=new GameObject("Flow3DTest");
            try
            {
                owner.transform.position=new Vector3(10,1,20);var v=go.AddComponent<CasterPressure3DFlowPrototype>();
                v.Configure(owner.transform,Vector3.forward,5,CasterPressure3DFlowProfile.Current);v.Sample(.65f);
                Assert.That(v.Visible,Is.True);Assert.That(v.PathPoint(0).y,Is.LessThan(.5));Assert.That(v.PathPoint(1).y,Is.GreaterThan(3));
                Assert.That(v.PathPoint(0).z,Is.GreaterThan(1));Assert.That(v.PathPoint(1).z,Is.LessThan(-2));
                var meshes=go.GetComponentsInChildren<MeshFilter>();Assert.That(meshes.Length,Is.EqualTo(3));
                Assert.That(meshes[0].sharedMesh.vertices.SequenceEqual(meshes[1].sharedMesh.vertices),Is.False);
                foreach(var mesh in meshes)foreach(var p in mesh.sharedMesh.vertices)Assert.That(float.IsNaN(p.x)||float.IsInfinity(p.x)||float.IsNaN(p.y)||float.IsInfinity(p.y)||float.IsNaN(p.z)||float.IsInfinity(p.z),Is.False);
                v.Sample(5.1f);var anchor=v.Anchor;Assert.That(v.ForwardWeight,Is.GreaterThan(.5f));Assert.That(v.Phase,Is.EqualTo("Release"));
                owner.transform.position+=Vector3.forward*100;v.Sample(5.2f);Assert.That(v.Anchor,Is.EqualTo(anchor));
                v.Sample(5.33f);Assert.That(v.Visible,Is.False);Assert.That(v.Phase,Is.EqualTo("Ended"));
            }
            finally{Object.DestroyImmediate(go);Object.DestroyImmediate(owner);}
        }
        [Test] public void RepeatedCancelResources()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();int m=Materials(),n=Meshes();
            for(int i=0;i<4;i++)
            {
                var owner=new GameObject("Flow3DOwner");var go=new GameObject("Flow3DCancel");
                try{var v=go.AddComponent<CasterPressure3DFlowPrototype>();v.Configure(owner.transform,Vector3.forward,5,CasterPressure3DFlowProfile.Current);v.Sample(i<2?.65f:5.1f);v.Dispose();v.Dispose();Assert.That(v.Visible,Is.False);}
                finally{Object.DestroyImmediate(go);Object.DestroyImmediate(owner);EditorUtility.UnloadUnusedAssetsImmediate();}
                Assert.That(Materials(),Is.EqualTo(m));Assert.That(Meshes(),Is.EqualTo(n));
            }
        }
        [Test] public void CanonicalPoseProtection()
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Canonical Production construction requires Play Mode");
            var p=CasterPressure3DFlowProfile.Current;var old=p.candidateEnabled;var oldWind=PurpleIngredientCasterPressureProfile.Current.candidateEnabled;
            var owner=new GameObject("Flow3DCanonicalOwner");var a=new GameObject("Flow3DBaseline");var b=new GameObject("Flow3DCandidate");
            PrototypeHollowPurplePresentationRuntime.OrbSequence x=null,y=null;
            try
            {
                owner.transform.position=new Vector3(0,1,0);PurpleIngredientCasterPressureProfile.Current.candidateEnabled=false;
                p.candidateEnabled=false;x=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(a.transform,owner.transform,0,null,false);
                p.candidateEnabled=true;y=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(b.transform,owner.transform,0,null,false);
                Assert.That(a.GetComponentInChildren<CasterPressure3DFlowPrototype>(),Is.Null);Assert.That(b.GetComponentInChildren<CasterPressure3DFlowPrototype>(),Is.Not.Null);
                foreach(float t in new[]{.35f,.65f,1.1f,1.444f,1.6f,2.2f,3.5f,5.01f,5.2f,5.5f,6f})
                {
                    x.Update(t,1/60f);y.Update(t,1/60f);
                    foreach(var name in new[]{"HollowPurpleBlueOrbRoot","HollowPurpleRedOrbRoot","HollowPurpleDenseBody"})
                    {
                        var u=a.transform.Find("HollowPurpleCanonicalOrbSequence/"+name);var v=b.transform.Find("HollowPurpleCanonicalOrbSequence/"+name);
                        Assert.That(v.position,Is.EqualTo(u.position));Assert.That(v.localScale,Is.EqualTo(u.localScale));Assert.That(v.gameObject.activeSelf,Is.EqualTo(u.gameObject.activeSelf));
                    }
                }
                Assert.That(b.GetComponentInChildren<CasterPressure3DFlowPrototype>().Visible,Is.False);
            }
            finally{x?.Dispose();y?.Dispose();p.candidateEnabled=old;PurpleIngredientCasterPressureProfile.Current.candidateEnabled=oldWind;Object.Destroy(a);Object.Destroy(b);Object.Destroy(owner);}
        }
        static string Output=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/CasterPressure3DFlowPrototypeQA"));
        public static string Run()=>Execute(false);
        public static string Play()=>Execute(true);
        static string Execute(bool play)
        {
            Assert.That(Application.isPlaying,Is.EqualTo(play));Directory.CreateDirectory(Output);
            var report=new XElement("test-run",new XAttribute("harness","Explicit NUnit assertion invocation; not Unity Test Runner discovery"));int passed=0,failed=0;
            foreach(object instance in play?new object[]{new CasterPressure3DFlowValidation()}:new object[]{new CasterPressure3DFlowValidation(),new PurpleIngredientPairPolishValidation()})
            foreach(var method in instance.GetType().GetMethods().Where(m=>m.GetCustomAttribute<TestAttribute>()!=null && ((m.Name==nameof(CanonicalPoseProtection))==play)))
            {
                var item=new XElement("test-case",new XAttribute("name",instance.GetType().Name+"."+method.Name));
                try{method.Invoke(instance,null);item.SetAttributeValue("result","Passed");passed++;}
                catch(Exception e){item.SetAttributeValue("result","Failed");item.Add(new XElement("failure",(e.InnerException??e).ToString()));failed++;}
                report.Add(item);
            }
            report.SetAttributeValue("passed",passed);report.SetAttributeValue("failed",failed);report.SetAttributeValue("total",passed+failed);
            new XDocument(report).Save(Path.Combine(Output,play?"play-canonical.xml":"targeted.xml"));
            if(failed>0)throw new Exception(report.ToString());return passed+" PASS / 0 FAIL; actual assertions executed";
        }
    }
}
