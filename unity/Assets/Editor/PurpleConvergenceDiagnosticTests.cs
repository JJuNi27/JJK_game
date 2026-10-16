using System;
using System.IO;
using System.Linq;
using System.Collections;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;
namespace JJKGame.EditorTools
{
    public sealed class PurpleConvergenceDiagnosticTests
    {
        [Test] public void SeparateOffEightFrameEnvelopeAndRealPresentationHold()
        {
            var p=PurpleConvergenceDiagnosticProfile.Current;Assert.That(p.candidateEnabled,Is.False);
            var root=new GameObject("DiagnosticClockTest");var camera=root.AddComponent<Camera>();
            var effect=root.AddComponent<PurpleConvergenceDiagnostic>();
            try
            {
                effect.Configure(1.444f,Vector3.forward*5,camera,p);effect.SetPreview(camera,1.45f);effect.Sample(1.45f);
                Assert.That(effect.FlashWeight,Is.EqualTo(1));
                effect.SetPreviewTime(1.45f+1/60f);Assert.That(effect.FlashWeight,Is.EqualTo(1));
                effect.SetPreviewTime(1.45f+4/60f);Assert.That(effect.FlashWeight,Is.InRange(.1f,.9f));
                Assert.That(effect.MapPresentationTime(1.45f+4/60f),Is.EqualTo(1.444f));
                effect.SetPreviewTime(1.45f+8/60f+.00001f);Assert.That(effect.FlashWeight,Is.Zero);Assert.That(effect.Holding,Is.True);
                effect.SetPreviewTime(1.45f+.141f);Assert.That(effect.Holding,Is.False);
                effect.SetPreviewTime(1.45f+.33f);Assert.That(effect.MapPresentationTime(1.78f),Is.EqualTo(1.78f));
                var shader=Resources.Load<Shader>("VFX/PurpleConvergenceRadial");Assert.That(shader,Is.Not.Null);
                Assert.That(ShaderUtil.GetShaderMessages(shader).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
            }
            finally{effect.Dispose();Object.DestroyImmediate(root);}
        }
        [Test] public void TransientCameraVolumeAndRepeatCleanupRestoreExactly()
        {
            int before=Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("PurpleConvergenceDiagnostic"));
            for(int i=0;i<3;i++)
            {
                var root=new GameObject("DiagnosticCleanupTest");var camera=root.AddComponent<Camera>();var data=root.AddComponent<UniversalAdditionalCameraData>();
                data.volumeLayerMask=1;data.renderPostProcessing=false;camera.transform.position=new Vector3(2,3,4);var pos=camera.transform.position;var rot=camera.transform.rotation;
                var effectRoot=new GameObject("SeparateContactCentre");effectRoot.transform.SetParent(root.transform);
                var effect=effectRoot.AddComponent<PurpleConvergenceDiagnostic>();
                try
                {
                    effect.Configure(1,Vector3.forward*5,camera,PurpleConvergenceDiagnosticProfile.Current);effect.SetPreview(camera,1);effect.Sample(1);effect.ApplyRender(camera);
                    Assert.That(Vector3.Distance(pos,camera.transform.position),Is.GreaterThan(.4f));Assert.That(data.renderPostProcessing,Is.True);Assert.That(data.volumeLayerMask.value,Is.Not.EqualTo(1));
                    var volume=root.GetComponentInChildren<Volume>();Assert.That(volume.weight,Is.EqualTo(1));
                    effect.Dispose();Assert.That(camera.transform.position,Is.EqualTo(pos));Assert.That(Quaternion.Angle(camera.transform.rotation,rot),Is.LessThan(.01f));Assert.That(data.volumeLayerMask.value,Is.EqualTo(1));Assert.That(data.renderPostProcessing,Is.False);Assert.That(volume.weight,Is.Zero);
                }
                finally{effect.Dispose();Object.DestroyImmediate(root);}
            }
            Assert.That(Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("PurpleConvergenceDiagnostic")),Is.EqualTo(before));
            Assert.That(Resources.FindObjectsOfTypeAll<VolumeProfile>().Count(v=>v.name=="PurpleConvergenceDiagnostic_Volume"),Is.Zero);
        }
    }
    public static class PurpleConvergenceDiagnosticLiveProbe
    {
        public static string Start()
        {
            string path=Path.GetFullPath("Logs/PurpleConvergenceDiagnosticQA/live_hitstop.json");
            if(!Application.isPlaying)throw new InvalidOperationException("Play Mode required");
            File.WriteAllText(path,"{\"status\":\"running\"}");EditorApplication.isPaused=false;
            Camera.main.GetComponent<JJKGame.CameraSystem.SimpleCameraFollow>().StartCoroutine(Run(path));return path;
        }
        private static IEnumerator Run(string path)
        {
            float saved=Time.timeScale;bool background=Application.runInBackground;Application.runInBackground=true;
            var results=new System.Collections.Generic.List<object>();GameObject root=null;int passed=0;
            try
            {
                yield return null;
                for(int stage=0;stage<3;stage++)
                {
                    float normal=stage==2?.5f:1;Time.timeScale=normal;
                    root=new GameObject("DiagnosticLiveProbe");var effect=root.AddComponent<PurpleConvergenceDiagnostic>();
                    effect.Configure(0,Camera.main.transform.position+Camera.main.transform.forward*5,Camera.main,PurpleConvergenceDiagnosticProfile.Current);
                    effect.Sample(0);float start=effect.HitStopStart,engaged=Time.timeScale;
                    if(stage==1){effect.Dispose();Object.Destroy(root);}
                    yield return null;
                    while(Mathf.Abs(Time.timeScale-normal)>.0001f && Time.unscaledTime-start<.4f)yield return null;
                    float end=Time.unscaledTime,duration=end-start;
                    bool pass=Mathf.Abs(engaged-normal*.01f)<.0001f && Mathf.Abs(Time.timeScale-normal)<.0001f && duration>=.139f && duration<.20f;
                    if(pass)passed++;
                    Debug.Log($"ConvergenceDiagnostic QA start={start:F6} end={end:F6} actualDuration={duration:F6} pass={pass}");
                    results.Add(new{scenario=stage,pass,start,end,actualDuration=duration,engaged,restored=Time.timeScale});
                    effect.Dispose();if(root!=null)Object.Destroy(root);yield return null;
                }
                File.WriteAllText(path,Newtonsoft.Json.JsonConvert.SerializeObject(new{passed,failed=3-passed,results},Newtonsoft.Json.Formatting.Indented));
            }
            finally{if(root!=null)Object.Destroy(root);Time.timeScale=saved;Application.runInBackground=background;}
        }
    }
}
