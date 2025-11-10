using UnityEngine;

public class OnSeePlayerAngerTrigger : MonoBehaviour, IAngerTrigger
{
    public bool requireContinuousSight = false;
    public float requiredTime = 0.5f;

    private float _seenTimer;

    public bool ShouldTrigger(NPC npc)
    {
        if (!npc.noticesPlayer)
        {
            if (requireContinuousSight)
                _seenTimer = 0f;
            return false;
        }

        if (!requireContinuousSight)
            return true;

        _seenTimer += Time.deltaTime;
        return _seenTimer >= requiredTime;
    }
}
