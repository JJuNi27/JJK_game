using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/시전자 입체 풍압 단일 흐름")]
    public sealed class CasterPressure3DFlowProfile : ScriptableObject
    {
        public bool candidateEnabled;
        public Vector3[] path = {new Vector3(-4,.12f,2.5f),new Vector3(-2,.65f,1.4f),new Vector3(-.7f,1.25f,.9f),new Vector3(1.3f,1.8f,.7f),new Vector3(1.55f,2.6f,-.8f),new Vector3(.1f,3.1f,-2.3f),new Vector3(-2.1f,3.25f,-3.1f)};
        [Range(.6f,2.5f)] public float breadth=1.65f;
        [Range(.2f,1.5f)] public float depth=.85f;
        [Range(.3f,1)] public float density=.86f;
        [Range(.5f,3)] public float flowSpeed=1.25f;
        [Range(1,5)] public float crestEmission=2.6f;
        [Range(.1f,.6f)] public float releaseSeconds=.32f;
        [Range(1,6)] public float forwardSweep=3.8f;
        static CasterPressure3DFlowProfile current;
        public static CasterPressure3DFlowProfile Current=>current!=null?current:current=Resources.Load<CasterPressure3DFlowProfile>("VFX/CasterPressure3DFlowProfile");
    }
}
