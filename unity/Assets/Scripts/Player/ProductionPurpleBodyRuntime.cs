using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    public sealed class ProductionPurpleBodyRuntime : MonoBehaviour
    {
        private const int ArcCount=8,Rings=32,Sides=7;
        private ProductionPurpleBodySettings profile;
        private Material bodyMaterial,arcMaterial;
        private Mesh arcMesh;
        private float diameter,unit;
        private readonly Vector4[] sources=new Vector4[6];
        private static readonly Vector3[] Anchors={new Vector3(-.105f,.075f,-.075f),new Vector3(.11f,-.065f,.055f),new Vector3(.045f,.11f,.13f)};
        private readonly Vector3[][] paths=new Vector3[ArcCount][];
        private readonly Vector3[] guide=new Vector3[6];
        private readonly Vector3[] vertices=new Vector3[ArcCount*Rings*Sides],normals=new Vector3[ArcCount*Rings*Sides];
        private readonly Color[] colours=new Color[ArcCount*Rings*Sides];
        public void Configure(ProductionPurpleBodySettings settings,float bodyDiameter)
        {
            profile=settings;diameter=bodyDiameter;unit=diameter/.75f;
            var proxy=GameObject.CreatePrimitive(PrimitiveType.Cube);proxy.name="R3_TornPlasmaVolume";proxy.transform.SetParent(transform,false);proxy.transform.localScale=Vector3.one*unit;
            var collider=proxy.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            bodyMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R3")){name="PurpleProduction_Body_Runtime"};
            var renderer=proxy.GetComponent<Renderer>();renderer.sharedMaterial=bodyMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            bodyMaterial.SetVector("_R3Settings",new Vector4(settings.shellEmission,settings.shellDensity,settings.internalEnergy,settings.instability));
            bodyMaterial.SetVector("_R3Flow",new Vector4(settings.inwardSpeed,settings.turbulenceSpeed,settings.voidContrast,settings.plasmaScale));
            var arcs=new GameObject("R3_VolumetricArcMesh");arcs.transform.SetParent(transform,false);
            arcMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R3Arc")){name="PurpleProduction_Arc_Runtime"};
            arcMesh=new Mesh{name="PurpleProduction_TubeArcs_Runtime"};arcMesh.MarkDynamic();arcs.AddComponent<MeshFilter>().sharedMesh=arcMesh;
            var meshRenderer=arcs.AddComponent<MeshRenderer>();meshRenderer.sharedMaterial=arcMaterial;meshRenderer.shadowCastingMode=ShadowCastingMode.Off;meshRenderer.receiveShadows=false;
            var uv=new Vector2[vertices.Length];var indices=new int[ArcCount*(Rings-1)*Sides*6];int at=0;
            for(int arc=0;arc<ArcCount;arc++)
            {
                paths[arc]=new Vector3[Rings];
                for(int ring=0;ring<Rings;ring++)for(int side=0;side<Sides;side++)
                {
                    int v=(arc*Rings+ring)*Sides+side;uv[v]=new Vector2(ring/(float)(Rings-1),side/(float)Sides);
                    if(ring==Rings-1)continue;int next=(arc*Rings+ring)*Sides+(side+1)%Sides;
                    indices[at++]=v;indices[at++]=next;indices[at++]=v+Sides;indices[at++]=next;indices[at++]=next+Sides;indices[at++]=v+Sides;
                }
            }
            arcMesh.vertices=vertices;arcMesh.normals=normals;arcMesh.colors=colours;arcMesh.uv=uv;arcMesh.triangles=indices;
            Render(1.5f);
        }
        private static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+31.7f)*43758.5453f,1f);
        private static Vector3 Direction(float seed){float y=Hash(seed+5)*1.7f-.85f,a=Hash(seed+7)*Mathf.PI*2,s=Mathf.Sqrt(1-y*y);return new Vector3(Mathf.Cos(a)*s,y,Mathf.Sin(a)*s);}
        private void Path(int arc,float seed,float t)
        {
            for(int n=0;n<Rings;n++)
            {
                float u=n/(float)(Rings-1),segment=u*5;int j=Mathf.Min(4,Mathf.FloorToInt(segment));float f=segment-j;
                Vector3 p=Vector3.Lerp(guide[j],guide[j+1],f);
                // Jagged local discharge; each event has its own seed and short restrikes.
                Vector3 ripple=Vector3.Lerp(Direction(seed+j*13),Direction(seed+(j+1)*13),f)
                    *(.014f+.004f*Mathf.Sin(t*53+j))*Mathf.Sin(u*Mathf.PI);
                paths[arc][n]=p+ripple;
            }
        }
        private void Tube(int arc,float width,float power,float t)
        {
            var path=paths[arc];Vector3 previous=Vector3.up;
            for(int n=0;n<Rings;n++)
            {
                Vector3 tangent=(path[Mathf.Min(n+1,Rings-1)]-path[Mathf.Max(0,n-1)]).normalized;
                Vector3 across=Vector3.ProjectOnPlane(previous,tangent).normalized;if(across.sqrMagnitude<.1f)across=Vector3.Cross(tangent,Vector3.right).normalized;
                Vector3 second=Vector3.Cross(tangent,across).normalized;previous=across;
                float u=n/(float)(Rings-1),radius=width*(.22f+.78f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(Mathf.PI*u)),.45f))*(.6f+.4f*Mathf.Abs(Mathf.Sin(n*1.6f+t*53+arc)));
                for(int side=0;side<Sides;side++)
                {
                    float a=side*Mathf.PI*2/Sides;Vector3 normal=across*Mathf.Cos(a)+second*Mathf.Sin(a);int v=(arc*Rings+n)*Sides+side;
                    vertices[v]=(path[n]+normal*radius)*unit;normals[v]=normal;colours[v]=new Color(1,arc<6?1:0,1,power);
                }
            }
        }
        private void Event(int lane,float time,out float age,out float life,out float seed)
        {
            float birth=lane*.097f;int index=0;
            // Nonuniform gaps are deterministic for comparison scrubbing, independent of render frame rate.
            while(index<64)
            {
                float next=birth+.19f+Hash(index*17+lane*59+2)*.27f;
                if(next>time)break;birth=next;index++;
            }
            seed=index*41+lane*67;life=profile.arcLifetime*(.75f+Hash(seed+8)*.65f);age=time-birth;
        }
        public void Render(float t)
        {
            if(bodyMaterial==null)return;
            sources[0]=new Vector4(-.023f,.008f+.018f*Mathf.Sin(t*2.7f),-.025f,.063f);
            sources[1]=new Vector4(.038f*Mathf.Cos(t*2.3f),-.023f,.027f,.052f);
            sources[2]=new Vector4(.015f,.042f*Mathf.Cos(t*3.1f),.002f,.042f);
            var primary=new Vector4();var secondary=new Vector4();
            for(int i=0;i<6;i++)
            {
                if(i>=3){Vector3 p=Quaternion.Euler(0,t*(i%2==0?-13:9),t*(i==5?7:-5))*Anchors[i-3];sources[i]=new Vector4(p.x,p.y,p.z,.041f+(i==4?.006f:0));}
                float spike=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*(7.3f+i*.93f)+i*2.2f)*Mathf.Cos(t*2.17f+i)),8);
                float power=(i<3?.8f:.22f)+profile.instability*(spike*(i<3?1.1f:1.4f)+.15f*Mathf.Sin(t*(2.7f+i*.71f)+i*1.8f));
                if(i<3)primary[i]=power;else secondary[i-3]=power;
            }
            bodyMaterial.SetVectorArray("_Sources",sources);bodyMaterial.SetVector("_PrimaryPower",primary);bodyMaterial.SetVector("_SecondaryPower",secondary);bodyMaterial.SetFloat("_PhaseTime",t);
            arcMaterial.SetVector("_BodyCentre",new Vector4(transform.position.x,transform.position.y,transform.position.z,diameter*.5f*transform.lossyScale.x));arcMaterial.SetFloat("_PhaseTime",t);
            for(int lane=0;lane<3;lane++)
            {
                Event(lane,t,out float age,out float life,out float seed);
                float phase=age/life;
                float pulse=phase<0 || phase>1?0:Mathf.Clamp01(phase/.08f)*Mathf.Pow(1-phase,.65f);
                // New spatial direction, length and branch for every birth. Some roots stay behind the shell.
                Vector3 a=Direction(seed+1),axis=Direction(seed+3),b=(a+axis*.6f).normalized;
                guide[0]=sources[lane==0?0:lane+2];guide[1]=Vector3.Lerp(guide[0],a*.31f,.55f)+axis*.045f;guide[2]=a*.365f;
                guide[3]=Vector3.Slerp(a,b,.48f)*(.37f+Hash(seed+10)*.055f);
                guide[4]=b*(.38f+Hash(seed+11)*.06f);guide[5]=(b+axis*.2f).normalized*.375f*profile.outerReach;
                float growth=Mathf.Clamp01(age/.024f);
                for(int n=1;n<6;n++)guide[n]=Vector3.Lerp(guide[0],guide[n],growth);
                Path(lane*2,seed,t);float width=profile.arcRadius/5*.75f*(.65f+Hash(seed+4)*.65f)*(.65f+pulse*.6f);
                Tube(lane*2,width,pulse,t);
                Vector3 start=paths[lane*2][16],end=(b+Direction(seed+21)*.65f).normalized*(.4f+Hash(seed+9)*.10f);
                for(int n=0;n<6;n++){float u=n/5f;guide[n]=Vector3.Lerp(start,end,u)+Direction(seed+n*7)*Mathf.Sin(u*Mathf.PI)*.055f;}
                Path(lane*2+1,seed+71,t);Tube(lane*2+1,width*.35f,pulse*.75f,t);
            }
            for(int i=6;i<8;i++)
            {
                Event(i,t,out float age,out float life,out float seed);float phase=Mathf.Clamp01(age/life);Vector3 d=Direction(seed);
                for(int n=0;n<Rings;n++)paths[i][n]=d*(.38f+phase*.11f+n/(float)(Rings-1)*.035f);
                Tube(i,.0018f,age<0 || age>life?0:Mathf.Sin(phase*Mathf.PI)*.6f,t);
            }
            arcMesh.vertices=vertices;arcMesh.normals=normals;arcMesh.colors=colours;arcMesh.RecalculateBounds();
        }
        private void OnDestroy(){if(bodyMaterial!=null)Destroy(bodyMaterial);if(arcMaterial!=null)Destroy(arcMaterial);if(arcMesh!=null)Destroy(arcMesh);}
    }
}
