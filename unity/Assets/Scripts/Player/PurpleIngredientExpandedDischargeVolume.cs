using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    public struct PurpleIngredientDischargeSignal
    {
        public float strength;
        public Vector3 localDirection;
    }

    /// <summary>Opt-in multi-scale discharge graph; the existing Hybrid runtime stays independent.</summary>
    public sealed class PurpleIngredientExpandedDischargeVolume : MonoBehaviour
    {
        private const int Sides=5,RingsPerEdge=3,MainNodes=9,MaxMainEdges=8,MaxBranches=5,MaxEdges=MaxMainEdges+MaxBranches*3;
        private enum Scale { Large, Medium, Local }
        private sealed class EventSlot
        {
            public Scale scale;
            public int id,lastEpoch=int.MinValue,lastTopology=int.MinValue;
            public Mesh mesh;
            public MeshRenderer renderer;
            public Vector3[] vertices;
            public Vector2[] uv;
            public Color[] colors;
            public int[] triangles;
            public Vector3[] path=new Vector3[MainNodes];
            public int revision;
            public int lastAttachment=-1;
        }

        private EventSlot[] slots=Array.Empty<EventSlot>();
        private Material material;
        private bool blue;
        private float flowReach,largeReach,mediumReach,localReach,depth,intensity;
        private int largeStates,mediumStates,localStates;
        private float largeRate,mediumRate,localRate,largeLifeMin,largeLifeMax,mediumLifeMin,mediumLifeMax,localLifeMin,localLifeMax;
        private int largeCount,mediumCount,localCount;
        public int ActiveEventCount { get; private set; }
        public int ActiveLargeCount { get; private set; }
        public int ActiveMediumCount { get; private set; }
        public int ActiveLocalCount { get; private set; }
        public int TopologyRevisionCount { get; private set; }
        public int BranchAttachmentChangeCount { get; private set; }
        public int LastDisconnectedSegmentCount { get; private set; }
        public PurpleIngredientDischargeSignal LastSignal { get; private set; }
        public float MaximumReachSeen { get; private set; }

        private static float H(float n)=>Mathf.Repeat(Mathf.Sin(n*127.1f+311.7f)*43758.5453f,1);
        private static float Smooth(float a,float b,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,x));

        public void Configure(bool blueIngredient,float baseReach,PurpleIngredientExpandedDischargeProfile profile)
        {
            blue=blueIngredient;flowReach=baseReach;depth=profile.depth;intensity=profile.intensity;
            largeReach=profile.largeReach;mediumReach=profile.mediumReach;localReach=profile.localReach;
            largeCount=profile.largeEvents;mediumCount=profile.mediumEvents;localCount=profile.localEvents;
            largeRate=profile.largeRate;mediumRate=profile.mediumRate;localRate=profile.localRate;
            largeLifeMin=profile.largeLifeMin;largeLifeMax=profile.largeLifeMax;mediumLifeMin=profile.mediumLifeMin;mediumLifeMax=profile.mediumLifeMax;localLifeMin=profile.localLifeMin;localLifeMax=profile.localLifeMax;
            largeStates=profile.largeTopologyStates;mediumStates=profile.mediumTopologyStates;localStates=profile.localTopologyStates;
            material=new Material(Resources.Load<Shader>("VFX/PurpleIngredientExpandedDischarge")){name="PurpleIngredientExpandedDischarge_Runtime"};
            material.SetColor("_Color",blue?new Color(.055f,.58f,3.8f,1):new Color(4.8f,.035f,.14f,1));
            slots=new EventSlot[largeCount+mediumCount+localCount];int at=0;
            AddSlots(Scale.Large,largeCount,ref at);AddSlots(Scale.Medium,mediumCount,ref at);AddSlots(Scale.Local,localCount,ref at);
        }

        private void AddSlots(Scale scale,int count,ref int at)
        {
            for(int k=0;k<count;k++)
            {
                var root=new GameObject("ExpandedDischarge_"+scale+"_"+k);root.transform.SetParent(transform,false);
                var mesh=new Mesh{name="PurpleIngredientExpandedDischarge_"+scale};mesh.MarkDynamic();
                int vertexCount=MaxEdges*RingsPerEdge*Sides;var vertices=new Vector3[vertexCount];var uv=new Vector2[vertexCount];var colors=new Color[vertexCount];var tris=new int[MaxEdges*(RingsPerEdge-1)*Sides*6];
                int t=0;
                for(int e=0;e<MaxEdges;e++)for(int r=0;r<RingsPerEdge-1;r++)for(int s=0;s<Sides;s++)
                {
                    int a=e*RingsPerEdge*Sides+r*Sides+s,b=e*RingsPerEdge*Sides+r*Sides+(s+1)%Sides,c=a+Sides,d=b+Sides;
                    tris[t++]=a;tris[t++]=b;tris[t++]=c;tris[t++]=b;tris[t++]=d;tris[t++]=c;
                }
                root.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=root.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                renderer.enabled=false;mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=tris;
                slots[at]=new EventSlot{scale=scale,id=at,mesh=mesh,renderer=renderer,vertices=vertices,uv=uv,colors=colors,triangles=tris};at++;
            }
        }

        public PurpleIngredientDischargeSignal Sample(float clock,float fusion)
        {
            ActiveEventCount=ActiveLargeCount=ActiveMediumCount=ActiveLocalCount=0;MaximumReachSeen=0;LastDisconnectedSegmentCount=0;
            var result=new PurpleIngredientDischargeSignal();float flow=PurpleIngredientOuterExplorationVolume.Weight(fusion);material.SetFloat("_Phase",clock);
            foreach(var slot in slots)
            {
                float rate=Rate(slot.scale),scaled=clock*rate+slot.id*.6180339f+(blue?.173f:.397f);
                int epoch=Mathf.FloorToInt(scaled);float phase=Mathf.Repeat(scaled,1f)/rate;
                float lifeMin=LifeMin(slot.scale),lifeMax=LifeMax(slot.scale),life=Mathf.Lerp(lifeMin,lifeMax,H(epoch*31+slot.id*17+7));
                bool live=phase<life&&flow>.001f;slot.renderer.enabled=live;if(!live)continue;
                ActiveEventCount++;if(slot.scale==Scale.Large)ActiveLargeCount++;else if(slot.scale==Scale.Medium)ActiveMediumCount++;else ActiveLocalCount++;
                float p=phase/life;int states=TopologyStates(slot.scale),topology=Mathf.Min(states-1,Mathf.FloorToInt(p*states));
                if(epoch!=slot.lastEpoch||topology!=slot.lastTopology){slot.lastEpoch=epoch;slot.lastTopology=topology;slot.revision++;TopologyRevisionCount++;}
                float eventSeed=epoch*71+slot.id*29+topology*101+(blue?13:47);
                float envelope=blue?Smooth(.06f,.66f,p)*(1-Smooth(.84f,1,p)):Smooth(0,.12f,p)*(1-.72f*Smooth(.72f,1,p));
                float flicker=.54f+.46f*H(Mathf.Floor(p*(slot.scale==Scale.Large?16:23))+eventSeed*3);
                float flash=envelope*flicker*intensity*flow;
                Vector3 direction=Direction(eventSeed+3),side=Vector3.Cross(direction,Direction(eventSeed+11)).normalized;
                if(side.sqrMagnitude<.01f)side=Vector3.Cross(direction,Vector3.up).normalized;
                Vector3 depthAxis=Vector3.Cross(direction,side).normalized;Quaternion roll=Quaternion.AngleAxis(H(eventSeed+21)*360,direction);side=roll*side;depthAxis=roll*depthAxis;
                float reach=flowReach*Reach(slot.scale)*Mathf.Lerp(.82f,1.34f,H(eventSeed+31));MaximumReachSeen=Mathf.Max(MaximumReachSeen,reach);
                int gap=1+Mathf.FloorToInt(H(eventSeed+49)*(MainEdges(slot.scale)-2));int branchAttach=1+Mathf.FloorToInt(H(eventSeed+57)*(MainNodes-3));
                if(slot.lastAttachment>=0&&slot.lastAttachment!=branchAttach)BranchAttachmentChangeCount++;
                slot.lastAttachment=branchAttach;
                LastDisconnectedSegmentCount++;
                var path=slot.path;
                for(int n=0;n<MainNodes;n++)
                {
                    float u=n/(float)(MainNodes-1),radial;
                    float curl,z;
                    if(slot.scale==Scale.Local)
                    {
                        radial=blue?Mathf.Lerp(1.2f,.68f,u)*(1-.12f*p):Mathf.Lerp(.94f,1.42f,u)*Mathf.Lerp(.92f,1.06f,p);
                        curl=Mathf.Sin(u*9+p*(blue?19:27)+eventSeed)*.19f;
                        z=Mathf.Sin(u*13+p*(blue?21:-25)+eventSeed)*depth*.13f;
                    }
                    else
                    {
                        if(blue)radial=Mathf.Lerp(reach,.2f,u)*(1-.26f*p);
                        else radial=Mathf.Lerp(.72f,reach,u)*Mathf.Lerp(.73f,1.06f,p);
                        curl=Mathf.Sin(u*7+p*(blue?14:21)+eventSeed)*reach*(slot.scale==Scale.Large?.2f:.14f);
                        z=Mathf.Sin(u*9+p*(blue?17:-19)+eventSeed)*depth;
                    }
                    path[n]=direction*radial+side*curl+depthAxis*z;
                }
                int mainEdges=MainEdges(slot.scale),branchCount=BranchCount(slot.scale),visibleBranch=1+Mathf.FloorToInt(H(eventSeed+83)*branchCount);
                for(int e=0;e<MaxEdges;e++)
                {
                    Vector3 a,b,c;bool alive=false;
                    if(e<mainEdges)
                    {
                        alive=e!=gap&&H(eventSeed+e*7)>(slot.scale==Scale.Large?.29f:slot.scale==Scale.Medium?.38f:.44f);
                        if(e==mainEdges-1&&slot.scale!=Scale.Local){alive=false;LastDisconnectedSegmentCount++;}
                        a=path[e];b=path[e+1];c=Vector3.Lerp(a,b,.52f)+side*Mathf.Sin(p*17+e*4+eventSeed)*reach*.09f+depthAxis*Mathf.Cos(p*13+e+eventSeed)*depth*.08f;
                    }
                    else
                    {
                        int branch=(e-mainEdges)/3,branchPart=(e-mainEdges)%3;
                        alive=branch<branchCount&&branch<visibleBranch&&H(eventSeed+branch*19+branchPart*43+113)>.27f;
                        int attach=(branch==0)?branchAttach:Mathf.Clamp(branchAttach+(branch%2==0?2:-2),1,MainNodes-2);
                        a=path[attach];Vector3 branchDir=(side*(H(eventSeed+branch*5+127)>.5f?1:-1)+depthAxis*(H(eventSeed+branch*7+139)-.5f)+direction*(blue?-.25f:.38f)).normalized;
                        float branchReach=reach*(slot.scale==Scale.Large?.32f:slot.scale==Scale.Medium?.48f:.18f);
                        Vector3 branchEnd=a+branchDir*branchReach+depthAxis*Mathf.Sin(p*18+branch+eventSeed)*depth*.16f;
                        b=Vector3.Lerp(a,branchEnd,(branchPart+.5f)/3f);
                        c=Vector3.Lerp(a,branchEnd,(branchPart+1f)/3f);
                    }
                    WriteEdge(slot,e,a,b,c,alive,flash,p,reach,eventSeed+e*3);
                }
                slot.mesh.vertices=slot.vertices;slot.mesh.colors=slot.colors;slot.mesh.uv=slot.uv;slot.mesh.RecalculateBounds();
                if(flash>result.strength){result.strength=flash;result.localDirection=direction;}
            }
            LastSignal=result;return result;
        }

        private static void WriteEdge(EventSlot slot,int edge,Vector3 a,Vector3 b,Vector3 c,bool alive,float flash,float phase,float reach,float seed)
        {
            float length=(c-a).magnitude;float gap=Mathf.Clamp(length*.17f,.025f,.24f);Vector3 forward=(c-a).normalized;
            Vector3 left=Vector3.Cross(forward,Vector3.up);if(left.sqrMagnitude<.001f)left=Vector3.Cross(forward,Vector3.right);left.Normalize();Vector3 up=Vector3.Cross(forward,left).normalized;
            Vector3[] centres={Vector3.Lerp(a,b,.14f),b,Vector3.Lerp(b,c,.86f)};
            float scale=slot.scale==Scale.Large?reach*.045f:slot.scale==Scale.Medium?reach*.075f:.055f;
            float pulse=slot.scale==Scale.Local?Mathf.Lerp(1.25f,.6f,phase):Mathf.Lerp(.65f,1.3f,Mathf.Sin(phase*Mathf.PI));
            float[] widths={.68f,1f,.56f};
            for(int ring=0;ring<RingsPerEdge;ring++)for(int s=0;s<Sides;s++)
            {
                int index=(edge*RingsPerEdge+ring)*Sides+s;float angle=s*Mathf.PI*2/Sides;
                float rough=.48f+H(seed+ring*13+s*5)*.85f;float radius=alive?scale*reach*pulse*widths[ring]*rough:0;
                float along=ring==0?gap:ring==2?-gap:0;
                slot.vertices[index]=alive?centres[ring]+forward*along+(left*Mathf.Cos(angle)+up*Mathf.Sin(angle))*radius:Vector3.zero;
                slot.colors[index]=new Color(1,1,1,alive?flash*(.52f+.48f*H(seed+ring*17)):0);
                slot.uv[index]=new Vector2(ring/(float)(RingsPerEdge-1),s/(float)Sides);
            }
        }

        private float Rate(Scale s)=>s==Scale.Large?largeRate:s==Scale.Medium?mediumRate:localRate;
        private float LifeMin(Scale s)=>s==Scale.Large?largeLifeMin:s==Scale.Medium?mediumLifeMin:localLifeMin;
        private float LifeMax(Scale s)=>s==Scale.Large?largeLifeMax:s==Scale.Medium?mediumLifeMax:localLifeMax;
        private float Reach(Scale s)=>s==Scale.Large?largeReach:s==Scale.Medium?mediumReach:localReach;
        private int TopologyStates(Scale s)=>s==Scale.Large?largeStates:s==Scale.Medium?mediumStates:localStates;
        private int MainEdges(Scale s)=>s==Scale.Large?8:s==Scale.Medium?6:3;
        private int BranchCount(Scale s)=>s==Scale.Large?5:s==Scale.Medium?3:1;

        private Vector3 Direction(float seed)
        {
            float y=H(seed)*2-1,angle=H(seed+1.7f)*Mathf.PI*2,r=Mathf.Sqrt(1-y*y);
            return new Vector3(Mathf.Cos(angle)*r,y,Mathf.Sin(angle)*r);
        }

        private static void Release(UnityEngine.Object obj){if(obj==null)return;if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
        private void OnDestroy(){foreach(var slot in slots)if(slot!=null)Release(slot.mesh);Release(material);}
    }
}
