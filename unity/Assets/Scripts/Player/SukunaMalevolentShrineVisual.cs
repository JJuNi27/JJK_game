using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class SukunaMalevolentShrineVisual : MonoBehaviour
    {
        private readonly List<Material> runtimeMaterials = new List<Material>();
        private readonly List<LineRenderer> groundLines = new List<LineRenderer>();
        private readonly List<Color> groundColors = new List<Color>();

        private float radius;
        private float startedAt;
        private float fadeStartedAt;
        private bool fadingOut;
        private Transform shrineRoot;
        private Light shrineLight;

        public void Configure(float newRadius)
        {
            radius = Mathf.Max(1f, newRadius);
            startedAt = Time.time;
            BuildOpenDomainFloor();
            BuildShrine();
        }

        public void PulseSlashAt(Vector3 worldPosition, int pulseIndex)
        {
            float angle = (pulseIndex * 47f + worldPosition.x * 11f + worldPosition.z * 7f) % 180f;
            Vector3 directionA = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 directionB = Quaternion.Euler(0f, angle + 82f, 0f) * Vector3.forward;
            float length = 2.3f + (pulseIndex % 3) * 0.35f;
            float height = 0.55f + (pulseIndex % 4) * 0.18f;

            CreateTemporarySlash(
                worldPosition + Vector3.up * height,
                directionA,
                length,
                new Color(1f, 0.12f, 0.08f, 0.96f)
            );
            CreateTemporarySlash(
                worldPosition + Vector3.up * (height + 0.28f),
                directionB,
                length * 0.82f,
                new Color(1f, 0.64f, 0.18f, 0.90f)
            );
        }

        public void BeginFadeOut()
        {
            if (fadingOut)
            {
                return;
            }

            fadingOut = true;
            fadeStartedAt = Time.time;
        }

        private void Update()
        {
            float elapsed = Time.time - startedAt;
            float pulse = 1f + Mathf.Sin(elapsed * 5.5f) * 0.025f;
            if (shrineRoot != null)
            {
                shrineRoot.localScale = Vector3.one * pulse;
            }
            if (shrineLight != null && !fadingOut)
            {
                shrineLight.intensity = 4.8f + Mathf.Sin(elapsed * 8f) * 0.8f;
            }

            float alphaMultiplier = 1f;
            if (fadingOut)
            {
                const float fadeDuration = 0.55f;
                alphaMultiplier = 1f - Mathf.Clamp01((Time.time - fadeStartedAt) / fadeDuration);
                if (alphaMultiplier <= 0f)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            for (int index = 0; index < groundLines.Count; index++)
            {
                LineRenderer line = groundLines[index];
                if (line == null)
                {
                    continue;
                }

                Color color = groundColors[index];
                color.a *= alphaMultiplier * (0.78f + Mathf.Sin(elapsed * 3.2f + index) * 0.16f);
                line.startColor = color;
                line.endColor = color;
            }

            if (shrineLight != null && fadingOut)
            {
                shrineLight.intensity = 5f * alphaMultiplier;
            }
        }

        private void OnDestroy()
        {
            foreach (Material material in runtimeMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }

        private void BuildOpenDomainFloor()
        {
            CreateGroundRing("ShrineRangeOuter", radius, 0.12f, new Color(0.72f, 0.03f, 0.025f, 0.78f));
            CreateGroundRing("ShrineRangeMiddle", radius * 0.66f, 0.08f, new Color(0.92f, 0.12f, 0.04f, 0.60f));
            CreateGroundRing("ShrineRangeInner", radius * 0.33f, 0.065f, new Color(1f, 0.42f, 0.08f, 0.56f));

            for (int index = 0; index < 12; index++)
            {
                float angle = index / 12f * Mathf.PI * 2f;
                Vector3 end = new Vector3(Mathf.Cos(angle) * radius, 0.035f, Mathf.Sin(angle) * radius);
                CreateGroundSpoke($"ShrineSpoke_{index}", end, new Color(0.65f, 0.02f, 0.02f, 0.34f));
            }
        }

        private void BuildShrine()
        {
            GameObject rootObject = new GameObject("OpenMalevolentShrine");
            rootObject.transform.SetParent(transform, false);
            rootObject.transform.localPosition = Vector3.up * 0.05f;
            shrineRoot = rootObject.transform;

            Material dark = CreateMaterial(new Color(0.055f, 0.008f, 0.010f, 1f));
            Material red = CreateMaterial(new Color(0.34f, 0.015f, 0.018f, 1f), true);
            Material bone = CreateMaterial(new Color(0.38f, 0.25f, 0.18f, 1f));
            Material ember = CreateMaterial(new Color(0.95f, 0.12f, 0.025f, 1f), true);

            CreatePart("ShrineBase", PrimitiveType.Cube, shrineRoot, new Vector3(0f, 0.24f, 0f), new Vector3(5.8f, 0.45f, 4.5f), Vector3.zero, dark);
            CreatePart("ShrinePlatform", PrimitiveType.Cube, shrineRoot, new Vector3(0f, 0.54f, 0f), new Vector3(4.9f, 0.30f, 3.7f), Vector3.zero, red);

            for (int side = -1; side <= 1; side += 2)
            {
                CreatePart($"PillarFront_{side}", PrimitiveType.Cylinder, shrineRoot, new Vector3(side * 1.65f, 1.75f, 1.15f), new Vector3(0.30f, 1.55f, 0.30f), Vector3.zero, bone);
                CreatePart($"PillarBack_{side}", PrimitiveType.Cylinder, shrineRoot, new Vector3(side * 1.65f, 1.75f, -1.15f), new Vector3(0.30f, 1.55f, 0.30f), Vector3.zero, bone);
                CreatePart($"SideJaw_{side}", PrimitiveType.Cube, shrineRoot, new Vector3(side * 2.15f, 1.40f, 0f), new Vector3(0.38f, 1.25f, 2.7f), new Vector3(0f, 0f, side * -7f), dark);
            }

            CreatePart("RoofLower", PrimitiveType.Cube, shrineRoot, new Vector3(0f, 3.05f, 0f), new Vector3(5.5f, 0.38f, 4.1f), new Vector3(0f, 0f, 0f), dark);
            CreatePart("RoofUpper", PrimitiveType.Cube, shrineRoot, new Vector3(0f, 3.38f, 0f), new Vector3(4.4f, 0.30f, 3.25f), new Vector3(0f, 0f, 0f), red);
            CreatePart("RoofCrest", PrimitiveType.Cube, shrineRoot, new Vector3(0f, 3.70f, 0f), new Vector3(2.8f, 0.25f, 0.55f), Vector3.zero, bone);

            CreatePart("CentralMouth", PrimitiveType.Cube, shrineRoot, new Vector3(0f, 1.65f, 1.68f), new Vector3(2.4f, 1.15f, 0.28f), Vector3.zero, dark);
            for (int tooth = -4; tooth <= 4; tooth++)
            {
                CreatePart($"UpperTooth_{tooth}", PrimitiveType.Capsule, shrineRoot, new Vector3(tooth * 0.24f, 2.04f, 1.90f), new Vector3(0.09f, 0.24f, 0.09f), new Vector3(0f, 0f, 180f), bone);
                CreatePart($"LowerTooth_{tooth}", PrimitiveType.Capsule, shrineRoot, new Vector3(tooth * 0.24f, 1.25f, 1.90f), new Vector3(0.09f, 0.24f, 0.09f), Vector3.zero, bone);
            }

            for (int flame = 0; flame < 6; flame++)
            {
                float angle = flame / 6f * Mathf.PI * 2f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * 2.8f, 0.75f, Mathf.Sin(angle) * 2.3f);
                CreatePart($"Ember_{flame}", PrimitiveType.Sphere, shrineRoot, position, Vector3.one * 0.22f, Vector3.zero, ember);
            }

            GameObject lightObject = new GameObject("MalevolentShrineLight");
            lightObject.transform.SetParent(shrineRoot, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.0f, 0.8f);
            shrineLight = lightObject.AddComponent<Light>();
            shrineLight.type = LightType.Point;
            shrineLight.color = new Color(1f, 0.08f, 0.025f);
            shrineLight.range = 18f;
            shrineLight.intensity = 5f;
            shrineLight.shadows = LightShadows.None;
        }

        private void CreateGroundRing(string objectName, float ringRadius, float width, Color color)
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localPosition = Vector3.up * 0.035f;

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 128;
            line.startWidth = width;
            line.endWidth = width;
            line.numCornerVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = CreateMaterial(color, true);

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = index / (float)line.positionCount * Mathf.PI * 2f;
                line.SetPosition(index, new Vector3(Mathf.Cos(angle) * ringRadius, 0f, Mathf.Sin(angle) * ringRadius));
            }

            groundLines.Add(line);
            groundColors.Add(color);
        }

        private void CreateGroundSpoke(string objectName, Vector3 end, Color color)
        {
            GameObject lineObject = new GameObject(objectName);
            lineObject.transform.SetParent(transform, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(0f, 0.035f, 0f));
            line.SetPosition(1, end);
            line.startWidth = 0.035f;
            line.endWidth = 0.015f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = CreateMaterial(color, true);

            groundLines.Add(line);
            groundColors.Add(color);
        }

        private void CreateTemporarySlash(Vector3 center, Vector3 direction, float length, Color color)
        {
            GameObject slashObject = new GameObject("MalevolentShrineSureHitSlash");
            LineRenderer line = slashObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, center - direction.normalized * length * 0.5f);
            line.SetPosition(1, center + direction.normalized * length * 0.5f);
            line.startWidth = 0.14f;
            line.endWidth = 0.025f;
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0.08f);
            line.numCapVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = CreateMaterial(color, true);
            Destroy(slashObject, 0.19f);
        }

        private Transform CreatePart(string objectName, PrimitiveType primitive, Transform parent, Vector3 position, Vector3 scale, Vector3 euler, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = objectName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.Euler(euler);
            part.transform.localScale = scale;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }
            return part.transform;
        }

        private Material CreateMaterial(Color color, bool emission = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader) { color = color };
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.2f);
            }
            runtimeMaterials.Add(material);
            return material;
        }
    }
}
