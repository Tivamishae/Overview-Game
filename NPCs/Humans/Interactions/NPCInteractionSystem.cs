using UnityEngine;

public class NPCInteractionSystem : MonoBehaviour
{
    public static NPCInteractionSystem Instance { get; private set; }

    [Header("UI References")]
    public GameObject npcQuoteDisplay;
    // Future: public GameObject skillPointDisplay;
    public GameObject npcTradingDisplay;

    [Header("Settings")]
    public float interactionMaxDistance = 5f;

    public InteractableNPC currentNPC;
    private Transform playerTransform;
    public event System.Action<InteractableNPC> OnNPCInteracted;

    [Header("Interaction Lock Settings")]
    private float interactionCooldown = 0.3f;
    private bool interactionLocked = false;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Assume player has tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Hide UI on start
        if (npcQuoteDisplay != null)
            npcQuoteDisplay.SetActive(false);

        if (npcTradingDisplay != null)
            npcTradingDisplay.SetActive(false);
    }

    void Update()
    {
        if (currentNPC != null && playerTransform != null)
        {
            float distance = Vector3.Distance(currentNPC.transform.position, playerTransform.position);
            if (distance > interactionMaxDistance)
            {
                EndInteraction();
            }

            // Allow conversation progression while active
            if (Input.GetKeyDown(KeyCode.E))
            {
                HumanInteraction interaction = currentNPC.GetComponent<HumanInteraction>();
                if (interaction is Conversation)
                {
                    interaction.Progress();
                }
            }
        }
    }

    public void StartInteraction(InteractableNPC npc)
    {
        if (interactionLocked || npc == null || npc == currentNPC)
            return;

        EndInteraction(); // Ends previous one if needed
        currentNPC = npc;
        OnNPCInteracted?.Invoke(npc);

        // Mark NPC as being interacted with
        npc.SetInteractionState(true);

        HumanInteraction interaction = npc.GetComponent<HumanInteraction>();

        if (interaction is Conversation)
        {
            if (npcQuoteDisplay != null)
                npcQuoteDisplay.SetActive(true);
        }

        npc.Interact();
    }




    public void EndInteraction()
    {
        if (currentNPC != null)
        {
            currentNPC.SetInteractionState(false); // Reset interaction flag
        }

        if (npcQuoteDisplay != null)
            npcQuoteDisplay.SetActive(false);

        currentNPC = null;
        interactionLocked = true; // Lock begins now (on end)
        StartCoroutine(UnlockInteractionAfterDelay());
    }


    public bool IsInteractionLocked()
    {
        return interactionLocked;
    }


    public bool IsInteracting()
    {
        return currentNPC != null;
    }

    private System.Collections.IEnumerator UnlockInteractionAfterDelay()
    {
        yield return new WaitForSeconds(interactionCooldown);
        interactionLocked = false;
    }
}
