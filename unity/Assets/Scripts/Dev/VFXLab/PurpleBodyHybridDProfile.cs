using UnityEngine;

namespace JJKGame.Dev.PurpleBodyExploration
{
    [CreateAssetMenu(menuName="JJK Game/시각 탐색/Purple Hybrid D 설정")]
    public sealed class PurpleBodyHybridDProfile : ScriptableObject
    {
        public PurpleBodyExplorationProfile commonConditions;
        [Range(.005f,.05f)] public float tearingAmplitude=.032f;
        [Range(.2f,1.2f)] public float implosionSpeed=.58f;
        [Range(.1f,1f)] public float surfaceSpeed=.24f;
        [Range(0f,1f)] public float internalDepth=.98f;
        [Range(.5f,4f)] public float streamEnergy=3.3f;
        [Range(0f,1f)] public float edgeEnergy=.25f;
    }
}
