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
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4HVisualRndDataTests
    {
        [Test] public void AtlasOriginalDeterministicBlackInteriorRedEdgeAndTemporalReplacement()
        {
            var a=ShibuyaBlackFlashRndPreview.BuildAtlas(71);var b=ShibuyaBlackFlashRndPreview.BuildAtlas(71);var c=ShibuyaBlackFlashRndPreview.BuildAtlas(72);
            try {
                var pixels=a.GetPixels32();Assert.That(pixels,Is.EqualTo(b.GetPixels32()));Assert.That(pixels.SequenceEqual(c.GetPixels32()),Is.False);
                int black=pixels.Count(x=>x.a>220&&x.r<20),red=pixels.Count(x=>x.a>100&&x.r>150);
                Assert.That(black,Is.GreaterThan(red),"black surfaces dominate red edges");
                Assert.That(red,Is.GreaterThan(100));Assert.That(pixels.Count(x=>x.a==0),Is.GreaterThan(pixels.Length/2));
                var frame2=new List<Color32>();var frame3=new List<Color32>();
                for(int y=0;y<192;y++)for(int x=0;x<192;x++){frame2.Add(pixels[y*768+384+x]);frame3.Add(pixels[y*768+576+x]);}
                Assert.That(frame2.SequenceEqual(frame3),Is.False,"contours actually replace between frames");
                Shader shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shibuya/Editor/ShibuyaBlackFlashRnd.shader");
                Assert.That(shader,Is.Not.Null);Assert.That(ShaderUtil.ShaderHasError(shader),Is.False);
            }finally{Object.DestroyImmediate(a);Object.DestroyImmediate(b);Object.DestroyImmediate(c);}
        }
    }
    public sealed class ShibuyaPass4HVisualRndTests:ShibuyaPass4CFixture
    {
        bool completed;
        ShibuyaBlackFlashRndPreview preview;
        [UnitySetUp] public IEnumerator Ready(){completed=false;Place(dummy,player.transform.position+Vector3.forward*1.4f);yield return null;}
        [UnityTearDown] public IEnumerator DisposePreview(){preview?.Dispose();preview=null;yield return null;}
        [TearDown] public void Completion()=>Assert.That(completed,Is.True,"reach final RND assertions");
        [UnityTest] public IEnumerator SameConditionsAllKindsAnglesAndOriginalSpeedEvidence()
        {
            Assert.That(SystemInfo.supportsAsyncGPUReadback,Is.True);
            float hp=dummy.Health.CurrentHealth,energy=input.EnergyCharge.Resource.CurrentEnergy;int zone=player.ZoneStacks;
            var production=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();int accepted=production.AcceptedCount,observers=ShibuyaCombatDamageResults.ObserverCount;
            Vector3 point=dummy.transform.position+Vector3.up;var metrics=new List<object>();
            for(int kind=0;kind<4;kind++) {
                preview=new ShibuyaBlackFlashRndPreview((ShibuyaBlackFlashRndKind)kind,point,Vector3.forward,71,4);
                string label=((ShibuyaBlackFlashRndKind)kind).ToString();
                if(preview.Atlas!=null)File.WriteAllBytes(Path.Combine(Root,"RND-"+label+"-atlas.png"),preview.Atlas.EncodeToPNG());
                for(int view=0;view<4;view++) {
                    string viewName=new[]{"front","side","diagonal","orbit"}[view];
                    SetView(point,view,0);float[] ages={.016f,.05f,.10f,.16f,.26f,.40f,.60f};
                    if(view<3)foreach(float age in ages){preview.Sample(age,camera);Capture("RND-stills/"+label+"-"+viewName+"-"+age.ToString("F3",System.Globalization.CultureInfo.InvariantCulture));}
                    yield return CaptureClip(point,label,view,metrics);
                }
                metrics.Add(new{kind=label,vertices=preview.VertexCount,renderers=preview.Root.GetComponentsInChildren<Renderer>(true).Length,diameter=4,seed=71});
                preview.Dispose();preview=null;yield return Wait(.05f);
            }
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(input.EnergyCharge.Resource.CurrentEnergy,Is.EqualTo(energy));Assert.That(player.ZoneStacks,Is.EqualTo(zone));
            Assert.That(production.AcceptedCount,Is.EqualTo(accepted));Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers));Assert.That(Time.timeScale,Is.EqualTo(1));
            Record("RND-capture-metrics",new{metrics,untouchedHp=hp,energy,zone,accepted,observers,alignment="first black experimental body at clip .100s (age .015, prefix .085); legacy prefix .100"});completed=true;
        }
        void SetView(Vector3 point,int view,float time)
        {
            float angle=view==0?0:view==1?90:view==2?45:time*180;
            float a=angle*Mathf.Deg2Rad;camera.transform.position=point+new Vector3(Mathf.Sin(a)*6,2,-Mathf.Cos(a)*6);camera.transform.LookAt(point);
        }
        IEnumerator CaptureClip(Vector3 point,string label,int view,List<object> metrics)
        {
            string name=label+"-"+new[]{"front","side","diagonal","orbit"}[view],folder=Path.Combine(Root,"RND-video/"+name);Directory.CreateDirectory(folder);
            var rt=new RenderTexture(480,360,24,RenderTextureFormat.ARGB32);rt.Create();
            var bytes=new List<byte[]>();var times=new List<double>();var frames=new List<int>();int pending=0,errors=0,last=-1;
            // Synchronous still/PNG work precedes this coroutine. Do not start its clock
            // in that stale runtime frame: otherwise the first gap can swallow the burst.
            preview.Sample(-1,camera);
            for(int warmup=0;warmup<2;warmup++){int f=Time.frameCount;while(Time.frameCount==f)yield return null;}
            double start=Time.unscaledTimeAsDouble;
            try {
                while(Time.unscaledTimeAsDouble-start<1) {
                    if(last==Time.frameCount){yield return null;continue;}last=Time.frameCount;
                    double t=Time.unscaledTimeAsDouble-start;SetView(point,view,(float)t);preview.Sample((float)t-(preview.Kind==ShibuyaBlackFlashRndKind.Legacy?.1f:.085f),camera);
                    RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.StandardRequest{destination=rt});
                    int index=bytes.Count;bytes.Add(null);times.Add(t);frames.Add(last);pending++;
                    AsyncGPUReadback.Request(rt,0,TextureFormat.RGBA32,r=>{if(r.hasError)errors++;else bytes[index]=r.GetData<byte>().ToArray();pending--;});yield return null;
                }
                yield return Until(()=>pending==0,5);Assert.That(errors,Is.Zero);Assert.That(frames.Distinct().Count(),Is.EqualTo(bytes.Count));Assert.That(bytes.Count,Is.GreaterThan(10));
                double maxGap=times.Zip(times.Skip(1),(a,b)=>b-a).Max();
                Record("RND-timing-"+name,new{times,frames,maxGap});
                Assert.That(maxGap,Is.LessThan(.055),"recording cannot omit a meaningful part of the .30s burst");
                var tex=new Texture2D(480,360,TextureFormat.RGBA32,false);
                try{for(int i=0;i<bytes.Count;i++){tex.LoadRawTextureData(bytes[i]);tex.Apply();File.WriteAllBytes(Path.Combine(folder,"frame-"+i.ToString("D4")+".png"),tex.EncodeToPNG());}}
                finally{Object.Destroy(tex);}
                metrics.Add(new{name,times,frames,source="actual unscaled runtime frames, raw GPU readback, encode afterward"});
            }finally{rt.Release();Object.Destroy(rt);}
        }
        [UnityTest] public IEnumerator RepeatedPreviewDisposalAndDefaultPresentationIsolation()
        {
            var production=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();var profile=production.Profile;
            int observers=ShibuyaCombatDamageResults.ObserverCount,accepted=production.AcceptedCount;
            foreach(ShibuyaBlackFlashRndKind kind in Enum.GetValues(typeof(ShibuyaBlackFlashRndKind)))for(int j=0;j<3;j++) {
                preview=new ShibuyaBlackFlashRndPreview(kind,dummy.transform.position+Vector3.up,Vector3.forward,71,4);
                for(int i=0;i<45;i++){preview.Sample(i/60f,camera);Assert.That(preview.Root.transform.position,Is.EqualTo(dummy.transform.position+Vector3.up));}
                Assert.That(preview.Root.activeSelf,Is.True);preview.Sample(.8f,camera);Assert.That(preview.Root.activeSelf,Is.False);
                foreach(var filter in preview.Root.GetComponentsInChildren<MeshFilter>(true))Assert.That(filter.sharedMesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)),Is.True);
                preview.Dispose();preview=null;yield return Wait(.04f);
            }
            yield return Wait(.15f);
            Assert.That(Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("BlackFlash RND owned")),Is.Zero);
            Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name=="BlackFlash RND mesh"),Is.Zero);
            Assert.That(Resources.FindObjectsOfTypeAll<Texture2D>().Count(m=>m.name=="BlackFlash RND atlas"),Is.Zero);
            Assert.That(production.Profile,Is.SameAs(profile));Assert.That(production.AcceptedCount,Is.EqualTo(accepted));Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers));
            Record("RND-cleanup",new{cycles=12,materials=0,meshes=0,textures=0,accepted,observers});completed=true;
        }
        [UnityTest] public IEnumerator HiddenReviewWindowDefersPlaybackUntilFreshFrame()
        {
            var window=ScriptableObject.CreateInstance<ShibuyaBlackFlashRndWindow>();
            try {
                window.BeginPreview(Object.FindFirstObjectByType<ShibuyaMatch>());
                Assert.That(window.WaitingForFirstFrame,Is.True);
                yield return Until(()=>!window.WaitingForFirstFrame);
                Assert.That(window.PreviewAge,Is.LessThan(.055));
                yield return Wait(.1f);Assert.That(window.PreviewAge,Is.GreaterThan(.05));
                Record("RND-window-start",new{window.PreviewAge,window.WaitingForFirstFrame,note="hidden window instance, no Show/GetWindow/focus calls"});completed=true;
            }finally{Object.DestroyImmediate(window);}
        }
    }
}
