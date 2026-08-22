using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DisallowMultipleComponent]
    public sealed class MegumiDivineDogSummon : MonoBehaviour
    {
        private readonly List<Material> runtimeMaterials = new List<Material>();

        private Health owner;
        private Health currentTarget;
        private float expiresAt;
        private float damage;
        private float moveSpeed;
        private float attackRange;
        private float attackInterval;
        private float nextAttackAt;
        private bool dismissing;

        public void Initialize(
            Health summonOwner,
            Health preferredTarget,
            float lifetime,
            float summonDamage,
            float summonMoveSpeed,
            float summonAttackRange,
            float summonAttackInterval
        )
        {
            owner = summonOwner;
            currentTarget = IsValidTarget(preferredTarget) ? preferredTarget : null;
            expiresAt = Time.time + Mathf.Max(0.5f, lifetime);
            damage = Mathf.Max(0.1f, summonDamage);
            moveSpeed = Mathf.Max(0.1f, summonMoveSpeed);
            attackRange = Mathf.Max(0.2f, summonAttackRange);
            attackInterval = Mathf.Max(0.1f, summonAttackInterval);
            nextAttackAt = Time.time + 0.28f;
            BuildPrototypeVisual();
        }

        private void Update()
        {
            if (owner == null || owner.IsDead || Time.time >= expiresAt)
            {
                Dismiss();
                return;
            }

            if (!IsValidTarget(currentTarget))
            {
                currentTarget = FindNearestTarget();
            }

            if (currentTarget == null)
            {
                FollowOwner();
                return;
            }

            ChaseAndAttack(currentTarget);
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
            runtimeMaterials.Clear();
        }

        public void Dismiss()
        {
            if (dismissing)
            {
                return;
            }

            dismissing = true;
            if (owner != null)
            {
                TechniquePresentationRequests.Raise(
                    TechniquePresentationRequest.AtWorldPoint(
                        owner,
                        TechniquePresentationId.DivineDog,
                        TechniquePresentationPhase.End,
                        transform.position
                    )
                );
            }
            Destroy(gameObject);
        }

        private void ChaseAndAttack(Health target)
        {
            Vector3 offset = target.transform.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > 0.001f)
            {
                Vector3 direction = offset / distance;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction, Vector3.up),
                    1f - Mathf.Exp(-14f * Time.deltaTime)
                );

                if (distance > attackRange * 0.85f)
                {
                    transform.position += direction * moveSpeed * Time.deltaTime;
                }
            }

            if (distance > attackRange || Time.time < nextAttackAt)
            {
                return;
            }

            nextAttackAt = Time.time + attackInterval;
            Vector3 hitPoint = target.transform.position + Vector3.up * 0.75f;
            DamageContext context = new DamageContext(
                damage,
                owner.gameObject,
                DamageDeliveryType.CursedTechnique,
                DamageTraits.None,
                "옥견",
                hitPoint
            );
            if (target.ReceiveDamage(context) != DamageResolution.Applied)
            {
                return;
            }

            Vector3 hitDirection = target.transform.position - transform.position;
            hitDirection.y = 0f;
            if (hitDirection.sqrMagnitude <= 0.001f)
            {
                hitDirection = transform.forward;
            }
            ApplyHitReaction(target, hitDirection.normalized * 4.2f, 0.14f);

            TechniquePresentationRequests.Raise(
                TechniquePresentationRequest.AtPose(
                    owner,
                    TechniquePresentationId.DivineDog,
                    TechniquePresentationPhase.Impact,
                    hitPoint,
                    hitDirection
                )
            );
            CombatAudioEvents.Raise(
                CombatAudioEvent.ForOwner(owner, CombatAudioEventId.DivineDog, 2)
            );
        }

        private void FollowOwner()
        {
            Vector3 desired =
                owner.transform.position
                - owner.transform.forward * 1.1f
                + owner.transform.right * 0.8f;
            desired.y = owner.transform.position.y + 0.10f;
            Vector3 offset = desired - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= 0.04f)
            {
                return;
            }

            Vector3 direction = offset.normalized;
            transform.position += direction * (moveSpeed * 0.70f) * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                1f - Mathf.Exp(-10f * Time.deltaTime)
            );
        }

        private Health FindNearestTarget()
        {
            Health[] allHealth = FindObjectsByType<Health>(FindObjectsSortMode.None);
            Health nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (Health candidate in allHealth)
            {
                if (!IsValidTarget(candidate))
                {
                    continue;
                }

                Vector3 offset = candidate.transform.position - transform.position;
                offset.y = 0f;
                float distance = offset.sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }
            return nearest;
        }

        private bool IsValidTarget(Health candidate)
        {
            return candidate != null
                && candidate != owner
                && !candidate.IsDead
                && candidate.gameObject.activeInHierarchy;
        }

        private static void ApplyHitReaction(Health target, Vector3 impulse, float stun)
        {
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, stun);
                    break;
                }
            }
        }

        private void BuildPrototypeVisual()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            if (shader == null)
            {
                return;
            }

            Material body = new Material(shader) { color = new Color(0.025f, 0.045f, 0.060f, 1f) };
            Material accent = new Material(shader) { color = new Color(0.12f, 0.46f, 0.50f, 1f) };
            runtimeMaterials.Add(body);
            runtimeMaterials.Add(accent);

            CreatePart("Body", PrimitiveType.Capsule, new Vector3(0f, 0.45f, 0f), new Vector3(0.42f, 0.42f, 0.72f), new Vector3(90f, 0f, 0f), body);
            CreatePart("Chest", PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0.38f), new Vector3(0.48f, 0.42f, 0.50f), Vector3.zero, body);
            CreatePart("Head", PrimitiveType.Sphere, new Vector3(0f, 0.70f, 0.72f), new Vector3(0.40f, 0.36f, 0.45f), Vector3.zero, body);
            CreatePart("Muzzle", PrimitiveType.Cube, new Vector3(0f, 0.62f, 1.02f), new Vector3(0.24f, 0.18f, 0.32f), Vector3.zero, accent);
            CreatePart("LeftEar", PrimitiveType.Cube, new Vector3(-0.18f, 0.96f, 0.73f), new Vector3(0.14f, 0.34f, 0.12f), new Vector3(0f, 0f, -18f), body);
            CreatePart("RightEar", PrimitiveType.Cube, new Vector3(0.18f, 0.96f, 0.73f), new Vector3(0.14f, 0.34f, 0.12f), new Vector3(0f, 0f, 18f), body);

            CreatePart("FrontLeftLeg", PrimitiveType.Capsule, new Vector3(-0.22f, 0.20f, 0.40f), new Vector3(0.15f, 0.32f, 0.15f), Vector3.zero, body);
            CreatePart("FrontRightLeg", PrimitiveType.Capsule, new Vector3(0.22f, 0.20f, 0.40f), new Vector3(0.15f, 0.32f, 0.15f), Vector3.zero, body);
            CreatePart("BackLeftLeg", PrimitiveType.Capsule, new Vector3(-0.22f, 0.20f, -0.35f), new Vector3(0.15f, 0.32f, 0.15f), Vector3.zero, body);
            CreatePart("BackRightLeg", PrimitiveType.Capsule, new Vector3(0.22f, 0.20f, -0.35f), new Vector3(0.15f, 0.32f, 0.15f), Vector3.zero, body);
            CreatePart("Tail", PrimitiveType.Capsule, new Vector3(0f, 0.58f, -0.72f), new Vector3(0.12f, 0.42f, 0.12f), new Vector3(62f, 0f, 0f), body);
        }

        private void CreatePart(
            string partName,
            PrimitiveType type,
            Vector3 localPosition,
            Vector3 localScale,
            Vector3 localEuler,
            Material material
        )
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = partName;
            part.transform.SetParent(transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.transform.localRotation = Quaternion.Euler(localEuler);
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
        }
    }
}
