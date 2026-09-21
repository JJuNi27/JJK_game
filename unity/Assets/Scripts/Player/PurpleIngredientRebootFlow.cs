using UnityEngine;
namespace JJKGame.Player
{
    internal static class PurpleIngredientRebootFlow
    {
        public static void Apply(LineRenderer line,Vector3[] buffer,PurpleIngredientRebootProfile profile,bool blue,bool mote,int index,float clock,float fusion)
        {
            float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.72f,fusion));
            if(blend<=0)return;
            bool band=!mote && index<6;
            float speed=blue?profile.blueCollapseSpeed:profile.redPressureSpeed;
            int group=band?index/2:index;
            float phase=Mathf.Repeat(clock*speed*(mote?1.35f:1)+group*(band?.31f:.173f),1);
            float headRadius=blue?Mathf.Lerp(profile.bandReach,.82f,phase):Mathf.Lerp(1.02f,profile.bandReach,phase);
            Quaternion plane=band?Quaternion.Euler(34+group*53,group*71+15,group*37):Quaternion.Euler(index*39,index*79,17);
            int count=mote?6:32;
            Vector3 oldHead=line.GetPosition(0),oldTail=line.GetPosition(line.positionCount-1);
            line.positionCount=count;
            for(int j=0;j<count;j++)
            {
                float u=j/(float)(count-1),radius=headRadius,angle;
                if(band)
                {
                    angle=u*(blue?2.1f:1.7f)+(index%2)*Mathf.PI+group*.7f+(blue?phase*1.7f:phase*.22f);
                    radius*=1+.025f*Mathf.Sin(angle*7+group+clock*9);
                }
                else
                {
                    float progress=Mathf.Max(0,phase-u*(mote?.12f:.23f));
                    radius=blue?Mathf.Lerp(profile.bandReach+.25f,.72f,progress):Mathf.Lerp(1.02f,profile.bandReach+.35f,progress);
                    angle=index*2.399f+(blue?progress*2.4f:.11f*Mathf.Sin(progress*15+index));
                }
                Vector3 target=plane*new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),.06f*Mathf.Sin(angle*3+group)).normalized*radius;
                Vector3 previous=mote?Vector3.Lerp(oldHead,oldTail,u):buffer[j];
                if(mote)line.SetPosition(j,Vector3.Lerp(previous,target,blend));else buffer[j]=Vector3.Lerp(previous,target,blend);
            }
            if(!mote)line.SetPositions(buffer);
            float fade=Mathf.Pow(Mathf.Sin(phase*Mathf.PI),.7f);
            float width=band?profile.bandWidth*(1+.22f*Mathf.Sin(clock*13+group)):profile.bandWidth*(mote?.25f:.43f);
            line.startWidth=Mathf.Lerp(line.startWidth,width,blend);line.endWidth=Mathf.Lerp(line.endWidth,band?width*.8f:.005f,blend);
            Color head=Color.white;head.a=fade*(band?.9f:profile.supportingFlow);
            Color tail=head;tail.a*=band?.7f:.1f;
            line.startColor=Color.Lerp(line.startColor,head,blend);line.endColor=Color.Lerp(line.endColor,tail,blend);
        }
    }
}
