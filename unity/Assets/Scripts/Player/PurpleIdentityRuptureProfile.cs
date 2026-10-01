using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 수렴 파열 후보")]
    public sealed class PurpleIdentityRuptureProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(2,24)] public float coreRadiance=8;
        [Range(0,.04f)] public float edgeBreakup=.028f;
        [Range(.5f,3)] public float fissureContrast=2.2f;
        [Range(.5f,3)] public float energyChurn=1.8f;
        [Range(0,1)] public float frontOpening=.94f;
        [Range(.03f,.2f)] public float tearWidth=.105f;
        [Range(1.1f,1.9f)] public float dischargeReach=1.55f;
        [Range(.5f,5)] public float dischargeEmission=2.8f;

        private static PurpleIdentityRuptureProfile current;
        public static PurpleIdentityRuptureProfile Current => current!=null?current:
            current=Resources.Load<PurpleIdentityRuptureProfile>("VFX/PurpleIdentityRuptureProfile");

        // World-space event directions are shared by the body opening and emission.
        public static void SampleEvents(float time,Vector4[] events)
        {
            for(int lane=0;lane<3;lane++)
            {
                float clock=time*(lane==0?2.11f:lane==1?2.74f:3.15f)+lane*.39f;
                float epoch=Mathf.Floor(clock),age=Mathf.Repeat(clock,1);
                Vector3 axis=new Vector3(Hash(epoch*3+lane*17)-.5f,Hash(epoch*7+lane*29)-.5f,Hash(epoch*11+lane*43)-.5f).normalized;
                if(lane==0)axis=(axis*.35f+new Vector3(-.25f,.36f,-.87f)).normalized;
                if(lane==1)axis=(axis*.35f+new Vector3(.83f,.18f,-.48f)).normalized;
                float pulse=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.055f))*
                    (1-Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.72f)/.24f)));
                events[lane]=new Vector4(axis.x,axis.y,axis.z,pulse);
            }
        }
        private static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+31.7f)*43758.5453f,1);
    }
}
