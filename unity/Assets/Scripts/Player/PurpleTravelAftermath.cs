using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>Bounded, individually ageing surface wounds. No uniform strip or damage.</summary>
    public sealed class PurpleTravelAftermath : MonoBehaviour
    {
        private const int Capacity=64;
        private const float Spacing=.9f;
        private readonly RaycastHit[] surfaceHits=new RaycastHit[32];
        private readonly MeshRenderer[] patches=new MeshRenderer[Capacity];
        private readonly MaterialPropertyBlock[] properties=new MaterialPropertyBlock[Capacity];
        private readonly float[] born=new float[Capacity];
        private readonly Vector3[] centres=new Vector3[Capacity];
        private readonly LineRenderer[] arcs=new LineRenderer[8];
        private Material scarMaterial, filamentMaterial;
        private Mesh patchMesh;
        private Vector3 start,forward,right;
        private float scarWidth, life;
        private int sampled;

        public void Configure(Vector3 origin,Vector3 direction,float visualDiameter)
        {
            start=origin; forward=direction.normalized; right=Vector3.Cross(Vector3.up,forward).normalized;
            scarWidth=visualDiameter*GojoPolishSettings.Current.purpleScarWidthMultiplier;
            life=GojoPolishSettings.Current.purpleScarDuration;
            scarMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleScar")) { name="PurpleScar_Runtime" };
            filamentMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleFilament")) { name="PurpleScarDischarge_Runtime" };
            patchMesh=new Mesh { name="PurpleScarPatch_Runtime" };
            patchMesh.vertices=new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(-.5f,0,.5f),new Vector3(.5f,0,.5f)};
            patchMesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one};
            patchMesh.triangles=new[]{0,2,1,1,2,3}; patchMesh.RecalculateBounds();
            for(int i=0;i<Capacity;i++)
            {
                var go=new GameObject("PurpleBrokenScar_"+i,typeof(MeshFilter),typeof(MeshRenderer));
                go.transform.SetParent(transform,false); go.GetComponent<MeshFilter>().sharedMesh=patchMesh;
                var mr=go.GetComponent<MeshRenderer>(); mr.sharedMaterial=scarMaterial;
                mr.shadowCastingMode=ShadowCastingMode.Off; mr.receiveShadows=false; mr.enabled=false;
                patches[i]=mr; properties[i]=new MaterialPropertyBlock();
            }
            for(int i=0;i<arcs.Length;i++)
            {
                var go=new GameObject("PurpleResidualDischarge_"+i); go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>(); arcs[i]=line; line.sharedMaterial=filamentMaterial;
                line.positionCount=6; line.useWorldSpace=true; line.startWidth=.04f; line.endWidth=.004f;
                line.shadowCastingMode=ShadowCastingMode.Off; line.receiveShadows=false; line.enabled=false;
            }
        }
        private static float Hash(float v)=>Mathf.Repeat(Mathf.Sin(v*127.1f)*43758.5453f,1f);

        public void Render(Vector3 orb,float elapsed,float fade,bool travelling,float visualDiameter=0)
        {
            if(travelling && visualDiameter>0) scarWidth=visualDiameter*GojoPolishSettings.Current.purpleScarWidthMultiplier;
            float distance=Vector3.Dot(orb-start,forward);
            while(travelling && sampled<Capacity && (sampled+.5f)*Spacing<distance)
            {
                AddScar(sampled,elapsed); sampled++;
            }
            for(int i=0;i<sampled;i++)
            {
                float age=elapsed-born[i];
                patches[i].enabled=age<life;
                if(!patches[i].enabled) continue;
                properties[i].SetFloat("_Age",age); patches[i].SetPropertyBlock(properties[i]);
            }
            for(int i=0;i<arcs.Length;i++)
            {
                int index=sampled-1-i*3;
                bool visible=index>=0 && elapsed-born[Mathf.Max(0,index)]<life*.7f && Hash(Mathf.Floor(elapsed*9)+i*7)>.72f;
                arcs[i].enabled=visible;
                if(!visible) continue;
                float age=elapsed-born[index];
                Color tint=new Color(.5f,.09f,1.1f,(1-age/life)*.45f);
                arcs[i].startColor=tint; tint.a=0; arcs[i].endColor=tint;
                for(int j=0;j<6;j++)
                    arcs[i].SetPosition(j,centres[index]+right*((j/5f-.5f)*scarWidth*.35f)
                        +forward*(Hash(index*13+j*7)-.5f)*.5f+Vector3.up*(.04f+Mathf.Sin(j/5f*Mathf.PI)*.20f));
            }
        }
        private void AddScar(int index,float elapsed)
        {
            Vector3 sample=start+forward*((index+.5f)*Spacing),normal=Vector3.up;
            Vector3 surface=sample-Vector3.up*.7f; bool ground=false; float nearest=float.PositiveInfinity;
            int count=Physics.RaycastNonAlloc(sample+Vector3.up*2,Vector3.down,surfaceHits,8,~0,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++)
            {
                var hit=surfaceHits[i];
                if(hit.collider.GetComponentInParent<Health>()!=null || hit.distance>=nearest) continue;
                nearest=hit.distance; surface=hit.point+hit.normal*.045f; normal=hit.normal; ground=true;
            }
            centres[index]=surface; born[index]=elapsed;
            var patch=patches[index].transform;
            patch.SetPositionAndRotation(surface+right*(Hash(index*3)-.5f)*scarWidth*.18f,
                Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,normal).normalized,normal));
            patch.localScale=new Vector3(scarWidth*(.42f+Hash(index*11)*.58f),1,Spacing*(.75f+Hash(index*17)*.60f));
            properties[index].SetFloat("_Seed",index*13.17f); properties[index].SetFloat("_Life",life);
            properties[index].SetFloat("_Ground",ground?1:0);
        }
        private void OnDestroy()
        {
            if(scarMaterial!=null) Destroy(scarMaterial);
            if(filamentMaterial!=null) Destroy(filamentMaterial);
            if(patchMesh!=null) Destroy(patchMesh);
        }
    }
}
