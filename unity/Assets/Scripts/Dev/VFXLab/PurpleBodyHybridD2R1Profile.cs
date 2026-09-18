using UnityEngine;
namespace JJKGame.Dev.PurpleBodyExploration
{
    [CreateAssetMenu(menuName="JJK Game/시각 탐색/Purple D2-R1 설정")]
    public sealed class PurpleBodyHybridD2R1Profile : ScriptableObject
    {
        public PurpleBodyHybridD2Profile frozenD2;
        [Range(1f,4f)] public float shellEmission=1.8f;
        [Range(.2f,2f)] public float shellDensity=.8f;
        [Range(4f,24f)] public float internalEnergy=16f;
        [Range(.2f,1.2f)] public float inwardSpeed=.58f;
        [Range(.1f,1.5f)] public float turbulenceSpeed=.7f;
        [Range(.15f,.7f)] public float heroWidth=.42f;
        [Range(1f,1.7f)] public float outerReach=1.32f;
        [Range(.3f,1.5f)] public float instability=1f;
    }
}
