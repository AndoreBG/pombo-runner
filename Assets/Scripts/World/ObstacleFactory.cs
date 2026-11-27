using UnityEngine;

namespace PigeonRooftopRush
{


    public static class ObstacleFactory
    {
        private static readonly string[] ObstacleKeys =
        {
            "building", "antenna", "wire", "billboard", "tree"
        };

        public static string[] ObstaclePoolKeys => ObstacleKeys;

        private static readonly Color[] WallColors =
        {
            new Color(0.55f, 0.58f, 0.66f),
            new Color(0.66f, 0.60f, 0.55f),
            new Color(0.52f, 0.62f, 0.60f),
            new Color(0.63f, 0.56f, 0.62f),
            new Color(0.58f, 0.63f, 0.70f)
        };

        private static readonly Color[] DecorColors =
        {
            new Color(0.48f, 0.52f, 0.60f),
            new Color(0.58f, 0.53f, 0.49f),
            new Color(0.47f, 0.56f, 0.55f),
            new Color(0.55f, 0.50f, 0.55f),
            new Color(0.51f, 0.56f, 0.63f)
        };


        public static GameObject CreateBuilding()
        {
            var root = NewRoot("Obstacle_Building");
            float w = Random.Range(3f, 4.2f);
            float h = Random.Range(11f, 16f);
            float d = Random.Range(3f, 4.2f);
            Color wall = Pick(WallColors);

            Cube(root.transform, new Vector3(w, h, d),
                new Vector3(0f, h * 0.5f, 0f), MaterialFactory.Flat(wall));
            Cube(root.transform, new Vector3(w * 0.6f, 0.6f, d * 0.6f),
                new Vector3(0f, h + 0.3f, 0f), MaterialFactory.Flat(Darker(wall)));

            AddBox(root, new Vector3(w, h, d), new Vector3(0f, h * 0.5f, 0f));
            AddMarker(root);
            AddBlobShadow(root.transform, w * 1.15f, d * 1.15f);
            return root;
        }

        public static GameObject CreateAntenna()
        {
            var root = NewRoot("Obstacle_Antenna");
            float h = Random.Range(9f, 13f);

            Cube(root.transform, new Vector3(1.6f, 1f, 1.6f),
                new Vector3(0f, 0.5f, 0f), MaterialFactory.Flat(new Color(0.4f, 0.42f, 0.46f)));
            Cylinder(root.transform, new Vector3(0.28f, h * 0.5f, 0.28f),
                new Vector3(0f, h * 0.5f + 1f, 0f), MaterialFactory.Flat(new Color(0.75f, 0.76f, 0.78f)));
            Sphere(root.transform, new Vector3(0.34f, 0.34f, 0.34f),
                new Vector3(0f, h + 1.1f, 0f), MaterialFactory.Flat(new Color(0.95f, 0.25f, 0.22f)));


            AddBox(root, new Vector3(0.7f, h + 1.4f, 0.7f), new Vector3(0f, (h + 1.4f) * 0.5f, 0f));
            AddBox(root, new Vector3(1.6f, 1f, 1.6f), new Vector3(0f, 0.5f, 0f));
            AddMarker(root);
            AddBlobShadow(root.transform, 1.6f, 1.6f);
            return root;
        }

        public static GameObject CreateWire()
        {
            var root = NewRoot("Obstacle_Wire");
            float y = Random.Range(4f, 16f);


            Cube(root.transform, new Vector3(17f, 0.5f, 0.4f),
                new Vector3(0f, y, 0f), MaterialFactory.Flat(new Color(0.14f, 0.14f, 0.16f)));

            Cube(root.transform, new Vector3(0.35f, y, 0.35f),
                new Vector3(-8.4f, y * 0.5f, 0f), MaterialFactory.Flat(new Color(0.35f, 0.3f, 0.26f)));
            Cube(root.transform, new Vector3(0.35f, y, 0.35f),
                new Vector3(8.4f, y * 0.5f, 0f), MaterialFactory.Flat(new Color(0.35f, 0.3f, 0.26f)));

            AddBox(root, new Vector3(17f, 0.5f, 0.5f), new Vector3(0f, y, 0f));
            AddMarker(root);
            return root;
        }

        public static GameObject CreateBillboard()
        {
            var root = NewRoot("Obstacle_Billboard");
            float y = Random.Range(5.5f, 13f);
            Color face = Pick(WallColors);

            Cube(root.transform, new Vector3(6.5f, 2.6f, 0.6f),
                new Vector3(0f, y, 0f), MaterialFactory.Flat(face));
            Cube(root.transform, new Vector3(6.9f, 3.0f, 0.35f),
                new Vector3(0f, y, 0.35f), MaterialFactory.Flat(Darker(face)));

            Cube(root.transform, new Vector3(0.3f, y, 0.3f),
                new Vector3(-2.4f, y * 0.5f, 0.4f), MaterialFactory.Flat(new Color(0.33f, 0.35f, 0.38f)));
            Cube(root.transform, new Vector3(0.3f, y, 0.3f),
                new Vector3(2.4f, y * 0.5f, 0.4f), MaterialFactory.Flat(new Color(0.33f, 0.35f, 0.38f)));

            AddBox(root, new Vector3(6.9f, 3.0f, 0.9f), new Vector3(0f, y, 0.15f));
            AddMarker(root);
            AddBlobShadow(root.transform, 6.5f, 1.6f);
            return root;
        }

        public static GameObject CreateTree()
        {
            var root = NewRoot("Obstacle_Tree");

            Cylinder(root.transform, new Vector3(0.5f, 1.6f, 0.5f),
                new Vector3(0f, 1.6f, 0f), MaterialFactory.Flat(new Color(0.42f, 0.3f, 0.2f)));
            Sphere(root.transform, new Vector3(3.4f, 3.2f, 3.4f),
                new Vector3(0f, 4.6f, 0f), MaterialFactory.Flat(new Color(0.28f, 0.55f, 0.3f)));

            AddBox(root, new Vector3(3.0f, 5.4f, 3.0f), new Vector3(0f, 3.0f, 0f));
            AddMarker(root);
            AddBlobShadow(root.transform, 3.6f, 3.6f);
            return root;
        }


        public static GameObject CreateWind(WindType type)
        {
            bool ascending = type == WindType.Ascending;
            var root = NewRoot(ascending ? "Wind_Ascending" : "Wind_Turbulence");

            Color boxColor = ascending
                ? new Color(0.24f, 0.78f, 1f, 0.16f)
                : new Color(1f, 0.32f, 0.32f, 0.16f);
            Color stripeColor = ascending
                ? new Color(0.55f, 0.9f, 1f, 0.55f)
                : new Color(1f, 0.55f, 0.55f, 0.55f);

            Cube(root.transform, new Vector3(10f, 18f, 6f),
                new Vector3(0f, 9f, 0f), MaterialFactory.Transparent(boxColor));

            const int stripeCount = 6;
            var stripes = new Transform[stripeCount];
            Material stripeMat = MaterialFactory.Transparent(stripeColor);
            for (int i = 0; i < stripeCount; i++)
            {
                var s = Cube(root.transform,
                    new Vector3(0.35f, Random.Range(1.8f, 3f), 0.35f),
                    new Vector3(Random.Range(-4f, 4f), Random.Range(0.5f, 17.5f), Random.Range(-2.4f, 2.4f)),
                    stripeMat);
                stripes[i] = s.transform;
            }

            var box = root.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(10f, 18f, 6f);
            box.center = new Vector3(0f, 9f, 0f);

            var zone = root.AddComponent<WindZone>();
            zone.Setup(type, stripes);
            return root;
        }


        public static GameObject CreateDecorBuilding()
        {
            var root = NewRoot("Decor_Building");
            float w = Random.Range(4f, 8f);
            float h = Random.Range(6f, 26f);
            float d = Random.Range(4f, 8f);
            Color wall = Pick(DecorColors);

            Cube(root.transform, new Vector3(w, h, d),
                new Vector3(0f, h * 0.5f, 0f), MaterialFactory.Flat(wall));
            Cube(root.transform, new Vector3(w * 0.55f, 1.2f, d * 0.55f),
                new Vector3(0f, h + 0.6f, 0f), MaterialFactory.Flat(Darker(wall)));

            AddBlobShadow(root.transform, w * 1.1f, d * 1.1f);
            return root;
        }

        public static GameObject CreateRoadStripe()
        {
            var root = NewRoot("Road_Stripe");
            Cube(root.transform, new Vector3(0.35f, 0.05f, 3f),
                new Vector3(0f, 0.04f, 0f),
                MaterialFactory.Flat(new Color(0.92f, 0.9f, 0.75f)));
            return root;
        }


        private static GameObject NewRoot(string name)
        {
            return new GameObject(name);
        }

        private static GameObject Cube(Transform parent, Vector3 scale, Vector3 pos, Material mat)
        {
            return Part(parent, PrimitiveType.Cube, scale, pos, Quaternion.identity, mat);
        }

        private static GameObject Sphere(Transform parent, Vector3 scale, Vector3 pos, Material mat)
        {
            return Part(parent, PrimitiveType.Sphere, scale, pos, Quaternion.identity, mat);
        }

        private static GameObject Cylinder(Transform parent, Vector3 scale, Vector3 pos, Material mat)
        {
            return Part(parent, PrimitiveType.Cylinder, scale, pos, Quaternion.identity, mat);
        }

        private static GameObject Part(
            Transform parent, PrimitiveType type, Vector3 scale, Vector3 pos,
            Quaternion rot, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            go.transform.localScale = scale;

            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = mat;
            return go;
        }

        private static void AddBox(GameObject root, Vector3 size, Vector3 center)
        {
            var box = root.AddComponent<BoxCollider>();
            box.size = size;
            box.center = center;
        }

        private static void AddMarker(GameObject root)
        {
            root.AddComponent<ObstacleMarker>();
        }

        private static void AddBlobShadow(Transform parent, float sizeX, float sizeZ)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "BlobShadow";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(sizeX, sizeZ, 1f);

            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = MaterialFactory.Transparent(new Color(0f, 0f, 0f, 0.16f));
        }

        private static Color Pick(Color[] palette)
        {
            return palette[Random.Range(0, palette.Length)];
        }

        private static Color Darker(Color c)
        {
            return new Color(c.r * 0.75f, c.g * 0.75f, c.b * 0.75f);
        }
    }
}
