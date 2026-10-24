using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4B3bBefore : ShibuyaPass4B3aFixture
    {
        [UnityTest] public IEnumerator SurfaceDetailsBeforeAnyRenderingChange()
        {
            Capacity(100);frame.auraHeld=true;yield return Wait(.7f);
            var ps=aura.Visual.GetComponentInChildren<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);ps.useAutoRandomSeed=false;ps.randomSeed=38421;
            Views("A-original");LayerCapture("A-isolation");
            settings.wisps.enabled=false;yield return null;Views("A-details-off");
            Record("A-before",new{detailsDisabledOnly=true,mainEnabled=settings.mainAura.enabled,interior=settings.innerFlowVisibility,inkEnabled=settings.ink.enabled});
        }
    }
    public sealed class ShibuyaPass4B3bTests : ShibuyaPass4B3aFixture
    {
        private CursedEnergyAuraProfile baseline;
        private IEnumerator Use(float multiplier,bool candidate=false,Action<CursedEnergyAuraProfile> change=null)
        {
            if(baseline==null)baseline=Clone(settings);
            settings=Clone(baseline);settings.maxEnergySizeCurve=AnimationCurve.Constant(0,10,multiplier);
            if(candidate)CursedEnergyAuraSilhouettePreview.Candidate(settings);change?.Invoke(settings);
            aura.Configure(settings);frame.auraHeld=true;yield return Wait(1.6f);
            var ps=aura.Visual.GetComponentInChildren<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);ps.useAutoRandomSeed=false;ps.randomSeed=38421;
            Assert.That(aura.SizeMultiplier,Is.EqualTo(multiplier).Within(.001f));
        }
        private void SixViews(string folder)
        {
            Views(folder);Phase();Pose(new Vector3(0,1.65f,2.6f));Capture(folder+"/close");
        }
        [UnityTest] public IEnumerator BoundaryAreaAndDetachSeparateEvidence()
        {
            foreach(float edge in new[]{1f,1.15f,1.3f}) {
                yield return Use(1,false,p=>{p.wisps.enabled=false;p.inkBoundaryScale=edge;});SixViews("B-boundary-"+edge.ToString("0.00"));
            }
            yield return Use(1,false,p=>{p.wisps.enabled=false;p.detachedTips=false;});SixViews("C-off");
            yield return Use(1,false,p=>{p.wisps.enabled=false;p.detachedTips=true;});SixViews("C-on");
            yield return Clip("C-tip-close",2.5f,false);
            Record("B-C-controls",new{boundaries=new[]{1,1.15,1.3},area=settings.inkInteriorArea,meshes=aura.Visual.OwnedMeshCount,materials=aura.Visual.OwnedMaterialCount});
        }
        [UnityTest] public IEnumerator GrowthIsolationAndScreenSilhouettes()
        {
            var metrics=new List<object>();
            foreach(float multiple in new[]{1f,1.5f,1.7f,2,2.5f,2.7f,3}) {
                Capacity(multiple<1.2f?100:multiple<2?150:200);
                yield return Use(multiple,false,p=>p.wisps.enabled=false);
                SixViews("E-old-"+multiple.ToString("0.00"));
                Measure("old",multiple,metrics);
                yield return Use(multiple,true);
                SixViews("E-new-"+multiple.ToString("0.00"));
                Measure("candidate",multiple,metrics);
            }
            // D holds reach and ink constant: isolate local shape preservation at high growth.
            yield return Use(3,false,p=>p.wisps.enabled=false);SixViews("D-old");
            yield return Use(3,false,p=>{p.wisps.enabled=false;p.preserveLocalFlameShape=true;});SixViews("D-shape-only");
            Record("screen-silhouettes",metrics);
        }
        private void Measure(string variant,float multiplier,List<object> records)
        {
            Pose(new Vector3(0,2.2f,4.5f));
            var rs=aura.Visual.GetComponentsInChildren<Renderer>();var previous=rs.Select(r=>r.enabled).ToArray();
            for(int i=0;i<rs.Length;i++)rs[i].enabled=false;
            Capture("measure/body");
            var projected=aura.Visual.SurfacePoints.Select(p=>p.Source).Distinct().SelectMany(r=>r.GetComponent<MeshFilter>().sharedMesh.vertices.Select(v=>camera.WorldToViewportPoint(r.transform.TransformPoint(v)))).ToArray();
            var body=new RectInt(0,0,Mathf.CeilToInt((projected.Max(v=>v.x)-projected.Min(v=>v.x))*960),Mathf.CeilToInt((projected.Max(v=>v.y)-projected.Min(v=>v.y))*720));
            for(int i=0;i<rs.Length;i++)rs[i].enabled=previous[i];
            // Measure the continuous main field only. Stochastic details/tips cannot inflate it.
            foreach(var r in rs)r.enabled=r.name=="MainAura"||r.name=="InkRuptures";
            foreach(float time in new[]{.8f,1.4f,2f,2.6f,3.2f}) {
                Phase(time);string name="measure/"+variant+"-"+multiplier.ToString("0.00")+"-"+time.ToString("0.0");Capture(name);
                var pixels=BoundsOf(name,true);
                records.Add(new{variant,multiplier,phase=time,width=pixels.width,height=pixels.height,bodyWidth=body.width,bodyHeight=body.height,widthRatio=pixels.width/(float)body.width,heightRatio=pixels.height/(float)body.height,carrier=Metric(aura.MaximumEnergy)});
            }
            for(int i=0;i<rs.Length;i++)rs[i].enabled=previous[i];
        }
        private static RectInt BoundsOf(string name,bool auraOnly)
        {
            var t=new Texture2D(2,2);var body=new Texture2D(2,2);
            try {
                t.LoadImage(File.ReadAllBytes(Path.Combine(Root,name+".png")));
                if(auraOnly)body.LoadImage(File.ReadAllBytes(Path.Combine(Root,"measure/body.png")));
                var pixels=t.GetPixels32();var without=auraOnly?body.GetPixels32():null;
                int xmin=960,ymin=720,xmax=0,ymax=0;
                for(int y=150;y<640;y++)for(int x=200;x<760;x++) {
                    var p=pixels[y*960+x];bool visible;
                    if(auraOnly) {var b=without[y*960+x];visible=Math.Max(Math.Abs(p.r-b.r),Math.Max(Math.Abs(p.g-b.g),Math.Abs(p.b-b.b)))>12;}
                    else visible=p.b>p.r*1.25f&&p.b>p.g*1.05f&&p.b>65||p.r>140&&p.g>130&&p.b>110&&y>250&&y<500&&x>410&&x<550;
                    if(!visible)continue;xmin=Math.Min(xmin,x);xmax=Math.Max(xmax,x);ymin=Math.Min(ymin,y);ymax=Math.Max(ymax,y);
                }
                Assert.That(xmax,Is.GreaterThan(xmin));return new RectInt(xmin,ymin,xmax-xmin+1,ymax-ymin+1);
            }finally{Object.Destroy(t);Object.Destroy(body);}
        }
        [UnityTest] public IEnumerator IntegratedOriginalSpeedHighGrowthAndMovement()
        {
            foreach(float multiple in new[]{1f,1.7f,2.7f,3}) {
                yield return Use(multiple,true);yield return Orbit("integrated-"+multiple.ToString("0.00")+"-orbit");
            }
            yield return Clip("integrated-moving",3,true);
            frame.move=Vector2.zero;Place(dummy,player.transform.position+player.transform.forward*1.2f);
            frame.attack=true;yield return null;frame.attack=false;yield return Clip("integrated-attack",1.2f,false);
            frame.auraHeld=false;yield return Clip("integrated-release",.6f,false);
            Assert.That(aura.Visual.HasVisibleRenderers,Is.False);Assert.That(aura.Visual.ParticleCount,Is.Zero);
        }
        private IEnumerator Clip(string name,float seconds,bool move)
        {
            var samples=new List<object>();double start=Time.unscaledTimeAsDouble;int index=0;
            while(Time.unscaledTimeAsDouble-start<seconds) {
                float t=(float)(Time.unscaledTimeAsDouble-start);if(move)frame.move=t<seconds*.5f?Vector2.up:Vector2.right;
                Pose(name.StartsWith("C")?new Vector3(0,1.65f,2.6f):new Vector3(3,2.2f,3));Capture(name+"/frame-"+index.ToString("D4"));
                samples.Add(new{index,time=Time.unscaledTimeAsDouble-start,fitMs=aura.Visual.FieldFitMilliseconds});index++;yield return Wait(1f/30);
            }
            Record(name+"-timestamps",samples);
        }
        [UnityTest] public IEnumerator High270CompleteOrbit()
        {
            yield return Use(2.7f,true);var samples=new List<object>();double start=Time.unscaledTimeAsDouble;int index=0;double previous=0,maxGap=0;
            while(Time.unscaledTimeAsDouble-start<4) {
                double elapsed=Time.unscaledTimeAsDouble-start;float angle=(float)elapsed*90*Mathf.Deg2Rad;
                Pose(new Vector3(Mathf.Sin(angle)*4.5f,2.2f,Mathf.Cos(angle)*4.5f));Capture("complete-270-orbit/frame-"+index.ToString("D4"));
                samples.Add(new{index,time=elapsed});maxGap=Math.Max(maxGap,elapsed-previous);previous=elapsed;index++;yield return Wait(1f/30);
            }
            Pose(new Vector3(0,2.2f,4.5f));Capture("complete-270-orbit/frame-"+index.ToString("D4"));
            double end=Time.unscaledTimeAsDouble-start;samples.Add(new{index,time=end});maxGap=Math.Max(maxGap,end-previous);
            Record("complete-270-orbit-timestamps",samples);Record("complete-270-orbit-coverage",new{degrees=360,duration=end,maxGap});
            Assert.That(maxGap,Is.LessThan(.20),"A source gap must not conceal a large portion of the orbit");
        }
        [UnityTest] public IEnumerator MatchedIsolatedMotionAndCarrierBudget()
        {
            var cases=new[]{"A-full","A-off","B-15","B-30","C-on","D-old","D-shape","E-integrated-3"};
            var budget=new List<object>();
            foreach(string key in cases) {
                float multiple=key.StartsWith("D")||key.StartsWith("E")?3:1;
                yield return Use(multiple,key.StartsWith("E"),p=>{
                    p.wisps.enabled=key=="A-full";
                    if(key=="B-15")p.inkBoundaryScale=1.15f;if(key=="B-30")p.inkBoundaryScale=1.3f;
                    if(key=="C-on")p.detachedTips=true;if(key=="D-shape")p.preserveLocalFlameShape=true;
                });
                var times=new List<object>();double start=Time.unscaledTimeAsDouble;int index=0;
                while(Time.unscaledTimeAsDouble-start<2.5) {
                    float t=(float)(Time.unscaledTimeAsDouble-start);Pose(new Vector3(3.18f,2.2f,3.18f));Phase(2+t);
                    Capture("isolated-motion/"+key+"/frame-"+index.ToString("D4"));times.Add(new{index,time=Time.unscaledTimeAsDouble-start});index++;yield return Wait(1f/30);
                }
                Record("isolated-motion/"+key+"-timestamps",times);
                var mesh=Field;var vertices=mesh.vertices.Select(v=>camera.WorldToViewportPoint(aura.Visual.transform.TransformPoint(v))).ToArray();var tri=mesh.triangles;double area=0;
                for(int i=0;i<tri.Length;i+=3) {
                    Vector3 a=vertices[tri[i]],b=vertices[tri[i+1]],c=vertices[tri[i+2]];
                    area+=Math.Abs((b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x))*.5*960*720;
                }
                budget.Add(new{key,multiple,carrierTrianglePixelArea=area,carrierVertices=mesh.vertexCount,materials=aura.Visual.OwnedMaterialCount,meshes=aura.Visual.OwnedMeshCount});
            }
            Record("carrier-budget",new{samples=budget,limitation="Projected undeformed carrier triangle area, not measured GPU overdraw or GPU time"});
        }
        [UnityTest] public IEnumerator CandidateSafetyInspectorAndOwnedResourceCleanup()
        {
            yield return Use(3,true);var v=aura.Visual;
            Assert.That(v.OwnedMaterialCount,Is.EqualTo(4));Assert.That(v.OwnedMeshCount,Is.EqualTo(4));
            Assert.That(settings.innerFlowVisibility,Is.EqualTo(.65f));Assert.That(settings.innerFlowDensity,Is.EqualTo(1));
            var toggle=new SerializedObject(settings);toggle.FindProperty("mainAura").FindPropertyRelative("enabled").boolValue=false;toggle.ApplyModifiedProperties();yield return Wait(.05f);
            Assert.That(v.transform.Find("DetachedTips").GetComponent<Renderer>().enabled,Is.False);
            toggle.Update();toggle.FindProperty("mainAura").FindPropertyRelative("enabled").boolValue=true;toggle.ApplyModifiedProperties();yield return Wait(.05f);
            Assert.That(v.transform.Find("DetachedTips").GetComponent<Renderer>().enabled,Is.True);
            var cycles=(int[])typeof(CursedEnergyAuraVisual).GetField("tipCycles",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(v);
            var beforeCycles=cycles.ToArray();var roots=v.EnergyMaterial.GetVectorArray("_TipRoots").Select(x=>v.transform.TransformPoint((Vector3)x)).ToArray();
            frame.move=Vector2.right;yield return Wait(.05f);frame.move=Vector2.zero;
            var afterRoots=v.EnergyMaterial.GetVectorArray("_TipRoots");int stable=0;
            for(int i=0;i<roots.Length;i++)if(cycles[i]==beforeCycles[i]) {Assert.That(Vector3.Distance(roots[i],v.transform.TransformPoint((Vector3)afterRoots[i])),Is.LessThan(.001f));stable++;}
            Assert.That(stable,Is.GreaterThan(0));
            Assert.That(combat.strikes.Select(s=>s.damage),Is.EqualTo(new[]{20f,20,20,25}));
            foreach(string key in new[]{"preserveLocalFlameShape","extraFieldReach","inkBoundaryScale","inkInteriorArea","detachedTips","tipCycleRate"})Assert.That(CursedEnergyAuraProfileInspector.Labels.ContainsKey(key),Is.True,key);
            var so=new SerializedObject(settings);Undo.IncrementCurrentGroup();so.FindProperty("inkBoundaryScale").floatValue=1.15f;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(settings.inkBoundaryScale,Is.EqualTo(1.3f));
            var stats=new List<double>();for(int i=0;i<12;i++) {
                frame.auraHeld=false;yield return Wait(.05f);frame.auraHeld=true;frame.move=i%2==0?Vector2.right:Vector2.left;yield return Wait(.08f);stats.Add(v.FieldFitMilliseconds);
                Assert.That(v.OwnedMeshCount,Is.EqualTo(4));Assert.That(v.OwnedMaterialCount,Is.EqualTo(4));
            }
            Record("resource-budget",new{materials=v.OwnedMaterialCount,meshes=v.OwnedMeshCount,tipVertices=v.transform.Find("DetachedTips").GetComponent<MeshFilter>().sharedMesh.vertexCount,fitMs=stats});
            frame.move=Vector2.zero;frame.auraHeld=false;yield return Wait(.4f);Assert.That(v.HasVisibleRenderers,Is.False);
            Object.Destroy(v.gameObject);yield return null;yield return null;
            Assert.That(Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("CursedAura_Owned_")),Is.Zero);
            Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("CursedAura_OwnedVolume_")),Is.Zero);
        }
        [UnityTest] public IEnumerator FixedCameraFragmentOverdrawEvidence()
        {
            // Reuse the exact current vertex/discard logic in a temporary, unsaved QA shader.
            // Every surviving fragment contributes white at alpha .1: C=1-.9^N.
            string source=File.ReadAllText(CursedEnergyAuraAssets.Root+"/CursedEnergyAuraVolume.shader")
                .Replace("Shader \"JJK/Shibuya/Cursed Energy Volume\"","Shader \"Hidden/JJK/PASS4B3b Overdraw\"")
                .Replace("return half4(_InkColor.rgb,alpha);","return half4(1,1,1,.1);")
                .Replace("return half4(color,alpha);","return half4(1,1,1,.1);");
            Shader debug=ShaderUtil.CreateShaderAsset(source,true);var records=new List<object>();
            try {
                Assert.That(ShaderUtil.ShaderHasError(debug),Is.False);
                foreach(var item in new[]{(1f,false),(2f,false),(2.5f,false),(3f,false),(2.7f,true),(3f,true)}) {
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
