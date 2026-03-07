using UnityEditor;
using UnityEngine;

public class NoiseTextureGenerator : EditorWindow
{
    int size = 256;
    float scale = 20f;
    int seed = 0;
    bool seamless = true;

    [MenuItem("Tools/Generate Noise Texture")]
    static void Init() => GetWindow<NoiseTextureGenerator>("Noise Generator");

    void OnGUI()
    {
        size = EditorGUILayout.IntField("Resolution", size);
        scale = EditorGUILayout.FloatField("Scale", scale);
        seed = EditorGUILayout.IntField("Seed", seed);
        seamless = EditorGUILayout.Toggle("Seamless", seamless);

        if (GUILayout.Button("Generate & Save")) Generate();
    }

    void Generate()
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.R8, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = seamless ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

        var rng = new System.Random(seed);
        float offsetX = rng.Next(0, 1000);
        float offsetY = rng.Next(0, 1000);

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float u = (float)x / size;
                float v = (float)y / size;
                float noise = seamless 
                    ? PerlinSeamless(u * scale + offsetX, v * scale + offsetY, scale)
                    : Mathf.PerlinNoise(u * scale + offsetX, v * scale + offsetY);
                tex.SetPixel(x, y, new Color(noise, noise, noise, 1));
            }
        }
        tex.Apply();

        string path = EditorUtility.SaveFilePanelInProject("Save Noise", "AsteroidNoise", "png", "");
        if (!string.IsNullOrEmpty(path)) {
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();
        }
    }

    float PerlinSeamless(float x, float y, float scale)
    {
        float nx = x / scale, ny = y / scale;
        float a = Mathf.PerlinNoise(nx, ny);
        float b = Mathf.PerlinNoise(nx + 1, ny);
        float c = Mathf.PerlinNoise(nx, ny + 1);
        float d = Mathf.PerlinNoise(nx + 1, ny + 1);
        
        float ux = x - Mathf.Floor(x), uy = y - Mathf.Floor(y);
        return Mathf.Lerp( Mathf.Lerp(a, b, ux), Mathf.Lerp(c, d, ux), uy);
    }
}
