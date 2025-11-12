using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class SummonEnemy : Attack
{
    [Header("Summon Settings")]
    [SerializeField] private GameObject summonPrefab;
    [SerializeField] private int maxActiveSummons = 3;
    [SerializeField] private float summonRadius = 5f;
    [SerializeField] private float spawnDistanceFromPlayer = 4f;
    [SerializeField] private float navSampleMaxDistance = 2f;
    [SerializeField] private bool spawnAroundPlayer = false;
    [SerializeField] private bool spawnOnNavMesh = true;

    [Header("Effects & Audio")]
    [SerializeField] private GameObject summonVFXPrefab;
    [SerializeField] private AudioClip summonSound;

    // Internal tracking
    private readonly List<GameObject> activeSummons = new();

    public override string AnimationTrigger => "Summon";

    protected override void PerformAttack(NPC npc)
    {
        // Step 1: Clean up nulls
        activeSummons.RemoveAll(s => s == null);

        // Step 2: Check if we can summon more
        if (summonPrefab == null)
        {
            Debug.LogWarning($"{npc.name} tried to summon, but no prefab assigned!");
            return;
        }

        if (activeSummons.Count >= maxActiveSummons)
        {
            // Optional behavior: refresh existing summons instead
            RefreshSummons(npc);
            return;
        }

        // Step 3: Pick spawn location
        Vector3 spawnPos = GetSpawnPosition(npc);

        // Step 4: Validate against NavMesh
        if (spawnOnNavMesh && NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, navSampleMaxDistance, NavMesh.AllAreas))
            spawnPos = hit.position;

        // Step 5: Instantiate summon
        GameObject summon = Instantiate(summonPrefab, spawnPos, Quaternion.identity);

        // Step 6: Orient toward player
        if (npc.player)
        {
            Vector3 lookAt = npc.player.transform.position;
            lookAt.y = summon.transform.position.y;
            summon.transform.LookAt(lookAt);
        }

        // Step 7: Add to active list
        activeSummons.Add(summon);

        // Step 8: Play VFX & SFX
        if (summonVFXPrefab)
        {
            GameObject fx = Instantiate(summonVFXPrefab, spawnPos, Quaternion.identity);
            Object.Destroy(fx, 3f);
        }

        if (summonSound)
            AudioSystem.Instance.PlayClipFollow(summonSound, npc.transform, 1f);
    }

    private Vector3 GetSpawnPosition(NPC npc)
    {
        Vector3 basePos = npc.transform.position;

        if (npc.player != null)
        {
            if (spawnAroundPlayer)
            {
                // Around player, random circle
                Vector3 randomDir = Random.insideUnitCircle.normalized * spawnDistanceFromPlayer;
                return npc.player.transform.position + new Vector3(randomDir.x, 0f, randomDir.y);
            }
            else
            {
                // In front of player
                Vector3 dir = (npc.player.transform.position - basePos).normalized;
                return basePos + dir * spawnDistanceFromPlayer;
            }
        }

        // Default: random around caster
        Vector3 random = Random.insideUnitSphere * summonRadius;
        random.y = 0f;
        return basePos + random;
    }

    private void RefreshSummons(NPC npc)
    {
    }

    public override void StopPerformAttack()
    {
        base.StopPerformAttack();
        // No need to cancel anything, summons are independent
    }
}
