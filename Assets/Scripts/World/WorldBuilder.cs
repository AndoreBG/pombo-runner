using UnityEngine;

namespace PigeonRooftopRush
{


    public static class WorldBuilder
    {

        public static readonly Color SkyColor = new Color(0.45f, 0.62f, 0.78f, 1f);

        public static void SetupGlobals(Light sun)
        {
            if (sun != null)
            {

                Shader.SetGlobalVector("_PomboLightDir", -sun.transform.forward);


                Color lit = sun.color * (sun.intensity * 0.5f);
                Shader.SetGlobalColor("_PomboLightColor",
                    new Color(lit.r, lit.g, lit.b, 1f));
            }
            else
            {
                Shader.SetGlobalVector("_PomboLightDir", new Vector3(0.4f, 0.8f, 0.3f));
                Shader.SetGlobalColor("_PomboLightColor", Color.white);
            }

            Shader.SetGlobalColor("_PomboAmbient", new Color(0.34f, 0.35f, 0.38f, 1f));
            Shader.SetGlobalColor("_PomboFogColor", SkyColor);
            Shader.SetGlobalVector("_PomboFogParams", new Vector4(30f, 68f, 0f, 0f));
        }

        public static void SetupCamera(Camera cam)
        {
            if (cam == null) return;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = SkyColor;
        }

        public static Transform BuildWorldRoot()
        {
            var go = new GameObject("WorldRoot");
            return go.transform;
        }

        public static void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, 0f, 60f);
            ground.transform.localScale = new Vector3(40f, 1f, 20f);

            var collider = ground.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = MaterialFactory.Flat(new Color(0.21f, 0.22f, 0.24f));
        }


        public static Transform BuildBlobShadow(out Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "PlayerBlobShadow";
            go.transform.position = new Vector3(0f, 0.03f, 0f);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(1f, 1f, 1f);

            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            material = MaterialFactory.Transparent(new Color(0f, 0f, 0f, 0.25f));
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = material;

            return go.transform;
        }
    }
}
