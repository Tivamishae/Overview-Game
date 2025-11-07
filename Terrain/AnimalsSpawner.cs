using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[System.Serializable]
public class AnimalSpawnable
{
    public GameObject prefab;
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
    public int weight = 1;          // spawn weight (higher = more common)
    public int groupSizeMin = 1;    // min group size
    public int groupSizeMax = 3;    // max group size
}

public class AnimalsSpawner : MonoBehaviour
{
    public static AnimalsSpawner Instance { get; private set; }

    [Header("Spawner Settings")]
    public Transform player;
    public List<AnimalSpawnable> spawnables;
    public int maxAnimals = 30;
    public float minSpawnDistance = 30f;
    public float maxSpawnDistance = 150;

    [Header("Respawn Settings")]
    public float respawnDelay = 15f; // Seconds away before recycle

    private List<SpawnedAnimal> activeAnimals = new List<SpawnedAnimal>();
    private int totalWeight;

    private class SpawnedAnimal
    {
        public GameObject obj;
        public float tooFarTimer;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Precompute total weight
        foreach (var s in spawnables)
            totalWeight += Mathf.Max(1, s.weight);
    }

    private void Update()
    {
        MaintainPopulation();
        UpdateRespawns();
    }

    private void MaintainPopulation()
    {
        // Keep filling until we reach the cap
        while (activeAnimals.Count < maxAnimals)
        {
            AnimalSpawnable chosen = ChooseSpawnable();

            int groupSize = Random.Range(chosen.groupSizeMin, chosen.groupSizeMax + 1);
            for (int i = 0; i < groupSize; i++)
            {
                if (FindValidSpawnPoint(out Vector3 spawnPos))
                {
                    GameObject animal = Instantiate(chosen.prefab, spawnPos, Quaternion.identity);
                    float scale = Random.Range(chosen.scaleRange.x, chosen.scaleRange.y);
                    animal.transform.localScale = Vector3.one * scale;

                    activeAnimals.Add(new SpawnedAnimal { obj = animal, tooFarTimer = 0f });

                    if (activeAnimals.Count >= maxAnimals) return;
                }
            }
        }
    }

    private AnimalSpawnable ChooseSpawnable()
    {
        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var s in spawnables)
        {
            cumulative += Mathf.Max(1, s.weight);
            if (roll < cumulative)
                return s;
        }

        return spawnables[0]; // fallback
    }

    private bool FindValidSpawnPoint(out Vector3 position)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 candidate = player.position + new Vector3(circle.x, 0, circle.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }
        }

        position = Vector3.zero;
        return false;
    }

    private void UpdateRespawns()
    {
        for (int i = activeAnimals.Count - 1; i >= 0; i--)
        {
            var animal = activeAnimals[i];
            if (animal.obj == null)
            {
                activeAnimals.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(player.position, animal.obj.transform.position);

            if (dist > maxSpawnDistance)
            {
                animal.tooFarTimer += Time.deltaTime;

                if (animal.tooFarTimer >= respawnDelay)
                {
                    if (FindValidSpawnPoint(out Vector3 newPos))
                    {
                        var agent = animal.obj.GetComponent<NavMeshAgent>();
                        if (agent != null) agent.Warp(newPos);
                        else animal.obj.transform.position = newPos;

                        animal.tooFarTimer = 0f;
                    }
                }
            }
            else
            {
                animal.tooFarTimer = 0f;
            }
        }
    }
}
