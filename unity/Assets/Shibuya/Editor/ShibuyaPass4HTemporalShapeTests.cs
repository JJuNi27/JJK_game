using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public sealed class ShibuyaPass4HTemporalShapeTests : ShibuyaPass4CFixture
    {
        readonly List<ShibuyaBlackFlashTemporalShapePreview> previews=new List<ShibuyaBlackFlashTemporalShapePreview>();
        ShibuyaBlackFlashTemporalShapeProfile qa;
        Vector3 point;
        bool completed;
        [UnitySetUp] public IEnumerator Prepare()
        {
            completed=false;Place(dummy,player.transform.position+Vector3.forward*1.4f);yield return null;
            point=dummy.transform.position+Vector3.up;
            qa=AssetDatabase.LoadAssetAtPath<ShibuyaBlackFlashTemporalShapeProfile>(ShibuyaBlackFlashTemporalShapePreview.SettingsPath);
            Assert.That(qa,Is.Not.Null);Assert.That(ShaderUtil.ShaderHasError(AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShape.shader")),Is.False);
        }
        [UnityTearDown] public IEnumerator Dispose(){foreach(var p in previews)p.Dispose();previews.Clear();yield return null;}
        [TearDown] public void Completion()=>Assert.That(completed,Is.True,"reach final temporal/shape assertions and evidence records");
        void CreateAll(){for(int k=0;k<4;k++)previews.Add(new ShibuyaBlackFlashTemporalShapePreview((ShibuyaTemporalShapeKind)k,point,Vector3.forward,71,qa));}
        void View(int view,float elapsed=0)
        {
            float angle=view==0?0:view==1?90:view==2?45:elapsed*90;
            float a=angle*Mathf.Deg2Rad;camera.transform.position=point+new Vector3(Mathf.Sin(a)*6,2,-Mathf.Cos(a)*6);camera.transform.LookAt(point);
        }
        void Only(int index,float age,ShibuyaTemporalShapeView view=ShibuyaTemporalShapeView.Composite)
        {
            foreach(var p in previews)p.Root.SetActive(false);previews[index].Sample(age,view);
        }
        Color32[] Pixels(RenderTexture rt)
        {
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.StandardRequest{destination=rt});
            var old=RenderTexture.active;var t=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);
            try{RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);t.Apply();return t.GetPixels32();}
            finally{RenderTexture.active=old;Object.DestroyImmediate(t);}
        }
        static void Save(string path,Color32[] pixels,int width,int height)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));var tex=new Texture2D(width,height,TextureFormat.RGBA32,false);
            try{tex.SetPixels32(pixels);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}finally{Object.DestroyImmediate(tex);}
        }
        [UnityTest] public IEnumerator FactorialControlsAndProductionPixelParity()
        {
            CreateAll();View(1);var original=Resources.Load<ShibuyaBlackFlashProfile>("Shibuya Black Flash");
            var meshes=previews.Select(p=>p.Renderers.Select(r=>r.GetComponent<MeshFilter>().sharedMesh).ToArray()).ToArray();
            Assert.That(meshes[0][2].vertices,Is.EqualTo(meshes[1][2].vertices));
            Assert.That(meshes[2][2].vertices,Is.EqualTo(meshes[3][2].vertices));
            Assert.That(meshes[0][2].vertices.SequenceEqual(meshes[2][2].vertices),Is.False);
            foreach(var p in previews){Assert.That(p.Strokes,Is.EqualTo(33));Assert.That(p.Visual.VertexCount,Is.EqualTo(previews[0].Visual.VertexCount));Assert.That(p.BirthGrowth,Is.EqualTo(original.ruptureGrowthTime));}
            for(int i=1;i<4;i++)for(int layer=0;layer<5;layer++) {
                Assert.That(meshes[i][layer].triangles,Is.EqualTo(meshes[0][layer].triangles));
                var a=new List<Vector4>();var b=new List<Vector4>();meshes[0][layer].GetUVs(0,a);meshes[i][layer].GetUVs(0,b);Assert.That(a,Is.EqualTo(b),"same timing/seed streams");
                if(layer!=2)Assert.That(meshes[i][layer].vertices,Is.EqualTo(meshes[0][layer].vertices));
                Assert.That(meshes[i][layer].vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)),Is.True);
            }
            float radius=meshes[0][2].vertices.Max(v=>v.magnitude);
            Assert.That(meshes[2][2].vertices.Max(v=>v.magnitude),Is.EqualTo(radius).Within(.0001f));
            Assert.That(previews[0].Profile.branchDuration,Is.EqualTo(previews[1].Profile.branchDuration));
            Assert.That(previews[0].Profile.ruptureGrowthTime,Is.EqualTo(.095f));Assert.That(previews[1].Profile.ruptureGrowthTime,Is.EqualTo(.03f));
            var rt=new RenderTexture(480,360,24,RenderTextureFormat.ARGB32);rt.Create();var parity=new List<object>();
            try {
                foreach(float age in new[]{.0167f,.05f,.1f,.15f,.2f,.3f}) {
                    Only(0,age);var before=Pixels(rt);var ink=previews[0].Renderers[1].sharedMaterial;var smoke=previews[0].Renderers[4].sharedMaterial;var shader=ink.shader;
                    Color32[] after;
                    try{ink.shader=original.inkShader;smoke.shader=original.inkShader;Only(0,age);after=Pixels(rt);}
                    finally{ink.shader=shader;smoke.shader=shader;}
                    int max=before.Zip(after,(a,b)=>Math.Max(Math.Max(Math.Abs(a.r-b.r),Math.Abs(a.g-b.g)),Math.Max(Math.Abs(a.b-b.b),Math.Abs(a.a-b.a)))).Max();
                    parity.Add(new{age,maxChannelDifference=max});Assert.That(max,Is.LessThanOrEqualTo(1),"QA BASE must match production shader");
                }
                Record("TS-factorial-controls",new{parity,strokeCount=33,baseRadius=radius,shapeRadius=meshes[2][2].vertices.Max(v=>v.magnitude),profiles=previews.Select(p=>p.Metrics(.1f)).ToArray()});
            }finally{rt.Release();Object.DestroyImmediate(rt);}
            completed=true;yield return null;
        }
        [UnityTest] public IEnumerator AllCandidatesAnglesOriginalSpeedAndLayerTelemetry()
        {
            CreateAll();Assert.That(SystemInfo.supportsAsyncGPUReadback,Is.True);
            var production=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();
            float hp=dummy.Health.CurrentHealth,energy=input.EnergyCharge.Resource.CurrentEnergy;int accepted=production.AcceptedCount,observers=ShibuyaCombatDamageResults.ObserverCount;
            var rt=new RenderTexture(480,360,24,RenderTextureFormat.ARGB32);rt.Create();
            try {
                string reuse=SessionState.GetString("BlackFlashTS.ReuseCapture","");
                if(string.IsNullOrEmpty(reuse))for(int view=0;view<4;view++)yield return Clip(rt,view);
                else {
                    var reused=new List<object>();
                    foreach(string view in new[]{"front","side","diagonal","orbit"}) {
                        string path=Path.Combine(reuse,"TS-timing-"+view+".json");
                        var data=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path));
                        int count=data["times"].Count();double maxGap=(double)data["maxGap"];
                        Assert.That(maxGap,Is.LessThan(.055));Assert.That(count,Is.GreaterThan(view=="orbit"?80:20));
                        Assert.That(data["frames"].Select(v=>(int)v).Distinct().Count(),Is.EqualTo(count));
                        for(int k=0;k<4;k++)Assert.That(Directory.GetFiles(Path.Combine(reuse,"TS-video",view+"-"+k),"frame-*.png").Length,Is.EqualTo(count));
                        reused.Add(new{view,count,maxGap,source=path});
                    }
                    Record("TS-reused-capture",new{reused,reason="all four native capture loops passed their assertions before subsequent Vector3 JSON serialization failed; visual code unchanged"});
                }
                View(1);var stages=new List<object>();
                for(int k=0;k<4;k++)foreach(float age in new[]{0,.0167f,.0333f,.05f,.0833f,.1f,.15f,.2f,.3f}) {
                    Only(k,age);stages.Add(previews[k].Metrics(age));Save(Path.Combine(Root,"TS-stills",k+"-"+age.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+".png"),Pixels(rt),480,360);
                }
                Record("TS-uniform-timeline-calculated",stages);
                var oldMask=camera.cullingMask;var oldFlags=camera.clearFlags;var oldColor=camera.backgroundColor;
                var gpu=new List<object>();
                try {
                    camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
                    for(int k=0;k<4;k++)foreach(float age in new[]{0,.0167f,.0333f,.05f,.0833f,.1f,.15f,.2f,.3f}) {
                        Only(k,age,ShibuyaTemporalShapeView.Rupture);var pixels=Pixels(rt);int count=0,minX=480,minY=360,maxX=-1,maxY=-1;
                        for(int n=0;n<pixels.Length;n++)if(pixels[n].a>12){count++;minX=Math.Min(minX,n%480);maxX=Math.Max(maxX,n%480);minY=Math.Min(minY,n/480);maxY=Math.Max(maxY,n/480);}
                        gpu.Add(new{candidate=k,age,pixels=count,width=maxX<0?0:maxX-minX+1,height=maxY<0?0:maxY-minY+1,criterion="isolated rupture alpha>12/255, side camera; no actor/background occlusion"});
                        Save(Path.Combine(Root,"TS-gpu-masks",k+"-"+age.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+".png"),pixels,480,360);
                    }
                }finally{camera.cullingMask=oldMask;camera.clearFlags=oldFlags;camera.backgroundColor=oldColor;}
                Record("TS-GPU-observed-masks",gpu);
                for(int k=0;k<4;k++)foreach(ShibuyaTemporalShapeView layer in Enum.GetValues(typeof(ShibuyaTemporalShapeView))) {
                    Only(k,.15f,layer);Save(Path.Combine(Root,"TS-layers",k+"-"+layer+".png"),Pixels(rt),480,360);
                }
                Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(input.EnergyCharge.Resource.CurrentEnergy,Is.EqualTo(energy));Assert.That(production.AcceptedCount,Is.EqualTo(accepted));Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers));Assert.That(Time.timeScale,Is.EqualTo(1));
                Record("TS-isolation",new{hp,energy,accepted,observers,seed=71,totalLife=previews[0].Profile.Lifetime});
                yield return FrameDebuggerProbe();
                completed=true;
            }finally{rt.Release();Object.DestroyImmediate(rt);}
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
            foreach(var p in previews)p.Root.SetActive(false);
            for(int n=0;n<2;n++){int f=Time.frameCount;while(Time.frameCount==f)yield return null;}
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
            Record("TS-timing-"+label,new{times,frames,maxGap,seed=71,source="real unscaled runtime sample; same age/frame for all four candidates",prefix=.1,cycles=view==3?4:1,orbit=view==3?"360 degrees / four bursts":"fixed"});
            var tex=new Texture2D(480,360,TextureFormat.RGBA32,false);
            try {for(int k=0;k<4;k++) {
                string folder=Path.Combine(Root,"TS-video",label+"-"+k);Directory.CreateDirectory(folder);
                for(int i=0;i<times.Count;i++){tex.LoadRawTextureData(buffers[k][i]);tex.Apply();File.WriteAllBytes(Path.Combine(folder,"frame-"+i.ToString("D4")+".png"),tex.EncodeToPNG());}
            }}finally{Object.DestroyImmediate(tex);}
            Assert.That(maxGap,Is.LessThan(.055),"capture must not swallow the fast growth interval");
            }finally{for(int k=0;k<4;k++)foreach(var bytes in buffers[k])if(bytes.IsCreated)bytes.Dispose();}
        }
        IEnumerator FrameDebuggerProbe()
        {
            Only(0,.15f);View(1);
            var type=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditorInternal.FrameDebuggerInternal.FrameDebuggerUtility");
            if(type==null){Record("TS-frame-debugger",new{attempted=false,reason="internal utility unavailable; use layer captures"});yield break;}
            const BindingFlags flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;
            bool prior=UnityEngine.FrameDebugger.enabled;
            if(prior){Record("TS-frame-debugger",new{attempted=false,reason="user already enabled debugger; state preserved"});yield break;}
            var enable=type.GetMethod("SetEnabled",flags);bool supported=(bool)type.GetProperty("locallySupported",flags).GetValue(null);
            if(!supported){Record("TS-frame-debugger",new{attempted=false,supported,reason="backend reports unsupported"});yield break;}
            double end=EditorApplication.timeSinceStartup+1;EditorApplication.CallbackFunction safety=null;
            safety=()=>{if(EditorApplication.timeSinceStartup>end){enable.Invoke(null,new object[]{false,0});EditorApplication.update-=safety;}};
            EditorApplication.update+=safety;
            try {
                enable.Invoke(null,new object[]{true,0});
                for(int i=0;i<3;i++){EditorApplication.QueuePlayerLoopUpdate();UnityEditorInternal.InternalEditorUtility.RepaintAllViews();yield return null;}
                int count=(int)type.GetProperty("count",flags).GetValue(null);var events=new List<object>();
                for(int i=0;i<count;i++) {
                    var obj=(Object)type.GetMethod("GetFrameEventObject",flags).Invoke(null,new object[]{i});
                    events.Add(new{index=i,name=(string)type.GetMethod("GetFrameEventInfoName",flags).Invoke(null,new object[]{i}),target=obj==null?null:obj.name,type=obj==null?null:obj.GetType().FullName});
                }
                Record("TS-frame-debugger",new{attempted=true,supported,enabledDuringRead=UnityEngine.FrameDebugger.enabled,count,events,scope="actual local debugger API; no window opened/focused; no event RT screenshots"});
            }finally{enable.Invoke(null,new object[]{false,0});EditorApplication.update-=safety;}
            Assert.That(UnityEngine.FrameDebugger.enabled,Is.False);
        }
        [UnityTest] public IEnumerator RepeatedSwitchCleanupAndRuntimeIsolation()
        {
            var production=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();int accepted=production.AcceptedCount,observers=ShibuyaCombatDamageResults.ObserverCount;var profile=production.Profile;
            for(int j=0;j<3;j++)for(int k=0;k<4;k++) {
                var p=new ShibuyaBlackFlashTemporalShapePreview((ShibuyaTemporalShapeKind)k,point,Vector3.forward,71,qa);
                for(int i=0;i<50;i++)p.Sample(i/60f,(ShibuyaTemporalShapeView)(i%5));p.Dispose();yield return null;
            }
            yield return Wait(.1f);
            Assert.That(Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name=="BlackFlash TS owned material"),Is.Zero);
            Assert.That(Resources.FindObjectsOfTypeAll<ShibuyaBlackFlashProfile>().Count(m=>m.name=="BlackFlash TS owned profile"),Is.Zero);
            Assert.That(Resources.FindObjectsOfTypeAll<GameObject>().Count(g=>g.name.StartsWith("BlackFlash TS ")),Is.Zero);
            Assert.That(production.Profile,Is.SameAs(profile));Assert.That(production.AcceptedCount,Is.EqualTo(accepted));Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers));
            Record("TS-cleanup",new{cycles=12,ownedMaterials=0,ownedProfiles=0,ownedRoots=0,accepted,observers});
            completed=true;
        }
    }
}
