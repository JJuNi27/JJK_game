namespace JJKGame.Core
{
    /// <summary>
    /// One release per cast. Animation adapters retain the token captured at begin;
    /// cancellation/new casts invalidate old callbacks. Timer remains the missing-event fallback.
    /// Uses the caller's clock, so existing scaled/unscaled timing ownership stays explicit.
    /// </summary>
    public sealed class TechniqueReleaseClock
    {
        private int generation;
        private bool active;
        private bool acceptPresentationRelease;
        private bool releaseRequested;
        private float fallbackAt;
        public int Token => generation;

        public int Begin(float now, float fallbackDuration, bool allowPresentationRelease)
        {
            generation++;
            active = true;
            releaseRequested = false;
            acceptPresentationRelease = allowPresentationRelease;
            fallbackAt = now + System.Math.Max(0f, fallbackDuration);
            return generation;
        }

        public bool RequestRelease(int token)
        {
            if (!active || token != generation || !acceptPresentationRelease || releaseRequested)
                return false;
            releaseRequested = true;
            return true;
        }

        public bool TryConsume(float now)
        {
            if (!active || (!releaseRequested && now < fallbackAt)) return false;
            active = false;
            return true;
        }

        public void Cancel()
        {
            active = false;
            generation++;
        }
    }
}
