using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnable
{
    public GameObject prefab;
    public Vector2 scaleRange = new Vector2(0.9f, 1.1f);
    public int weight = 1;
    public int groupSizeMin = 1;
    public int groupSizeMax = 3;
}

public class NightEnemySpawner : MonoBehaviour
{
    public static NightEnemySpawner Instance { get; private set; }

    [Header("Spawner Settings")]
    public Transform player;
    public List<EnemySpawnable> spawnables;
    public int maxEnemies = 20;
    public float minSpawnDistance = 50f;
    public float maxSpawnDistance = 150f;
    public float minEnemySpacing = 10f;      // distance between spawned enemies

    [Header("Wave Settings")]
    public Vector2 waveDelayRange = new Vector2(10f, 20f);  // delay between waves
    public Vector2 waveSizeRange = new Vector2(3, 6);       // enemies per wave
    public Vector2 perSpawnDelay = new Vector2(0.3f, 1f);   // delay between each enemy spawn

    [Header("Respawn Settings")]
    public float respawnDelay = 20f;

    private List<SpawnedEnemy> activeEnemies = new();
    private int totalWeight;
    private bool wasDay = true;
    private bool spawningWave = false;

    private class SpawnedEnemy
    {
        public GameObject obj;
        public EnemySpawnable data;
        public float tooFarTimer;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var s in spawnables)
            totalWeight += Mathf.Max(1, s.weight);
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        bool isDay = TimeCycle.Instance != null && TimeCycle.Instance.isDay;

        if (isDay)
        {
            HandleDaytimeCleanup();
            wasDay = true;
            return;
        }

        if (wasDay && !isDay)
        {
            wasDay = false;
        }

        UpdateRespawns();
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // wait until it�s night
            yield return new WaitUntil(() => TimeCycle.Instance != null && !TimeCycle.Instance.isDay);

            // Only spawn waves if below cap and not already spawning
            if (!spawningWave && activeEnemies.Count < maxEnemies)
            {
                spawningWave = true;
                int waveSize = Mathf.RoundToInt(Random.Range(waveSizeRange.x, waveSizeRange.y));
                yield return StartCoroutine(SpawnWave(waveSize));
                spawningWave = false;
            }

            // wait a random delay between waves
            float delay = Random.Range(waveDelayRange.x, waveDelayRange.y);
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator SpawnWave(int waveSize)
    {
        for (int i = 0; i < waveSize; i++)
        {
            if (activeEnemies.Count >= maxEnemies) yield break;

            EnemySpawnable chosen = ChooseSpawnable();
            if (FindValidSpawnPoint(out Vector3 spawnPos))
            {
                GameObject enemyObj = MobPoolManager.Instance.GetFromPool(chosen.prefab);
                NPC enemy = enemyObj.GetComponent<NPC>();

                enemy.poolPrefabReference = chosen.prefab;
                enemy.ResetNPC(); // make sure it�s ready

                enemyObj.transform.position = spawnPos;
                enemyObj.transform.rotation = Quaternion.identity;

                float scale = Random.Range(chosen.scaleRange.x, chosen.scaleRange.y);
                enemyObj.transform.localScale = Vector3.one * scale;

                activeEnemies.Add(new SpawnedEnemy
                {
                    obj = enemyObj,
                    data = chosen,
                    tooFarTimer = 0f
                });

            }

            yield return new WaitForSeconds(Random.Range(perSpawnDelay.x, perSpawnDelay.y));
        }
    }

    private EnemySpawnable ChooseSpawnable()
    {
        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var s in spawnables)
        {
            cumulative += Mathf.Max(1, s.weight);
            if (roll < cumulative)
                return s;
        }
        return spawnables[0];
    }

    private bool FindValidSpawnPoint(out Vector3 position)
    {
        for (int i = 0; i < 15; i++)
        {
            Vector2 circle = Random.insideUnitCircle * (maxSpawnDistance - minSpawnDistance);
            circle = circle.normalized * Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 candidate = player.position + new Vector3(circle.x, 0, circle.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                // Check distance from player and other enemies
                if (Vector3.Distance(player.position, hit.position) < minSpawnDistance)
                    continue;

                bool tooCloseToOther = false;
                foreach (var e in activeEnemies)
                {
                    if (e.obj == null) continue;
                    if (Vector3.Distance(e.obj.transform.position, hit.position) < minEnemySpacing)
                    {
                        tooCloseToOther = true;
                        break;
                    }
                }

                if (!tooCloseToOther)
                {
                    position = hit.position;
                    return true;
                }
            }
        }

        position = Vector3.zero;
        return false;
    }

    private void UpdateRespawns()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = activeEnemies[i];
            if (enemy.obj == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(player.position, enemy.obj.transform.position);

            if (dist > maxSpawnDistance)
            {
                enemy.tooFarTimer += Time.deltaTime;

                if (enemy.tooFarTimer >= respawnDelay)
                {
                    if (FindValidSpawnPoint(out Vector3 newPos))
                    {
                        var agent = enemy.obj.GetComponent<NavMeshAgent>();
                        if (agent != null) agent.Warp(newPos);
                        else enemy.obj.transform.position = newPos;

                        enemy.tooFarTimer = 0f;

                        if (enemy.obj.TryGetComponent(out NPC e))
                        {
                            e.ResetNPC(); // ensures correct animation/state reset
                        }
                    }
                }
            }
            else
            {
                enemy.tooFarTimer = 0f;
            }
        }
    }

    private void HandleDaytimeCleanup()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = activeEnemies[i];
            if (enemy.obj != null)
            {
                MobPoolManager.Instance.ReturnToPool(enemy.data.prefab, enemy.obj);
            }
            activeEnemies.RemoveAt(i);
        }
    }
}
