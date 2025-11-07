using UnityEngine;

public class ArmMovements : MonoBehaviour
{
    public Animator ArmAnimator;

    public bool ActionInProgress = false;

    private void Start()
    {
        ArmAnimator.SetBool("isIdle", true);
    }

    public void ActionFinished()
    {
        ActionInProgress = false;
    }

    public void ActionStarted()
    {
        ActionInProgress = true;
    }

    public void IsIdle() {
        ArmAnimator.SetBool("isIdle", true);
        ArmAnimator.SetBool("isRunning", false);
    }

    public void IsWalking() {
        ArmAnimator.SetBool("isIdle", false);
        ArmAnimator.SetBool("isRunning", false);
    }

    public void IsRunning() {
        ArmAnimator.SetBool("isIdle", false);
        ArmAnimator.SetBool("isRunning", true);
    }

    public void EquipItem(bool action)
    {
        ArmAnimator.SetBool("isEquipped", action);
    }

    // --- Public methods to trigger animations ---
    public void Throwing()
    {
        if (ActionInProgress) return;
        ArmAnimator.SetTrigger("Throwing");
        ActionStarted();
    }

    public void Hitting()
    {
        if (ActionInProgress) return;
        ArmAnimator.SetTrigger("Hitting");
        ActionStarted();
    }

    public void HittingWithTool()
    {
        ItemRaycaster.Instance.PlayerHitting(PlayerStats.Instance.AttackDamage);
    }

    public void SwingSound()
    {
        AudioSystem.Instance.PlayClipFollow(AudioPreloader.Instance.GetClip("Sounds/PlayerSounds/Swing"), transform, 1f);
    }

    public void BlowgunSound()
    {
        AudioSystem.Instance.PlayClipFollow(AudioPreloader.Instance.GetClip("Sounds/PlayerSounds/Blowgun"), transform, 1f);
    }

    public void EatingSound()
    {
        AudioSystem.Instance.PlayClipFollow(AudioPreloader.Instance.GetClip("Sounds/PlayerSounds/Eating"), transform, 1f);
    }

    public void Casting()
    {
        if (ActionInProgress) return;
        ArmAnimator.SetTrigger("Casting");
        ActionStarted();
    }

    public void SpearHitting()
    {
        if (ActionInProgress) return;
        ArmAnimator.SetTrigger("SpearHitting");
        ActionStarted();
    }

    public void Eating()
    {
        if (ActionInProgress) return;
        ArmAnimator.SetTrigger("Eating");
        ActionStarted();
    }

    public void Pickup()
    {
        if (ActionInProgress) return;
        ArmAnimator.SetTrigger("Pickup");
        ActionStarted();
    }

    public void ConsumeItem()
    {
        ItemConsumption.Instance.ConsumeEquippedItem();
    }

    public void BlowgunShooting()
    {
        if (ActionInProgress) return;
        ArmAnimator.SetTrigger("BlowgunShooting");
        ActionStarted();
    }

    public void InstantiateProjectile()
    {
        GetComponent<ProjectileThrower>().Shooting();
        BlowgunShooting();
    }

    public void Throw() // Separate logic hook
    {
        GetComponent<ProjectileThrower>().Throwing();
        SwingSound();
    }

    public void EquippingItem(bool isEquipping)
    {
        if (isEquipping)
            Debug.Log("Equipping item.");
        else
            Debug.Log("Unequipping item.");
    }
}
