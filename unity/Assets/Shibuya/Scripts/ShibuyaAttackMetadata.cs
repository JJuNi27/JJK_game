using System;
using UnityEngine;
namespace JJKGame.Shibuya
{
    public enum ShibuyaAttackReach { [InspectorName("미분류")] Unspecified, [InspectorName("근접")] Melee, [InspectorName("원거리")] Ranged, [InspectorName("특수")] Special }
    public enum ShibuyaDamageNature { [InspectorName("미분류")] Unspecified, [InspectorName("체술")] Taijutsu, [InspectorName("술식")] Technique, [InspectorName("복합")] Hybrid }
    public enum ShibuyaAttackOrigin { [InspectorName("미분류")] Unspecified, [InspectorName("평타")] Basic, [InspectorName("스킬")] Skill, [InspectorName("반격")] Counter, [InspectorName("궁극기")] Ultimate, [InspectorName("기타")] Other }
    [Serializable]
    public sealed class ShibuyaAttackMetadata
    {
        public ShibuyaAttackReach reach;
        public ShibuyaDamageNature nature;
        public ShibuyaAttackOrigin origin;
        public bool criticalAllowed;
        public bool blackFlashAllowed;
        public bool directHybridHit;
        public bool CanBlackFlash=>blackFlashAllowed&&reach==ShibuyaAttackReach.Melee
            &&(nature==ShibuyaDamageNature.Taijutsu||(nature==ShibuyaDamageNature.Hybrid&&directHybridHit));
        public static ShibuyaAttackMetadata Basic()=>new ShibuyaAttackMetadata{reach=ShibuyaAttackReach.Melee,nature=ShibuyaDamageNature.Taijutsu,origin=ShibuyaAttackOrigin.Basic,criticalAllowed=true,blackFlashAllowed=true};
        public static ShibuyaAttackMetadata Grab()=>new ShibuyaAttackMetadata{reach=ShibuyaAttackReach.Melee,nature=ShibuyaDamageNature.Taijutsu,origin=ShibuyaAttackOrigin.Other};
    }
}
