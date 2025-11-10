using UnityEngine;

public class ProximityAngerTrigger : MonoBehaviour, IAngerTrigger
{
    public float provokeRadius = 5f;
    public float requiredTime = 2f;

    private float _timer;

    public bool ShouldTrigger(NPC npc)
    {
        if (npc.player == null) return false;

        float dist = Vector3.Distance(npc.transform.position, npc.player.transform.position);

        if (dist <= provokeRadius)
        {
            _timer += Time.deltaTime;
            return _timer >= requiredTime;
        }

        _timer = 0f;
        return false;
    }
}
