using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    // Runtime-only experiment. Never saves or repairs the original profiles or combat data.
    [InitializeOnLoad]
    public static class CursedEnergyAuraSilhouettePreview
    {
        private static CursedEnergyAuraProfile original,preview;
        private static CursedEnergyAuraController target;
        static CursedEnergyAuraSilhouettePreview()
        {
            EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.ExitingPlayMode)Restore();};
            AssemblyReloadEvents.beforeAssemblyReload+=Restore;
        }
        public static void Candidate(CursedEnergyAuraProfile p)
        {
            p.wisps.enabled=false;p.preserveLocalFlameShape=true;p.extraFieldReach=.65f;
            p.inkBoundaryScale=1.3f;p.inkInteriorArea=.8f;p.detachedTips=true;
        }
        [MenuItem("JJK Game/시부야 PASS4B3b/통합 후보/새 후보 곡선으로 비교 (Play Mode 전용)")]
        public static void NormalCurve()=>Show(-1);
        [MenuItem("JJK Game/시부야 PASS4B3b/통합 후보/배율 1.00 미리보기 (Play Mode 전용)")]
        public static void One()=>Show(1);
        [MenuItem("JJK Game/시부야 PASS4B3b/통합 후보/배율 1.70 미리보기 (Play Mode 전용)")]
        public static void Medium()=>Show(1.7f);
        [MenuItem("JJK Game/시부야 PASS4B3b/통합 후보/배율 2.70 미리보기 (Play Mode 전용)")]
        public static void Large()=>Show(2.7f);
        [MenuItem("JJK Game/시부야 PASS4B3b/통합 후보/배율 3.00 미리보기 (Play Mode 전용)")]
        public static void Largest()=>Show(3);
        [MenuItem("JJK Game/시부야 PASS4B3b/기존 프로필로 복귀")]
        public static void Restore()
        {
            if(target!=null&&original!=null)target.Configure(original);
            if(preview!=null)Object.DestroyImmediate(preview);preview=null;original=null;target=null;
        }
        private static void Show(float multiplier)
        {
            ShowProfile(p=>{
                Candidate(p);
                if(multiplier>0)p.maxEnergySizeCurve=AnimationCurve.Constant(0,10,multiplier);
                else p.maxEnergySizeCurve=new AnimationCurve(new Keyframe(.5f,.85f),new Keyframe(1,1),new Keyframe(1.5f,1.7f),new Keyframe(2,2.7f),new Keyframe(3,3));
            },"PASS4B3b 실험 후보 — 저장하지 않음");
        }
        public static void ShowProfile(System.Action<CursedEnergyAuraProfile> configure,string label)
        {
            if(!EditorApplication.isPlaying) {Debug.Log("PASS4B3b 후보는 Play Mode에서만 사용합니다. 저장 자산은 변경하지 않습니다.");return;}
            Restore();var match=Object.FindFirstObjectByType<ShibuyaMatch>();if(match==null)return;
            target=match.Player.GetComponent<CursedEnergyAuraController>();if(target==null)return;
            original=target.Profile;preview=Object.Instantiate(original);preview.name=label;preview.hideFlags=HideFlags.DontSave;
            configure(preview);
            target.Configure(preview);Selection.activeObject=preview;
            Debug.Log(label+" 활성화. F 홀드로 확인하세요. 최대 주력량·원본 프로필은 변경되지 않습니다.");
        }
    }
}
