using System;
using System.Collections;
using System.IO;
using JJKGame.Dev.PurpleBodyExploration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.EditorTools
{
    public sealed class PurpleBodyExplorationTests
    {
        private const string Output="D:/JJK_game/unity/Logs/PurpleBodyExplorationQA";
        private static string frameOutput;
        [UnityTearDown] public IEnumerator Cleanup()
        { Time.timeScale=1; if(EditorApplication.isPlaying) yield return new ExitPlayMode(); }

        private static int OwnedMaterialCount()
        {
            int count=0;foreach(var m in Resources.FindObjectsOfTypeAll<Material>()) if(m.name.StartsWith("PurpleBodyExplore_"))count++;
            return count;
        }
        [Test] public void CommonProfileHasKoreanInspectorAndNoProductionReference()
        {
            var profile=Resources.Load<PurpleBodyExplorationProfile>("VFX/PurpleBodyExplorationProfile");
            Assert.That(profile,Is.Not.Null); Assert.That(profile.coreRatio,Is.InRange(.15f,.20f));
            var editor=UnityEditor.Editor.CreateEditor(profile); Assert.That(editor,Is.TypeOf<PurpleBodyExplorationProfileEditor>());
            var so=new SerializedObject(profile);var p=so.GetIterator();bool children=true;
            while(p.NextVisible(children))
            {
                children=false;if(p.name=="m_Script")continue;
                Assert.That(System.Text.RegularExpressions.Regex.IsMatch(PurpleBodyExplorationProfileEditor.Label(p.name),"[가-힣]"),Is.True,p.name);
                Assert.That(p.propertyType,Is.Not.EqualTo(SerializedPropertyType.ObjectReference),"No production data/material reference");
            }
            Object.DestroyImmediate(editor);
        }

        [UnityTest] public IEnumerator SwitchingAndClosingPreservesSceneUserPrototypeAndCamera()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;
            var user=GameObject.Find("Purple_UserPrototype");Assert.That(user,Is.Not.Null);
            Vector3 userPosition=user.transform.position;int childCount=user.transform.childCount;
            var sentinel=new GameObject("TestOwnedMaterialSentinel",typeof(MeshRenderer));
            var userMat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));sentinel.GetComponent<Renderer>().sharedMaterial=userMat;
            var main=Camera.main;var data=main.GetComponent<UniversalAdditionalCameraData>();
            bool post=data.renderPostProcessing;int layers=data.volumeLayerMask;float fov=main.fieldOfView;
            var profile=Resources.Load<PurpleBodyExplorationProfile>("VFX/PurpleBodyExplorationProfile");
            int materials=OwnedMaterialCount();
            for(int repeat=0;repeat<3;repeat++)
            {
                Vector3 cameraPosition=main.transform.position;Quaternion cameraRotation=main.transform.rotation;
                var root=new GameObject("TestOnlyExploration");var bench=root.AddComponent<PurpleBodyExplorationBench>();bench.Configure(profile,main,320,180);
                Assert.That(bench.BenchmarkCamera,Is.Not.SameAs(main));Assert.That(bench.BenchmarkCamera.tag,Is.EqualTo("Untagged"));
                var comparisonData=bench.BenchmarkCamera.GetComponent<UniversalAdditionalCameraData>();
                Assert.That(comparisonData.renderPostProcessing,Is.EqualTo(post)); Assert.That((int)comparisonData.volumeLayerMask,Is.EqualTo(layers));
                bench.Sample(1.5f,PurpleBenchmarkView.SlightLow);
                Vector3 shot=bench.BenchmarkCamera.transform.position;Quaternion angle=bench.BenchmarkCamera.transform.rotation;
                for(int variant=0;variant<3;variant++)
                {
                    bench.Select((PurpleBodyVariant)variant);
                    Assert.That(bench.Clock,Is.EqualTo(1.5f));
                    Assert.That(Vector3.Distance(shot,bench.BenchmarkCamera.transform.position),Is.LessThan(.00001f));
                    Assert.That(Quaternion.Angle(angle,bench.BenchmarkCamera.transform.rotation),Is.LessThan(.001f));
                    var body=bench.Candidate((PurpleBodyVariant)variant);
                    Assert.That(body.transform.position,Is.EqualTo(bench.Centre));
                    Assert.That(body.PreviewMaterial.GetFloat("_CoreRatio"),Is.EqualTo(profile.coreRatio));
                    Assert.That(body.PreviewMaterial.GetFloat("_Density"),Is.EqualTo(profile.density));
                    Assert.That(root.GetComponentsInChildren<ParticleSystem>(true),Is.Empty);
                    Assert.That(root.GetComponentsInChildren<LineRenderer>(true),Is.Empty);
                    Assert.That(root.GetComponentsInChildren<JJKGame.Core.Health>(true),Is.Empty);
                    Assert.That(root.GetComponentsInChildren<JJKGame.Player.PurpleEnergyBody>(true),Is.Empty);
                }
                Assert.That(main.transform.position,Is.EqualTo(cameraPosition));Assert.That(Quaternion.Angle(main.transform.rotation,cameraRotation),Is.LessThan(.001f));
                Assert.That(main.fieldOfView,Is.EqualTo(fov));Assert.That(data.renderPostProcessing,Is.EqualTo(post));
                Assert.That(user.transform.position,Is.EqualTo(userPosition));Assert.That(user.transform.childCount,Is.EqualTo(childCount));
                Assert.That(sentinel.GetComponent<Renderer>().sharedMaterial,Is.SameAs(userMat));
                root.SetActive(false);Object.Destroy(root);yield return null;yield return null;
                Assert.That(Object.FindFirstObjectByType<PurpleBodyExplorationBench>(),Is.Null);
                Assert.That(OwnedMaterialCount(),Is.EqualTo(materials));
            }
            Object.Destroy(sentinel);Object.Destroy(userMat);yield return new ExitPlayMode();
        }

        [UnityTest] public IEnumerator EqualConditionFrontSideLowAndThreeSecondMotionRender()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;
            // Every run keeps its own evidence; rerunning tests must not replace a reviewed comparison.
            frameOutput=Path.Combine(Output,"Runs",DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff")+"_"+Guid.NewGuid().ToString("N").Substring(0,8));
            Directory.CreateDirectory(frameOutput);
            Debug.Log("Purple body comparison frames: "+frameOutput);
            var root=new GameObject("ExplorationRenderReview");var bench=root.AddComponent<PurpleBodyExplorationBench>();
            var profile=Resources.Load<PurpleBodyExplorationProfile>("VFX/PurpleBodyExplorationProfile");
            bench.Configure(profile,Camera.main,960,540);bench.BenchmarkCamera.enabled=false;
            Time.timeScale=0;
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                for(int variant=0;variant<3;variant++)
                {
                    bench.Select((PurpleBodyVariant)variant);string label=((char)('A'+variant)).ToString();
                    foreach(var view in new[]{PurpleBenchmarkView.Front,PurpleBenchmarkView.Side,PurpleBenchmarkView.SlightLow})
                    { bench.Sample(1.5f,view);Capture(bench,label+"_"+view+"_still"); }
                    var cameraData=bench.BenchmarkCamera.GetComponent<UniversalAdditionalCameraData>();
                    bool post=cameraData.renderPostProcessing;
                    try
                    { cameraData.renderPostProcessing=false;bench.Sample(1.5f,PurpleBenchmarkView.Front);Capture(bench,label+"_Front_NoPost"); }
                    finally { cameraData.renderPostProcessing=post; }
                    foreach(var view in new[]{PurpleBenchmarkView.Front,PurpleBenchmarkView.Orbit})
                    {
                        for(int frame=0;frame<=90;frame++)
                        { bench.Sample(frame/30f,view);Capture(bench,label+"_"+view+"_"+frame.ToString("D3")); }
                    }
                }
                var shader=Resources.Load<Shader>("VFX/HollowPurpleBodyExploration");Assert.That(shader.isSupported,Is.True);
                foreach(var message in ShaderUtil.GetShaderMessages(shader))
                    Assert.That(message.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),message.message);
            }
            finally { ShaderUtil.allowAsyncCompilation=async; }
            // Also exercise the real MonoBehaviour update clock, separate from deterministic frame sampling.
            bench.Sample(0,PurpleBenchmarkView.Front);bench.Playing=true;
            double deadline=EditorApplication.timeSinceStartup+12;
            while(bench.Playing) { Assert.That(EditorApplication.timeSinceStartup,Is.LessThan(deadline));yield return null; }
            Assert.That(bench.Clock,Is.EqualTo(3));
            Time.timeScale=1;root.SetActive(false);Object.Destroy(root);yield return null;yield return null;
            Assert.That(Object.FindFirstObjectByType<PurpleBodyPrototype>(),Is.Null);
            yield return new ExitPlayMode();
        }
        private static void Capture(PurpleBodyExplorationBench bench,string label)
        {
            Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(GraphicsDeviceType.Null));
            bench.RenderNow();var previous=RenderTexture.active;
            var image=new Texture2D(bench.Preview.width,bench.Preview.height,TextureFormat.RGB24,false);
            try
            {
                RenderTexture.active=bench.Preview;image.ReadPixels(new Rect(0,0,image.width,image.height),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(frameOutput,label+".png"),image.EncodeToPNG());
            }
            finally { RenderTexture.active=previous;Object.DestroyImmediate(image); }
        }
    }
}
