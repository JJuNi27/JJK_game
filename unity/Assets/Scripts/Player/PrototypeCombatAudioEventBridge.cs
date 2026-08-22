using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Gate 4F adapter from semantic CombatAudioEvent requests to the current
    /// PrototypeCombatAudio implementation. Production audio can replace this adapter
    /// without forcing gameplay producers to know AudioClip/AudioSource details.
    /// </summary>
    [DefaultExecutionOrder(1430)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class PrototypeCombatAudioEventBridge : MonoBehaviour
    {
        private Health ownHealth;
        private PrototypeCombatAudio prototypeAudio;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            BasicAttack[] attacks = FindObjectsByType<BasicAttack>(FindObjectsSortMode.None);
            foreach (BasicAttack attack in attacks)
            {
                if (
                    attack == null
                    || attack.GetComponent<PrototypeCombatAudioEventBridge>() != null
                )
                {
                    continue;
                }

                attack.gameObject.AddComponent<PrototypeCombatAudioEventBridge>();
            }
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            prototypeAudio = PrototypeCombatAudio.GetOrCreate(gameObject);
        }

        private void OnEnable()
        {
            CombatAudioEvents.Raised -= HandleAudioEvent;
            CombatAudioEvents.Raised += HandleAudioEvent;
        }

        private void OnDisable()
        {
            CombatAudioEvents.Raised -= HandleAudioEvent;
        }

        private void HandleAudioEvent(CombatAudioEvent audioEvent)
        {
            if (ownHealth == null || audioEvent.Owner == null || audioEvent.Owner != ownHealth)
            {
                return;
            }

            prototypeAudio ??= PrototypeCombatAudio.GetOrCreate(gameObject);
            if (prototypeAudio == null)
            {
                return;
            }

            switch (audioEvent.EventId)
            {
                case CombatAudioEventId.BasicSwing:
                    prototypeAudio.PlayBasicSwing(Mathf.Max(1, audioEvent.Variant));
                    break;
                case CombatAudioEventId.BasicHit:
                    prototypeAudio.PlayBasicHit(Mathf.Max(1, audioEvent.Variant));
                    break;
                case CombatAudioEventId.Dodge:
                    prototypeAudio.PlayDodge();
                    break;
                case CombatAudioEventId.GojoBlueCast:
                    prototypeAudio.PlayBlueCast();
                    break;
                case CombatAudioEventId.GojoBlueImpact:
                    prototypeAudio.PlayBlueImpact();
                    break;
                case CombatAudioEventId.GojoRedCast:
                    prototypeAudio.PlayRedCast();
                    break;
                case CombatAudioEventId.GojoRedImpact:
                    prototypeAudio.PlayRedImpact();
                    break;
                case CombatAudioEventId.HollowPurple:
                    prototypeAudio.PlayPurple();
                    break;
                case CombatAudioEventId.UnlimitedVoid:
                    prototypeAudio.PlayDomain();
                    break;
                case CombatAudioEventId.PlayerHit:
                    prototypeAudio.PlayPlayerHit();
                    break;
                case CombatAudioEventId.Victory:
                    prototypeAudio.PlayVictory();
                    break;
                case CombatAudioEventId.Defeat:
                    prototypeAudio.PlayDefeat();
                    break;
            }
        }
    }
}
