using System;

namespace JJKGame.Core
{
    /// <summary>
    /// Semantic notification for a confirmed basic-attack hit. The gameplay producer
    /// reports only the chain step; presentation consumers own camera, flash, and
    /// hit-stop tuning.
    /// </summary>
    public readonly struct BasicHitPresentationRequest
    {
        public BasicHitPresentationRequest(Health owner, int chainStep)
        {
            Owner = owner;
            ChainStep = Math.Clamp(chainStep, 1, 3);
        }

        public Health Owner { get; }
        public int ChainStep { get; }
    }

    public static class BasicHitPresentationRequests
    {
        public static event Action<BasicHitPresentationRequest> Requested;

        public static void Raise(BasicHitPresentationRequest request)
        {
            Requested?.Invoke(request);
        }
    }
}
