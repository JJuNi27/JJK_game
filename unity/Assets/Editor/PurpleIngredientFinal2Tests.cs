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
    public sealed class PurpleIngredientFinal2Tests
    {
        private const string Output="D:/JJK_game/unity/Logs/PurpleIngredientFinal2QA";
        private static PurpleIngredientFinal2Profile Profile=>PurpleIngredientFinal2Profile.Current;
        private static object Call(object o,string method,params object[] args)=>o.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(o,args);
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;if(Profile!=null){Profile.candidateEnabled=false;}PurpleIngredientReboot2Profile.Current.candidateEnabled=false;PurpleIngredientFinalProfile.Current.candidateEnabled=false;if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        private static int OwnedMaterials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("PurpleProduction_") || m.name.StartsWith("PurplePolish_") || m.name.StartsWith("PurpleIngredient"));
        private static void ShaderCheck()
        {
            foreach(string s in new[]{"PurpleTravelWake","PurpleTravelHalo","PurpleTravelDistortion","PurplePolishHalo","HollowPurpleIngredientPolish","HollowPurpleIngredientMacro","HollowPurpleIngredientReboot","HollowPurpleIngredientPressure","HollowPurpleIngredientPressure2","PurpleIngredientCollision","HollowPurpleIngredientRupture","PurpleIngredientCollisionFinal","PurpleIngredientCollisionTear","PurpleIngredientBurstVolume","PurpleFusionBirth","HollowPurpleHybridD2R3","HollowPurpleHybridD2R3Arc","PurpleOuterR2Particle"})
            {var shader=Resources.Load<Shader>("VFX/"+s);Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);foreach(var m in ShaderUtil.GetShaderMessages(shader))Assert.That(m.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),m.message);}
        }
        [UnityTest]public IEnumerator TravelTargetsRangeAndRepeatedCleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
            Profile.candidateEnabled=true;int baseline=OwnedMaterials(),peak=0;float maximumLag=0;
            int meshBaseline=Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleProduction_") || m.name.StartsWith("PurpleIngredientBurst_"));
            for(int pass=0;pass<4;pass++)
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
                    if(pass==3 && Time.unscaledTime-start>1.97f){owner.SetActive(false);Assert.That(sequence.Update(Time.unscaledTime,0),Is.False);break;}
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
                Assert.That(sawTravel,Is.EqualTo(pass!=3));
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
                Assert.That(Object.FindFirstObjectByType<PurpleFusionBirthAccent>(),Is.Null);
                Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleProduction_") || m.name.StartsWith("PurpleIngredientBurst_")),Is.EqualTo(meshBaseline));
            }
            ShaderCheck();Directory.CreateDirectory(Output);File.WriteAllText(Output+"/lifecycle.json","{\"casts\":4,\"first_hit\":true,\"behind_target_undamaged\":true,\"max_range\":true,\"travel_cancel\":true,\"birth_cancel\":true,\"terminal_legacy\":true,\"material_growth\":0,\"mesh_growth\":0,\"peak_particles\":"+peak+",\"maximum_residual_m\":"+maximumLag.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
            yield return new ExitPlayMode();
        }
        [UnityTest]public IEnumerator CollisionContactWindowAndCleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
            Profile.candidateEnabled=true;int baseline=OwnedMaterials();float contactTime=0;
            for(int repeat=0;repeat<3;repeat++)
            {
                var actor=new GameObject("ContactCaster");var host=new GameObject("ContactSequence");
                var seq=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,actor.transform,0,null,false);
                var accent=host.GetComponentInChildren<PurpleIngredientCollisionAccent>(true);Assert.That(accent,Is.Not.Null);contactTime=accent.ContactTime;
                seq.Update(contactTime,0);
                var blue=host.transform.Find("HollowPurpleCanonicalOrbSequence/HollowPurpleBlueOrbRoot");
                var red=host.transform.Find("HollowPurpleCanonicalOrbSequence/HollowPurpleRedOrbRoot");
                Assert.That(Vector3.Distance(blue.position,red.position),Is.EqualTo((blue.lossyScale.x+red.lossyScale.x)*1.025f).Within(.0001));
                int visible=0;
                for(int frame=0;frame<180;frame++){seq.Update(frame/60f,1/60f);if(accent.IsVisible)visible++;}
                Assert.That(visible,Is.EqualTo(PurpleIngredientReboot2Profile.Current.collisionFrames));
                Assert.That(contactTime+PurpleIngredientReboot2Profile.Current.collisionFrames/60f,Is.LessThan(GojoPolishSettings.Current.PurpleHoldStart-.055f),"Contact frame must finish before existing Birth flash");
                seq.Update(contactTime+.01f,0);seq.Dispose();Object.Destroy(host);Object.Destroy(actor);yield return null;yield return null;
                Assert.That(OwnedMaterials(),Is.EqualTo(baseline));Assert.That(Object.FindFirstObjectByType<PurpleIngredientCollisionAccent>(),Is.Null);
            }
            ShaderCheck();Directory.CreateDirectory(Output);File.WriteAllText(Output+"/collision.json","{\"contact_seconds\":"+contactTime.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"nominal_60hz_frames\":2,\"repeated_contact_cancel\":3,\"material_growth\":0,\"birth_overlap\":false}");
            yield return new ExitPlayMode();
        }
        private static Texture2D Read(Camera camera,RenderTexture target)
        {
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});var old=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(960,540,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();RenderTexture.active=old;return image;
        }
        private static byte[] Still(Camera camera,RenderTexture rt,string path){var im=Read(camera,rt);byte[] pixels=im.GetRawTextureData<byte>().ToArray();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);return pixels;}
        [UnityTest]public IEnumerator FormationComparison()=>RenderComparison(true);
        [UnityTest]public IEnumerator BurstVisualPreview()=>RenderComparison(false);
        [UnityTest]public IEnumerator IngredientDirectionsAndRepeatedCleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
            int materials=OwnedMaterials();Profile.candidateEnabled=true;int meshCount=Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleIngredientBurst_"));
            for(int repeat=0;repeat<3;repeat++)
            {
                foreach(bool red in new[]{false,true})
                {
                    var host=new GameObject("IngredientDirection");var ingredient=host.AddComponent<PurpleFusionIngredient>();ingredient.Configure(red);
                    var burst=host.GetComponent<PurpleIngredientBurstVolume>();Assert.That(burst,Is.Not.Null);
                    var filters=host.GetComponentsInChildren<MeshFilter>().Where(f=>f.sharedMesh.name.StartsWith("PurpleIngredientBurst_")).ToArray();Assert.That(filters.Length,Is.EqualTo(3));
                    ingredient.Render(.05f,0,Vector3.right*3);Vector3 before=Vector3.zero;foreach(var v in filters[0].sharedMesh.vertices.Take(8))before+=v/8;
                    ingredient.Render(.10f,0,Vector3.right*3);Vector3 after=Vector3.zero;foreach(var v in filters[0].sharedMesh.vertices.Take(8))after+=v/8;
                    Assert.That(red?after.magnitude>before.magnitude:after.magnitude<before.magnitude,Is.True,"Volume burst must move with named polarity");
                    Assert.That(filters[0].sharedMesh.colors.Average(c=>c.a),Is.GreaterThan(.8f),"Mid-event bulk must not be nearly transparent");
                    Assert.That(filters[0].sharedMesh.bounds.size.z,Is.GreaterThan(.10f),"Burst must have physical depth");
                    Assert.That(host.GetComponentsInChildren<LineRenderer>().Length,Is.EqualTo(31));
                    Object.Destroy(host);yield return null;yield return null;
                    Assert.That(OwnedMaterials(),Is.EqualTo(materials));Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleIngredientBurst_")),Is.EqualTo(meshCount));
                }
            }
            ShaderCheck();Directory.CreateDirectory(Output);File.WriteAllText(Output+"/ingredient_lifecycle.json","{\"spawns\":6,\"direction_checks\":6,\"volume_meshes_per_ingredient\":3,\"mesh_growth\":0,\"material_growth\":0,\"line_renderers_per_ingredient\":31}");
            yield return new ExitPlayMode();
        }
        [UnityTest]public IEnumerator FusionTrajectoryAndBodyLock()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
            Assert.That(Profile.candidateEnabled,Is.False,"Saved candidate must default off");
            string production=JsonUtility.ToJson(ProductionPurpleVisualProfile.Current),timing=JsonUtility.ToJson(GojoPolishSettings.Current);
            var editor=Editor.CreateEditor(Profile);Assert.That(editor,Is.TypeOf<PurpleIngredientFinal2ProfileEditor>());Object.DestroyImmediate(editor);
            var poses=new System.Collections.Generic.List<Vector3>();
            for(int variant=0;variant<2;variant++)
            {
                Profile.candidateEnabled=variant==1;
                var actor=new GameObject("LockCaster");var host=new GameObject("LockComparison");
                var seq=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,actor.transform,0,null,false);int index=0;float lastMergeSample=-1;
                foreach(float time in new[]{.1f,.5f,1.3f,1.7f,1.93f,2.03f,3.5f,5.16f,5.6f})
                {
                    seq.Update(time,1/30f);
                    if(time>GojoPolishSettings.Current.PurpleFusionStart && time<GojoPolishSettings.Current.PurpleHoldStart)lastMergeSample=time;
                    foreach(string name in new[]{"HollowPurpleBlueOrbRoot","HollowPurpleRedOrbRoot","HollowPurpleDenseBody"})
                    {
                        var tr=host.transform.Find("HollowPurpleCanonicalOrbSequence/"+name);
                        foreach(Vector3 value in new[]{tr.position,tr.lossyScale,tr.forward})
                        {
                            if(variant==0)poses.Add(value);
                            else
                            {
                                Vector3 expected=poses[index];
                                Assert.That(Vector3.Distance(value,expected),Is.LessThan(.00001f),name+" at "+time+" sample "+index);
                            }
                            index++;
                        }
                    }
                }
                Assert.That(host.GetComponentsInChildren<ProductionPurpleBodyRuntime>(true).Length,Is.EqualTo(1));
                seq.Dispose();Object.Destroy(host);Object.Destroy(actor);yield return null;yield return null;Assert.That(OwnedMaterials(),Is.Zero);
            }
            Assert.That(JsonUtility.ToJson(ProductionPurpleVisualProfile.Current),Is.EqualTo(production));Assert.That(JsonUtility.ToJson(GojoPolishSettings.Current),Is.EqualTo(timing));
            Directory.CreateDirectory(Output);File.WriteAllText(Output+"/locks.json","{\"canonical_pose_samples\":81,\"unexpected_pose_differences\":0,\"all_poses_identical\":true,\"production_profile_unchanged\":true,\"timing_profile_unchanged\":true,\"saved_candidates_off\":true}");
            yield return new ExitPlayMode();
        }
        private IEnumerator RenderComparison(bool full)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();PurpleIngredientReboot2Profile.Current.candidateEnabled=true;PurpleIngredientFinalProfile.Current.candidateEnabled=true;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/CombatMVP.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;yield return null;Time.timeScale=0;
            var actor=GameObject.Find("GojoPlayer");var stage=new Vector3(512,0,512);actor.transform.SetPositionAndRotation(stage,Quaternion.identity);
            var camera=Camera.main;camera.enabled=false;
            var side=new GameObject("PolishQAObserver").AddComponent<Camera>();side.CopyFrom(camera);side.enabled=false;side.fieldOfView=45;side.farClipPlane=180;
            var data=side.gameObject.AddComponent<UniversalAdditionalCameraData>();var original=camera.GetComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=original.renderPostProcessing;data.volumeLayerMask=original.volumeLayerMask;data.requiresColorTexture=true;data.requiresDepthTexture=true;
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=stage+new Vector3(0,-.15f,35);floor.transform.localScale=new Vector3(100,.2f,150);floor.GetComponent<Collider>().enabled=false;
            var material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.SetColor("_BaseColor",new Color(.19f,.22f,.26f));floor.GetComponent<Renderer>().sharedMaterial=material;
            var grid=new GameObject("PolishQA_SpaceMarkers");var gridMat=new Material(material);gridMat.SetColor("_BaseColor",new Color(.08f,.1f,.14f));
            for(int z=-10;z<=80;z+=5){var bar=GameObject.CreatePrimitive(PrimitiveType.Cube);bar.transform.SetParent(grid.transform);bar.transform.position=stage+new Vector3(0,-.035f,z);bar.transform.localScale=new Vector3(90,.01f,.06f);bar.GetComponent<Collider>().enabled=false;bar.GetComponent<Renderer>().sharedMaterial=gridMat;}
            var rt=new RenderTexture(960,540,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);rt.Create();camera.targetTexture=rt;side.targetTexture=rt;
            string folder=Output+"/"+(full?"Final":"OuterPreview")+"/"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");Directory.CreateDirectory(folder);Debug.Log("Polish comparison render: "+folder);
            var established=PurpleFinalPolishProfile.Current;bool previousOuter=established.outerPolishEnabled,previousBirth=established.fusionBirthEnabled;established.outerPolishEnabled=true;established.fusionBirthEnabled=true;bool previousReboot=PurpleIngredientRebootProfile.Current.candidateEnabled;PurpleIngredientRebootProfile.Current.candidateEnabled=true;bool previousMacro=PurpleIngredientMacroProfile.Current.candidateEnabled;PurpleIngredientMacroProfile.Current.candidateEnabled=true;bool previousIngredient=PurpleIngredientPolishProfile.Current.candidateEnabled;PurpleIngredientPolishProfile.Current.candidateEnabled=true;bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            Vector3 origin=PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(stage,Vector3.forward);
            try
            {
                for(int variant=0;variant<2;variant++)
                foreach(bool caster in full?new[]{false,true}:new[]{false})
                {
                    actor.transform.SetPositionAndRotation(stage,Quaternion.identity);
                    camera.transform.SetPositionAndRotation(stage+new Vector3(0,4,-11),Quaternion.Euler(12,0,0));camera.fieldOfView=60;
                    Profile.candidateEnabled=variant>0;
                    var host=new GameObject("PolishComparison");var seq=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,actor.transform,0,caster?camera:null,caster);
                    string name=(caster?"Caster_":"Side_")+(variant==0?"Baseline":"Candidate");
                    try
                    {
                        using(var output=new FileStream(folder+"/"+name+".rgb24",FileMode.CreateNew))
                        for(int frame=0;frame<(full?175:1);frame++)
                        {
                            float time=full?frame/30f:.833333f;seq.Update(time,1/30f);
                            if(variant==1 && (!full || frame==25))
                            {
                                File.WriteAllLines(folder+"/burst-materials.txt",host.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("IngredientBulkBurst_")).Select(r=>{
                                    var m=r.sharedMaterial;var mesh=r.GetComponent<MeshFilter>().sharedMesh;var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);
                                    return r.transform.parent.name+"/"+r.name+" enabled="+r.enabled+" shader="+m.shader.name+" colour="+m.GetColor("_Color")+" blockEmpty="+block.isEmpty+" alpha="+mesh.colors.Select(c=>c.a).DefaultIfEmpty(0).Average()+" normals="+mesh.normals.Length+" queue="+m.renderQueue;
                                }));
                            }
                            float distance=Mathf.Clamp01((time-GojoPolishSettings.Current.PurpleReleaseTime)/1.6f)*48;
                            Vector3 offset=new Vector3(5,2,-12);side.transform.SetPositionAndRotation(origin+Vector3.forward*distance+offset,Quaternion.LookRotation(-offset));
                            var im=Read(caster?camera:side,rt);byte[] bytes=im.GetRawTextureData<byte>().ToArray();output.Write(bytes,0,bytes.Length);
                            if(!full || frame==10 || frame==25 || frame==32 || frame==43 || frame==49 || frame==60 || frame==105 || frame==160)File.WriteAllBytes(folder+"/"+name+"_"+frame.ToString("D3")+".png",im.EncodeToPNG());
                            Object.DestroyImmediate(im);
                        }
                    }
                    finally{seq.Dispose();Object.Destroy(host);}
                    yield return null;yield return null;
                    if(!caster && full)
                    {
                        actor.transform.SetPositionAndRotation(stage,Quaternion.identity);
                        var detailHost=new GameObject("FusionContactDetail");
                        var detail=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(detailHost.transform,actor.transform,0,null,false);
                        try
                        {
                            using(var detailOutput=new FileStream(folder+"/Fusion_"+(variant==0?"Baseline":"Candidate")+".rgb24",FileMode.CreateNew))
                            for(int frame=0;frame<49;frame++)
                            {
                                detail.Update(1.3f+frame/60f,1/60f);
                                Vector3 offset=new Vector3(5,2,-12);side.transform.SetPositionAndRotation(origin+offset,Quaternion.LookRotation(-offset));
                                var im=Read(side,rt);byte[] bytes=im.GetRawTextureData<byte>().ToArray();detailOutput.Write(bytes,0,bytes.Length);
                                if(frame<16)File.WriteAllBytes(folder+"/Fusion_"+(variant==0?"Baseline":"Candidate")+"_"+frame.ToString("D3")+".png",im.EncodeToPNG());
                                Object.DestroyImmediate(im);
                            }
                        }
                        finally{detail.Dispose();Object.Destroy(detailHost);}
                        yield return null;yield return null;
                    }
                    Assert.That(OwnedMaterials(),Is.Zero,"All candidate and production materials must be released");
                    if(caster){Assert.That(camera.fieldOfView,Is.EqualTo(60).Within(.001));Assert.That(Vector3.Distance(camera.transform.position,stage+new Vector3(0,4,-11)),Is.LessThan(.001));}
                }
                ShaderCheck();File.WriteAllText(folder+"/format.json","{\"width\":960,\"height\":540,\"fps\":30,\"start\":0,\"frames\":"+(full?175:1)+",\"camera_restore\":true,\"material_growth\":0}");
            }
            finally{PurpleIngredientRebootProfile.Current.candidateEnabled=previousReboot;PurpleIngredientMacroProfile.Current.candidateEnabled=previousMacro;PurpleIngredientPolishProfile.Current.candidateEnabled=previousIngredient;established.outerPolishEnabled=previousOuter;established.fusionBirthEnabled=previousBirth;Profile.candidateEnabled=false;ShaderUtil.allowAsyncCompilation=async;camera.targetTexture=null;side.targetTexture=null;rt.Release();Object.Destroy(rt);Object.Destroy(side.gameObject);Object.Destroy(floor);Object.Destroy(grid);Object.Destroy(material);Object.Destroy(gridMat);camera.enabled=true;}
            yield return new ExitPlayMode();
        }
    }
}
