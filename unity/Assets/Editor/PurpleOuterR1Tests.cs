using System;
using System.Collections;
using System.IO;
using JJKGame.Dev.PurpleBodyExploration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace JJKGame.EditorTools
{
    public sealed class PurpleOuterR1Tests
    {
        private const string Output="D:/JJK_game/unity/Logs/PurpleOuterR1QA";
        private static PurpleOuterR1Profile Profile=>Resources.Load<PurpleOuterR1Profile>("VFX/PurpleOuterR1Profile");
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        private static Texture2D Read(PurpleOuterR1Bench bench)
        {
            bench.Common.RenderNow();var old=RenderTexture.active;var image=new Texture2D(960,540,TextureFormat.RGB24,false);
            try{RenderTexture.active=bench.Common.Preview;image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();}
            finally{RenderTexture.active=old;}return image;
        }
        private static void Still(PurpleOuterR1Bench bench,string file)
        {var image=Read(bench);try{File.WriteAllBytes(file,image.EncodeToPNG());}finally{Object.DestroyImmediate(image);}}
        private static void ShaderCheck()
        {
            foreach(string path in new[]{"VFX/PurpleOuterHalo","VFX/PurpleOuterParticle","VFX/PurpleOuterDistortion"})
            {var shader=Resources.Load<Shader>(path);Assert.That(shader.isSupported,Is.True);foreach(var msg in ShaderUtil.GetShaderMessages(shader))Assert.That(msg.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),msg.message);}
        }
        [UnityTest] public IEnumerator SafePreviewCompileAndClose()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;Time.timeScale=0;
            var main=Camera.main;var data=main.GetComponent<UniversalAdditionalCameraData>();var user=GameObject.Find("Purple_UserPrototype");Assert.That(user,Is.Not.Null);
            var pos=main.transform.position;float fov=main.fieldOfView;var target=main.targetTexture;bool post=data.renderPostProcessing;
            var userPosition=user.transform.position;int children=user.transform.childCount;
            var editor=UnityEditor.Editor.CreateEditor(Profile);Assert.That(editor,Is.TypeOf<PurpleOuterR1ProfileEditor>());Object.DestroyImmediate(editor);
            var root=new GameObject("LightweightOuterR1PreviewTest");var bench=root.AddComponent<PurpleOuterR1Bench>();bench.Configure(Profile,main);bench.Common.BenchmarkCamera.enabled=false;
            string folder=Path.Combine(Output,"Previews",Profile.previewStage+"_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));Directory.CreateDirectory(folder);Debug.Log("OuterR1 preview: "+folder);
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                bench.Select(false);bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Body_Front.png"));
                bench.Select(true);
                bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"OuterR1_Front.png"));
                bench.Sample(1.5f,PurpleBenchmarkView.Side);Still(bench,Path.Combine(folder,"OuterR1_Side.png"));bench.Sample(2.4f,PurpleBenchmarkView.Orbit);Still(bench,Path.Combine(folder,"OuterR1_Orbit.png"));
                if(Profile.previewStage>=PurpleOuterStage.Lightning)
                {
                    float best=0,power=-1;
                    for(int frame=0;frame<=90;frame++){bench.Sample(frame/30f,PurpleBenchmarkView.Front);if(bench.Outer.ArcPreviewPower>power){best=frame/30f;power=bench.Outer.ArcPreviewPower;}}
                    bench.Sample(best,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"OuterR1_Burst.png"));
                    Assert.That(bench.Outer.GetComponentsInChildren<ParticleSystem>(true).Length,Is.EqualTo(2));
                    int particles=0;foreach(var ps in bench.Outer.GetComponentsInChildren<ParticleSystem>())particles+=ps.particleCount;
                    Assert.That(particles,Is.GreaterThan(0));
                    File.WriteAllText(Path.Combine(folder,"burst-time.txt"),best.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if(Profile.previewStage==PurpleOuterStage.Distortion)
                {
                    bench.Outer.Stage=PurpleOuterStage.Lightning;bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Distortion_Off.png"));
                    bench.Outer.Stage=PurpleOuterStage.Distortion;bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Distortion_On.png"));
                }
                ShaderCheck();
            }
            finally{ShaderUtil.allowAsyncCompilation=async;}
            Assert.That(main.transform.position,Is.EqualTo(pos));Assert.That(main.fieldOfView,Is.EqualTo(fov));Assert.That(main.targetTexture,Is.SameAs(target));Assert.That(data.renderPostProcessing,Is.EqualTo(post));
            Assert.That(user.transform.position,Is.EqualTo(userPosition));Assert.That(user.transform.childCount,Is.EqualTo(children));
            Assert.That(root.GetComponentsInChildren<JJKGame.Core.Health>(true),Is.Empty);
            root.SetActive(false);Object.Destroy(root);yield return null;yield return null;
            Assert.That(Object.FindFirstObjectByType<PurpleOuterR1Runtime>(),Is.Null);
            foreach(var m in Resources.FindObjectsOfTypeAll<Material>())Assert.That(m.name.StartsWith("PurpleOuterR1_"),Is.False,"OuterR1 material cleanup");
            yield return new ExitPlayMode();
        }
        [UnityTest]public IEnumerator MinimumStillAndMotion()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;Time.timeScale=0;
            var root=new GameObject("OuterR1MinimumRender");var bench=root.AddComponent<PurpleOuterR1Bench>();bench.Configure(Profile,Camera.main);bench.Common.BenchmarkCamera.enabled=false;
            string folder=Path.Combine(Output,"Motion",DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));Directory.CreateDirectory(folder);Debug.Log("OuterR1 motion: "+folder);
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                foreach(var view in new[]{PurpleBenchmarkView.Front,PurpleBenchmarkView.Side,PurpleBenchmarkView.SlightLow})
                {bench.Sample(1.5f,view);Still(bench,Path.Combine(folder,"OuterR1_"+view+".png"));}
                foreach(var view in new[]{PurpleBenchmarkView.Front,PurpleBenchmarkView.Orbit})
                using(var output=new FileStream(Path.Combine(folder,view+".rgb24"),FileMode.CreateNew))
                {
                    for(int frame=0;frame<=90;frame++)
                    {bench.Sample(frame/30f,view);var image=Read(bench);try{byte[] bytes=image.GetRawTextureData<byte>().ToArray();output.Write(bytes,0,bytes.Length);}finally{Object.DestroyImmediate(image);}}
                }
                File.WriteAllText(Path.Combine(folder,"format.json"),"{\"width\":960,\"height\":540,\"fps\":30,\"frames\":91,\"format\":\"RGB24 bottom-up\"}");ShaderCheck();
            }
            finally{ShaderUtil.allowAsyncCompilation=async;}
            root.SetActive(false);Object.Destroy(root);yield return null;yield return null;yield return new ExitPlayMode();
        }
    }
}
