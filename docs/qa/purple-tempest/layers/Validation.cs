using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using JJKGame.Player;
using NUnit.Framework;
using Object=UnityEngine.Object;
public static class TempestLayersValidation
{
    static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurpleTempestLayersQA"));
    static int Materials()=>Resources.FindObjectsOfTypeAll<Material>().Count(x=>x.name.StartsWith("PurpleTempest_") || x.name.StartsWith("PurplePressureStorm_") || x.name.StartsWith("PurpleProduction_") || x.name.StartsWith("PurpleIdentity"));
    static int Meshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(x=>x.name.StartsWith("PurpleTempest_") || x.name.StartsWith("PurplePressureStorm_") || x.name.StartsWith("PurpleProduction_"));
    public static string Begin()
    {
        Assert.That(Application.isPlaying,Is.True);
        Assert.That(PurpleTempestLayersProfile.Current.candidateEnabled,Is.False);
        Assert.That(Object.FindObjectsByType<PurpleTempestLayersRuntime>(FindObjectsSortMode.None).Length,Is.Zero);
        foreach(var name in new[]{"PurpleTempestWind","PurpleTempestDischarge"})
        {
            var shader=Resources.Load<Shader>("VFX/"+name);Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);
            Assert.That(ShaderUtil.GetShaderMessages(shader).Count(x=>x.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
        }
        var editor=Editor.CreateEditor(PurpleTempestLayersProfile.Current);
        try{Assert.That(editor.GetType().Name,Is.EqualTo("PurpleTempestLayersProfileEditor"));}finally{Object.DestroyImmediate(editor);}
        File.WriteAllLines(Path.Combine(Root,"resource-baseline.txt"),new[]{Materials().ToString(),Meshes().ToString()});
        return "PASS: saved OFF; shaders supported/errors 0; scoped inspector; baseline resources "+Materials()+" / "+Meshes();
    }
    public static string Cycle()
    {
        var p=PurpleTempestLayersProfile.Current;bool saved=p.candidateEnabled;bool savedStorm=PurplePressureStormProfile.Current.candidateEnabled;PurplePressureStormProfile.Current.candidateEnabled=true;
        var wrapped=PurpleIdentityWrappedProfile.Current;bool savedWrapped=wrapped.candidateEnabled;
        GameObject baseline=null,candidate=null,terminal=null;
        try
        {
            wrapped.candidateEnabled=true;p.candidateEnabled=false;
            baseline=new GameObject("TempestLayersValidationBaseline");var a=baseline.AddComponent<ProductionPurpleVisual>();a.Configure(Vector3.forward*30);
            a.Render(2.7f,-1,false);
            Assert.That(baseline.GetComponentInChildren<PurpleTempestLayersRuntime>(true),Is.Null,"OFF must not spawn candidate");
            Assert.That(baseline.GetComponentInChildren<PurplePressureStormRuntime>(true),Is.Not.Null);
            var bodyA=baseline.GetComponentInChildren<ProductionPurpleBodyRuntime>();
            var shaderA=bodyA.transform.Find("R3_TornPlasmaVolume").GetComponent<Renderer>().sharedMaterial.shader;
            p.candidateEnabled=true;
            candidate=new GameObject("TempestLayersValidationCandidate");var b=candidate.AddComponent<ProductionPurpleVisual>();b.Configure(Vector3.forward*30);
            b.Render(2.7f,-1,false);
            Assert.That(candidate.GetComponentsInChildren<PurpleTempestLayersRuntime>(true).Length,Is.EqualTo(1));
            Assert.That(candidate.GetComponentInChildren<PurplePressureStormRuntime>(true),Is.Null);
            var bodyB=candidate.GetComponentInChildren<ProductionPurpleBodyRuntime>();var rendererB=bodyB.transform.Find("R3_TornPlasmaVolume").GetComponent<Renderer>();
            Assert.That(rendererB.sharedMaterial.shader,Is.EqualTo(shaderA),"Charge body shader must remain unchanged");
            Assert.That(shaderA,Is.EqualTo(Resources.Load<Shader>("VFX/PurpleIdentityWrappedBody")));
            int materials=Materials(),meshes=Meshes();
            for(int frame=0;frame<120;frame++)b.Render(2.7f+frame/60f,-1,false);
            Assert.That(Materials(),Is.EqualTo(materials));Assert.That(Meshes(),Is.EqualTo(meshes));
            b.Render(5.4f,.26f,true);
            Assert.That(rendererB.sharedMaterial.shader,Is.EqualTo(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R3")),"Travel body unchanged");
            terminal=new GameObject("TempestLayersValidationTerminal");terminal.AddComponent<PurpleTerminalBurst>().Configure(Vector3.forward*6,Vector3.forward,1);
            Assert.That(terminal.GetComponentInChildren<PurpleTempestLayersRuntime>(true),Is.Null);
            Assert.That(terminal.GetComponentInChildren<ProductionPurpleBodyRuntime>(true),Is.Null);
            return "PASS: OFF gate, charge/travel body identity, 120 samples without material/mesh growth, terminal exclusion";
        }
        finally
        {
            p.candidateEnabled=saved;wrapped.candidateEnabled=savedWrapped;PurplePressureStormProfile.Current.candidateEnabled=savedStorm;
            foreach(var go in new[]{baseline,candidate,terminal})if(go!=null)go.name+="_PendingCleanup";
        }
    }
    public static string CheckCleanup()
    {
        var expected=File.ReadAllLines(Path.Combine(Root,"resource-baseline.txt"));
        Assert.That(Materials(),Is.EqualTo(int.Parse(expected[0])));Assert.That(Meshes(),Is.EqualTo(int.Parse(expected[1])));
        Assert.That(Object.FindObjectsByType<PurpleTempestLayersRuntime>(FindObjectsSortMode.None).Length,Is.Zero);
        Assert.That(PurpleTempestLayersProfile.Current.candidateEnabled,Is.False);
        return "PASS: deferred cleanup; material/mesh return to baseline "+Materials()+" / "+Meshes()+"; candidate OFF";
    }
    public static string DestroyAfterPlayerLoop()
    {
        foreach(var go in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(x=>x.name.StartsWith("TempestLayersValidation") && x.name.EndsWith("_PendingCleanup")).ToArray())Object.Destroy(go.gameObject);
        return "Destroyed after actual player loop";
    }
    public static string Inventory()
    {
        return "materials="+Materials()+" meshes="+Meshes()+" newMaterials="+Resources.FindObjectsOfTypeAll<Material>().Count(x=>x.name.StartsWith("PurpleTempest_"))+" newMeshes="+Resources.FindObjectsOfTypeAll<Mesh>().Count(x=>x.name.StartsWith("PurpleTempest_"))+" newRoots="+Object.FindObjectsByType<PurpleTempestLayersRuntime>(FindObjectsSortMode.None).Length+"\n"+string.Join("\n",Resources.FindObjectsOfTypeAll<Material>().Where(x=>x.name.StartsWith("Purple")).GroupBy(x=>x.name).Select(x=>x.Key+": "+x.Count()));
    }
    public static string ActiveState()
    {
        return "frame="+Time.frameCount+" paused="+EditorApplication.isPaused+" playing="+Application.isPlaying+" scale="+Time.timeScale+"\n"+string.Join("\n",Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x is PurpleTempestLayersRuntime || x is ProductionPurpleOuterRuntime).Select(x=>x.name+" active="+x.gameObject.activeInHierarchy+" awake="+x.didAwake+" started="+x.didStart));
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
