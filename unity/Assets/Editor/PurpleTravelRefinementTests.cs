using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public sealed class PurpleTravelRefinementTests
    {
        private const string Output="D:/JJK_game/unity/Logs/PurpleTravelRefinementQA";
        private static PurpleTravelVisualProfile Profile=>Resources.Load<PurpleTravelVisualProfile>("VFX/PurpleTravelVisualProfile");
        private static object Call(object o,string method,params object[] args)=>o.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(o,args);
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        private static int OwnedMaterials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("PurpleProduction_"));
        private static void ShaderCheck()
        {
            foreach(string s in new[]{"PurpleTravelWake","PurpleTravelHalo","PurpleTravelDistortion"})
            {var shader=Resources.Load<Shader>("VFX/"+s);Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);foreach(var m in ShaderUtil.GetShaderMessages(shader))Assert.That(m.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),m.message);}
        }
        [UnityTest]public IEnumerator TravelTargetsRangeAndRepeatedCleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            int baseline=OwnedMaterials(),peak=0;float maximumLag=0;
            for(int pass=0;pass<3;pass++)
            {
                var owner=new GameObject("TravelCaster",typeof(GojoTechniqueChainController));var chain=owner.GetComponent<GojoTechniqueChainController>();
                var host=new GameObject("TravelLifecycle");Vector3 origin=PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(Vector3.zero,Vector3.forward);
                GameObject target=null,behind=null;
                if(pass==0)
                {
                    target=new GameObject("First",typeof(Health),typeof(BoxCollider));target.transform.position=origin+Vector3.forward*12;
                    behind=new GameObject("Behind",typeof(Health),typeof(BoxCollider));behind.transform.position=origin+Vector3.forward*30;
                }
                Physics.SyncTransforms();Call(chain,"QueuePurpleDamage",Vector3.forward);
                float start=chain.PurplePresentationStartedAt;var sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,owner.transform,start,null,false);
                double deadline=EditorApplication.timeSinceStartup+25;bool sawTravel=false;
                while(Time.unscaledTime<start+GojoPolishSettings.Current.PurpleReleaseTime+chain.PurpleLaunchDuration+.06f)
                {
                    Assert.That(EditorApplication.timeSinceStartup,Is.LessThan(deadline));sequence.Update(Time.unscaledTime,Time.unscaledDeltaTime);
                    var outer=host.GetComponentInChildren<ProductionPurpleOuterRuntime>();
                    if(outer!=null)
                    {
                        if(Time.unscaledTime<start+GojoPolishSettings.Current.PurpleReleaseTime)Assert.That(outer.TravelResponseActive,Is.False);
                        sawTravel|=outer.TravelResponseActive;maximumLag=Mathf.Max(maximumLag,outer.MaxResidualDistance);
                        int count=0;foreach(var ps in outer.GetComponentsInChildren<ParticleSystem>())count+=ps.particleCount;peak=Mathf.Max(peak,count);
                    }
                    if(pass==2 && sawTravel){owner.SetActive(false);Assert.That(sequence.Update(Time.unscaledTime,0),Is.False);break;}
                    yield return null;
                }
                Assert.That(sawTravel,Is.True);
                if(pass<2)
                {
                    Assert.That(sequence.HasTerminated,Is.True);Assert.That(host.GetComponentInChildren<ProductionPurpleVisual>(),Is.Null);
                    var terminal=host.GetComponentInChildren<PurpleTerminalBurst>();Assert.That(terminal,Is.Not.Null);
                    Assert.That(terminal.GetComponentsInChildren<ProductionPurpleOuterRuntime>(true),Is.Empty,"Travel effect must never enter terminal");
                    Assert.That(terminal.GetComponentInChildren<PurpleEnergyBody>(true).UsesProductionCandidate,Is.False);
                    Assert.That(Vector3.Distance(sequence.TerminalPosition,origin+Vector3.forward*(pass==0?12:chain.PurpleRange)),Is.LessThan(.02f));
                    if(pass==0){Assert.That(target.GetComponent<Health>().CurrentHealth,Is.EqualTo(45));Assert.That(behind.GetComponent<Health>().CurrentHealth,Is.EqualTo(100));}
                }
                sequence.Dispose();Object.Destroy(host);Object.Destroy(owner);if(target!=null)Object.Destroy(target);if(behind!=null)Object.Destroy(behind);yield return null;yield return null;
                Assert.That(OwnedMaterials(),Is.EqualTo(baseline));Assert.That(Object.FindFirstObjectByType<ProductionPurpleVisual>(),Is.Null);
            }
            ShaderCheck();Directory.CreateDirectory(Output);File.WriteAllText(Output+"/lifecycle.json","{\"casts\":3,\"first_hit\":true,\"behind_target_undamaged\":true,\"max_range\":true,\"travel_cancel\":true,\"terminal_legacy\":true,\"material_growth\":0,\"peak_particles\":"+peak+",\"maximum_residual_m\":"+maximumLag.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
            yield return new ExitPlayMode();
        }
        private static Texture2D Read(Camera camera,RenderTexture target)
        {
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});var old=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(960,540,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();RenderTexture.active=old;return image;
        }
        private static byte[] Still(Camera camera,RenderTexture rt,string path){var im=Read(camera,rt);byte[] pixels=im.GetRawTextureData<byte>().ToArray();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);return pixels;}
        [UnityTest]public IEnumerator ChargeLockAndTravelRender()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/CombatMVP.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;yield return null;Time.timeScale=0;
            var actor=GameObject.Find("GojoPlayer");actor.transform.SetPositionAndRotation(new Vector3(512,0,512),Quaternion.identity);
            var camera=Camera.main;camera.enabled=false;camera.transform.SetPositionAndRotation(actor.transform.position+new Vector3(0,4,-11),Quaternion.Euler(12,0,0));
            var side=new GameObject("TravelQAObserver").AddComponent<Camera>();side.CopyFrom(camera);side.enabled=false;side.fieldOfView=55;side.farClipPlane=180;
            var data=side.gameObject.AddComponent<UniversalAdditionalCameraData>();var original=camera.GetComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=original.renderPostProcessing;data.volumeLayerMask=original.volumeLayerMask;data.requiresColorTexture=true;data.requiresDepthTexture=true;
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="TravelQA_Ground";floor.transform.position=actor.transform.position+new Vector3(0,-.15f,35);floor.transform.localScale=new Vector3(100,.2f,150);floor.GetComponent<Collider>().enabled=false;
            var material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.SetColor("_BaseColor",new Color(.19f,.22f,.26f));floor.GetComponent<Renderer>().sharedMaterial=material;
            var grid=new GameObject("TravelQA_SpaceMarkers");var gridMat=new Material(material);gridMat.SetColor("_BaseColor",new Color(.08f,.1f,.14f));
            for(int z=-10;z<=80;z+=5){var bar=GameObject.CreatePrimitive(PrimitiveType.Cube);bar.transform.SetParent(grid.transform);bar.transform.position=actor.transform.position+new Vector3(0,-.035f,z);bar.transform.localScale=new Vector3(90,.01f,.06f);bar.GetComponent<Collider>().enabled=false;bar.GetComponent<Renderer>().sharedMaterial=gridMat;}
            var rt=new RenderTexture(960,540,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);rt.Create();camera.targetTexture=rt;side.targetTexture=rt;
            string folder=Output+"/Render/"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");Directory.CreateDirectory(folder);Debug.Log("Travel refinement render: "+folder);
            bool enabled=Profile.travelRefinementEnabled,async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;byte[] before=null;
            Vector3 origin=PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(actor.transform.position,Vector3.forward);
            try
            {
                foreach(bool refined in new[]{false,true})
                {
                    // The live scene controller can reposition the actor between capture passes.
                    // Re-establish the identical isolated stage before constructing each sequence.
                    actor.transform.SetPositionAndRotation(new Vector3(512,0,512),Quaternion.identity);
                    Profile.travelRefinementEnabled=refined;var host=new GameObject("TravelComparison");var sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,actor.transform,0,null,false);
                    sequence.Update(3.5f,0);side.transform.SetPositionAndRotation(origin+new Vector3(12,3,-11),Quaternion.LookRotation(new Vector3(-12,-3,11)));
                    byte[] pixels=Still(side,rt,folder+(refined?"/Charge_After.png":"/Charge_Before.png"));if(!refined)before=pixels;else Assert.That(pixels,Is.EqualTo(before),"Charge pixels must be exactly unchanged");
                    using(var output=new FileStream(folder+(refined?"/Side_After.rgb24":"/Side_Before.rgb24"),FileMode.CreateNew))
                    for(int frame=0;frame<=60;frame++)
                    {
                        float time=4.85f+frame/30f;sequence.Update(time,1/30f);float distance=Mathf.Clamp01((time-GojoPolishSettings.Current.PurpleReleaseTime)/1.6f)*48;
                        Vector3 focus=origin+Vector3.forward*distance;Vector3 offset=new Vector3(13,3,-10);side.transform.SetPositionAndRotation(focus+offset,Quaternion.LookRotation(-offset));
                        var image=Read(side,rt);byte[] bytes=image.GetRawTextureData<byte>().ToArray();output.Write(bytes,0,bytes.Length);Object.DestroyImmediate(image);
                        if(refined && (frame==13 || frame==27))Still(side,rt,folder+(frame==13?"/Release.png":"/Travel.png"));
                    }
                    sequence.Dispose();Object.Destroy(host);yield return null;yield return null;
                }
                actor.transform.SetPositionAndRotation(new Vector3(512,0,512),Quaternion.identity);
                camera.transform.SetPositionAndRotation(actor.transform.position+new Vector3(0,4,-11),Quaternion.Euler(12,0,0));
                Profile.travelRefinementEnabled=true;camera.enabled=true;var casterHost=new GameObject("TravelCasterMovie");var casterSequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(casterHost.transform,actor.transform,0,camera,true);
                using(var output=new FileStream(folder+"/Caster.rgb24",FileMode.CreateNew))
                for(int frame=0;frame<=120;frame++)
                {float time=2.85f+frame/30f;casterSequence.Update(time,1/30f);var im=Read(camera,rt);byte[] bytes=im.GetRawTextureData<byte>().ToArray();output.Write(bytes,0,bytes.Length);Object.DestroyImmediate(im);}
                casterSequence.Dispose();Object.Destroy(casterHost);ShaderCheck();
                File.WriteAllText(folder+"/format.json","{\"width\":960,\"height\":540,\"fps\":30,\"side_frames\":61,\"side_start\":4.85,\"caster_frames\":121,\"caster_start\":2.85,\"charge_pixel_difference\":0}");
            }
            finally{Profile.travelRefinementEnabled=enabled;ShaderUtil.allowAsyncCompilation=async;camera.targetTexture=null;side.targetTexture=null;rt.Release();Object.Destroy(rt);Object.Destroy(side.gameObject);Object.Destroy(floor);Object.Destroy(grid);Object.Destroy(material);Object.Destroy(gridMat);camera.enabled=true;}
            yield return null;yield return null;Assert.That(OwnedMaterials(),Is.Zero);yield return new ExitPlayMode();
        }
    }
}
