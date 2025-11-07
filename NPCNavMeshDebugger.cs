using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshAgentDebugger : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform player;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        bool canReachPlayer = NavMesh.SamplePosition(player.position, out NavMeshHit hit, 2f, NavMesh.AllAreas);

        Debug.Log(
            $"{name} | " +
            $"pathStatus: {agent.pathStatus} | " +
            $"pathPending: {agent.pathPending} | " +       // <--- NEW
            $"remaining: {agent.remainingDistance:F2} | " +
            $"velocity: {agent.velocity.magnitude:F2} | " +
            $"isStopped: {agent.isStopped} | " +
            $"hasPath: {agent.hasPath} | " +
            $"canReachPlayer: {canReachPlayer}"
        );
    }

    void OnDrawGizmos()
    {
        if (agent == null || agent.path == null) return;

        Gizmos.color = Color.red;
        var path = agent.path;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            Gizmos.DrawSphere(path.corners[i], 0.1f);
        }

        if (path.corners.Length > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(path.corners[path.corners.Length - 1], 0.2f);
        }
    }
}
