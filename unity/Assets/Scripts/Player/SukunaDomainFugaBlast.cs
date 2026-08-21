using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class SukunaDomainFugaBlast : MonoBehaviour
    {
        private readonly List<Material> runtimeMaterials = new List<Material>();
        private readonly List<LineRenderer> rings = new List<LineRenderer>();

        private float radius;
        private float startedAt;
        private Light blastLight;
        private Transform core;

        public static void Detonate(
            Health owner,
            Vector3 center,
            float blastRadius,
            float damage,
            float knockback,
            float hitStun
        )
        {
            GameObject blastObject = new GameObject("MalevolentShrineFugaBlast");
            blastObject.transform.position = center + Vector3.up * 0.18f;
            SukunaDomainFugaBlast blast = blastObject.AddComponent<SukunaDomainFugaBlast>();
            blast.Initialize(owner, blastRadius, damage, knockback, hitStun);
        }

        private void Initialize(
            Health owner,
            float blastRadius,
            float damage,
            float knockback,
            float hitStun
        )
        {
            radius = Mathf.Max(1f, blastRadius);
            startedAt = Time.time;
            BuildVisual();
            ApplyDomainWideDamage(owner, damage, knockback, hitStun);

            if (owner != null)
            {
                SukunaCombatAudio.GetOrCreate(owner.gameObject)?.PlayDomainFuga();
            }
        }

        private void Update()
        {
            const float duration = 0.72f;
            float normalized = Mathf.Clamp01((Time.time - startedAt) / duration);
            float eased = 1f - Mathf.Pow(1f - normalized, 3f);
            float scale = Mathf.Lerp(0.7f, radius, eased);
            float alpha = 1f - normalized;

            for (int index = 0; index < rings.Count; index++)
            {
                LineRenderer ring = rings[index];
                if (ring == null)
                {
                    continue;
                }

                float stagger = 1f - index * 0.11f;
                ring.transform.localScale = Vector3.one * scale * stagger;
                Color color = index switch
                {
                    0 => new Color(1f, 0.08f, 0.01f, 0.98f * alpha),
                    1 => new Color(1f, 0.34f, 0.02f, 0.90f * alpha),
                    _ => new Color(1f, 0.82f, 0.20f, 0.80f * alpha),
                };
                ring.startColor = color;
                ring.endColor = color;
            }

            if (core != null)
            {
                float coreScale = Mathf.Lerp(1.2f, radius * 0.42f, eased);
                core.localScale = Vector3.one * coreScale;
            }

            if (blastLight != null)
            {
                blastLight.range = Mathf.Lerp(radius * 0.3f, radius * 1.25f, eased);
                blastLight.intensity = Mathf.Lerp(13f, 0f, normalized);
            }

            if (normalized >= 1f)
            {
                Destroy(gameObject);
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

        private void ApplyDomainWideDamage(
            Health owner,
            float damage,
            float knockback,
            float hitStun
        )
        {
            Health[] allHealth = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health target in allHealth)
            {
                if (target == null || target == owner || target.IsDead)
                {
                    continue;
                }

                Vector3 offset = target.transform.position - transform.position;
                offset.y = 0f;
                if (offset.magnitude > radius)
                {
                    continue;
                }

                Vector3 hitPoint = target.transform.position + Vector3.up * 0.8f;
                DamageContext context = new DamageContext(
                    damage,
                    owner != null ? owner.gameObject : gameObject,
                    DamageDeliveryType.CursedTechnique,
                    DamageTraits.None,
                    "복마어주자 · 푸가",
                    hitPoint
                );
                if (target.ReceiveDamage(context) != DamageResolution.Applied)
                {
                    continue;
                }

                Vector3 pushDirection = target.transform.position - transform.position;
                pushDirection.y = 0f;
                if (pushDirection.sqrMagnitude <= 0.001f)
                {
                    pushDirection = Vector3.forward;
                }
                ApplyHitReaction(target, pushDirection.normalized * knockback, hitStun);
            }
        }

        private void BuildVisual()
        {
            rings.Add(CreateRing("DomainFugaGroundOuter", 0.13f, new Color(1f, 0.08f, 0.01f, 0.98f), Quaternion.Euler(90f, 0f, 0f)));
            rings.Add(CreateRing("DomainFugaGroundMiddle", 0.10f, new Color(1f, 0.36f, 0.02f, 0.90f), Quaternion.Euler(90f, 0f, 0f)));
            rings.Add(CreateRing("DomainFugaVertical", 0.085f, new Color(1f, 0.88f, 0.22f, 0.82f), Quaternion.identity));

            Shader shader = ResolveShader();
            if (shader != null)
            {
                GameObject coreObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coreObject.name = "DomainFugaCore";
                coreObject.transform.SetParent(transform, false);
                coreObject.transform.localPosition = Vector3.up * 0.55f;
                coreObject.transform.localScale = Vector3.one * 1.2f;
                Collider collider = coreObject.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                Material coreMaterial = CreateMaterial(new Color(1f, 0.16f, 0.01f, 0.82f), true);
                Renderer renderer = coreObject.GetComponent<Renderer>();
                if (renderer != null && coreMaterial != null)
                {
                    renderer.material = coreMaterial;
                }
                core = coreObject.transform;
            }

            GameObject lightObject = new GameObject("DomainFugaLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition = Vector3.up * 1.5f;
            blastLight = lightObject.AddComponent<Light>();
            blastLight.type = LightType.Point;
            blastLight.color = new Color(1f, 0.10f, 0.01f);
            blastLight.range = radius * 0.3f;
            blastLight.intensity = 13f;
            blastLight.shadows = LightShadows.None;
        }

        private LineRenderer CreateRing(
            string objectName,
            float width,
            Color color,
            Quaternion localRotation
        )
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localRotation = localRotation;

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 96;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            Material material = CreateMaterial(color, true);
            if (material != null)
            {
                line.material = material;
            }

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = index / (float)line.positionCount * Mathf.PI * 2f;
                line.SetPosition(index, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f));
            }
            return line;
        }

        private Material CreateMaterial(Color color, bool emission)
        {
            Shader shader = ResolveShader();
            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader) { color = color };
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.6f);
            }
            runtimeMaterials.Add(material);
            return material;
        }

        private static Shader ResolveShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            return shader != null ? shader : Shader.Find("Sprites/Default");
        }

        private static void ApplyHitReaction(Health targetHealth, Vector3 impulse, float stun)
        {
            MonoBehaviour[] behaviours = targetHealth.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, stun);
                    break;
                }
            }
        }
    }
}
