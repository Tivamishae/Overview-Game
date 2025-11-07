using UnityEngine;
using TMPro;

public class ItemRaycaster : MonoBehaviour
{
    public static ItemRaycaster Instance { get; private set; }

    public Camera playerCamera;
    public float rayDistance = 3f;

    public InteractableObject CurrentItem;
    public InteractableNPC CurrentNPC;
    public GeneralBossScript CurrentBoss;
    public CreatureMover CurrentCreature; // <-- ADD THIS at the top
    public Enemy CurrentEnemy;
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

            // --- Handle NPC interaction ---
            if (CurrentNPC != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!NPCInteractionSystem.Instance.IsInteracting() &&
                        !NPCInteractionSystem.Instance.IsInteractionLocked())
                    {
                        NPCInteractionSystem.Instance.StartInteraction(CurrentNPC);
                    }
                }
            }

            // --- Handle BOSS interaction (attack only, not E) ---
            // You can extend this later if needed
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
            // --- Check for NPC ---
            InteractableNPC npc = hit.collider.GetComponent<InteractableNPC>();
            if (npc != null)
            {
                CurrentNPC = npc;
                CurrentCreature = null;
                CurrentItem = null;
                CurrentBoss = null;
                CurrentEnemy = null;
                UISystem.Instance.ObjectName.GetComponent<TextMeshProUGUI>().text = "Talk";
                UISystem.Instance.ObjectName.SetActive(true);
                return;
            }

            // --- Check for Boss ---
            GeneralBossScript boss = hit.collider.GetComponent<GeneralBossScript>();
            if (boss != null)
            {
                CurrentBoss = boss;
                CurrentCreature = null;
                CurrentItem = null;
                CurrentNPC = null;
                CurrentEnemy = null;
                UISystem.Instance.ObjectName.GetComponent<TextMeshProUGUI>().text = "Boss";
                UISystem.Instance.ObjectName.SetActive(true);
                return;
            }

            // --- Check for CreatureMover ---
            CreatureMover creature = hit.collider.GetComponent<CreatureMover>();
            if (creature != null)
            {
                CurrentCreature = creature;
                CurrentItem = null;
                CurrentNPC = null;
                CurrentBoss = null;
                CurrentEnemy = null;
                UISystem.Instance.ObjectName.GetComponent<TextMeshProUGUI>().text = creature.Name;
                UISystem.Instance.ObjectName.SetActive(true);
                return;
            }

            // ---  NEW: Check for Enemy ---
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                CurrentEnemy = enemy;
                CurrentCreature = null;
                CurrentItem = null;
                CurrentNPC = null;
                CurrentBoss = null;
                UISystem.Instance.ObjectName.GetComponent<TextMeshProUGUI>().text = enemy.Name;
                UISystem.Instance.ObjectName.SetActive(true);
                return;
            }

            // --- Check for Item ---
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                CurrentItem = interactable;
                CurrentCreature = null;
                CurrentNPC = null;
                CurrentBoss = null;
                CurrentEnemy = null;
                UISystem.Instance.ObjectName.GetComponent<TextMeshProUGUI>().text = interactable.Name;
                UISystem.Instance.ObjectName.SetActive(true);
                return;
            }
        }

        // Nothing hit
        CurrentItem = null;
        CurrentCreature = null;
        CurrentNPC = null;
        CurrentBoss = null;
        CurrentEnemy = null;
        UISystem.Instance.ObjectName.SetActive(false);
    }


    public void PlayerHitting(float damage)
    {
        if (CurrentNPC != null)
        {
            CurrentNPC.OnPlayerHit(damage);
        }

        if (CurrentBoss != null)
        {
            CurrentBoss.TakeDamage(damage);
        }

        if (CurrentCreature != null)
        {
            CurrentCreature.TakeDamage(damage);
        }

        if (CurrentEnemy != null)
        {
            CurrentEnemy.TakeDamage(damage);
        }

        if (CurrentItem != null && CurrentItem is MineableObject)
        {
            CurrentItem.Interact();
        }
    }


}
