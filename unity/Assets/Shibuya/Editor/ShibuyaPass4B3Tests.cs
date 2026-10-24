using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public abstract class ShibuyaPass4B3Fixture
    {
        protected ShibuyaFighter player,dummy;
        protected ShibuyaPlayerInput input;
        protected ShibuyaInputFrame frame;
        protected CursedEnergyAuraController aura;
        protected CursedEnergyAuraProfile settings;
        protected ShibuyaCombatProfile combat;
        protected Camera camera;
        protected static string Root=>ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass4B3QA");
        [UnitySetUp] public IEnumerator Setup()
        {
            frame=default;EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();yield return null;
            var match=Object.FindFirstObjectByType<ShibuyaMatch>();player=match.Player;dummy=match.Dummy;
            input=player.GetComponent<ShibuyaPlayerInput>();input.TestInput=()=>frame;
            aura=input.Aura;settings=Clone(aura.Profile);aura.Configure(settings);
            combat=Clone(player.Profile);player.Configure(combat,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Player Stats.asset"));
            var orbit=Object.FindFirstObjectByType<ShibuyaOrbitCamera>();camera=orbit.GetComponent<Camera>();orbit.enabled=false;
            Place(player,new Vector3(0,.03f,-1.4f));Place(dummy,new Vector3(8,.03f,8));player.transform.rotation=Quaternion.identity;
            yield return Wait(.1f);
        }
        protected static T Clone<T>(T asset) where T:ScriptableObject {var copy=Object.Instantiate(asset);copy.name="PASS4B3 QA "+asset.name;return copy;}
        [UnityTearDown] public IEnumerator Leave(){if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        [TearDown] public void Cleanup(){foreach(var a in Resources.FindObjectsOfTypeAll<ScriptableObject>())if(a.name.StartsWith("PASS4B3 QA ",StringComparison.Ordinal)&&!AssetDatabase.Contains(a))Object.DestroyImmediate(a);}
        protected static IEnumerator Wait(float seconds){double end=Time.unscaledTimeAsDouble+seconds;while(Time.unscaledTimeAsDouble<end)yield return null;}
        protected static void Place(ShibuyaFighter f,Vector3 p){var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=p;cc.enabled=true;Physics.SyncTransforms();}
        protected void Pose(Vector3 offset){camera.transform.position=player.transform.position+offset;camera.transform.LookAt(player.transform.position+Vector3.up*1.15f);}
        protected void Capture(string name)
        {
            string path=Path.Combine(Root,name+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));
            var rt=RenderTexture.GetTemporary(960,720,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var previous=RenderTexture.active;
            var image=new Texture2D(960,720,TextureFormat.RGB24,false);
            try{RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.StandardRequest{destination=rt});RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
            finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);Object.Destroy(image);}
        }
        protected void Views(string folder)
        {
            // All views are captured in the same frame at the same shader phase.
            foreach(var m in aura.Visual.GetComponentsInChildren<Renderer>().Select(r=>r.sharedMaterial).Where(m=>m!=null&&m.HasProperty("_AuraTime")).Distinct())m.SetFloat("_AuraTime",2);
            var offsets=new[]{new Vector3(0,2.2f,4.5f),new Vector3(4.5f,2.2f,0),new Vector3(3.18f,2.2f,3.18f),new Vector3(0,2.2f,-4.5f),new Vector3(2.1f,5.2f,2.1f)};
            var names=new[]{"front","side","diagonal","back","high"};
            for(int i=0;i<offsets.Length;i++){Pose(offsets[i]);Capture(folder+"/"+names[i]);}
        }
        protected IEnumerator Orbit(string folder)
        {
            var samples=new List<object>();double start=Time.unscaledTimeAsDouble;int index=0;
            while(Time.unscaledTimeAsDouble-start<4){float t=(float)(Time.unscaledTimeAsDouble-start),angle=t*90*Mathf.Deg2Rad;Pose(new Vector3(Mathf.Sin(angle)*4.5f,2.2f,Mathf.Cos(angle)*4.5f));Capture(folder+"/frame-"+index.ToString("D4"));samples.Add(new{index,time=Time.unscaledTimeAsDouble-start});index++;yield return Wait(1f/30);}
            Record(folder+"-timestamps",samples);
        }
        protected static void Record(string name,object data){Directory.CreateDirectory(Root);File.WriteAllText(Path.Combine(Root,name+".json"),Newtonsoft.Json.JsonConvert.SerializeObject(data,Newtonsoft.Json.Formatting.Indented));}
    }
    public sealed class ShibuyaPass4B3Baseline : ShibuyaPass4B3Fixture
    {
        [UnityTest] public IEnumerator OriginalPass4B2VisualEvidence()
        {
            frame.auraHeld=true;yield return Wait(.7f);
            Views("baseline");yield return Orbit("baseline-orbit");
            Record("baseline",new{vertices=aura.Visual.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.vertexCount),fieldFits=aura.Visual.FieldFitCount});
        }
    }
    public sealed class ShibuyaPass4B3Tests : ShibuyaPass4B3Fixture
    {
        private CursedEnergyProfile reserve;
        private void Capacity(float value)
        {
            if(reserve==null){reserve=Clone(combat.cursedEnergy);combat.cursedEnergy=reserve;}
            var so=new SerializedObject(reserve);so.FindProperty("maxEnergy").floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();
        }
        private Mesh Field=>aura.Visual.transform.Find("MainAura").GetComponent<MeshFilter>().sharedMesh;
        private float FieldWidth=>Field.vertices.Max(v=>v.x)-Field.vertices.Min(v=>v.x);
        private object ShapeMetric(float max)=>new{max,multiplier=aura.SizeMultiplier,expansion=aura.Visual.Expansion,width=FieldWidth,
            minY=Field.vertices.Min(v=>v.y),maxY=Field.vertices.Max(v=>v.y),vertices=Field.vertexCount,fitMs=aura.Visual.FieldFitMilliseconds};
        [UnityTest] public IEnumerator CurveDataAndSafety()
        {
            Assert.That(combat.cursedEnergy,Is.Not.Null);Assert.That(combat.cursedEnergy.MaxEnergy,Is.EqualTo(100));
            foreach(var pair in new[]{new[]{50f,.85f},new[]{100f,1},new[]{150f,1.5f},new[]{200f,2f},new[]{250f,2.2f},new[]{300f,2.3f}})
                Assert.That(settings.SizeMultiplier(pair[0]),Is.EqualTo(pair[1]).Within(.00001f));
            float early=settings.SizeMultiplier(200)-settings.SizeMultiplier(150),late=settings.SizeMultiplier(300)-settings.SizeMultiplier(250);
            Assert.That(late,Is.LessThan(early));
            foreach(float invalid in new[]{0f,-100,float.NaN,float.PositiveInfinity})Assert.That(settings.SizeMultiplier(invalid),Is.EqualTo(1));
            Assert.That(settings.SizeMultiplier(float.MaxValue),Is.InRange(.5f,3));
            settings.maxEnergySizeCurve=AnimationCurve.Constant(0,5,99);Assert.That(settings.SizeMultiplier(100),Is.EqualTo(3));
            settings.maxEnergySizeCurve=AnimationCurve.Constant(0,5,-2);Assert.That(settings.SizeMultiplier(100),Is.EqualTo(.5f));
            settings.maxEnergySizeCurve=null;Assert.That(settings.SizeMultiplier(100),Is.EqualTo(1));
            settings.referenceMaxEnergy=float.NaN;settings.minimumSizeMultiplier=float.NaN;settings.maximumSizeMultiplier=-2;
            Assert.That(settings.SizeMultiplier(100),Is.EqualTo(1));
            Record("curve-safety",new{defaultValues="50=.85,100=1,150=1.5,200=2,250=2.2,300=2.3",invalidInputs="finite fallback/clamp verified"});yield return null;
        }
        [UnityTest] public IEnumerator MaximumChangesSmoothlyAndCurrentEnergyDoesNotMatter()
        {
            Capacity(100);frame.auraHeld=true;yield return Wait(.4f);Assert.That(aura.SizeMultiplier,Is.EqualTo(1));
            Capacity(200);yield return null;yield return null;
            float intermediate=aura.SizeMultiplier;Assert.That(intermediate,Is.GreaterThan(1).And.LessThan(2));
            yield return Wait(1.6f);Assert.That(aura.SizeMultiplier,Is.EqualTo(2).Within(.001f));
            var energy=CursedEnergyController.GetOrCreate(player.gameObject);energy.ApplyProfile(reserve);energy.enabled=false;
            float width=FieldWidth;var measurements=new List<object>();
            foreach(float current in new[]{200f,10,0,180}) {
                energy.SetCurrentEnergy(current);yield return Wait(.12f);
                Assert.That(aura.SizeMultiplier,Is.EqualTo(2).Within(.001f));Assert.That(FieldWidth,Is.EqualTo(width).Within(.00001f));
                measurements.Add(new{current=energy.CurrentEnergy,maximum=aura.MaximumEnergy,multiplier=aura.SizeMultiplier,width=FieldWidth});
            }
            Capacity(150);yield return Wait(1.6f);Assert.That(aura.SizeMultiplier,Is.EqualTo(1.5f).Within(.001f));
            Record("capacity-and-current",new{intermediate,measurements,finalMultiplier=aura.SizeMultiplier});
        }
        [UnityTest] public IEnumerator ExpansionPreservesRootsBodyAndInk()
        {
            Capacity(100);frame.auraHeld=true;yield return Wait(.5f);var visual=aura.Visual;
            var roots=Enumerable.Range(0,visual.BodyAnchorCount).Select(visual.BodyRootWorldPosition).ToArray();
            var controller=player.GetComponent<CharacterController>();float height=controller.height,radius=controller.radius;
            var scale=player.transform.localScale;float baseY=Field.vertices.Min(v=>v.y),previousWidth=0;
            var main=Field;var evidence=new List<object>();
            foreach(float amount in new[]{100f,150,200}) {
                Capacity(amount);yield return Wait(1.6f);
                Assert.That(Field,Is.SameAs(main));Assert.That(FieldWidth,Is.GreaterThan(previousWidth));previousWidth=FieldWidth;
                Assert.That(Field.vertices.Min(v=>v.y),Is.EqualTo(baseY).Within(.0001f));
                for(int i=0;i<roots.Length;i++)Assert.That(Vector3.Distance(roots[i],visual.BodyRootWorldPosition(i)),Is.LessThan(.001f));
                var ink=visual.transform.Find("InkRuptures").GetComponent<MeshFilter>().sharedMesh;
                Assert.That(ink.vertices,Is.EqualTo(Field.vertices));Assert.That(visual.OwnedMeshCount,Is.EqualTo(3));
                Assert.That(player.transform.localScale,Is.EqualTo(scale));Assert.That(controller.height,Is.EqualTo(height));Assert.That(controller.radius,Is.EqualTo(radius));
                Assert.That(combat.strikes.Select(s=>s.damage),Is.EqualTo(new[]{20f,20,20,25}));
                evidence.Add(ShapeMetric(amount));
            }
            Record("expansion-roots-collision",evidence);
        }
        [UnityTest] public IEnumerator ColorProfilesAndInspectorUndoKeepCapacityIndependent()
        {
            Capacity(200);frame.auraHeld=true;yield return Wait(1.6f);
            foreach(string preset in new[]{"Pink Aura","Green Aura"}) {
                var other=Clone(AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(CursedEnergyAuraAssets.Root+"/Presets/"+preset+".asset"));
                aura.Configure(other);yield return Wait(.3f);Assert.That(aura.SizeMultiplier,Is.EqualTo(2));
                Assert.That(aura.Visual.EnergyMaterial.GetColor("_MainColor"),Is.EqualTo(other.mainColor));
                Assert.That(aura.Visual.EnergyMaterial.GetColor("_InkColor"),Is.EqualTo(other.inkColor));
                Capture(preset.Replace(' ','-'));
            }
            var so=new SerializedObject(settings);float old=settings.referenceMaxEnergy;Undo.IncrementCurrentGroup();
            so.FindProperty("referenceMaxEnergy").floatValue=125;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();
            Assert.That(settings.referenceMaxEnergy,Is.EqualTo(old));
            foreach(string name in new[]{"referenceMaxEnergy","baselineExpansion","maxEnergySizeCurve","minimumSizeMultiplier","maximumSizeMultiplier","sizeResponseTime"})
                Assert.That(CursedEnergyAuraProfileInspector.Labels.ContainsKey(name),Is.True,name);
            Assert.That(ShibuyaInspector.KoreanLabel("cursedEnergy"),Is.Not.EqualTo("cursedEnergy"));
            Assert.That(AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Dummy Stats.asset").MaxHealth,Is.EqualTo(1000));
            Assert.That(AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Player Stats.asset").MaxHealth,Is.EqualTo(100));
        }
        private IEnumerator SizeEvidence(float amount)
        {
            Capacity(amount);frame.auraHeld=true;yield return Wait(1.6f);
            Assert.That(aura.SizeMultiplier,Is.EqualTo(settings.SizeMultiplier(amount)).Within(.001f));
            string name="max-"+amount.ToString("0");Views(name);Record(name,ShapeMetric(amount));yield return Orbit(name+"-orbit");
        }
        [UnityTest] public IEnumerator Size100VisualEvidence(){yield return SizeEvidence(100);}
        [UnityTest] public IEnumerator Size150VisualEvidence(){yield return SizeEvidence(150);}
        [UnityTest] public IEnumerator Size200VisualAndMovingEvidence()
        {
            yield return SizeEvidence(200);Assert.That(aura.Visual,Is.Not.Null,"size evidence must complete before moving capture");
            var samples=new List<object>();double start=Time.unscaledTimeAsDouble;int index=0;
            while(Time.unscaledTimeAsDouble-start<3){float t=(float)(Time.unscaledTimeAsDouble-start);frame.move=t<1.5f?Vector2.up:Vector2.right;Pose(new Vector3(3,2.2f,3));Capture("max-200-moving/frame-"+index.ToString("D4"));
                samples.Add(new{index,time=Time.unscaledTimeAsDouble-start,x=player.transform.position.x,z=player.transform.position.z,multiplier=aura.SizeMultiplier,fitCount=aura.Visual.FieldFitCount,fitMs=aura.Visual.FieldFitMilliseconds});index++;yield return Wait(1f/30);}
            frame.move=Vector2.zero;Record("max-200-moving-timestamps",samples);
        }
        [UnityTest] public IEnumerator RepeatedSizeChangesAndFKeepResourceCounts()
        {
            Capacity(100);frame.auraHeld=true;yield return Wait(.3f);var visual=aura.Visual;
            int materials=visual.OwnedMaterialCount,meshes=visual.OwnedMeshCount;
            for(int i=0;i<15;i++){frame.auraHeld=false;yield return Wait(.04f);Capacity(i%2==0?200:100);frame.auraHeld=true;yield return Wait(.08f);}
            Assert.That(aura.Visual,Is.SameAs(visual));Assert.That(visual.OwnedMaterialCount,Is.EqualTo(materials));Assert.That(visual.OwnedMeshCount,Is.EqualTo(meshes));
            frame.auraHeld=false;yield return Wait(.4f);Assert.That(visual.HasVisibleRenderers,Is.False);Assert.That(visual.ParticleCount,Is.Zero);
            Object.Destroy(visual.gameObject);yield return null;yield return null;
            Assert.That(Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("CursedAura_Owned_")),Is.Zero);
            Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("CursedAura_OwnedVolume_")),Is.Zero);
            Record("capacity-cleanup",new{cycles=15,materials,meshes,afterMaterials=0,afterMeshes=0});
        }
    }
}
