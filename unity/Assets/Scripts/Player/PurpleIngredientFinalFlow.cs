using UnityEngine;
namespace JJKGame.Player
{
    internal static class PurpleIngredientFinalFlow
    {
        private static float Hash(float v)=>Mathf.Repeat(Mathf.Sin(v*127.1f+19.3f)*43758.5453f,1);
        public static void Apply(LineRenderer line,Vector3[] buffer,PurpleIngredientReboot2Profile p,bool blue,int index,float clock,float fusion)
        {
            float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.72f,fusion));if(blend<=0)return;
            // Reuse the ten existing arcs: six-point angular paths, not long smooth bands.
            float rate=(blue?p.collapseSpeed:p.pressureSpeed)*1.85f;
            float tick=clock*rate+index*.173f,epoch=Mathf.Floor(tick),phase=Mathf.Repeat(tick,1);
            float seed=index+epoch*7,duty=.62f+Hash(seed+23)*.12f;
            float advance=Mathf.Clamp01(phase/duty);
            Quaternion plane=Quaternion.Euler(index*39+Hash(seed)*40,index*71+19,index*47);
            float turn=blue?1.15f:.18f,span=.48f+Hash(seed+9)*.28f;
            for(int j=0;j<32;j++)
            {
                float u=j/31f,progress=Mathf.Clamp01(advance-u*.18f);
                // Keep enough of each brief rupture outside the opaque mass to read at caster distance.
                float r=blue?Mathf.Lerp(p.energyReach,.76f,Mathf.Pow(progress,1.45f)):Mathf.Lerp(1.01f,p.energyReach,Mathf.Pow(progress,.55f));
                float knot=u*5,k=Mathf.Floor(knot),f=knot-k;
                float jag=Mathf.Lerp(Hash(seed*13+k*3),Hash(seed*13+(k+1)*3),f)-.5f;
                float angle=index*2.399f+Hash(seed)*.9f+progress*turn-u*span+jag*.30f;
                Vector3 target=plane*new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),jag*.21f).normalized*r;
                buffer[j]=Vector3.Lerp(buffer[j],target,blend);
            }
            line.SetPositions(buffer);
            float fade=Mathf.SmoothStep(0,1,advance/.09f)*(1-Mathf.SmoothStep(.68f,1,advance));
            fade*=.72f+.28f*Mathf.Pow(Mathf.Sin(clock*103+seed),2);
            float width=p.bandWidth*(.66f+Hash(seed+4)*.32f);
            line.startWidth=Mathf.Lerp(line.startWidth,width,blend);line.endWidth=Mathf.Lerp(line.endWidth,.009f,blend);
            Color head=new Color(1,1,1,fade),tail=new Color(1,1,1,fade*.75f);
            line.startColor=Color.Lerp(line.startColor,head,blend);line.endColor=Color.Lerp(line.endColor,tail,blend);
        }
    }
}
