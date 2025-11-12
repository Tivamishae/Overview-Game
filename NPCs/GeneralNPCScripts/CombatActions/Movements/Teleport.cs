using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Teleport : Movement
{
    [Header("Teleport Settings")]
    [SerializeField] private float teleportRadius = 5f;
    [SerializeField] private int teleportSampleTries = 10;
    [SerializeField] private int teleportFallbackTries = 10;
    [SerializeField] private float navSampleMaxDistance = 5f;
    [SerializeField] private bool behindPlayer = true;
    [SerializeField] private bool useVFX = true;
    [SerializeField] private GameObject teleportVFXPrefab;
    [SerializeField] private AudioClip teleportSound;

    private Coroutine activeRoutine;

    public override string AnimationTrigger => "Teleport";

    public override IEnumerator PerformMovementRoutine(NPC npc)
    {
        // Step 1: Choose location
        bool success = ChooseLocation(npc, out Vector3 teleportPosition);
        if (success)
        {
            npc.PlayTrigger(AnimationTrigger);
            yield return new WaitForSeconds(0.25f); // short pre-teleport delay
            // Departure VFX
            if (useVFX && teleportVFXPrefab)
                Instantiate(teleportVFXPrefab, npc.transform.position, Quaternion.identity);

            // Teleport
            if (npc.agent && npc.agent.enabled)
                npc.agent.Warp(teleportPosition);
            else
                npc.transform.position = teleportPosition;

            // Arrival VFX
            if (useVFX && teleportVFXPrefab)
                Instantiate(teleportVFXPrefab, teleportPosition, Quaternion.identity);

            // Sound
            if (teleportSound)
                AudioSystem.Instance.PlayClipFollow(teleportSound, npc.transform, 1f);

            npc.FacePlayer();
        }

        yield return new WaitForSeconds(0.25f);
        npc.PlayBool("Moving", false);
        activeRoutine = null;
    }

public bool ChooseLocation(NPC npc, out Vector3 position)
{
    // always assign a default first
    position = npc.transform.position;

    Vector3 playerPos = npc.player.transform.position;
    Vector3 baseDir = behindPlayer ? -npc.player.transform.forward : npc.player.transform.forward;

    // Try back/front half first
    for (int i = 0; i < teleportSampleTries; i++)
    {
        float angle = Random.Range(-90f, 90f);
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;
        Vector3 candidate = playerPos + dir.normalized * teleportRadius;

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleMaxDistance, NavMesh.AllAreas))
        {
            position = hit.position;
            location = position;
            return true;
        }
    }

    // Fallback anywhere
    for (int i = 0; i < teleportFallbackTries; i++)
    {
        float angle = Random.Range(0f, 360f);
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        Vector3 candidate = playerPos + dir.normalized * teleportRadius;

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleMaxDistance, NavMesh.AllAreas))
        {
            position = hit.position;
            location = position;
            return true;
        }
    }

    // If all fails, return false, but position still has a value
    return false;
}

}
