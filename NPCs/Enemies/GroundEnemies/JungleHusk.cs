using UnityEngine;
using UnityEngine.AI;

public class JungleHusk : NPC
{
    [Header("Attack Settings")]
    public GameObject rightHandBone;

    [Header("Audio Logic")]
    private int stepsUntilGrunt = 25;

    #region Animator

    protected override void EnterAngry()
    {
        base.EnterAngry();

        PlayBool("Angry", true);
    }

    protected override void EndAngry()
    {
        base.EndAngry();

        PlayBool("Angry", false);
    }

    #endregion
}
