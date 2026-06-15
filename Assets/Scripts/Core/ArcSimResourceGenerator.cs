using UnityEngine;
using UnityEditor;

public class ArcSimResourceGenerator
{
    [MenuItem("ArcSim/Generate All Resources")]
    public static void GenerateAll()
    {
        GenerateParticleMaterial();
        GenerateThermalMaterial();
        GenerateColorRampTexture();
        GenerateParticleTexture();
    }

    [MenuItem("ArcSim/Generate Particle Material")]
    public static void GenerateParticleMaterial()
    {
        Shader arcShader = Shader.Find("ArcSim/ArcGlow");
        if (arcShader == null)
        {
            Debug.LogError("ArcSim/ArcGlow shader not found. Ensure ArcGlowShader.shader is in the project.");
            return;
        }

        Material mat = new Material(arcShader);
        mat.SetColor("_GlowColor", new Color(1f, 0.6f, 0.2f, 1f));
        mat.SetColor("_CoreColor", new Color(1f, 1f, 1f, 1f));
        mat.SetFloat("_GlowIntensity", 3f);
        mat.SetFloat("_GlowRadius", 0.3f);
        mat.SetFloat("_TemperatureScale", 0.5f);

        AssetDatabase.CreateAsset(mat, "Assets/Resources/Materials/ArcGlowMaterial.mat");
        AssetDatabase.SaveAssets();
        Debug.Log("ArcGlowMaterial generated at Assets/Resources/Materials/");
    }

    [MenuItem("ArcSim/Generate Thermal Material")]
    public static void GenerateThermalMaterial()
    {
        Shader thermalShader = Shader.Find("ArcSim/ThermalOverlay");
        if (thermalShader == null)
        {
            Debug.LogError("ArcSim/ThermalOverlay shader not found.");
            return;
        }

        Material mat = new Material(thermalShader);
        mat.SetFloat("_MinTemp", 300f);
        mat.SetFloat("_MaxTemp", 20000f);
        mat.SetFloat("_Opacity", 0.6f);

        AssetDatabase.CreateAsset(mat, "Assets/Resources/Materials/ThermalOverlayMaterial.mat");
        AssetDatabase.SaveAssets();
        Debug.Log("ThermalOverlayMaterial generated at Assets/Resources/Materials/");
    }

    [MenuItem("ArcSim/Generate Color Ramp Texture")]
    public static void GenerateColorRampTexture()
    {
        int width = 256;
        Texture2D ramp = new Texture2D(width, 1, TextureFormat.RGBA32, false);
        ramp.wrapMode = TextureWrapMode.Clamp;
        ramp.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);
            Color c;
            if (t < 0.25f)
            {
                float s = t * 4f;
                c = new Color(0f, 0f, 0.5f + s * 0.5f, 1f);
            }
            else if (t < 0.5f)
            {
                float s = (t - 0.25f) * 4f;
                c = new Color(0f, s, 1f - s * 0.5f, 1f);
            }
            else if (t < 0.75f)
            {
                float s = (t - 0.5f) * 4f;
                c = new Color(s, 1f, 0f, 1f);
            }
            else
            {
                float s = (t - 0.75f) * 4f;
                c = new Color(1f, 1f - s * 0.5f, s, 1f);
            }
            ramp.SetPixel(x, 0, c);
        }
        ramp.Apply();

        byte[] png = ramp.EncodeToPNG();
        string path = "Assets/Resources/Textures/ThermalColorRamp.png";
        System.IO.File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path);
        AssetDatabase.SaveAssets();
        Debug.Log("ThermalColorRamp generated at Assets/Resources/Textures/");
    }

    [MenuItem("ArcSim/Generate Particle Texture")]
    public static void GenerateParticleTexture()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float center = size * 0.5f;
        float radius = center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;

                float alpha = Mathf.Exp(-dist * dist * 4f);
                float core = Mathf.Exp(-dist * dist * 16f);
                float r = 1f;
                float g = Mathf.Lerp(0.4f, 1f, core);
                float b = Mathf.Lerp(0.1f, 1f, core);

                tex.SetPixel(x, y, new Color(r, g, b, alpha));
            }
        }
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        string path = "Assets/Resources/Textures/ParticleGlow.png";
        System.IO.File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path);
        AssetDatabase.SaveAssets();
        Debug.Log("ParticleGlow texture generated at Assets/Resources/Textures/");
    }
}
