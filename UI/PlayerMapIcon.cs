using UnityEngine;

public class PlayerMapIcon : MonoBehaviour
{
    [SerializeField] private Transform player; // assign player in inspector
    [SerializeField] private float height = 2400f;

    void LateUpdate()
    {
        if (player == null) return;

        // Follow player but stay at fixed height above
        Vector3 pos = player.position;
        pos.y += height;
        transform.position = pos;
    }
}
