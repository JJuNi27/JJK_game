using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public abstract class ShibuyaPass4B3cFixture : ShibuyaPass4B3aFixture
    {
        private bool framePolicySaved;
        private int previousFrameRate;
        [UnityTearDown] public IEnumerator RestoreFramePolicy()
        {
            if(framePolicySaved)Application.targetFrameRate=previousFrameRate;
            framePolicySaved=false;yield break;
        }
        protected IEnumerator Use(float multiple,bool connected=false)
        {
            if(!framePolicySaved){previousFrameRate=Application.targetFrameRate;framePolicySaved=true;Application.targetFrameRate=60;}
            settings=Clone(AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(CursedEnergyAuraAssets.DefaultPath));
            if(connected)CursedEnergyAuraEndPreview.Candidate(settings);else CursedEnergyAuraSilhouettePreview.Candidate(settings);
            settings.maximumSizeMultiplier=4;
            settings.maxEnergySizeCurve=CursedEnergyAuraEndPreview.CurveAt200(settings,multiple);
            Capacity(200);aura.Configure(settings);frame.auraHeld=true;yield return Wait(1.6f);
            Assert.That(aura.MaximumEnergy,Is.EqualTo(200));Assert.That(aura.SizeMultiplier,Is.EqualTo(multiple).Within(.001f));
            var ps=aura.Visual.GetComponentInChildren<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);ps.useAutoRandomSeed=false;ps.randomSeed=38421;
        }
        protected void FixedPhase(float time=2)
        {
            var type=typeof(CursedEnergyAuraVisual);var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            type.GetField("age",flags).SetValue(aura.Visual,time);type.GetField("tipRootsReady",flags).SetValue(aura.Visual,false);
            type.GetMethod("LateUpdate",flags).Invoke(aura.Visual,null);Phase(time);
        }
        protected void EndViews(string folder)
        {
            FixedPhase();Views(folder);
            Pose(new Vector3(.02f,5.5f,.02f));Capture(folder+"/top");
            Pose(new Vector3(0,1.65f,2.6f));Capture(folder+"/close");
        }
        protected object EndMetric()
        {
            var v=Field.vertices;int stride=49,last=40*stride;
            return new{vertices=v.Length,triangles=Field.triangles.Length/3,minY=v.Min(p=>p.y),maxY=v.Max(p=>p.y),
                lowerWidth=v.Take(stride).Max(p=>p.x)-v.Take(stride).Min(p=>p.x),upperWidth=v.Skip(last).Max(p=>p.x)-v.Skip(last).Min(p=>p.x),sourceMinY=aura.Visual.FieldSourceBounds.min.y};
        }
        protected void UnderView(string folder)
        {
            FixedPhase();Pose(new Vector3(1.2f,-1.5f,2));Capture(folder+"/under");
        }
    }
    public sealed class ShibuyaPass4B3cBefore : ShibuyaPass4B3cFixture
    {
        [UnityTest] public IEnumerator OriginalOpenEndsEvidence()
        {
            var metrics=new List<object>();
            foreach(float multiple in new[]{2.7f,3f}) {
                yield return Use(multiple);EndViews("before-"+multiple.ToString("0.00"));metrics.Add(new{multiple,geometry=EndMetric()});
                // Lift the whole actor only for the underside camera; no production movement is changed.
                Place(player,new Vector3(0,3,-1.4f));UnderView("before-"+multiple.ToString("0.00"));
                Place(player,new Vector3(0,.03f,-1.4f));yield return null;
            }
            Record("before-end-geometry",metrics);
        }
    }
    public sealed class ShibuyaPass4B3cTests : ShibuyaPass4B3cFixture
    {
        [UnityTest] public IEnumerator FourTimesGeometryAndPreviewRestore()
        {
            yield return Use(4,true);var points=Field.vertices;var indices=Field.triangles;float smallest=float.MaxValue;int reversed=0;
            for(int i=0;i<indices.Length;i+=3) {
                Vector3 a=points[indices[i]],b=points[indices[i+1]],c=points[indices[i+2]],n=Vector3.Cross(b-a,c-a),center=(a+b+c)/3;
                smallest=Mathf.Min(smallest,n.magnitude*.5f);if(Vector3.Dot(n,new Vector3(center.x,0,center.z))<-.000001f)reversed++;
            }
            Assert.That(smallest,Is.GreaterThan(.00000001f));Assert.That(reversed,Is.Zero);
            var original=aura.Profile;CursedEnergyAuraEndPreview.Size400();yield return null;
            Assert.That(aura.Profile,Is.Not.SameAs(original));Assert.That(aura.Profile.upperFieldConnection,Is.EqualTo(1));Assert.That(aura.Profile.SizeMultiplier(200),Is.EqualTo(4));
            CursedEnergyAuraEndPreview.Restore();Assert.That(aura.Profile,Is.SameAs(original));Assert.That(aura.MaximumEnergy,Is.EqualTo(200));
            Record("four-geometry-preview",new{smallestArea=smallest,reversed,vertices=points.Length,triangles=indices.Length/3,previewRestored=true});
        }
        [UnityTest] public IEnumerator EndGeometryAndUnchangedSideEvidence()
        {
            yield return Use(3);var original=Field.vertices;var oldNormals=Field.normals;var indices=Field.triangles;float minY=original.Min(v=>v.y);
            yield return Use(3,true);var points=Field.vertices;float largestSideChange=0,largestNormalChange=0;
            for(int row=5;row<=31;row++)for(int side=0;side<49;side++) {
                int i=row*49+side;largestSideChange=Mathf.Max(largestSideChange,Vector3.Distance(original[i],points[i]));largestNormalChange=Mathf.Max(largestNormalChange,Vector3.Distance(oldNormals[i],Field.normals[i]));
            }
            Assert.That(largestSideChange,Is.EqualTo(0));Assert.That(largestNormalChange,Is.LessThan(.00001f));
            Assert.That(Field.triangles,Is.EqualTo(indices));Assert.That(points.Min(v=>v.y),Is.EqualTo(minY).Within(.00001f));
            float smallestArea=float.MaxValue;int reversed=0;
            for(int i=0;i<indices.Length;i+=3) {
                Vector3 a=points[indices[i]],b=points[indices[i+1]],c=points[indices[i+2]],n=Vector3.Cross(b-a,c-a),center=(a+b+c)/3;
                smallestArea=Mathf.Min(smallestArea,n.magnitude*.5f);
                if(Vector3.Dot(n,new Vector3(center.x,0,center.z))<-.000001f)reversed++;
            }
            Assert.That(smallestArea,Is.GreaterThan(.00000001f));Assert.That(reversed,Is.Zero);
            foreach(var v in points)Assert.That(float.IsNaN(v.x)||float.IsInfinity(v.x)||float.IsNaN(v.y)||float.IsInfinity(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.z),Is.False);
            Assert.That((points[0]-points[24]).magnitude,Is.LessThan(.04f));Assert.That((points[40*49]-points[40*49+24]).magnitude,Is.LessThan(.04f));
            EndViews("after-3.00");Place(player,new Vector3(0,3,-1.4f));UnderView("after-3.00");
            Record("connected-geometry",new{largestSideChange,largestNormalChange,smallestArea,reversed,ends=EndMetric(),materials=aura.Visual.OwnedMaterialCount,meshes=aura.Visual.OwnedMeshCount});
        }
        [UnityTest] public IEnumerator FourCurvesKeepHundredOneFiftyAndCurrentEnergyIndependent()
        {
            var source=AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(CursedEnergyAuraAssets.DefaultPath);
            foreach(float multiple in new[]{2.7f,3,3.5f,4}) {
                yield return Use(multiple,true);
                Assert.That(settings.SizeMultiplier(100),Is.EqualTo(source.SizeMultiplier(100)));
                Assert.That(settings.SizeMultiplier(150),Is.EqualTo(source.SizeMultiplier(150)));
                Assert.That(settings.SizeMultiplier(200),Is.EqualTo(multiple));
            }
            var energy=CursedEnergyController.GetOrCreate(player.gameObject);energy.ApplyProfile(reserve);energy.enabled=false;
            var original=Field.vertices;
            foreach(float current in new[]{200f,10,0,175}) {
                energy.SetCurrentEnergy(current);yield return Wait(.08f);Assert.That(aura.SizeMultiplier,Is.EqualTo(4));Assert.That(Field.vertices,Is.EqualTo(original));
            }
            settings.maxEnergySizeCurve=AnimationCurve.Constant(0,10,99);Assert.That(settings.SizeMultiplier(200),Is.EqualTo(4));
            // PASS4B.4a promotes the approved end connections; the saved size policy is unchanged.
            Assert.That(source.maximumSizeMultiplier,Is.EqualTo(3));Assert.That(source.upperFieldConnection,Is.EqualTo(1));Assert.That(source.lowerFieldConnection,Is.EqualTo(1));
            foreach(string key in new[]{"upperFieldConnection","lowerFieldConnection"})Assert.That(CursedEnergyAuraProfileInspector.Labels.ContainsKey(key),Is.True);
            var so=new SerializedObject(settings);Undo.IncrementCurrentGroup();so.FindProperty("upperFieldConnection").floatValue=.5f;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(settings.upperFieldConnection,Is.EqualTo(1));
            Record("curve-current-independence",new{actualMaximum=aura.MaximumEnergy,default100=source.SizeMultiplier(100),default150=source.SizeMultiplier(150),experiment200=new[]{2.7,3,3.5,4},currentEnergyIndependent=true});
        }
        [UnityTest] public IEnumerator FourSizesConnectedViewsOrbitsAndMovingFour()
        {
            var measures=new List<object>();
            foreach(float multiple in new[]{2.7f,3,3.5f,4}) {
                yield return Use(multiple,true);string folder="size-"+multiple.ToString("0.00");EndViews(folder);
                Measure(folder,multiple,measures);yield return RealOrbit(folder+"-orbit");
                if(multiple==2.7f) {Place(player,new Vector3(0,3,-1.4f));UnderView("after-2.70");Place(player,new Vector3(0,.03f,-1.4f));yield return null;EndViews("after-2.70");}
            }
            Record("screen-size-metrics",measures);yield return Motion("four-moving",3,true);
            frame.move=Vector2.zero;frame.jump=true;yield return Wait(.05f);frame.jump=false;yield return Motion("four-jump",.8f,false);
            frame.auraHeld=false;yield return Motion("four-release",.5f,false);Assert.That(aura.Visual.HasVisibleRenderers,Is.False);
        }
        private IEnumerator RealOrbit(string folder)
        {
            var samples=new List<object>();typeof(CursedEnergyAuraVisual).GetField("age",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(aura.Visual,2f);
            Capture(folder+"-warmup");yield return Wait(.3f);
            double start=Time.unscaledTimeAsDouble;int index=0;double last=0,maxGap=0;
            while(Time.unscaledTimeAsDouble-start<4) {
                double t=Time.unscaledTimeAsDouble-start;float a=(float)t*90*Mathf.Deg2Rad;Pose(new Vector3(Mathf.Sin(a)*4.5f,2.2f,Mathf.Cos(a)*4.5f));
                Capture(folder+"/frame-"+index.ToString("D4"));samples.Add(new{index,time=t});maxGap=Math.Max(maxGap,t-last);last=t;index++;yield return Wait(1f/30);
            }
            Pose(new Vector3(0,2.2f,4.5f));Capture(folder+"/frame-"+index.ToString("D4"));double end=Time.unscaledTimeAsDouble-start;samples.Add(new{index,time=end});maxGap=Math.Max(maxGap,end-last);
            Record(folder+"-timestamps",samples);Record(folder+"-coverage",new{degrees=360,duration=end,maxGap});Assert.That(maxGap,Is.LessThan(.25f));
        }
        private IEnumerator Motion(string folder,float duration,bool move)
        {
            var times=new List<object>();double start=Time.unscaledTimeAsDouble;int index=0;
            while(Time.unscaledTimeAsDouble-start<duration) {
                float t=(float)(Time.unscaledTimeAsDouble-start);if(move)frame.move=t<duration*.5f?Vector2.up:Vector2.right;
                Pose(new Vector3(3.18f,2.2f,3.18f));Capture(folder+"/frame-"+index.ToString("D4"));
                times.Add(new{index,time=Time.unscaledTimeAsDouble-start,playerY=player.transform.position.y,fieldMinY=Field.vertices.Min(p=>p.y),fitMs=aura.Visual.FieldFitMilliseconds});index++;yield return Wait(1f/30);
            }
            Record(folder+"-timestamps",times);
        }
        private void Measure(string folder,float multiple,List<object> result)
        {
            Pose(new Vector3(0,2.2f,4.5f));var renderers=aura.Visual.GetComponentsInChildren<Renderer>();var enabled=renderers.Select(r=>r.enabled).ToArray();
            foreach(var r in renderers)r.enabled=false;Capture("measure/body");var body=new Texture2D(2,2);body.LoadImage(File.ReadAllBytes(Path.Combine(Root,"measure/body.png")));var background=body.GetPixels32();
            var projected=aura.Visual.SurfacePoints.Select(p=>p.Source).Distinct().SelectMany(r=>r.GetComponent<MeshFilter>().sharedMesh.vertices.Select(v=>camera.WorldToViewportPoint(r.transform.TransformPoint(v)))).ToArray();
            float bodyWidth=(projected.Max(v=>v.x)-projected.Min(v=>v.x))*960,bodyHeight=(projected.Max(v=>v.y)-projected.Min(v=>v.y))*720;
            try {
                foreach(var r in renderers)r.enabled=r.name=="MainAura"||r.name=="InkRuptures";
                foreach(float t in new[]{.8f,1.4f,2,2.6f,3.2f}) {
                    FixedPhase(t);string file="measure/"+folder+"-"+t.ToString("0.0");Capture(file);var texture=new Texture2D(2,2);
                    try {
                        texture.LoadImage(File.ReadAllBytes(Path.Combine(Root,file+".png")));var pixels=texture.GetPixels32();int xmin=960,xmax=0,ymin=720,ymax=0;
                        for(int y=0;y<720;y++)for(int x=0;x<960;x++) {int i=y*960+x;var p=pixels[i];var b=background[i];if(Math.Max(Math.Abs(p.r-b.r),Math.Max(Math.Abs(p.g-b.g),Math.Abs(p.b-b.b)))<=12)continue;xmin=Math.Min(xmin,x);xmax=Math.Max(xmax,x);ymin=Math.Min(ymin,y);ymax=Math.Max(ymax,y);}
                        Assert.That(xmax,Is.GreaterThan(xmin));result.Add(new{multiple,phase=t,width=xmax-xmin+1,height=ymax-ymin+1,widthRatio=(xmax-xmin+1)/bodyWidth,heightRatio=(ymax-ymin+1)/bodyHeight,clipped=xmin==0||xmax==959||ymin==0||ymax==719});
                    }finally{Object.Destroy(texture);}
                }
            }finally{Object.Destroy(body);for(int i=0;i<renderers.Length;i++)renderers[i].enabled=enabled[i];}
        }
        [UnityTest] public IEnumerator FourTimesJumpColorsAndResourceSafety()
        {
            yield return Use(4,true);var visual=aura.Visual;var cc=player.GetComponent<CharacterController>();float radius=cc.radius,height=cc.height;Vector3 scale=player.transform.localScale;float localBase=Field.vertices.Min(v=>v.y);
            frame.jump=true;yield return Wait(.05f);frame.jump=false;yield return Wait(.10f);
            Record("jump-observation",new{y=player.transform.position.y,delta=Time.deltaTime,unscaled=Time.unscaledDeltaTime,playerState=player.State.ToString()});
            Assert.That(player.transform.position.y,Is.GreaterThan(.2f));Assert.That(Field.vertices.Min(v=>v.y),Is.EqualTo(localBase).Within(.001f));Assert.That(visual.HasVisibleRenderers,Is.True);
            yield return Wait(1);Assert.That(cc.radius,Is.EqualTo(radius));Assert.That(cc.height,Is.EqualTo(height));Assert.That(player.transform.localScale,Is.EqualTo(scale));
            Assert.That(combat.strikes.Select(s=>s.damage),Is.EqualTo(new[]{20f,20,20,25}));
            foreach(string color in new[]{"Pink Aura","Green Aura"}) {
                var p=Clone(AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(CursedEnergyAuraAssets.Root+"/Presets/"+color+".asset"));CursedEnergyAuraEndPreview.Candidate(p);p.maximumSizeMultiplier=4;p.maxEnergySizeCurve=CursedEnergyAuraEndPreview.CurveAt200(p,4);settings=p;aura.Configure(p);yield return Wait(.1f);
                Assert.That(visual.EnergyMaterial.GetColor("_MainColor"),Is.EqualTo(p.mainColor));Assert.That(visual.EnergyMaterial.GetColor("_InkColor"),Is.EqualTo(p.inkColor));FixedPhase();EndViews("color-"+color.Replace(' ','-'));
            }
            var costs=new List<double>();for(int i=0;i<12;i++){frame.auraHeld=false;yield return Wait(.05f);frame.auraHeld=true;frame.move=i%2==0?Vector2.right:Vector2.left;yield return Wait(.08f);costs.Add(visual.FieldFitMilliseconds);Assert.That(visual.OwnedMaterialCount,Is.EqualTo(4));Assert.That(visual.OwnedMeshCount,Is.EqualTo(4));}
            Record("four-budget",new{materials=visual.OwnedMaterialCount,meshes=visual.OwnedMeshCount,fitMs=costs});frame.move=Vector2.zero;frame.auraHeld=false;yield return Wait(.4f);Assert.That(visual.HasVisibleRenderers,Is.False);Assert.That(visual.ParticleCount,Is.Zero);
            Object.Destroy(visual.gameObject);yield return Wait(.10f);
            Record("cleanup-observation",new{frame=Time.frameCount,materials=Resources.FindObjectsOfTypeAll<Material>().Count(x=>x.name.StartsWith("CursedAura_Owned_")),meshes=Resources.FindObjectsOfTypeAll<Mesh>().Count(x=>x.name.StartsWith("CursedAura_OwnedVolume_"))});
            Assert.That(Resources.FindObjectsOfTypeAll<Material>().Count(x=>x.name.StartsWith("CursedAura_Owned_")),Is.Zero);Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Count(x=>x.name.StartsWith("CursedAura_OwnedVolume_")),Is.Zero);
        }
    }
    public sealed class ShibuyaPass4B3cRendering : ShibuyaPass4B3cFixture
    {
        [UnityTest] public IEnumerator ConnectedEndFragmentOverdrawEvidence()
        {
            // Reuse the exact current vertex/discard logic in a temporary, unsaved QA shader.
            // Every surviving fragment contributes white at alpha .1: C=1-.9^N.
            string source=File.ReadAllText(CursedEnergyAuraAssets.Root+"/CursedEnergyAuraVolume.shader")
                .Replace("Shader \"JJK/Shibuya/Cursed Energy Volume\"","Shader \"Hidden/JJK/PASS4B3c Overdraw\"")
                .Replace("return half4(_InkColor.rgb,alpha);","return half4(1,1,1,.1);")
                .Replace("return half4(color,alpha);","return half4(1,1,1,.1);");
            Shader debug=ShaderUtil.CreateShaderAsset(source,true);var records=new List<object>();
            try {
                Assert.That(ShaderUtil.ShaderHasError(debug),Is.False);
                foreach(var item in new[]{(3f,false),(2.7f,true),(3f,true),(3.5f,true),(4f,true)}) {
                    yield return Use(item.Item1,item.Item2);Pose(new Vector3(3.18f,2.2f,3.18f));
                    var rs=aura.Visual.GetComponentsInChildren<MeshRenderer>().Where(x=>x.name=="MainAura"||x.name=="InkRuptures"||x.name=="EnergyWisps"||x.name=="DetachedTips"||x.name=="DetachedTipInk").ToArray();
                    var previousMaterials=rs.Select(x=>x.sharedMaterial).ToArray();var previousLayers=rs.Select(x=>x.gameObject.layer).ToArray();
                    var materials=new Dictionary<Material,Material>();int previousMask=camera.cullingMask;var clear=camera.clearFlags;var background=camera.backgroundColor;
                    var additional=camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();bool post=additional.renderPostProcessing;
                    try {
                        foreach(var original in previousMaterials.Distinct()) {
                            var m=new Material(original){shader=debug};
                            foreach(string key in new[]{"_BodyRoots","_BodyNormals","_TipRoots"}) {var array=original.GetVectorArray(key);if(array!=null&&array.Length>0)m.SetVectorArray(key,array);}
                            materials.Add(original,m);
                        }
                        for(int i=0;i<rs.Length;i++){rs[i].sharedMaterial=materials[previousMaterials[i]];rs[i].gameObject.layer=31;}
                        camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;additional.renderPostProcessing=false;
                        foreach(float phase in new[]{.8f,1.4f,2,2.6f,3.2f}) {
                            Phase(phase);var rt=RenderTexture.GetTemporary(960,720,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);var previousRT=RenderTexture.active;
                            var image=new Texture2D(960,720,TextureFormat.RGBAFloat,false,true);
                            try {
                                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.StandardRequest{destination=rt});
                                RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();
                                double total=0,peak=0;int covered=0;
                                foreach(var pixel in image.GetPixels()) {
                                    if(pixel.r<.005f)continue;double count=Math.Log(Math.Max(.000001,1-Math.Min(.999999,pixel.r)))/Math.Log(.9);
                                    total+=count;peak=Math.Max(peak,count);covered++;
                                }
                                Assert.That(covered,Is.GreaterThan(0));Assert.That(double.IsNaN(peak)||double.IsInfinity(peak),Is.False);
                                records.Add(new{multiplier=item.Item1,candidate=item.Item2,phase,coveredPixels=covered,meanLayers=total/covered,peakLayers=peak,fragmentPixelSum=total});
                                string path=Path.Combine(Root,"overdraw/"+(item.Item2?"candidate":"baseline")+"-"+item.Item1.ToString("0.00")+"-"+phase.ToString("0.0")+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));
                                var preview=new Texture2D(960,720,TextureFormat.RGB24,false,true);
                                try{preview.SetPixels(image.GetPixels());preview.Apply();File.WriteAllBytes(path,preview.EncodeToPNG());}finally{Object.Destroy(preview);}
                            } finally {RenderTexture.active=previousRT;RenderTexture.ReleaseTemporary(rt);Object.Destroy(image);}
                        }
                    } finally {
                        for(int i=0;i<rs.Length;i++){rs[i].sharedMaterial=previousMaterials[i];rs[i].gameObject.layer=previousLayers[i];}
                        camera.cullingMask=previousMask;camera.clearFlags=clear;camera.backgroundColor=background;additional.renderPostProcessing=post;
                        foreach(var m in materials.Values)Object.Destroy(m);
                    }
                }
                Record("fragment-overdraw",new{method="Same deformation and alpha-discard; .1 white alpha per surviving fragment, linear float target; body/depth occlusion excluded conservatively; wrapper only, excludes rim/motes. Not GPU milliseconds.",samples=records});
            }finally {Object.Destroy(debug);}
        }
    }

}
