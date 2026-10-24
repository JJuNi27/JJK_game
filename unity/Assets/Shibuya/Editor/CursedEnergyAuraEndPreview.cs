using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    public static class CursedEnergyAuraEndPreview
    {
        public static void Candidate(CursedEnergyAuraProfile p)
        {
            CursedEnergyAuraSilhouettePreview.Candidate(p);p.upperFieldConnection=1;p.lowerFieldConnection=1;
        }
        // Only the 200-energy knot and following values change. 100/150 remain the source values.
        public static AnimationCurve CurveAt200(CursedEnergyAuraProfile p,float multiplier)
        {
            var source=p.maxEnergySizeCurve??CursedEnergyAuraProfile.DefaultSizeCurve();float ratio=200/p.ReferenceMaxEnergy;
            float delta=multiplier-source.Evaluate(ratio);var keys=source.keys.ToList();bool found=false;
            for(int i=0;i<keys.Count;i++)if(keys[i].time>=ratio) {var key=keys[i];key.value+=delta;found|=Mathf.Abs(key.time-ratio)<.00001f;keys[i]=key;}
            if(!found)keys.Add(new Keyframe(ratio,multiplier));
            return new AnimationCurve(keys.ToArray()){preWrapMode=source.preWrapMode,postWrapMode=source.postWrapMode};
        }
        private static void Show(float multiplier,bool connect,bool force)
        {
            CursedEnergyAuraSilhouettePreview.ShowProfile(p=>{
                if(connect)Candidate(p);else CursedEnergyAuraSilhouettePreview.Candidate(p);
                p.maximumSizeMultiplier=Mathf.Max(4,p.maximumSizeMultiplier);
                p.maxEnergySizeCurve=force?AnimationCurve.Constant(0,10,multiplier):CurveAt200(p,multiplier);
            },"PASS4B3c "+(connect?"상하 연결":"기존 4B3b")+" x"+multiplier.ToString("0.00")+" — 임시 미리보기");
        }
        [MenuItem("JJK Game/시부야 PASS4B3c/연결 후보 강제 배율 미리보기/2.70 (실제 주력량 불변)")]
        public static void Size270()=>Show(2.7f,true,true);
        [MenuItem("JJK Game/시부야 PASS4B3c/연결 후보 강제 배율 미리보기/3.00 (실제 주력량 불변)")]
        public static void Size300()=>Show(3,true,true);
        [MenuItem("JJK Game/시부야 PASS4B3c/연결 후보 강제 배율 미리보기/3.50 (실제 주력량 불변)")]
        public static void Size350()=>Show(3.5f,true,true);
        [MenuItem("JJK Game/시부야 PASS4B3c/연결 후보 강제 배율 미리보기/4.00 (실제 주력량 불변)")]
        public static void Size400()=>Show(4,true,true);
        [MenuItem("JJK Game/시부야 PASS4B3c/200 기준 곡선 실험/2.70")]
        public static void Curve270()=>Show(2.7f,true,false);
        [MenuItem("JJK Game/시부야 PASS4B3c/200 기준 곡선 실험/3.00")]
        public static void Curve300()=>Show(3,true,false);
        [MenuItem("JJK Game/시부야 PASS4B3c/200 기준 곡선 실험/3.50")]
        public static void Curve350()=>Show(3.5f,true,false);
        [MenuItem("JJK Game/시부야 PASS4B3c/200 기준 곡선 실험/4.00")]
        public static void Curve400()=>Show(4,true,false);
        [MenuItem("JJK Game/시부야 PASS4B3c/비교/기존 4B3b 3.00")]
        public static void Before()=>Show(3,false,true);
        [MenuItem("JJK Game/시부야 PASS4B3c/기존 저장 프로필로 복귀")]
        public static void Restore()=>CursedEnergyAuraSilhouettePreview.Restore();
    }
}
