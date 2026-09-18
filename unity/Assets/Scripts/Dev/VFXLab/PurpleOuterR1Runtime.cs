using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Dev.PurpleBodyExploration
{
    // An owned companion to the frozen body. Never writes into the body material or profile.
    public sealed class PurpleOuterR1Runtime : MonoBehaviour
    {
        private PurpleOuterR1Profile profile;
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
        private readonly ParticleSystem.Particle[] sparks=new ParticleSystem.Particle[64];
        private GameObject halo;
        private float radius;
        private Material distortionMaterial;
        private GameObject distortion;
        public PurpleOuterStage Stage {get;set;}
        public void Configure(PurpleOuterR1Profile settings,float diameter)
        {
            profile=settings;radius=diameter*.5f;Stage=profile.previewStage;
            halo=GameObject.CreatePrimitive(PrimitiveType.Cube);halo.name="OuterR1_VolumetricHalo";halo.transform.SetParent(transform,false);
            halo.transform.localScale=Vector3.one*diameter*profile.haloRatio*1.13f;
            var collider=halo.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            haloMaterial=new Material(Resources.Load<Shader>("VFX/PurpleOuterHalo")){name="PurpleOuterR1_Halo_Runtime"};
            var renderer=halo.GetComponent<Renderer>();renderer.sharedMaterial=haloMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            particleMaterial=new Material(Resources.Load<Shader>("VFX/PurpleOuterParticle")){name="PurpleOuterR1_Particle_Runtime"};
            shardMesh=CreateShard();debris=CreateParticles("OuterR1_PlasmaShards",64);
            segmentMesh=CreateSegment();lightning=CreateParticles("OuterR1_OutwardLightning",256);
            arcMaterial=new Material(particleMaterial){name="PurpleOuterR1_ArcParticles_Runtime"};
            lightning.GetComponent<ParticleSystemRenderer>().sharedMaterial=arcMaterial;
            lightning.GetComponent<ParticleSystemRenderer>().mesh=segmentMesh;
            distortion=GameObject.CreatePrimitive(PrimitiveType.Cube);distortion.name="OuterR1_BackgroundDistortionShell";distortion.transform.SetParent(transform,false);
            distortion.transform.localScale=Vector3.one*diameter*profile.distortionRatio;
            var distortionCollider=distortion.GetComponent<Collider>();distortionCollider.enabled=false;Destroy(distortionCollider);
            distortionMaterial=new Material(Resources.Load<Shader>("VFX/PurpleOuterDistortion")){name="PurpleOuterR1_Distortion_Runtime"};
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
            var mesh=new Mesh{name="PurpleOuterR1_Shard_Runtime"};
            mesh.vertices=new[]{new Vector3(0,-.5f,0),new Vector3(0,.5f,0),new Vector3(-.5f,0,0),new Vector3(.5f,0,0),new Vector3(0,0,-.5f),new Vector3(0,0,.5f)};
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
            var mesh=new Mesh{name="PurpleOuterR1_UnitSegment_Runtime"};
            mesh.vertices=new[]{new Vector3(-.5f,-.5f,-.5f),new Vector3(.5f,-.5f,-.5f),new Vector3(.5f,.5f,-.5f),new Vector3(-.5f,.5f,-.5f),
                new Vector3(-.5f,-.5f,.5f),new Vector3(.5f,-.5f,.5f),new Vector3(.5f,.5f,.5f),new Vector3(-.5f,.5f,.5f)};
            mesh.triangles=new[]{0,3,2,0,2,1,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,0,1,5,0,5,4,3,7,6,3,6,2};
            mesh.RecalculateBounds();mesh.RecalculateNormals();return mesh;
        }
        private void Segment(Vector3 a,Vector3 b,float width,float fade,float life,float age,ref int count)
        {
            Vector3 delta=b-a;float length=delta.magnitude;
            if(length<.001f || count+2>arcParticles.Length)return;
            for(int layer=0;layer<2;layer++)
            {
                float w=width*(layer==0?2.5f:.6f);
                arcParticles[count++]=new ParticleSystem.Particle{position=(a+b)*.5f,velocity=Vector3.zero,
                    startSize3D=new Vector3(w,length*1.04f,w),rotation3D=Quaternion.FromToRotation(Vector3.up,delta).eulerAngles,
                    startColor=layer==0?new Color(1,.005f,.44f,fade*.35f):new Color(1,.8f,.97f,fade),
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
                if(age<0 || age>life)continue;
                float u=age/life,pulse=Mathf.Clamp01(u/.09f)*Mathf.Pow(1-u,.55f);ArcPreviewPower+=pulse;
                Vector3 d=Direction(seed+2);d.y*=.5f;d.Normalize();Vector3 lateral=Vector3.Cross(d,Vector3.up).normalized;
                float reach=profile.arcReach*(.88f+Hash(seed+3)*.16f),growth=Mathf.Clamp01(age/.018f);
                Vector3 root=d*radius*.93f;
                for(int n=0;n<arcPoints.Length;n++)
                {
                    float s=n/(float)(arcPoints.Length-1);Vector3 noise=Direction(seed+n*11+Mathf.Floor(age*60)*.31f)*radius*.045f;
                    arcPoints[n]=root+d*(radius*(reach-.93f)*s*growth)+lateral*Mathf.Sin(s*5+seed)*radius*.13f*s+noise*Mathf.Sin(s*Mathf.PI);
                    if(n>0)Segment(arcPoints[n-1],arcPoints[n],profile.arcWidth*(1-s*.65f),pulse,life,age,ref count);
                }
                Vector3 branch=arcPoints[8],end=branch+(d+lateral*(Hash(seed+5)>.5f?1:-1)*.85f+Vector3.up*.35f).normalized*radius*.48f;
                for(int n=1;n<=7;n++)
                {
                    float s=n/7f;Vector3 next=Vector3.Lerp(arcPoints[8],end,s)+Direction(seed+n*19)*radius*.035f*Mathf.Sin(s*Mathf.PI);
                    Segment(branch,next,profile.arcWidth*.4f*(1-s*.6f),pulse*.75f,life,age,ref count);branch=next;
                }
            }
            lightning.SetParticles(arcParticles,count);lightning.Pause();
        }
        private void SampleDebris(float time)
        {
            int count=0;
            for(int i=0;i<profile.sparkCount;i++)
            {
                float cycle=.62f+Hash(i+13)*.35f,clock=time+Hash(i+1)*2,age=Mathf.Repeat(clock,cycle),seed=i*17+Mathf.Floor(clock/cycle)*31;
                float life=.28f+Hash(seed+8)*.27f;if(age>life)continue;
                Vector3 direction=Direction(seed),side=Vector3.Cross(direction,Vector3.up).normalized;
                float speed=profile.sparkSpeed*(.65f+Hash(seed+12)*.6f);
                Vector3 position=direction*(radius*1.015f+age*speed)+side*Mathf.Sin(age*17+seed)*age*.65f;
                Vector3 velocity=direction*speed+side*Mathf.Cos(age*17+seed)*.9f;
                float fade=Mathf.Clamp01(age/.035f)*Mathf.Pow(1-age/life,.65f);
                Color colour=i%4==0?new Color(1,.75f,.95f,fade):new Color(1,.025f,.52f,fade);
                sparks[count++]=new ParticleSystem.Particle{position=position,velocity=Vector3.zero,startLifetime=life,remainingLifetime=life-age,
                    startSize3D=new Vector3(.025f,.14f+Hash(seed+19)*.23f,.025f),rotation3D=Quaternion.FromToRotation(Vector3.up,velocity).eulerAngles,startColor=colour,randomSeed=(uint)(i+1)};
            }
            debris.SetParticles(sparks,count);debris.Pause();
        }
        public void Sample(float time)
        {
            if(haloMaterial==null)return;
            halo.SetActive(Stage>=PurpleOuterStage.Halo);
            haloMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius));
            haloMaterial.SetVector("_Halo",new Vector4(profile.haloRatio,profile.haloIntensity,time,0));
            particleMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius));
            particleMaterial.SetFloat("_Emission",profile.sparkEmission);
            debris.gameObject.SetActive(Stage>=PurpleOuterStage.Debris);
            if(debris.gameObject.activeSelf)SampleDebris(time);
            arcMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius));arcMaterial.SetFloat("_Emission",profile.arcEmission);
            lightning.gameObject.SetActive(Stage>=PurpleOuterStage.Lightning);
            if(lightning.gameObject.activeSelf)SampleLightning(time);
            distortion.SetActive(Stage>=PurpleOuterStage.Distortion);
            distortionMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius));
            distortionMaterial.SetVector("_Field",new Vector4(profile.distortionRatio,profile.distortionStrength,time,0));
        }
        private void OnDestroy(){if(haloMaterial!=null)Destroy(haloMaterial);if(particleMaterial!=null)Destroy(particleMaterial);if(arcMaterial!=null)Destroy(arcMaterial);if(distortionMaterial!=null)Destroy(distortionMaterial);if(shardMesh!=null)Destroy(shardMesh);if(segmentMesh!=null)Destroy(segmentMesh);}
    }
}
