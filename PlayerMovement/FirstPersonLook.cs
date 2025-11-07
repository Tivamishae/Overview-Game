using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;
    public bool isFrozen = false;
    Vector2 frozenRotation;

    void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        bool inventoryOpen = UISystem.Instance != null && UISystem.Instance.Inventory.activeSelf;
        bool TradingDisplayIsOpen = NPCInteractionSystem.Instance.npcTradingDisplay.activeSelf;
        bool storageDisplayIsOpen = StorageObjectSystem.Instance.storageDisplay.activeSelf;
        bool mapScreenIsOpen = MapSystem.Instance.Map.activeSelf;
        bool questScreenIsOpen = UISystem.Instance.QuestScreen.activeSelf;

        if (inventoryOpen || TradingDisplayIsOpen || storageDisplayIsOpen || questScreenIsOpen || mapScreenIsOpen)
        {
            if (!isFrozen)
            {
                // Freeze rotation when inventory just opened
                frozenRotation = velocity;
                isFrozen = true;
            }

            // Apply frozen rotation every frame
            transform.localRotation = Quaternion.AngleAxis(-frozenRotation.y, Vector3.right);
            character.localRotation = Quaternion.AngleAxis(frozenRotation.x, Vector3.up);
            return;
        }
        else
        {
            if (isFrozen)
            {
                // Unfreeze and resume from where it left off
                velocity = frozenRotation;
                isFrozen = false;
            }
        }

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Regular camera rotation
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }
}
