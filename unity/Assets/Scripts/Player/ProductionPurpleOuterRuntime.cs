using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // An owned companion to the frozen body. Never writes into the body material or profile.
    public sealed class ProductionPurpleOuterRuntime : MonoBehaviour
    {
        private ProductionPurpleOuterSettings profile;
        private Material haloMaterial;
        private Material particleMaterial;
        private Mesh shardMesh;
        private ParticleSystem debris;
        private Material arcMaterial;
        private Mesh segmentMesh;
        private ParticleSystem lightning;
        private readonly ParticleSystem.Particle[] arcParticles=new ParticleSystem.Particle[256];
        private readonly Vector3[] arcPoints=new Vector3[17];
        public float ArcPreviewPower {get;private set;}
        public float HeroPreviewPower {get;private set;}
        private readonly ParticleSystem.Particle[] sparks=new ParticleSystem.Particle[64];
        private GameObject halo;
        private float radius;
        private Material distortionMaterial;
        private GameObject distortion;
        private Vector3 travelVelocity; private float travelAge;
        private PurpleTravelVisualProfile travelProfile;
        private Shader chargeHaloShader,chargeDistortionShader,travelHaloShader,travelDistortionShader;
        private GameObject travelWake;
        private Material wakeMaterial;
        private float travelWeight;
        private float coronaContrast;
        public bool TravelResponseActive=>travelWeight>0;
        public float WakeMaximumLength=>radius*transform.lossyScale.x*(travelProfile!=null?travelProfile.wakeLengthRadii:0);
        public float MaxResidualDistance {get;private set;}
        private Vector3 Residual(float age,bool worldAnchored=false)
        {
            Vector3 world=-travelVelocity*Mathf.Min(age,travelAge,profile.residualSeconds)*(1-profile.velocityInheritance);
            world=Vector3.ClampMagnitude(world,radius*transform.lossyScale.x*profile.maxResidualRadius);
            if(TravelResponseActive)
            {
                // Constant-velocity production path: subtract motion since birth to retain a short world-space anchor.
                float retained=worldAnchored?1:1-travelProfile.partialInheritance;
                world=-travelVelocity*Mathf.Min(age,travelAge,travelProfile.residueLifetime)*retained;
                world=Vector3.ClampMagnitude(world,radius*transform.lossyScale.x*travelProfile.residueRadiusLimit);
            }
            MaxResidualDistance=Mathf.Max(MaxResidualDistance,world.magnitude);
            return transform.InverseTransformVector(world);
        }
        public void Configure(ProductionPurpleOuterSettings settings,float diameter)
        {
            profile=settings;radius=diameter*.5f;
            var candidate=PurpleFinalPolishProfile.Current;
            bool polish=candidate!=null && candidate.outerPolishEnabled;
            if(polish){profile=candidate.outer;coronaContrast=candidate.coronaContrast;}
            travelProfile=Resources.Load<PurpleTravelVisualProfile>("VFX/PurpleTravelVisualProfile");
            chargeHaloShader=Resources.Load<Shader>("VFX/PurpleOuterR2Halo");chargeDistortionShader=Resources.Load<Shader>("VFX/PurpleOuterR2Distortion");
            travelHaloShader=Resources.Load<Shader>("VFX/PurpleTravelHalo");travelDistortionShader=Resources.Load<Shader>("VFX/PurpleTravelDistortion");
            if(polish)chargeHaloShader=travelHaloShader=Resources.Load<Shader>("VFX/PurplePolishHalo");
            halo=GameObject.CreatePrimitive(PrimitiveType.Cube);halo.name="OuterR2_VolumetricHalo";halo.transform.SetParent(transform,false);
            halo.transform.localScale=Vector3.one*diameter*profile.haloRatio*1.23f;
            var collider=halo.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            haloMaterial=new Material(Resources.Load<Shader>("VFX/PurpleOuterR2Halo")){name="PurpleProduction_Halo_Runtime"};
            var renderer=halo.GetComponent<Renderer>();renderer.sharedMaterial=haloMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            particleMaterial=new Material(Resources.Load<Shader>("VFX/PurpleOuterR2Particle")){name="PurpleProduction_Particle_Runtime"};
            shardMesh=CreateShard();debris=CreateParticles("OuterR2_PlasmaShards",64);
            segmentMesh=CreateSegment();lightning=CreateParticles("OuterR2_OutwardLightning",256);
            arcMaterial=new Material(particleMaterial){name="PurpleProduction_ArcParticles_Runtime"};
            lightning.GetComponent<ParticleSystemRenderer>().sharedMaterial=arcMaterial;
            lightning.GetComponent<ParticleSystemRenderer>().mesh=segmentMesh;
            distortion=GameObject.CreatePrimitive(PrimitiveType.Cube);distortion.name="OuterR2_BackgroundDistortionShell";distortion.transform.SetParent(transform,false);
            distortion.transform.localScale=Vector3.one*diameter*profile.distortionRatio;
            var distortionCollider=distortion.GetComponent<Collider>();distortionCollider.enabled=false;Destroy(distortionCollider);
            distortionMaterial=new Material(Resources.Load<Shader>("VFX/PurpleOuterR2Distortion")){name="PurpleProduction_Distortion_Runtime"};
            var distortionRenderer=distortion.GetComponent<Renderer>();distortionRenderer.sharedMaterial=distortionMaterial;distortionRenderer.shadowCastingMode=ShadowCastingMode.Off;distortionRenderer.receiveShadows=false;
            Sample(1.5f);
        }
        private static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+31.7f)*43758.5453f,1);
        private static Vector3 Direction(float seed)
        {
            float y=Hash(seed+3)*1.6f-.8f,a=Hash(seed+7)*Mathf.PI*2,s=Mathf.Sqrt(1-y*y);
            return new Vector3(Mathf.Cos(a)*s,y,Mathf.Sin(a)*s);
        }
        private static Mesh CreateShard()
        {
            var mesh=new Mesh{name="PurpleProduction_Shard_Runtime"};
            mesh.vertices=new[]{new Vector3(0,-.55f,0),new Vector3(.17f,.5f,0),new Vector3(-.5f,-.1f,-.12f),new Vector3(.4f,.12f,.1f),new Vector3(0,.04f,-.5f),new Vector3(-.09f,-.16f,.4f)};
            mesh.triangles=new[]{0,2,4,0,4,3,0,3,5,0,5,2,1,4,2,1,3,4,1,5,3,1,2,5};
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        private ParticleSystem CreateParticles(string name,int capacity)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);
            var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.loop=false;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            main.scalingMode=ParticleSystemScalingMode.Hierarchy;main.maxParticles=capacity;main.startSize3D=true;main.startRotation3D=true;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Mesh;renderer.mesh=shardMesh;
            renderer.alignment=ParticleSystemRenderSpace.Local;renderer.sharedMaterial=particleMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            return ps;
        }
        private static Mesh CreateSegment()
        {
            var mesh=new Mesh{name="PurpleProduction_UnitSegment_Runtime"};
            mesh.vertices=new[]{new Vector3(-.5f,-.5f,-.5f),new Vector3(.5f,-.5f,-.5f),new Vector3(.33f,.5f,-.33f),new Vector3(-.33f,.5f,-.33f),
                new Vector3(-.5f,-.5f,.5f),new Vector3(.5f,-.5f,.5f),new Vector3(.33f,.5f,.33f),new Vector3(-.33f,.5f,.33f)};
            mesh.triangles=new[]{0,3,2,0,2,1,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,0,1,5,0,5,4,3,7,6,3,6,2};
            mesh.RecalculateBounds();mesh.RecalculateNormals();return mesh;
        }
        private void Segment(Vector3 a,Vector3 b,float width,float fade,float life,float age,ref int count)
        {
            Vector3 delta=b-a;float length=delta.magnitude;
            if(length<.001f || count+2>arcParticles.Length)return;
            for(int layer=0;layer<2;layer++)
            {
                float w=width*(layer==0?1.85f:.65f);
                arcParticles[count++]=new ParticleSystem.Particle{position=(a+b)*.5f,velocity=Vector3.zero,
                    startSize3D=new Vector3(w,length*1.04f,w),rotation3D=Quaternion.FromToRotation(Vector3.up,delta).eulerAngles,
                    startColor=layer==0?new Color(1,.005f,.44f,fade*.24f):new Color(1,.8f,.97f,fade),
                    startLifetime=life,remainingLifetime=Mathf.Max(.001f,life-age),randomSeed=(uint)count};
            }
        }
        private void SampleLightning(float time)
        {
            int count=0;ArcPreviewPower=0;
            for(int lane=0;lane<2;lane++)
            {
                float birth=.04f+lane*.13f;int epoch=0;
                while(epoch<64){float next=birth+.25f+Hash(epoch*17+lane*51+9)*.22f;if(next>time)break;birth=next;epoch++;}
                float seed=epoch*43+lane*59,age=time-birth,life=profile.arcLifetime*(.7f+Hash(seed+8)*.6f);
                if(TravelResponseActive)life=Mathf.Min(life,travelProfile.residueLifetime);
                if(age<0 || age>life)continue;
                float u=age/life,pulse=Mathf.Clamp01(u/.07f)*Mathf.Pow(1-u,.7f);
                pulse*=.45f+.55f*Mathf.Pow(.5f+.5f*Mathf.Sin(age*145+seed),2);ArcPreviewPower+=pulse;
                Vector3 d=Direction(seed+2);d.y*=.75f;d.Normalize();Vector3 lateral=Vector3.Cross(d,Vector3.up).normalized;
                float reach=profile.arcReach*(.76f+Hash(seed+3)*.30f),growth=Mathf.Clamp01(age/.012f);
                Vector3 root=d*radius*.93f+Residual(age,true);
                for(int n=0;n<arcPoints.Length;n++)
                {
                    float s=n/(float)(arcPoints.Length-1);Vector3 noise=Direction(seed+n*11+Mathf.Floor(age*70)*.31f)*radius*.065f;
                    arcPoints[n]=root+d*(radius*(reach-.93f)*s*growth)+lateral*Mathf.Sin(s*(5+Hash(seed+12)*5)+seed)*radius*.17f*s+noise*Mathf.Sin(s*Mathf.PI);
                    float taper=Mathf.Pow(1-s,1.25f)*1.65f+.035f;
                    float current=pulse*(.7f+.3f*Mathf.Sin(s*11-age*95+seed));
                    if(n>0)Segment(arcPoints[n-1],arcPoints[n],profile.arcWidth*taper,current,life,age,ref count);
                }
                int fork=5+Mathf.FloorToInt(Hash(seed+21)*6);
                Vector3 branch=arcPoints[fork],end=branch+(d+lateral*(Hash(seed+5)>.5f?1:-1)*(.6f+Hash(seed+22))
                    +Vector3.up*(Hash(seed+23)-.5f)).normalized*radius*(.28f+Hash(seed+24)*.34f);
                for(int n=1;n<=7;n++)
                {
                    float s=n/7f;Vector3 next=Vector3.Lerp(arcPoints[fork],end,s)+Direction(seed+n*19)*radius*.045f*Mathf.Sin(s*Mathf.PI);
                    Segment(branch,next,profile.arcWidth*(.5f*Mathf.Pow(1-s,1.3f)+.02f),pulse*.65f,life,age,ref count);branch=next;
                }
            }
            lightning.SetParticles(arcParticles,count);lightning.Pause();
        }
        private void SampleDebris(float time)
        {
            int count=0;HeroPreviewPower=0;
            for(int i=0;i<profile.sparkCount;i++)
            {
                float cycle=.62f+Hash(i+13)*.35f,clock=time+Hash(i+1)*2,age=Mathf.Repeat(clock,cycle),seed=i*17+Mathf.Floor(clock/cycle)*31;
                bool anchored=i%3==0;
                float life=.28f+Hash(seed+8)*.27f;if(TravelResponseActive && anchored)life=Mathf.Min(life,travelProfile.residueLifetime);if(age>life)continue;
                Vector3 direction=Direction(seed),side=Vector3.Cross(direction,Vector3.up).normalized;
                float speed=profile.sparkSpeed*(.65f+Hash(seed+12)*.6f);
                Vector3 position=direction*(radius*1.015f+age*speed)+side*Mathf.Sin(age*17+seed)*age*.65f;
                Vector3 velocity=direction*speed+side*Mathf.Cos(age*17+seed)*.9f;
                float fade=Mathf.Clamp01(age/.035f)*Mathf.Pow(1-age/life,.65f);
                Color colour=i%4==0?new Color(1,.75f,.95f,fade):new Color(1,.025f,.52f,fade);
                sparks[count++]=new ParticleSystem.Particle{position=position+Residual(age,anchored),velocity=Vector3.zero,startLifetime=life,remainingLifetime=life-age,
                    startSize3D=new Vector3(.025f+Hash(seed+20)*.024f,.14f+Hash(seed+19)*.28f,.03f),rotation3D=Quaternion.FromToRotation(Vector3.up,velocity).eulerAngles,startColor=colour,randomSeed=(uint)(i+1)};
            }
            for(int i=0;i<profile.heroFragments && count+4<=sparks.Length;i++)
            {
                float cycle=.91f+Hash(i+39)*.3f,clock=time+i*.29f,age=Mathf.Repeat(clock,cycle),seed=137+i*37+Mathf.Floor(clock/cycle)*19;
                bool anchored=i==0;
                float life=.39f+Hash(seed+5)*.10f;if(TravelResponseActive && anchored)life=Mathf.Min(life,travelProfile.residueLifetime);if(age>life)continue;
                Vector3 direction=Direction(seed),side=Vector3.Cross(direction,Vector3.up).normalized;
                float speed=profile.sparkSpeed*(.8f+Hash(seed+7)*.3f),fade=Mathf.Clamp01(age/.025f)*Mathf.Pow(1-age/life,.3f);
                Vector3 velocity=direction*speed+side*Mathf.Cos(age*14+seed)*1.1f;
                Vector3 rotation=Quaternion.FromToRotation(Vector3.up,velocity).eulerAngles;
                for(int layer=0;layer<4;layer++)
                {
                    float lag=layer<2?0:(layer-1)*.055f,sample=Mathf.Max(0,age-lag);
                    Vector3 position=direction*(radius*1.02f+sample*speed)+side*Mathf.Sin(sample*14+seed)*sample*.65f;
                    if(layer==0)HeroPreviewPower+=fade*Mathf.Max(0,new Vector2(position.x,position.y).magnitude-radius*1.12f);
                    float scale=profile.heroFragmentScale*(layer==0?1:layer==1?.5f:.48f);
                    sparks[count++]=new ParticleSystem.Particle{position=position+Residual(age,anchored),velocity=Vector3.zero,startLifetime=life,remainingLifetime=life-age,
                        startSize3D=new Vector3(.11f,.30f+Hash(seed+11)*.10f,.075f)*scale,rotation3D=rotation,
                        startColor=layer==1?new Color(1,.83f,.98f,fade):new Color(1,.015f,.58f,fade*(layer<2?1:.25f)),randomSeed=(uint)(100+i*4+layer)};
                }
            }
            debris.SetParticles(sparks,count);debris.Pause();
        }
        public void Sample(float time,Vector3 velocity=default,float ageSinceRelease=0)
        {
            if(haloMaterial==null)return;
            travelVelocity=velocity;travelAge=Mathf.Max(0,ageSinceRelease);MaxResidualDistance=0;
            travelWeight=travelProfile!=null && travelProfile.travelRefinementEnabled && travelAge>0?Mathf.Clamp01(velocity.magnitude/travelProfile.referenceSpeed)*Mathf.SmoothStep(0,1,travelAge/.035f):0;
            haloMaterial.shader=TravelResponseActive?travelHaloShader:chargeHaloShader;
            if(coronaContrast>0)haloMaterial.SetFloat("_CoronaContrast",coronaContrast);
            distortionMaterial.shader=TravelResponseActive?travelDistortionShader:chargeDistortionShader;
            float boost=TravelResponseActive?1+travelProfile.releaseBoost*Mathf.Exp(-travelAge/travelProfile.releasePeakSeconds):1;
            Vector3 direction=velocity.sqrMagnitude>.001f?velocity.normalized:Vector3.forward;
            var motion=new Vector4(direction.x,direction.y,direction.z,travelWeight*boost);
            halo.transform.localScale=Vector3.one*radius*2*(profile.haloRatio*1.23f+(TravelResponseActive?travelProfile.haloRearStretch*(1+travelProfile.releaseBoost):0));
            if(TravelResponseActive)
            {
                haloMaterial.SetVector("_Motion",motion);haloMaterial.SetFloat("_RearStretch",travelProfile.haloRearStretch);
                distortionMaterial.SetVector("_Motion",motion);distortionMaterial.SetFloat("_RearStrength",travelProfile.rearDistortion);
                if(travelWake==null)
                {
                    travelWake=GameObject.CreatePrimitive(PrimitiveType.Cube);travelWake.name="PurpleProduction_ShortPlasmaWake";travelWake.transform.SetParent(transform,false);
                    var collider=travelWake.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
                    wakeMaterial=new Material(Resources.Load<Shader>("VFX/PurpleTravelWake")){name="PurpleProduction_TravelWake_Runtime"};
                    var renderer=travelWake.GetComponent<Renderer>();renderer.sharedMaterial=wakeMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                }
                travelWake.SetActive(true);travelWake.transform.localScale=Vector3.one*radius*2*(1.25f+travelProfile.wakeLengthRadii);
                wakeMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius*transform.lossyScale.x));
                wakeMaterial.SetVector("_Motion",motion);
                wakeMaterial.SetVector("_Wake",new Vector4(travelProfile.wakeLengthRadii,travelProfile.wakeWidthRadii,time,travelProfile.wakeEmission));
            }
            else if(travelWake!=null)travelWake.SetActive(false);
            halo.SetActive(profile.haloEnabled);
            haloMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius*transform.lossyScale.x));
            haloMaterial.SetVector("_Halo",new Vector4(profile.haloRatio,profile.haloIntensity,time,0));
            particleMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius*transform.lossyScale.x));
            particleMaterial.SetFloat("_Emission",profile.sparkEmission);
            debris.gameObject.SetActive(profile.debrisEnabled);
            if(debris.gameObject.activeSelf)SampleDebris(time);
            arcMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius*transform.lossyScale.x));arcMaterial.SetFloat("_Emission",profile.arcEmission);
            lightning.gameObject.SetActive(profile.lightningEnabled);
            if(lightning.gameObject.activeSelf)SampleLightning(time);
            distortion.SetActive(profile.distortionEnabled);
            distortionMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius*transform.lossyScale.x));
            distortionMaterial.SetVector("_Field",new Vector4(profile.distortionRatio,profile.distortionStrength,time,0));
        }
        private void OnDestroy(){if(wakeMaterial!=null)Destroy(wakeMaterial);if(haloMaterial!=null)Destroy(haloMaterial);if(particleMaterial!=null)Destroy(particleMaterial);if(arcMaterial!=null)Destroy(arcMaterial);if(distortionMaterial!=null)Destroy(distortionMaterial);if(shardMesh!=null)Destroy(shardMesh);if(segmentMesh!=null)Destroy(segmentMesh);}
    }
}
