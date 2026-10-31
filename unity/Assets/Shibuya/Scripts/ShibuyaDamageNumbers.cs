using System;
using System.Globalization;
using JJKGame.Core;
using UnityEngine;
namespace JJKGame.Shibuya
{
    // Local-camera IMGUI overlay, matching the existing MVP UI stack. Fixed reusable slots;
    // no per-hit Canvas, GameObject, Text, Mesh or Material allocation.
    [DisallowMultipleComponent]
    public sealed class ShibuyaDamageNumbers : MonoBehaviour
    {
        struct Popup {public bool active;public ShibuyaCombatDamageResult result;public Vector3 localAnchor;public double started;public int lane,layer;public string text,zoneText;}
        public readonly struct Layout
        {
            public readonly Rect Rect;public readonly Color Color;public readonly int FontSize;public readonly float Alpha;
            public Layout(Rect rect,Color color,int size,float alpha){Rect=rect;Color=color;FontSize=size;Alpha=alpha;}
        }
        Popup[] pool=Array.Empty<Popup>();ShibuyaDamageResultKey[] recent=Array.Empty<ShibuyaDamageResultKey>();int recentUsed,recentIndex;
        readonly int[] laneUsage=new int[5];
        ShibuyaFighter localOwner;Camera view;ShibuyaDamageNumberProfile profile;
        Font ownedFont;GUIStyle style;bool subscribed;
        public ShibuyaDamageDisplayOptions Options {get;private set;}
        public ShibuyaDamageNumberProfile Profile=>profile;
        public Camera View=>view;
        public bool DisplayEnabled=>profile!=null&&profile.displayEnabled&&Options!=null&&Options.Enabled;
        public int ActiveCount {get;private set;}
        public int PresentedCount {get;private set;}
        public int Capacity=>pool.Length;
        public float LastDisplayedDamage {get;private set;}
        public ShibuyaAttackGrade LastDisplayedGrade {get;private set;}
        public string LastReason {get;private set;}="아직 피해 결과 없음";
        public string LastText {get;private set;}="";
        public ShibuyaCombatDamageResult LastResult {get;private set;}
        public bool OwnsFont=>ownedFont!=null;
        public Font OwnedFont=>ownedFont;
        public event Action<ShibuyaCombatDamageResult> Presented;
        public void Configure(ShibuyaFighter owner,Camera camera,ShibuyaDamageNumberProfile settings,ShibuyaDamageDisplayOptions options=null)
        {
            if(Options!=null)Options.Changed-=OnOption;
            if(ownedFont!=null)Destroy(ownedFont);ownedFont=null;style=null;
            Clear();localOwner=owner;view=camera;profile=settings;Options=options??new ShibuyaDamageDisplayOptions();
            Options.Changed+=OnOption;Resize();Subscribe();
        }
        void OnEnable()=>Subscribe();
        void Subscribe(){if(subscribed||!isActiveAndEnabled)return;ShibuyaCombatDamageResults.Resolved+=Present;subscribed=true;}
        void OnOption(bool enabled){if(!enabled){Clear();LastReason="표시 옵션 OFF";}}
        public void SetDisplayEnabled(bool enabled)=>Options?.SetEnabled(enabled);
        void Resize(){int capacity=profile!=null?Mathf.Clamp(profile.maximumVisible,1,128):0;if(pool.Length==capacity)return;
            Clear();pool=new Popup[capacity];recent=new ShibuyaDamageResultKey[Mathf.Max(128,capacity*8)];recentUsed=recentIndex=0;}
        public void Clear(){for(int i=0;i<pool.Length;i++)pool[i]=default;ActiveCount=0;}
        public void Present(ShibuyaCombatDamageResult result)
        {
            if(!isActiveAndEnabled||localOwner==null||view==null||profile==null)return;
            if(result.Attacker!=null&&result.Attacker.gameObject.scene!=localOwner.gameObject.scene)return;
            if(result.Victim!=null&&result.Victim.gameObject.scene!=localOwner.gameObject.scene)return;
            LastResult=result;
            if(result.Resolution!=DamageResolution.Applied){LastReason="피해 차단: "+result.Resolution;return;}
            if(!float.IsFinite(result.HealthLoss)||result.HealthLoss<=0){LastReason="유효 HP 감소 없음";return;}
            if(!Finite(result.WorldPoint)){LastReason="표시 위치 무효";return;}
            if(!DisplayEnabled){LastReason="표시 옵션 OFF";return;}
            Resize();for(int i=0;i<recentUsed;i++)if(recent[i].Equals(result.Key)){LastReason="중복 타격 결과";return;}
            recent[recentIndex]=result.Key;recentIndex=(recentIndex+1)%recent.Length;recentUsed=Mathf.Min(recent.Length,recentUsed+1);
            int slot=-1;double oldest=double.MaxValue;Array.Clear(laneUsage,0,laneUsage.Length);
            for(int i=0;i<pool.Length;i++){if(!pool[i].active){slot=i;break;}}
            for(int i=0;i<pool.Length;i++)if(pool[i].active&&pool[i].result.Key.Target==result.Key.Target)laneUsage[pool[i].lane]++;
            int lane=0;for(int i=1;i<laneUsage.Length;i++)if(laneUsage[i]<laneUsage[lane])lane=i;
            if(slot<0)for(int i=0;i<pool.Length;i++)if(pool[i].started<oldest){oldest=pool[i].started;slot=i;}
            if(slot<0)return;
            if(!pool[slot].active)ActiveCount++;
            string text=FormatDamage(result.HealthLoss);
            if(result.Grade==ShibuyaAttackGrade.Critical&&profile.showCriticalText)text=profile.criticalText+"\n"+text;
            if(result.Grade==ShibuyaAttackGrade.BlackFlash&&profile.showBlackFlashText)text=profile.blackFlashText+"\n"+text;
            string zoneText="";
            if(profile.showZoneText&&result.Zone!=ShibuyaZoneFeedback.None){
                zoneText=result.Zone==ShibuyaZoneFeedback.Entered?"ZONE 진입":"ZONE 연장";
                if(result.EnergyRestored>0)zoneText+=" · 주력 +"+FormatDamage(result.EnergyRestored);
            }
            pool[slot]=new Popup{active=true,result=result,started=Time.unscaledTimeAsDouble,
                localAnchor=result.Victim!=null?result.Victim.transform.InverseTransformPoint(result.WorldPoint):Vector3.zero,
                lane=lane,layer=laneUsage[lane],text=text,zoneText=zoneText};
            PresentedCount++;LastDisplayedDamage=result.HealthLoss;LastDisplayedGrade=result.Grade;LastText=text;LastReason="표시 결과 수락";Presented?.Invoke(result);
        }
        void Update(){if(profile==null)return;Resize();if(!DisplayEnabled){Clear();return;}
            for(int i=0;i<pool.Length;i++)if(pool[i].active&&Time.unscaledTimeAsDouble-pool[i].started>=Mathf.Max(.05f,profile.displayDuration)){pool[i]=default;ActiveCount--;}}
        static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
        public static string FormatDamage(float amount){if(!float.IsFinite(amount)||amount<=0)return "";
            double integer=Math.Round(amount);return integer>=1&&Math.Abs(amount-integer)<=.00001?integer.ToString("0",CultureInfo.InvariantCulture):amount.ToString("G9",CultureInfo.InvariantCulture);}
        public bool TryGetLayout(int slot,out Layout layout,out string reason)
        {
            layout=default;reason="표시 슬롯 없음";
            if(slot<0||slot>=pool.Length||!pool[slot].active||!DisplayEnabled||view==null||!view.isActiveAndEnabled)return false;
            var p=pool[slot];Vector3 anchor=p.result.WorldPoint;
            if(p.result.Victim!=null&&p.result.Victim.gameObject.activeInHierarchy)anchor=p.result.Victim.transform.TransformPoint(p.localAnchor);
            anchor+=profile.worldOffset;Vector3 projected=view.WorldToViewportPoint(anchor);
            if(!Finite(projected)||projected.z<=view.nearClipPlane){reason="카메라 뒤/근접면";return false;}
            if(projected.x<0||projected.x>1||projected.y<0||projected.y>1){reason="화면 밖";return false;}
            float age=(float)(Time.unscaledTimeAsDouble-p.started),t=Mathf.Clamp01(age/Mathf.Max(.05f,profile.displayDuration));
            float scale=Mathf.Clamp(view.pixelHeight/900f,.75f,1.5f);
            int baseSize=p.result.Grade==ShibuyaAttackGrade.BlackFlash?profile.blackFlashSize:p.result.Grade==ShibuyaAttackGrade.Critical?profile.criticalSize:profile.normalSize;
            float peak=p.result.Grade==ShibuyaAttackGrade.BlackFlash?profile.blackFlashPopScale:p.result.Grade==ShibuyaAttackGrade.Critical?profile.criticalPopScale:profile.normalPopScale;
            float pop=1+(Mathf.Max(1,peak)-1)*Mathf.Clamp01(profile.popCurve!=null?profile.popCurve.Evaluate(t):1-t);
            int size=Mathf.Max(8,Mathf.RoundToInt(baseSize*scale*pop));int lines=1;foreach(char c in p.text)if(c=='\n')lines++;
            float width=Mathf.Max(180*scale,size*10),height=size*1.25f*lines;
            var pixels=view.pixelRect;float x=pixels.x+projected.x*pixels.width+profile.screenOffset.x*scale;
            // First number stays central; subsequent live hits alternate left/right before a new row.
            int lane=p.lane==0?0:p.lane%2==1?(p.lane+1)/2:-(p.lane/2);
            x+=lane*profile.overlapSpacing*scale;
            float y=Screen.height-(pixels.y+projected.y*pixels.height)+profile.screenOffset.y*scale-profile.risePixels*scale*t-p.layer*size*.55f;
            Color color=p.result.Grade==ShibuyaAttackGrade.BlackFlash?profile.blackFlashColor:p.result.Grade==ShibuyaAttackGrade.Critical?profile.criticalColor:profile.normalColor;
            float alpha=Mathf.Clamp01(profile.opacityCurve!=null?profile.opacityCurve.Evaluate(t):1-t)*color.a;
            layout=new Layout(new Rect(x-width*.5f,y-height,width,height),color,size,alpha);reason="화면에 투영됨";return true;
        }
        public string GetPopupText(int slot)=>slot>=0&&slot<pool.Length&&pool[slot].active?pool[slot].text:"";
        void EnsureStyle(){if(style!=null)return;Font font=profile.font;
            // The OS family name is functional FontEngine data; renaming it breaks glyph loading.
            if(font==null){ownedFont=Font.CreateDynamicFontFromOSFont(profile.fallbackFontName,profile.blackFlashSize);font=ownedFont;}
            style=new GUIStyle(GUI.skin.label){font=font,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,richText=false,wordWrap=false};}
        void OnGUI(){if(!DisplayEnabled||ActiveCount==0||Event.current.type!=EventType.Repaint)return;EnsureStyle();Color before=GUI.color;
            try{for(int i=0;i<pool.Length;i++)if(TryGetLayout(i,out var layout,out _)){
                style.fontSize=layout.FontSize;Color ink=profile.outlineColor;ink.a*=layout.Alpha;
                GUI.color=Color.white;style.normal.textColor=ink;float width=profile.outlineWidth;
                foreach(var offset in outlineOffsets){var r=layout.Rect;r.position+=offset*width;GUI.Label(r,pool[i].text,style);}
                Color color=layout.Color;color.a=layout.Alpha;style.normal.textColor=color;GUI.Label(layout.Rect,pool[i].text,style);
                if(!string.IsNullOrEmpty(pool[i].zoneText)){
                    style.fontSize=Mathf.Max(8,Mathf.RoundToInt(layout.FontSize*profile.zoneTextScale));
                    Rect small=new Rect(layout.Rect.x,layout.Rect.yMax,layout.Rect.width,style.fontSize*1.5f);
                    style.normal.textColor=new Color(0,0,0,layout.Alpha);var shadow=small;shadow.position+=Vector2.one;GUI.Label(shadow,pool[i].zoneText,style);
                    Color auxiliary=profile.zoneTextColor;auxiliary.a*=layout.Alpha;style.normal.textColor=auxiliary;GUI.Label(small,pool[i].zoneText,style);
                }
            }}finally{GUI.color=before;}}
        static readonly Vector2[] outlineOffsets={Vector2.left,Vector2.right,Vector2.up,Vector2.down};
        void OnDisable(){if(subscribed){ShibuyaCombatDamageResults.Resolved-=Present;subscribed=false;}Clear();}
        void OnDestroy(){if(Options!=null)Options.Changed-=OnOption;if(ownedFont!=null)Destroy(ownedFont);ownedFont=null;style=null;}
    }
}
