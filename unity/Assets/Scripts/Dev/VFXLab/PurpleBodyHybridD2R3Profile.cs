using UnityEngine;
namespace JJKGame.Dev.PurpleBodyExploration
{
    [CreateAssetMenu(menuName="JJK Game/시각 탐색/Purple D2-R3 설정")]
    public sealed class PurpleBodyHybridD2R3Profile : ScriptableObject
    {
        public PurpleBodyHybridD2R2Profile frozenR2;
        [Range(1f,4f)] public float shellEmission=1.8f;
        [Range(.2f,2f)] public float shellDensity=.8f;
        [Range(4f,30f)] public float internalEnergy=22f;
        [Range(.2f,1.2f)] public float inwardSpeed=.58f;
        [Range(.1f,1.5f)] public float turbulenceSpeed=.8f;
        [Range(.015f,.12f)] public float arcRadius=.10f;
        [Range(6f,18f)] public float plasmaScale=11f;
        [Range(.05f,.20f)] public float arcLifetime=.105f;
        [Range(1f,1.7f)] public float outerReach=1.4f;
        [Range(.3f,1.5f)] public float instability=1f;
        [Range(.2f,2f)] public float voidContrast=1f;
    }
}
