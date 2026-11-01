using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Unity.Collections;
using Object=UnityEngine.Object;
namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4HArtDirectedTests : ShibuyaPass4CFixture
    {
        ShibuyaBlackFlashArtPreview art;
        ShibuyaBlackFlashRndPreview legacy;
        ShibuyaBlackFlashArtProfile qa;
        Vector3 point;bool completed;string stage;
        [UnitySetUp] public IEnumerator Prepare()
        {
            completed=false;stage="prepare";Place(dummy,player.transform.position+Vector3.forward*1.4f);yield return null;
            point=dummy.transform.position+Vector3.up-Vector3.forward*dummy.GetComponent<CharacterController>().radius;
            qa=AssetDatabase.LoadAssetAtPath<ShibuyaBlackFlashArtProfile>(ShibuyaBlackFlashArtPreview.ProfilePath);
            Assert.That(qa,Is.Not.Null);Assert.That(qa.atlas,Is.Not.Null);
            Assert.That(ShaderUtil.ShaderHasError(AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shibuya/Editor/ShibuyaBlackFlashArt.shader")),Is.False);
        }
        [UnityTearDown] public IEnumerator Dispose(){art?.Dispose();art=null;legacy?.Dispose();legacy=null;yield return null;}
        [TearDown] public void Completion()=>Assert.That(completed,Is.True,"all final art QA assertions and evidence reached; last stage="+stage);
        void Create(){art=new ShibuyaBlackFlashArtPreview(point,Vector3.forward,qa);legacy=new ShibuyaBlackFlashRndPreview(ShibuyaBlackFlashRndKind.Legacy,point,Vector3.forward,71,4);foreach(var t in legacy.Root.GetComponentsInChildren<Transform>())t.gameObject.layer=31;}
        void View(int view,float elapsed=0){float angle=view==0?0:view==1?90:view==2?45:elapsed*90;PoseAngle(angle);}
        void PoseAngle(float angle){float a=angle*Mathf.Deg2Rad;camera.transform.position=point+new Vector3(Mathf.Sin(a)*6,2,-Mathf.Cos(a)*6);camera.transform.LookAt(point);}
        void Only(int mode,float age){art.Root.SetActive(false);legacy.Root.SetActive(false);if(mode==0)legacy.Sample(age,camera);else art.Sample(age,mode==1?ShibuyaBlackFlashArtView.BlackMass:mode==2?ShibuyaBlackFlashArtView.Combined:ShibuyaBlackFlashArtView.RedEdge);}
        Color32[] Pixels(RenderTexture rt){RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.StandardRequest{destination=rt});var previous=RenderTexture.active;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);try{RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();return tex.GetPixels32();}finally{RenderTexture.active=previous;Object.DestroyImmediate(tex);}}
        static void Save(string path,Color32[] pixels,int w,int h){Directory.CreateDirectory(Path.GetDirectoryName(path));var t=new Texture2D(w,h,TextureFormat.RGBA32,false);try{t.SetPixels32(pixels);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());}finally{Object.DestroyImmediate(t);}}
        [UnityTest] public IEnumerator AtlasKeyPosesAndLayerOwnership()
        {
            yield return null;stage="create";Create();int w=qa.atlas.width/2,h=qa.atlas.height/2;var shapes=new List<Color32[]>();var areas=new List<object>();
            for(int i=0;i<4;i++){
                stage="pose-"+i;int cell=qa.poseOrder[i];var pixels=qa.atlas.GetPixels(cell%2*w,(1-cell/2)*h,w,h).Select(c=>(Color32)c).ToArray();shapes.Add(pixels);
                int opaque=pixels.Count(c=>c.a>200),transparent=pixels.Count(c=>c.a<5);Assert.That(opaque,Is.GreaterThan(1000));Assert.That(transparent,Is.GreaterThan(pixels.Length/2));
                Assert.That(pixels.Where(c=>c.a>200).Count(c=>c.r<20&&c.g<20&&c.b<20),Is.GreaterThan(opaque*.9f));
                areas.Add(new{pose=i,opaque,transparent,total=pixels.Length});Save(Path.Combine(Root,"Art-keyposes","pose-"+i+"-RGBA.png"),pixels,w,h);
            }
            stage="distinct masks";for(int i=0;i<4;i++)for(int j=0;j<i;j++)Assert.That(shapes[i].SequenceEqual(shapes[j]),Is.False,"unique authored silhouettes");
            Assert.That(shapes[3].Count(c=>c.a>200),Is.LessThan(shapes[1].Count(c=>c.a>200)*.6));
            stage="shared geometry";var filters=art.Root.GetComponentsInChildren<MeshFilter>(true);Assert.That(filters[0].sharedMesh,Is.SameAs(filters[1].sharedMesh));Assert.That(filters[2].sharedMesh,Is.SameAs(filters[3].sharedMesh));
            Assert.That(art.Root.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("L2")).All(r=>r.sharedMaterial.GetFloat("_DstBlend")==10&&r.sharedMaterial.GetFloat("_ZWrite")==1),Is.True);
            Assert.That(art.Root.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("L3")).All(r=>r.sharedMaterial.GetFloat("_DstBlend")==1&&r.sharedMaterial.GetFloat("_ZWrite")==0),Is.True);
            stage="render poses";var rt=new RenderTexture(480,360,24,RenderTextureFormat.ARGB32);rt.Create();View(1);
            try{foreach(float age in new[]{.016f,.06f,.14f,.235f}){Only(2,age);Save(Path.Combine(Root,"Art-keyposes","combined-"+age.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+".png"),Pixels(rt),480,360);}}
            finally{rt.Release();Object.DestroyImmediate(rt);}
            stage="record poses";Record("Art-keypose-areas",new{areas,atlasWidth=qa.atlas.width,atlasHeight=qa.atlas.height,vertices=art.VertexCount,renderers=art.RendererCount,source="new generated RGBA asset; infographic never cropped into asset"});completed=true;yield return null;
        }
        [UnityTest] public IEnumerator AnglesVideosAndActualGpuVisibility()
        {
            Create();var production=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();float hp=dummy.Health.CurrentHealth,energy=input.EnergyCharge.Resource.CurrentEnergy;int accepted=production.AcceptedCount,observers=ShibuyaCombatDamageResults.ObserverCount;
            var rt=new RenderTexture(480,360,24,RenderTextureFormat.ARGB32);rt.Create();
            try{
                for(int view=0;view<4;view++)yield return Clip(rt,view);
                var oldMask=camera.cullingMask;var oldFlags=camera.clearFlags;var oldColor=camera.backgroundColor;var metrics=new List<object>();
                try{camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
                    foreach(float angle in new[]{0f,45,90,135,180,225,270,315})foreach(int mode in new[]{1,2,3}){
                        PoseAngle(angle);Only(mode,.06f);var pixels=Pixels(rt);int visible=pixels.Count(c=>c.a>12);Assert.That(visible,Is.GreaterThan(250),"no angle loses the whole effect");
                        metrics.Add(new{angle,mode,visible,criterion="isolated GPU alpha > 12/255; peak pose, no actor occlusion"});Save(Path.Combine(Root,"Art-angles",mode+"-"+angle+".png"),pixels,480,360);
                    }
                }finally{camera.cullingMask=oldMask;camera.clearFlags=oldFlags;camera.backgroundColor=oldColor;}
                View(1);foreach(int mode in new[]{0,1,2,3})foreach(float age in new[]{.016f,.033f,.06f,.1f,.14f,.19f,.235f,.29f,.32f}){Only(mode,age);Save(Path.Combine(Root,"Art-phases",mode+"-"+age.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+".png"),Pixels(rt),480,360);}
                Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(input.EnergyCharge.Resource.CurrentEnergy,Is.EqualTo(energy));Assert.That(production.AcceptedCount,Is.EqualTo(accepted));Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers));Assert.That(Time.timeScale,Is.EqualTo(1));
                Record("Art-angle-visibility",new{metrics,hp,energy,accepted,observers,point=new[]{point.x,point.y,point.z},duration=qa.duration});completed=true;
            }finally{rt.Release();Object.DestroyImmediate(rt);}
        }
        [UnityTest] public IEnumerator RepeatPreviewCleanupAndDefaultCombatIsolation()
        {
            var production=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();var profile=production.Profile;int accepted=production.AcceptedCount,observers=ShibuyaCombatDamageResults.ObserverCount;
            for(int j=0;j<12;j++){art=new ShibuyaBlackFlashArtPreview(point,Vector3.forward,qa);for(int k=0;k<45;k++)art.Sample(k/120f,(ShibuyaBlackFlashArtView)(k%3));Assert.That(art.Root.activeSelf,Is.False);art.Dispose();art=null;yield return null;}
            yield return Wait(.05f);
            Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name=="BlackFlash Art owned mesh"),Is.Zero);Assert.That(Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name=="BlackFlash Art owned material"),Is.Zero);Assert.That(Resources.FindObjectsOfTypeAll<ShibuyaBlackFlashArtProfile>().Count(m=>m.name=="BlackFlash Art owned profile"),Is.Zero);
            Assert.That(production.Profile,Is.SameAs(profile));Assert.That(production.AcceptedCount,Is.EqualTo(accepted));Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers));
            Record("Art-cleanup",new{cycles=12,ownedMeshes=0,ownedMaterials=0,ownedProfiles=0,accepted,observers});completed=true;
        }
        IEnumerator Clip(RenderTexture rt,int view)
        {
            // Four simultaneous readbacks otherwise allocate ~166MB of managed byte arrays
            // per second. Prepare native storage before the recording clock, encode later.
            float duration=view==3?4:1;int capacity=Mathf.CeilToInt(duration*75)+8;
            var buffers=new NativeArray<byte>[4][];
            for(int k=0;k<4;k++){buffers[k]=new NativeArray<byte>[capacity];for(int i=0;i<capacity;i++)buffers[k][i]=new NativeArray<byte>(480*360*4,Allocator.Persistent,NativeArrayOptions.UninitializedMemory);}
            var times=new List<double>(capacity);var frames=new List<int>(capacity);int pending=0,errors=0;
            try {
            View(view);for(int k=0;k<4;k++){Only(k,.15f);RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.StandardRequest{destination=rt});}
            art.Root.SetActive(false);legacy.Root.SetActive(false);
            System.GC.Collect();System.GC.WaitForPendingFinalizers();
            for(int n=0;n<3;n++){int f=Time.frameCount;while(Time.frameCount==f)yield return null;}
            double start=Time.unscaledTimeAsDouble;int last=-1;
            while(Time.unscaledTimeAsDouble-start<duration) {
                if(times.Count>=capacity)break;
                if(Time.frameCount==last){yield return null;continue;}last=Time.frameCount;double t=Time.unscaledTimeAsDouble-start;float age=(float)(t%1)-.1f;View(view,(float)t);
                times.Add(t);frames.Add(last);
                for(int k=0;k<4;k++) {
                    Only(k,age);RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.StandardRequest{destination=rt});int index=times.Count-1;pending++;
                    AsyncGPUReadback.RequestIntoNativeArray(ref buffers[k][index],rt,0,TextureFormat.RGBA32,r=>{if(r.hasError)errors++;pending--;});
                }
                yield return null;
            }
            yield return Until(()=>pending==0,10);Assert.That(errors,Is.Zero);Assert.That(frames.Distinct().Count(),Is.EqualTo(frames.Count));Assert.That(frames.Count,Is.GreaterThan(duration*20));
            Assert.That(frames.Count,Is.LessThan(capacity),"do not silently truncate a capture");
            double maxGap=times.Zip(times.Skip(1),(a,b)=>b-a).Max();string label=new[]{"front","side","diagonal","orbit"}[view];
            Record("Art-timing-"+label,new{times,frames,maxGap,seed=71,source="real unscaled runtime sample; same age/frame for all four candidates",prefix=.1,cycles=view==3?4:1,orbit=view==3?"360 degrees / four bursts":"fixed"});
            var tex=new Texture2D(480,360,TextureFormat.RGBA32,false);
            try {for(int k=0;k<4;k++) {
                string folder=Path.Combine(Root,"Art-video",label+"-"+k);Directory.CreateDirectory(folder);
                for(int i=0;i<times.Count;i++){tex.LoadRawTextureData(buffers[k][i]);tex.Apply();File.WriteAllBytes(Path.Combine(folder,"frame-"+i.ToString("D4")+".png"),tex.EncodeToPNG());}
            }}finally{Object.DestroyImmediate(tex);}
            Assert.That(maxGap,Is.LessThan(.055),"capture must not swallow the fast growth interval");
            }finally{for(int k=0;k<4;k++)foreach(var bytes in buffers[k])if(bytes.IsCreated)bytes.Dispose();}
        }
    }
}
