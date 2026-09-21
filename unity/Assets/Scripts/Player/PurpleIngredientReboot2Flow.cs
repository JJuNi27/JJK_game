using UnityEngine;
namespace JJKGame.Player
{
    internal static class PurpleIngredientReboot2Flow
    {
        private static float Hash(float v)=>Mathf.Repeat(Mathf.Sin(v*127.1f+19.3f)*43758.5453f,1);
        public static void Apply(LineRenderer line,Vector3[] buffer,PurpleIngredientReboot2Profile p,bool blue,bool mote,int index,float clock,float fusion)
        {
            float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.72f,fusion));if(blend<=0)return;
            bool band=!mote && index<6,rupture=!mote && !band;
            float speed=blue?p.collapseSpeed:p.pressureSpeed;
            float tick=clock*(rupture?2.4f:speed)+(band?index*.193f:index*.173f);
            float epoch=Mathf.Floor(tick),phase=Mathf.Repeat(tick,1);
            float advance=rupture?Mathf.Clamp01(phase/(p.ruptureDuration*2.4f)):phase;
            float seed=index+epoch*7;
            Quaternion plane=Quaternion.Euler(index*39+Hash(seed)*40,index*71+19,index*47);
            int count=mote?6:32;Vector3 oldHead=line.GetPosition(0),oldTail=line.GetPosition(line.positionCount-1);line.positionCount=count;
            for(int j=0;j<count;j++)
            {
                float u=j/(float)(count-1),r,angle;
                if(band)
                {
                    float travel=blue?Mathf.Pow(advance,.7f):Mathf.Pow(advance,.8f);
                    r=blue?Mathf.Lerp(p.energyReach,.76f,travel):Mathf.Lerp(1.01f,p.energyReach,travel);
                    angle=index*2.399f+u*(1.25f+Hash(seed)*.65f)+(blue?advance*1.2f:advance*.13f);
                    r*=1+.055f*Mathf.Sin(angle*9+seed)+.025f*Mathf.Sin(angle*23-seed);
                }
                else
                {
                    float progress=Mathf.Max(0,advance-u*(mote?.19f:.35f));
                    r=blue?Mathf.Lerp(p.energyReach+.25f,.72f,progress):Mathf.Lerp(1.01f,p.energyReach+.35f,progress);
                    angle=index*2.399f+Hash(seed)*.9f+(blue?progress*1.9f:progress*.1f);
                    if(rupture)angle+=(Mathf.PingPong(u*7+seed,1)-.5f)*.16f*Mathf.Sin(u*Mathf.PI);
                }
                Vector3 target=plane*new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),.12f*Mathf.Sin(angle*4+seed)).normalized*r;
                Vector3 previous=mote?Vector3.Lerp(oldHead,oldTail,u):buffer[j];
                if(mote)line.SetPosition(j,Vector3.Lerp(previous,target,blend));else buffer[j]=Vector3.Lerp(previous,target,blend);
            }
            if(!mote)line.SetPositions(buffer);
            float fade=Mathf.Pow(Mathf.Max(0,Mathf.Sin(advance*Mathf.PI)),.65f);
            if(rupture)fade*=.65f+.35f*Mathf.Pow(Mathf.Sin(clock*83+index),2);
            float width=p.bandWidth*(band?(.8f+Hash(seed)*.5f):mote?.22f:.6f);
            line.startWidth=Mathf.Lerp(line.startWidth,width,blend);line.endWidth=Mathf.Lerp(line.endWidth,band?width*.38f:.004f,blend);
            Color head=Color.white;head.a=fade*(mote?.65f:1);
            Color tail=head;tail.a*=band?.65f:.05f;
            line.startColor=Color.Lerp(line.startColor,head,blend);line.endColor=Color.Lerp(line.endColor,tail,blend);
        }
    }
}
