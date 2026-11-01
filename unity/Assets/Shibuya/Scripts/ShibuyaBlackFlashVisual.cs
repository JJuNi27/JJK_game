using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Shibuya
{
    public sealed class ShibuyaBlackFlashVisual : MonoBehaviour
    {
        readonly Mesh[] meshes=new Mesh[5];readonly Transform[] layers=new Transform[5];
        readonly MeshRenderer[] renderers=new MeshRenderer[5];MaterialPropertyBlock block;
        readonly ShibuyaBlackFlashGeometry geometry=new ShibuyaBlackFlashGeometry();
        readonly float[] lives=new float[5];
        ShibuyaBlackFlashProfile profile;double started;float lifetime;int seed;
        public double Started=>started;
        public bool Live {get;private set;}
        public int VertexCount {get {int n=0;foreach(var m in meshes)if(m!=null)n+=m.vertexCount;return n;}}
        public void Initialize(Material ink,Material light,Material smoke)
        {
            block=new MaterialPropertyBlock();
            string[] names={"ContactLight","TornCore","BranchingRupture","BrokenPressure","SmokeEmbers"};
            for(int i=0;i<5;i++){
                var go=new GameObject(names[i]);go.transform.SetParent(transform,false);layers[i]=go.transform;
                meshes[i]=new Mesh{name="BlackFlash pooled "+names[i]};meshes[i].MarkDynamic();go.AddComponent<MeshFilter>().sharedMesh=meshes[i];
                var r=go.AddComponent<MeshRenderer>();renderers[i]=r;r.sharedMaterial=i==0||i==3?light:i==4?smoke:ink;
                r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;
            }
        }
        public void Play(Vector3 point,Vector3 direction,ShibuyaBlackFlashProfile settings,int patternSeed,double now)
        {
            profile=settings;seed=patternSeed;started=now;lifetime=settings.Lifetime;
            transform.SetPositionAndRotation(point,Quaternion.LookRotation(direction.sqrMagnitude>.0001f?direction.normalized:Vector3.forward));
            transform.localScale=Vector3.one*Mathf.Clamp(settings.scale,.1f,10);
            for(int i=0;i<5;i++)geometry.Build(meshes[i],i,settings,patternSeed+i*7919);
            Live=true;gameObject.SetActive(true);Sample(0);
        }
        public void Tick(double now){if(!Live)return;float age=(float)(now-started);if(age>=lifetime){Clear();return;}Sample(age);}
        public void Clear(){Live=false;gameObject.SetActive(false);}
        // Explicit visual age also makes same-seed, same-time multi-angle QA reproducible.
        public void Sample(float age)
        {
            if(profile==null)return;float a=Mathf.Max(0,age);
            lives[0]=profile.flashDuration;lives[1]=profile.coreDuration;lives[2]=profile.branchDuration;
            lives[3]=profile.pressureSize/Mathf.Max(.1f,profile.pressureSpeed);lives[4]=profile.residueDuration;
            for(int i=0;i<5;i++) {
                float delay=i==3?.025f:i==4?.065f:0;
                float t=Mathf.Max(0,a-delay),life=Mathf.Clamp(lives[i],.01f,lifetime);
                renderers[i].enabled=a>=delay&&t<life;
                float expansion=i==1?profile.coreExpansion:
                    i==3?Mathf.Min(profile.pressureSize,.24f+t*profile.pressureSpeed):1;
                layers[i].localScale=Vector3.one*Mathf.Max(.001f,expansion);
                layers[i].localPosition=Vector3.zero;
                Color color=i==0?Color.white*profile.flashIntensity:i==3?new Color(.8f,.85f,1,.5f):profile.lightningColor;
                float alpha=i==0?1:i==3?.5f:1;color*=Mathf.Max(0,profile.intensity);color.a=alpha;
                block.Clear();block.SetColor("_Color",color);block.SetFloat("_Age",t);block.SetFloat("_Life",life);
                block.SetFloat("_Mode",i==4?1:0);block.SetFloat("_Seed",(seed%997)*.13f);
                block.SetFloat("_Growth",Mathf.Clamp(profile.ruptureGrowthTime,.03f,.2f));
                block.SetFloat("_Morph",Mathf.Clamp01(profile.silhouetteChange));
                block.SetFloat("_Interval",Mathf.Clamp(profile.dischargeInterval,.02f,.12f));
                block.SetFloat("_DischargeLife",Mathf.Clamp(profile.residualDischargeTime,.1f,.7f));
                block.SetFloat("_SmokeSpread",Mathf.Clamp(profile.smokeSpread,.3f,3));
                renderers[i].SetPropertyBlock(block);
            }
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);}
    }
}
