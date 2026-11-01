using UnityEngine;

namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName = "JJK/시부야/공통 흑섬 VFX", fileName = "Shibuya Black Flash")]
    public sealed class ShibuyaBlackFlashProfile : ScriptableObject
    {
        public bool effectEnabled = true;
        [Min(.1f)] public float scale = 1;
        [Range(0, 5)] public float intensity = 1;
        [Min(.1f)] public float coreSize = .62f;
        [Range(1, 3)] public float coreExpansion = 1.65f;
        [ColorUsage(true, true)] public Color lightningColor = new Color(3.2f, .012f, .025f, 1);
        [Range(.02f, .4f)] public float branchWidth = .14f;
        [Min(.1f)] public float branchLength = 2.25f;
        [Range(3, 24)] public int branchCount = 11;
        [Range(0, 3)] public int forksPerBranch = 2;
        [Range(0, 1)] public float forwardBias = .32f;
        [Min(0)] public float flashIntensity = 5;
        [Min(.01f)] public float flashDuration = .045f;
        [Min(.02f)] public float coreDuration = .19f;
        [Min(.03f)] public float branchDuration = .28f;
        [Min(.1f)] public float pressureSize = 2.1f;
        [Min(.1f)] public float pressureSpeed = 11;
        [Range(0, 32)] public int smokeCount = 12;
        [Min(.02f)] public float smokeSize = .24f;
        [Min(.1f)] public float residueDuration = .62f;
        [Min(.1f)] public float duration = .75f;
        [Range(1, 16)] public int simultaneousLimit = 8;
        [Range(2, 8)] public int primaryBranchCount = 4;
        [Range(.15f, 1)] public float coreDensity = .42f;
        [Range(.03f, .2f)] public float ruptureGrowthTime = .095f;
        [Range(0, 1)] public float silhouetteChange = .32f;
        [Range(.02f, .12f)] public float dischargeInterval = .045f;
        [Range(.1f, .7f)] public float residualDischargeTime = .38f;
        [Range(.3f, 3)] public float smokeSpread = 1.8f;
        public Shader inkShader;
        public Shader lightShader;
        public float Lifetime => Mathf.Clamp(duration, .1f, 5);
    }
}
