/* using UnityEngine;

public class KillBoss : QuestPart
{
    [Header("BossKill Settings")]
    public string targetBossID; // assign the boss ID here
    public GeneralBossScript targetBoss;

    private void Update()
    {
        if (BossSystem.Instance.currentBoss != null)
        {
            if (isActive && BossSystem.Instance.currentBoss.bossID == targetBossID)
            {
                targetBoss = BossSystem.Instance.currentBoss;
            }
        }

        if (targetBoss != null)
        {
            if (targetBoss.isDead == true)
            {
                Complete();
                Debug.Log("jo");
            }
        }
    }
}
*/