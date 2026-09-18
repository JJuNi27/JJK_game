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
        [Header("Blue · 네 번의 수렴과 잔류")]
        public Vector4 blueHitAccents = new Vector4(.85f, 1f, 1.18f, 1.65f);
        [Range(.04f, .12f)] public float blueDustLeadTime = .10f;
        [Range(.25f, .45f)] public float blueAftermathDuration = .35f;
        [Range(.4f, .8f)] public float blueFinalCoreScale = .58f;
        [Range(0f, .3f)] public float blueFinalShake = .14f;
        [Header("Red · repulsive pressure, never flame")]
        [Range(0f, .5f)] public float redFlashIntensity = .38f;
        [Range(.5f, 2f)] public float redShockRingScale = 1.65f;
        [Range(.2f, 2f)] public float redAftermathDuration = 1.25f;
        [Range(8, 64)] public int redAftermathDensity = 48;
        [Header("Red · 표면에서 밀려나는 압력")]
        [Range(0f, .3f)] public float redFrontIrregularity = .16f;
        [Range(0, 24)] public int redSurfaceDebrisCount = 14;
        [Range(.5f, 2f)] public float redSurfaceReactionScale = 1f;
        [Header("Purple · completed fusion / corridor")]
        [Range(.15f, 1f)] public float purpleBlueLeadDuration = .45f;
        [Range(.15f, 1.5f)] public float purpleRedOppositionDuration = .65f;
        [Range(.2f, 2f)] public float purpleFusionDuration = .90f;
        [Range(8, 16)] public int purpleImpactFrameCount = 8;
        public float PurpleFusionStart => purpleBlueLeadDuration + purpleRedOppositionDuration;
        public float PurpleHoldStart => PurpleFusionStart + purpleFusionDuration;
        public float PurpleImpactDuration => purpleImpactFrameCount / 60f;
        public float PurpleImpactStart => PurpleHoldStart + purpleFusionHoldDuration;
        public float PurpleReleaseTime => PurpleImpactStart + PurpleImpactDuration;
        [Range(1f, 5f)] public float purpleFusionHoldDuration = 3f;
        [Range(.8f, 2f)] public float purpleFormationScale = 1.35f;
        [Range(1.5f, 3.5f)] public float purpleFormationSeparation = 2.25f;
        [Range(.5f, 1.5f)] public float purpleTerminalDuration = .95f;
        [Range(.5f, 2f)] public float purpleTerminalScale = 1f;
        [Range(1f, 3f)] public float purpleVisualScale = 1.65f;
        [Tooltip("완성 위치의 높이(Y)와 전방(Z) 거리입니다. 중앙 정렬을 위해 X는 사용하지 않습니다.")]
        public Vector3 purpleHoldOffset = new Vector3(0f, 1.3f, 2.2f);
        [Header("퍼플 공간 상흔")]
        [Tooltip("실제 퍼플 구체 지름 대비 상흔 폭 배율입니다.")]
        [Range(.5f, 2f)] public float purpleScarWidthMultiplier = 1.15f;
        [Range(.2f, 4f)] public float purpleScarDuration = 1.8f;
        [Header("퍼플 · 입체 에너지")]
        [Range(.5f, 3f)] public float purpleVolumeDensity = 1.8f;
        [Range(.3f, 2f)] public float purpleEmission = .65f;
        [Range(.5f, 2f)] public float purpleTurbulence = 1.3f;
        [Range(.3f, 2f)] public float purpleFlowSpeed = 1.1f;
        [Range(0f, 1f)] public float purpleHazeOpacity = .06f;
        [Range(.5f, 2f)] public float purpleRibbonWidth = 1.2f;
        [Range(.3f, 2f)] public float purpleLightningWidth = 1.0f;
        [Range(0f, 2f)] public float purpleLightningIntensity = 1.0f;
        [Range(1f, 4f)] public float purpleReleaseEmission = 2.5f;
        [Header("퍼플 · 짧은 방출 카메라")]
        public bool purpleCameraChoreography = true;
        public bool purpleImpactFrames = true;
        [Range(2f, 8f)] public float purpleCameraSideOffset = 5.5f;
        [Range(0f, 8f)] public float purpleCameraTensionFov = 5f;
        [Range(0f, 12f)] public float purpleReleaseFov = 9f;
        [Range(0f, .8f)] public float purpleCameraImpulse = .42f;
        [Range(.35f, 1f)] public float purpleCameraFollowThrough = .65f;
    }
}
