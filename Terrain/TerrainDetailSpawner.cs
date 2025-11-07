using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DetailTextureMapping
{
    public int detailPrototypeIndex;
    public List<int> textureLayerIndices = new(); // multiple texture layers
    public float density = 1f; // Higher = more per cell
}

[ExecuteInEditMode]
public class TerrainDetailSpawner : MonoBehaviour
{
    public Terrain terrain;
    public List<DetailTextureMapping> mappings = new();

    [ContextMenu("Apply Details by Texture")]
    public void ApplyDetails()
    {
        if (terrain == null) terrain = GetComponent<Terrain>();
        var data = terrain.terrainData;

        // Optional: Different randomness every time
        Random.InitState(System.DateTime.Now.Millisecond);

        int detailRes = data.detailResolution;
        int alphaRes = data.alphamapResolution;
        float coordRatio = (float)alphaRes / detailRes;

        float[,,] alphaMaps = data.GetAlphamaps(0, 0, alphaRes, alphaRes);

        foreach (var map in mappings)
        {
            if (map.detailPrototypeIndex >= data.detailPrototypes.Length)
            {
                Debug.LogWarning($"Invalid detail prototype index: {map.detailPrototypeIndex}");
                continue;
            }

            int[,] detailLayer = new int[detailRes, detailRes];
            int placed = 0;

            for (int y = 0; y < detailRes; y++)
            {
                for (int x = 0; x < detailRes; x++)
                {
                    int alphaX = Mathf.FloorToInt(x * coordRatio);
                    int alphaY = Mathf.FloorToInt(y * coordRatio);

                    float totalWeight = 0f;

                    // Sum contributions from all specified texture layers
                    foreach (int layerIndex in map.textureLayerIndices)
                    {
                        if (layerIndex >= 0 && layerIndex < data.alphamapLayers)
                            totalWeight += alphaMaps[alphaY, alphaX, layerIndex];
                    }

                    totalWeight = Mathf.Clamp01(totalWeight); // keep between 0–1

                    // Calculate spawn count (scaled by density and max cap)
                    float rawSpawn = totalWeight * map.density * 16f; // 16 = Unity’s per-cell max
                    int spawnCount = Mathf.Clamp(Mathf.FloorToInt(rawSpawn + Random.value), 0, 16);

                    detailLayer[y, x] = spawnCount;
                    placed += spawnCount;
                }
            }

            data.SetDetailLayer(0, 0, map.detailPrototypeIndex, detailLayer);
            Debug.Log($" Painted {placed} instances of prototype {map.detailPrototypeIndex} across {map.textureLayerIndices.Count} texture layers.");
        }

#if UNITY_EDITOR
        SceneView.RepaintAll();
        EditorUtility.SetDirty(terrain);
#endif

        Debug.Log(" Detail painting completed.");
    }

    [ContextMenu("Clear All Details")]
    public void ClearDetails()
    {
        if (terrain == null) terrain = GetComponent<Terrain>();
        var data = terrain.terrainData;

        for (int i = 0; i < data.detailPrototypes.Length; i++)
        {
            int[,] clear = new int[data.detailResolution, data.detailResolution];
            data.SetDetailLayer(0, 0, i, clear);
        }

#if UNITY_EDITOR
        SceneView.RepaintAll();
        EditorUtility.SetDirty(terrain);
#endif

        Debug.Log(" All detail layers cleared.");
    }
}
