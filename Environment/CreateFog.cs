using UnityEngine;

/// <summary>
/// Generates a 3D Perlin noise texture for use in volumetric fog or clouds.
/// Assigns it automatically to a target material property (like "_FogNoise").
/// </summary>
[ExecuteAlways]
public class CreateFog : MonoBehaviour
{
    [Header("Noise Settings")]
    [Range(16, 256)] public int resolution = 64;
    [Range(0.1f, 10f)] public float frequency = 3f;
    [Range(0f, 1f)] public float contrast = 0.5f;
    public bool seamless = true;
    public int seed = 0;

    [Header("Output")]
    public Material targetMaterial;
    public string textureProperty = "_FogNoise";

    private Texture3D noiseTex;

    void OnEnable()
    {
        GenerateNoise();
        ApplyToMaterial();
    }

    void OnValidate()
    {
        GenerateNoise();
        ApplyToMaterial();
    }

    void ApplyToMaterial()
    {
        if (targetMaterial && noiseTex)
            targetMaterial.SetTexture(textureProperty, noiseTex);
    }

    void GenerateNoise()
    {
        if (resolution <= 0) return;

        Random.InitState(seed);
        noiseTex = new Texture3D(resolution, resolution, resolution, TextureFormat.RFloat, false);
        noiseTex.wrapMode = TextureWrapMode.Repeat;

        Color[] colors = new Color[resolution * resolution * resolution];

        for (int z = 0; z < resolution; z++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // Normalized coordinates
                    float nx = (float)x / resolution;
                    float ny = (float)y / resolution;
                    float nz = (float)z / resolution;

                    if (seamless)
                    {
                        // Smooth looping noise using sine interpolation trick
                        float p = Mathf.PerlinNoise((nx + ny) * frequency, (nz + ny) * frequency);
                        float q = Mathf.PerlinNoise((nx - ny) * frequency, (nz - ny) * frequency);
                        float r = Mathf.PerlinNoise((nx + nz) * frequency, (ny - nz) * frequency);
                        float s = Mathf.PerlinNoise((nx - nz) * frequency, (ny + nz) * frequency);
                        float val = (p + q + r + s) * 0.25f;
                        val = Mathf.Pow(val, contrast * 2f);
                        colors[x + y * resolution + z * resolution * resolution] = new Color(val, 0, 0, 1);
                    }
                    else
                    {
                        // Simple 3D Perlin sampling
                        float val = Mathf.PerlinNoise(nx * frequency, ny * frequency);
                        val = Mathf.Lerp(val, Mathf.PerlinNoise(nz * frequency, nx * frequency), 0.5f);
                        val = Mathf.Pow(val, contrast * 2f);
                        colors[x + y * resolution + z * resolution * resolution] = new Color(val, 0, 0, 1);
                    }
                }
            }
        }

        noiseTex.SetPixels(colors);
        noiseTex.Apply();
    }
}
