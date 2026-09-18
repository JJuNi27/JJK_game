using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>Only the two ingredients of Purple. Standalone Blue/Red assets are untouched.</summary>
    public sealed class PurpleFusionIngredient : MonoBehaviour
    {
        private Material surface, filament;
        private readonly LineRenderer[] curves = new LineRenderer[10];
        private readonly LineRenderer[] motes = new LineRenderer[20];
        private LineRenderer bridge;
        private readonly Vector3[] points = new Vector3[32];
        private float polarity;

        public void Configure(bool red)
        {
            polarity=red?1:-1;
            surface=new Material(Resources.Load<Shader>("VFX/HollowPurpleIngredient")) { name="PurpleIngredient_Runtime" };
            surface.SetFloat("_Polarity",polarity);
            filament=new Material(Resources.Load<Shader>("VFX/HollowPurpleFilament")) { name="PurpleIngredientFlow_Runtime" };
            var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name=red?"RedRepulsionMass":"BlueAttractionMass";
            sphere.transform.SetParent(transform,false); sphere.transform.localScale=Vector3.one*2.05f;
            var col=sphere.GetComponent<Collider>(); col.enabled=false; Destroy(col);
            var mr=sphere.GetComponent<Renderer>(); mr.sharedMaterial=surface; mr.shadowCastingMode=ShadowCastingMode.Off; mr.receiveShadows=false;
            for(int i=0;i<curves.Length;i++) curves[i]=Line((red?"OutwardPressureArc_":"InwardSpiral_")+i,32);
            for(int i=0;i<motes.Length;i++) motes[i]=Line((red?"OutwardFragment_":"CapturedMote_")+i,2);
            bridge=Line("OpposedFieldTensionBridge",32); bridge.enabled=false;
        }

        private LineRenderer Line(string name,int count)
        {
            var go=new GameObject(name); go.transform.SetParent(transform,false);
            var l=go.AddComponent<LineRenderer>(); l.useWorldSpace=false; l.positionCount=count;
            l.sharedMaterial=filament; l.numCapVertices=1; l.shadowCastingMode=ShadowCastingMode.Off;
            l.receiveShadows=false; return l;
        }

        public void Render(float clock,float fusion,Vector3 opposite)
        {
            surface.SetFloat("_PhaseTime",clock); surface.SetFloat("_Fusion",fusion);
            Color tint=polarity<0?new Color(.035f,.7f,2.4f,1):new Color(2.4f,.02f,.065f,1);
            tint=Color.Lerp(tint,new Color(1.7f,.035f,2.5f,1),fusion*.6f);
            for(int i=0;i<curves.Length;i++)
            {
                float phase=Mathf.Repeat(clock*(polarity<0?.7f:1.15f)+i*.137f,1);
                Quaternion plane=Quaternion.Euler(i*31+17,i*73,i*47);
                for(int j=0;j<32;j++)
                {
                    float u=j/31f;
                    float radius=polarity<0?Mathf.Lerp(2.05f,.78f,Mathf.Clamp01(phase*.45f+u*.65f)):.98f+phase*1.35f;
                    float angle=i*2.399f+clock*polarity+u*(polarity<0?2.65f:.65f+i%3*.2f);
                    points[j]=plane*new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius,Mathf.Sin(u*4+i)*.18f);
                }
                var l=curves[i]; l.SetPositions(points);
                l.startWidth=polarity<0?.035f:.075f*(1-phase); l.endWidth=polarity<0?.085f:.016f;
                Color c=tint; c.a=(polarity<0?.70f:.85f)*(1-phase); l.startColor=c; c.a*=.12f; l.endColor=c;
            }
            for(int i=0;i<motes.Length;i++)
            {
                float phase=Mathf.Repeat(clock*(1+i%3*.2f)+i*.173f,1);
                float r=polarity<0?Mathf.Lerp(2.45f,.65f,phase):Mathf.Lerp(.98f,2.5f,phase);
                float a=i*2.399f+phase*(polarity<0?1.5f:.1f);
                Vector3 dir=Quaternion.Euler(i*39,i*29,0)*new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                var l=motes[i]; l.startWidth=.036f; l.endWidth=.006f;
                l.SetPosition(0,dir*r); l.SetPosition(1,dir*(r-polarity*.12f));
                Color c=tint; c.a=Mathf.Sin(phase*Mathf.PI)*.75f; l.startColor=l.endColor=c;
            }
            bridge.enabled=fusion>.02f && fusion<.94f;
            if(bridge.enabled)
            {
                Vector3 end=transform.InverseTransformPoint(opposite);
                for(int j=0;j<32;j++)
                {
                    float u=j/31f, envelope=Mathf.Sin(u*Mathf.PI);
                    points[j]=end*u+new Vector3(0,Mathf.Sin(u*29-clock*23)*.13f,Mathf.Cos(u*17+clock*13)*.16f)*envelope;
                }
                bridge.SetPositions(points); bridge.startWidth=.04f+fusion*.11f; bridge.endWidth=.018f;
                tint.a=Mathf.Sin(fusion*Mathf.PI); bridge.startColor=bridge.endColor=tint;
            }
        }

        private void OnDestroy() { if(surface!=null) Destroy(surface); if(filament!=null) Destroy(filament); }
    }
}
