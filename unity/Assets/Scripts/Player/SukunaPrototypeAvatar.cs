using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Player
{
    [DisallowMultipleComponent]
    public sealed class SukunaPrototypeAvatar : MonoBehaviour
    {
        private const string VisualRootName = "PrototypeSukunaAvatar";

        private readonly List<Material> runtimeMaterials = new List<Material>();
        private Transform visualRoot;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;

        public static SukunaPrototypeAvatar GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            SukunaPrototypeAvatar avatar = owner.GetComponent<SukunaPrototypeAvatar>();
            return avatar != null ? avatar : owner.AddComponent<SukunaPrototypeAvatar>();
        }

        private void Awake()
        {
            Build();
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
            float swing = Mathf.Sin(Time.time * 8.5f) * 19f * movement;

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
            Renderer rootRenderer = GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                rootRenderer.enabled = false;
            }

            Transform oldRoot = transform.Find(VisualRootName);
            if (oldRoot != null)
            {
                Destroy(oldRoot.gameObject);
            }

            GameObject rootObject = new GameObject(VisualRootName);
            rootObject.transform.SetParent(transform, false);
            visualRoot = rootObject.transform;

            Material uniform = CreateMaterial(new Color(0.075f, 0.025f, 0.025f, 1f));
            Material sash = CreateMaterial(new Color(0.46f, 0.05f, 0.04f, 1f));
            Material skin = CreateMaterial(new Color(0.92f, 0.69f, 0.57f, 1f));
            Material hair = CreateMaterial(new Color(0.94f, 0.40f, 0.48f, 1f));
            Material markings = CreateMaterial(new Color(0.06f, 0.015f, 0.02f, 1f));
            Material eyes = CreateMaterial(new Color(0.95f, 0.08f, 0.05f, 1f), true);
            Material shoes = CreateMaterial(new Color(0.02f, 0.016f, 0.018f, 1f));

            CreatePart("Torso", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.05f, 0f), new Vector3(0.60f, 0.72f, 0.35f), Vector3.zero, uniform);
            CreatePart("Sash", PrimitiveType.Cube, visualRoot, new Vector3(0f, -0.31f, 0.01f), new Vector3(0.57f, 0.16f, 0.36f), Vector3.zero, sash);

            leftArm = CreateLimb("LeftArm", new Vector3(-0.43f, 0.08f, 0f), uniform, skin);
            rightArm = CreateLimb("RightArm", new Vector3(0.43f, 0.08f, 0f), uniform, skin);
            leftLeg = CreateLeg("LeftLeg", new Vector3(-0.17f, -0.54f, 0f), uniform, shoes);
            rightLeg = CreateLeg("RightLeg", new Vector3(0.17f, -0.54f, 0f), uniform, shoes);

            Transform head = CreatePart("Head", PrimitiveType.Sphere, visualRoot, new Vector3(0f, 0.80f, 0f), new Vector3(0.43f, 0.50f, 0.40f), Vector3.zero, skin);
            BuildHair(head, hair);
            BuildEyesAndMarks(head, eyes, markings);
        }

        private void BuildHair(Transform head, Material hair)
        {
            CreatePart("HairCap", PrimitiveType.Sphere, head, new Vector3(0f, 0.20f, -0.01f), new Vector3(0.45f, 0.28f, 0.41f), Vector3.zero, hair);
            Vector3[] positions =
            {
                new Vector3(-0.25f, 0.31f, 0.01f),
                new Vector3(-0.11f, 0.38f, -0.02f),
                new Vector3(0.02f, 0.40f, -0.03f),
                new Vector3(0.15f, 0.36f, -0.01f),
                new Vector3(0.27f, 0.29f, 0.02f),
            };
            for (int index = 0; index < positions.Length; index++)
            {
                float zRotation = Mathf.Lerp(28f, -28f, (float)index / (positions.Length - 1));
                CreatePart(
                    $"HairSpike_{index}",
                    PrimitiveType.Capsule,
                    head,
                    positions[index],
                    new Vector3(0.15f, 0.28f, 0.15f),
                    new Vector3(0f, 0f, zRotation),
                    hair
                );
            }
        }

        private void BuildEyesAndMarks(Transform head, Material eyes, Material markings)
        {
            CreatePart("LeftEye", PrimitiveType.Sphere, head, new Vector3(-0.12f, 0.05f, 0.37f), new Vector3(0.075f, 0.042f, 0.024f), Vector3.zero, eyes);
            CreatePart("RightEye", PrimitiveType.Sphere, head, new Vector3(0.12f, 0.05f, 0.37f), new Vector3(0.075f, 0.042f, 0.024f), Vector3.zero, eyes);

            CreatePart("ForeheadMark", PrimitiveType.Cube, head, new Vector3(0f, 0.20f, 0.37f), new Vector3(0.05f, 0.15f, 0.018f), Vector3.zero, markings);
            CreatePart("LeftCheekMark", PrimitiveType.Cube, head, new Vector3(-0.23f, -0.05f, 0.33f), new Vector3(0.16f, 0.035f, 0.018f), new Vector3(0f, 0f, -18f), markings);
            CreatePart("RightCheekMark", PrimitiveType.Cube, head, new Vector3(0.23f, -0.05f, 0.33f), new Vector3(0.16f, 0.035f, 0.018f), new Vector3(0f, 0f, 18f), markings);
            CreatePart("NoseMark", PrimitiveType.Cube, head, new Vector3(0f, -0.02f, 0.39f), new Vector3(0.035f, 0.10f, 0.018f), Vector3.zero, markings);
        }

        private Transform CreateLimb(string name, Vector3 position, Material uniform, Material skin)
        {
            GameObject pivot = new GameObject(name);
            pivot.transform.SetParent(visualRoot, false);
            pivot.transform.localPosition = position;
            CreatePart("Sleeve", PrimitiveType.Capsule, pivot.transform, new Vector3(0f, -0.20f, 0f), new Vector3(0.20f, 0.48f, 0.20f), Vector3.zero, uniform);
            CreatePart("Hand", PrimitiveType.Sphere, pivot.transform, new Vector3(0f, -0.45f, 0f), new Vector3(0.17f, 0.17f, 0.17f), Vector3.zero, skin);
            return pivot.transform;
        }

        private Transform CreateLeg(string name, Vector3 position, Material uniform, Material shoes)
        {
            GameObject pivot = new GameObject(name);
            pivot.transform.SetParent(visualRoot, false);
            pivot.transform.localPosition = position;
            CreatePart("Trouser", PrimitiveType.Capsule, pivot.transform, new Vector3(0f, -0.17f, 0f), new Vector3(0.23f, 0.52f, 0.23f), Vector3.zero, uniform);
            CreatePart("Shoe", PrimitiveType.Cube, pivot.transform, new Vector3(0f, -0.46f, 0.07f), new Vector3(0.23f, 0.14f, 0.36f), Vector3.zero, shoes);
            return pivot.transform;
        }

        private Transform CreatePart(
            string objectName,
            PrimitiveType primitiveType,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Vector3 localEuler,
            Material material
        )
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = objectName;
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

            Material material = shader != null
                ? new Material(shader)
                : new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2f);
            }
            runtimeMaterials.Add(material);
            return material;
        }
    }
}
