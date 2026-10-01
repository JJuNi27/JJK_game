using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using JJKGame.Player;
using NUnit.Framework;
using Object=UnityEngine.Object;
public static class TempestPolishValidation
{
    static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurpleTempestPolishQA"));
    static int Materials()=>Resources.FindObjectsOfTypeAll<Material>().Count(x=>(x.name.StartsWith("PurpleTempest_") || x.name.StartsWith("PurpleTempestPolish_")) || x.name.StartsWith("PurplePressureStorm_") || x.name.StartsWith("PurpleProduction_") || x.name.StartsWith("PurpleIdentity"));
    static int Meshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(x=>(x.name.StartsWith("PurpleTempest_") || x.name.StartsWith("PurpleTempestPolish_")) || x.name.StartsWith("PurplePressureStorm_") || x.name.StartsWith("PurpleProduction_"));
    public static string Begin()
    {
        Assert.That(Application.isPlaying,Is.True);
        Assert.That(PurpleTempestPolishProfile.Current.candidateEnabled,Is.False);
        Assert.That(Object.FindObjectsByType<PurpleTempestPolishRuntime>(FindObjectsSortMode.None).Length,Is.Zero);
        foreach(var name in new[]{"PurpleTempestPolishWind","PurpleTempestPolishDischarge"})
        {
            var shader=Resources.Load<Shader>("VFX/"+name);Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);
            Assert.That(ShaderUtil.GetShaderMessages(shader).Count(x=>x.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
        }
        var editor=Editor.CreateEditor(PurpleTempestPolishProfile.Current);
        try{Assert.That(editor.GetType().Name,Is.EqualTo("PurpleTempestPolishProfileEditor"));}finally{Object.DestroyImmediate(editor);}
        File.WriteAllLines(Path.Combine(Root,"resource-baseline.txt"),new[]{Materials().ToString(),Meshes().ToString()});
        return "PASS: saved OFF; shaders supported/errors 0; scoped inspector; baseline resources "+Materials()+" / "+Meshes();
    }
    public static string Cycle()
    {
        var p=PurpleTempestPolishProfile.Current;bool saved=p.candidateEnabled;bool savedStorm=PurpleTempestLayersProfile.Current.candidateEnabled;PurpleTempestLayersProfile.Current.candidateEnabled=true;
        var wrapped=PurpleIdentityWrappedProfile.Current;bool savedWrapped=wrapped.candidateEnabled;
        GameObject baseline=null,candidate=null,terminal=null;
        try
        {
            wrapped.candidateEnabled=true;p.candidateEnabled=false;
            baseline=new GameObject("TempestPolishValidationBaseline");var a=baseline.AddComponent<ProductionPurpleVisual>();a.Configure(Vector3.forward*30);
            a.Render(2.7f,-1,false);
            Assert.That(baseline.GetComponentInChildren<PurpleTempestPolishRuntime>(true),Is.Null,"OFF must not spawn candidate");
            Assert.That(baseline.GetComponentInChildren<PurpleTempestLayersRuntime>(true),Is.Not.Null);
            var bodyA=baseline.GetComponentInChildren<ProductionPurpleBodyRuntime>();
            var shaderA=bodyA.transform.Find("R3_TornPlasmaVolume").GetComponent<Renderer>().sharedMaterial.shader;
            p.candidateEnabled=true;
            candidate=new GameObject("TempestPolishValidationCandidate");var b=candidate.AddComponent<ProductionPurpleVisual>();b.Configure(Vector3.forward*30);
            b.Render(2.7f,-1,false);
            Assert.That(candidate.GetComponentsInChildren<PurpleTempestPolishRuntime>(true).Length,Is.EqualTo(1));
            Assert.That(candidate.GetComponentInChildren<PurpleTempestLayersRuntime>(true),Is.Null);
            var bodyB=candidate.GetComponentInChildren<ProductionPurpleBodyRuntime>();var rendererB=bodyB.transform.Find("R3_TornPlasmaVolume").GetComponent<Renderer>();
            Assert.That(rendererB.sharedMaterial.shader,Is.EqualTo(shaderA),"Charge body shader must remain unchanged");
            Assert.That(shaderA,Is.EqualTo(Resources.Load<Shader>("VFX/PurpleIdentityWrappedBody")));
            int materials=Materials(),meshes=Meshes();
            for(int frame=0;frame<120;frame++)b.Render(2.7f+frame/60f,-1,false);
            var layer=candidate.GetComponentInChildren<PurpleTempestPolishRuntime>();
            var chargeMatrix=layer.transform.localToWorldMatrix;
            Vector3 releaseAt=layer.transform.position;
            int segments=layer.VisibleDarkSegments;
            foreach(var mesh in candidate.GetComponentsInChildren<MeshFilter>())
                foreach(var vertex in mesh.sharedMesh.vertices)
                    Assert.That(float.IsNaN(vertex.x) || float.IsNaN(vertex.y) || float.IsNaN(vertex.z) || float.IsInfinity(vertex.x) || float.IsInfinity(vertex.y) || float.IsInfinity(vertex.z),Is.False,"Mesh positions must remain finite");
            Assert.That(segments,Is.GreaterThan(0));Assert.That(segments,Is.LessThan(8*64));
            layer.transform.position+=Vector3.forward*1.5f;
            layer.Sample(5.2f,Vector3.forward*30,.05f);
            Assert.That(Vector3.Distance(layer.ChargeAnchor,releaseAt),Is.LessThan(.001f));
            Assert.That(layer.PresentationPhase,Is.EqualTo("Release"));
            Assert.That(layer.ChargeWindWeight,Is.GreaterThan(0));
            layer.transform.position+=Vector3.forward*15;
            layer.Sample(5.7f,Vector3.forward*30,.55f);
            Assert.That(Vector3.Distance(layer.ChargeAnchor,releaseAt),Is.LessThan(.001f));
            Assert.That(layer.PresentationPhase,Is.EqualTo("Travel"));
            Assert.That(layer.ChargeWindWeight,Is.Zero);Assert.That(layer.TravelWindWeight,Is.GreaterThan(0));
            layer.transform.position=releaseAt;
            Assert.That(Materials(),Is.EqualTo(materials));Assert.That(Meshes(),Is.EqualTo(meshes));
            b.Render(5.4f,.26f,true);
            Assert.That(rendererB.sharedMaterial.shader,Is.EqualTo(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R3")),"Travel body unchanged");
            terminal=new GameObject("TempestPolishValidationTerminal");terminal.AddComponent<PurpleTerminalBurst>().Configure(Vector3.forward*6,Vector3.forward,1);
            Assert.That(terminal.GetComponentInChildren<PurpleTempestPolishRuntime>(true),Is.Null);
            Assert.That(terminal.GetComponentInChildren<ProductionPurpleBodyRuntime>(true),Is.Null);
            return "PASS: OFF gate, charge/travel body identity, 120 samples without growth, charge anchor/release/travel transition, real dark gaps, terminal exclusion";
        }
        finally
        {
            p.candidateEnabled=saved;wrapped.candidateEnabled=savedWrapped;PurpleTempestLayersProfile.Current.candidateEnabled=savedStorm;
            foreach(var go in new[]{baseline,candidate,terminal})if(go!=null)go.name+="_PendingCleanup";
        }
    }
    public static string CheckCleanup()
    {
        var expected=File.ReadAllLines(Path.Combine(Root,"resource-baseline.txt"));
        Assert.That(Materials(),Is.EqualTo(int.Parse(expected[0])));Assert.That(Meshes(),Is.EqualTo(int.Parse(expected[1])));
        Assert.That(Object.FindObjectsByType<PurpleTempestPolishRuntime>(FindObjectsSortMode.None).Length,Is.Zero);
        Assert.That(PurpleTempestPolishProfile.Current.candidateEnabled,Is.False);
        return "PASS: deferred cleanup; material/mesh return to baseline "+Materials()+" / "+Meshes()+"; candidate OFF";
    }
    public static string DestroyAfterPlayerLoop()
    {
        foreach(var go in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(x=>x.name.StartsWith("TempestPolishValidation") && x.name.EndsWith("_PendingCleanup")).ToArray())Object.Destroy(go.gameObject);
        return "Destroyed after actual player loop";
    }
    public static string Inventory()
    {
        return "materials="+Materials()+" meshes="+Meshes()+" newMaterials="+Resources.FindObjectsOfTypeAll<Material>().Count(x=>(x.name.StartsWith("PurpleTempest_") || x.name.StartsWith("PurpleTempestPolish_")))+" newMeshes="+Resources.FindObjectsOfTypeAll<Mesh>().Count(x=>(x.name.StartsWith("PurpleTempest_") || x.name.StartsWith("PurpleTempestPolish_")))+" newRoots="+Object.FindObjectsByType<PurpleTempestPolishRuntime>(FindObjectsSortMode.None).Length+"\n"+string.Join("\n",Resources.FindObjectsOfTypeAll<Material>().Where(x=>x.name.StartsWith("Purple")).GroupBy(x=>x.name).Select(x=>x.Key+": "+x.Count()));
    }
    public static string ActiveState()
    {
        return "frame="+Time.frameCount+" paused="+EditorApplication.isPaused+" playing="+Application.isPlaying+" scale="+Time.timeScale+"\n"+string.Join("\n",Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x is PurpleTempestPolishRuntime || x is ProductionPurpleOuterRuntime).Select(x=>x.name+" active="+x.gameObject.activeInHierarchy+" awake="+x.didAwake+" started="+x.didStart));
    }
    public static string EnableBackgroundQa()
    {
        File.WriteAllText(Path.Combine(Root,"background-original.txt"),Application.runInBackground.ToString());
        Application.runInBackground=true;return "QA background ticking enabled, frame="+Time.frameCount;
    }
    public static string RestoreBackgroundQa()
    {
        Application.runInBackground=bool.Parse(File.ReadAllText(Path.Combine(Root,"background-original.txt")));return "Restored runInBackground="+Application.runInBackground;
    }
}

public static class TempestPolishEntry
{
    public static string Refresh()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        foreach(var path in new[]{"PurpleTempestPolishWind","PurpleTempestPolishDischarge"})
        {
            var shader=Resources.Load<Shader>("VFX/"+path);
            if(shader==null || !shader.isSupported)throw new System.Exception(path+" missing/unsupported");
            var errors=ShaderUtil.GetShaderMessages(shader).Where(x=>x.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error).ToArray();
            if(errors.Length>0)throw new System.Exception(string.Join(";",errors.Select(x=>x.message)));
        }
        return "2 shaders imported; errors=0; savedOff="+!JJKGame.Player.PurpleTempestPolishProfile.Current.candidateEnabled;
    }
    public static string Preview()
    {
        var output=JJKGame.EditorTools.PurpleTempestPolishCapture.Capture(0,false);
        for(int i=1;i<4;i++)JJKGame.EditorTools.PurpleTempestPolishCapture.Capture(i,false);
        return output;
    }
    public static string Full()
    {
        var output=JJKGame.EditorTools.PurpleTempestPolishCapture.Capture(0,true);
        for(int i=1;i<4;i++)JJKGame.EditorTools.PurpleTempestPolishCapture.Capture(i,true);
        return output;
    }
}
