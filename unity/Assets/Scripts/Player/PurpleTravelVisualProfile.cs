using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 이동 표현 설정")]
    public sealed class PurpleTravelVisualProfile : ScriptableObject
    {
        public bool travelRefinementEnabled=true;
        [Min(.1f)] public float referenceSpeed=30;
        [Range(.06f,.2f)] public float residueLifetime=.14f;
        [Range(0,1)] public float partialInheritance=.15f;
        [Range(.1f,1.5f)] public float residueRadiusLimit=1.3f;
        [Range(.5f,2f)] public float wakeLengthRadii=1.35f;
        [Range(.5f,1.5f)] public float wakeWidthRadii=1.12f;
        [Range(.1f,3f)] public float wakeEmission=1.15f;
        [Range(0,2)] public float releaseBoost=.8f;
        [Range(.06f,.35f)] public float releasePeakSeconds=.18f;
        [Range(0,.6f)] public float haloRearStretch=.35f;
        [Range(0,1)] public float rearDistortion=.8f;
    }
}
