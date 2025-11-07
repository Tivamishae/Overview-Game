using UnityEngine;
using UnityEngine.AI;

public class JungleHusk : Enemy
{
    [Header("Attack Settings")]
    public GameObject rightHandBone;

    [Header("Audio")]
    private AudioClip angrySoundClip;
    private AudioClip stepClip;
    private AudioClip meleeGruntClip;
    private AudioClip runningGruntClip;

    [Header("Audio Logic")]
    private int stepsUntilGrunt = 25;

    private void Start()
    {
        if (AudioPreloader.Instance)
        {
            angrySoundClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/JungleHusk/AngrySound");
            stepClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/JungleHusk/Step");
            meleeGruntClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/JungleHusk/MeleeGrunt");
            runningGruntClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/JungleHusk/RunningGrunt");
        }
    }

    #region Combat

    protected override void EnterCombat()
    {
        base.EnterCombat();
        PlayBool("isAngry", true);
    }

    protected override void EndCombat()
    {
        base.EndCombat();
        PlayBool("isAngry", false);
    }

    #endregion

    #region Melee Attack

    private void MeleeGrunt()
    {
        AudioSystem.Instance.PlayClipFollow(meleeGruntClip, transform, 1f);

        GameObject hitObj = new GameObject("HandHitbox");
        var hit = hitObj.AddComponent<UniversalHitCollider>();

        hit.Initialize(
            rightHandBone.transform,                           // parent to hand
            UniversalHitCollider.HitboxType.Static,
            20f,                                      // damage
            1.2f,                                     // radius
            1f                                     // duration
        );
    }

    #endregion

    #region Walking and Idle Sounds

    private void AngrySound()
    {
        AudioSystem.Instance.PlayClipFollow(angrySoundClip, transform, 1f);
    }

    private void Step()
    {
        AudioSystem.Instance.PlayClipFollow(stepClip, transform, 1f);
        stepsUntilGrunt--;
        if (stepsUntilGrunt == 0)
        {
            AudioSystem.Instance.PlayClipFollow(runningGruntClip, transform, 1f);
            stepsUntilGrunt = Random.Range(8, 16);
        }
    }

    #endregion
}
