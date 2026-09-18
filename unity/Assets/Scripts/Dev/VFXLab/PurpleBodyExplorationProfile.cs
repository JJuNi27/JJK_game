using UnityEngine;

namespace JJKGame.Dev.PurpleBodyExploration
{
    /// <summary>Shared experimental controls. No production profile or casting dependency.</summary>
    [CreateAssetMenu(menuName="JJK Game/시각 탐색/Purple 본체 공통 조건")]
    public sealed class PurpleBodyExplorationProfile : ScriptableObject
    {
        [Range(2f,8f)] public float bodyDiameter=5f;
        [Range(.12f,.22f)] public float coreRatio=.175f;
        [Range(.4f,2f)] public float density=1.15f;
        [Range(.2f,1.5f)] public float bodyEmission=.7f;
        [Range(2f,12f)] public float coreEmission=7f;
        [ColorUsage(false,true)] public Color nearBlack=new Color(.008f,.0015f,.020f);
        [ColorUsage(false,true)] public Color deepViolet=new Color(.065f,.008f,.18f);
        [ColorUsage(false,true)] public Color magenta=new Color(.8f,.016f,.27f);
        [ColorUsage(false,true)] public Color violet=new Color(.26f,.035f,.9f);
        [ColorUsage(false,true)] public Color hotPink=new Color(1.8f,.055f,.65f);
        [Range(.03f,.16f)] public float brokenSilhouette=.10f;
        [Range(.5f,1.5f)] public float depthStrength=1f;
        [Range(.2f,1.5f)] public float inwardSpeed=.75f;
        [Range(.1f,1.2f)] public float surfaceSpeed=.42f;
        [Range(.2f,2f)] public float outwardSpeed=1.1f;
    }
}
