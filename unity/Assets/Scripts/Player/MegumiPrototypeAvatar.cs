using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Gate 5A prototype-only visual for Megumi. This is intentionally simple and is not
    /// a production character model. It only gives the animation/presentation contracts a
    /// third visual root with the same minimal arm naming convention used by the prototype.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MegumiPrototypeAvatar : MonoBehaviour
    {
        public const string VisualRootName = "PrototypeMegumiAvatar";

        private readonly List<Material> runtimeMaterials = new List<Material>();
        private Transform visualRoot;

        public static MegumiPrototypeAvatar GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            MegumiPrototypeAvatar avatar = owner.GetComponent<MegumiPrototypeAvatar>();
            return avatar != null ? avatar : owner.AddComponent<MegumiPrototypeAvatar>();
        }

        private void Awake()
        {
            Build();
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

        private void Build()
        {
            Renderer ownerRenderer = GetComponent<Renderer>();
            if (ownerRenderer != null)
            {
                ownerRenderer.enabled = false;
            }

            Transform existing = transform.Find(VisualRootName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            GameObject rootObject = new GameObject(VisualRootName);
            rootObject.transform.SetParent(transform, false);
            visualRoot = rootObject.transform;

            Material uniform = CreateMaterial(new Color(0.025f, 0.055f, 0.085f, 1f));
            Material uniformDark = CreateMaterial(new Color(0.012f, 0.025f, 0.040f, 1f));
            Material skin = CreateMaterial(new Color(0.88f, 0.68f, 0.56f, 1f));
            Material hair = CreateMaterial(new Color(0.018f, 0.025f, 0.032f, 1f));
            Material eye = CreateMaterial(new Color(0.10f, 0.16f, 0.20f, 1f));

            CreatePart(
                "Torso",
                PrimitiveType.Cube,
                visualRoot,
                new Vector3(0f, 0.04f, 0f),
                new Vector3(0.56f, 0.70f, 0.33f),
                Vector3.zero,
                uniform
            );
            CreatePart(
                "Collar",
                PrimitiveType.Cube,
                visualRoot,
                new Vector3(0f, 0.44f, 0.01f),
                new Vector3(0.48f, 0.17f, 0.31f),
                Vector3.zero,
                uniformDark
            );

            Transform leftArm = CreatePivot("LeftArm", new Vector3(-0.41f, 0.09f, 0f));
            Transform rightArm = CreatePivot("RightArm", new Vector3(0.41f, 0.09f, 0f));
            CreatePart("Sleeve", PrimitiveType.Capsule, leftArm, new Vector3(0f, -0.20f, 0f), new Vector3(0.19f, 0.47f, 0.19f), Vector3.zero, uniform);
            CreatePart("Sleeve", PrimitiveType.Capsule, rightArm, new Vector3(0f, -0.20f, 0f), new Vector3(0.19f, 0.47f, 0.19f), Vector3.zero, uniform);
            CreatePart("LeftHand", PrimitiveType.Sphere, leftArm, new Vector3(0f, -0.45f, 0f), Vector3.one * 0.16f, Vector3.zero, skin);
            CreatePart("RightHand", PrimitiveType.Sphere, rightArm, new Vector3(0f, -0.45f, 0f), Vector3.one * 0.16f, Vector3.zero, skin);

            BuildLeg("LeftLeg", -0.17f, uniformDark);
            BuildLeg("RightLeg", 0.17f, uniformDark);

            Transform head = CreatePart(
                "Head",
                PrimitiveType.Sphere,
                visualRoot,
                new Vector3(0f, 0.78f, 0f),
                new Vector3(0.42f, 0.48f, 0.39f),
                Vector3.zero,
                skin
            );
            CreatePart("LeftEye", PrimitiveType.Sphere, head, new Vector3(-0.115f, 0.035f, 0.36f), new Vector3(0.055f, 0.038f, 0.022f), Vector3.zero, eye);
            CreatePart("RightEye", PrimitiveType.Sphere, head, new Vector3(0.115f, 0.035f, 0.36f), new Vector3(0.055f, 0.038f, 0.022f), Vector3.zero, eye);

            Vector3[] hairPositions =
            {
                new Vector3(0f, 0.31f, -0.02f),
                new Vector3(-0.18f, 0.27f, 0f),
                new Vector3(0.18f, 0.27f, 0f),
                new Vector3(-0.28f, 0.15f, 0f),
                new Vector3(0.28f, 0.15f, 0f),
            };
            Vector3[] hairEuler =
            {
                Vector3.zero,
                new Vector3(0f, 0f, 28f),
                new Vector3(0f, 0f, -28f),
                new Vector3(0f, 0f, 48f),
                new Vector3(0f, 0f, -48f),
            };

            for (int index = 0; index < hairPositions.Length; index++)
            {
                CreatePart(
                    $"HairSpike_{index}",
                    PrimitiveType.Capsule,
                    head,
                    hairPositions[index],
                    new Vector3(0.13f, 0.30f, 0.13f),
                    hairEuler[index],
                    hair
                );
            }
        }

        private void BuildLeg(string name, float x, Material material)
        {
            Transform leg = CreatePivot(name, new Vector3(x, -0.54f, 0f));
            CreatePart("Trouser", PrimitiveType.Capsule, leg, new Vector3(0f, -0.18f, 0f), new Vector3(0.22f, 0.51f, 0.22f), Vector3.zero, material);
            CreatePart("Shoe", PrimitiveType.Cube, leg, new Vector3(0f, -0.47f, 0.07f), new Vector3(0.23f, 0.14f, 0.34f), Vector3.zero, material);
        }

        private Transform CreatePivot(string name, Vector3 localPosition)
        {
            GameObject pivot = new GameObject(name);
            pivot.transform.SetParent(visualRoot, false);
            pivot.transform.localPosition = localPosition;
            return pivot.transform;
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
            part.transform.localScale = localScale;
            part.transform.localRotation = Quaternion.Euler(localEuler);

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }

            return part.transform;
        }

        private Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader) { color = color };
            runtimeMaterials.Add(material);
            return material;
        }
    }
}
