/* using UnityEngine;

public class Waypoint : QuestPart
{
    [Header("Waypoint Settings")]
    private GameObject waypointPrefab; // The object to spawn (e.g., marker, effect)
    public bool useWorldPosition = true;
    public Vector3 worldPosition;
    public Transform targetParent; // Optional parent to spawn under if not using world position
    public float completeDistance = 3f; // Distance to trigger completion

    private GameObject spawnedWaypoint;
    private Transform player;

    // Override description to include distance
    public override string Description
    {
        get
        {
            if (player != null && spawnedWaypoint != null)
            {
                float distance = Vector3.Distance(player.position, spawnedWaypoint.transform.position);
                return base.Description + $" ({distance:F1}m)";
            }
            return base.Description;
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        waypointPrefab = Resources.Load<GameObject>("2D/RuntimeCanvases/QuestWaypointWorldspace");

        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("WaypointPart: No object with tag 'Player' found!");
        }

        // Only spawn if this is the current quest
        if (parentQuest == QuestSystem.Instance.currentQuest && waypointPrefab != null)
        {
            SpawnWaypoint();
        }
    }

    private void Update()
    {
        if (!isActive || isCompleted || isFailed || player == null)
            return;

        //  If this quest is not the current quest, remove marker
        if (parentQuest != QuestSystem.Instance.currentQuest)
        {
            RemoveWaypoint();
            return;
        }

        // Ensure the waypoint exists if we�re current quest
        if (spawnedWaypoint == null && waypointPrefab != null)
        {
            SpawnWaypoint();
        }

        // Check player distance to waypoint
        if (spawnedWaypoint != null)
        {
            float distance = Vector3.Distance(player.position, spawnedWaypoint.transform.position);
            if (distance <= completeDistance)
            {
                Complete();
            }
        }
    }

    private void SpawnWaypoint()
    {
        if (useWorldPosition)
        {
            spawnedWaypoint = Instantiate(waypointPrefab, worldPosition, Quaternion.identity);
        }
        else if (targetParent != null)
        {
            spawnedWaypoint = Instantiate(waypointPrefab, targetParent.position, Quaternion.identity, targetParent);
        }
        else
        {
            Debug.LogWarning("WaypointPart: No spawn position set!");
        }
    }

    private void RemoveWaypoint()
    {
        if (spawnedWaypoint != null)
        {
            Destroy(spawnedWaypoint);
            spawnedWaypoint = null;
        }
    }

    protected override void OnCompleted()
    {
        base.OnCompleted();
        RemoveWaypoint();
    }

    protected override void OnFailed()
    {
        base.OnFailed();
        RemoveWaypoint();
    }
}
*/