using System;
using System.Collections;
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
    public sealed class ShibuyaPass4B0Tests
    {
        private ShibuyaFighter p,d;
        private ShibuyaPlayerInput input;
        private ShibuyaInputFrame frame;
        private CursedEnergyAuraController aura;
        private CursedEnergyAuraProfile settings;
        private static string Root=>ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass4B0QA");
        [UnitySetUp] public IEnumerator Setup()
        {
            frame=default;EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);yield return new EnterPlayMode();yield return null;
            var m=Object.FindFirstObjectByType<ShibuyaMatch>();p=m.Player;d=m.Dummy;
            input=p.GetComponent<ShibuyaPlayerInput>();input.TestInput=()=>frame;aura=input.Aura;
            Assert.That(aura,Is.Not.Null);
            settings=Object.Instantiate(aura.Profile);settings.name="PASS4B0 QA Profile";aura.Configure(settings);
            Place(p,new Vector3(0,.03f,-1.4f));Place(d,new Vector3(0,.03f,0));p.transform.rotation=Quaternion.identity;
            yield return Wait(.1f);
        }
        [UnityTearDown] public IEnumerator Leave(){if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        [TearDown] public void Cleanup(){foreach(var a in Resources.FindObjectsOfTypeAll<ScriptableObject>())if(a.name.StartsWith("PASS4B0 QA",StringComparison.Ordinal)&&!AssetDatabase.Contains(a))Object.DestroyImmediate(a);}
        [UnityTest] public IEnumerator VisualActivationLoopViewsAndAlternatePalettes()
        {
            var orbit=Object.FindFirstObjectByType<ShibuyaOrbitCamera>();var camera=orbit.GetComponent<Camera>();orbit.enabled=false;
            Place(d,new Vector3(8,.03f,8));Pose(camera,new Vector3(0,2.2f,4.5f));Capture(camera,"00-no-F");
            frame.auraHeld=true;yield return Wait(.065f);Assert.That(aura.Envelope,Is.GreaterThan(0));Capture(camera,"01-activation");
            yield return Wait(1.2f);Assert.That(aura.Envelope,Is.EqualTo(1));Assert.That(aura.Visual.HasVisibleRenderers);Assert.That(aura.Visual.RimCount,Is.GreaterThan(4));
            Capture(camera,"02-front-loop");
            Pose(camera,new Vector3(4.5f,2.2f,0));yield return Wait(.15f);Capture(camera,"03-side");
            Pose(camera,new Vector3(0,2.2f,-4.5f));yield return Wait(.15f);Capture(camera,"04-back");
            Pose(camera,new Vector3(2.1f,5.2f,2.1f));yield return Wait(.15f);Capture(camera,"05-high-angle");
            Pose(camera,new Vector3(-4.5f,2.2f,0));yield return Wait(.15f);Capture(camera,"05b-left");
            Pose(camera,new Vector3(3.18f,2.2f,3.18f));yield return Wait(.15f);Capture(camera,"05c-diagonal");
            Pose(camera,new Vector3(0,1.8f,2.5f));yield return Wait(.15f);Capture(camera,"05d-close");
            var pink=Object.Instantiate(settings);pink.name="PASS4B0 QA Pink";pink.mainColor=new Color(1,.035f,.36f);aura.Configure(pink);
            Pose(camera,new Vector3(0,2.2f,4.5f));yield return Wait(.3f);Capture(camera,"06-pink");
            Assert.That(aura.Visual.EnergyMaterial.GetColor("_InkColor"),Is.EqualTo(settings.inkColor));
            var green=Object.Instantiate(settings);green.name="PASS4B0 QA Green";green.mainColor=new Color(.08f,1,.24f);aura.Configure(green);
            yield return Wait(.3f);Capture(camera,"07-green");
            frame.auraHeld=false;yield return Wait(settings.fadeOut+.1f);Capture(camera,"08-released");
            Assert.That(aura.Envelope,Is.Zero);Assert.That(aura.Visual.HasVisibleRenderers,Is.False);Assert.That(aura.Visual.ParticleCount,Is.Zero);
        }
        [UnityTest] public IEnumerator RepeatAndTwelveSecondHoldKeepResourcesBounded()
        {
            frame.auraHeld=true;yield return Wait(.2f);var first=aura.Visual;
            foreach(var filter in first.GetComponentsInChildren<MeshFilter>())
                foreach(var vertex in filter.sharedMesh.vertices)
                    Assert.That(float.IsNaN(vertex.x)||float.IsNaN(vertex.y)||float.IsNaN(vertex.z)||float.IsInfinity(vertex.sqrMagnitude),Is.False,"non-finite aura geometry");
            int objects=p.GetComponentsInChildren<Transform>(true).Length,materials=OwnedMaterials(),meshes=OwnedMeshes();
            for(int i=0;i<30;i++){frame.auraHeld=false;yield return Wait(.025f);frame.auraHeld=true;yield return Wait(.025f);}
            yield return Wait(12);
            Assert.That(aura.Visual,Is.SameAs(first));Assert.That(p.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(objects));
            Assert.That(OwnedMaterials(),Is.EqualTo(materials));Assert.That(OwnedMeshes(),Is.EqualTo(meshes));
            Assert.That(first.OwnedMaterialCount,Is.EqualTo(4));Assert.That(first.OwnedMeshCount,Is.EqualTo(3));Assert.That(first.ParticleCount,Is.LessThanOrEqualTo(64));
            Record("resource-counts",new{cycles=30,holdSeconds=12,objects,materials,meshes,particles=first.ParticleCount,vertices=first.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.vertexCount)});
            frame.auraHeld=false;yield return Wait(.4f);Assert.That(first.ParticleCount,Is.Zero);Assert.That(first.HasVisibleRenderers,Is.False);
        }
        [UnityTest] public IEnumerator OrbitAndTemporalFlameEvidence()
        {
            Place(d,new Vector3(8,.03f,8));
            var orbit=Object.FindFirstObjectByType<ShibuyaOrbitCamera>();var camera=orbit.GetComponent<Camera>();orbit.enabled=false;
            frame.auraHeld=true;yield return Wait(.5f);
            var filters=aura.Visual.GetComponentsInChildren<MeshFilter>();
            var main=filters.Single(f=>f.name=="MainAura").sharedMesh;
            var ink=filters.Single(f=>f.name=="InkRuptures").sharedMesh;
            Assert.That(ink.vertices,Is.EqualTo(main.vertices),"Ink and energy must share the same volume");
            Assert.That(ink.uv2,Is.EqualTo(main.uv2),"Ink must not run a different lifetime phase");
            var samples=new System.Collections.Generic.List<object>();
            double start=Time.unscaledTimeAsDouble;int index=0;
            while(Time.unscaledTimeAsDouble-start<6) {
                float elapsed=(float)(Time.unscaledTimeAsDouble-start);
                float angle=elapsed<2?90:90+(elapsed-2)*90;
                float radians=angle*Mathf.Deg2Rad;
                Pose(camera,new Vector3(Mathf.Sin(radians)*4.5f,2.2f+Mathf.Max(0,elapsed-4)*.6f,Mathf.Cos(radians)*4.5f));
                string name="motion/frame-"+index.ToString("D4");
                Directory.CreateDirectory(Path.Combine(Root,"motion"));Capture(camera,name);
                samples.Add(new{index,time=Time.unscaledTimeAsDouble-start,angle});index++;
                yield return Wait(1f/30);
            }
            Record("motion-timestamps",samples);
            Assert.That(index,Is.GreaterThan(12));
        }
        [UnityTest] public IEnumerator MovingSurfaceAuraEvidence()
        {
            Place(d,new Vector3(8,.03f,8));frame.auraHeld=true;
            var orbit=Object.FindFirstObjectByType<ShibuyaOrbitCamera>();var camera=orbit.GetComponent<Camera>();orbit.enabled=false;
            var samples=new System.Collections.Generic.List<object>();double start=Time.unscaledTimeAsDouble;int index=0;
            Directory.CreateDirectory(Path.Combine(Root,"moving"));
            while(Time.unscaledTimeAsDouble-start<3) {
                float elapsed=(float)(Time.unscaledTimeAsDouble-start);frame.move=elapsed<1.5f?Vector2.up:Vector2.right;
                Pose(camera,new Vector3(3,2.2f,3));Capture(camera,"moving/frame-"+index.ToString("D4"));
                samples.Add(new{index,time=Time.unscaledTimeAsDouble-start,x=p.transform.position.x,z=p.transform.position.z,
                    fieldFits=aura.Visual==null?0:aura.Visual.FieldFitCount,fieldFitMs=aura.Visual==null?0:aura.Visual.FieldFitMilliseconds});index++;
                yield return Wait(1f/30);
            }
            frame.move=Vector2.zero;Record("moving-timestamps",samples);
        }
        [UnityTest] public IEnumerator SurfaceRootsFollowAnimatedPartWithoutScaleDetachment()
        {
            frame.auraHeld=true;yield return Wait(.3f);var visual=aura.Visual;
            int index=Enumerable.Range(0,visual.BodyAnchorCount).First(i=>visual.SurfacePoints[i].Region==CursedAuraBodyRegion.Arm);
            var point=visual.SurfacePoints[index];var source=point.Source;
            Vector3 before=visual.BodyRootWorldPosition(index),old=source.transform.localPosition;
            source.transform.localPosition+=Vector3.right*.2f;yield return null;yield return null;
            Vector3 after=visual.BodyRootWorldPosition(index);
            Assert.That(Vector3.Distance(after,before),Is.EqualTo(.2f).Within(.005f));
            source.transform.localPosition=old;yield return null;yield return null;
            before=visual.BodyRootWorldPosition(index);
            var scaled=Object.Instantiate(settings);scaled.name="PASS4B0 QA Scaled Surface";
            scaled.scale=1.5f;scaled.width*=1.4f;aura.Configure(scaled);yield return null;yield return null;
            Assert.That(visual.EnergyMaterial.GetVector("_Shape").x,Is.EqualTo(scaled.width/.75f).Within(.001f));
            Assert.That(Vector3.Distance(before,visual.BodyRootWorldPosition(index)),Is.LessThan(.005f));
            Record("surface-follow",new{index,source=source.name,partMotion=Vector3.Distance(after,before),rootError=Vector3.Distance(before,visual.BodyRootWorldPosition(index))});
        }
        [UnityTest] public IEnumerator ContinuousFieldTopologyHeightAndLayerEvidence()
        {
            Place(d,new Vector3(8,.03f,8));frame.auraHeld=true;yield return Wait(.4f);var visual=aura.Visual;
            var mesh=visual.transform.Find("MainAura").GetComponent<MeshFilter>().sharedMesh;
            Assert.That(visual.MainFlowCount,Is.Zero,"main must not be emitted from surface roots");
            Assert.That(mesh.uv3.All(x=>x.y==2),Is.True);
            var adjacency=new System.Collections.Generic.List<int>[mesh.vertexCount];
            for(int i=0;i<adjacency.Length;i++)adjacency[i]=new System.Collections.Generic.List<int>();
            int[] indices=mesh.triangles;
            for(int i=0;i<indices.Length;i+=3)for(int j=0;j<3;j++) {
                int a=indices[i+j],b=indices[i+(j+1)%3];adjacency[a].Add(b);adjacency[b].Add(a);
            }
            var seen=new bool[mesh.vertexCount];var stack=new System.Collections.Generic.Stack<int>();stack.Push(0);int connected=0;
            while(stack.Count>0){int at=stack.Pop();if(seen[at])continue;seen[at]=true;connected++;foreach(int n in adjacency[at])if(!seen[n])stack.Push(n);}
            Assert.That(connected,Is.EqualTo(mesh.vertexCount),"field split into separate flames");
            var vertices=mesh.vertices.Select(visual.transform.TransformPoint).ToArray();
            Assert.That(vertices.Min(v=>v.y-p.transform.position.y),Is.LessThan(.12f));
            Assert.That(vertices.Max(v=>v.y-p.transform.position.y),Is.GreaterThan(1.9f));
            foreach(var v in vertices)Assert.That(float.IsNaN(v.sqrMagnitude)||float.IsInfinity(v.sqrMagnitude),Is.False);
            int fit=visual.FieldFitCount;yield return Wait(.2f);Assert.That(visual.FieldFitCount,Is.EqualTo(fit),"stationary field refitted unnecessarily");
            var orbit=Object.FindFirstObjectByType<ShibuyaOrbitCamera>();var camera=orbit.GetComponent<Camera>();orbit.enabled=false;Pose(camera,new Vector3(3,2.2f,3));
            Capture(camera,"10-combined");settings.wisps.enabled=false;settings.bodyRim.enabled=false;settings.motes.enabled=false;
            yield return Wait(.1f);Capture(camera,"11-main-field-only");
            settings.mainAura.enabled=false;settings.wisps.enabled=true;
            yield return Wait(.1f);Capture(camera,"12-detail-only");
            Record("continuous-field",new{vertices=mesh.vertexCount,connected,fitCount=visual.FieldFitCount,fitMilliseconds=visual.FieldFitMilliseconds,
                minHeight=vertices.Min(v=>v.y-p.transform.position.y),maxHeight=vertices.Max(v=>v.y-p.transform.position.y),detailCount=visual.BodyAnchorCount});
        }
        [UnityTest] public IEnumerator FullBodyMeshSurfaceDistributionAndDeterminism()
        {
            frame.auraHeld=true;yield return Wait(.3f);var visual=aura.Visual;
            Assert.That(visual.BodyAnchorCount,Is.EqualTo(settings.detailFlowCount));
            var points=visual.SurfacePoints;Assert.That(points.Select(x=>x.Source).Distinct().Count(),Is.EqualTo(6));
            foreach(var region in new[]{CursedAuraBodyRegion.Head,CursedAuraBodyRegion.Trunk,CursedAuraBodyRegion.Arm,CursedAuraBodyRegion.Leg})
                Assert.That(points.Any(x=>x.Region==region),region.ToString());
            foreach(var parent in new[]{"LeftArm","RightArm","LeftLeg","RightLeg"})
                Assert.That(points.Any(x=>x.Source.transform.parent.name==parent),parent);
            foreach(var point in points) {
                Assert.That(point.UsesMeshSurface,Is.True);Assert.That(point.Source.name,Is.Not.EqualTo("FaceDirection"));
                var mesh=point.Source.GetComponent<MeshFilter>().sharedMesh;var indices=mesh.triangles;var vertices=mesh.vertices;int tri=point.Triangle*3;
                Vector3 exact=vertices[indices[tri]]*point.Barycentric.x+vertices[indices[tri+1]]*point.Barycentric.y+vertices[indices[tri+2]]*point.Barycentric.z;
                Assert.That(Vector3.Distance(exact,point.LocalPosition),Is.LessThan(.00001f));
            }
            for(int i=0;i<points.Count;i++)
                Assert.That(Vector3.Distance(points[i].WorldPosition,visual.BodyRootWorldPosition(i)),Is.EqualTo(settings.surfaceOffset).Within(.001f));
            foreach(var axis in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right})
                Assert.That(points.Any(x=>Vector3.Dot(x.WorldNormal,axis)>.7f),"missing normal direction "+axis);
            Assert.That(points.Where(x=>x.Region==CursedAuraBodyRegion.Leg).Min(x=>x.WorldPosition.y-p.transform.position.y),Is.LessThan(.2f));
            Assert.That(points.Where(x=>x.Region==CursedAuraBodyRegion.Head).Max(x=>x.WorldPosition.y-p.transform.position.y),Is.GreaterThan(1.5f));
            var positions=points.Select(x=>x.LocalPosition).ToArray();
            Record("surface-distribution",points.Select((x,i)=>new{index=i,detail=true,source=x.Source.transform.parent.name+"/"+x.Source.name,
                region=x.Region.ToString(),mesh=x.UsesMeshSurface,triangle=x.Triangle,
                x=x.WorldPosition.x,y=x.WorldPosition.y-p.transform.position.y,z=x.WorldPosition.z,
                nx=x.WorldNormal.x,ny=x.WorldNormal.y,nz=x.WorldNormal.z}).ToArray());
            // Rebuild a different count and restore: stable sampling without touching Unity Random.
            var changed=Object.Instantiate(settings);changed.name="PASS4B0 QA Rebuild Surface";changed.detailFlowCount=28;
            aura.Configure(changed);yield return null;yield return null;aura.Configure(settings);yield return null;yield return null;
            Assert.That(visual.SurfacePoints.Select(x=>x.LocalPosition).ToArray(),Is.EqualTo(positions));
            Assert.That(visual.OwnedMeshCount,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator MovementRotationAndJumpFollowWithoutBuffs()
        {
            Place(d,new Vector3(8,.03f,8)); // Measure free locomotion, not running into the dummy capsule.
            frame.auraHeld=true;frame.move=Vector2.up;yield return Wait(.3f);var v=aura.Visual;
            Vector3 before=p.transform.position;frame.run=true;yield return Wait(.2f);Assert.That(Vector3.Distance(before,p.transform.position),Is.GreaterThan(.2f));
            Assert.That(v.transform.parent,Is.EqualTo(p.transform));Assert.That(v.transform.localPosition,Is.EqualTo(settings.offset));
            frame.move=Vector2.zero;frame.jump=true;yield return null;frame.jump=false;yield return Wait(.16f);
            Assert.That(p.transform.position.y,Is.GreaterThan(.3f));Assert.That(v.transform.position.y,Is.EqualTo(p.transform.position.y).Within(.001f));
            p.transform.rotation=Quaternion.Euler(0,120,0);yield return null;
            Assert.That(Quaternion.Angle(v.transform.rotation,p.transform.rotation),Is.LessThan(.001f));
            Assert.That(p.Health.CurrentHealth,Is.EqualTo(100));Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator GroundComboWithFHeldPreservesEightyFiveDamage()
        {
            frame.auraHeld=true;yield return PressAttack();
            for(int i=1;i<=3;i++){yield return Until(()=>p.ComboStep==i&&p.AttackElapsed>=p.AttackDuration-.12f);yield return PressAttack();yield return Until(()=>p.ComboStep==i+1);}
            yield return Until(()=>p.ConfirmedHitCount==4);
            Assert.That(d.Health.CurrentHealth,Is.EqualTo(915));Assert.That(aura.Held);Assert.That(aura.Envelope,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator FHeldLockSelectionAndHighestAngleCapture()
        {
            var orbit=Object.FindFirstObjectByType<ShibuyaOrbitCamera>();var camera=orbit.GetComponent<Camera>();orbit.enabled=false;
            camera.transform.position=p.transform.position+new Vector3(0,2.2f,-4.5f);
            camera.transform.LookAt(d.transform.position+Vector3.up);frame.auraHeld=true;frame.lockHeld=true;
            yield return Wait(.12f);frame.lockHeld=false;yield return Wait(.12f);
            Assert.That(input.LockOn.LockedTarget,Is.SameAs(d));Assert.That(aura.Envelope,Is.EqualTo(1));
            p.transform.rotation=Quaternion.Euler(0,180,0);camera.transform.rotation=Quaternion.Euler(0,180,0);
            yield return PressAttack();yield return Until(()=>p.ConfirmedHitCount==1);
            Assert.That(d.Health.CurrentHealth,Is.EqualTo(980));Assert.That(aura.Held);
            Assert.That(input.LockOn.LockedTarget,Is.SameAs(d));yield return Wait(.3f);
            Pose(camera,new Vector3(1.05f,5.23f,1.05f));yield return Wait(.1f);Capture(camera,"09-highest-70deg");
            Assert.That(Vector3.Angle(camera.transform.forward,Vector3.down),Is.EqualTo(20).Within(.2f));
        }
        [UnityTest] public IEnumerator UpperPursuitAndAirFiveHitsWorkWithFHeld()
        {
            frame.auraHeld=true;frame.forwardKey=true;yield return PressAttack();
            for(int i=1;i<=3;i++){yield return Until(()=>p.ComboStep==i&&p.AttackElapsed>=p.AttackDuration-.12f);yield return PressAttack();yield return Until(()=>p.ComboStep==i+1);}
            yield return Until(()=>p.ConfirmedHitCount==4);frame.jump=true;yield return null;frame.jump=false;yield return Until(()=>p.CanAirAttack);
            frame.forwardKey=false;float hp=d.Health.CurrentHealth;int first=p.AcceptedAttackCount;yield return PressAttack();
            for(int i=0;i<2;i++) {yield return Until(()=>p.AcceptedAttackCount==first+i+1&&p.AttackElapsed>=p.AttackDuration-p.AerialDefinition.aerialLinkInputWindow+.015f);yield return PressAttack();yield return Until(()=>p.AcceptedAttackCount==first+i+2);}
            yield return Until(()=>!p.IsAerialHovering);
            Assert.That(p.ConfirmedHitCount,Is.EqualTo(9));Assert.That(hp-d.Health.CurrentHealth,Is.EqualTo(62));
            Assert.That(p.AirChaseCount,Is.EqualTo(1));Assert.That(aura.Held);Assert.That(aura.Visual.HasVisibleRenderers);
        }
        [UnityTest] public IEnumerator HitSuppressionResumesHeldAuraAndDeathDisableClear()
        {
            frame.auraHeld=true;yield return Wait(.25f);p.ApplyHitReaction(Vector3.zero,.45f);
            yield return Wait(.28f);Assert.That(aura.Held);Assert.That(aura.Envelope,Is.Zero);Assert.That(aura.Visual.ParticleCount,Is.Zero);
            yield return Wait(.4f);Assert.That(aura.Envelope,Is.EqualTo(1));
            aura.enabled=false;Assert.That(aura.Envelope,Is.Zero);Assert.That(aura.Visual.HasVisibleRenderers,Is.False);
            aura.enabled=true;yield return Wait(.2f);p.Health.Kill();yield return null;yield return null;
            Assert.That(aura.Envelope,Is.Zero);Assert.That(aura.Visual.ParticleCount,Is.Zero);Assert.That(aura.Visual.HasVisibleRenderers,Is.False);
        }
        [UnityTest] public IEnumerator SeparateProfilesDoNotShareMaterialsAndDestroyReleasesOwnedObjects()
        {
            frame.auraHeld=true;yield return Wait(.2f);int beforeMaterials=OwnedMaterials(),beforeMeshes=OwnedMeshes();
            var pink=Object.Instantiate(settings);pink.name="PASS4B0 QA Other";pink.mainColor=new Color(1,.02f,.35f);
            var other=d.gameObject.AddComponent<CursedEnergyAuraController>();other.Configure(pink);other.SetHeld(true);yield return Wait(.2f);
            Assert.That(other.Visual.EnergyMaterial,Is.Not.SameAs(aura.Visual.EnergyMaterial));
            Assert.That(other.Visual.EnergyMaterial.GetColor("_MainColor"),Is.EqualTo(pink.mainColor));
            Assert.That(aura.Visual.EnergyMaterial.GetColor("_MainColor"),Is.EqualTo(settings.mainColor));
            Assert.That(other.Visual.EnergyMaterial.GetColor("_InkColor"),Is.EqualTo(settings.inkColor));
            Object.Destroy(other);yield return Wait(.1f);
            Assert.That(OwnedMaterials(),Is.EqualTo(beforeMaterials));Assert.That(OwnedMeshes(),Is.EqualTo(beforeMeshes));
        }
        [UnityTest] public IEnumerator ShaderKoreanFieldsUndoAndAssetReferencesAreValid()
        {
            foreach(var f in typeof(CursedEnergyAuraProfile).GetFields(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public))
                Assert.That(CursedEnergyAuraProfileInspector.Labels.ContainsKey(f.Name),f.Name);
            foreach(var path in new[]{"CursedEnergyAuraVolume","CursedEnergyAuraRim","CursedEnergyAuraMote"}) {
                var s=AssetDatabase.LoadAssetAtPath<Shader>(CursedEnergyAuraAssets.Root+"/"+path+".shader");
                Assert.That(s,Is.Not.Null);Assert.That(s.isSupported);Assert.That(ShaderUtil.ShaderHasError(s),Is.False,path);
            }
            var so=new SerializedObject(settings);float old=settings.intensity;
            Undo.IncrementCurrentGroup();so.FindProperty("intensity").floatValue=2.5f;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();
            Assert.That(settings.intensity,Is.EqualTo(2.5f));Undo.PerformUndo();Assert.That(settings.intensity,Is.EqualTo(old));
            Assert.That(settings.visualPrefab.GetComponent<CursedEnergyAuraVisual>(),Is.Not.Null);
            Assert.That(p.Profile.aerialCombat.aerialMaxHoverTime,Is.EqualTo(1.2f));yield return null;
        }
        private IEnumerator PressAttack(){frame.attack=true;yield return null;frame.attack=false;yield return null;}
        private void Pose(Camera c,Vector3 offset){c.transform.position=p.transform.position+offset;c.transform.LookAt(p.transform.position+Vector3.up*1.15f);}
        private static void Capture(Camera camera,string name)
        {
            Directory.CreateDirectory(Root);var rt=RenderTexture.GetTemporary(960,720,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var old=RenderTexture.active;var image=new Texture2D(960,720,TextureFormat.RGB24,false);
            try {
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.StandardRequest{destination=rt});
                RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Root,name+".png"),image.EncodeToPNG());
            } finally {RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);Object.Destroy(image);}
        }
        private static int OwnedMaterials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("CursedAura_Owned_",StringComparison.Ordinal));
        private static int OwnedMeshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("CursedAura_OwnedVolume_",StringComparison.Ordinal));
        private static void Record(string name,object value){Directory.CreateDirectory(Root);File.WriteAllText(Path.Combine(Root,name+".json"),Newtonsoft.Json.JsonConvert.SerializeObject(value,Newtonsoft.Json.Formatting.Indented));}
        private static void Place(ShibuyaFighter f,Vector3 at){var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=at;cc.enabled=true;Physics.SyncTransforms();}
        private static IEnumerator Wait(float seconds){double until=Time.unscaledTimeAsDouble+seconds;while(Time.unscaledTimeAsDouble<until)yield return null;}
        private static IEnumerator Until(Func<bool> ready){double until=Time.unscaledTimeAsDouble+6;while(!ready()&&Time.unscaledTimeAsDouble<until)yield return null;Assert.That(ready(),"PASS4B0 timed out");}
    }
}
