using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    public sealed class ProductionParticleVfxInstance : MonoBehaviour, IPresentationVfxInstance
    {
        private readonly List<Material> runtimeMaterials = new List<Material>(4);
        private readonly List<Color> materialColors = new List<Color>(4);
        private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>(4);

        private Transform followTarget;
        private Vector3 followOffset;
        private float duration;
        private float startedAt;
        private bool useUnscaledTime;
        private bool stopping;
        private float stopStartedAt;
        private bool destroying;

        public bool IsAlive => !destroying && this != null && gameObject != null;
        private float CurrentTime => useUnscaledTime ? Time.unscaledTime : Time.time;

        public static ProductionParticleVfxInstance Spawn(
            PresentationVfxSpawnRequest request,
            Transform runtimeRoot
        )
        {
            if (request.Duration <= 0f)
            {
                return null;
            }

            GameObject host = new GameObject($"ParticleVfx_{request.StyleId}");
            host.transform.SetParent(runtimeRoot, true);
            host.transform.position = ResolvePosition(request);

            Vector3 direction = request.HasDirection ? request.Direction : Vector3.forward;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            host.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            ProductionParticleVfxInstance instance =
                host.AddComponent<ProductionParticleVfxInstance>();
            instance.followTarget = request.FollowsTarget ? request.FollowTarget : null;
            instance.followOffset = request.FollowLocalOffset;
            instance.duration = Mathf.Max(0.05f, request.Duration);
            instance.useUnscaledTime = request.TimePolicy == PresentationVfxTimePolicy.Unscaled;
            instance.startedAt = instance.CurrentTime;
            instance.Build(request);
            return instance;
        }

        public void Stop(PresentationVfxStopMode mode = PresentationVfxStopMode.FadeOut)
        {
            if (destroying)
            {
                return;
            }

            if (mode == PresentationVfxStopMode.Immediate)
            {
                destroying = true;
                Destroy(gameObject);
                return;
            }

            if (stopping)
            {
                return;
            }

            stopping = true;
            stopStartedAt = CurrentTime;
            foreach (ParticleSystem system in particleSystems)
            {
                if (system != null)
                {
                    system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }

        private void Update()
        {
            if (followTarget != null)
            {
                transform.position =
                    followTarget.position + followTarget.TransformDirection(followOffset);
                transform.rotation = followTarget.rotation;
            }

            float elapsed = CurrentTime - startedAt;
            float fade = 1f;
            if (stopping)
            {
                fade = 1f - Mathf.Clamp01((CurrentTime - stopStartedAt) / 0.14f);
                ApplyMaterialFade(fade);
            }

            if (elapsed >= duration || (stopping && fade <= 0f))
            {
                destroying = true;
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            destroying = true;
            foreach (Material material in runtimeMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            runtimeMaterials.Clear();
            materialColors.Clear();
            particleSystems.Clear();
        }

        private void Build(PresentationVfxSpawnRequest request)
        {
            float outer = Mathf.Max(0.25f, request.EndRadius);
            float inner = Mathf.Max(0.05f, request.StartRadius);
            switch (request.StyleId)
            {
                case PresentationVfxStyleId.GojoBlue:
                    BuildGojoBlue(inner, outer);
                    break;
                case PresentationVfxStyleId.GojoRed:
                    BuildGojoRed(inner, outer);
                    break;
                case PresentationVfxStyleId.HollowPurpleRelease:
                    BuildPurpleRelease(inner, outer);
                    break;
                case PresentationVfxStyleId.HollowPurpleFormation:
                    BuildPurpleFormation(outer);
                    break;
                case PresentationVfxStyleId.FugaCharge:
                    BuildFugaCharge(outer, request.Amplified);
                    break;
                case PresentationVfxStyleId.FugaRelease:
                    BuildFugaRelease(inner, request.Amplified);
                    break;
                case PresentationVfxStyleId.FugaImpact:
                    BuildFugaImpact(outer, request.Amplified);
                    break;
                case PresentationVfxStyleId.SukunaDismantle:
                    BuildSlash(false, outer);
                    break;
                case PresentationVfxStyleId.SukunaCleave:
                    BuildSlash(true, outer);
                    break;
                case PresentationVfxStyleId.UnlimitedVoidAnticipation:
                    BuildUnlimitedVoid(false, outer);
                    break;
                case PresentationVfxStyleId.UnlimitedVoidActive:
                    BuildUnlimitedVoid(true, outer);
                    break;
                case PresentationVfxStyleId.MalevolentShrineAnticipation:
                    BuildMalevolentShrine(false, outer);
                    break;
                case PresentationVfxStyleId.MalevolentShrineActive:
                    BuildMalevolentShrine(true, outer);
                    break;
                case PresentationVfxStyleId.DivineDogRelease:
                    BuildDivineDog(false, outer);
                    break;
                case PresentationVfxStyleId.DivineDogImpact:
                    BuildDivineDog(true, outer);
                    break;
                case PresentationVfxStyleId.NueRelease:
                case PresentationVfxStyleId.NueImpact:
                    BuildNue(request.StyleId == PresentationVfxStyleId.NueImpact, outer);
                    break;
                case PresentationVfxStyleId.BasicHit1:
                case PresentationVfxStyleId.BasicHit2:
                case PresentationVfxStyleId.BasicHitFinisher:
                    BuildBasicHit(request.StyleId);
                    break;
                default:
                    CreateBurst("GenericBurst", request.PrimaryColor, 12, 0.18f, 1.2f, 3f, 0.08f, outer, false);
                    CreateMotes("GenericMotes", request.SecondaryColor, 10, 0.28f, 0.2f, 1.5f, 0.06f, outer * 0.5f, false, false);
                    break;
            }

            foreach (ParticleSystem system in particleSystems)
            {
                system.Play(true);
            }
        }

        private void BuildGojoBlue(float inner, float outer)
        {
            CreateCore("BlueDenseCore", new Color(0.02f, 0.12f, 0.82f, 0.92f), 18, inner, 0.12f);
            CreateMotes("BlueInwardStreaks", new Color(0.05f, 0.55f, 1f, 0.88f), 34, 0.34f, -5.5f, -2.8f, 0.075f, outer, true, true);
            CreateMotes("BlueCompressionMotes", new Color(0.16f, 0.88f, 1f, 0.72f), 22, 0.42f, -2.2f, -0.7f, 0.045f, outer * 0.72f, true, false);
        }

        private void BuildGojoRed(float inner, float outer)
        {
            CreateCore("RedCompactCore", new Color(0.72f, 0.015f, 0.025f, 0.94f), 14, inner, 0.14f);
            CreateBurst("RedRepulsionFront", new Color(1f, 0.06f, 0.035f, 0.88f), 30, 0.18f, 7f, 12f, 0.08f, outer * 0.35f, true);
            CreateBurst("RedWarmEdge", new Color(1f, 0.28f, 0.04f, 0.62f), 18, 0.25f, 3.5f, 7f, 0.055f, outer * 0.55f, false);
        }

        private void BuildPurpleRelease(float inner, float outer)
        {
            CreateMotesAtOffset("PurpleBlueMerge", new Color(0.05f, 0.36f, 1f, 0.82f), -Vector3.right * 1.2f, 18, 3.8f, inner);
            CreateMotesAtOffset("PurpleRedMerge", new Color(1f, 0.04f, 0.08f, 0.82f), Vector3.right * 1.2f, 18, 3.8f, inner);
            CreateMotes("PurpleTravelFragments", new Color(0.72f, 0.08f, 1f, 0.78f), 32, 0.40f, 8f, 15f, 0.085f, outer * 0.25f, false, true);
        }

        private void BuildPurpleFormation(float outer)
        {
            CreateCore("PurpleIgnitionCore", new Color(0.58f, 0.025f, 1f, 0.96f), 24, outer * 0.18f, 0.13f);
            CreateBurst("PurpleIgnitionFragments", new Color(0.95f, 0.32f, 1f, 0.82f), 28, 0.24f, 2.5f, 6.5f, 0.07f, outer * 0.25f, true);
            CreateMotes("PurpleUnstableMotes", new Color(0.36f, 0.02f, 0.70f, 0.68f), 22, 0.34f, 0.4f, 2f, 0.06f, outer * 0.7f, false, false);
        }

        private void BuildFugaCharge(float outer, bool amplified)
        {
            int density = amplified ? 40 : 26;
            CreateCore("FugaDarkCore", new Color(0.48f, 0.01f, 0.005f, 0.88f), 14, outer * 0.12f, 0.13f);
            CreateMotes("FugaInwardEmbers", new Color(1f, 0.16f, 0.015f, 0.82f), density, 0.40f, -4f, -1.2f, 0.06f, outer, true, true);
            CreateMotes("FugaHeatMotes", new Color(0.30f, 0.008f, 0.004f, 0.72f), density / 2, 0.48f, -1.4f, -0.3f, 0.10f, outer * 0.75f, true, false);
        }

        private void BuildFugaRelease(float inner, bool amplified)
        {
            CreateCore("FugaProjectileCore", new Color(1f, 0.12f, 0.01f, 0.94f), amplified ? 24 : 18, inner, 0.13f);
            CreateForwardStreaks("FugaForwardStreak", new Color(1f, 0.34f, 0.025f, 0.86f), amplified ? 32 : 22, 0.28f, 10f, 18f, 0.085f);
            CreateMotes("FugaResidualEmber", new Color(0.50f, 0.015f, 0.005f, 0.68f), amplified ? 28 : 18, 0.42f, 0.5f, 2.8f, 0.055f, inner * 1.8f, false, false);
        }

        private void BuildFugaImpact(float outer, bool amplified)
        {
            int density = amplified ? 56 : 38;
            CreateBurst("FugaRadialFlame", new Color(1f, 0.10f, 0.01f, 0.88f), density, 0.34f, 6f, 13f, 0.13f, outer * 0.25f, true);
            CreateBurst("FugaOuterFire", new Color(1f, 0.36f, 0.025f, 0.74f), density, 0.50f, 2.8f, 7.5f, 0.18f, outer * 0.45f, false);
            CreateMotes("FugaResidualEmber", new Color(0.38f, 0.008f, 0.004f, 0.70f), density, 0.62f, 1f, 4f, 0.065f, outer * 0.75f, false, true);
        }

        private void BuildSlash(bool cleave, float length)
        {
            Color edge = cleave ? new Color(1f, 0.34f, 0.08f, 0.88f) : new Color(0.92f, 0.08f, 0.07f, 0.82f);
            CreateForwardStreaks(cleave ? "CleaveCut" : "DismantleCut", edge, cleave ? 10 : 7, 0.12f, length * 5f, length * 8f, cleave ? 0.065f : 0.045f);
            CreateForwardStreaks("CutHighlight", new Color(1f, 0.82f, 0.72f, 0.62f), cleave ? 5 : 3, 0.08f, length * 6f, length * 9f, 0.025f);
        }

        private void BuildUnlimitedVoid(bool active, float outer)
        {
            if (!active)
            {
                CreateMotes("VoidOrderedMotes", new Color(0.18f, 0.64f, 1f, 0.70f), 26, 0.55f, -0.6f, 0.5f, 0.045f, outer, true, false);
                CreateMotes("VoidSlowOrbit", new Color(0.58f, 0.42f, 1f, 0.55f), 16, 0.62f, 0.1f, 0.7f, 0.035f, outer * 0.75f, false, false);
                return;
            }

            CreateMotes("VoidNearStars", new Color(0.30f, 0.72f, 1f, 0.74f), 48, 0.75f, 0.2f, 2.2f, 0.055f, outer * 0.75f, false, true);
            CreateMotes("VoidFarDepth", new Color(0.64f, 0.54f, 1f, 0.48f), 54, 0.92f, 0.05f, 0.8f, 0.035f, outer, false, false);
            CreateMotes("VoidPalePoints", new Color(0.82f, 0.94f, 1f, 0.44f), 34, 0.66f, 0.4f, 1.4f, 0.025f, outer * 0.9f, false, false);
        }

        private void BuildMalevolentShrine(bool active, float outer)
        {
            if (!active)
            {
                CreateMotes("ShrineCurseMotes", new Color(0.44f, 0.005f, 0.008f, 0.76f), 32, 0.58f, -1.2f, 0.5f, 0.065f, outer, true, true);
                CreateVerticalStreaks("ShrineRisingCuts", new Color(0.86f, 0.025f, 0.018f, 0.62f), 14, 0.34f, outer * 0.6f);
                return;
            }

            CreateFieldSlashes("ShrineCutA", new Color(0.78f, 0.018f, 0.015f, 0.80f), outer, Quaternion.Euler(0f, 24f, 18f));
            CreateFieldSlashes("ShrineCutB", new Color(1f, 0.34f, 0.22f, 0.56f), outer, Quaternion.Euler(14f, -38f, -22f));
            CreateFieldSlashes("ShrineDarkCuts", new Color(0.24f, 0.002f, 0.004f, 0.66f), outer, Quaternion.Euler(-12f, 67f, 32f));
        }

        private void BuildDivineDog(bool impact, float outer)
        {
            if (!impact)
            {
                CreateVerticalStreaks("DogShadowRise", new Color(0.015f, 0.025f, 0.070f, 0.86f), 28, 0.52f, outer);
                CreateMotes("DogTealEyes", new Color(0.12f, 0.66f, 0.62f, 0.66f), 12, 0.34f, 0.4f, 1.8f, 0.045f, outer * 0.45f, false, false);
                return;
            }

            CreateClawStreaks("DogClaw", new Color(0.40f, 0.94f, 0.86f, 0.86f), outer);
            CreateBurst("DogDarkFragments", new Color(0.02f, 0.08f, 0.10f, 0.72f), 10, 0.20f, 1.5f, 4f, 0.06f, outer * 0.25f, true);
        }

        private void BuildNue(bool impact, float outer)
        {
            CreateForwardStreaks("NueElectricStreak", new Color(0.42f, 0.78f, 1f, 0.88f), impact ? 18 : 12, impact ? 0.16f : 0.22f, 10f, 20f, 0.035f);
            CreateBurst("NueFlicker", new Color(0.88f, 0.96f, 1f, 0.74f), impact ? 14 : 8, 0.10f, 3f, 8f, 0.025f, outer * 0.2f, true);
        }

        private void BuildBasicHit(PresentationVfxStyleId style)
        {
            bool finisher = style == PresentationVfxStyleId.BasicHitFinisher;
            bool second = style == PresentationVfxStyleId.BasicHit2;
            Color color = finisher
                ? new Color(1f, 0.70f, 0.22f, 0.88f)
                : second
                    ? new Color(0.58f, 0.84f, 1f, 0.78f)
                    : new Color(0.82f, 0.94f, 1f, 0.72f);
            CreateBurst("ContactSparks", color, finisher ? 14 : second ? 9 : 6, finisher ? 0.18f : 0.11f, 3f, finisher ? 8f : 5f, finisher ? 0.065f : 0.040f, 0.12f, true);
            if (finisher)
            {
                CreateBurst("FinisherSecondary", new Color(1f, 0.90f, 0.58f, 0.58f), 8, 0.13f, 1.5f, 4f, 0.09f, 0.18f, false);
            }
        }

        private ParticleSystem CreateCore(string name, Color color, int count, float radius, float size)
        {
            ParticleSystem system = CreateSystem(name, color, count, duration, 0f, 0.08f, size, false, ParticleSystemShapeType.Sphere, radius);
            ParticleSystem.MainModule main = system.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            return system;
        }

        private ParticleSystem CreateBurst(string name, Color color, int count, float lifetime, float minSpeed, float maxSpeed, float size, float radius, bool stretched)
        {
            return CreateSystem(name, color, count, lifetime, minSpeed, maxSpeed, size, stretched, ParticleSystemShapeType.Sphere, radius);
        }

        private ParticleSystem CreateMotes(string name, Color color, int count, float lifetime, float minSpeed, float maxSpeed, float size, float radius, bool inward, bool stretched)
        {
            ParticleSystem system = CreateSystem(name, color, count, lifetime, minSpeed, maxSpeed, size, stretched, ParticleSystemShapeType.Sphere, radius);
            ParticleSystem.ShapeModule shape = system.shape;
            shape.radiusThickness = 1f;
            ParticleSystem.MainModule main = system.main;
            if (inward)
            {
                main.startSpeed = new ParticleSystem.MinMaxCurve(-Mathf.Abs(maxSpeed), -Mathf.Abs(minSpeed));
            }
            ParticleSystem.NoiseModule noise = system.noise;
            noise.enabled = true;
            noise.strength = size * 4f;
            noise.frequency = 0.45f;
            return system;
        }

        private void CreateMotesAtOffset(string name, Color color, Vector3 localOffset, int count, float speed, float radius)
        {
            ParticleSystem system = CreateMotes(name, color, count, 0.26f, speed * 0.55f, speed, 0.055f, radius, true, true);
            system.transform.localPosition = localOffset;
        }

        private void CreateForwardStreaks(string name, Color color, int count, float lifetime, float minSpeed, float maxSpeed, float size)
        {
            ParticleSystem system = CreateSystem(name, color, count, lifetime, minSpeed, maxSpeed, size, true, ParticleSystemShapeType.Cone, 0.16f);
            ParticleSystem.ShapeModule shape = system.shape;
            shape.angle = 6f;
            shape.length = 0.15f;
        }

        private void CreateVerticalStreaks(string name, Color color, int count, float lifetime, float radius)
        {
            ParticleSystem system = CreateSystem(name, color, count, lifetime, 1.5f, 4.5f, 0.055f, true, ParticleSystemShapeType.Circle, radius);
            system.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        }

        private void CreateFieldSlashes(string name, Color color, float radius, Quaternion rotation)
        {
            ParticleSystem system = CreateSystem(name, color, 24, 0.16f, 8f, 18f, 0.045f, true, ParticleSystemShapeType.Box, radius);
            system.transform.localRotation = rotation;
            ParticleSystem.ShapeModule shape = system.shape;
            shape.scale = new Vector3(radius * 1.7f, 1.6f, radius * 1.7f);
        }

        private void CreateClawStreaks(string name, Color color, float radius)
        {
            for (int index = -1; index <= 1; index++)
            {
                ParticleSystem system = CreateSystem($"{name}_{index + 2}", color, 3, 0.14f, 7f, 11f, 0.045f, true, ParticleSystemShapeType.Cone, 0.02f);
                system.transform.localPosition = Vector3.right * index * radius * 0.22f;
                system.transform.localRotation = Quaternion.Euler(0f, index * 6f, index * 12f);
                ParticleSystem.ShapeModule shape = system.shape;
                shape.angle = 1.5f;
            }
        }

        private ParticleSystem CreateSystem(
            string name,
            Color color,
            int count,
            float particleLifetime,
            float minSpeed,
            float maxSpeed,
            float size,
            bool stretched,
            ParticleSystemShapeType shapeType,
            float shapeRadius
        )
        {
            GameObject child = new GameObject(name, typeof(ParticleSystem));
            child.transform.SetParent(transform, false);
            ParticleSystem system = child.GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = system.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = Mathf.Max(0.05f, duration);
            main.startLifetime = new ParticleSystem.MinMaxCurve(particleLifetime * 0.68f, particleLifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.72f, size);
            main.startColor = color;
            main.maxParticles = Mathf.Clamp(count * 2, 8, 160);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = shapeType;
            shape.radius = Mathf.Max(0.01f, shapeRadius);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f),
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(color.a, 0.10f),
                    new GradientAlphaKey(color.a * 0.75f, 0.62f),
                    new GradientAlphaKey(0f, 1f),
                }
            );
            colorOverLifetime.color = gradient;

            ParticleSystemRenderer renderer = child.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = stretched
                ? ParticleSystemRenderMode.Stretch
                : ParticleSystemRenderMode.Billboard;
            renderer.velocityScale = stretched ? 0.13f : 0f;
            renderer.lengthScale = stretched ? 2.2f : 1f;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 35;
            renderer.material = CreateMaterial(color);

            particleSystems.Add(system);
            return system;
        }

        private Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }
            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader) { color = color };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_EmissionColor"))
            {
                Color emission = color * 1.65f;
                emission.a = color.a;
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
            runtimeMaterials.Add(material);
            materialColors.Add(color);
            return material;
        }

        private void ApplyMaterialFade(float fade)
        {
            for (int index = 0; index < runtimeMaterials.Count; index++)
            {
                Material material = runtimeMaterials[index];
                if (material == null)
                {
                    continue;
                }
                Color color = materialColors[index];
                color.a *= fade;
                material.color = color;
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", color);
                }
            }
        }

        private static Vector3 ResolvePosition(PresentationVfxSpawnRequest request)
        {
            if (request.FollowsTarget && request.FollowTarget != null)
            {
                return request.FollowTarget.position
                    + request.FollowTarget.TransformDirection(request.FollowLocalOffset);
            }
            return request.WorldPosition;
        }
    }
}
