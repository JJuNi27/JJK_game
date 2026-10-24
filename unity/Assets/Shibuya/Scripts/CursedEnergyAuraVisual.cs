using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Shibuya
{
    [DefaultExecutionOrder(100)]
    public sealed class CursedEnergyAuraVisual : MonoBehaviour
    {
        [SerializeField] private Material energyTemplate,inkTemplate,rimTemplate,moteTemplate;
        private readonly List<Material> ownedMaterials=new List<Material>();
        private readonly List<Mesh> ownedMeshes=new List<Mesh>();
        private readonly List<Renderer> rims=new List<Renderer>();
        private MaterialPropertyBlock[] blocks;
        private MeshRenderer[] layers;
        private Material energy,ink,rim,mote;
        private ParticleSystem particles;
        private ShibuyaFighter owner;
        private CursedEnergyAuraProfile appliedProfile;
        private int revision=-1;
        private bool emitting;
        private float age;
        private Renderer[] bodySources;
        private List<CursedAuraSurfacePoint> surfacePoints=new List<CursedAuraSurfacePoint>();
        private readonly Vector4[] bodyRoots=new Vector4[CursedEnergyAuraSurface.Capacity];
        private readonly Vector4[] bodyNormals=new Vector4[CursedEnergyAuraSurface.Capacity];
        private CursedEnergyAuraField field;
        private MeshRenderer[] tips;
        private readonly Vector4[] tipRoots=new Vector4[CursedEnergyAuraGeometry.TipCount];
        private readonly Vector3[] tipBirthWorld=new Vector3[CursedEnergyAuraGeometry.TipCount];
        private readonly int[] tipCycles=new int[CursedEnergyAuraGeometry.TipCount];
        private bool tipRootsReady;
        private float expansion=1;
        public float Expansion => expansion;
        private int requestedDetail=-1;
        private float layoutLowerWeight=-1;
        public int BodyAnchorCount=>surfacePoints.Count;
        public int MainFlowCount=>0; // The continuous main field has no emitter roots.
        public int FieldFitCount=>field==null?0:field.FitCount;
        public double FieldFitMilliseconds=>field==null?0:field.LastFitMilliseconds;
        public Bounds FieldSourceBounds=>field==null?default:field.SourceBounds;
        public IReadOnlyList<CursedAuraSurfacePoint> SurfacePoints=>surfacePoints;
        public Vector3 BodyRootWorldPosition(int index)=>transform.TransformPoint((Vector3)bodyRoots[index]);
        public int OwnedMaterialCount=>ownedMaterials.Count;
        public int OwnedMeshCount=>ownedMeshes.Count;
        public int ParticleCount=>particles==null?0:particles.particleCount;
        public int RimCount=>rims.Count;
        public Material EnergyMaterial=>energy;
        public bool HasVisibleRenderers {
            get {if(layers!=null)foreach(var r in layers)if(r!=null&&r.enabled)return true;foreach(var r in rims)if(r!=null&&r.enabled)return true;return false;}
        }
        public void SetTemplates(Material e,Material i,Material r,Material m) {energyTemplate=e;inkTemplate=i;rimTemplate=r;moteTemplate=m;}
        private Material Own(Material source)
        {
            var m=new Material(source){name="CursedAura_Owned_"+source.name};ownedMaterials.Add(m);return m;
        }
        public void Bind(ShibuyaFighter fighter,Renderer[] requestedBodyRenderers=null)
        {
            if(owner!=null)return;owner=fighter;
            blocks=new[]{new MaterialPropertyBlock(),new MaterialPropertyBlock(),new MaterialPropertyBlock()};
            var sources=fighter.GetComponentsInChildren<Renderer>();
            bodySources=CursedEnergyAuraSurface.Collect(owner,requestedBodyRenderers,rimTemplate.shader);
            energy=Own(energyTemplate);ink=Own(inkTemplate);rim=Own(rimTemplate);mote=Own(moteTemplate);
            layers=new MeshRenderer[3];
            for(int i=0;i<3;i++) {
                var go=new GameObject(i==0?"MainAura":i==1?"InkRuptures":"EnergyWisps");go.transform.SetParent(transform,false);
                go.AddComponent<MeshFilter>();layers[i]=go.AddComponent<MeshRenderer>();
                layers[i].sharedMaterial=i==1?ink:energy;NoShadows(layers[i]);
            }
            var rimRoot=new GameObject("BodyRim");rimRoot.transform.SetParent(transform,false);
            foreach(var source in sources) {
                if(source==null || source.GetComponentInParent<CursedEnergyAuraVisual>()!=null)continue;
                if(source.sharedMaterial!=null&&source.sharedMaterial.shader==rimTemplate.shader)continue;
                var go=new GameObject("AuraRim_"+source.name);go.transform.SetParent(source.transform,false);
                Renderer target=null;
                if(source is SkinnedMeshRenderer skin) {
                    var r=go.AddComponent<SkinnedMeshRenderer>();r.sharedMesh=skin.sharedMesh;r.bones=skin.bones;r.rootBone=skin.rootBone;r.localBounds=skin.localBounds;target=r;
                } else if(source.GetComponent<MeshFilter>() is MeshFilter filter && filter.sharedMesh!=null) {
                    go.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;target=go.AddComponent<MeshRenderer>();
                }
                if(target==null){Destroy(go);continue;}
                var mats=new Material[target is SkinnedMeshRenderer sk?sk.sharedMesh.subMeshCount:go.GetComponent<MeshFilter>().sharedMesh.subMeshCount];
                for(int n=0;n<mats.Length;n++)mats[n]=rim;target.sharedMaterials=mats;
                NoShadows(target);rims.Add(target);
            }
            var micro=new GameObject("MicroParticles");micro.transform.SetParent(transform,false);
            particles=micro.AddComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.playOnAwake=false;main.loop=true;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            main.useUnscaledTime=true;main.maxParticles=64;main.startLifetime=new ParticleSystem.MinMaxCurve(.16f,.4f);
            main.startSpeed=new ParticleSystem.MinMaxCurve(.4f,1.4f);main.startSize=new ParticleSystem.MinMaxCurve(.018f,.045f);
            var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(1.1f,1.7f,.85f);shape.position=Vector3.up*.9f;
            var velocity=particles.velocityOverLifetime;
            velocity.x=new ParticleSystem.MinMaxCurve(0f,0f);velocity.y=new ParticleSystem.MinMaxCurve(.7f,1.8f);velocity.z=new ParticleSystem.MinMaxCurve(0f,0f);
            velocity.enabled=true;
            var color=particles.colorOverLifetime;color.enabled=true;
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
                new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.15f),new GradientAlphaKey(0,1)});color.color=gradient;
            var pr=micro.GetComponent<ParticleSystemRenderer>();pr.sharedMaterial=mote;NoShadows(pr);
            StopImmediate();
        }
        private static void NoShadows(Renderer r){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;}
        private void EnsureSurfaceLayout(CursedEnergyAuraProfile p)
        {
            int detail=Mathf.Clamp(p.detailFlowCount,1,32);
            if(requestedDetail==detail&&Mathf.Approximately(layoutLowerWeight,p.lowerBodyWeight))return;
            surfacePoints=CursedEnergyAuraSurface.Build(owner,bodySources,p);
            layoutLowerWeight=p.lowerBodyWeight;requestedDetail=detail;
            if(field==null) {
                field=new CursedEnergyAuraField(bodySources);
                for(int i=0;i<2;i++){var mesh=field.CreateMesh(i);ownedMeshes.Add(mesh);layers[i].GetComponent<MeshFilter>().sharedMesh=mesh;}
                ownedMeshes.Add(null);
            }
            if(ownedMeshes[2]!=null)Destroy(ownedMeshes[2]);
            ownedMeshes[2]=CursedEnergyAuraGeometry.Build(2,surfacePoints,0);layers[2].GetComponent<MeshFilter>().sharedMesh=ownedMeshes[2];
        }
        private void UpdateBodyRoots()
        {
            for(int i=0;i<surfacePoints.Count;i++) {
                var point=surfacePoints[i];if(point.Source==null)continue;
                Vector3 normal=point.WorldNormal;
                Vector3 at=point.WorldPosition+normal*appliedProfile.surfaceOffset;
                Vector3 local=transform.InverseTransformPoint(at);bodyRoots[i]=new Vector4(local.x,local.y,local.z,1);
                Vector3 direction=transform.InverseTransformDirection(normal);bodyNormals[i]=new Vector4(direction.x,direction.y,direction.z,0);
            }
            energy.SetVectorArray("_BodyRoots",bodyRoots);ink.SetVectorArray("_BodyRoots",bodyRoots);
            energy.SetVectorArray("_BodyNormals",bodyNormals);ink.SetVectorArray("_BodyNormals",bodyNormals);
        }
        // Resolve attachments after the block rig's LateUpdate pose, without owning that pose.
        private void LateUpdate()
        {
            if(owner==null||energy==null||!HasVisibleRenderers)return;
            UpdateBodyRoots();
            if(field!=null&&(layers[0].enabled||layers[1].enabled))field.Update(transform,ownedMeshes[0],ownedMeshes[1],appliedProfile,expansion);
            if(field!=null) {
                var ends=new Vector4(appliedProfile.upperFieldConnection,appliedProfile.lowerFieldConnection,field.SourceBounds.min.y+.001f,0);
                energy.SetVector("_EndConnection",ends);ink.SetVector("_EndConnection",ends);
            }
            if(tips!=null&&tips[0].enabled&&field.FitCount>0) {
                float time=age*appliedProfile.motionSpeed;
                for(int i=0;i<tipRoots.Length;i++) {
                    float seed=CursedEnergyAuraGeometry.TipSeed(i);
                    int cycle=Mathf.FloorToInt(time*appliedProfile.tipCycleRate*(.78f+seed*.54f)+CursedEnergyAuraGeometry.TipPhase(i));
                    if(!tipRootsReady||cycle!=tipCycles[i]) {
                        // Sample exactly at birth so the subsequent path separates from the field.
                        float birth=(cycle-CursedEnergyAuraGeometry.TipPhase(i))/(appliedProfile.tipCycleRate*(.78f+seed*.54f));
                        Vector3 at=field.Crest(i,cycle,birth,appliedProfile,expansion,out _);
                        tipBirthWorld[i]=transform.TransformPoint(at);tipCycles[i]=cycle;
                    }
                    // Detached pieces stay at their birth location as the fighter moves away.
                    Vector3 local=transform.InverseTransformPoint(tipBirthWorld[i]);tipRoots[i]=new Vector4(local.x,local.y,local.z,1);
                }
                tipRootsReady=true;energy.SetVectorArray("_TipRoots",tipRoots);ink.SetVectorArray("_TipRoots",tipRoots);
            }
        }
        private void EnsureTips()
        {
            if(tips!=null)return;
            var mesh=CursedEnergyAuraGeometry.BuildTips();ownedMeshes.Add(mesh);tips=new MeshRenderer[2];
            for(int i=0;i<2;i++) {
                var go=new GameObject(i==0?"DetachedTips":"DetachedTipInk");go.transform.SetParent(transform,false);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;tips[i]=go.AddComponent<MeshRenderer>();tips[i].sharedMaterial=i==0?energy:ink;NoShadows(tips[i]);
            }
        }
        private void Apply(CursedEnergyAuraProfile p)
        {
            foreach(var m in new[]{energy,ink}) {
                m.SetColor("_MainColor",p.mainColor);m.SetColor("_AccentColor",p.Accent);m.SetColor("_InkColor",p.inkColor);
                m.SetFloat("_Irregularity",p.irregularity);m.SetFloat("_MotionSpeed",p.motionSpeed);
                m.SetVector("_Shape",new Vector4(p.width/.75f,p.height/2.4f,0,0));
                m.SetVector("_FlowDirection",new Vector4(p.outwardFlow,p.upwardFlow,0,0));
                m.SetVector("_FieldParams",new Vector4(p.fieldThickness,p.fieldDensity,p.detailScale,0));
                m.SetVector("_InnerFlow",new Vector4(p.innerFlowVisibility,p.innerFlowDensity,0,0));
                m.SetVector("_FlameStyle",new Vector4(p.preserveLocalFlameShape?1:0,p.BaseExpansion,p.inkBoundaryScale,p.inkInteriorArea));
                m.SetFloat("_TipRate",p.tipCycleRate);
            }
            rim.SetColor("_Tint",p.Rim);mote.SetColor("_Tint",p.Particle*p.motes.intensity*p.intensity);
            var main=particles.main;main.startColor=new ParticleSystem.MinMaxGradient(new Color(.015f,.015f,.02f,1),Color.white);
            var emission=particles.emission;emission.rateOverTime=p.particleRate;
            appliedProfile=p;revision=p.Revision;
        }
        public void Sample(CursedEnergyAuraProfile p,float envelope,bool emit,float sizeMultiplier=1)
        {
            if(owner==null||p==null){StopImmediate();return;}
            if(appliedProfile!=p||revision!=p.Revision)Apply(p);
            expansion=p.BaseExpansion*sizeMultiplier;
            energy.SetFloat("_AuraExpansion",expansion);ink.SetFloat("_AuraExpansion",expansion);
            float vertical=p.VerticalExpansion(expansion);
            energy.SetFloat("_AuraVerticalExpansion",vertical);ink.SetFloat("_AuraVerticalExpansion",vertical);
            transform.localPosition=p.offset;transform.localRotation=Quaternion.identity;
            transform.localScale=Vector3.one*(p.scale*owner.GetComponent<CharacterController>().height/1.9f);
            EnsureSurfaceLayout(p);
            if(p.detachedTips)EnsureTips();
            age+=Time.unscaledDeltaTime;
            energy.SetFloat("_AuraTime",age);ink.SetFloat("_AuraTime",age);
            energy.SetFloat("_InkCoverage",p.ink.enabled?p.ink.opacity*p.ink.intensity:0);
            for(int i=0;i<3;i++) {
                var setting=i==0?p.mainAura:i==1?p.ink:p.wisps;
                layers[i].enabled=setting.enabled&&(i!=1||p.mainAura.enabled)&&envelope>.001f;
                blocks[i].SetFloat("_Strength",envelope*p.intensity*setting.intensity);
                blocks[i].SetFloat("_Opacity",setting.opacity);blocks[i].SetFloat("_Wisp",i==2?1:0);
                layers[i].SetPropertyBlock(blocks[i]);
            }
            if(tips!=null)for(int i=0;i<2;i++) {
                tips[i].enabled=p.detachedTips&&p.mainAura.enabled&&(i==0||p.ink.enabled)&&envelope>.001f;
                tips[i].SetPropertyBlock(blocks[i]);
            }
            rim.SetFloat("_Strength",envelope*p.intensity*p.bodyRim.intensity*p.bodyRim.opacity);
            foreach(var r in rims)if(r!=null)r.enabled=p.bodyRim.enabled&&envelope>.001f;
            bool next=emit&&p.motes.enabled&&p.particleRate>0;
            if(next&&!emitting){particles.Play();particles.Emit(7);}
            if(!next&&emitting)particles.Stop(true,ParticleSystemStopBehavior.StopEmitting);
            emitting=next;
            mote.SetFloat("_Strength",envelope*p.motes.opacity);
            if(envelope<=.001f)particles.Clear();
        }
        public void StopImmediate()
        {
            if(layers!=null)foreach(var r in layers)if(r!=null)r.enabled=false;
            if(tips!=null)foreach(var r in tips)if(r!=null)r.enabled=false;
            foreach(var r in rims)if(r!=null)r.enabled=false;
            if(particles!=null)particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            emitting=false;
        }
        private void OnDestroy()
        {
            foreach(var r in rims)if(r!=null)Destroy(r.gameObject);
            foreach(var mesh in ownedMeshes)if(mesh!=null)Destroy(mesh);
            foreach(var mat in ownedMaterials)if(mat!=null)Destroy(mat);
            ownedMeshes.Clear();ownedMaterials.Clear();
        }
    }
}
