using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Prototype-only observation source for audio events that are not yet emitted by a
    /// dedicated production gameplay/presentation source. It observes existing prototype
    /// health/result/visual state and raises semantic CombatAudioEvents, but never plays clips.
    /// </summary>
    [DefaultExecutionOrder(1420)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class PrototypeCombatAudioEventSource : MonoBehaviour
    {
        private readonly HashSet<Health> trackedHealth = new HashSet<Health>();

        private Health ownHealth;
        private Transform purpleVisual;
        private Transform domainVisual;
        private float lastOwnerHealth;
        private float nextHealthRefreshAt;
        private bool purpleWasActive;
        private bool domainWasActive;
        private bool ownerHealthBound;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            BasicAttack[] attacks = FindObjectsByType<BasicAttack>(FindObjectsSortMode.None);
            foreach (BasicAttack attack in attacks)
            {
                if (
                    attack == null
                    || attack.GetComponent<PrototypeCombatAudioEventSource>() != null
                )
                {
                    continue;
                }

                attack.gameObject.AddComponent<PrototypeCombatAudioEventSource>();
            }
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
        }

        private void Start()
        {
            RefreshHealthBindings();
            LocateTechniqueVisuals();
        }

        private void Update()
        {
            if (Time.time >= nextHealthRefreshAt)
            {
                RefreshHealthBindings();
            }

            DetectTechniqueVisualActivations();
        }

        private void OnDestroy()
        {
            if (ownHealth != null && ownerHealthBound)
            {
                ownHealth.HealthChanged -= HandleOwnerHealthChanged;
            }

            foreach (Health health in trackedHealth)
            {
                if (health != null)
                {
                    health.Died -= HandleAnyDeath;
                }
            }
        }

        private void RefreshHealthBindings()
        {
            nextHealthRefreshAt = Time.time + 0.5f;
            Health[] healthObjects = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health health in healthObjects)
            {
                if (health == null || !trackedHealth.Add(health))
                {
                    continue;
                }

                health.Died += HandleAnyDeath;
            }

            if (ownHealth != null && !ownerHealthBound)
            {
                ownerHealthBound = true;
                lastOwnerHealth = ownHealth.CurrentHealth;
                ownHealth.HealthChanged += HandleOwnerHealthChanged;
            }
        }

        private void HandleOwnerHealthChanged(Health _, float currentHealth)
        {
            if (currentHealth < lastOwnerHealth)
            {
                Raise(CombatAudioEventId.PlayerHit);
            }

            lastOwnerHealth = currentHealth;
        }

        private void HandleAnyDeath(Health deadHealth)
        {
            if (deadHealth == ownHealth)
            {
                Raise(CombatAudioEventId.Defeat);
                return;
            }

            bool foundLivingOpponent = false;
            foreach (Health health in trackedHealth)
            {
                if (health != null && health != ownHealth && !health.IsDead)
                {
                    foundLivingOpponent = true;
                    break;
                }
            }

            if (!foundLivingOpponent)
            {
                Raise(CombatAudioEventId.Victory);
            }
        }

        private void LocateTechniqueVisuals()
        {
            purpleVisual ??= transform.Find("HollowPurplePrototypeVisual");
            domainVisual ??= transform.Find("UnlimitedVoidPrototypeVisual");
        }

        private void DetectTechniqueVisualActivations()
        {
            LocateTechniqueVisuals();

            bool purpleActive = purpleVisual != null && purpleVisual.gameObject.activeInHierarchy;
            if (purpleActive && !purpleWasActive)
            {
                Raise(CombatAudioEventId.HollowPurple);
            }
            purpleWasActive = purpleActive;

            bool domainActive = domainVisual != null && domainVisual.gameObject.activeInHierarchy;
            if (domainActive && !domainWasActive)
            {
                Raise(CombatAudioEventId.UnlimitedVoid);
            }
            domainWasActive = domainActive;
        }

        private void Raise(CombatAudioEventId eventId)
        {
            if (ownHealth == null)
            {
                return;
            }

            CombatAudioEvents.Raise(CombatAudioEvent.ForOwner(ownHealth, eventId));
        }
    }
}
