using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class EquipmentItem : MonoBehaviour
{
    public int itemID;

    [Header("Equip Transform Offset")]
    public Vector3 localPositionOffset;
    public Vector3 localRotationOffset;

    [Header("Mouse Actions")]
    public ArmActionType leftClickAction;
    public ArmActionType rightClickAction;

    private UnityAction leftAction;
    private UnityAction rightAction;

    private ArmMovements armMovements;
    public float Damage;

    public enum ArmActionType
    {
        None,
        Hitting,
        Throwing,
        SpearHitting,
        Casting,
        Eating,
        BlowgunShooting
    }

    private void Start()
    {
        armMovements = GameObject.Find("First Person Controller/First Person Camera/Arm").GetComponent<ArmMovements>();

        leftAction = GetActionForType(leftClickAction);
        rightAction = GetActionForType(rightClickAction);
    }


    private UnityAction GetActionForType(ArmActionType type)
    {
        switch (type)
        {
            case ArmActionType.Hitting: return armMovements.Hitting;
            case ArmActionType.Throwing: return armMovements.Throwing;
            case ArmActionType.SpearHitting: return armMovements.SpearHitting;
            case ArmActionType.Casting: return armMovements.Casting;
            case ArmActionType.Eating: return armMovements.Eating;
            case ArmActionType.BlowgunShooting: return armMovements.BlowgunShooting;
            default: return null;
        }
    }

    public void PerformLeftClick()
    {
        leftAction?.Invoke();
    }

    public void PerformRightClick()
    {
        rightAction?.Invoke();
    }
}
