using UnityEngine;
using TMPro;

public class ItemRaycaster : MonoBehaviour
{
    public static ItemRaycaster Instance { get; private set; }

    public Camera playerCamera;
    public float rayDistance = 3f;

    public InteractableObject CurrentItem;
    public NPC currentNPC;
    public ArmMovements armMovement;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (!UISystem.Instance.Inventory.activeSelf && StorageObjectSystem.Instance.currentStorage == null)
        {
            CheckForTarget();

            // --- Handle ITEM interaction ---
            if (CurrentItem != null)
            {
                if (Input.GetKeyDown(KeyCode.E) && CurrentItem is not MineableObject)
                {
                    CurrentItem.Interact();
                    CurrentItem = null;
                    UISystem.Instance.ObjectName.SetActive(false);
                    AudioClip hitSound = Resources.Load<AudioClip>("Sounds/InteractableObjects/PickupItem");
                    AudioSystem.Instance.PlayClipAtPoint(hitSound, playerCamera.transform.position, 1f);
                    armMovement.Pickup();
                }
            }

            if (currentNPC != null)
            {
                if (Input.GetKeyDown(KeyCode.E) && currentNPC is Villager villager)
                {
                    if (!NPCInteractionSystem.Instance.IsInteracting() &&
                        !NPCInteractionSystem.Instance.IsInteractionLocked())
                    {
                        NPCInteractionSystem.Instance.StartInteraction(villager);
                    }
                }
            }
        }
        else
        {
            UISystem.Instance.ObjectName.SetActive(false);
        }
    }

    void CheckForTarget()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            InteractableObject interactableObject = hit.collider.GetComponent<InteractableObject>();
            if (interactableObject != null)
            {
                CurrentItem = interactableObject;
                UISystem.Instance.ObjectName.GetComponent<TextMeshProUGUI>().text = interactableObject.Name;
                UISystem.Instance.ObjectName.SetActive(true);
                return;
            }

            NPC npc = hit.collider.GetComponent<NPC>();
            if (npc != null)
            {
                currentNPC = npc;
                UISystem.Instance.ObjectName.GetComponent<TextMeshProUGUI>().text = npc.npcName;
                UISystem.Instance.ObjectName.SetActive(true);
                return;
            }
        }

        // Nothing hit
        currentNPC = null;
        CurrentItem = null;
        UISystem.Instance.ObjectName.SetActive(false);
    }


    public void PlayerHitting(float damage)
    {
        if (currentNPC != null)
        {
            currentNPC.takeDamage(damage);
        }

        if (CurrentItem != null && CurrentItem is MineableObject)
        {
            CurrentItem.Interact();
        }
    }


}
