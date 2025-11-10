using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class NPCWorkCity : MonoBehaviour
{
    public bool villagerReadyToBeWorking = true;
    public bool villagerReadyToBreak = false;
    public List<Townsman> detectedVillagers = new();
    public List<NPCWorkArea> detectedWorkAreas = new();
    public List<Townsman> wanderingVillagers = new();

    private SphereCollider cityTrigger;

    private void Awake()
    {
        cityTrigger = GetComponent<SphereCollider>();
        cityTrigger.isTrigger = true;
    }

    private void Update()
    {
        if (villagerReadyToBeWorking == true)
        {
            StartCoroutine(AssignWorkCooldown());
        }

        if (villagerReadyToBreak)
        {
            StartCoroutine(NPCBreak());
        }
    }
    

    private IEnumerator AssignWorkCooldown()
    {
        foreach (Townsman NPC in detectedVillagers)
        {
            if (NPC.Work == null)
            {
                wanderingVillagers.Add(NPC);
            }
        }
        villagerReadyToBeWorking = false;

        yield return null;

        if (wanderingVillagers.Count > 0) { TryAssignWork(wanderingVillagers[0]); }

        yield return new WaitForSeconds(60f);

        villagerReadyToBeWorking = true;
    }

    private IEnumerator NPCBreak()
    {
        foreach (Townsman NPC in detectedVillagers)
        {
            if (NPC.Work != null)
            {
                // 70% chance to lose job
                if (Random.value <= 0.7f)
                {
                    NPC.Work.hasAWorker = false;
                    NPC.Work = null;
                    wanderingVillagers.Add(NPC);
                }
            }
        }
        villagerReadyToBreak = false;

        yield return new WaitForSeconds(300f);
    }


    private void OnTriggerEnter(Collider other)
    {
        // Detect Villager
        Townsman villager = other.GetComponent<Townsman>();
        if (villager != null && !detectedVillagers.Contains(villager))
        {
            detectedVillagers.Add(villager);
            Debug.Log($"Villager entered: {villager.name}");
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
        // Remove Villager
        Townsman villager = other.GetComponent<Townsman>();
        if (villager != null && detectedVillagers.Contains(villager))
        {
            detectedVillagers.Remove(villager);
            Debug.Log($"Villager exited: {villager.name}");
        }

        // Remove NPCWorkArea
        NPCWorkArea workArea = other.GetComponent<NPCWorkArea>();
        if (workArea != null && detectedWorkAreas.Contains(workArea))
        {
            detectedWorkAreas.Remove(workArea);
            Debug.Log($"Work area exited: {workArea.name}");
        }
    }

    private void TryAssignWork(Townsman villager)
    {
        foreach (var workArea in detectedWorkAreas)
        {
            if (workArea.hasAWorker == false)
            {
                villager.Work = workArea;
                workArea.hasAWorker = true;
                workArea.worker = villager.gameObject;
                wanderingVillagers.Remove(villager);
                Debug.Log($"Assigned {villager.npcName} to {workArea.name}");
                break;
            }
        }
    }
}
