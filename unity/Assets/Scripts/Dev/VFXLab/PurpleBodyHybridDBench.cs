using UnityEngine;

namespace JJKGame.Dev.PurpleBodyExploration
{
    /// <summary>Composes the preserved ABC bench and an owned D body; no change to the original ABC implementation.</summary>
    public sealed class PurpleBodyHybridDBench : MonoBehaviour
    {
        // Keep the preserved ABC window usable at the same time, outside either preview camera's far plane.
        private static readonly Vector3 StageOffset=new Vector3(256,0,0);
        public PurpleBodyExplorationBench CommonBench { get; private set; }
        public PurpleBodyHybridDPrototype Hybrid { get; private set; }
        public int Selected { get; private set; }=3;
        public bool Playing { get; set; }
        public Vector3 Centre=>CommonBench.Centre+StageOffset;
        public void Configure(PurpleBodyHybridDProfile profile,Camera labCamera,int width=960,int height=540)
        {
            if(CommonBench!=null)return;
            CommonBench=gameObject.AddComponent<PurpleBodyExplorationBench>();CommonBench.Configure(profile.commonConditions,labCamera,width,height);
            transform.position+=StageOffset;
            var go=new GameObject("CodexPurpleBodyPrototype_D");go.transform.SetParent(transform,false);go.transform.position=Centre;
            Hybrid=go.AddComponent<PurpleBodyHybridDPrototype>();Hybrid.Configure(profile);
            Select(3);Sample(1.5f,PurpleBenchmarkView.Front);
        }
        public void Select(int variant)
        {
            Selected=Mathf.Clamp(variant,0,3);
            CommonBench.Select((PurpleBodyVariant)Mathf.Min(Selected,2));
            if(Selected==3)for(int i=0;i<3;i++)CommonBench.Candidate((PurpleBodyVariant)i).gameObject.SetActive(false);
            Hybrid.gameObject.SetActive(Selected==3);
        }
        public void Sample(float time,PurpleBenchmarkView view)
        {
            CommonBench.Playing=false;CommonBench.Sample(time,view);Hybrid.Render(CommonBench.Clock);
            CommonBench.BenchmarkCamera.transform.position+=StageOffset;
        }
        private void Update()
        {
            if(!Playing || CommonBench==null)return;
            Sample(CommonBench.Clock+Time.unscaledDeltaTime,CommonBench.View);
            if(CommonBench.Clock>=3)Playing=false;
        }
    }
}
