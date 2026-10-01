using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>Separate, short lived energy discharges layered over the persistent ingredient flow.</summary>
    public sealed class PurpleIngredientOuterDischargeVolume : MonoBehaviour
    {
        private const int Events=6,Nodes=11,Sides=6,BranchNodes=5;
        private readonly Mesh[] mainMeshes=new Mesh[Events],branchMeshes=new Mesh[Events];
        private readonly MeshRenderer[] mainRenderers=new MeshRenderer[Events],branchRenderers=new MeshRenderer[Events];
        private readonly Vector3[] mainVertices=new Vector3[Nodes*Sides],branchVertices=new Vector3[BranchNodes*Sides];
        private readonly Color[] mainColors=new Color[Nodes*Sides],branchColors=new Color[BranchNodes*Sides];
        private readonly Vector2[] mainUv=new Vector2[Nodes*Sides],branchUv=new Vector2[BranchNodes*Sides];
        private readonly Vector3[] path=new Vector3[Nodes];
        private readonly bool[] segmentAlive=new bool[Nodes-1];
        private Material material;
        private MaterialPropertyBlock block;
        private bool inward;
        private float outerReach,reachMultiplier,depth,eventRate,intensity;
        public int ActiveEventCount { get; private set; }
        public float LastReach { get; private set; }
        public float LastCoupling { get; private set; }
        public float LastStartRadius { get; private set; }
        public float LastEndRadius { get; private set; }

        private static float Hash(float value)=>Mathf.Repeat(Mathf.Sin(value*127.1f+19.3f)*43758.5453f,1);
        private static float Smooth(float a,float b,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,x));

        public void Configure(bool blue,float flowReach,float reachScale,float depthRange,float rate,float strength)
        {
            inward=blue;outerReach=flowReach;reachMultiplier=reachScale;depth=depthRange;eventRate=rate;intensity=strength;
            material=new Material(Resources.Load<Shader>("VFX/PurpleIngredientOuterDischarge")){name="PurpleIngredientOuterDischarge_Runtime"};
            material.SetColor("_Color",inward?new Color(.055f,.58f,3.8f,1):new Color(4.8f,.035f,.14f,1));
            block=new MaterialPropertyBlock();
            int[] mainTriangles=TubeTriangles(Nodes),branchTriangles=TubeTriangles(BranchNodes);
            for(int i=0;i<Events;i++)
            {
                var root=new GameObject("IngredientDischargeEvent_"+i);root.transform.SetParent(transform,false);
                mainMeshes[i]=CreateMesh("PurpleIngredientDischarge_Main",mainVertices,mainUv,mainColors,mainTriangles);
                var main=root.AddComponent<MeshFilter>();main.sharedMesh=mainMeshes[i];
                mainRenderers[i]=root.AddComponent<MeshRenderer>();SetupRenderer(mainRenderers[i]);mainRenderers[i].enabled=false;
                var branch=new GameObject("DischargeBranch_"+i);branch.transform.SetParent(root.transform,false);
                branchMeshes[i]=CreateMesh("PurpleIngredientDischarge_Branch",branchVertices,branchUv,branchColors,branchTriangles);
                branch.AddComponent<MeshFilter>().sharedMesh=branchMeshes[i];
                branchRenderers[i]=branch.AddComponent<MeshRenderer>();SetupRenderer(branchRenderers[i]);branchRenderers[i].enabled=false;
            }
        }

        private static int[] TubeTriangles(int nodes)
        {
            var tris=new int[(nodes-1)*Sides*6];int t=0;
            for(int i=0;i<nodes-1;i++)for(int j=0;j<Sides;j++)
            {
                int a=i*Sides+j,b=i*Sides+(j+1)%Sides,c=a+Sides,d=b+Sides;
                tris[t++]=a;tris[t++]=b;tris[t++]=c;tris[t++]=b;tris[t++]=d;tris[t++]=c;
            }
            return tris;
        }

        private static Mesh CreateMesh(string meshName,Vector3[] vertices,Vector2[] uv,Color[] colors,int[] triangles)
        {
            var mesh=new Mesh{name=meshName};mesh.MarkDynamic();mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;return mesh;
        }

        private void SetupRenderer(MeshRenderer renderer)
        {
            renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        }

        /// <returns>Current event energy for coupling to the existing body and persistent flow.</returns>
        public float Sample(float clock,float fusion)
        {
            ActiveEventCount=0;LastReach=0;LastCoupling=0;LastStartRadius=0;LastEndRadius=0;float flowWeight=PurpleIngredientOuterExplorationVolume.Weight(fusion);
            material.SetFloat("_PhaseTime",clock);
            for(int e=0;e<Events;e++)
            {
                float batchTick=clock*eventRate,epoch=Mathf.Floor(batchTick),phase=Mathf.Repeat(batchTick,1);
                float eventSeed=epoch*23+e*11+5;
                float startPhase=Hash(epoch*19+e*7+31)*.24f;
                float life=Mathf.Lerp(.15f,.29f,Hash(eventSeed));
                bool selected=Hash(epoch*41+e*17+59)>.24f;
                float eventPhase=phase-startPhase;
                bool live=selected&&eventPhase>=0&&eventPhase<life&&flowWeight>.001f;
                mainRenderers[e].enabled=live;branchRenderers[e].enabled=live;
                if(!live)continue;

                ActiveEventCount++;
                float p=eventPhase/life;
                float rise=Smooth(0,.12f,p),decay=1-Smooth(.57f,1,p);
                float flicker=.58f+.42f*Hash(Mathf.Floor(p*9)+epoch*37+e*17);
                float flash=rise*decay*flicker*intensity*flowWeight;
                LastCoupling=Mathf.Max(LastCoupling,flash);
                float seed=epoch*43+e*17+(inward?3:11);
                Vector3 direction=RandomDirection(seed),side=Vector3.Cross(direction,RandomDirection(seed+7)).normalized;
                if(side.sqrMagnitude<.1f)side=Vector3.Cross(direction,Vector3.up).normalized;
                Vector3 depthAxis=Vector3.Cross(direction,side).normalized;
                Quaternion orientation=Quaternion.AngleAxis(Hash(seed+4)*360,direction);
                side=orientation*side;depthAxis=orientation*depthAxis;
                float reach=outerReach*reachMultiplier*Mathf.Lerp(.76f,1.34f,Hash(seed+13));
                LastStartRadius=Mathf.Max(LastStartRadius,inward?reach:.84f);
                LastEndRadius=inward?.19f:Mathf.Max(LastEndRadius,reach);
                float pulseScale=Mathf.Lerp(.72f,1.18f,Smooth(0,.20f,p))*(1-.24f*Smooth(.62f,1,p));
                float bendSign=Hash(seed+21)>.5f?1:-1;

                for(int n=0;n<Nodes;n++)
                {
                    float u=n/(float)(Nodes-1);
                    if(n>0)segmentAlive[n-1]=Hash(seed+n*9+Mathf.Floor(p*7)*17)>.31f+.16f*Smooth(.42f,1,p);
                    Vector3 point;
                    if(inward)
                    {
                        float radius=Mathf.Lerp(reach,.19f,u)*pulseScale;
                        float curl=Mathf.Sin(u*Mathf.PI*1.35f+p*8+seed)*(.16f+u*.38f)*bendSign;
                        point=direction*radius+side*curl*reach+depthAxis*Mathf.Sin(u*5+p*10+seed)*depth;
                    }
                    else
                    {
                        float radius=Mathf.Lerp(.84f,reach,u)*pulseScale;
                        float flare=Mathf.Sin(u*Mathf.PI+p*11+seed)*(.11f+u*.32f);
                        point=direction*radius+side*flare*reach+depthAxis*Mathf.Sin(u*7-p*9+seed)*depth;
                    }
                    float tearing=(Hash(seed+n*13+Mathf.Floor(p*6)*29)-.5f)*(.18f+reach*.045f);
                    path[n]=point+side*tearing+depthAxis*Mathf.Sin(u*9+p*15+seed)*depth*.11f;
                }

                for(int n=0;n<Nodes;n++)
                {
                    bool present=(n>0&&segmentAlive[n-1])||(n<Nodes-1&&segmentAlive[n]);
                    float u=n/(float)(Nodes-1);
                    float taper=Mathf.Sin(Mathf.PI*(.04f+.92f*u));
                    float radius=(.095f+Hash(seed+n*5)*.13f)*taper*(.62f+.75f*rise);
                    if(!present)radius=0;
                    Vector3 tangent=path[Mathf.Min(n+1,Nodes-1)]-path[Mathf.Max(0,n-1)];
                    WriteRing(mainVertices,mainUv,mainColors,n,path[n],tangent,radius,u,flash*(present?1:0),seed);
                }
                mainMeshes[e].vertices=mainVertices;mainMeshes[e].uv=mainUv;mainMeshes[e].colors=mainColors;mainMeshes[e].RecalculateNormals();mainMeshes[e].RecalculateBounds();

                int branchStart=4+(int)(Hash(seed+28)*4);Vector3 branchOrigin=path[branchStart];
                Vector3 outward=inward?(path[branchStart+1]-path[branchStart]).normalized:(path[branchStart]-path[branchStart-1]).normalized;
                Vector3 branchDirection=(side*(inward?-1:1)+outward*.35f+depthAxis*(Hash(seed+33)-.5f)).normalized;
                for(int n=0;n<BranchNodes;n++)
                {
                    float u=n/(float)(BranchNodes-1);
                    bool present=n<BranchNodes-1&&Hash(seed+71+n*11+Mathf.Floor(p*8)*23)>.42f;
                    float grow=Smooth(0,.28f,p)*(1-.48f*Smooth(.62f,1,p));
                    Vector3 point=branchOrigin+branchDirection*(reach*.39f*u*grow)+depthAxis*Mathf.Sin(u*Mathf.PI+p*13+seed)*depth*.28f;
                    Vector3 tangent=branchDirection+depthAxis*.3f;
                    float radius=present?(.075f*(1-u)*(.5f+rise)):0;
                    WriteRing(branchVertices,branchUv,branchColors,n,point,tangent,radius,u,flash*(present?1:0)*.85f,seed+43);
                }
                branchMeshes[e].vertices=branchVertices;branchMeshes[e].uv=branchUv;branchMeshes[e].colors=branchColors;branchMeshes[e].RecalculateNormals();branchMeshes[e].RecalculateBounds();
                block.Clear();block.SetFloat("_EventFlash",flash);block.SetFloat("_Breakup",Smooth(.04f,.48f,p));
                mainRenderers[e].SetPropertyBlock(block);branchRenderers[e].SetPropertyBlock(block);
                LastReach=Mathf.Max(LastReach,reach*pulseScale);
            }
            return LastCoupling;
        }

        private static Vector3 RandomDirection(float seed)
        {
            float y=Hash(seed)*2-1,angle=Hash(seed+1.7f)*Mathf.PI*2,r=Mathf.Sqrt(1-y*y);
            return new Vector3(Mathf.Cos(angle)*r,y,Mathf.Sin(angle)*r);
        }

        private static void WriteRing(Vector3[] vertices,Vector2[] uv,Color[] colors,int ring,Vector3 centre,Vector3 tangent,float radius,float u,float alpha,float seed)
        {
            if(tangent.sqrMagnitude<.0001f)tangent=Vector3.forward;tangent.Normalize();
            Vector3 axis=Vector3.Cross(tangent,Mathf.Abs(tangent.y)<.9f?Vector3.up:Vector3.right).normalized;
            Vector3 binormal=Vector3.Cross(tangent,axis).normalized;
            for(int s=0;s<Sides;s++)
            {
                int index=ring*Sides+s;float angle=s*Mathf.PI*2/Sides;
                float rough=.62f+Hash(seed+ring*7+s*3)*.8f;
                vertices[index]=centre+(axis*Mathf.Cos(angle)+binormal*Mathf.Sin(angle))*radius*rough;
                uv[index]=new Vector2(u,s/(float)Sides);colors[index]=new Color(1,1,1,alpha);
            }
        }

        private static void Release(Object asset)
        {
            if(asset==null)return;
            if(Application.isPlaying)Destroy(asset);else DestroyImmediate(asset);
        }

        private void OnDestroy()
        {
            foreach(var mesh in mainMeshes)Release(mesh);foreach(var mesh in branchMeshes)Release(mesh);Release(material);
        }
    }
}
