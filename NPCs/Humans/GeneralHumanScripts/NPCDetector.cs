using System.Collections.Generic;
using UnityEngine;

public class NPCDetector : MonoBehaviour
{
    public List<GuardNPC> nearbyGuards = new List<GuardNPC>();

    private void OnTriggerEnter(Collider other)
    {
        GuardNPC guard = other.GetComponent<GuardNPC>();
        if (guard != null && !nearbyGuards.Contains(guard))
        {
            nearbyGuards.Add(guard);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GuardNPC guard = other.GetComponent<GuardNPC>();
        if (guard != null && nearbyGuards.Contains(guard))
        {
            nearbyGuards.Remove(guard);
        }
    }
}
