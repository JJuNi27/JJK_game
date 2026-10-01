using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>Opt-in outer-energy exploration. It owns only its short-lived meshes and never writes formation transforms.</summary>
    public sealed class PurpleIngredientOuterExplorationVolume : MonoBehaviour
    {
        private const int Groups=4,Sides=8,MainRings=8,BranchRings=4,Vertices=(MainRings+BranchRings)*Sides;
        private readonly Mesh[] meshes=new Mesh[Groups];
        private readonly MeshRenderer[] renderers=new MeshRenderer[Groups];
        private readonly Vector3[] vertices=new Vector3[Vertices],path=new Vector3[MainRings];
        private readonly Color[] colors=new Color[Vertices];
        private readonly Vector2[] uv=new Vector2[Vertices];
        private Material material;
        private bool blue;
        private float reach,depth,violence,rate;
        private static float H(float v)=>Mathf.Repeat(Mathf.Sin(v*127.1f+19.3f)*43758.5453f,1);
        public static float Weight(float fusion)=>1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.72f,fusion));

        public void Configure(bool inward,float energyReach,float frontBackDepth,float burstViolence,float eventRate)
        {
            blue=inward;reach=energyReach;depth=frontBackDepth;violence=burstViolence;rate=eventRate;
            material=new Material(Resources.Load<Shader>("VFX/PurpleIngredientOuterExploration")){name="PurpleIngredientOuterExploration_Runtime"};
            material.SetColor("_Color",blue?new Color(.08f,.72f,5.8f):new Color(6.8f,.08f,.24f));
            material.SetFloat("_Violence",violence);
            var triangles=new int[((MainRings-1)+(BranchRings-1))*Sides*6];int t=0;
            for(int tube=0;tube<2;tube++)
            {
                int start=tube==0?0:MainRings,rings=tube==0?MainRings:BranchRings;
                for(int j=0;j<rings-1;j++)for(int k=0;k<Sides;k++)
                {
                    int a=(start+j)*Sides+k,b=(start+j)*Sides+(k+1)%Sides,c=a+Sides,d=b+Sides;
                    triangles[t++]=a;triangles[t++]=b;triangles[t++]=c;triangles[t++]=b;triangles[t++]=d;triangles[t++]=c;
                }
            }
            for(int i=0;i<Groups;i++)
            {
                var go=new GameObject("IngredientOuterExploration_"+i);go.transform.SetParent(transform,false);
                var mesh=new Mesh{name="PurpleIngredientOuterExploration_Mesh"};mesh.MarkDynamic();mesh.vertices=vertices;mesh.triangles=triangles;
                meshes[i]=mesh;go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.enabled=false;renderers[i]=r;
            }
        }

        public void Sample(float clock,float fusion,float dischargeCoupling=0,Vector3 dischargeDirection=default)
        {
            float weight=Weight(fusion),coupling=Mathf.Clamp01(dischargeCoupling/2.7f);material.SetFloat("_PhaseTime",clock);
            for(int group=0;group<Groups;group++)
            {
                float tick=clock*rate+(blue?group*.27f:group*.31f),epoch=Mathf.Floor(tick),phase=Mathf.Repeat(tick,1);
                float seed=epoch*23+group*13,life=.86f+.09f*H(seed+4),p=Mathf.Clamp01(phase/life);
                float fade=Mathf.SmoothStep(0,1,p/.075f)*(1-Mathf.SmoothStep(.70f,1,p))*weight;
                renderers[group].enabled=fade>.004f;if(!renderers[group].enabled)continue;
                float growth=Mathf.SmoothStep(0,1,Mathf.Clamp01(p/.52f));
                float collapse=Mathf.SmoothStep(.55f,1,p);
                float baseAngle=group*Mathf.PI*.5f+.35f+(H(seed)-.5f)*1.25f;
                Quaternion plane=Quaternion.Euler(28+H(seed+2)*130,group*43+H(seed+8)*90,group*57+H(seed+5)*160);
                for(int j=0;j<MainRings;j++)
                {
                    float u=j/(float)(MainRings-1),knot=u*4.5f,k=Mathf.Floor(knot),f=knot-k;
                    float jag=Mathf.Lerp(H(seed+k*3),H(seed+(k+1)*3),f)-.5f;
                    float wave=Mathf.Sin(u*5.7f+p*(blue?9f:11f)+seed*.23f);
                    float radial=blue?Mathf.Lerp(reach*1.16f,.55f,p):Mathf.Lerp(.62f,reach*1.28f,p);
                    radial+=wave*.22f*violence*growth+jag*.36f*violence*growth+coupling*(.32f+u*.46f);
                    float angle=baseAngle+(blue?p*1.5f:-p*.55f)-u*(blue?.88f:.48f)+jag*.65f;
                    float z=Mathf.Lerp(-depth,depth,H(seed+17+j*2)) + Mathf.Sin(p*8+u*7+seed)*depth*.38f;
                    Vector3 local=plane*new Vector3(Mathf.Cos(angle)*radial,Mathf.Sin(angle)*radial,z);
                    float pinch=1-Mathf.SmoothStep(.58f,1,p)*.52f;
                    path[j]=local*pinch*(1+coupling*.13f);
                }
                for(int j=0;j<MainRings;j++)
                {
                    float u=j/(float)(MainRings-1);
                    Vector3 tangent=path[Mathf.Min(j+1,MainRings-1)]-path[Mathf.Max(0,j-1)];
                    float spatial=dischargeDirection.sqrMagnitude>.001f?Mathf.Pow(Mathf.Clamp01(Vector3.Dot(path[j].normalized,dischargeDirection.normalized)),10):0;
                    float radius=.15f*(.74f+H(seed+j*5)*.45f)*Mathf.Sin(Mathf.PI*(.18f+.82f*u))*(.72f+.28f*growth)*(1+coupling*.62f+spatial*coupling*.75f);
                    if(j==0)radius*=.55f;
                    Ring(j,path[j],tangent,radius,u,fade*(1+coupling*.68f+spatial*coupling*.55f),seed);
                }
                Vector3 branchStart=path[2],branchDirection=(path[2]-path[5]).normalized;
                Vector3 bend=Vector3.Cross(branchDirection,plane*Vector3.forward).normalized*(H(seed+19)>.5f?1:-1);
                for(int j=0;j<BranchRings;j++)
                {
                    float u=j/(float)(BranchRings-1);
                    Vector3 centre=branchStart+branchDirection*u*(.55f+.35f*growth)+bend*(u*.48f+Mathf.Sin(u*Mathf.PI+p*7)*.12f);
                    Ring(MainRings+j,centre,branchDirection+bend*.72f,.07f*(1-u)*(.8f+.4f*growth)*(1+coupling*.5f),u,fade*(.9f+coupling*.65f),seed+5);
                }
                meshes[group].vertices=vertices;meshes[group].colors=colors;meshes[group].uv=uv;meshes[group].RecalculateNormals();meshes[group].RecalculateBounds();
            }
        }

        private void Ring(int ring,Vector3 centre,Vector3 tangent,float radius,float u,float fade,float seed)
        {
            if(tangent.sqrMagnitude<.0001f)tangent=Vector3.forward;tangent.Normalize();
            Vector3 x=Vector3.Cross(tangent,Mathf.Abs(tangent.z)<.9f?Vector3.forward:Vector3.up).normalized,y=Vector3.Cross(tangent,x);
            for(int k=0;k<Sides;k++)
            {
                int index=ring*Sides+k;float angle=k*Mathf.PI*2/Sides;
                float serration=.78f+H(seed+ring*7+k)*.45f;
                vertices[index]=centre+(x*Mathf.Cos(angle)+y*Mathf.Sin(angle))*radius*serration;
                uv[index]=new Vector2(u,k/(float)Sides);colors[index]=new Color(1,1,1,fade);
            }
        }

        private static void Release(UnityEngine.Object asset)
        {
            if(asset==null)return;
            if(Application.isPlaying)Destroy(asset);else DestroyImmediate(asset);
        }

        private void OnDestroy(){foreach(var m in meshes)Release(m);Release(material);}
    }
}
