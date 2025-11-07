using UnityEngine;

[ExecuteInEditMode]
public class TerrainPainter : MonoBehaviour
{
    public Terrain terrain;

    [Header("Height Settings")]
    public float sandHeight = 20f;
    public float volcanoHeight = 120f; // volcano starts at this height

    [Header("Slope Settings")]
    public float slopeThreshold = 40f;
    public float transitionSlopeMin = 25f;
    public float transitionSlopeMax = 40f;

    [Header("Volcano Settings")]
    public float volcanoStrength = 0.5f; // blend factor based on height*slope
    public float volcanoSlopeThreshold = 20f; // how steep it must be for volcano texture

    [Header("Layer Indices")]
    public int sandLayer = 0;
    public int grassLayer = 1;
    public int transitionLayer = 2;
    public int rockLayer = 3;
    public int volcanoLayer = 4; // new volcano layer index

    [ContextMenu("Apply Texture Rules")]
    public void ApplyTextures()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();

        var data = terrain.terrainData;
        if (data.terrainLayers.Length < 5)
        {
            Debug.LogError("Please assign at least 5 Terrain Layers (sand, grass, transition, rock, volcano).");
            return;
        }

        int res = data.alphamapResolution;
        float[,,] splatmapData = new float[res, res, data.terrainLayers.Length];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float normX = (float)x / (res - 1);
                float normY = (float)y / (res - 1);

                float height = data.GetInterpolatedHeight(normX, normY);
                float slope = data.GetSteepness(normX, normY);

                float[] weights = new float[data.terrainLayers.Length];

                // Volcano texture based on height * slope
                float volcanoFactor = (height / volcanoHeight) * (slope / volcanoSlopeThreshold);

                if (volcanoFactor >= volcanoStrength)
                {
                    weights[volcanoLayer] = 1f;
                }
                else if (slope > slopeThreshold)
                {
                    weights[rockLayer] = 1f;
                }
                else if (height < sandHeight)
                {
                    weights[sandLayer] = 1f;
                }
                else if (slope >= transitionSlopeMin && slope <= transitionSlopeMax)
                {
                    weights[transitionLayer] = 1f;
                }
                else
                {
                    weights[grassLayer] = 1f;
                }

                // Normalize
                float total = 0f;
                foreach (float w in weights) total += w;
                for (int i = 0; i < weights.Length; i++)
                    weights[i] /= total;

                for (int i = 0; i < data.terrainLayers.Length; i++)
                    splatmapData[y, x, i] = weights[i];
            }
        }

        data.SetAlphamaps(0, 0, splatmapData);
        Debug.Log("Terrain painting complete (with volcano layer).");
    }
}
