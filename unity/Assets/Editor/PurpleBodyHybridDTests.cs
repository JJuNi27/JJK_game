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
    public sealed class PurpleBodyHybridDTests
    {
        [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        private static PurpleBodyHybridDProfile Profile=>Resources.Load<PurpleBodyHybridDProfile>("VFX/PurpleBodyHybridDProfile");
        private static int MaterialCount()
        {
            int count=0;
            foreach(var m in Resources.FindObjectsOfTypeAll<Material>())
                if(m.name.StartsWith("PurpleBodyExplore_") || m.name.StartsWith("PurpleHybridD_"))count++;
            return count;
        }
        [Test] public void SeparateProfileHasKoreanInspectorAndReadsPreservedCommonConditions()
        {
            Assert.That(Profile,Is.Not.Null);
            Assert.That(Profile.commonConditions,Is.SameAs(Resources.Load<PurpleBodyExplorationProfile>("VFX/PurpleBodyExplorationProfile")));
            Assert.That(Profile.commonConditions.coreRatio,Is.InRange(.15f,.20f));
            var editor=UnityEditor.Editor.CreateEditor(Profile);Assert.That(editor,Is.TypeOf<PurpleBodyHybridDProfileEditor>());
            var so=new SerializedObject(Profile);var p=so.GetIterator();bool children=true;
            while(p.NextVisible(children))
            {
                children=false;if(p.name=="m_Script")continue;
                Assert.That(System.Text.RegularExpressions.Regex.IsMatch(PurpleBodyHybridDProfileEditor.Label(p.name),"[가-힣]"),Is.True,p.name);
                if(p.propertyType==SerializedPropertyType.ObjectReference)Assert.That(p.name,Is.EqualTo("commonConditions"));
            }
            Object.DestroyImmediate(editor);
        }
        [UnityTest] public IEnumerator SwitchingClosingAndConcurrentABCDoNotChangeProtectedObjects()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;Time.timeScale=0;
            var main=Camera.main;var data=main.GetComponent<UniversalAdditionalCameraData>();
            var user=GameObject.Find("Purple_UserPrototype");Assert.That(user,Is.Not.Null);
            var userPos=user.transform.position;var userRot=user.transform.rotation;var userScale=user.transform.localScale;int children=user.transform.childCount;
            var oldRoot=new GameObject("ExistingABCWindowTest");var old=oldRoot.AddComponent<PurpleBodyExplorationBench>();old.Configure(Profile.commonConditions,main,320,180);
            old.BenchmarkCamera.enabled=false;old.Sample(1.5f,PurpleBenchmarkView.Front);byte[] abcBefore=ReadFrame(old);
            int count=MaterialCount();
            for(int repeat=0;repeat<3;repeat++)
            {
                var mainPos=main.transform.position;var mainRot=main.transform.rotation;float fov=main.fieldOfView;
                bool post=data.renderPostProcessing;int mask=data.volumeLayerMask;var texture=main.targetTexture;
                var root=new GameObject("OwnedDComparisonTest");var d=root.AddComponent<PurpleBodyHybridDBench>();d.Configure(Profile,main,320,180);
                Assert.That(Vector3.Distance(old.Centre,d.Centre),Is.GreaterThan(2*old.BenchmarkCamera.farClipPlane));
                d.CommonBench.BenchmarkCamera.enabled=false;d.Sample(1.5f,PurpleBenchmarkView.SlightLow);
                var shot=d.CommonBench.BenchmarkCamera.transform.position;var angle=d.CommonBench.BenchmarkCamera.transform.rotation;
                for(int variant=0;variant<4;variant++)
                {
                    d.Select(variant);Assert.That(d.CommonBench.Clock,Is.EqualTo(1.5f));
                    Assert.That(d.CommonBench.BenchmarkCamera.transform.position,Is.EqualTo(shot));
                    Assert.That(Quaternion.Angle(d.CommonBench.BenchmarkCamera.transform.rotation,angle),Is.LessThan(.001f));
                    Assert.That(d.Hybrid.gameObject.activeSelf,Is.EqualTo(variant==3));
                    for(int i=0;i<3;i++)
                    {
                        var body=d.CommonBench.Candidate((PurpleBodyVariant)i);
                        Assert.That(body.gameObject.activeSelf,Is.EqualTo(variant==i));Assert.That(body.transform.position,Is.EqualTo(d.Centre));
                    }
                    Assert.That(root.GetComponentsInChildren<ParticleSystem>(true),Is.Empty);
                    Assert.That(root.GetComponentsInChildren<LineRenderer>(true),Is.Empty);
                    Assert.That(root.GetComponentsInChildren<JJKGame.Core.Health>(true),Is.Empty);
                    Assert.That(root.GetComponentsInChildren<JJKGame.Player.PurpleEnergyBody>(true),Is.Empty);
                }
                Assert.That(d.Hybrid.PreviewMaterial.GetFloat("_CoreRatio"),Is.EqualTo(Profile.commonConditions.coreRatio));
                Assert.That(d.Hybrid.PreviewMaterial.GetColor("_NearBlack"),Is.EqualTo(Profile.commonConditions.nearBlack));
                Assert.That(main.transform.position,Is.EqualTo(mainPos));Assert.That(Quaternion.Angle(main.transform.rotation,mainRot),Is.LessThan(.001f));
                Assert.That(main.fieldOfView,Is.EqualTo(fov));Assert.That(main.targetTexture,Is.SameAs(texture));
                Assert.That(data.renderPostProcessing,Is.EqualTo(post));Assert.That((int)data.volumeLayerMask,Is.EqualTo(mask));
                Assert.That(user.transform.position,Is.EqualTo(userPos));Assert.That(user.transform.rotation,Is.EqualTo(userRot));
                Assert.That(user.transform.localScale,Is.EqualTo(userScale));Assert.That(user.transform.childCount,Is.EqualTo(children));
                CollectionAssert.AreEqual(abcBefore,ReadFrame(old),"Opening D must not appear inside the existing ABC preview");
                root.SetActive(false);Object.Destroy(root);yield return null;yield return null;
                Assert.That(Object.FindFirstObjectByType<PurpleBodyHybridDBench>(),Is.Null);Assert.That(MaterialCount(),Is.EqualTo(count));
            }
            oldRoot.SetActive(false);Object.Destroy(oldRoot);yield return null;yield return null;yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator EqualConditionABCDStillAndThreeSecondRender()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;
            string path=Path.Combine("D:/JJK_game/unity/Logs/PurpleHybridDQA/Runs",DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff")+"_"+Guid.NewGuid().ToString("N").Substring(0,8));
            Directory.CreateDirectory(path);Debug.Log("Hybrid D comparison frames: "+path);
            var root=new GameObject("HybridDRenderReview");var d=root.AddComponent<PurpleBodyHybridDBench>();d.Configure(Profile,Camera.main);
            var common=d.CommonBench;common.BenchmarkCamera.enabled=false;Time.timeScale=0;
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                for(int variant=0;variant<4;variant++)
                {
                    d.Select(variant);string label=((char)('A'+variant)).ToString();
                    foreach(var view in new[]{PurpleBenchmarkView.Front,PurpleBenchmarkView.Side,PurpleBenchmarkView.SlightLow})
                    {d.Sample(1.5f,view);File.WriteAllBytes(Path.Combine(path,label+"_"+view+"_still.png"),ReadFrame(common));}
                    var cameraData=common.BenchmarkCamera.GetComponent<UniversalAdditionalCameraData>();bool post=cameraData.renderPostProcessing;
                    try{cameraData.renderPostProcessing=false;d.Sample(1.5f,PurpleBenchmarkView.Front);File.WriteAllBytes(Path.Combine(path,label+"_Front_NoPost.png"),ReadFrame(common));}
                    finally{cameraData.renderPostProcessing=post;}
                    foreach(var view in new[]{PurpleBenchmarkView.Front,PurpleBenchmarkView.Orbit})
                    for(int frame=0;frame<=90;frame++)
                    {d.Sample(frame/30f,view);File.WriteAllBytes(Path.Combine(path,label+"_"+view+"_"+frame.ToString("D3")+".png"),ReadFrame(common));}
                }
                var shader=Resources.Load<Shader>("VFX/HollowPurpleHybridD");Assert.That(shader.isSupported,Is.True);
                foreach(var message in ShaderUtil.GetShaderMessages(shader))
                    Assert.That(message.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),message.message);
            }
            finally{ShaderUtil.allowAsyncCompilation=async;}
            d.Sample(0,PurpleBenchmarkView.Front);d.Playing=true;double deadline=EditorApplication.timeSinceStartup+12;
            while(d.Playing){Assert.That(EditorApplication.timeSinceStartup,Is.LessThan(deadline));yield return null;}
            Assert.That(common.Clock,Is.EqualTo(3));Time.timeScale=1;root.SetActive(false);Object.Destroy(root);yield return null;yield return null;
            Assert.That(Object.FindFirstObjectByType<PurpleBodyHybridDPrototype>(),Is.Null);yield return new ExitPlayMode();
        }
        private static byte[] ReadFrame(PurpleBodyExplorationBench common)
        {
            Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(GraphicsDeviceType.Null));common.RenderNow();
            var previous=RenderTexture.active;var image=new Texture2D(common.Preview.width,common.Preview.height,TextureFormat.RGB24,false);
            try{RenderTexture.active=common.Preview;image.ReadPixels(new Rect(0,0,image.width,image.height),0,0);image.Apply();return image.EncodeToPNG();}
            finally{RenderTexture.active=previous;Object.DestroyImmediate(image);}
        }
    }
}
