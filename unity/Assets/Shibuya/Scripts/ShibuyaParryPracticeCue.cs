#if UNITY_EDITOR
using System;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    // Opt-in Editor practice overlay only; never decides an input, hit or parry result.
    public sealed class ShibuyaParryPracticeCue : MonoBehaviour
    {
        [NonSerialized] public float secondsUntilHit=-1;
        private ShibuyaFighter owner;
        private string message;
        private float messageUntil;
        public bool CombatEnded=>owner==null||owner.Health.IsDead||owner.State==ShibuyaFighterState.Finished;
        private void Awake(){owner=GetComponent<ShibuyaFighter>();}
        private void OnEnable(){owner.ParrySucceeded+=Parried;owner.GuardBlocked+=Blocked;owner.Health.DamageResolved+=Damaged;}
        private void OnDisable(){if(owner==null)return;owner.ParrySucceeded-=Parried;owner.GuardBlocked-=Blocked;owner.Health.DamageResolved-=Damaged;}
        private void Parried(ShibuyaFighter f){message="패링 성공 — 직접 다음 행동을 선택하세요";messageUntil=Time.unscaledTime+1;}
        private void Blocked(ShibuyaFighter f){message="일반 가드";messageUntil=Time.unscaledTime+1;}
        private void Damaged(Health h,DamageContext c,DamageResolution r){if(r==DamageResolution.Applied){message="피격";messageUntil=Time.unscaledTime+1;}}
        private void OnGUI()
        {
            if(owner==null||owner.Health.IsDead||owner.State==ShibuyaFighterState.Finished)return;
            string text=Time.unscaledTime<messageUntil?message:secondsUntilHit>=0?$"개발용 패링 연습 — 공격 예고 {secondsUntilHit:F2}초":"개발용 패링 연습";
            GUI.Box(new Rect(Screen.width*.5f-220,30,440,36),text);
        }
    }
}
#endif
