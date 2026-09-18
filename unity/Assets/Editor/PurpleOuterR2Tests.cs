using System;
using System.Collections;
using System.IO;
using JJKGame.Dev.PurpleBodyExploration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace JJKGame.EditorTools
{
    public sealed class PurpleOuterR2Tests
    {
        private const string Output="D:/JJK_game/unity/Logs/PurpleOuterR2QA";
        private static PurpleOuterR2Profile Profile=>Resources.Load<PurpleOuterR2Profile>("VFX/PurpleOuterR2Profile");
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        private static Texture2D Read(PurpleOuterR2Bench bench)
        {
            bench.Common.RenderNow();var old=RenderTexture.active;var image=new Texture2D(960,540,TextureFormat.RGB24,false);
            try{RenderTexture.active=bench.Common.Preview;image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();}
            finally{RenderTexture.active=old;}return image;
        }
        private static void Still(PurpleOuterR2Bench bench,string file)
        {var image=Read(bench);try{File.WriteAllBytes(file,image.EncodeToPNG());}finally{Object.DestroyImmediate(image);}}
        private static GameObject BrightBackdrop(Transform parent,Vector3 centre,out Material light,out Material grid)
        {
            var root=new GameObject("OuterR2_QA_BrightBackdrop");root.transform.SetParent(parent,false);root.transform.position=centre+Vector3.forward*7;
            light=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="OuterR2_QA_Backdrop_Light"};light.SetColor("_BaseColor",new Color(.8f,.82f,.85f));
            grid=new Material(light){name="OuterR2_QA_Backdrop_Grid"};grid.SetColor("_BaseColor",new Color(.36f,.40f,.46f));
            void Plate(Vector3 p,Vector3 scale,Material material)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.transform.SetParent(root.transform,false);go.transform.localPosition=p;go.transform.localScale=scale;
                go.GetComponent<Collider>().enabled=false;Object.Destroy(go.GetComponent<Collider>());
                var renderer=go.GetComponent<Renderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            }
            Plate(Vector3.zero,new Vector3(34,20,.12f),light);
            for(int x=-14;x<=14;x+=2)Plate(new Vector3(x,0,-.08f),new Vector3(.08f,20,.04f),grid);
            for(int y=-8;y<=8;y+=2)Plate(new Vector3(0,y,-.08f),new Vector3(34,.08f,.04f),grid);
            return root;
        }
        private static void ShaderCheck()
        {
            foreach(string path in new[]{"VFX/PurpleOuterR2Halo","VFX/PurpleOuterR2Particle","VFX/PurpleOuterR2Distortion"})
            {var shader=Resources.Load<Shader>(path);Assert.That(shader.isSupported,Is.True);foreach(var msg in ShaderUtil.GetShaderMessages(shader))Assert.That(msg.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),msg.message);}
        }
        [UnityTest] public IEnumerator SafePreviewCompileAndClose()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;Time.timeScale=0;
            var main=Camera.main;var data=main.GetComponent<UniversalAdditionalCameraData>();var user=GameObject.Find("Purple_UserPrototype");Assert.That(user,Is.Not.Null);
            var pos=main.transform.position;float fov=main.fieldOfView;var target=main.targetTexture;bool post=data.renderPostProcessing;
            var userPosition=user.transform.position;int children=user.transform.childCount;
            var editor=UnityEditor.Editor.CreateEditor(Profile);Assert.That(editor,Is.TypeOf<PurpleOuterR2ProfileEditor>());Object.DestroyImmediate(editor);
            var root=new GameObject("LightweightOuterR2PreviewTest");var bench=root.AddComponent<PurpleOuterR2Bench>();bench.Configure(Profile,main);bench.Common.BenchmarkCamera.enabled=false;
            string folder=Path.Combine(Output,"Previews",Profile.previewStage+"_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));Directory.CreateDirectory(folder);Debug.Log("OuterR2 preview: "+folder);
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                bench.Select(false);bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Body_Front.png"));
                bench.Select(true);
                bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"OuterR2_Front.png"));
                bench.Sample(1.5f,PurpleBenchmarkView.Side);Still(bench,Path.Combine(folder,"OuterR2_Side.png"));bench.Sample(2.4f,PurpleBenchmarkView.Orbit);Still(bench,Path.Combine(folder,"OuterR2_Orbit.png"));
                if(Profile.previewStage>=PurpleOuterStage.Lightning)
                {
                    float best=0,power=-1;
                    for(int frame=0;frame<=90;frame++){bench.Sample(frame/30f,PurpleBenchmarkView.Front);if(bench.Outer.ArcPreviewPower>power){best=frame/30f;power=bench.Outer.ArcPreviewPower;}}
                    bench.Sample(best,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"OuterR2_Burst.png"));
                    Assert.That(bench.Outer.GetComponentsInChildren<ParticleSystem>(true).Length,Is.EqualTo(2));
                    int particles=0;foreach(var ps in bench.Outer.GetComponentsInChildren<ParticleSystem>())particles+=ps.particleCount;
                    Assert.That(particles,Is.GreaterThan(0));
                    File.WriteAllText(Path.Combine(folder,"burst-time.txt"),best.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if(Profile.previewStage==PurpleOuterStage.Distortion)
                {
                    bench.Outer.Stage=PurpleOuterStage.Lightning;bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Distortion_Off.png"));
                    bench.Outer.Stage=PurpleOuterStage.Distortion;bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Distortion_On.png"));
                }
                if(Profile.previewStage>=PurpleOuterStage.Debris)
                {
                    var backdrop=BrightBackdrop(root.transform,bench.Body.transform.position,out var light,out var grid);
                    bench.Select(false);bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Bright_Body.png"));
                    bench.SelectCandidate(1);bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Bright_OuterR1.png"));
                    bench.Select(true);bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Bright_OuterR2.png"));
                    float heroTime=0,heroPower=-1;
                    for(int frame=0;frame<=90;frame++){bench.Sample(frame/30f,PurpleBenchmarkView.Front);if(bench.Outer.HeroPreviewPower>heroPower){heroTime=frame/30f;heroPower=bench.Outer.HeroPreviewPower;}}
                    bench.Sample(heroTime,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Bright_HeroFragment.png"));
                    File.WriteAllText(Path.Combine(folder,"hero-time.txt"),heroTime.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    if(Profile.previewStage==PurpleOuterStage.Distortion)
                    {
                        bench.Outer.Stage=PurpleOuterStage.Lightning;bench.Sample(1.5f,PurpleBenchmarkView.Front);Still(bench,Path.Combine(folder,"Bright_Distortion_Off.png"));
                        bench.Outer.Stage=PurpleOuterStage.Distortion;bench.Sample(2.4f,PurpleBenchmarkView.Orbit);Still(bench,Path.Combine(folder,"Bright_Orbit.png"));
                    }
                    backdrop.SetActive(false);Object.Destroy(backdrop);Object.Destroy(light);Object.Destroy(grid);
                }
                ShaderCheck();
            }
            finally{ShaderUtil.allowAsyncCompilation=async;}
            Assert.That(main.transform.position,Is.EqualTo(pos));Assert.That(main.fieldOfView,Is.EqualTo(fov));Assert.That(main.targetTexture,Is.SameAs(target));Assert.That(data.renderPostProcessing,Is.EqualTo(post));
            Assert.That(user.transform.position,Is.EqualTo(userPosition));Assert.That(user.transform.childCount,Is.EqualTo(children));
            Assert.That(root.GetComponentsInChildren<JJKGame.Core.Health>(true),Is.Empty);
            root.SetActive(false);Object.Destroy(root);yield return null;yield return null;
            Assert.That(Object.FindFirstObjectByType<PurpleOuterR2Runtime>(),Is.Null);
            foreach(var m in Resources.FindObjectsOfTypeAll<Material>())Assert.That(m.name.StartsWith("PurpleOuterR2_"),Is.False,"OuterR2 material cleanup");
            yield return new ExitPlayMode();
        }
        [UnityTest]public IEnumerator MinimumStillAndMotion()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;Time.timeScale=0;
            var root=new GameObject("OuterR2MinimumRender");var bench=root.AddComponent<PurpleOuterR2Bench>();bench.Configure(Profile,Camera.main);bench.Common.BenchmarkCamera.enabled=false;
            string folder=Path.Combine(Output,"Motion",DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));Directory.CreateDirectory(folder);Debug.Log("OuterR2 motion: "+folder);
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                foreach(var view in new[]{PurpleBenchmarkView.Front,PurpleBenchmarkView.Side,PurpleBenchmarkView.SlightLow})
                {bench.Sample(1.5f,view);Still(bench,Path.Combine(folder,"OuterR2_"+view+".png"));}
                foreach(var view in new[]{PurpleBenchmarkView.Front,PurpleBenchmarkView.Orbit})
                using(var output=new FileStream(Path.Combine(folder,view+".rgb24"),FileMode.CreateNew))
                {
                    for(int frame=0;frame<=90;frame++)
                    {bench.Sample(frame/30f,view);var image=Read(bench);try{byte[] bytes=image.GetRawTextureData<byte>().ToArray();output.Write(bytes,0,bytes.Length);}finally{Object.DestroyImmediate(image);}}
                }
                File.WriteAllText(Path.Combine(folder,"format.json"),"{\"width\":960,\"height\":540,\"fps\":30,\"frames\":91,\"format\":\"RGB24 bottom-up\"}");ShaderCheck();
            }
            finally{ShaderUtil.allowAsyncCompilation=async;}
            root.SetActive(false);Object.Destroy(root);yield return null;yield return null;yield return new ExitPlayMode();
        }
        [UnityTest]public IEnumerator HeroFragmentMotionReview()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;yield return null;Time.timeScale=0;
            var root=new GameObject("OuterR2HeroMotionReview");var bench=root.AddComponent<PurpleOuterR2Bench>();bench.Configure(Profile,Camera.main);bench.Common.BenchmarkCamera.enabled=false;
            bench.Outer.Stage=PurpleOuterStage.Debris;
            var backdrop=BrightBackdrop(root.transform,bench.Body.transform.position,out var light,out var grid);
            var particles=new ParticleSystem.Particle[64];var ps=bench.Outer.GetComponentInChildren<ParticleSystem>();
            float best=0,score=-1;
            for(int frame=0;frame<=90;frame++)
            {
                bench.Sample(frame/30f,PurpleBenchmarkView.Front);int count=ps.GetParticles(particles);float value=0;
                for(int j=0;j<count;j++)
                {
                    var p=particles[j];if(p.startSize3D.y<.5f)continue;
                    Vector3 world=ps.transform.TransformPoint(p.position),eye=bench.Common.BenchmarkCamera.transform.position;
                    Vector3 ray=(world-eye).normalized,origin=eye-bench.Body.transform.position;
                    float clearance=(origin-ray*Vector3.Dot(origin,ray)).magnitude;
                    value+=p.startColor.a/255f*Mathf.Max(0,clearance-2.8f)*p.startSize3D.y;
                }
                if(value>score){score=value;best=frame/30f;}
            }
            string folder=Path.Combine(Output,"HeroMotion",DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));Directory.CreateDirectory(folder);
            float start=Mathf.Clamp(best-.25f,0,2.4f);bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            try
            {
                using(var output=new FileStream(Path.Combine(folder,"Front.rgb24"),FileMode.CreateNew))
                for(int frame=0;frame<19;frame++)
                {bench.Sample(start+frame/30f,PurpleBenchmarkView.Front);var image=Read(bench);try{byte[] bytes=image.GetRawTextureData<byte>().ToArray();output.Write(bytes,0,bytes.Length);}finally{Object.DestroyImmediate(image);}}
                File.WriteAllText(Path.Combine(folder,"format.json"),"{\"width\":960,\"height\":540,\"fps\":30,\"frames\":19,\"start\":"+start.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"best\":"+best.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
                ShaderCheck();Debug.Log("OuterR2 hero motion: "+folder);
            }
            finally{ShaderUtil.allowAsyncCompilation=async;}
            backdrop.SetActive(false);Object.Destroy(light);Object.Destroy(grid);root.SetActive(false);Object.Destroy(root);yield return null;yield return null;yield return new ExitPlayMode();
        }
    }
}
