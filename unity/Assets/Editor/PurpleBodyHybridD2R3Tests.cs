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
    public sealed class PurpleBodyHybridD2R3Tests
    {
        private const string Output="D:/JJK_game/unity/Logs/PurpleD2R3QA";
        private static PurpleBodyHybridD2R3Profile Profile=>Resources.Load<PurpleBodyHybridD2R3Profile>("VFX/PurpleBodyHybridD2R3Profile");
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        private static Texture2D Read(PurpleBodyHybridD2R3Bench bench)
        {
            bench.Common.RenderNow();var old=RenderTexture.active;var image=new Texture2D(960,540,TextureFormat.RGB24,false);
            try{RenderTexture.active=bench.Common.Preview;image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();}
            finally{RenderTexture.active=old;}return image;
        }
        private static void Still(PurpleBodyHybridD2R3Bench bench,string file)
        {var image=Read(bench);try{File.WriteAllBytes(file,image.EncodeToPNG());}finally{Object.DestroyImmediate(image);}}
        private static void ShaderCheck()
        {
            foreach(string path in new[]{"VFX/HollowPurpleHybridD2R3","VFX/HollowPurpleHybridD2R3Arc"})
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
            var editor=UnityEditor.Editor.CreateEditor(Profile);Assert.That(editor,Is.TypeOf<PurpleBodyHybridD2R3ProfileEditor>());Object.DestroyImmediate(editor);
            var root=new GameObject("LightweightD2R3PreviewTest");var bench=root.AddComponent<PurpleBodyHybridD2R3Bench>();bench.Configure(Profile,main);bench.Common.BenchmarkCamera.enabled=false;
            string folder=Path.Combine(Output,"Previews",DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));Directory.CreateDirectory(folder);Debug.Log("D2R3 preview: "+folder);
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                bench.Select(false);bench.Sample(1.5f,PurpleBenchmarkView.Front);bench.Common.RenderNow();
                bench.Select(true);bench.Sample(.35f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"R3_Front_035.png"));
                bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"R3_Front.png"));
                bench.Sample(1.5f,PurpleBenchmarkView.Side);Still(bench,Path.Combine(folder,"R3_Side.png"));ShaderCheck();
            }
            finally{ShaderUtil.allowAsyncCompilation=async;}
            Assert.That(main.transform.position,Is.EqualTo(pos));Assert.That(main.fieldOfView,Is.EqualTo(fov));Assert.That(main.targetTexture,Is.SameAs(target));Assert.That(data.renderPostProcessing,Is.EqualTo(post));
            Assert.That(user.transform.position,Is.EqualTo(userPosition));Assert.That(user.transform.childCount,Is.EqualTo(children));
            Assert.That(root.GetComponentsInChildren<JJKGame.Core.Health>(true),Is.Empty);
            root.SetActive(false);Object.Destroy(root);yield return null;yield return null;
            Assert.That(Object.FindFirstObjectByType<PurpleBodyHybridD2R3Prototype>(),Is.Null);
            foreach(var m in Resources.FindObjectsOfTypeAll<Material>())Assert.That(m.name.StartsWith("PurpleHybridD2R3_"),Is.False,"D2R3 material cleanup");
            yield return new ExitPlayMode();
        }
        [UnityTest]public IEnumerator MinimumStillAndMotion()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;Time.timeScale=0;
            var root=new GameObject("D2R3MinimumRender");var bench=root.AddComponent<PurpleBodyHybridD2R3Bench>();bench.Configure(Profile,Camera.main);bench.Common.BenchmarkCamera.enabled=false;
            string folder=Path.Combine(Output,"Motion",DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));Directory.CreateDirectory(folder);Debug.Log("D2R3 motion: "+folder);
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                foreach(var view in new[]{PurpleBenchmarkView.Front,PurpleBenchmarkView.Side,PurpleBenchmarkView.SlightLow})
                {bench.Sample(1.5f,view);Still(bench,Path.Combine(folder,"R3_"+view+".png"));}
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
