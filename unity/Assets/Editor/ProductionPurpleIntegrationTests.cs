using System;
using System.Collections;
using System.IO;
using System.Reflection;
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
    public sealed class ProductionPurpleIntegrationTests
    {
        private const string Output="D:/JJK_game/unity/Logs/PurpleProductionIntegrationQA";
        private static object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        private static int Materials(){int n=0;foreach(var m in Resources.FindObjectsOfTypeAll<Material>())if(m.name.StartsWith("PurpleProduction_"))n++;return n;}
        private static int Meshes(){int n=0;foreach(var m in Resources.FindObjectsOfTypeAll<Mesh>())if(m.name.StartsWith("PurpleProduction_"))n++;return n;}
        private static void Shaders()
        {
            foreach(string name in new[]{"HollowPurpleHybridD2R3","HollowPurpleHybridD2R3Arc","PurpleOuterR2Halo","PurpleOuterR2Particle","PurpleOuterR2Distortion"})
            {var shader=Resources.Load<Shader>("VFX/"+name);Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);foreach(var m in ShaderUtil.GetShaderMessages(shader))Assert.That(m.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),m.message);}
        }
        [UnityTest]public IEnumerator RepeatedLifecycleAndTravelSpace()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            var profile=ProductionPurpleVisualProfile.Current;Assert.That(profile.useIntegratedCandidate,Is.True);
            var editor=Editor.CreateEditor(profile);Assert.That(editor,Is.TypeOf<ProductionPurpleVisualProfileEditor>());Object.DestroyImmediate(editor);
            int initial=Materials(),meshes=Meshes(),peakParticles=0;float largestResidual=0;long allocated=0;
            for(int pass=0;pass<5;pass++)
            {
                var caster=new GameObject("IntegrationCaster",typeof(GojoTechniqueChainController));var host=new GameObject("IntegrationSequence");
                var sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,caster.transform,0,null,false);
                sequence.Update(3,.016f);
                var visual=host.GetComponentInChildren<ProductionPurpleVisual>();Assert.That(visual,Is.Not.Null);
                Assert.That(host.GetComponentsInChildren<ProductionPurpleBodyRuntime>(true).Length,Is.EqualTo(1));
                Assert.That(host.GetComponentsInChildren<ProductionPurpleOuterRuntime>(true).Length,Is.EqualTo(1));
                Assert.That(host.transform.Find("HollowPurpleCanonicalOrbSequence/HollowPurpleDenseBody/PurpleWarpedSpaceWake"),Is.Null,"No duplicate legacy distortion");
                int materialCount=Materials();Assert.That(materialCount-initial,Is.EqualTo(6));
                var outer=host.GetComponentInChildren<ProductionPurpleOuterRuntime>();var systems=outer.GetComponentsInChildren<ParticleSystem>();Assert.That(systems.Length,Is.EqualTo(2));
                var fixedParticles=new ParticleSystem.Particle[64];var movingParticles=new ParticleSystem.Particle[64];
                outer.Sample(2.933333f);int n=systems[0].GetParticles(fixedParticles);
                outer.Sample(2.933333f,Vector3.forward*30,.3f);Assert.That(systems[0].GetParticles(movingParticles),Is.EqualTo(n));
                bool moved=false;
                for(int j=0;j<n;j++){Vector3 delta=outer.transform.TransformVector(movingParticles[j].position-fixedParticles[j].position);if(delta.magnitude>.01f)moved=true;Assert.That(delta.z,Is.LessThanOrEqualTo(.001f));}
                Assert.That(moved,Is.True,"Travel particles must lag their charge-space positions");
                long before=GC.GetAllocatedBytesForCurrentThread();
                for(int frame=0;frame<48;frame++)
                {
                    sequence.Update(GojoPolishSettings.Current.PurpleReleaseTime+frame/60f,1/60f);
                    int particles=0;foreach(var ps in systems)particles+=ps.particleCount;
                    peakParticles=Mathf.Max(peakParticles,particles);largestResidual=Mathf.Max(largestResidual,visual.MaxResidualDistance);
                }
                allocated+=GC.GetAllocatedBytesForCurrentThread()-before;
                Assert.That(peakParticles,Is.LessThanOrEqualTo(128));
                Assert.That(Materials(),Is.EqualTo(materialCount),"No per-frame material creation");
                Assert.That(largestResidual,Is.LessThanOrEqualTo(GojoPolishSettings.PurpleShellDiameter*1.65f*1.16f*.5f*.55f+.01f));
                if(pass%2==0){caster.SetActive(false);Assert.That(sequence.Update(6,.016f),Is.False);}
                else{sequence.Update(GojoPolishSettings.Current.PurpleReleaseTime+1.61f,.016f);Assert.That(sequence.HasTerminated,Is.True);Assert.That(host.GetComponentInChildren<ProductionPurpleVisual>(),Is.Null);}
                sequence.Dispose();Object.Destroy(host);Object.Destroy(caster);yield return null;yield return null;
                Assert.That(Materials(),Is.EqualTo(initial));Assert.That(Meshes(),Is.EqualTo(meshes));Assert.That(Object.FindFirstObjectByType<ProductionPurpleVisual>(),Is.Null);
            }
            Shaders();Directory.CreateDirectory(Output);
            File.WriteAllText(Output+"/lifecycle.json","{\"repeat_casts\":5,\"peak_particles\":"+peakParticles+",\"max_residual_world\":"+largestResidual.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"sample_and_assertion_allocated_bytes\":"+allocated+",\"material_growth\":0,\"mesh_growth\":0}");
            yield return new ExitPlayMode();
        }
        [UnityTest]public IEnumerator ActualCombatNoHitMaxRange()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/CombatMVP.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;
            var actor=GameObject.Find("GojoPlayer");actor.transform.rotation=Quaternion.identity;
            foreach(var bot in Object.FindObjectsByType<JJKGame.Enemy.CurseBotController>(FindObjectsSortMode.None))bot.enabled=false;
            foreach(var health in Object.FindObjectsByType<JJKGame.Core.Health>(FindObjectsSortMode.None))if(health.gameObject!=actor)foreach(var c in health.GetComponentsInChildren<Collider>())c.enabled=false;
            var chain=actor.GetComponent<GojoTechniqueChainController>();Vector3 origin=PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(actor.transform.position,Vector3.forward);
            Physics.SyncTransforms();Call(chain,"ActivatePurple");
            double deadline=EditorApplication.timeSinceStartup+30;
            while(Time.unscaledTime<chain.PurplePresentationStartedAt+GojoPolishSettings.Current.PurpleReleaseTime+chain.PurpleLaunchDuration+.04f){Assert.That(EditorApplication.timeSinceStartup,Is.LessThan(deadline));yield return null;}
            var terminal=Object.FindFirstObjectByType<PurpleTerminalBurst>();Assert.That(terminal,Is.Not.Null);
            Assert.That(Vector3.Distance(terminal.transform.position,origin+Vector3.forward*chain.PurpleRange),Is.LessThan(.01f));
            Assert.That(Object.FindFirstObjectByType<ProductionPurpleVisual>(),Is.Null);
            float until=Time.unscaledTime+2.3f;while(Time.unscaledTime<until)yield return null;
            Assert.That(Object.FindFirstObjectByType<PurpleEnergyBody>(),Is.Null);Assert.That(Materials(),Is.Zero);
            yield return new ExitPlayMode();
        }
        private static Texture2D Read(Camera camera,RenderTexture target)
        {
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(960,540,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();RenderTexture.active=old;return image;
        }
        private static void Still(Camera camera,RenderTexture target,string path){var im=Read(camera,target);File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
        [UnityTest]public IEnumerator ProductionSequenceRenderReview()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/CombatMVP.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;Time.timeScale=0;
            foreach(var bot in Object.FindObjectsByType<JJKGame.Enemy.CurseBotController>(FindObjectsSortMode.None))bot.enabled=false;
            var actor=GameObject.Find("GojoPlayer");actor.transform.rotation=Quaternion.identity;
            var main=Camera.main;main.enabled=false;var initialPos=main.transform.position;var initialRot=main.transform.rotation;
            var observer=new GameObject("IntegrationObserver").AddComponent<Camera>();observer.CopyFrom(main);observer.enabled=false;observer.fieldOfView=55;
            var data=observer.gameObject.AddComponent<UniversalAdditionalCameraData>();var mainData=main.GetComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=mainData.renderPostProcessing;data.volumeLayerMask=mainData.volumeLayerMask;data.requiresColorTexture=true;data.requiresDepthTexture=true;
            var target=new RenderTexture(960,540,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);target.Create();main.targetTexture=target;observer.targetTexture=target;
            var profile=ProductionPurpleVisualProfile.Current;bool was=profile.useIntegratedCandidate;
            string folder=Output+"/Render/"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");Directory.CreateDirectory(folder);Debug.Log("Production integration render: "+folder);
            Vector3 origin=PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(actor.transform.position,Vector3.forward);
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                // Existing production and integrated charge, same scene/camera/time. No asset writes.
                foreach(bool integrated in new[]{false,true})
                {
                    profile.useIntegratedCandidate=integrated;var host=new GameObject("ChargeComparison");var sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,actor.transform,0,null,false);
                    sequence.Update(3.5f,0);observer.transform.SetPositionAndRotation(origin+new Vector3(9,3,-13),Quaternion.LookRotation(new Vector3(-9,-3,13)));
                    Still(observer,target,folder+(integrated?"/Integrated_Charge.png":"/Legacy_Charge.png"));sequence.Dispose();Object.Destroy(host);yield return null;yield return null;
                }
                profile.useIntegratedCandidate=true;
                foreach(string view in new[]{"Caster","Observer","Travel"})
                {
                    main.transform.SetPositionAndRotation(initialPos,initialRot);main.enabled=view=="Caster";
                    var host=new GameObject("IntegrationMovie_"+view);var sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,actor.transform,0,view=="Caster"?main:null,view=="Caster");
                    using(var output=new FileStream(folder+"/"+view+".rgb24",FileMode.CreateNew))
                    for(int frame=0;frame<=210;frame++)
                    {
                        float t=frame/30f;sequence.Update(t,1/30f);
                        float distance=Mathf.Clamp01((t-GojoPolishSettings.Current.PurpleReleaseTime)/1.6f)*48;
                        Vector3 focus=origin+Vector3.forward*(view=="Travel"?distance:20);
                        Vector3 offset=view=="Travel"?new Vector3(12,4,-13):new Vector3(24,10,-25);
                        observer.transform.SetPositionAndRotation(focus+offset,Quaternion.LookRotation(-offset));
                        var image=Read(view=="Caster"?main:observer,target);try{byte[] bytes=image.GetRawTextureData<byte>().ToArray();output.Write(bytes,0,bytes.Length);}finally{Object.DestroyImmediate(image);}
                    }
                    sequence.Dispose();Object.Destroy(host);yield return null;yield return null;
                }
                // Temporary bright/dark background props; no Scene, material asset or settings changes.
                main.enabled=false;var stillHost=new GameObject("BackgroundReadability");var stillSeq=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(stillHost.transform,actor.transform,0,null,false);stillSeq.Update(3.5f,0);
                observer.transform.SetPositionAndRotation(origin+new Vector3(9,3,-13),Quaternion.LookRotation(new Vector3(-9,-3,13)));
                var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=origin+Vector3.forward*9;wall.transform.localScale=new Vector3(70,35,.2f);wall.GetComponent<Collider>().enabled=false;
                var material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));wall.GetComponent<Renderer>().sharedMaterial=material;
                foreach(bool bright in new[]{true,false}){material.SetColor("_BaseColor",bright?new Color(.8f,.82f,.85f):new Color(.008f,.01f,.018f));Still(observer,target,folder+(bright?"/Bright_Charge.png":"/Dark_Charge.png"));}
                Object.Destroy(wall);Object.Destroy(material);stillSeq.Dispose();Object.Destroy(stillHost);
                Shaders();File.WriteAllText(folder+"/format.json","{\"width\":960,\"height\":540,\"fps\":30,\"frames\":211,\"format\":\"RGB24 bottom-up\",\"seconds\":7}");
            }
            finally{profile.useIntegratedCandidate=was;ShaderUtil.allowAsyncCompilation=async;main.targetTexture=null;observer.targetTexture=null;target.Release();Object.Destroy(target);Object.Destroy(observer.gameObject);main.enabled=true;}
            yield return null;yield return null;Assert.That(Materials(),Is.Zero);yield return new ExitPlayMode();
        }
    }
}
