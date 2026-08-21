using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Runtime safety bootstrap for the Hollow Purple orb presentation override.
    /// The prototype fighter shell can be created after scene load, so a one-shot
    /// AfterSceneLoad search can miss it. This runner keeps looking until the
    /// Gojo technique controller exists, then attaches the orb visual override.
    /// </summary>
    public sealed class PrototypeHollowPurpleOrbBootstrap : MonoBehaviour
    {
        private float nextScanAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRunner()
        {
            if (FindFirstObjectByType<PrototypeHollowPurpleOrbBootstrap>() != null)
            {
                return;
            }

            GameObject runner = new GameObject("PrototypeHollowPurpleOrbBootstrap");
            DontDestroyOnLoad(runner);
            runner.AddComponent<PrototypeHollowPurpleOrbBootstrap>();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanAt)
            {
                return;
            }

            nextScanAt = Time.unscaledTime + 0.20f;

            GojoTechniqueChainController[] gojoControllers =
                FindObjectsByType<GojoTechniqueChainController>(FindObjectsSortMode.None);

            foreach (GojoTechniqueChainController controller in gojoControllers)
            {
                if (
                    controller == null
                    || controller.GetComponent<PrototypeHollowPurpleOrbVisual>() != null
                )
                {
                    continue;
                }

                controller.gameObject.AddComponent<PrototypeHollowPurpleOrbVisual>();
            }
        }
    }
}
