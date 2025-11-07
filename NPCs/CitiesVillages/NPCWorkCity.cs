using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class NPCWorkCity : MonoBehaviour
{
    public bool npcReadyToBeWorking = true;
    public bool npcReadyToBreak = false;
    public List<Townsman> detectedTownsmen = new();
    public List<NPCWorkArea> detectedWorkAreas = new();
    public List<Townsman> wanderingTownsmen = new();

    private SphereCollider cityTrigger;

    private void Awake()
    {
        cityTrigger = GetComponent<SphereCollider>();
        cityTrigger.isTrigger = true;
    }

    private void Update()
    {
        if (npcReadyToBeWorking == true)
        {
            StartCoroutine(AssignWorkCooldown());
        }

        if (npcReadyToBreak)
        {
            StartCoroutine(NPCBreak());
        }
    }
    

    private IEnumerator AssignWorkCooldown()
    {
        foreach (Townsman NPC in detectedTownsmen)
        {
            if (NPC.Work == null)
            {
                wanderingTownsmen.Add(NPC);
            }
        }
        npcReadyToBeWorking = false;

        yield return null;

        if (wanderingTownsmen.Count > 0) { TryAssignWork(wanderingTownsmen[0]); }

        yield return new WaitForSeconds(60f);

        npcReadyToBeWorking = true;
    }

    private IEnumerator NPCBreak()
    {
        foreach (Townsman NPC in detectedTownsmen)
        {
            if (NPC.Work != null)
            {
                // 70% chance to lose job
                if (Random.value <= 0.7f)
                {
                    NPC.Work.hasAWorker = false;
                    NPC.Work = null;
                    wanderingTownsmen.Add(NPC);
                }
            }
        }
        npcReadyToBreak = false;

        yield return new WaitForSeconds(300f);
    }


    private void OnTriggerEnter(Collider other)
    {
        // Detect Townsman
        Townsman townsman = other.GetComponent<Townsman>();
        if (townsman != null && !detectedTownsmen.Contains(townsman))
        {
            detectedTownsmen.Add(townsman);
            Debug.Log($"Townsman entered: {townsman.name}");
        }

        // Detect NPCWorkArea
        NPCWorkArea workArea = other.GetComponent<NPCWorkArea>();
        if (workArea != null && !detectedWorkAreas.Contains(workArea))
        {
            detectedWorkAreas.Add(workArea);
            Debug.Log($"Work area entered: {workArea.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove Townsman
        Townsman townsman = other.GetComponent<Townsman>();
        if (townsman != null && detectedTownsmen.Contains(townsman))
        {
            detectedTownsmen.Remove(townsman);
            Debug.Log($"Townsman exited: {townsman.name}");
        }

        // Remove NPCWorkArea
        NPCWorkArea workArea = other.GetComponent<NPCWorkArea>();
        if (workArea != null && detectedWorkAreas.Contains(workArea))
        {
            detectedWorkAreas.Remove(workArea);
            Debug.Log($"Work area exited: {workArea.name}");
        }
    }

    private void TryAssignWork(Townsman npc)
    {
        foreach (var workArea in detectedWorkAreas)
        {
            if (workArea.hasAWorker == false)
            {
                npc.Work = workArea;
                workArea.hasAWorker = true;
                workArea.worker = npc.gameObject;
                wanderingTownsmen.Remove(npc);
                Debug.Log($"Assigned {npc.name} to {workArea.name}");
                break;
            }
        }
    }
}
