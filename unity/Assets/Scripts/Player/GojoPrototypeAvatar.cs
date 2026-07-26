using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Player
{
    [DisallowMultipleComponent]
    public sealed class GojoPrototypeAvatar : MonoBehaviour
    {
        private const string VisualRootName = "PrototypeGojoAvatar";

        private readonly List<Material> runtimeMaterials = new List<Material>();
        private GojoVariantController variant;
        private Transform visualRoot;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;

        public static GojoPrototypeAvatar GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            GojoPrototypeAvatar avatar = owner.GetComponent<GojoPrototypeAvatar>();
            return avatar != null ? avatar : owner.AddComponent<GojoPrototypeAvatar>();
        }

        private void Awake()
        {
            variant = GojoVariantController.GetOrCreate(gameObject);
            Rebuild();
        }

        private void Update()
        {
            if (visualRoot == null)
            {
                return;
            }

            CharacterController controller = GetComponent<CharacterController>();
            float movement = controller != null
                ? Mathf.Clamp01(new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude / 5.5f)
                : 0f;
            float swing = Mathf.Sin(Time.time * 8f) * 18f * movement;

            if (leftArm != null)
            {
                leftArm.localRotation = Quaternion.Euler(swing, 0f, 0f);
            }
            if (rightArm != null)
            {
                rightArm.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            }
            if (leftLeg != null)
            {
                leftLeg.localRotation = Quaternion.Euler(-swing * 0.65f, 0f, 0f);
            }
            if (rightLeg != null)
            {
                rightLeg.localRotation = Quaternion.Euler(swing * 0.65f, 0f, 0f);
            }

            visualRoot.localPosition = Vector3.up * (Mathf.Sin(Time.time * 2.2f) * 0.015f);
        }

        private void OnDestroy()
        {
            foreach (Material material in runtimeMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }

        public void Rebuild()
        {
            variant ??= GojoVariantController.GetOrCreate(gameObject);
            HidePrototypeCapsule();

            Transform existing = transform.Find(VisualRootName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            foreach (Material material in runtimeMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            runtimeMaterials.Clear();

            GameObject rootObject = new GameObject(VisualRootName);
            rootObject.transform.SetParent(transform, false);
            visualRoot = rootObject.transform;

            Material uniform = CreateMaterial(new Color(0.025f, 0.035f, 0.065f, 1f));
            Material uniformHighlight = CreateMaterial(new Color(0.055f, 0.075f, 0.13f, 1f));
            Material skin = CreateMaterial(new Color(0.94f, 0.76f, 0.65f, 1f));
            Material hair = CreateMaterial(new Color(0.90f, 0.94f, 1f, 1f));
            Material black = CreateMaterial(new Color(0.005f, 0.008f, 0.018f, 1f));
            Material eyeBlue = CreateMaterial(new Color(0.12f, 0.70f, 1f, 1f), true);
            Material shoe = CreateMaterial(new Color(0.015f, 0.018f, 0.028f, 1f));

            BuildBody(uniform, uniformHighlight, skin, shoe);
            BuildHead(skin, hair, black, eyeBlue);
        }

        private void BuildBody(Material uniform, Material uniformHighlight, Material skin, Material shoe)
        {
            CreatePart("Torso", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.05f, 0f), new Vector3(0.58f, 0.72f, 0.34f), Vector3.zero, uniform);
            CreatePart("HighCollar", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.48f, 0.01f), new Vector3(0.48f, 0.22f, 0.32f), Vector3.zero, uniformHighlight);
            CreatePart("Waist", PrimitiveType.Cube, visualRoot, new Vector3(0f, -0.32f, 0f), new Vector3(0.50f, 0.16f, 0.30f), Vector3.zero, uniform);

            leftArm = CreateLimb("LeftArm", new Vector3(-0.42f, 0.08f, 0f), uniform);
            rightArm = CreateLimb("RightArm", new Vector3(0.42f, 0.08f, 0f), uniform);
            CreatePart("LeftHand", PrimitiveType.Sphere, leftArm, new Vector3(0f, -0.45f, 0f), new Vector3(0.17f, 0.17f, 0.17f), Vector3.zero, skin);
            CreatePart("RightHand", PrimitiveType.Sphere, rightArm, new Vector3(0f, -0.45f, 0f), new Vector3(0.17f, 0.17f, 0.17f), Vector3.zero, skin);

            leftLeg = CreateLeg("LeftLeg", new Vector3(-0.17f, -0.54f, 0f), uniform);
            rightLeg = CreateLeg("RightLeg", new Vector3(0.17f, -0.54f, 0f), uniform);
            CreatePart("LeftShoe", PrimitiveType.Cube, leftLeg, new Vector3(0f, -0.46f, 0.07f), new Vector3(0.23f, 0.14f, 0.36f), Vector3.zero, shoe);
            CreatePart("RightShoe", PrimitiveType.Cube, rightLeg, new Vector3(0f, -0.46f, 0.07f), new Vector3(0.23f, 0.14f, 0.36f), Vector3.zero, shoe);
        }

        private void BuildHead(Material skin, Material hair, Material black, Material eyeBlue)
        {
            CreatePart("Neck", PrimitiveType.Cylinder, visualRoot, new Vector3(0f, 0.53f, 0f), new Vector3(0.16f, 0.12f, 0.16f), Vector3.zero, skin);
            Transform head = CreatePart("Head", PrimitiveType.Sphere, visualRoot, new Vector3(0f, 0.78f, 0f), new Vector3(0.43f, 0.50f, 0.40f), Vector3.zero, skin);

            BuildHair(head, hair);
            if (variant != null && variant.UsesRoundSunglasses)
            {
                BuildSunglasses(head, black);
            }
            else if (variant != null && variant.ShowsEyes)
            {
                BuildEyes(head, eyeBlue);
            }
            else
            {
                BuildBlindfold(head, black);
            }
        }

        private void BuildHair(Transform head, Material hair)
        {
            CreatePart("HairCap", PrimitiveType.Sphere, head, new Vector3(0f, 0.20f, -0.01f), new Vector3(0.45f, 0.30f, 0.41f), Vector3.zero, hair);

            Vector3[] tuftPositions =
            {
                new Vector3(-0.25f, 0.32f, 0.02f),
                new Vector3(-0.12f, 0.38f, -0.02f),
                new Vector3(0f, 0.41f, -0.04f),
                new Vector3(0.13f, 0.38f, -0.02f),
                new Vector3(0.26f, 0.31f, 0.02f),
                new Vector3(-0.30f, 0.20f, -0.02f),
                new Vector3(0.30f, 0.20f, -0.02f),
            };
            Vector3[] tuftRotations =
            {
                new Vector3(0f, 0f, 28f),
                new Vector3(0f, 0f, 13f),
                Vector3.zero,
                new Vector3(0f, 0f, -13f),
                new Vector3(0f, 0f, -28f),
                new Vector3(0f, 0f, 44f),
                new Vector3(0f, 0f, -44f),
            };

            for (int index = 0; index < tuftPositions.Length; index++)
            {
                CreatePart($"HairTuft_{index}", PrimitiveType.Capsule, head, tuftPositions[index], new Vector3(0.15f, 0.29f, 0.15f), tuftRotations[index], hair);
            }
        }

        private void BuildBlindfold(Transform head, Material black)
        {
            CreatePart("Blindfold", PrimitiveType.Cube, head, new Vector3(0f, 0.04f, 0.36f), new Vector3(0.52f, 0.16f, 0.08f), Vector3.zero, black);
            CreatePart("BlindfoldBand", PrimitiveType.Cube, head, new Vector3(0f, 0.04f, -0.28f), new Vector3(0.47f, 0.12f, 0.07f), Vector3.zero, black);
        }

        private void BuildSunglasses(Transform head, Material black)
        {
            CreateLens("LeftLens", head, new Vector3(-0.14f, 0.04f, 0.35f), black);
            CreateLens("RightLens", head, new Vector3(0.14f, 0.04f, 0.35f), black);
            CreatePart("GlassesBridge", PrimitiveType.Cube, head, new Vector3(0f, 0.04f, 0.37f), new Vector3(0.10f, 0.035f, 0.035f), Vector3.zero, black);
        }

        private void BuildEyes(Transform head, Material eyeBlue)
        {
            CreatePart("LeftEye", PrimitiveType.Sphere, head, new Vector3(-0.12f, 0.05f, 0.37f), new Vector3(0.075f, 0.045f, 0.025f), Vector3.zero, eyeBlue);
            CreatePart("RightEye", PrimitiveType.Sphere, head, new Vector3(0.12f, 0.05f, 0.37f), new Vector3(0.075f, 0.045f, 0.025f), Vector3.zero, eyeBlue);
        }

        private void CreateLens(string name, Transform parent, Vector3 position, Material material)
        {
            CreatePart(name, PrimitiveType.Cylinder, parent, position, new Vector3(0.12f, 0.025f, 0.12f), new Vector3(90f, 0f, 0f), material);
        }

        private Transform CreateLimb(string name, Vector3 position, Material material)
        {
            GameObject pivotObject = new GameObject(name);
            pivotObject.transform.SetParent(visualRoot, false);
            pivotObject.transform.localPosition = position;
            CreatePart("Sleeve", PrimitiveType.Capsule, pivotObject.transform, new Vector3(0f, -0.20f, 0f), new Vector3(0.20f, 0.48f, 0.20f), Vector3.zero, material);
            return pivotObject.transform;
        }

        private Transform CreateLeg(string name, Vector3 position, Material material)
        {
            GameObject pivotObject = new GameObject(name);
            pivotObject.transform.SetParent(visualRoot, false);
            pivotObject.transform.localPosition = position;
            CreatePart("Trouser", PrimitiveType.Capsule, pivotObject.transform, new Vector3(0f, -0.17f, 0f), new Vector3(0.23f, 0.52f, 0.23f), Vector3.zero, material);
            return pivotObject.transform;
        }

        private Transform CreatePart(
            string name,
            PrimitiveType primitiveType,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Vector3 localEuler,
            Material material
        )
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(localEuler);
            part.transform.localScale = localScale;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }

            return part.transform;
        }

        private Material CreateMaterial(Color color, bool emission = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = shader != null ? new Material(shader) : new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.2f);
            }
            runtimeMaterials.Add(material);
            return material;
        }

        private void HidePrototypeCapsule()
        {
            Renderer rootRenderer = GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                rootRenderer.enabled = false;
            }
        }
    }
}
