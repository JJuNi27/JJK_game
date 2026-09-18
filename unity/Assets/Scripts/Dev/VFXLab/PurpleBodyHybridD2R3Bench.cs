using UnityEngine;
namespace JJKGame.Dev.PurpleBodyExploration
{
    public sealed class PurpleBodyHybridD2R3Bench : MonoBehaviour
    {
        private static readonly Vector3 Offset=new Vector3(1280,0,0);
        public PurpleBodyExplorationBench Common { get; private set; }
        public PurpleBodyHybridD2R2Prototype Frozen { get; private set; }
        public PurpleBodyHybridD2R3Prototype Hybrid { get; private set; }
        public bool ShowR3 { get; private set; }=true;
        public bool Playing { get; set; }
        public void Configure(PurpleBodyHybridD2R3Profile profile,Camera camera)
        {
            if(Common!=null)return;
            Common=gameObject.AddComponent<PurpleBodyExplorationBench>();Common.Configure(profile.frozenR2.frozenR1.frozenD2.preservedD.commonConditions,camera);
            transform.position+=Offset;
            for(int i=0;i<3;i++)Common.Candidate((PurpleBodyVariant)i).gameObject.SetActive(false);
            var frozen=new GameObject("Frozen_R2_Comparison");frozen.transform.SetParent(transform,false);frozen.transform.position=Common.Centre+Offset;
            Frozen=frozen.AddComponent<PurpleBodyHybridD2R2Prototype>();Frozen.Configure(profile.frozenR2);
            var refined=new GameObject("CodexPurpleBody_D2R3");refined.transform.SetParent(transform,false);refined.transform.position=Common.Centre+Offset;
            Hybrid=refined.AddComponent<PurpleBodyHybridD2R3Prototype>();Hybrid.Configure(profile);
            Select(true);Sample(1.5f,PurpleBenchmarkView.Front);
        }
        public void Select(bool refined){ShowR3=refined;Frozen.gameObject.SetActive(!refined);Hybrid.gameObject.SetActive(refined);}
        public void Sample(float time,PurpleBenchmarkView view)
        {
            Common.Playing=false;Common.Sample(time,view);Common.BenchmarkCamera.transform.position+=Offset;
            Frozen.Render(Common.Clock);Hybrid.Render(Common.Clock);
        }
        private void Update(){if(!Playing || Common==null)return;Sample(Common.Clock+Time.unscaledDeltaTime,Common.View);if(Common.Clock>=3)Playing=false;}
    }
}
