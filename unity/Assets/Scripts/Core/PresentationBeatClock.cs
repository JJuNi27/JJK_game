using System;

namespace JJKGame.Core
{
    /// <summary>Ordered presentation beats, independent of damage and animation clocks.
    /// Hosts supply elapsed time; a hitch delivers every crossed beat exactly once.</summary>
    public sealed class PresentationBeatClock
    {
        private float[] times = Array.Empty<float>();
        private int next;
        public void Reset(params float[] orderedTimes)
        {
            times = (float[])orderedTimes.Clone();
            for (int i = 0; i < times.Length; i++)
                times[i] = Math.Max(i == 0 ? 0f : times[i - 1], times[i]);
            next = 0;
        }
        public bool TryConsume(float elapsed, out int beat)
        {
            beat = next;
            if (next >= times.Length || elapsed < times[next]) return false;
            next++;
            return true;
        }
    }
}
