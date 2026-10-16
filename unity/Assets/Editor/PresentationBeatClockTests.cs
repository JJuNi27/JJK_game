using JJKGame.Core;
using NUnit.Framework;

namespace JJKGame.EditorTools
{
    public sealed class PresentationBeatClockTests
    {
        [Test]
        public void HitchDeliversAllCrossedBeatsOnceInOrder()
        {
            var clock = new PresentationBeatClock();
            clock.Reset(0f, 0.22f, 0.94f, 1.28f);
            for (int i = 0; i < 4; i++)
            {
                Assert.That(clock.TryConsume(2f, out int beat), Is.True);
                Assert.That(beat, Is.EqualTo(i));
            }
            Assert.That(clock.TryConsume(2f, out _), Is.False);
        }

        [Test]
        public void PauseDoesNotRepeatAndResetStartsNewSequence()
        {
            var clock = new PresentationBeatClock();
            clock.Reset(0f, 0.5f);
            Assert.That(clock.TryConsume(0f, out _), Is.True);
            Assert.That(clock.TryConsume(0f, out _), Is.False);
            Assert.That(clock.TryConsume(0.49f, out _), Is.False);
            clock.Reset(0f, 0.5f);
            Assert.That(clock.TryConsume(0f, out int beat), Is.True);
            Assert.That(beat, Is.Zero);
        }
    }
}
