using UnityEngine;

public class NPCWorkArea : MonoBehaviour
{
    [Header("Offset relative to this object (world aligned, not rotated)")]
    public Vector3 localOffset = new Vector3(0, 0, 1f);

    public bool hasAWorker = false;
    public GameObject worker;

    [Header("Raycast settings")]
    public float raycastHeight = 5f;   // how far above the work area to start
    public float raycastDistance = 20f;
    public LayerMask groundMask = ~0;  // hit everything by default

    [Header("Debug")]
    public Vector3 finalGroundPosition;
    public bool hitSuccessful;

    void Start()
    {
        // Start position = world pos + world-up lift + raw offset
        Vector3 rayOrigin = transform.position + localOffset + Vector3.up * raycastHeight;

        Debug.DrawRay(rayOrigin, Vector3.down * raycastDistance, Color.yellow);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            finalGroundPosition = hit.point;
            hitSuccessful = true;
            // Optional: only log once
            // Debug.Log($"{name}: Found ground at {finalGroundPosition} (collider: {hit.collider.name})");
        }
        else
        {
            hitSuccessful = false;
            // Debug.LogWarning($"{name}: No hit. Origin was {rayOrigin}");
        }
    }

    public Vector3 GetStandingPosition() => finalGroundPosition;

    public Quaternion GetStandingRotation()
    {
        Vector3 dirToWorkbench = (transform.position - finalGroundPosition).normalized;
        dirToWorkbench.y = 0f;
        return dirToWorkbench == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(dirToWorkbench);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 rayOrigin = Application.isPlaying
            ? transform.position + localOffset + Vector3.up * raycastHeight
            : transform.position + localOffset + Vector3.up * raycastHeight;

        Gizmos.color = hitSuccessful ? Color.green : Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * raycastDistance);
        Gizmos.DrawWireSphere(finalGroundPosition, 0.1f);
    }
}
