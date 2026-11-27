using System.Collections.Generic;
using UnityEngine;

namespace PigeonRooftopRush
{


    public static class MaterialFactory
    {
        private static Shader _flat;
        private static Shader _transparent;
        private static readonly Dictionary<int, Material> Cache = new Dictionary<int, Material>();

        private static Shader FlatShader
        {
            get
            {
                if (_flat == null)
                {
                    _flat = Shader.Find("Pombo/Flat");
                    if (_flat == null)
                        Debug.LogError("Shader 'Pombo/Flat' nao encontrado (verifique Always Included Shaders).");
                }
                return _flat;
            }
        }

        private static Shader TransparentShader
        {
            get
            {
                if (_transparent == null)
                {
                    _transparent = Shader.Find("Pombo/Transparent");
                    if (_transparent == null)
                        Debug.LogError("Shader 'Pombo/Transparent' nao encontrado (verifique Always Included Shaders).");
                }
                return _transparent;
            }
        }

        public static Material Flat(Color color)
        {
            int key = color.GetHashCode() ^ 0x51ED;
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = new Material(FlatShader);
            mat.SetColor("_BaseColor", color.linear);
            Cache[key] = mat;
            return mat;
        }

        public static Material Transparent(Color color)
        {
            int key = color.GetHashCode() ^ 0x7A11;
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = new Material(TransparentShader);

            Color c = color.linear;
            c.a = color.a;
            mat.SetColor("_BaseColor", c);
            mat.renderQueue = 3000;
            Cache[key] = mat;
            return mat;
        }
    }
}
