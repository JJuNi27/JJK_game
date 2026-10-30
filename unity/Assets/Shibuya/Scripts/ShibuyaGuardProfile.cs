using UnityEngine;

namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName="JJK Game/시부야/공통 가드 설정",fileName="Shibuya Guard")]
    public sealed class ShibuyaGuardProfile : ScriptableObject
    {
        public bool guardEnabled=true;
        public ShibuyaParryProfile parry;
        public Shader shieldShader;
        [Min(1)] public float maximumGuard=100;
        [Range(0,90)] public float halfAngle=60;
        [Range(0,1)] public float movementMultiplier=.4f;
        [Min(0)] public float recoveryDelay=1.2f,recoveryPerSecond=20,breakLockDuration=.8f;
        [Min(0)] public float fallbackGuardDamage=16;
        [Range(0,1)] public float stableThreshold=.7f,criticalThreshold=.4f;
        public Color stableColor=new Color(.2f,.85f,1,.18f);
        public Color strainedColor=new Color(.4f,.75f,1,.23f);
        public Color criticalColor=new Color(.75f,.65f,1,.3f);
        [Min(0)] public float impactStrength=.12f,criticalInstability=.4f,crackStrength=1;
        [Min(0.01f)] public float impactDuration=.18f,breakVisualDuration=.45f;
        [Min(0)] public float breakVisualStrength=.7f;
        [Min(.1f)] public float shieldRadius=.8f,shieldHeight=1.45f;
        public float shieldCenterHeight=1.1f;
        [Range(0,150)] public float guardArmAngle=80;
        [Range(0,60)] public float breakLeanAngle=25;
    }
}
