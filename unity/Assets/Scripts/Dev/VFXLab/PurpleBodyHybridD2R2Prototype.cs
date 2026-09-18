using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Dev.PurpleBodyExploration
{
    public sealed class PurpleBodyHybridD2R2Prototype : MonoBehaviour
    {
        private const int ArcCount=8,Rings=32,Sides=7;
        private PurpleBodyHybridD2R2Profile profile;
        private Material bodyMaterial,arcMaterial;
        private Mesh arcMesh;
        private float diameter,unit;
        private readonly Vector4[] sources=new Vector4[6];
        private static readonly Vector3[] Anchors={new Vector3(-.15f,.10f,-.085f),new Vector3(.14f,-.11f,.06f),new Vector3(.05f,.15f,.16f)};
        private readonly Vector3[][] paths=new Vector3[ArcCount][];
        private readonly Vector3[] guide=new Vector3[6];
        private readonly Vector3[] vertices=new Vector3[ArcCount*Rings*Sides],normals=new Vector3[ArcCount*Rings*Sides];
        private readonly Color[] colours=new Color[ArcCount*Rings*Sides];
        public void Configure(PurpleBodyHybridD2R2Profile settings)
        {
            profile=settings;diameter=settings.frozenR1.frozenD2.preservedD.commonConditions.bodyDiameter;unit=diameter/.75f;
            var proxy=GameObject.CreatePrimitive(PrimitiveType.Cube);proxy.name="R2_TornPlasmaVolume";proxy.transform.SetParent(transform,false);proxy.transform.localScale=Vector3.one*unit;
            var collider=proxy.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            bodyMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R2")){name="PurpleHybridD2R2_Body_Runtime"};
            var renderer=proxy.GetComponent<Renderer>();renderer.sharedMaterial=bodyMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            bodyMaterial.SetVector("_R2Settings",new Vector4(settings.shellEmission,settings.shellDensity,settings.internalEnergy,settings.instability));
            bodyMaterial.SetVector("_R2Flow",new Vector4(settings.inwardSpeed,settings.turbulenceSpeed,settings.voidContrast,0));
            var arcs=new GameObject("R2_VolumetricArcMesh");arcs.transform.SetParent(transform,false);
            arcMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R2Arc")){name="PurpleHybridD2R2_Arc_Runtime"};
            arcMesh=new Mesh{name="R2_TubeArcs_Runtime"};arcMesh.MarkDynamic();arcs.AddComponent<MeshFilter>().sharedMesh=arcMesh;
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
                Vector3 a=guide[Mathf.Max(0,j-1)],b=guide[j],c=guide[j+1],d=guide[Mathf.Min(5,j+2)];
                Vector3 p=.5f*((2*b)+(-a+c)*f+(2*a-5*b+4*c-d)*f*f+(-a+3*b-3*c+d)*f*f*f);
                Vector3 ripple=Direction(seed+n*3)*(.012f+.007f*Mathf.Sin(t*21+n))*Mathf.Sin(u*Mathf.PI);
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
                float u=n/(float)(Rings-1),radius=width*(.35f+.65f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(Mathf.PI*u)),.45f))*(.7f+.3f*Mathf.Sin(n*1.6f+t*19+arc));
                for(int side=0;side<Sides;side++)
                {
                    float a=side*Mathf.PI*2/Sides;Vector3 normal=across*Mathf.Cos(a)+second*Mathf.Sin(a);int v=(arc*Rings+n)*Sides+side;
                    vertices[v]=(path[n]+normal*radius)*unit;normals[v]=normal;colours[v]=new Color(1,arc<6?1:0,1,power);
                }
            }
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
                if(i>=3){Vector3 p=Quaternion.Euler(0,t*(i%2==0?-13:9),t*(i==5?7:-5))*Anchors[i-3];sources[i]=new Vector4(p.x,p.y,p.z,.042f+(i==4?.01f:0));}
                float spike=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*(7.3f+i*.93f)+i*2.2f)*Mathf.Cos(t*2.17f+i)),8);
                float power=(i<3?.8f:.65f)+profile.instability*(spike*.9f+.22f*Mathf.Sin(t*(2.7f+i*.71f)+i*1.8f));
                if(i<3)primary[i]=power;else secondary[i-3]=power;
            }
            bodyMaterial.SetVectorArray("_Sources",sources);bodyMaterial.SetVector("_PrimaryPower",primary);bodyMaterial.SetVector("_SecondaryPower",secondary);bodyMaterial.SetFloat("_PhaseTime",t);
            arcMaterial.SetVector("_BodyCentre",new Vector4(transform.position.x,transform.position.y,transform.position.z,diameter*.5f));arcMaterial.SetFloat("_PhaseTime",t);
            for(int lane=0;lane<3;lane++)
            {
                float epoch=t/(.43f+lane*.137f)+lane*.29f,phase=Mathf.Repeat(epoch,1),seed=Mathf.Floor(epoch)*19+lane*37;
                float pulse=Mathf.SmoothStep(0,1,Mathf.Clamp01(phase/.09f))*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-.52f)/.40f)));
                // Author front / rear / flank lanes in body space; geometry never turns to face the camera.
                Vector3 a=lane==0?new Vector3(-.55f,.35f,-.78f):lane==1?new Vector3(.55f,.35f,.75f):new Vector3(.85f,-.35f,-.3f);
                a=(a+Direction(seed)*.17f).normalized;
                Vector3 axis=(Vector3.up+Direction(seed+3)*.45f).normalized,b=Quaternion.AngleAxis(40+Hash(seed+2)*45,axis)*a;
                guide[0]=sources[lane==0?0:lane+2];guide[1]=Vector3.Lerp(guide[0],a*.35f,.55f);guide[2]=a*.374f;
                guide[3]=Vector3.Slerp(a,b,.53f)*.383f;guide[4]=b*.385f;guide[5]=(b+axis*.25f).normalized*.375f*profile.outerReach;
                Path(lane*2,seed,t);Tube(lane*2,profile.arcRadius/5*.75f,pulse,t);
                Vector3 start=paths[lane*2][16],end=Direction(seed+21)*.46f;
                for(int n=0;n<6;n++){float u=n/5f;guide[n]=Vector3.Lerp(start,end,u)+axis*Mathf.Sin(u*Mathf.PI)*.08f;}
                Path(lane*2+1,seed+71,t);Tube(lane*2+1,profile.arcRadius/5*.38f,pulse*.7f,t);
            }
            for(int i=6;i<8;i++)
            {
                float epoch=t/(.38f+i*.047f)+i*.31f,phase=Mathf.Repeat(epoch,1);Vector3 d=Direction(Mathf.Floor(epoch)*13+i*47);
                for(int n=0;n<Rings;n++)paths[i][n]=d*(.38f+phase*.11f+n/(float)(Rings-1)*.035f);
                Tube(i,.0022f,Mathf.Sin(phase*Mathf.PI)*.8f,t);
            }
            arcMesh.vertices=vertices;arcMesh.normals=normals;arcMesh.colors=colours;arcMesh.RecalculateBounds();
        }
        private void OnDestroy(){if(bodyMaterial!=null)Destroy(bodyMaterial);if(arcMaterial!=null)Destroy(arcMaterial);if(arcMesh!=null)Destroy(arcMesh);}
    }
}
