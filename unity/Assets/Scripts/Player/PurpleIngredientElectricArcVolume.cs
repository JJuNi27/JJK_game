using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>Independent, short 3D lightning occurrences around a Purple ingredient mass.</summary>
    public sealed class PurpleIngredientElectricArcVolume : MonoBehaviour
    {
        private const int Points = 8;
        private LineRenderer[] lines;
        private Material material;
        private PurpleIngredientElectricArcProfile profile;
        private bool blue;
        private readonly Vector3[] path = new Vector3[Points];
        public int PeakVisibleCount { get; private set; }

        private static float Hash(float v) => Mathf.Repeat(Mathf.Sin(v * 127.1f + 19.3f) * 43758.5453f, 1f);

        public void Configure(bool blueIngredient, PurpleIngredientElectricArcProfile settings)
        {
            blue = blueIngredient;
            profile = settings;
            var shader = Resources.Load<Shader>("VFX/HollowPurpleFilament");
            material = new Material(shader) { name = "PurpleIngredientElectricArc_Runtime" };
            material.SetColor("_Color", blue ? new Color(.34f, 1.2f, 3.8f) : new Color(4.5f, 1.2f, 1f));
            lines = new LineRenderer[profile.arcSlots];
            for (int i = 0; i < lines.Length; i++)
            {
                var child = new GameObject("ElectricArc_" + i);
                child.transform.SetParent(transform, false);
                var line = child.AddComponent<LineRenderer>();
                line.sharedMaterial = material;
                line.useWorldSpace = false;
                line.positionCount = Points;
                line.numCapVertices = 0;
                line.numCornerVertices = 0;
                line.shadowCastingMode = ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.enabled = false;
                lines[i] = line;
            }
        }

        public void Sample(float clock, float fusion)
        {
            if (lines == null) return;
            float weight = PurpleIngredientBurstVolume.Weight(fusion);
            int visible = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                float rate = profile.eventRate * Mathf.Lerp(.65f, 1.42f, Hash(i * 13.43f + 4));
                float tick = clock * rate + Hash(i * 7.81f + 2);
                int epoch = Mathf.FloorToInt(tick);
                float phase = Mathf.Repeat(tick, 1);
                float seed = i * 43.7f + epoch * 79.31f + (blue ? 0 : 419);
                float duty = Mathf.Lerp(.27f, .56f, Hash(seed + 3));
                bool active = weight > .01f && phase < duty;
                line.enabled = active;
                if (!active) continue;
                visible++;
                float age = phase / duty;
                bool far = Hash(seed + 5) > .68f;
                float reach = far
                    ? Mathf.Lerp(profile.nearReach + .15f, profile.farReach, Hash(seed + 6))
                    : Mathf.Lerp(1.13f, profile.nearReach, Hash(seed + 6));
                Vector3 direction = Unit(seed + 11);
                Vector3 tangent = Vector3.Cross(direction, Mathf.Abs(direction.y) < .8f ? Vector3.up : Vector3.right).normalized;
                Vector3 binormal = Vector3.Cross(direction, tangent);
                float startRadius = blue ? reach : 1.01f;
                float endRadius = blue ? .98f : reach;
                float jag = far ? .38f : .24f;
                int twitch = Mathf.FloorToInt(age * 3);
                for (int p = 0; p < Points; p++)
                {
                    float u = p / (float)(Points - 1);
                    float envelope = Mathf.Sin(u * Mathf.PI);
                    float side = (Hash(seed + p * 11.2f + twitch * 59) - .5f) * jag * envelope;
                    float depth = (Hash(seed + p * 17.4f + twitch * 31) - .5f) * jag * envelope;
                    path[p] = direction * Mathf.Lerp(startRadius, endRadius, u)
                        + tangent * side + binormal * depth;
                }
                line.SetPositions(path);
                float flash = Mathf.Clamp01(age * 8) * Mathf.Clamp01((1 - age) * 5);
                flash *= (.65f + .35f * Hash(seed + twitch * 23)) * weight;
                line.startWidth = profile.lineWidth * (far ? 1.1f : .85f);
                line.endWidth = profile.lineWidth * .35f;
                Color start = blue ? new Color(.75f, 1.35f, 2.1f) : new Color(2.1f, .8f, .8f);
                Color end = blue ? new Color(.25f, .85f, 1.8f) : new Color(1.8f, .18f, .3f);
                start *= profile.brightness;
                end *= profile.brightness;
                start.a = flash;
                end.a = flash * .35f;
                line.startColor = start;
                line.endColor = end;
            }
            PeakVisibleCount = Mathf.Max(PeakVisibleCount, visible);
        }

        private static Vector3 Unit(float seed)
        {
            float z = Hash(seed) * 2 - 1;
            float angle = Hash(seed + 1) * Mathf.PI * 2;
            float horizontal = Mathf.Sqrt(1 - z * z);
            return new Vector3(Mathf.Cos(angle) * horizontal, z, Mathf.Sin(angle) * horizontal);
        }

        private void OnDestroy()
        {
            if (material == null) return;
            if (Application.isPlaying) Destroy(material); else DestroyImmediate(material);
        }
    }
}
