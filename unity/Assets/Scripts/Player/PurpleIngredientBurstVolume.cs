using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    /// <summary>Three short, owned volumetric ruptures. No formation transform or camera writes.</summary>
    public sealed class PurpleIngredientBurstVolume : MonoBehaviour
    {
        private const int Sides=8,MainRings=9,BranchRings=5,Vertices=(MainRings+BranchRings)*Sides;
        private readonly Mesh[] meshes=new Mesh[3];
        private readonly MeshRenderer[] renderers=new MeshRenderer[3];
        private readonly Vector3[] vertices=new Vector3[Vertices],path=new Vector3[MainRings];
        private readonly Color[] colors=new Color[Vertices];
        private readonly Vector2[] uv=new Vector2[Vertices];
        private Material material;
        private bool blue;
        private float reach;
        private static float H(float v)=>Mathf.Repeat(Mathf.Sin(v*127.1f+19.3f)*43758.5453f,1);
        public static float Weight(float fusion)=>1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.72f,fusion));
        public void Configure(bool inward,float energyReach)
        {
            blue=inward;reach=energyReach;
            material=new Material(Resources.Load<Shader>("VFX/PurpleIngredientBurstVolume")){name="PurpleIngredientBurst_Runtime"};
            material.SetColor("_Color",blue?new Color(.10f,.72f,5.5f):new Color(6.8f,.10f,.20f));
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
            for(int i=0;i<3;i++)
            {
                var go=new GameObject("IngredientBulkBurst_"+i);go.transform.SetParent(transform,false);
                var mesh=new Mesh{name="PurpleIngredientBurst_Mesh"};mesh.MarkDynamic();mesh.vertices=vertices;mesh.triangles=triangles;
                meshes[i]=mesh;go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.enabled=false;renderers[i]=r;
            }
        }
        public void Sample(float clock,float fusion)
        {
            float weight=Weight(fusion);material.SetFloat("_PhaseTime",clock);
            for(int group=0;group<3;group++)
            {
                float tick=clock*(blue?3.1f:3.5f)+group*.333333f,epoch=Mathf.Floor(tick),phase=Mathf.Repeat(tick,1);
                float seed=epoch*17+group*11,duty=.82f+.08f*H(seed+4),p=Mathf.Clamp01(phase/duty);
                float fade=Mathf.SmoothStep(0,1,p/.065f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.78f,1,p)))*weight;
                renderers[group].enabled=fade>.005f;if(!renderers[group].enabled)continue;
                float a=group*2.0944f+.45f+(H(seed)-.5f)*.8f;
                float head=blue?Mathf.Lerp(reach,.90f,p):Mathf.Lerp(1.05f,reach,Mathf.Pow(p,.58f));
                for(int j=0;j<MainRings;j++)
                {
                    float u=j/(float)(MainRings-1),jag=(H(seed+j*5)-.5f)*Mathf.Sin(u*Mathf.PI);
                    float angle=a+(blue?p*.85f:p*.10f)-u*.47f+jag*.18f;
                    float r=head+(blue?1:-1)*u*.65f;
                    path[j]=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),-.35f+H(seed+7)*.25f+jag*.32f).normalized*r;
                }
                for(int j=0;j<MainRings;j++)
                {
                    float u=j/(float)(MainRings-1);
                    Vector3 tangent=path[Mathf.Min(j+1,MainRings-1)]-path[Mathf.Max(0,j-1)];
                    float radius=.11f*(.70f+H(seed+j*3)*.30f)*Mathf.Pow(1-u,.62f);
                    if(j==0)radius*=.65f;
                    Ring(j,path[j],tangent,radius,u,fade,seed);
                }
                Vector3 branchStart=path[3],branchDirection=(path[3]-path[5]).normalized;
                Vector3 bend=Vector3.Cross(branchDirection,Vector3.forward).normalized*(H(seed+19)>.5f?1:-1);
                for(int j=0;j<BranchRings;j++)
                {
                    float u=j/(float)(BranchRings-1);
                    Vector3 centre=branchStart+branchDirection*u*.45f+bend*(u*.30f+(j%2==0?-.06f:.06f)*Mathf.Sin(u*Mathf.PI));
                    Ring(MainRings+j,centre,branchDirection+bend*.5f,.045f*(1-u),u,fade*.85f,seed+5);
                }
                meshes[group].vertices=vertices;meshes[group].colors=colors;meshes[group].uv=uv;meshes[group].RecalculateNormals();meshes[group].RecalculateBounds();
            }
        }
        private void Ring(int ring,Vector3 centre,Vector3 tangent,float radius,float u,float fade,float seed)
        {
            tangent.Normalize();Vector3 x=Vector3.Cross(tangent,Mathf.Abs(tangent.z)<.9f?Vector3.forward:Vector3.up).normalized,y=Vector3.Cross(tangent,x);
            for(int k=0;k<Sides;k++)
            {
                int index=ring*Sides+k;float angle=k*Mathf.PI*2/Sides;
                vertices[index]=centre+(x*Mathf.Cos(angle)+y*Mathf.Sin(angle))*radius*(.85f+H(seed+ring+k)*.3f);
                uv[index]=new Vector2(u,k/(float)Sides);colors[index]=new Color(1,1,1,fade);
            }
        }
        private void OnDestroy(){foreach(var m in meshes)if(m!=null)Destroy(m);if(material!=null)Destroy(material);}
    }
}
