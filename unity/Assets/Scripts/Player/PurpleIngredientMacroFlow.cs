using UnityEngine;
namespace JJKGame.Player
{
    /// <summary>Advected trails on the existing ingredient lines, without additional emitters.</summary>
    internal static class PurpleIngredientMacroFlow
    {
        private static float Hash(float seed)=>Mathf.Repeat(Mathf.Sin(seed*127.1f+31.7f)*43758.5453f,1);
        private static Vector3 Path(bool blue,float progress,int index,float epoch,float reach)
        {
            float seed=index*2.399f+Hash(index+epoch*7)*1.7f;
            Quaternion plane=Quaternion.Euler(index*31+17+Hash(epoch+index)*45,index*73,index*47);
            float radius=blue?Mathf.Lerp(reach,.55f,Mathf.Pow(progress,.8f)):Mathf.Lerp(1.02f,reach,progress);
            float angle=seed+(blue?3.2f*Mathf.Pow(progress,1.15f):.14f*Mathf.Sin(progress*13+seed));
            Vector3 direction=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),Mathf.Sin(progress*(blue?5:13)+seed)*(blue?.22f:.16f));
            return plane*direction.normalized*radius;
        }
        public static void Apply(LineRenderer line,Vector3[] buffer,PurpleIngredientMacroProfile profile,bool blue,bool mote,int index,float clock,float fusion,Color tint)
        {
            if(fusion>=.72f)return;
            float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.72f,fusion));
            float tick=clock*(blue?profile.blueSpeed:profile.redSpeed)*(mote?1.2f:1)+index*(mote?.173f:.137f);
            float phase=Mathf.Repeat(tick,1),epoch=Mathf.Floor(tick);
            float advance=blue?phase:Mathf.Clamp01(phase/.65f);
            float span=profile.trailSpan*(mote?.7f:1);
            int count=mote?6:32;
            Vector3 oldHead=line.GetPosition(0),oldTail=line.GetPosition(line.positionCount-1);
            line.positionCount=count;
            for(int j=0;j<count;j++)
            {
                float u=j/(float)(count-1);
                Vector3 previous=mote?Vector3.Lerp(oldHead,oldTail,u):buffer[j];
                Vector3 target=Path(blue,Mathf.Max(0,advance-u*span),index+(mote?17:0),epoch,profile.flowReach+(mote?.2f:0));
                Vector3 value=Vector3.Lerp(previous,target,blend);
                if(mote)line.SetPosition(j,value);else buffer[j]=value;
            }
            if(!mote)line.SetPositions(buffer);
            float fade=blue?Mathf.Pow(Mathf.Sin(phase*Mathf.PI),.7f):Mathf.Sin(advance*Mathf.PI)*(.55f+.45f*Mathf.Pow(Mathf.Sin(clock*43+index),2));
            float width=(blue?profile.blueWidth:profile.redWidth)*(mote?.44f:(index%3==0?1.35f:.8f));
            line.startWidth=Mathf.Lerp(line.startWidth,width,blend);line.endWidth=Mathf.Lerp(line.endWidth,mote?.004f:.012f,blend);
            // A hot pressure front needs luminance in grayscale, not a larger white core.
            Color head=(blue?tint:tint+new Color(0,.24f,.30f,0))*(blue?profile.blueEmission:profile.redEmission);head.a=fade*.95f;
            Color tail=tint*(blue?profile.blueEmission:profile.redEmission);tail.a=fade*.035f;
            line.startColor=Color.Lerp(line.startColor,head,blend);line.endColor=Color.Lerp(line.endColor,tail,blend);
        }
    }
}
