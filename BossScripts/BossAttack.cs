using UnityEngine;

public abstract class BossAttack : MonoBehaviour
{
    public abstract string TriggerName { get; }

    public abstract void Execute(GeneralBossScript boss);
}
