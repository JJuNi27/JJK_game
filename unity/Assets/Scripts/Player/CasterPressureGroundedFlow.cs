using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // A single pressure stream. Its visible sections traverse the feet-relative guide independently;
    // the guide is never rendered as a complete permanent S-shaped object.
    public sealed class CasterPressureGroundedFlow : MonoBehaviour
    {
        const int Steps=48,Across=13,Layers=3;
        sealed class Surface {public Mesh mesh;public Material material;public MeshRenderer renderer;public Vector3[] vertices;}
        readonly Surface[] surfaces=new Surface[Layers];
        CasterPressureGroundedFlowProfile profile;
        Transform owner;CharacterController controller;Vector3 direction;float fusionStart,holdStart,releaseTime;
        bool released,disposed;
        public Vector3 Anchor {get;private set;}
        public float Weight {get;private set;}
        public float ForwardWeight {get;private set;}
        public float StageDensity {get;private set;}
        public bool Visible=>!disposed && surfaces[0]!=null && surfaces[0].renderer!=null && surfaces[0].renderer.enabled;
        public string Phase {get;private set;}="Charge";
        static float H(float n)=>Mathf.Repeat(Mathf.Sin(n*127.1f+31.7f)*43758.5453f,1);
        Vector3 Feet=>owner.position+owner.TransformVector(Vector3.up*(controller!=null?controller.center.y-controller.height*.5f:profile.fallbackFootOffset));
        public void Configure(Transform caster,Vector3 heading,float fusion,float hold,float release,CasterPressureGroundedFlowProfile settings)
        {
            owner=caster;controller=caster.GetComponent<CharacterController>();direction=heading.normalized;
            fusionStart=fusion;holdStart=hold;releaseTime=release;profile=settings;Anchor=Feet;
            for(int l=0;l<Layers;l++)
            {
                var s=new Surface();surfaces[l]=s;
                s.mesh=new Mesh{name="PurpleIngredientGroundedFlow_"+l};s.mesh.MarkDynamic();
                s.material=new Material(Resources.Load<Shader>("VFX/CasterPressureGroundedFlow")){name="PurpleIngredientGroundedFlow_"+l};s.material.SetFloat("_Role",l);
                s.vertices=new Vector3[(Steps+1)*Across];var uv=new Vector2[s.vertices.Length];var tris=new int[Steps*(Across-1)*6];int at=0;
                for(int n=0;n<=Steps;n++)for(int j=0;j<Across;j++)
                {
                    int k=n*Across+j;uv[k]=new Vector2(n/(float)Steps,j/(float)(Across-1));
                    if(n<Steps && j<Across-1){tris[at++]=k;tris[at++]=k+Across;tris[at++]=k+1;tris[at++]=k+1;tris[at++]=k+Across;tris[at++]=k+Across+1;}
                }
                s.mesh.vertices=s.vertices;s.mesh.uv=uv;s.mesh.triangles=tris;
                var go=new GameObject(l==0?"Broad lower pressure":l==1?"Near-body compression":"Fast local crest");go.transform.SetParent(transform,false);
                go.AddComponent<MeshFilter>().sharedMesh=s.mesh;s.renderer=go.AddComponent<MeshRenderer>();s.renderer.sharedMaterial=s.material;
                s.renderer.shadowCastingMode=ShadowCastingMode.Off;s.renderer.receiveShadows=false;s.renderer.enabled=false;
            }
        }
        public Vector3 PathPoint(float u)
        {
            var p=profile.path;if(p==null || p.Length<2)return Vector3.zero;
            float f=Mathf.Clamp01(u)*(p.Length-1);int i=Mathf.Min(Mathf.FloorToInt(f),p.Length-2);float t=f-i;
            Vector3 a=p[Mathf.Max(0,i-1)],b=p[i],c=p[i+1],d=p[Mathf.Min(p.Length-1,i+2)];
            return .5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);
        }
        void Packet(int role,float clock,out float age,out float seed,out float start,out float span,out float energy)
        {
            float duration=role==0?profile.broadSeconds:role==1?profile.innerSeconds:profile.crestSeconds;
            float tick=clock/Mathf.Max(.01f,duration)+(role==0?0:role==1?.43f:.17f);
            float epoch=Mathf.Floor(tick);age=Mathf.Repeat(tick,1);seed=epoch*37+role*71;
            // Broad density stays present while shorter local sections are replaced at a different cadence.
            energy=role==0?.58f+.42f*Mathf.Sin(age*Mathf.PI):Mathf.SmoothStep(0,1,age/.09f)*(1-Mathf.SmoothStep(.73f,1,age));
            span=role==0?.43f+H(seed+3)*.12f:role==1?.22f+H(seed+3)*.12f:.14f+H(seed+3)*.075f;
            float progression=role==0?Mathf.Pow(age,.72f):role==1?Mathf.Pow(age,.55f):age;
            start=Mathf.Lerp(.015f,.98f-span,progression);
            if(role==2)
            {
                float innerAge,innerSeed,innerStart,innerSpan,innerEnergy;
                Packet(1,clock,out innerAge,out innerSeed,out innerStart,out innerSpan,out innerEnergy);
                start=Mathf.Clamp01(innerStart+innerSpan*(.1f+H(seed+13)*.6f));
            }
        }
        public void Sample(float clock)
        {
            if(disposed)return;
            if(owner==null || profile==null){foreach(var s in surfaces)if(s!=null)s.renderer.enabled=false;return;}
            float age=clock-releaseTime;
            if(age<0){released=false;Anchor=Feet;}else if(!released){Anchor=Feet;released=true;}
            transform.SetPositionAndRotation(Anchor,Quaternion.LookRotation(direction));
            float q=Mathf.Clamp01(age/Mathf.Max(.001f,profile.releaseSeconds));
            Weight=age<0?Mathf.SmoothStep(0,1,clock/.22f):1-Mathf.SmoothStep(0,1,q);
            ForwardWeight=age<0?Mathf.SmoothStep(0,1,Mathf.InverseLerp(releaseTime-.18f,releaseTime,clock))*.32f:Mathf.Sin(q*Mathf.PI);
            Phase=age<0?"Charge":q<1?"Release":"Ended";
            float gathering=Mathf.SmoothStep(0,1,clock/Mathf.Max(.01f,fusionStart));
            float fuse=Mathf.Exp(-Mathf.Pow((clock-(fusionStart+holdStart)*.5f)/Mathf.Max(.01f,(holdStart-fusionStart)*.55f),2));
            StageDensity=Mathf.Lerp(.7f,1,gathering)*Mathf.Lerp(1,profile.fusionDensityBoost,fuse);
            for(int l=0;l<Layers;l++)
            {
                var s=surfaces[l];s.renderer.enabled=Weight>.001f;if(!s.renderer.enabled)continue;
                float packetAge,seed,start,span,energy;Packet(l,clock,out packetAge,out seed,out start,out span,out energy);
                for(int n=0;n<=Steps;n++)
                {
                    float u=n/(float)Steps,guide=Mathf.Clamp01(start+u*span);
                    Vector3 c=PathPoint(guide);
                    Vector3 tangent=(PathPoint(Mathf.Min(1,guide+.012f))-PathPoint(Mathf.Max(0,guide-.012f))).normalized;
                    Vector3 side=Vector3.Cross(tangent,Vector3.up).normalized,lift=Vector3.Cross(side,tangent).normalized;
                    // The section's actual route/attachment changes between packets, beyond shader flicker.
                    float localBend=Mathf.Sin(u*Mathf.PI)*(.13f+H(seed+11)*.18f);
                    c+=side*localBend*(H(seed+19)*2-1);
                    if(l==1)c=Vector3.Lerp(c,new Vector3(c.x*.62f,c.y*.74f,c.z*.62f),.65f);
                    float compression=.72f+.2f*Mathf.Sin(u*8-packetAge*9+seed)+.1f*Mathf.Sin(u*17+packetAge*12);
                    for(int j=0;j<Across;j++)
                    {
                        float v=j/(float)(Across-1);
                        float theta=(v-.5f)*(l==0?2.9f:l==1?3.8f:.32f)+u*(l==0?.8f:l==1?-1.3f:.8f);
                        float w=profile.breadth*compression*(l==0?1:l==1?.72f:.24f);
                        float d=profile.depth*(l==0?1:l==1?.72f:.18f);
                        Vector3 p=c+side*(Mathf.Sin(theta)*w)+lift*(Mathf.Cos(theta)*d);
                        p.y+=.07f*Mathf.Sin(u*13+v*4-packetAge*11+seed)*(l<2?1:.2f);
                        // Limit ground penetration without flattening the layer onto a floor card.
                        p.y=.04f+.5f*(p.y+Mathf.Sqrt(p.y*p.y+.025f));
                        p.z+=ForwardWeight*profile.forwardSweep*(.35f+guide);p.x*=1-ForwardWeight*.22f;
                        s.vertices[n*Across+j]=p;
                    }
                }
                s.mesh.vertices=s.vertices;s.mesh.RecalculateBounds();s.mesh.RecalculateNormals();
                s.material.SetFloat("_Clock",clock);s.material.SetFloat("_Seed",H(seed+17)*31);
                s.material.SetFloat("_Weight",Weight*energy*StageDensity);s.material.SetFloat("_Density",profile.density);s.material.SetFloat("_Crest",profile.crestEmission);
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
