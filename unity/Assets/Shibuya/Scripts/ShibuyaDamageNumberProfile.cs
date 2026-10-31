using UnityEngine;
namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName="JJK/시부야/전투 데미지 표시",fileName="Shibuya Damage Numbers")]
    public sealed class ShibuyaDamageNumberProfile : ScriptableObject
    {
        public bool displayEnabled=true;
        public Color normalColor=Color.white,criticalColor=new Color(1,.78f,.12f),blackFlashColor=new Color(1,.12f,.20f);
        [Min(8)] public int normalSize=32,criticalSize=40,blackFlashSize=50;
        [Min(.05f)] public float displayDuration=.85f;
        [Min(0)] public float risePixels=65;
        [Min(1)] public float normalPopScale=1.15f,criticalPopScale=1.35f,blackFlashPopScale=1.6f;
        public AnimationCurve popCurve=new AnimationCurve(new Keyframe(0,1),new Keyframe(.15f,0),new Keyframe(1,0));
        public AnimationCurve opacityCurve=new AnimationCurve(new Keyframe(0,1),new Keyframe(.6f,1),new Keyframe(1,0));
        public Vector3 worldOffset=new Vector3(0,.65f,0);
        public Vector2 screenOffset=new Vector2(0,-8);
        [Min(0)] public float overlapSpacing=112;
        [Range(1,128)] public int maximumVisible=32;
        public bool showCriticalText=true,showBlackFlashText=true,showZoneText=true;
        [Range(.2f,1)] public float zoneTextScale=.42f;
        public Color zoneTextColor=new Color(.75f,.9f,1);
        public string criticalText="CRITICAL!",blackFlashText="흑섬!";
        public Color outlineColor=new Color(.025f,.012f,.025f,1);
        [Range(0,6)] public float outlineWidth=2;
        public Font font;
        public string fallbackFontName="Malgun Gothic";
    }
    // Persistence/menu adapters can inject a shared settings instance; no combat dependency or PlayerPrefs writes.
    public sealed class ShibuyaDamageDisplayOptions
    {
        public bool Enabled {get;private set;}
        public event System.Action<bool> Changed;
        public ShibuyaDamageDisplayOptions(bool enabled=true){Enabled=enabled;}
        public void SetEnabled(bool enabled){if(Enabled==enabled)return;Enabled=enabled;Changed?.Invoke(enabled);}
    }
}
