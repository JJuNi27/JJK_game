using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Shibuya
{
    [DisallowMultipleComponent]
    public sealed class ShibuyaGuardVisual : MonoBehaviour
    {
        private ShibuyaFighter owner;
        private GameObject surface;
        private Mesh mesh;
        private Material material;
        private MeshRenderer rendererHandle;
        private MaterialPropertyBlock properties;
        private float impactAt=-100,breakAt=-100;
        public bool IsVisible=>rendererHandle!=null&&rendererHandle.enabled;
        public MeshRenderer SurfaceRenderer=>rendererHandle;
        public float BreakProgress {get;private set;}
        private void Awake()
        {
            owner=GetComponent<ShibuyaFighter>();
            var p=owner.GuardSettings;
            // Keep presentation outside body-renderer discovery used by the protected F aura.
            surface=new GameObject("GuardPresentation");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(surface,gameObject.scene);
            var filter=surface.AddComponent<MeshFilter>();rendererHandle=surface.AddComponent<MeshRenderer>();
            mesh=new Mesh {name="Shibuya Guard curved surface (runtime)"};
            var vertices=new Vector3[13*9];var uv=new Vector2[vertices.Length];var triangles=new int[12*8*6];int k=0;
            for(int y=0;y<9;y++)for(int x=0;x<13;x++) {
                int i=y*13+x;float u=x/12f,v=y/8f,a=(u-.5f)*2.25f;
                vertices[i]=new Vector3(Mathf.Sin(a),v-.5f,Mathf.Cos(a));uv[i]=new Vector2(u,v);
                if(x<12&&y<8){triangles[k++]=i;triangles[k++]=i+13;triangles[k++]=i+1;triangles[k++]=i+1;triangles[k++]=i+13;triangles[k++]=i+14;}
            }
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.bounds=new Bounds(Vector3.zero,Vector3.one*4);filter.sharedMesh=mesh;
            Shader shader=p.shieldShader!=null?p.shieldShader:Shader.Find("JJK/Shibuya Guard");
            material=new Material(shader){name="Shibuya Guard (runtime)"};rendererHandle.sharedMaterial=material;
            rendererHandle.shadowCastingMode=ShadowCastingMode.Off;rendererHandle.receiveShadows=false;
            rendererHandle.enabled=false;properties=new MaterialPropertyBlock();
        }
        private void OnEnable(){owner.GuardBlocked+=Impact;owner.GuardBroken+=Break;}
        private void OnDisable(){if(owner!=null){owner.GuardBlocked-=Impact;owner.GuardBroken-=Break;}if(rendererHandle!=null)rendererHandle.enabled=false;impactAt=breakAt=-100;}
        private void Impact(ShibuyaFighter f){impactAt=Time.time;}
        private void Break(ShibuyaFighter f){breakAt=Time.time;}
        private void LateUpdate()
        {
            var p=owner.GuardSettings;if(p==null){rendererHandle.enabled=false;return;}
            bool broken=Time.time-breakAt<p.breakVisualDuration;
            rendererHandle.enabled=owner.isActiveAndEnabled&&!owner.Health.IsDead&&owner.State!=ShibuyaFighterState.Finished&&(owner.IsGuarding||broken);
            if(!rendererHandle.enabled)return;
            var state=owner.GuardVisualState;float level=state==ShibuyaGuardVisualState.Stable?0:state==ShibuyaGuardVisualState.Strained?1:2;
            Color color=level==0?p.stableColor:level==1?p.strainedColor:p.criticalColor;
            float impact=Mathf.Clamp01(1-(Time.time-impactAt)/Mathf.Max(.01f,p.impactDuration));
            BreakProgress=broken?Mathf.Clamp01((Time.time-breakAt)/Mathf.Max(.01f,p.breakVisualDuration)):0;
            surface.transform.SetPositionAndRotation(transform.TransformPoint(new Vector3(0,p.shieldCenterHeight,0)),transform.rotation);
            surface.transform.localScale=Vector3.Scale(transform.lossyScale,new Vector3(p.shieldRadius,p.shieldHeight,p.shieldRadius));
            properties.SetColor("_Tint",color);properties.SetFloat("_Impact",impact*p.impactStrength*(1+level));
            properties.SetFloat("_Instability",level>1?p.criticalInstability:0);
            properties.SetFloat("_Cracks",level>1?p.crackStrength:0);properties.SetFloat("_Break",BreakProgress);
            properties.SetFloat("_BreakForce",p.breakVisualStrength);rendererHandle.SetPropertyBlock(properties);
        }
        private void OnDestroy(){if(surface!=null)Destroy(surface);if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
