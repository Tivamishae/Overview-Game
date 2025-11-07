using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class SummonBoss : InteractableObject
{
    [Header("Boss Prefab Settings")]
    public GameObject bossPrefab;        // Assign in Inspector
    public float summonRadius = 5f;

    [Header("Teleport Settings")]
    public Transform playerArenaSpawnPoint; // Assign in Inspector (where player should go for the fight)

    private GameObject currentBoss;

    public Transform player;

    void Awake()
    {
        Name = "Summon Boss"; // This name appears in UI via ItemRaycaster


        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found with tag 'Player'.");
            return;
        }
    }

    public override void Interact()
    {
        SummonTheBoss();
    }

    public void SummonTheBoss()
    {
        // Prevent summoning if boss already exists
        if (BossSystem.Instance != null && BossSystem.Instance.IsBossActive())
        {
            Debug.Log("A boss is already active. Cannot summon another.");
            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogWarning("Boss prefab not assigned.");
            return;
        }

        // --- Save player return point ---
        if (BossSystem.Instance != null)
        {
            BossSystem.Instance.GiveReturnPoint(player.position);
            Debug.Log("Saved player return point: " + player.position);
        }

        // --- Teleport player to arena spawn point ---
        if (playerArenaSpawnPoint != null)
        {
            player.position = playerArenaSpawnPoint.position;
            Debug.Log("Teleported player to arena spawn: " + playerArenaSpawnPoint.position);
        }
        else
        {
            Debug.LogWarning("Player arena spawn point not assigned in inspector.");
        }
        StartCoroutine(BossSpawnDelay());
    }

    private IEnumerator BossSpawnDelay()
    {
        yield return null;

        // --- Generate random NavMesh position for boss spawn ---
        Vector3 randomDirection = Random.insideUnitSphere * summonRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, summonRadius, NavMesh.AllAreas))
        {
            currentBoss = Instantiate(bossPrefab, hit.position, Quaternion.identity);

            // Register with BossSystem
            GeneralBossScript bossScript = currentBoss.GetComponent<GeneralBossScript>();
            if (bossScript != null && BossSystem.Instance != null)
            {
                BossSystem.Instance.RegisterBoss(bossScript);
                Debug.Log("Boss summoned and registered at: " + hit.position);
            }
            else
            {
                Debug.LogWarning("Summoned boss does not have GeneralBossScript or BossSystem is missing.");
            }
        }
        else
        {
            Debug.LogWarning("No valid NavMesh position found within radius.");
        }
    }
}
