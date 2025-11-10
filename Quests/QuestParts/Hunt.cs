/* using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Hunt : QuestPart
{
    [Header("Hunt Settings")]
    public string targetCreatureName;   // Match by CreatureMover.Name
    public int requiredKills = 1;       // How many kills required
    private int currentKills = 0;
    private CreatureMover targetCreature;

    [Header("Marker Settings")]
    private GameObject markerPrefab;        // The world marker prefab
    private GameObject spawnedMarker;       // Instance of the marker

    // Expose marker for CompassBar
    public GameObject SpawnedMarker => spawnedMarker;

    // Override Description so UI always shows progress
    public override string Description
    {
        get
        {
            return base.Description + $" ({currentKills}/{requiredKills})";
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        currentKills = 0;

        // Load marker prefab (same as Waypoint)
        markerPrefab = Resources.Load<GameObject>("2D/RuntimeCanvases/QuestWaypointWorldspace");

        // Find first target
        targetCreature = FindClosestTarget();

        if (targetCreature != null)
        {
            SpawnMarker(targetCreature.transform);
            Debug.Log($"Hunt quest started: Kill {requiredKills}x {targetCreatureName}");
        }
        else
        {
            Debug.LogWarning($"No creature of type '{targetCreatureName}' found in scene.");
        }
    }

    private void Update()
    {
        if (!isActive || isCompleted || isFailed) return;

        //  Only allow marker if this quest is the current quest
        if (parentQuest != QuestSystem.Instance.currentQuest)
        {
            RemoveMarker();
            return;
        }

        if (targetCreature == null || targetCreature.Health <= 0f)
        {
            // Creature was killed
            if (targetCreature != null && targetCreature.Health <= 0f)
            {
                RegisterKill();
            }

            if (currentKills < requiredKills)
            {
                targetCreature = FindClosestTarget();

                if (targetCreature != null)
                {
                    UpdateMarker(targetCreature.transform.position);
                }
                else
                {
                    // Hide marker until another target is found
                    RemoveMarker();
                    Debug.Log($"Hunt quest: No {targetCreatureName} available. Waiting...");
                }
            }
        }
        else
        {
            // Keep marker following moving target
            if (spawnedMarker != null && targetCreature != null)
            {
                spawnedMarker.transform.position = targetCreature.transform.position;
            }
        }
    }


    private void RegisterKill()
    {
        currentKills++;
        Debug.Log($"Killed {targetCreatureName} ({currentKills}/{requiredKills})");

        // Refresh quest UI
        parentQuest?.CheckQuestProgress();

        if (currentKills >= requiredKills)
        {
            Complete();
        }
    }

    private CreatureMover FindClosestTarget()
    {
        CreatureMover[] creatures = GameObject.FindObjectsOfType<CreatureMover>();

        // Filter by alive + name match
        List<CreatureMover> valid = creatures
            .Where(c => !c.isDead && c.Name == targetCreatureName)
            .ToList();

        if (valid.Count == 0)
            return null;

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            return valid[0]; // fallback: just pick first

        return valid
            .OrderBy(c => Vector3.Distance(c.transform.position, player.position))
            .FirstOrDefault();
    }

    private void SpawnMarker(Transform parent)
    {
        if (markerPrefab != null && parent != null)
        {
            RemoveMarker();
            spawnedMarker = Instantiate(markerPrefab, parent);
            spawnedMarker.transform.localPosition = Vector3.up * 2f; // offset above creature
            spawnedMarker.transform.localRotation = Quaternion.identity;
            spawnedMarker.transform.localScale = Vector3.one; // force neutral scale
        }
    }



    private void UpdateMarker(Vector3 newPosition)
    {
        if (spawnedMarker != null)
        {
            spawnedMarker.transform.position = newPosition;
        }
        else
        {
            SpawnMarker(targetCreature.transform);
        }
    }

    private void RemoveMarker()
    {
        if (spawnedMarker != null)
        {
            Destroy(spawnedMarker);
            spawnedMarker = null;
        }
    }

    protected override void OnCompleted()
    {
        base.OnCompleted();
        RemoveMarker();
        Debug.Log($"Hunt quest completed: {requiredKills}x {targetCreatureName} killed.");
    }

    protected override void OnFailed()
    {
        base.OnFailed();
        RemoveMarker();
    }
}
*/