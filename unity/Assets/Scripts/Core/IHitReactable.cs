using UnityEngine;

namespace JJKGame.Core
{
    public interface IHitReactable
    {
        void ApplyHitReaction(Vector3 impulse, float stunDuration);
    }
}
