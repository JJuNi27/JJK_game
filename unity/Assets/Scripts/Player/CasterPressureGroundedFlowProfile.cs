using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/시전자 하체 풍압 후보")]
    public sealed class CasterPressureGroundedFlowProfile : ScriptableObject
    {
        public bool candidateEnabled;
        // Positions are relative to the feet, not to the character controller centre.
        public Vector3[] path={new Vector3(-3.4f,.16f,1.5f),new Vector3(-1.8f,.28f,-.1f),new Vector3(-.8f,.42f,-1.25f),new Vector3(.7f,.6f,-1.3f),new Vector3(1.5f,.85f,.05f),new Vector3(.7f,.58f,1.65f),new Vector3(-1.2f,.22f,2.5f)};
        [Range(.5f,2)] public float breadth=1.15f;
        [Range(.1f,.8f)] public float depth=.36f;
        [Range(.3f,1)] public float density=.96f;
        [Range(.3f,1.2f)] public float broadSeconds=.62f;
        [Range(.15f,.7f)] public float innerSeconds=.37f;
        [Range(.07f,.3f)] public float crestSeconds=.14f;
        [Range(1,4)] public float crestEmission=2.4f;
        [Range(1,1.8f)] public float fusionDensityBoost=1.3f;
        [Range(.1f,.6f)] public float releaseSeconds=.32f;
        [Range(1,6)] public float forwardSweep=3.8f;
        public float fallbackFootOffset=-1;
        static CasterPressureGroundedFlowProfile current;
        public static CasterPressureGroundedFlowProfile Current=>current!=null?current:current=Resources.Load<CasterPressureGroundedFlowProfile>("VFX/CasterPressureGroundedFlowProfile");
    }
}
