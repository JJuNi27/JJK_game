using UnityEngine;
namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName="JJK/시부야/공통 잡기 설정")]
    public sealed class ShibuyaGrabProfile : ScriptableObject
    {
        public bool grabEnabled=true;
        public ShibuyaAttackMetadata attack=ShibuyaAttackMetadata.Grab();
        [Min(.1f)] public float range=1.55f;
        [Range(0,90)] public float halfAngle=50;
        [Min(.01f)] public float startup=.18f;
        [Min(.01f)] public float activeTime=.08f;
        [Min(.1f)] public float damage=15;
        [Min(0)] public float contactDuration=.10f;
        [Min(0)] public float pushDistance=1.4f;
        [Min(.01f)] public float victimStun=.40f;
        [Min(0)] public float successRecovery=.25f;
        [Min(0)] public float failureRecovery=.50f;
        [Min(0)] public float motionStrength=1;
    }
}
