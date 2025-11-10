using System;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    [Header("General Movement")]
    public float speed;
    public Rigidbody rb;
    public List<Func<float>> speedOverrides = new List<Func<float>>();

    [Header("Running")]
    public bool canRun = true;
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;
    public bool IsRunning { get; private set; }

    [Header("Jumping")]
    public float jumpStrength = 2;
    public GroundCheck groundCheck;
    public event Action Jumped;

    [Header("Crouching")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchSpeed = 2;
    public Transform headToLower;
    public float crouchYHeadPosition = 1;
    public CapsuleCollider colliderToLower;
    private float? defaultHeadYLocalPosition;
    private float? defaultColliderHeight;
    public bool IsCrouched { get; private set; }
    public event Action CrouchStart, CrouchEnd;

    public bool ScreensOpen;

    [Header("Arm Actions")]
    public GameObject Arm;
    private ArmMovements armMovements;
    public EquipmentItem equippedItem;

    void Reset()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!groundCheck) groundCheck = GetComponentInChildren<GroundCheck>();
        if (!colliderToLower) colliderToLower = GetComponentInChildren<CapsuleCollider>();
        if (!headToLower) headToLower = GetComponentInChildren<Camera>().transform;
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (Arm != null)
            armMovements = Arm.GetComponent<ArmMovements>();
    }

    void Update()
    {
        bool inventoryOpen = UISystem.Instance != null && UISystem.Instance.Inventory.activeSelf;
        bool TradingDisplayIsOpen = NPCInteractionSystem.Instance.npcTradingDisplay.activeSelf;
        bool storageDisplayIsOpen = StorageObjectSystem.Instance.storageDisplay.activeSelf;
        bool mapScreenIsOpen = MapSystem.Instance.Map.activeSelf;
        bool questScreenIsOpen = UISystem.Instance.QuestScreen.activeSelf;

        speed = PlayerStats.Instance.Speed;
        runSpeed = PlayerStats.Instance.Speed * 1.5f;

        ScreensOpen = !(inventoryOpen || TradingDisplayIsOpen ||  storageDisplayIsOpen || questScreenIsOpen || mapScreenIsOpen);

        if (ScreensOpen)
        {
            HandleRunning();
            MovePlayer();
            HandleJumping();
            HandleCrouching();
            HandleMouseActions();
        }
    }

    void HandleRunning()
    {
        IsRunning = canRun && Input.GetKey(runningKey);
    }

    void MovePlayer()
    {
        float currentSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            currentSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 move = transform.rotation * new Vector3(input.x * currentSpeed, rb.linearVelocity.y, input.y * currentSpeed);
        rb.linearVelocity = move;

        // --- Movement State ---
        bool isMoving = input.sqrMagnitude > 0.01f;   // has movement input
        bool isGrounded = groundCheck == null || groundCheck.isGrounded;

        if (!isMoving && isGrounded)
        {
            armMovements?.IsIdle();
        }
        else
        {

            if (IsRunning)
                armMovements?.IsRunning();
            else
                armMovements?.IsWalking();
        }
    }


    void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && (!groundCheck || groundCheck.isGrounded))
        {
            rb.AddForce(Vector3.up * 100 * jumpStrength);
            Jumped?.Invoke();
        }
    }

    void HandleMouseActions()
    {
        if (HotbarLogic.Instance?.currentEquipped != null)
        {
            EquipmentItem item = HotbarLogic.Instance.currentEquipped.GetComponent<EquipmentItem>();
            equippedItem = item;

            if (item != null && ScreensOpen)
            {
                if (Input.GetMouseButton(0)) // While holding left click
                    item.PerformLeftClick();

                if (Input.GetMouseButton(1)) // While holding right click
                    item.PerformRightClick();
            }
        }
        else
        {
            // Default arm actions if no item is equipped
            if (Input.GetMouseButton(0))
                armMovements?.Hitting();
        }
    }




    void HandleCrouching()
    {
        if (Input.GetKey(crouchKey))
        {
            if (!defaultHeadYLocalPosition.HasValue)
                defaultHeadYLocalPosition = headToLower.localPosition.y;
            if (!defaultColliderHeight.HasValue)
                defaultColliderHeight = colliderToLower.height;

            headToLower.localPosition = new Vector3(
                headToLower.localPosition.x,
                crouchYHeadPosition,
                headToLower.localPosition.z
            );

            float loweringAmount = defaultHeadYLocalPosition.Value - crouchYHeadPosition;
            colliderToLower.height = Mathf.Max(defaultColliderHeight.Value - loweringAmount, 0);
            colliderToLower.center = Vector3.up * colliderToLower.height * 0.5f;

            if (!IsCrouched)
            {
                IsCrouched = true;
                SetSpeedOverrideActive(true);
                CrouchStart?.Invoke();
            }
        }
        else
        {
            if (IsCrouched)
            {
                headToLower.localPosition = new Vector3(
                    headToLower.localPosition.x,
                    defaultHeadYLocalPosition.Value,
                    headToLower.localPosition.z
                );

                colliderToLower.height = defaultColliderHeight.Value;
                colliderToLower.center = Vector3.up * colliderToLower.height * 0.5f;

                IsCrouched = false;
                SetSpeedOverrideActive(false);
                CrouchEnd?.Invoke();
            }
        }
    }

    void SetSpeedOverrideActive(bool state)
    {
        if (state)
        {
            if (!speedOverrides.Contains(CrouchSpeedOverride))
                speedOverrides.Add(CrouchSpeedOverride);
        }
        else
        {
            if (speedOverrides.Contains(CrouchSpeedOverride))
                speedOverrides.Remove(CrouchSpeedOverride);
        }
    }

    float CrouchSpeedOverride() => crouchSpeed;
}
