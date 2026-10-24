using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public abstract class ShibuyaPass4B3aFixture : ShibuyaPass4B3Fixture
    {
        protected CursedEnergyProfile reserve;
        protected void Capacity(float amount)
        {
            if(reserve==null){reserve=Clone(combat.cursedEnergy);combat.cursedEnergy=reserve;}
            var so=new SerializedObject(reserve);so.FindProperty("maxEnergy").floatValue=amount;so.ApplyModifiedPropertiesWithoutUndo();
        }
        protected void Phase(float time=2)
        {
            foreach(var m in aura.Visual.GetComponentsInChildren<Renderer>().Select(r=>r.sharedMaterial).Where(m=>m!=null&&m.HasProperty("_AuraTime")).Distinct())m.SetFloat("_AuraTime",time);
        }
        protected Mesh Field=>aura.Visual.transform.Find("MainAura").GetComponent<MeshFilter>().sharedMesh;
        protected object Metric(float amount)=>new{maximum=amount,multiplier=aura.SizeMultiplier,expansion=aura.Visual.Expansion,
            width=Field.vertices.Max(v=>v.x)-Field.vertices.Min(v=>v.x),minY=Field.vertices.Min(v=>v.y),maxY=Field.vertices.Max(v=>v.y),vertices=Field.vertexCount};
        protected void LayerCapture(string folder)
        {
            Phase();Pose(new Vector3(3.18f,2.2f,3.18f));
            var renderers=aura.Visual.GetComponentsInChildren<Renderer>();var enabled=renderers.Select(r=>r.enabled).ToArray();
            Capture(folder+"/combined");
            foreach(var r in renderers)r.enabled=r.name=="MainAura"||r.name=="InkRuptures";
            Capture(folder+"/main-field");
            foreach(var r in renderers)r.enabled=r.name=="EnergyWisps";
            Capture(folder+"/detail");
            for(int i=0;i<renderers.Length;i++)renderers[i].enabled=enabled[i];
        }
    }
    public sealed class ShibuyaPass4B3aBaseline : ShibuyaPass4B3aFixture
    {
        [UnityTest] public IEnumerator OriginalPass4B3SizeAndInternalEvidence()
        {
            frame.auraHeld=true;
            foreach(float amount in new[]{100f,150,200}) {
                Capacity(amount);yield return Wait(1.6f);string label="before-"+amount.ToString("0");
                Views(label);Record(label,Metric(amount));if(amount==100)LayerCapture("before-layers");
            }
        }
    }
    public sealed class ShibuyaPass4B3aTests : ShibuyaPass4B3aFixture
    {
        private void ComparisonCurve(bool high)
        {
            var keys=CursedEnergyAuraProfile.DefaultSizeCurve().keys;
            if(high)for(int i=0;i<keys.Length;i++) {
                if(keys[i].time>=2)keys[i].value+=.5f;
                if(keys[i].time==1.5f)keys[i].outTangent=2;
                if(keys[i].time==2)keys[i].inTangent=2;
            }
            settings.maxEnergySizeCurve=new AnimationCurve(keys);
        }
        private void FieldMode(float mode)
        {
            foreach(var r in aura.Visual.GetComponentsInChildren<Renderer>())if(r.sharedMaterial!=null&&r.sharedMaterial.HasProperty("_FieldViewMode"))r.sharedMaterial.SetFloat("_FieldViewMode",mode);
        }
        private void Inner(float visibility)
        {
            foreach(var r in aura.Visual.GetComponentsInChildren<Renderer>())if(r.sharedMaterial!=null&&r.sharedMaterial.HasProperty("_InnerFlow"))r.sharedMaterial.SetVector("_InnerFlow",new Vector4(visibility,settings.innerFlowDensity,0,0));
        }
        private static (int changed,float meanGreenDelta) Difference(string a,string b,RectInt region)
        {
            var x=new Texture2D(2,2);var y=new Texture2D(2,2);
            try{
                x.LoadImage(File.ReadAllBytes(Path.Combine(Root,a+".png")));y.LoadImage(File.ReadAllBytes(Path.Combine(Root,b+".png")));
                var p=x.GetPixels32();var q=y.GetPixels32();int changed=0,n=0;double delta=0;
                for(int row=region.yMin;row<region.yMax;row++)for(int col=region.xMin;col<region.xMax;col++) {
                    int i=row*x.width+col;float d=(q[i].g-p[i].g)/255f;delta+=d;n++;
                    if(Math.Abs(q[i].r-p[i].r)>2||Math.Abs(q[i].g-p[i].g)>2||Math.Abs(q[i].b-p[i].b)>2)changed++;
                }
                return (changed,(float)(delta/n));
            }finally{Object.Destroy(x);Object.Destroy(y);}
        }
        private double ProjectedCarrierArea()
        {
            var mesh=Field;var matrix=aura.Visual.transform.localToWorldMatrix;
            var screen=mesh.vertices.Select(v=>camera.WorldToViewportPoint(matrix.MultiplyPoint3x4(v))).ToArray();
            int[] tris=mesh.triangles;double sum=0;
            for(int i=0;i<tris.Length;i+=3){var a=screen[tris[i]];var b=screen[tris[i+1]];var c=screen[tris[i+2]];if(a.z<=0||b.z<=0||c.z<=0)continue;
                sum+=Math.Abs((b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x))*.5*960*720;}
            return sum; // Unculled CPU carrier only: NOT a measurement of GPU overdraw.
        }
        [UnityTest] public IEnumerator SizeCurveClampsAndInspectorSettings()
        {
            Assert.That(settings.SizeMultiplier(100),Is.EqualTo(1));Assert.That(settings.SizeMultiplier(150),Is.EqualTo(1.5f));Assert.That(settings.SizeMultiplier(200),Is.EqualTo(2));
            Assert.That(settings.SizeMultiplier(250)-settings.SizeMultiplier(200),Is.LessThan(settings.SizeMultiplier(200)-settings.SizeMultiplier(150)));
            ComparisonCurve(true);Assert.That(settings.SizeMultiplier(200),Is.EqualTo(2.5f));
            settings.maxEnergySizeCurve=AnimationCurve.Constant(0,10,20);Assert.That(settings.SizeMultiplier(200),Is.EqualTo(3));
            foreach(string field in new[]{"verticalSizeResponse","innerFlowVisibility","innerFlowDensity"})Assert.That(CursedEnergyAuraProfileInspector.Labels.ContainsKey(field),Is.True);
            var so=new SerializedObject(settings);float old=settings.innerFlowVisibility;Undo.IncrementCurrentGroup();so.FindProperty("innerFlowVisibility").floatValue=.2f;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(settings.innerFlowVisibility,Is.EqualTo(old));
            yield return null;
        }
        [UnityTest] public IEnumerator SameHundredSizeRootsAndBoundedVerticalGrowth()
        {
            Capacity(100);frame.auraHeld=true;yield return Wait(1.6f);
            var previous=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Logs/ShibuyaPass4B3aQA/20261024-size-interior/before/legacy/ShibuyaPass4B3QA/before-100.json"));
            float width=Field.vertices.Max(v=>v.x)-Field.vertices.Min(v=>v.x);
            // Baseline Metric uses 'width', 'minY', 'maxY' and is captured before runtime edits.
            Assert.That(width,Is.EqualTo((float)previous["width"]).Within(.0001f));
            Assert.That(Field.vertices.Max(v=>v.y),Is.EqualTo((float)previous["maxY"]).Within(.0001f));
            var visual=aura.Visual;var roots=Enumerable.Range(0,visual.BodyAnchorCount).Select(visual.BodyRootWorldPosition).ToArray();
            var cc=player.GetComponent<CharacterController>();var scale=player.transform.localScale;float radius=cc.radius,height=cc.height,minY=Field.vertices.Min(v=>v.y),last=0;
            var data=new List<object>();
            foreach(var sample in new[]{(100f,false),(150f,false),(200f,false),(200f,true)}) {
                ComparisonCurve(sample.Item2);Capacity(sample.Item1);yield return Wait(1.8f);
                float current=Field.vertices.Max(v=>v.x)-Field.vertices.Min(v=>v.x);Assert.That(current,Is.GreaterThan(last));last=current;
                for(int i=0;i<roots.Length;i++)Assert.That(Vector3.Distance(roots[i],visual.BodyRootWorldPosition(i)),Is.LessThan(.001f));
                Assert.That(Field.vertices.Min(v=>v.y),Is.EqualTo(minY).Within(.0001f));Assert.That(player.transform.localScale,Is.EqualTo(scale));Assert.That(cc.radius,Is.EqualTo(radius));Assert.That(cc.height,Is.EqualTo(height));
                Assert.That(visual.MainFlowCount,Is.Zero);Assert.That(visual.BodyAnchorCount,Is.EqualTo(24));Assert.That(Field.vertexCount,Is.EqualTo(2009));
                Assert.That(visual.transform.Find("InkRuptures").GetComponent<MeshFilter>().sharedMesh.vertices,Is.EqualTo(Field.vertices));
                if(sample.Item2)Assert.That(settings.VerticalExpansion(visual.Expansion),Is.LessThan(visual.Expansion));
                data.Add(Metric(sample.Item1));
            }
            Record("size-roots",data);
        }
        [UnityTest] public IEnumerator InteriorVisibilityAndOuterInkIsolationEvidence()
        {
            Capacity(100);frame.auraHeld=true;yield return Wait(1.6f);Phase();Pose(new Vector3(0,2.2f,4.5f));
            var all=aura.Visual.GetComponentsInChildren<Renderer>();var enabled=all.Select(r=>r.enabled).ToArray();
            Inner(0);Capture("inner/combined-before");Inner(settings.innerFlowVisibility);Capture("inner/combined-after");
            foreach(var r in all)r.enabled=r.name=="MainAura"||r.name=="InkRuptures";
            FieldMode(2);Inner(0);Capture("inner/flow-before");Inner(settings.innerFlowVisibility);Capture("inner/flow-after");
            FieldMode(1);Inner(0);Capture("inner/outer-before");Inner(settings.innerFlowVisibility);Capture("inner/outer-after");
            var outer=Difference("inner/outer-before","inner/outer-after",new RectInt(0,0,960,720));
            Assert.That(outer.changed,Is.Zero,"inner visibility must not change outer/ink pixels");
            var inside=Difference("inner/flow-before","inner/flow-after",new RectInt(420,200,120,250));
            Assert.That(inside.changed,Is.GreaterThan(100),"need visible interior pixels, not just a new material value");Assert.That(inside.meanGreenDelta,Is.GreaterThan(0));
            Capture("inner/outer-only");FieldMode(2);Capture("inner/interior-only");FieldMode(0);
            foreach(var r in all)r.enabled=r.name=="EnergyWisps";Capture("inner/detail-only");
            for(int i=0;i<all.Length;i++)all[i].enabled=enabled[i];Capture("inner/combined");
            Record("inner-pixel-evidence",new{outerChanged=outer.changed,bodyRoiChanged=inside.changed,meanGreenDelta=inside.meanGreenDelta,roi="bottom-left x420/y200/w120/h250",visibility=settings.innerFlowVisibility,density=settings.innerFlowDensity});
        }
        [UnityTest] public IEnumerator FourSizesVisualAndMovementEvidence()
        {
            frame.auraHeld=true;var area=new List<object>();
            foreach(var sample in new[]{(100f,false,"100-x1"),(150f,false,"150-x1.5"),(200f,false,"200-x2"),(200f,true,"200-x2.5")}) {
                ComparisonCurve(sample.Item2);Capacity(sample.Item1);yield return Wait(1.8f);Assert.That(aura.SizeMultiplier,Is.EqualTo(settings.SizeMultiplier(sample.Item1)).Within(.001f));
                Views(sample.Item3);Record(sample.Item3,Metric(sample.Item1));Pose(new Vector3(3.18f,2.2f,3.18f));area.Add(new{label=sample.Item3,projectedCarrierTrianglePixelSum=ProjectedCarrierArea()});
                yield return Orbit(sample.Item3+"-orbit");
            }
            Record("projected-carrier-area-proxy",area);
            var samples=new List<object>();double start=Time.unscaledTimeAsDouble;int index=0;
            while(Time.unscaledTimeAsDouble-start<3){float t=(float)(Time.unscaledTimeAsDouble-start);frame.move=t<1.5f?Vector2.up:Vector2.right;Pose(new Vector3(3,2.2f,3));Capture("200-x2.5-moving/frame-"+index.ToString("D4"));
                samples.Add(new{index,time=Time.unscaledTimeAsDouble-start,x=player.transform.position.x,z=player.transform.position.z,fitCount=aura.Visual.FieldFitCount,fitMs=aura.Visual.FieldFitMilliseconds});index++;yield return Wait(1f/30);}
            frame.move=Vector2.zero;Record("200-x2.5-moving-timestamps",samples);
        }
        [UnityTest] public IEnumerator CurrentEnergyColorsAndRepeatedResources()
        {
            ComparisonCurve(true);Capacity(200);frame.auraHeld=true;yield return Wait(1.8f);
            var energy=CursedEnergyController.GetOrCreate(player.gameObject);energy.ApplyProfile(reserve);energy.enabled=false;
            foreach(float current in new[]{200f,10,0,180}){energy.SetCurrentEnergy(current);yield return Wait(.1f);Assert.That(aura.SizeMultiplier,Is.EqualTo(2.5f));}
            var visual=aura.Visual;int materials=visual.OwnedMaterialCount,meshes=visual.OwnedMeshCount;
            for(int i=0;i<12;i++){frame.auraHeld=false;yield return Wait(.04f);frame.auraHeld=true;yield return Wait(.07f);}
            foreach(string name in new[]{"Pink Aura","Green Aura"}) {
                var other=Clone(AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(CursedEnergyAuraAssets.Root+"/Presets/"+name+".asset"));
                other.maxEnergySizeCurve=settings.maxEnergySizeCurve;other.maximumSizeMultiplier=3;aura.Configure(other);yield return Wait(.3f);
                Assert.That(aura.SizeMultiplier,Is.EqualTo(2.5f));Assert.That(visual.EnergyMaterial.GetColor("_MainColor"),Is.EqualTo(other.mainColor));
                Assert.That(visual.EnergyMaterial.GetColor("_InkColor"),Is.EqualTo(other.inkColor));Pose(new Vector3(3.18f,2.2f,3.18f));Capture(name.Replace(' ','-'));
            }
            Assert.That(visual.OwnedMaterialCount,Is.EqualTo(materials));Assert.That(visual.OwnedMeshCount,Is.EqualTo(meshes));
            frame.auraHeld=false;yield return Wait(.4f);Assert.That(visual.HasVisibleRenderers,Is.False);Assert.That(visual.ParticleCount,Is.Zero);
            Object.Destroy(visual.gameObject);yield return null;yield return null;
            int remainingMaterials=Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("CursedAura_Owned_"));int remainingMeshes=Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("CursedAura_OwnedVolume_"));
            Assert.That(remainingMaterials,Is.Zero);Assert.That(remainingMeshes,Is.Zero);Record("resources",new{materials,meshes,cycles=12,remainingMaterials,remainingMeshes,currentEnergyIndependent=true});
        }
        [UnityTest] public IEnumerator CloseInteriorMotionEvidence()
        {
            Capacity(100);frame.auraHeld=true;yield return Wait(1.6f);Pose(new Vector3(0,1.65f,2.4f));
            var samples=new List<object>();double start=Time.unscaledTimeAsDouble;int index=0;
            while(Time.unscaledTimeAsDouble-start<2.5){Capture("interior-close/frame-"+index.ToString("D4"));samples.Add(new{index,time=Time.unscaledTimeAsDouble-start});index++;yield return Wait(1f/30);}
            Record("interior-close-timestamps",samples);
        }
        [UnityTest] public IEnumerator InspectorInteriorDensityUpdatesOnLiveProfile()
        {
            Capacity(100);frame.auraHeld=true;yield return Wait(.4f);var so=new SerializedObject(settings);
            so.FindProperty("innerFlowVisibility").floatValue=.65f;so.FindProperty("innerFlowDensity").floatValue=.5f;so.ApplyModifiedProperties();
            yield return null;yield return null;
            Assert.That(aura.Visual.EnergyMaterial.GetVector("_InnerFlow").y,Is.EqualTo(.5f));
            foreach(var r in aura.Visual.GetComponentsInChildren<Renderer>())r.enabled=r.name=="MainAura"||r.name=="InkRuptures";
            Phase();Pose(new Vector3(0,2.2f,4.5f));FieldMode(2);Capture("density/low");
            so.Update();so.FindProperty("innerFlowDensity").floatValue=1.5f;so.ApplyModifiedProperties();yield return null;yield return null;
            Assert.That(aura.Visual.EnergyMaterial.GetVector("_InnerFlow").y,Is.EqualTo(1.5f));
            foreach(var r in aura.Visual.GetComponentsInChildren<Renderer>())r.enabled=r.name=="MainAura"||r.name=="InkRuptures";
            Phase();FieldMode(2);Capture("density/high");var difference=Difference("density/low","density/high",new RectInt(420,200,120,250));
            Assert.That(difference.changed,Is.GreaterThan(0));Assert.That(difference.meanGreenDelta,Is.GreaterThan(0));FieldMode(0);
            Record("density-update",new{low=.5f,high=1.5f,changed=difference.changed,greenDelta=difference.meanGreenDelta});
        }
    }
}
