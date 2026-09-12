using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>Shared combat/lab art tuning. Gameplay radius remains on the chain controller.</summary>
    [CreateAssetMenu(menuName = "JJK/Gojo Polish Settings")]
    public sealed class GojoPolishSettings : ScriptableObject
    {
        public const float PurpleShellDiameter = 3.72f;
        private static GojoPolishSettings cached;
        public static GojoPolishSettings Current
        {
            get
            {
                if (cached == null) cached = Resources.Load<GojoPolishSettings>("VFX/GojoPolishSettings");
                if (cached == null)
                {
                    cached = CreateInstance<GojoPolishSettings>();
                    cached.hideFlags = HideFlags.HideAndDontSave;
                }
                return cached;
            }
        }

        [Header("Blue · inward compression / active rubble")]
        public bool blueCompressionEnabled = false;
        [Range(0f, 2f)] public float blueCompressionStrength = 0.7f;
        [Range(.5f, 2.5f)] public float blueDebrisScale = 1.2f;
        [Header("Red · repulsive pressure, never flame")]
        [Range(0f, .5f)] public float redFlashIntensity = .38f;
        [Range(.5f, 2f)] public float redShockRingScale = 1.65f;
        [Range(.2f, 2f)] public float redAftermathDuration = 1.25f;
        [Range(8, 64)] public int redAftermathDensity = 48;
        [Header("Purple · completed fusion / corridor")]
        [Range(0f, 1f)] public float purpleFusionHoldDuration = .32f;
        [Range(1f, 3f)] public float purpleVisualScale = 1.65f;
        [Tooltip("완성 위치의 높이(Y)와 전방(Z) 거리입니다. 중앙 정렬을 위해 X는 사용하지 않습니다.")]
        public Vector3 purpleHoldOffset = new Vector3(0f, 1.3f, 2.2f);
        [Header("퍼플 공간 상흔")]
        [Tooltip("실제 퍼플 구체 지름 대비 상흔 폭 배율입니다.")]
        [Range(.5f, 2f)] public float purpleScarWidthMultiplier = 1.15f;
        [Range(.2f, 4f)] public float purpleScarDuration = 1.8f;
    }
}
