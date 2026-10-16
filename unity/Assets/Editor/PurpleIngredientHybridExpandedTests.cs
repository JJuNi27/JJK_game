using System;
using System.Collections;
using System.IO;
using System.Linq;
using JJKGame.Core;
using JJKGame.Player;
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
    public sealed class PurpleIngredientHybridExpandedTests
    {
        private bool oldReboot,oldFinal,oldFinal2,oldOuter,oldHybrid,oldExpanded;
        private static int Materials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m!=null&&(m.name.StartsWith("PurpleProduction_")||m.name.StartsWith("PurplePolish_")||m.name.StartsWith("PurpleIngredient")));
        private static int Meshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m!=null&&m.name.StartsWith("PurpleIngredientExpandedDischarge"));
        private static string Output=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurpleIngredientHybridExpandedQA/Final"));

        [SetUp] public void Capture()
        {
            oldReboot=PurpleIngredientReboot2Profile.Current.candidateEnabled;oldFinal=PurpleIngredientFinalProfile.Current.candidateEnabled;
            oldFinal2=PurpleIngredientFinal2Profile.Current.candidateEnabled;oldOuter=PurpleIngredientOuterExplorationProfile.Current.candidateEnabled;
            oldHybrid=PurpleIngredientHybridProfile.Current.candidateEnabled;oldExpanded=PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled;
        }

        [TearDown] public void Restore()
        {
            PurpleIngredientReboot2Profile.Current.candidateEnabled=oldReboot;PurpleIngredientFinalProfile.Current.candidateEnabled=oldFinal;
            PurpleIngredientFinal2Profile.Current.candidateEnabled=oldFinal2;PurpleIngredientOuterExplorationProfile.Current.candidateEnabled=oldOuter;
            PurpleIngredientHybridProfile.Current.candidateEnabled=oldHybrid;PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled=oldExpanded;
            foreach(var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x!=null&&x.name.StartsWith("ExpandedDischargeLifecycle")))Object.DestroyImmediate(go);
        }

        [Test] public void TopologyChurnMultiScaleSpatialCouplingAndCleanup()
        {
            var profile=PurpleIngredientExpandedDischargeProfile.Current;Assert.That(profile,Is.Not.Null);Assert.That(profile.candidateEnabled,Is.False,"Expanded candidate is saved OFF");
            PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
            PurpleIngredientFinal2Profile.Current.candidateEnabled=true;PurpleIngredientOuterExplorationProfile.Current.candidateEnabled=true;
            PurpleIngredientHybridProfile.Current.candidateEnabled=true;profile.candidateEnabled=true;
            int materialBase=Materials(),meshBase=Meshes();bool allScales=false;
            foreach(bool red in new[]{false,true})
            {
                var host=new GameObject("ExpandedDischargeLifecycle_"+(red?"Red":"Blue"));var ingredient=host.AddComponent<PurpleFusionIngredient>();ingredient.Configure(red);
                var volume=host.GetComponent<PurpleIngredientExpandedDischargeVolume>();Assert.That(volume,Is.Not.Null);
                Assert.That(host.GetComponent<PurpleIngredientOuterDischargeVolume>(),Is.Null,"Expanded candidate must select its independent discharge graph");
                int maxLarge=0,maxMedium=0,maxLocal=0;float maxReach=0,maxCoupling=0;Vector3 lastDirection=Vector3.zero;
                for(int frame=0;frame<480;frame++)
                {
                    float t=frame/120f;ingredient.Render(t,0,Vector3.right*3);
                    maxLarge=Mathf.Max(maxLarge,volume.ActiveLargeCount);maxMedium=Mathf.Max(maxMedium,volume.ActiveMediumCount);maxLocal=Mathf.Max(maxLocal,volume.ActiveLocalCount);
                    maxReach=Mathf.Max(maxReach,volume.MaximumReachSeen);maxCoupling=Mathf.Max(maxCoupling,volume.LastSignal.strength);
                    if(volume.ActiveEventCount>0)Assert.That(volume.LastDisconnectedSegmentCount,Is.GreaterThan(0),"Live event geometry must contain physically absent segments");
                    if(volume.LastSignal.strength>0)lastDirection=volume.LastSignal.localDirection;
                }
                Assert.That(volume.TopologyRevisionCount,Is.GreaterThan(60));Assert.That(volume.BranchAttachmentChangeCount,Is.GreaterThan(8));
                Assert.That(maxLarge,Is.GreaterThan(0));Assert.That(maxMedium,Is.GreaterThan(0));Assert.That(maxLocal,Is.GreaterThan(0));allScales=true;
                Assert.That(maxReach,Is.GreaterThan(PurpleIngredientOuterExplorationProfile.Current.energyReach*2.1f));
                Assert.That(maxCoupling,Is.GreaterThan(.1f));Assert.That(lastDirection.sqrMagnitude,Is.GreaterThan(.8f));
                Object.DestroyImmediate(host);EditorUtility.UnloadUnusedAssetsImmediate();
                Assert.That(Materials(),Is.EqualTo(materialBase),"Hybrid-expanded materials return to the pre-test baseline");Assert.That(Meshes(),Is.EqualTo(meshBase));
            }
            foreach(var shaderPath in new[]{"VFX/PurpleIngredientExpandedDischarge","VFX/HollowPurpleIngredientReboot"})
            {
                var shader=Resources.Load<Shader>(shaderPath);Assert.That(shader,Is.Not.Null,shaderPath);
                foreach(var msg in ShaderUtil.GetShaderMessages(shader))Assert.That(msg.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),shaderPath+": "+msg.message);
            }
            Directory.CreateDirectory(Output);File.WriteAllText(Path.Combine(Output,"lifecycle.json"),"{\"large_medium_local_active\":"+allScales.ToString().ToLowerInvariant()+",\"event_resources_return_to_baseline\":true,\"shader_checks\":2}");
        }

        [UnityTest] public IEnumerator SideCasterHybridVsExpandedAtRealTime()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            string output=Path.Combine(Output,DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));Directory.CreateDirectory(output);
            PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
            PurpleIngredientFinal2Profile.Current.candidateEnabled=true;PurpleIngredientOuterExplorationProfile.Current.candidateEnabled=true;
            PurpleIngredientHybridProfile.Current.candidateEnabled=true;PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled=false;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/CombatMVP.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            Time.timeScale=0;var actor=GameObject.Find("GojoPlayer");Assert.That(actor,Is.Not.Null);var stage=new Vector3(512,0,512);actor.transform.SetPositionAndRotation(stage,Quaternion.identity);
            var caster=Camera.main;Assert.That(caster,Is.Not.Null);caster.enabled=false;caster.fieldOfView=60;
            var side=new GameObject("HybridExpandedQAObserver").AddComponent<Camera>();side.CopyFrom(caster);side.enabled=false;side.fieldOfView=45;side.farClipPlane=180;
            var sideData=side.gameObject.AddComponent<UniversalAdditionalCameraData>();var casterData=caster.GetComponent<UniversalAdditionalCameraData>();
            sideData.renderPostProcessing=casterData.renderPostProcessing;sideData.volumeLayerMask=casterData.volumeLayerMask;sideData.requiresColorTexture=true;sideData.requiresDepthTexture=true;
            var target=new RenderTexture(960,540,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);target.Create();caster.targetTexture=target;side.targetTexture=target;
            Vector3 origin=PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(stage,Vector3.forward);int materialStart=Materials(),meshStart=Meshes();
            try
            {
                for(int mode=0;mode<2;mode++)foreach(bool casterView in new[]{false,true})
                {
                    actor.transform.SetPositionAndRotation(stage,Quaternion.identity);caster.transform.SetPositionAndRotation(stage+new Vector3(0,4,-11),Quaternion.Euler(12,0,0));
                    PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled=mode==1;
                    var host=new GameObject("HybridExpandedABSequence");var sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,actor.transform,0,casterView?caster:null,casterView);
                    string stem=(casterView?"Caster":"Side")+(mode==0?"_Baseline":"_Candidate");
                    try
                    {
                        using(var stream=new FileStream(Path.Combine(output,stem+".rgb24"),FileMode.CreateNew))
                        for(int frame=0;frame<=96;frame++)
                        {
                            float t=frame/30f;sequence.Update(t,1/30f);
                            Vector3 offset=new Vector3(5,2,-12);side.transform.SetPositionAndRotation(origin+Vector3.forward*Mathf.Clamp01((t-GojoPolishSettings.Current.PurpleReleaseTime)/1.6f)*48+offset,Quaternion.LookRotation(-offset));
                            Camera view=casterView?caster:side;RenderPipeline.SubmitRenderRequest(view,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
                            var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(960,540,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();RenderTexture.active=previous;
                            stream.Write(image.GetRawTextureData<byte>().ToArray(),0,960*540*3);
                            if(frame==23||frame==48||frame==72||frame==96)File.WriteAllBytes(Path.Combine(output,stem+"_"+frame.ToString("D3")+".png"),image.EncodeToPNG());Object.DestroyImmediate(image);
                        }
                    }
                    finally{sequence.Dispose();Object.Destroy(host);}
                    for(int settle=0;settle<8&&(Materials()>materialStart||Meshes()>meshStart);settle++)yield return null;
                    Assert.That(Materials(),Is.EqualTo(materialStart),"Sequence-owned Purple materials return to the pre-render baseline");
                    Assert.That(Meshes(),Is.EqualTo(meshStart),"Expanded discharge meshes are released after sequence disposal");
                }
                File.WriteAllText(Path.Combine(output,"format.json"),"{\"width\":960,\"height\":540,\"fps\":30,\"frames\":97,\"duration_seconds\":3.2333,\"playback\":\"real-time\",\"baseline\":\"current HYBRID\",\"candidate\":\"EXPANDED HYBRID\"}");
                File.WriteAllText(Path.Combine(output,"cleanup.json"),"{\"owned_materials_before\":"+materialStart+",\"owned_materials_after_sequences\":"+Materials()+",\"material_delta\":"+(Materials()-materialStart)+",\"expanded_meshes_before\":"+meshStart+",\"expanded_meshes_after_sequences\":"+Meshes()+",\"expanded_mesh_delta\":"+(Meshes()-meshStart)+"}");
            }
            finally
            {
                PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled=oldExpanded;caster.targetTexture=null;side.targetTexture=null;
                target.Release();Object.Destroy(target);Object.Destroy(side.gameObject);caster.enabled=true;Time.timeScale=1;
            }
            yield return new ExitPlayMode();
        }
    }
}
