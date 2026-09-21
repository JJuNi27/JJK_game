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
        private float compressionStrength;
        private PurpleIngredientPolishProfile polish;
        private PurpleIngredientMacroProfile macro;
        private PurpleIngredientRebootProfile reboot;
        private PurpleIngredientReboot2Profile reboot2;
        private Material pressure;
        private bool finalRupture;
        private PurpleIngredientBurstVolume bulkBursts;

        public void Configure(bool red, float compression=0)
        {
            compressionStrength=compression;
            polarity=red?1:-1;
            var candidate=PurpleIngredientPolishProfile.Current;
            polish=candidate!=null && candidate.candidateEnabled?candidate:null;
            var aggressive=PurpleIngredientMacroProfile.Current;
            macro=aggressive!=null && aggressive.candidateEnabled?aggressive:null;
            var dense=PurpleIngredientRebootProfile.Current;
            var second=PurpleIngredientReboot2Profile.Current;
            reboot2=second!=null && second.candidateEnabled?second:null;
            finalRupture=reboot2!=null && PurpleIngredientFinalProfile.Current!=null && PurpleIngredientFinalProfile.Current.candidateEnabled;
            reboot=dense!=null && (dense.candidateEnabled || reboot2!=null)?dense:null;
            surface=new Material(Resources.Load<Shader>(reboot!=null?"VFX/HollowPurpleIngredientReboot":macro!=null?"VFX/HollowPurpleIngredientMacro":polish!=null?"VFX/HollowPurpleIngredientPolish":"VFX/HollowPurpleIngredient")) { name="PurpleIngredient_Runtime" };
            surface.SetFloat("_Polarity",polarity);
            if(polish!=null)surface.SetFloat("_Breakup",polish.silhouetteBreakup);
            if(macro!=null)surface.SetFloat("_Breakup",macro.silhouetteBreakup);
            if(reboot!=null)
            {
                surface.SetFloat("_Breakup",reboot.silhouetteBreakup);surface.SetFloat("_Emission",reboot.crackEmission);
                pressure=new Material(Resources.Load<Shader>(finalRupture?"VFX/HollowPurpleIngredientRupture":reboot2!=null?"VFX/HollowPurpleIngredientPressure2":"VFX/HollowPurpleIngredientPressure")){name="PurpleIngredientPressure_Runtime"};
                pressure.SetColor("_Color",reboot2!=null?(red?new Color(3.2f,.08f,.13f):new Color(.12f,.65f,3.2f)):(red?new Color(2.2f,.012f,.028f):new Color(.04f,.38f,2.6f)));
            }
            filament=new Material(Resources.Load<Shader>("VFX/HollowPurpleFilament")) { name="PurpleIngredientFlow_Runtime" };
            var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name=red?"RedRepulsionMass":"BlueAttractionMass";
            sphere.transform.SetParent(transform,false); sphere.transform.localScale=Vector3.one*2.05f;
            var col=sphere.GetComponent<Collider>(); col.enabled=false; Destroy(col);
            var mr=sphere.GetComponent<Renderer>(); mr.sharedMaterial=surface; mr.shadowCastingMode=ShadowCastingMode.Off; mr.receiveShadows=false;
            for(int i=0;i<curves.Length;i++) curves[i]=Line((red?"OutwardPressureArc_":"InwardSpiral_")+i,32);
            for(int i=0;i<motes.Length;i++) motes[i]=Line((red?"OutwardFragment_":"CapturedMote_")+i,2);
            bridge=Line("OpposedFieldTensionBridge",32); bridge.enabled=false;
            if(finalRupture && PurpleIngredientFinal2Profile.Current!=null && PurpleIngredientFinal2Profile.Current.candidateEnabled)
            {bulkBursts=gameObject.AddComponent<PurpleIngredientBurstVolume>();bulkBursts.Configure(!red,reboot2.energyReach);}
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
            if(bulkBursts!=null)bulkBursts.Sample(clock,fusion);
            if(pressure!=null){pressure.SetFloat("_PhaseTime",clock);pressure.SetFloat("_Blend",1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.72f,fusion)));}
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
                if(polish!=null && fusion<=.72f)
                {
                    // Keep the established late-fusion Birth compression exactly as authored.
                    bool blue=polarity<0;
                    float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.72f,fusion));
                    float flowPhase=Mathf.Repeat(clock*(blue?.9f:1.55f)+i*.137f,1);
                    float advance=blue?flowPhase:Mathf.Clamp01(flowPhase/.55f);
                    for(int j=0;j<32;j++)
                    {
                        float u=j/31f;
                        float r=blue?Mathf.Lerp(2.05f,.78f,Mathf.Clamp01(advance*.5f+u*.65f)):.98f+advance*1.35f-u*.30f;
                        float angle=i*2.399f+clock*polarity+u*(blue?1.45f:.32f+i%3*.09f);
                        points[j]=Vector3.Lerp(points[j],plane*new Vector3(Mathf.Cos(angle)*r,Mathf.Sin(angle)*r,Mathf.Sin(u*4+i)*.18f),blend);
                    }
                    l.SetPositions(points);
                    float width=blue?polish.blueFlowWidth:polish.redFlowWidth;
                    float glow=blue?polish.blueFlowEmission:polish.redFlowEmission;
                    float fade=blue?Mathf.Pow(Mathf.Sin(flowPhase*Mathf.PI),2):Mathf.Sin(advance*Mathf.PI)*(.7f+.3f*Mathf.Sin(clock*47+i));
                    l.startWidth=Mathf.Lerp(l.startWidth,(blue?.025f:.085f)*width,blend);l.endWidth=Mathf.Lerp(l.endWidth,(blue?.075f:.008f)*width,blend);
                    c=tint*glow;c.a=fade*(blue?.26f:.9f);l.startColor=Color.Lerp(l.startColor,c,blend);c.a=fade*(blue?.95f:.08f);l.endColor=Color.Lerp(l.endColor,c,blend);
                }
                if(macro!=null)PurpleIngredientMacroFlow.Apply(l,points,macro,polarity<0,false,i,clock,fusion,tint);
                if(reboot!=null)
                {
                    l.sharedMaterial=fusion<.72f?pressure:filament;
                    if(finalRupture)PurpleIngredientFinalFlow.Apply(l,points,reboot2,polarity<0,i,clock,fusion);
                    else if(reboot2!=null)PurpleIngredientReboot2Flow.Apply(l,points,reboot2,polarity<0,false,i,clock,fusion);
                    else PurpleIngredientRebootFlow.Apply(l,points,reboot,polarity<0,false,i,clock,fusion);
                }
                FadeLegacyLine(l,fusion);
            }
            for(int i=0;i<motes.Length;i++)
            {
                float phase=Mathf.Repeat(clock*(1+i%3*.2f)+i*.173f,1);
                float r=polarity<0?Mathf.Lerp(2.45f,.65f,phase):Mathf.Lerp(.98f,2.5f,phase);
                float a=i*2.399f+phase*(polarity<0?1.5f:.1f);
                Vector3 dir=Quaternion.Euler(i*39,i*29,0)*new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                var l=motes[i]; l.positionCount=2; l.startWidth=.036f; l.endWidth=.006f;
                l.SetPosition(0,dir*r); l.SetPosition(1,dir*(r-polarity*.12f));
                Color c=tint; c.a=Mathf.Sin(phase*Mathf.PI)*.75f; l.startColor=l.endColor=c;
                if(polish!=null && fusion<=.72f)
                {
                    bool blue=polarity<0;
                    float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.72f,fusion));
                    float width=blue?polish.blueFlowWidth:polish.redFlowWidth;
                    l.startWidth=Mathf.Lerp(.036f,.036f*width,blend);l.endWidth=.006f;
                    l.SetPosition(1,dir*(r-polarity*Mathf.Lerp(.12f,polish.moteLength,blend)));
                    c=tint*(blue?polish.blueFlowEmission:polish.redFlowEmission);c.a=Mathf.Sin(phase*Mathf.PI)*.9f;
                    l.startColor=Color.Lerp(l.startColor,c,blend);c.a*=.1f;l.endColor=Color.Lerp(l.endColor,c,blend);
                }
                if(macro!=null)PurpleIngredientMacroFlow.Apply(l,points,macro,polarity<0,true,i,clock,fusion,tint);
                if(reboot!=null)
                {
                    l.sharedMaterial=fusion<.72f?pressure:filament;
                    if(reboot2!=null)PurpleIngredientReboot2Flow.Apply(l,points,reboot2,polarity<0,true,i,clock,fusion);
                    else PurpleIngredientRebootFlow.Apply(l,points,reboot,polarity<0,true,i,clock,fusion);
                }
                FadeLegacyLine(l,fusion);
                if(compressionStrength>0 && fusion>.72f && i<8)
                {
                    // Reuse existing motes in world space: ingredient parents shrink to 4%
                    // at fusion end, so simply brightening their local lines is unreadable.
                    float inward=Mathf.Clamp01((fusion-.72f)/.28f);
                    Vector3 centre=(transform.position+opposite)*.5f;
                    Vector3 direction=Quaternion.Euler(i*43+polarity*23,i*79,polarity*61)*Vector3.up;
                    float distance=Mathf.Lerp(6.2f+i%3*.35f,.6f,Mathf.Pow(inward,.7f));
                    float length=Mathf.Lerp(.35f,1.05f,Mathf.Sin(inward*Mathf.PI));
                    l.SetPosition(0,transform.InverseTransformPoint(centre+direction*distance));
                    l.SetPosition(1,transform.InverseTransformPoint(centre+direction*(distance+length)));
                    float scale=Mathf.Max(.01f,transform.lossyScale.x);
                    l.startWidth=.095f/scale;l.endWidth=.012f/scale;
                    c=tint*1.8f;c.a=Mathf.Sin(inward*Mathf.PI)*compressionStrength;l.startColor=c;c.a*=.15f;l.endColor=c;
                }
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

        private void FadeLegacyLine(LineRenderer line,float fusion)
        {
            if(bulkBursts==null)return;
            float visible=1-PurpleIngredientBurstVolume.Weight(fusion);
            line.enabled=visible>.001f;
            Color a=line.startColor,b=line.endColor;a.a*=visible;b.a*=visible;line.startColor=a;line.endColor=b;
        }

        private void OnDestroy() { if(surface!=null) Destroy(surface); if(filament!=null) Destroy(filament); if(pressure!=null)Destroy(pressure); }
    }
}
