using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnablePrefab
{
    public GameObject[] prefabs; // Array of prefabs to pick from
    public float density = 0.01f; // % of texture weight per texel
    public Vector2 sizeRange = new Vector2(0.8f, 1.2f);

    public Vector3 rotationOffset = Vector3.zero;
    public Vector3 positionOffset = Vector3.zero;
}

[System.Serializable]
public class TexturePrefabGroup
{
    public string name;
    public int textureIndex; // Match with terrain texture index
    public List<SpawnablePrefab> spawnables;
}

[ExecuteInEditMode]
public class TerrainObjectSpawner : MonoBehaviour
{
    public Terrain terrain;
    public List<TexturePrefabGroup> texturePrefabGroups;
    public Transform spawnedParent;

    [ContextMenu("Spawn Objects")]
    public void SpawnObjects()
    {
        if (terrain == null) terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("No terrain assigned or found.");
            return;
        }

        ClearSpawnedObjects();

        if (spawnedParent == null)
        {
            GameObject holder = new GameObject("SpawnedObjects");
            holder.transform.parent = this.transform;
            spawnedParent = holder.transform;
        }

        var data = terrain.terrainData;
        int res = data.alphamapResolution;
        float[,,] alphaMaps = data.GetAlphamaps(0, 0, res, res);

        foreach (var group in texturePrefabGroups)
        {
            int textureIndex = group.textureIndex;

            foreach (var spawnable in group.spawnables)
            {
                if (spawnable.prefabs == null || spawnable.prefabs.Length == 0)
                    continue;

                int placedCount = 0;

                for (int y = 0; y < res; y++)
                {
                    for (int x = 0; x < res; x++)
                    {
                        float textureWeight = alphaMaps[y, x, textureIndex];
                        float chance = textureWeight * spawnable.density;

                        if (Random.value < chance)
                        {
                            Vector3 basePos = new Vector3(
                                (float)x / res * data.size.x,
                                0,
                                (float)y / res * data.size.z
                            );

                            basePos.y = terrain.SampleHeight(basePos) + terrain.transform.position.y;

                            Vector3 finalPos = basePos + spawnable.positionOffset;
                            Quaternion finalRot = Quaternion.Euler(
                                spawnable.rotationOffset + new Vector3(0, Random.Range(0f, 360f), 0)
                            );

                            // Randomly select one prefab
                            GameObject prefab = spawnable.prefabs[Random.Range(0, spawnable.prefabs.Length)];
                            if (prefab == null) continue;

                            GameObject instance = Instantiate(prefab, finalPos, finalRot, spawnedParent);
                            float scale = Random.Range(spawnable.sizeRange.x, spawnable.sizeRange.y);
                            instance.transform.localScale = Vector3.one * scale;

                            //  Collision check (root + children, ignore self & terrain)
                            Collider[] ownColliders = instance.GetComponentsInChildren<Collider>(true);
                            HashSet<Collider> selfColliders = new HashSet<Collider>(ownColliders);

                            bool collides = false;
                            foreach (var col in ownColliders)
                            {
                                Collider[] hits = Physics.OverlapBox(
                                    col.bounds.center,
                                    col.bounds.extents,
                                    col.transform.rotation
                                );

                                foreach (var h in hits)
                                {
                                    if (h.isTrigger) continue;
                                    if (h.gameObject == terrain.gameObject) continue; // ignore terrain
                                    if (selfColliders.Contains(h)) continue;         // ignore self colliders

                                    // anything else = collision
                                    collides = true;
                                    break;
                                }

                                if (collides) break;
                            }

                            if (collides)
                            {
                                DestroyImmediate(instance); //  remove
                            }
                            else
                            {
                                placedCount++; //  keep
                            }
                        }
                    }
                }

                Debug.Log($"Placed {placedCount} instances from prefab group on texture '{group.name}'.");
            }
        }
    }

    [ContextMenu("Clear Spawned Objects")]
    public void ClearSpawnedObjects()
    {
        if (spawnedParent == null)
        {
            Debug.Log("No spawned objects to clear.");
            return;
        }

        int count = spawnedParent.childCount;

        for (int i = spawnedParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(spawnedParent.GetChild(i).gameObject);
        }

        Debug.Log($"Cleared {count} spawned objects.");
    }
}
