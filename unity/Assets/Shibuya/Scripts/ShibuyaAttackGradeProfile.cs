using UnityEngine;
namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName="JJK/시부야/치명타·흑섬·Zone",fileName="Shibuya Attack Grades")]
    public sealed class ShibuyaAttackGradeProfile : ScriptableObject
    {
        [Range(0,100)] public float criticalChance=25;
        [Min(1)] public float criticalMultiplier=1.5f;
        public bool allowCriticalModifiers=true;
        public bool blackFlashCapable; // Character capability is explicitly opt-in, independent of criticals.
        [Range(0,100)] public float normalConditional=15,zoneConditional=24,stackConditional=4;
        [Range(0,100)] public float normalFinalCap=15,zoneFinalCap=20;
        [Min(1)] public float blackFlashMultiplier=2.5f;
        [Min(0)] public float blackFlashEnergy=10,zoneDuration=12;
        [Range(0,100)] public float zoneDamageBonus=10,zoneCostReduction=15,zoneChargeBonus=20;
    }
}
