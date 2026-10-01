using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 질량-외곽 결합 후보")]
    public sealed class PurpleIdentityCoupledProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(2,24)] public float coreRadiance=8;
        [Range(0,.04f)] public float edgeBreakup=.028f;
        [Range(.5f,3)] public float fissureContrast=2.2f;
        [Range(.5f,3)] public float energyChurn=1.8f;
        [Range(0,1)] public float frontInterruption=.78f;
        [Range(1.1f,1.9f)] public float dischargeReach=1.62f;
        [Range(.5f,5)] public float dischargeEmission=2.8f;

        private static PurpleIdentityCoupledProfile current;
        public static PurpleIdentityCoupledProfile Current => current!=null?current:
            current=Resources.Load<PurpleIdentityCoupledProfile>("VFX/PurpleIdentityCoupledProfile");

        // One deterministic event clock feeds both the body interruption and outer discharge.
        public static void SampleEvents(float time,Vector4[] events)
        {
            for(int lane=0;lane<3;lane++)
            {
                float clock=time*(2.5f+lane*.58f)+lane*.37f;
                float epoch=Mathf.Floor(clock),age=Mathf.Repeat(clock,1);
                Vector3 axis=new Vector3(EventHash(epoch,lane,1.1f)-.5f,
                    EventHash(lane,epoch,2.3f)-.5f,EventHash(epoch,4.7f,lane)-.5f);
                axis=(axis+new Vector3(.001f,.002f,.003f)).normalized;
                if(lane==0)axis=(axis*.35f+Vector3.back*.9f).normalized;
                float life=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.12f))*
                    (1-Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.52f)/.4f)));
                events[lane]=new Vector4(axis.x,axis.y,axis.z,life);
            }
        }
        private static float EventHash(float x,float y,float z)
        {
            x=Mathf.Repeat(x*.3183099f+.17f,1)*19.19f;
            y=Mathf.Repeat(y*.3183099f+.31f,1)*19.19f;
            z=Mathf.Repeat(z*.3183099f+.53f,1)*19.19f;
            return Mathf.Repeat(x*y*z*(x+y+z),1);
        }
    }
}
