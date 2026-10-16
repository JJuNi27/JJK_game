using System;
using System.IO;
using System.Linq;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace JJKGame.EditorTools
{
    public sealed class PurpleConvergenceImpactTests
    {
        [Test] public void SavedOffAndContactWindowHasOneTrigger()
        {
            var p=PurpleConvergenceImpactProfile.Current;Assert.That(p.candidateEnabled,Is.False);
            var root=new GameObject("ImpactWindowTest");var cam=root.AddComponent<Camera>();
            var effect=root.AddComponent<PurpleConvergenceImpact>();
            float originalScale=Time.timeScale;
            try
            {
                effect.Configure(1.444f,cam,p);effect.SetPreview(cam,0);effect.Sample(1.443f);Assert.That(effect.Triggered,Is.False);
                effect.SetPreviewTime(1.444f);effect.Sample(1.444f);Assert.That(effect.FlashVisible,Is.True);
                effect.SetPreviewTime(1.444f+1/60f);effect.Sample(1.445f);Assert.That(effect.FlashVisible,Is.True);
                effect.SetPreviewTime(1.444f+2/60f+.0001f);Assert.That(effect.FlashVisible,Is.False);
                effect.Sample(2);Assert.That(effect.TriggerCount,Is.EqualTo(1));Assert.That(Time.timeScale,Is.EqualTo(originalScale));
                var shader=Resources.Load<Shader>("VFX/PurpleConvergenceFlash");Assert.That(shader,Is.Not.Null);
                Assert.That(ShaderUtil.GetShaderMessages(shader).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
            }
            finally{effect.Dispose();Object.DestroyImmediate(root);}
        }
        [Test] public void CameraCancelRepeatRestoresPoseAndMaterialCount()
        {
            int before=Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name=="PurpleConvergenceImpact_Flash");
            for(int i=0;i<3;i++)
            {
                var root=new GameObject("ImpactCleanupTest");var cam=root.AddComponent<Camera>();cam.fieldOfView=57;
                var pos=new Vector3(3,4,5);var rot=Quaternion.Euler(11,22,33);cam.transform.SetPositionAndRotation(pos,rot);
                var effect=root.AddComponent<PurpleConvergenceImpact>();
                try
                {
                    effect.Configure(1,cam,PurpleConvergenceImpactProfile.Current);effect.SetPreview(cam,1);effect.Sample(1);effect.ApplyRenderOffset(cam);
                    Assert.That(Vector3.Distance(pos,cam.transform.position),Is.GreaterThan(.01f));
                    // Exercise the sequence's explicit disposal here. Actual OnDisable is tested by the live probe.
                    effect.Dispose();
                    Assert.That(cam.transform.position,Is.EqualTo(pos));Assert.That(Quaternion.Angle(cam.transform.rotation,rot),Is.LessThan(.0001f));Assert.That(cam.fieldOfView,Is.EqualTo(57));Assert.That(effect.FlashVisible,Is.False);
                }
                finally{effect.Dispose();Object.DestroyImmediate(root);}
            }
            Assert.That(Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name=="PurpleConvergenceImpact_Flash"),Is.EqualTo(before));
        }

    }
    public static class PurpleConvergenceLiveProbe
    {
        public static string Start()
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Play Mode required");
            string output=Path.GetFullPath("Logs/PurpleConvergenceImpactQA/live_hitstop.json");
            File.WriteAllText(output,"{\"status\":\"running\"}");
            EditorApplication.isPaused=false;
            Camera.main.GetComponent<JJKGame.CameraSystem.SimpleCameraFollow>().StartCoroutine(Run(output));
            return output;
        }
        private static System.Collections.IEnumerator Run(string output)
        {
            float originalScale=Time.timeScale;bool oldBackground=Application.runInBackground;
            Application.runInBackground=true;
            var results=new System.Collections.Generic.List<object>();GameObject host=null;int passed=0;
            try
            {
                // Begin on a REAL player frame, not an Editor eval call with a stale Time.unscaledTime.
                yield return null;
                for(int stage=0;stage<4;stage++)
                {
                    float expected=stage==2?.5f:stage==3?0:1;
                    Time.timeScale=expected;
                    host=new GameObject("ConvergenceLiveProbe");var effect=host.AddComponent<PurpleConvergenceImpact>();
                    effect.Configure(0,Camera.main,PurpleConvergenceImpactProfile.Current);
                    float begin=Time.unscaledTime;effect.Sample(0);
                    float engaged=Time.timeScale;
                    bool cancelRestored=true;
                    if(stage==1)
                    {
                        var cam=Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;
                        effect.ApplyRenderOffset(cam);effect.enabled=false;
                        cancelRestored=Vector3.Distance(cam.transform.position,pos)<.00001f && Quaternion.Angle(cam.transform.rotation,rot)<.01f;
                        Object.Destroy(host);
                    }
                    yield return null;
                    while(stage!=3 && Mathf.Abs(Time.timeScale-expected)>.0001f && Time.unscaledTime-begin<.3f)yield return null;
                    bool ok=Mathf.Abs(engaged-(stage==2?.005f:stage==3?0:.01f))<.0001f && Mathf.Abs(Time.timeScale-expected)<.0001f && cancelRestored;
                    if(ok)passed++;
                    results.Add(new{scenario=stage,pass=ok,engaged,restored=Time.timeScale,recoveredAt=Time.unscaledTime-begin,cancelRestored});
                    if(host!=null)Object.Destroy(host);
                    yield return null;
                }
                File.WriteAllText(output,Newtonsoft.Json.JsonConvert.SerializeObject(new{passed,failed=4-passed,results},Newtonsoft.Json.Formatting.Indented));
            }
            finally{if(host!=null)Object.Destroy(host);Time.timeScale=originalScale;Application.runInBackground=oldBackground;}
        }
    }
}
