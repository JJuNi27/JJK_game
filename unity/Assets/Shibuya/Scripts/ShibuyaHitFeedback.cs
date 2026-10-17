using UnityEngine;

namespace JJKGame.Shibuya
{
    public sealed class ShibuyaHitFeedback : MonoBehaviour
    {
        [SerializeField] private ShibuyaFighter source;
        [SerializeField] private Material impactMaterial;
        private AudioSource audioSource;
        private AudioClip clip;
        public void Configure(ShibuyaFighter actor, Material material) { source = actor; impactMaterial = material; }
        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.6f;
            audioSource.maxDistance = 30;
            const int rate = 22050, samples = 2646;
            float[] data = new float[samples];
            System.Random random = new System.Random(17);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                data[i] = (Mathf.Sin(t * 160f * Mathf.PI) * 0.5f + (float)(random.NextDouble() * 2 - 1) * 0.3f) * Mathf.Exp(-t * 42f);
            }
            clip = AudioClip.Create("Shibuya temporary impact", samples, 1, rate, false);
            clip.SetData(data, 0);
            source.HitConfirmed += OnHit;
        }
        private void OnHit(Vector3 point)
        {
            audioSource.transform.position = point;
            audioSource.PlayOneShot(clip, source.Profile.hitSoundVolume);
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "ShibuyaHitAccent";
            flash.transform.SetParent(transform, false);
            flash.transform.position = point;
            flash.transform.localScale = new Vector3(0.24f, 0.24f, 0.10f);
            Destroy(flash.GetComponent<Collider>());
            flash.GetComponent<Renderer>().sharedMaterial = impactMaterial;
            Destroy(flash, 0.08f);
        }
        private void OnDestroy()
        {
            if (source != null) source.HitConfirmed -= OnHit;
            if (clip != null) Destroy(clip);
        }
    }
}
