using UnityEngine;

namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName="JJK Game/시부야/공통 패링 설정",fileName="Shibuya Parry")]
    public sealed class ShibuyaParryProfile : ScriptableObject
    {
        public bool parryEnabled=true;
        [Min(0)] public float windowDuration=.12f,retryInterval=.5f;
        [Range(0,90)] public float halfAngle=60;
        [Min(0)] public float attackerStun=.35f,pushDistance=1.2f,successMotionDuration=.1f;
        [Min(0)] public float localHitStop;
        public Shader effectShader;
        public Color effectColor=new Color(.5f,.95f,1,1);
        [Min(0)] public float effectIntensity=2.8f,motionStrength=1;
        [Min(.01f)] public float effectSize=.9f,effectDuration=.22f;
    }
}
