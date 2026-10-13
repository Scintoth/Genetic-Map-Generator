using UnityEngine;
using UnityEngine.Rendering;

namespace GeneticMapGenerator.Unity.Rendering
{
    /// <summary>
    /// Creates runtime materials for whichever render pipeline is active (URP or built-in), so the
    /// adapter needs no authored assets. For player builds, add the shaders used here to
    /// Project Settings ▸ Graphics ▸ Always Included Shaders.
    /// </summary>
    public static class MaterialFactory
    {
        private const string UrpLit = "Universal Render Pipeline/Lit";
        private const string BuiltInLit = "Standard";
        private const string Fallback = "Sprites/Default";

        private static bool IsUniversalPipeline =>
            GraphicsSettings.currentRenderPipeline != null
            && GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("Universal");

        public static Material CreateOpaque(Color colour, Texture? texture = null, float smoothness = 0.1f)
        {
            var material = new Material(FindLitShader()) { color = colour };
            if (texture != null)
            {
                material.mainTexture = texture;
                SetIfPresent(material, "_BaseMap", texture);
            }

            SetIfPresent(material, "_BaseColor", colour);
            SetIfPresent(material, "_Glossiness", smoothness);
            SetIfPresent(material, "_Smoothness", smoothness);
            return material;
        }

        public static Material CreateTransparent(Color colour)
        {
            var material = CreateOpaque(colour, smoothness: 0.8f);

            if (IsUniversalPipeline)
            {
                material.SetFloat("_Surface", 1);
                material.SetFloat("_Blend", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                material.SetFloat("_Mode", 3);
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            material.SetOverrideTag("RenderType", "Transparent");
            SetIfPresent(material, "_SrcBlend", (float)BlendMode.One);
            SetIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetIfPresent(material, "_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        private static Shader FindLitShader() =>
            Shader.Find(IsUniversalPipeline ? UrpLit : BuiltInLit) ?? Shader.Find(Fallback);

        private static void SetIfPresent(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        private static void SetIfPresent(Material material, string property, Color value)
        {
            if (material.HasProperty(property))
            {
                material.SetColor(property, value);
            }
        }

        private static void SetIfPresent(Material material, string property, Texture value)
        {
            if (material.HasProperty(property))
            {
                material.SetTexture(property, value);
            }
        }
    }
}
