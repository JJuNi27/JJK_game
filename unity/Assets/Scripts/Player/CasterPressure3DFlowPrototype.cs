using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // One open, asymmetric stream with a diffuse envelope, an off-axis interior and a local crest.
    // No billboard, closed tube, circular orbit or parenting to the travelling Purple body.
    public sealed class CasterPressure3DFlowPrototype : MonoBehaviour
    {
        const int Steps=60,Across=13,Layers=3;
        sealed class Surface {public Mesh mesh;public Material material;public MeshRenderer renderer;public Vector3[] vertices;}
        readonly Surface[] surfaces=new Surface[Layers];
        CasterPressure3DFlowProfile profile;
        Transform owner;Vector3 direction;float releaseTime;bool released,disposed;
        public Vector3 Anchor {get;private set;}
        public float Weight {get;private set;}
        public float ForwardWeight {get;private set;}
        public bool Visible=>!disposed && surfaces[0]!=null && surfaces[0].renderer!=null && surfaces[0].renderer.enabled;
        public string Phase {get;private set;}="Charge";
        public void Configure(Transform caster,Vector3 heading,float release,CasterPressure3DFlowProfile settings)
        {
            owner=caster;direction=heading.normalized;releaseTime=release;profile=settings;Anchor=caster.position;
            for(int l=0;l<Layers;l++)
            {
                var s=new Surface();surfaces[l]=s;
                s.mesh=new Mesh{name="PurpleIngredient3DFlow_"+l};s.mesh.MarkDynamic();
                s.material=new Material(Resources.Load<Shader>("VFX/CasterPressure3DFlow")){name="PurpleIngredient3DFlow_"+l};
                s.material.SetFloat("_Role",l);
                s.vertices=new Vector3[(Steps+1)*Across];var uv=new Vector2[s.vertices.Length];var tris=new int[Steps*(Across-1)*6];int at=0;
                for(int n=0;n<=Steps;n++)for(int j=0;j<Across;j++)
                {
                    int k=n*Across+j;uv[k]=new Vector2(n/(float)Steps,j/(float)(Across-1));
                    if(n<Steps && j<Across-1){tris[at++]=k;tris[at++]=k+Across;tris[at++]=k+1;tris[at++]=k+1;tris[at++]=k+Across;tris[at++]=k+Across+1;}
                }
                s.mesh.vertices=s.vertices;s.mesh.uv=uv;s.mesh.triangles=tris;
                var go=new GameObject(l==0?"Outer diffuse pressure":l==1?"Inner compressed pressure":"Local sharp crest");go.transform.SetParent(transform,false);
                go.AddComponent<MeshFilter>().sharedMesh=s.mesh;s.renderer=go.AddComponent<MeshRenderer>();s.renderer.sharedMaterial=s.material;
                s.renderer.shadowCastingMode=ShadowCastingMode.Off;s.renderer.receiveShadows=false;s.renderer.enabled=false;
            }
        }
        // Catmull-Rom path through artist-facing controls. Density travels along it; the path does not orbit the caster.
        public Vector3 PathPoint(float u)
        {
            var p=profile.path;if(p==null || p.Length<2)return Vector3.zero;
            float f=Mathf.Clamp01(u)*(p.Length-1);int i=Mathf.Min(Mathf.FloorToInt(f),p.Length-2);float t=f-i;
            Vector3 a=p[Mathf.Max(0,i-1)],b=p[i],c=p[i+1],d=p[Mathf.Min(p.Length-1,i+2)];
            return .5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);
        }
        public void Sample(float clock)
        {
            if(disposed)return;
            if(owner==null || profile==null){foreach(var s in surfaces)if(s!=null)s.renderer.enabled=false;return;}
            float age=clock-releaseTime;
            if(age<0){released=false;Anchor=owner.position;}else if(!released){Anchor=owner.position;released=true;}
            transform.SetPositionAndRotation(Anchor,Quaternion.LookRotation(direction));
            float q=Mathf.Clamp01(age/Mathf.Max(.001f,profile.releaseSeconds));
            Weight=age<0?Mathf.SmoothStep(0,1,clock/.22f):1-Mathf.SmoothStep(0,1,q);
            ForwardWeight=age<0?Mathf.SmoothStep(0,1,Mathf.InverseLerp(releaseTime-.18f,releaseTime,clock))*.32f:Mathf.Sin(q*Mathf.PI);
            Phase=age<0?"Charge":q<1?"Release":"Ended";
            float time=clock*profile.flowSpeed;
            for(int l=0;l<Layers;l++)
            {
                var s=surfaces[l];s.renderer.enabled=Weight>.001f;if(!s.renderer.enabled)continue;
                for(int n=0;n<=Steps;n++)
                {
                    float u=n/(float)Steps;Vector3 c=PathPoint(u);
                    Vector3 tangent=(PathPoint(Mathf.Min(1,u+.012f))-PathPoint(Mathf.Max(0,u-.012f))).normalized;
                    Vector3 side=Vector3.Cross(tangent,Vector3.up).normalized;
                    Vector3 lift=Vector3.Cross(side,tangent).normalized;
                    float compression=.7f+.28f*Mathf.Sin(u*9+time*1.7f)+.16f*Mathf.Sin(u*17-time*2.4f);
                    // Each open section has a different cross curvature and interior position, not concentric copies.
                    if(l==1)c+=side*(.23f*Mathf.Sin(u*11-time*2.1f))+lift*(.25f*Mathf.Sin(u*8+1.2f));
                    for(int j=0;j<Across;j++)
                    {
                        float v=j/(float)(Across-1),x=v-.5f;
                        float theta=(v-.5f)*(l==0?3.7f:l==1?4.5f:.28f)+u*(l==0?1.4f:l==1?-2.1f:1.4f);
                        float w=profile.breadth*compression*(l==0?1:l==1?.52f:.18f);
                        float d=profile.depth*(l==0?1:l==1?.7f:.15f);
                        // Open crescent sections, unsealed and irregular; not intersecting flat cards or a solid hose.
                        Vector3 p=c+side*(Mathf.Sin(theta)*w)+lift*(Mathf.Cos(theta)*d);
                        p+=lift*(.11f*Mathf.Sin(u*13+v*4-time*2.3f))*(l<2?1:.2f);
                        if(l==2)p+=lift*profile.depth*.7f;
                        p.z+=ForwardWeight*profile.forwardSweep*(.35f+u);p.x*=1-ForwardWeight*.22f;
                        s.vertices[n*Across+j]=p;
                    }
                }
                s.mesh.vertices=s.vertices;s.mesh.RecalculateBounds();s.mesh.RecalculateNormals();
                s.material.SetFloat("_Clock",time);s.material.SetFloat("_Weight",Weight);s.material.SetFloat("_Density",profile.density);s.material.SetFloat("_Crest",profile.crestEmission);
            }
        }
        public void Dispose()
        {
            if(disposed)return;disposed=true;
            foreach(var s in surfaces)if(s!=null){if(s.renderer!=null)s.renderer.enabled=false;Release(s.material);Release(s.mesh);}
        }
        static void Release(Object obj){if(obj==null)return;if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
        void OnDestroy()=>Dispose();
    }
}
