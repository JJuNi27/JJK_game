using UnityEngine;

namespace JJKGame.Player
{
    public sealed class UnlimitedVoidPrototypeVisual : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float expansionDuration = 0.35f;
        [SerializeField, Min(0.1f)] private float pulseSpeed = 2.4f;
        [SerializeField, Min(0f)] private float rotationSpeed = 22f;

        private Transform ringRoot;
        private Light domainLight;
        private float activatedAt;
        private bool built;
        private float visualRadius = 10f;

        public void Configure(float domainRadius)
        {
            visualRadius = Mathf.Clamp(domainRadius * 0.35f, 6f, 12f);
            BuildVisual();
        }

        private void Awake()
        {
            BuildVisual();
        }

        private void OnEnable()
        {
            BuildVisual();
            activatedAt = Time.time;
            transform.localScale = Vector3.one * 0.05f;
        }

        private void Update()
        {
            float elapsed = Time.time - activatedAt;
            float normalized = Mathf.Clamp01(elapsed / expansionDuration);
            float eased = 1f - Mathf.Pow(1f - normalized, 3f);
            float pulse = 1f + Mathf.Sin(elapsed * pulseSpeed * Mathf.PI * 2f) * 0.035f;
            transform.localScale = Vector3.one * Mathf.Max(0.05f, eased * pulse);

            if (ringRoot != null)
            {
                ringRoot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
            }

            if (domainLight != null)
            {
                domainLight.intensity = 2.4f + Mathf.Sin(elapsed * pulseSpeed * 2f) * 0.6f;
            }
        }

        private void BuildVisual()
        {
            if (built)
            {
                return;
            }

            built = true;
            ringRoot = new GameObject("VoidRings").transform;
            ringRoot.SetParent(transform, false);

            CreateRing(
                "GroundRing",
                visualRadius,
                0.14f,
                new Color(0.18f, 0.72f, 1f, 0.95f),
                Vector3.zero,
                Quaternion.identity,
                RingPlane.XZ
            );
            CreateRing(
                "OrbitRingA",
                visualRadius * 0.48f,
                0.09f,
                new Color(0.55f, 0.32f, 1f, 0.9f),
                Vector3.up * 1.3f,
                Quaternion.Euler(18f, 0f, 12f),
                RingPlane.XY
            );
            CreateRing(
                "OrbitRingB",
                visualRadius * 0.38f,
                0.075f,
                new Color(0.12f, 0.95f, 0.92f, 0.85f),
                Vector3.up * 1.3f,
                Quaternion.Euler(-12f, 28f, 0f),
                RingPlane.YZ
            );

            GameObject lightObject = new GameObject("VoidLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition = Vector3.up * 2f;
            domainLight = lightObject.AddComponent<Light>();
            domainLight.type = LightType.Point;
            domainLight.color = new Color(0.25f, 0.48f, 1f);
            domainLight.range = visualRadius * 1.8f;
            domainLight.intensity = 2.5f;
            domainLight.shadows = LightShadows.None;
        }

        private void CreateRing(
            string objectName,
            float radius,
            float width,
            Color color,
            Vector3 localPosition,
            Quaternion localRotation,
            RingPlane plane
        )
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(ringRoot, false);
            ringObject.transform.localPosition = localPosition;
            ringObject.transform.localRotation = localRotation;

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 96;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null)
            {
                Material material = new Material(shader)
                {
                    color = color,
                };
                line.material = material;
            }

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = (float)index / line.positionCount * Mathf.PI * 2f;
                float cosine = Mathf.Cos(angle) * radius;
                float sine = Mathf.Sin(angle) * radius;
                Vector3 point = plane switch
                {
                    RingPlane.XY => new Vector3(cosine, sine, 0f),
                    RingPlane.YZ => new Vector3(0f, cosine, sine),
                    _ => new Vector3(cosine, 0f, sine),
                };
                line.SetPosition(index, point);
            }
        }

        private enum RingPlane
        {
            XZ,
            XY,
            YZ,
        }
    }
}
