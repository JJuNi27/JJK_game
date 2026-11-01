using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4H0Rev1DataTests
    {
        [Test] public void ForwardPrimaryFanMotionStreamsAndSeedIsolation()
        {
            var p=Resources.Load<ShibuyaBlackFlashProfile>("Shibuya Black Flash");var m=new Mesh();var g=new ShibuyaBlackFlashGeometry();
            var state=UnityEngine.Random.state;
            try {
                g.Build(m,2,p,71);var v=m.vertices;var uv=new List<Vector4>();m.GetUVs(0,uv);
                Assert.That(uv.Count,Is.EqualTo(v.Length));Assert.That(uv.All(x=>float.IsFinite(x.x)&&float.IsFinite(x.y)&&float.IsFinite(x.z)),Is.True);
                var indices=Enumerable.Range(0,v.Length).Where(i=>uv[i].y==1).ToArray();
                Assert.That(indices.Select(i=>uv[i].z).Distinct().Count(),Is.EqualTo(p.primaryBranchCount));
                Assert.That(indices.Average(i=>v[i].z),Is.GreaterThan(.4f),"actual primary geometry projects into strike +Z");
                Assert.That(indices.Max(i=>v[i].z),Is.GreaterThan(1.2f));
                Assert.That(indices.Max(i=>v[i].x)-indices.Min(i=>v[i].x),Is.GreaterThan(1));
                var first=v;g.Build(m,2,p,71);Assert.That(m.vertices,Is.EqualTo(first));
                Assert.That(JsonUtility.ToJson(UnityEngine.Random.state),Is.EqualTo(JsonUtility.ToJson(state)),"presentation random must be isolated");
                g.Build(m,2,p,72);Assert.That(m.vertices.SequenceEqual(first),Is.False);
            } finally {Object.DestroyImmediate(m);}
        }
    }
    public sealed class ShibuyaPass4H0Rev1Tests:ShibuyaPass4CFixture
    {
        bool completed;
        [UnitySetUp] public IEnumerator Ready(){completed=false;Place(dummy,player.transform.position+Vector3.forward*1.4f);yield return null;}
        [TearDown] public void Completion()=>Assert.That(completed,Is.True,"reach final REV1 assertions");
        [UnityTest] public IEnumerator SixPhasesAnglesAndRetainedGpuAnimation()
        {
            var owner=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();Vector3 point=dummy.transform.position+Vector3.up;
            var v=owner.Preview(point,Vector3.forward,71);
            var meshes=v.GetComponentsInChildren<MeshFilter>(true).Select(x=>x.sharedMesh).ToArray();
            var ids=meshes.Select(m=>m.GetInstanceID()).ToArray();var positions=meshes.Select(m=>m.vertices).ToArray();
            var poses=new[]{new Vector3(0,2,-6),new Vector3(4,2,1),new Vector3(1.8f,1.6f,-2.8f),new Vector3(4,2,-3),new Vector3(1,5,-1)};
            string[] names={"combat","side","close","diagonal","high"};
            float[] ages={.02f,.05f,.09f,.14f,.21f,.32f,.43f,.6f};
            for(int i=0;i<poses.Length;i++) {
                camera.transform.position=point+poses[i];camera.transform.LookAt(point);
                foreach(float age in ages){v.Sample(age);Capture("REV1-phases/"+names[i]+"-"+age.ToString("F2",System.Globalization.CultureInfo.InvariantCulture));}
            }
            for(int i=0;i<120;i++)v.Sample(i*.005f);
            Assert.That(meshes.Select(m=>m.GetInstanceID()),Is.EqualTo(ids));
            for(int i=0;i<meshes.Length;i++)Assert.That(meshes[i].vertices,Is.EqualTo(positions[i]),"fixed CPU buffers; GPU deforms from phase data");
            Assert.That(v.VertexCount,Is.LessThan(20000));Assert.That(Time.timeScale,Is.EqualTo(1));
            Record("REV1-phases",new{v.VertexCount,ages,ids,note="same seed/sample time; CPU topology retained; actual GPU images are visual evidence"});
            completed=true;yield return null;
        }
        [UnityTest] public IEnumerator AsyncOriginalSpeedMovingCameraCapture()
        {
            Assert.That(SystemInfo.supportsAsyncGPUReadback,Is.True,"no silent screenshot fallback");
            var owner=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();Vector3 point=dummy.transform.position+Vector3.up;
            var rt=new RenderTexture(640,480,24,RenderTextureFormat.ARGB32);rt.Create();
            var frames=new List<byte[]>();var stamps=new List<double>();var intervals=new List<float>();
            var gameFrames=new List<int>();int pending=0,errors=0,lastFrame=-1;
            var v=owner.Preview(point,Vector3.forward,71);double start=v.Started;
            try {
                while(Time.unscaledTimeAsDouble-start<1.05) {
                    // Editor test coroutines may resume repeatedly within one runtime frame.
                    if(Time.frameCount==lastFrame){yield return null;continue;}
                    lastFrame=Time.frameCount;gameFrames.Add(lastFrame);
                    double age=Time.unscaledTimeAsDouble-start;float a=(float)age*.8f;
                    camera.transform.position=point+new Vector3(Mathf.Sin(a)*6,2,-Mathf.Cos(a)*6);camera.transform.LookAt(point);
                    RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.StandardRequest{destination=rt});
                    int index=frames.Count;frames.Add(null);stamps.Add(age);intervals.Add(Time.unscaledDeltaTime);pending++;
                    AsyncGPUReadback.Request(rt,0,TextureFormat.RGBA32,request=>{if(request.hasError)errors++;else frames[index]=request.GetData<byte>().ToArray();pending--;});
                    yield return null;
                }
                yield return Until(()=>pending==0,5);Assert.That(errors,Is.Zero);Assert.That(frames.All(x=>x!=null),Is.True);
                string folder=Path.Combine(Root,"REV1-async-video");Directory.CreateDirectory(folder);
                var tex=new Texture2D(640,480,TextureFormat.RGBA32,false);
                try{for(int i=0;i<frames.Count;i++){tex.LoadRawTextureData(frames[i]);tex.Apply();File.WriteAllBytes(Path.Combine(folder,"frame-"+i.ToString("D4")+".png"),tex.EncodeToPNG());}}
                finally{Object.Destroy(tex);}
                Assert.That(gameFrames.Distinct().Count(),Is.EqualTo(frames.Count));
                Record("REV1-async-timestamps",new{times=stamps,intervals,gameFrames,frames=frames.Count,width=640,height=480,seed=71,note="actual realtime sample intervals, encode after capture, no fixed captureDeltaTime"});
                Assert.That(owner.AcceptedCount,Is.Zero);Assert.That(Time.timeScale,Is.EqualTo(1));completed=true;
            } finally {rt.Release();Object.Destroy(rt);}
        }
    }
}
