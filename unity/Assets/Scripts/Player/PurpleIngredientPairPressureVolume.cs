using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // Four broad swept air volumes, synchronized to existing PairPolish force events.
    // No camera-facing disc, billboard sheet, or changes to either ingredient transform.
    public sealed class PurpleIngredientPairPressureVolume : MonoBehaviour
    {
        const int Count=4,Nodes=18,Sides=10;
        readonly Mesh[] meshes=new Mesh[Count];
        readonly MeshRenderer[] renderers=new MeshRenderer[Count];
        readonly Vector3[] path=new Vector3[Nodes],vertices=new Vector3[Nodes*Sides];
        readonly Color[] colors=new Color[Nodes*Sides];
        readonly Vector2[] uv=new Vector2[Nodes*Sides];
        bool blue;
        Material material;
        PurpleIngredientPairPressureProfile profile;
        PurpleIngredientPairPolishProfile pair;
        Vector3 toward;
        float coupling;
        public float LeadingRadius { get; private set; }
        public float ActivePressure { get; private set; }
        public void Configure(bool inward)
        {
            blue=inward;profile=PurpleIngredientPairPressureProfile.Current;pair=PurpleIngredientPairPolishProfile.Current;
            material=new Material(Resources.Load<Shader>("VFX/PurpleIngredientPairPressure")){name="PurpleIngredientPairPressure_Material"};
            material.SetColor("_Color",blue?new Color(.34f,.65f,.79f):new Color(.84f,.29f,.33f));
            material.SetFloat("_Density",profile.density);material.SetFloat("_Gain",profile.diffuseEmission);
            material.SetFloat("_Crest",profile.crestEmission);material.SetFloat("_Erosion",profile.erosion);
            var triangles=new int[(Nodes-1)*Sides*6];int t=0;
            for(int j=0;j<Nodes-1;j++)for(int k=0;k<Sides;k++)
            {
                int a=j*Sides+k,b=j*Sides+(k+1)%Sides,c=a+Sides,d=b+Sides;
                triangles[t++]=a;triangles[t++]=b;triangles[t++]=c;triangles[t++]=b;triangles[t++]=d;triangles[t++]=c;
            }
            for(int i=0;i<Count;i++)
            {
                var go=new GameObject("MacroPressure_"+i);go.transform.SetParent(transform,false);
                meshes[i]=new Mesh{name="PurpleIngredientPairPressure_Mesh_"+i};meshes[i].MarkDynamic();
                meshes[i].vertices=vertices;meshes[i].triangles=triangles;
                go.AddComponent<MeshFilter>().sharedMesh=meshes[i];renderers[i]=go.AddComponent<MeshRenderer>();
                renderers[i].sharedMaterial=material;renderers[i].shadowCastingMode=ShadowCastingMode.Off;renderers[i].receiveShadows=false;
            }
        }
        void Event(float clock,int index,out float p,out float seed)
        {
            int i=index*3;
            float rate=blue?pair.blueRate*(.8f+PurpleIngredientFlowFirstVolume.H(i+8)*.55f):pair.redRate*(.7f+PurpleIngredientFlowFirstVolume.H(i+9)*.85f);
            float tick=clock*rate+i*(blue?.317f:.417f);
            p=Mathf.Repeat(tick,1);seed=i*31+Mathf.Floor(tick)*57+(blue?0:113);
        }
        Vector3 BluePath(float q,float seed)
        {
            float r=Mathf.Lerp(profile.pressureReach,.38f,Mathf.Pow(Mathf.Clamp01(q),2.3f));
            float a=seed*.23f+(1-q)*2.35f;
            var plane=Quaternion.Euler(PurpleIngredientFlowFirstVolume.H(seed+1)*180,PurpleIngredientFlowFirstVolume.H(seed+2)*360,PurpleIngredientFlowFirstVolume.H(seed+3)*360);
            return plane*new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,Mathf.Sin(q*7+seed)*.13f)+toward*coupling*Mathf.Sin(q*Mathf.PI)*.65f;
        }
        public void Sample(float clock,float fusion,Vector3 opposite)
        {
            material.SetFloat("_PhaseTime",clock);
            toward=transform.InverseTransformDirection((opposite-transform.position).normalized);
            coupling=profile.gapBend*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.04f,.48f,fusion));
            float weight=PurpleIngredientBurstVolume.Weight(fusion);
            LeadingRadius=0;ActivePressure=0;
            for(int i=0;i<Count;i++)
            {
                Event(clock,i,out float p,out float seed);
                renderers[i].enabled=weight>.001f;if(!renderers[i].enabled)continue;
                float burst=Mathf.Clamp01((p-pair.redTension)/(1-pair.redTension));
                float strength=blue?Mathf.SmoothStep(0,1,p/.08f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.89f,1,p))):
                    (p<pair.redTension?.2f+.2f*p/pair.redTension:Mathf.Pow(1-burst,.7f));
                float width;
                if(blue)
                {
                    for(int j=0;j<Nodes;j++)path[j]=BluePath(Mathf.Clamp01(p-(1-j/(float)(Nodes-1))*profile.blueSpan),seed);
                    width=profile.pressureWidth*(1-.73f*p);
                }
                else
                {
                    var plane=Quaternion.Euler(PurpleIngredientFlowFirstVolume.H(seed+1)*180,PurpleIngredientFlowFirstVolume.H(seed+2)*360,PurpleIngredientFlowFirstVolume.H(seed+3)*360);
                    var dir=plane*new Vector3(Mathf.Cos(seed*.37f),Mathf.Sin(seed*.37f),0);
                    var side=Vector3.Cross(dir,Mathf.Abs(dir.y)>.8f?Vector3.right:Vector3.up).normalized;
                    var up=Vector3.Cross(side,dir);
                    float radius=Mathf.Lerp(1.02f,profile.pressureReach,Mathf.Pow(burst,.57f));
                    float opening=profile.redSpread*(.16f+burst*1.25f);
                    for(int j=0;j<Nodes;j++)
                    {
                        float u=j/(float)(Nodes-1),offset=(u-.5f)*opening;
                        path[j]=dir*(radius-offset*offset*.19f)+side*offset+up*Mathf.Sin(u*5+seed)*burst*.19f-toward*coupling*Mathf.Sin(p*Mathf.PI)*.38f;
                    }
                    width=profile.pressureWidth*(.24f+burst*.92f);
                }
                if(i==0)LeadingRadius=blue?path[Nodes-1].magnitude:path[Nodes/2].magnitude;
                ActivePressure=Mathf.Max(ActivePressure,strength*weight);
                for(int j=0;j<Nodes;j++)
                {
                    float u=j/(float)(Nodes-1);
                    var tangent=(path[Mathf.Min(Nodes-1,j+1)]-path[Mathf.Max(0,j-1)]).normalized;
                    if(tangent.sqrMagnitude<.1f)tangent=path[j].normalized;
                    var axis=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.8f?Vector3.right:Vector3.up).normalized;
                    var other=Vector3.Cross(tangent,axis);
                    float taper=Mathf.Pow(Mathf.Max(.02f,Mathf.Sin(u*Mathf.PI)),.65f);
                    float localWidth=width*taper*(.84f+.16f*Mathf.Sin(u*11+seed));
                    for(int k=0;k<Sides;k++)
                    {
                        int index=j*Sides+k;float a=k/(float)Sides*Mathf.PI*2;
                        vertices[index]=path[j]+axis*Mathf.Cos(a)*localWidth+other*Mathf.Sin(a)*localWidth*profile.pressureDepth;
                        uv[index]=new Vector2(u,k/(float)Sides);
                        colors[index]=new Color(blue?Mathf.SmoothStep(.35f,1,u):.55f+burst*.45f,0,0,strength*weight);
                    }
                }
                meshes[i].vertices=vertices;meshes[i].uv=uv;meshes[i].colors=colors;meshes[i].RecalculateNormals();meshes[i].RecalculateBounds();
            }
        }
        void OnDestroy()
        {
            foreach(var mesh in meshes)Release(mesh);Release(material);
        }
        static void Release(Object item){if(item==null)return;if(Application.isPlaying)Destroy(item);else DestroyImmediate(item);}
    }
}
