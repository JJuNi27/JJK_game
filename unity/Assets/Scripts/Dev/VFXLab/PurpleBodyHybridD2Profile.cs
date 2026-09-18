using UnityEngine;
namespace JJKGame.Dev.PurpleBodyExploration
{
    [CreateAssetMenu(menuName="JJK Game/시각 탐색/Purple Hybrid D2 설정")]
    public sealed class PurpleBodyHybridD2Profile : ScriptableObject
    {
        public PurpleBodyHybridDProfile preservedD;
        [Range(1f,5f)] public float luminosity=2.8f;
        [Range(.1f,.6f)] public float darkSeparation=.48f;
        [Range(2f,12f)] public float plasmaEnergy=7.5f;
        [Range(.2f,1.2f)] public float inwardSpeed=.58f;
        [Range(.1f,1.5f)] public float turbulenceSpeed=.7f;
        [Range(.025f,.25f)] public float ruptureWidth=.20f;
        [Range(.5f,3f)] public float ruptureEnergy=1.4f;
    }
}
