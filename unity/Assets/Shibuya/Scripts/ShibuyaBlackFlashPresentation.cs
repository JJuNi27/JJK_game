using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    // One observer owned by the scene's match; independent of cameras, HUD options and actor life.
    [DisallowMultipleComponent]
    public sealed class ShibuyaBlackFlashPresentation : MonoBehaviour
    {
        readonly List<ShibuyaBlackFlashVisual> pool=new List<ShibuyaBlackFlashVisual>(16);
        readonly HashSet<ShibuyaDamageResultKey> seen=new HashSet<ShibuyaDamageResultKey>();
        Material ink,smokeMaterial,lightMaterial;bool subscribed;
        public ShibuyaBlackFlashProfile Profile {get;private set;}
        public int AcceptedCount {get;private set;}
        public int DuplicateCount {get;private set;}
        public int RecycledLiveCount {get;private set;}
        public int PreviewCount {get;private set;}
        public int Capacity=>pool.Count;
        public int ActiveCount {get {int n=0;foreach(var p in pool)if(p!=null&&p.Live)n++;return n;}}
        public ShibuyaBlackFlashVisual LastVisual {get;private set;}
        public ShibuyaCombatDamageResult LastResult {get;private set;}
        public static ShibuyaBlackFlashPresentation Attach(ShibuyaMatch match)
        {
            var existing=match.GetComponent<ShibuyaBlackFlashPresentation>();if(existing!=null)return existing;
            var profile=Resources.Load<ShibuyaBlackFlashProfile>("Shibuya Black Flash");if(profile==null)return null;
            var result=match.gameObject.AddComponent<ShibuyaBlackFlashPresentation>();result.Configure(profile);return result;
        }
        public void Configure(ShibuyaBlackFlashProfile profile)
        {
            Profile=profile;
            if(ink==null&&profile!=null&&profile.inkShader!=null&&profile.lightShader!=null){
                ink=new Material(profile.inkShader){name="BlackFlash owned ink"};
                smokeMaterial=new Material(profile.inkShader){name="BlackFlash owned smoke"};smokeMaterial.SetFloat("_ZWrite",0);
                lightMaterial=new Material(profile.lightShader){name="BlackFlash owned light"};
            }
        }
        void OnEnable(){if(subscribed)return;ShibuyaCombatDamageResults.Resolved+=PresentEvent;subscribed=true;}
        void OnDisable(){if(subscribed)ShibuyaCombatDamageResults.Resolved-=PresentEvent;subscribed=false;foreach(var p in pool)if(p!=null)p.Clear();}
        public bool Present(ShibuyaCombatDamageResult result)
        {
            if(!isActiveAndEnabled||Profile==null||!Profile.effectEnabled||ink==null||lightMaterial==null)return false;
            if(result.Resolution!=DamageResolution.Applied||result.Grade!=ShibuyaAttackGrade.BlackFlash||!float.IsFinite(result.HealthLoss)||result.HealthLoss<=0||!Finite(result.WorldPoint))return false;
            if(result.Attacker!=null&&result.Attacker.gameObject.scene!=gameObject.scene)return false;
            if(result.Victim!=null&&result.Victim.gameObject.scene!=gameObject.scene)return false;
            if(!seen.Add(result.Key)){DuplicateCount++;return false;}
            Vector3 direction=result.Attacker!=null?result.WorldPoint-(result.Attacker.transform.position+Vector3.up):Vector3.forward;
            if(direction.sqrMagnitude<.0001f)direction=result.Attacker!=null?result.Attacker.transform.forward:Vector3.forward;
            LastResult=result;AcceptedCount++;
            PlayAt(result.WorldPoint,direction,result.Key.GetHashCode());return true;
        }
        // Only presentation input: no damage, random grade, resource or Zone API is invoked here.
        public ShibuyaBlackFlashVisual PlayAt(Vector3 point,Vector3 direction,int seed)
        {
            if(Profile==null||ink==null||lightMaterial==null||!isActiveAndEnabled||!Finite(point)||!Finite(direction))return null;
            int limit=Mathf.Clamp(Profile.simultaneousLimit,1,16);
            while(pool.Count>limit){var last=pool[pool.Count-1];pool.RemoveAt(pool.Count-1);Destroy(last.gameObject);}
            ShibuyaBlackFlashVisual slot=null;foreach(var p in pool)if(!p.Live){slot=p;break;}
            if(slot==null&&pool.Count<limit){var go=new GameObject("BlackFlash pooled impact");go.transform.SetParent(transform,false);slot=go.AddComponent<ShibuyaBlackFlashVisual>();slot.Initialize(ink,lightMaterial,smokeMaterial);pool.Add(slot);}
            if(slot==null){slot=pool[0];foreach(var p in pool)if(p.Started<slot.Started)slot=p;RecycledLiveCount++;}
            slot.Play(point,direction,Profile,seed,Time.unscaledTimeAsDouble);LastVisual=slot;return slot;
        }
        // Event delegate is void; public acceptance result is useful for deterministic validation.
        void PresentEvent(ShibuyaCombatDamageResult result)=>Present(result);
        static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
        void Update(){double now=Time.unscaledTimeAsDouble;foreach(var p in pool)p.Tick(now);}
        void OnDestroy(){foreach(var p in pool)if(p!=null)Destroy(p.gameObject);pool.Clear();if(ink!=null)Destroy(ink);if(smokeMaterial!=null)Destroy(smokeMaterial);if(lightMaterial!=null)Destroy(lightMaterial);seen.Clear();}
#if UNITY_EDITOR
        public ShibuyaBlackFlashVisual Preview(Vector3 point,Vector3 direction,int seed){PreviewCount++;return PlayAt(point,direction,seed);}
#endif
    }
}
