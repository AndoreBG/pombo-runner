using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // WorldBuilder — cenário base e globals dos shaders próprios:
    //   - chão estático (asfalto) fora do worldRoot (não se move)
    //   - sombra blob do jogador
    //   - uniforms globais: luz, ambiente, névoa e cor do céu
    // A cor do céu (clear color) usa EXATAMENTE o mesmo valor bruto da névoa
    // para casar pixel a pixel no horizonte.
    // ---------------------------------------------------------------------
    public static class WorldBuilder
    {
        // valor bruto (sem conversão gamma) — igual ao clear color da câmera
        public static readonly Color SkyColor = new Color(0.45f, 0.62f, 0.78f, 1f);

        public static void SetupGlobals(Light sun)
        {
            if (sun != null)
            {
                // direção LUMINOSA (superficie -> luz) = -forward da luz
                Shader.SetGlobalVector("_PomboLightDir", -sun.transform.forward);
                Color lit = sun.color * sun.intensity;
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
            ground.transform.localScale = new Vector3(40f, 1f, 20f); // 400 x 200

            var collider = ground.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = MaterialFactory.Flat(new Color(0.21f, 0.22f, 0.24f));
        }

        /// <summary>Sombra blob seguindo o jogador (chão, y fixo).</summary>
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
