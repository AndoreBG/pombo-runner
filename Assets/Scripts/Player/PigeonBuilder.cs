using UnityEngine;

namespace PigeonRooftopRush
{


    public static class PigeonBuilder
    {
        private static readonly Color BodyColor = new Color(0.62f, 0.67f, 0.74f);
        private static readonly Color WingColor = new Color(0.47f, 0.52f, 0.60f);
        private static readonly Color BeakColor = new Color(0.96f, 0.58f, 0.16f);
        private static readonly Color EyeColor = new Color(0.08f, 0.08f, 0.10f);

        public static PigeonFlightController Build(Vector3 position)
        {
            var root = new GameObject("Pigeon");
            root.transform.position = position;

            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = 1f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var col = root.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.55f;
            col.center = new Vector3(0f, 0.05f, 0.05f);

            root.AddComponent<CollisionHandler>();
            var flight = root.AddComponent<PigeonFlightController>();

            BuildModel(root.transform, flight);
            return flight;
        }

        private static void BuildModel(Transform root, PigeonFlightController flight)
        {
            var model = new GameObject("Model").transform;
            model.SetParent(root, false);


            Part(model, PrimitiveType.Capsule, BodyColor,
                new Vector3(0f, 0f, 0f), new Vector3(0.55f, 0.45f, 0.5f),
                Quaternion.Euler(90f, 0f, 0f));


            Part(model, PrimitiveType.Sphere, BodyColor,
                new Vector3(0f, 0.17f, 0.40f), new Vector3(0.34f, 0.34f, 0.34f),
                Quaternion.identity);


            Part(model, PrimitiveType.Cube, BeakColor,
                new Vector3(0f, 0.13f, 0.60f), new Vector3(0.09f, 0.07f, 0.20f),
                Quaternion.identity);


            Part(model, PrimitiveType.Sphere, EyeColor,
                new Vector3(-0.10f, 0.23f, 0.50f), new Vector3(0.06f, 0.06f, 0.06f),
                Quaternion.identity);
            Part(model, PrimitiveType.Sphere, EyeColor,
                new Vector3(0.10f, 0.23f, 0.50f), new Vector3(0.06f, 0.06f, 0.06f),
                Quaternion.identity);


            Part(model, PrimitiveType.Cube, WingColor,
                new Vector3(0f, 0.05f, -0.54f), new Vector3(0.30f, 0.05f, 0.36f),
                Quaternion.Euler(-12f, 0f, 0f));


            var wingL = new GameObject("WingLeft").transform;
            wingL.SetParent(model, false);
            wingL.localPosition = new Vector3(-0.18f, 0.10f, 0.02f);
            Part(wingL, PrimitiveType.Cube, WingColor,
                new Vector3(-0.42f, 0f, 0f), new Vector3(0.85f, 0.05f, 0.50f),
                Quaternion.identity);

            var wingR = new GameObject("WingRight").transform;
            wingR.SetParent(model, false);
            wingR.localPosition = new Vector3(0.18f, 0.10f, 0.02f);
            Part(wingR, PrimitiveType.Cube, WingColor,
                new Vector3(0.42f, 0f, 0f), new Vector3(0.85f, 0.05f, 0.50f),
                Quaternion.identity);

            flight.ModelRoot = model;
            flight.WingLeft = wingL;
            flight.WingRight = wingR;
        }

        private static GameObject Part(
            Transform parent, PrimitiveType type, Color color,
            Vector3 localPos, Vector3 localScale, Quaternion localRot)
        {
            var go = GameObject.CreatePrimitive(type);
            var t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            t.localRotation = localRot;
            t.localScale = localScale;

            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = MaterialFactory.Flat(color);

            go.name = type.ToString();
            return go;
        }
    }
}
