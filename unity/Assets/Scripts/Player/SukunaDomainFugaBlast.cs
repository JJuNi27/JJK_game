using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class SukunaDomainFugaBlast : MonoBehaviour
    {
        private float radius;
        private float startedAt;
        private PresentationVfxHandle presentationHandle;

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
            presentationHandle = PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.AtWorld(
                    transform.position,
                    new Color(0.72f, 0.008f, 0.004f, 0.92f),
                    new Color(1f, 0.28f, 0.015f, 0.76f),
                    0.7f,
                    radius,
                    0.72f,
                    0f,
                    PresentationVfxTimePolicy.Scaled,
                    PresentationVfxStyleId.FugaImpact,
                    Vector3.up,
                    true
                )
            );
            ApplyDomainWideDamage(owner, damage, knockback, hitStun);

            if (owner != null)
            {
                SukunaCombatAudio.GetOrCreate(owner.gameObject)?.PlayDomainFuga();
            }
        }

        private void Update()
        {
            if (Time.time - startedAt >= 0.72f)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            presentationHandle.Stop(PresentationVfxStopMode.Immediate);
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
