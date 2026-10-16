using System;
using System.Collections;
using System.IO;
using System.Linq;
using JJKGame.Player;
using JJKGame.Core;
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
    public sealed class PurpleIngredientHybridTests
    {
        private bool oldReboot,oldFinal,oldFinal2,oldOuter,oldHybrid;
        private static int Materials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m!=null&&m.name.StartsWith("PurpleIngredient"));
        private static int Meshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m!=null&&(m.name.StartsWith("PurpleIngredient")||m.name.StartsWith("PurpleIngredientOuterExploration")));

        [SetUp] public void CaptureSettings()
        {
            oldReboot=PurpleIngredientReboot2Profile.Current.candidateEnabled;
            oldFinal=PurpleIngredientFinalProfile.Current.candidateEnabled;
            oldFinal2=PurpleIngredientFinal2Profile.Current.candidateEnabled;
            oldOuter=PurpleIngredientOuterExplorationProfile.Current.candidateEnabled;
            oldHybrid=PurpleIngredientHybridProfile.Current.candidateEnabled;
        }

        [TearDown] public void RestoreSettings()
        {
            PurpleIngredientReboot2Profile.Current.candidateEnabled=oldReboot;
            PurpleIngredientFinalProfile.Current.candidateEnabled=oldFinal;
            PurpleIngredientFinal2Profile.Current.candidateEnabled=oldFinal2;
            PurpleIngredientOuterExplorationProfile.Current.candidateEnabled=oldOuter;
            PurpleIngredientHybridProfile.Current.candidateEnabled=oldHybrid;
            foreach(var host in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(go=>go!=null&&go.name=="HybridIngredientLifecycle"))Object.DestroyImmediate(host);
        }

        [Test] public void DischargeIsEventDrivenDirectionalAndCleansUp()
        {
            var profile=PurpleIngredientHybridProfile.Current;
            Assert.That(profile,Is.Not.Null);Assert.That(profile.candidateEnabled,Is.False,"Saved Hybrid candidate defaults off");
            var shader=Resources.Load<Shader>("VFX/PurpleIngredientOuterDischarge");Assert.That(shader,Is.Not.Null);
            PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
            PurpleIngredientFinal2Profile.Current.candidateEnabled=true;PurpleIngredientOuterExplorationProfile.Current.candidateEnabled=true;profile.candidateEnabled=true;
            int materialBaseline=Materials(),meshBaseline=Meshes();
            foreach(bool red in new[]{false,true})
            {
                var host=new GameObject("HybridIngredientLifecycle");var ingredient=host.AddComponent<PurpleFusionIngredient>();ingredient.Configure(red);
                var discharge=host.GetComponent<PurpleIngredientOuterDischargeVolume>();var flow=host.GetComponent<PurpleIngredientOuterExplorationVolume>();
                Assert.That(discharge,Is.Not.Null);Assert.That(flow,Is.Not.Null);
                Assert.That(host.GetComponentsInChildren<MeshFilter>().Count(f=>f.sharedMesh!=null&&f.sharedMesh.name.StartsWith("PurpleIngredientDischarge_")),Is.EqualTo(12));
                int activeFrames=0,starts=0,maxActive=0;bool wasActive=false;float maxReach=0,maxCoupling=0,startRadius=0,endRadius=0;
                for(int frame=0;frame<480;frame++)
                {
                    float clock=frame/120f;ingredient.Render(clock,0,Vector3.right*3);
                    bool active=discharge.ActiveEventCount>0;activeFrames+=active?1:0;maxActive=Mathf.Max(maxActive,discharge.ActiveEventCount);
                    if(active&&!wasActive)starts++;wasActive=active;maxReach=Mathf.Max(maxReach,discharge.LastReach);maxCoupling=Mathf.Max(maxCoupling,discharge.LastCoupling);
                    if(active){startRadius=Mathf.Max(startRadius,discharge.LastStartRadius);endRadius=Mathf.Max(endRadius,discharge.LastEndRadius);}
                }
                Assert.That(starts,Is.GreaterThan(3),"Independent bursts must repeatedly appear after gaps");
                Assert.That(activeFrames,Is.LessThan(360),"Energy events must have visible no-event intervals");
                Assert.That(maxActive,Is.GreaterThan(1),"Multiple irregular discharges should overlap");
                Assert.That(maxReach,Is.GreaterThan(PurpleIngredientOuterExplorationProfile.Current.energyReach*1.6f));
                Assert.That(maxCoupling,Is.GreaterThan(.2f),"A discharge must drive body/flow coupling");
                Assert.That(red?endRadius>startRadius*2f:startRadius>endRadius*2f,Is.True,"Blue must collapse inward while Red expands outward");
                Object.DestroyImmediate(host);EditorUtility.UnloadUnusedAssetsImmediate();
                Assert.That(Materials(),Is.LessThanOrEqualTo(materialBaseline));Assert.That(Meshes(),Is.EqualTo(meshBaseline));
            }
            foreach(var message in ShaderUtil.GetShaderMessages(shader))Assert.That(message.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),message.message);
        }

        [UnityTest] public IEnumerator SideCasterBaselineCandidateMotion()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurpleIngredientHybridQA/Final/"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff")));
            Directory.CreateDirectory(output);
            PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
            PurpleIngredientFinal2Profile.Current.candidateEnabled=true;PurpleIngredientOuterExplorationProfile.Current.candidateEnabled=true;PurpleIngredientHybridProfile.Current.candidateEnabled=false;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/CombatMVP.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            Time.timeScale=0;var actor=GameObject.Find("GojoPlayer");Assert.That(actor,Is.Not.Null);
            var stage=new Vector3(512,0,512);actor.transform.SetPositionAndRotation(stage,Quaternion.identity);
            var caster=Camera.main;Assert.That(caster,Is.Not.Null);caster.enabled=false;caster.fieldOfView=60;
            var side=new GameObject("HybridQAObserver").AddComponent<Camera>();side.CopyFrom(caster);side.enabled=false;side.fieldOfView=45;side.farClipPlane=180;
            var sideData=side.gameObject.AddComponent<UniversalAdditionalCameraData>();var casterData=caster.GetComponent<UniversalAdditionalCameraData>();
            sideData.renderPostProcessing=casterData.renderPostProcessing;sideData.volumeLayerMask=casterData.volumeLayerMask;sideData.requiresColorTexture=true;sideData.requiresDepthTexture=true;
            var rt=new RenderTexture(960,540,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);rt.Create();caster.targetTexture=rt;side.targetTexture=rt;
            Vector3 origin=PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(stage,Vector3.forward);
            try
            {
                for(int variant=0;variant<2;variant++)
                foreach(bool casterView in new[]{false,true})
                {
                    actor.transform.SetPositionAndRotation(stage,Quaternion.identity);caster.transform.SetPositionAndRotation(stage+new Vector3(0,4,-11),Quaternion.Euler(12,0,0));
                    PurpleIngredientHybridProfile.Current.candidateEnabled=variant==1;
                    var host=new GameObject("HybridABSequence");var sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,actor.transform,0,casterView?caster:null,casterView);
                    string prefix=(casterView?"Caster":"Side")+(variant==0?"_Baseline":"_Candidate");
                    try
                    {
                        using(var stream=new FileStream(Path.Combine(output,prefix+".rgb24"),FileMode.CreateNew))
                        for(int frame=0;frame<=96;frame++)
                        {
                            float time=frame/30f;sequence.Update(time,1/30f);
                            Vector3 offset=new Vector3(5,2,-12);side.transform.SetPositionAndRotation(origin+Vector3.forward*Mathf.Clamp01((time-GojoPolishSettings.Current.PurpleReleaseTime)/1.6f)*48+offset,Quaternion.LookRotation(-offset));
                            Camera view=casterView?caster:side;RenderPipeline.SubmitRenderRequest(view,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});
                            var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(960,540,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();RenderTexture.active=previous;
                            stream.Write(image.GetRawTextureData<byte>().ToArray(),0,960*540*3);if(frame==23||frame==48||frame==72||frame==96)File.WriteAllBytes(Path.Combine(output,prefix+"_"+frame.ToString("D3")+".png"),image.EncodeToPNG());Object.DestroyImmediate(image);
                        }
                    }
                    finally{sequence.Dispose();Object.Destroy(host);}
                    yield return null;yield return null;Assert.That(Materials(),Is.Zero,"Hybrid and Purple materials must be released");
                }
                File.WriteAllText(Path.Combine(output,"format.json"),"{\"width\":960,\"height\":540,\"fps\":30,\"frames\":97,\"duration_seconds\":3.2333,\"playback\":\"real-time\",\"camera_restore\":true}");
            }
            finally{PurpleIngredientHybridProfile.Current.candidateEnabled=false;caster.targetTexture=null;side.targetTexture=null;rt.Release();Object.Destroy(rt);Object.Destroy(side.gameObject);caster.enabled=true;Time.timeScale=1;}
            yield return new ExitPlayMode();
        }
    }
}
