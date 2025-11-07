using UnityEngine;

public class UniversalHitCollider : MonoBehaviour
{
    public enum HitboxType { Static, Expanding }

    private HitboxType type;
    private float damage;
    private float duration;
    private float maxRadius;
    private float elapsed;
    private bool hasDamagedPlayer = false;

    private SphereCollider col;

    /// <summary>
    /// Initialize a hitbox and parent it to a target object.
    /// For Static: radius stays constant
    /// For Expanding: radius expands then shrinks over time
    /// </summary>
    /// <param name="parent">Object the hitbox will follow</param>
    /// <param name="t">Hitbox type (Static or Expanding)</param>
    /// <param name="dmg">Damage to apply on hit</param>
    /// <param name="radius">Static radius or max expanding radius</param>
    /// <param name="dur">Lifetime in seconds</param>
    public void Initialize(Transform parent, HitboxType t, float dmg, float radius, float dur)
    {
        // Parent to the provided object
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        type = t;
        damage = dmg;
        maxRadius = radius;
        duration = dur;

        // Collider Setup
        col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;

        if (type == HitboxType.Static)
        {
            col.radius = maxRadius;
        }
        else // Expanding
        {
            col.radius = 0.01f; // start small
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        // Expand + shrink using a sine curve
        if (type == HitboxType.Expanding)
        {
            float t = elapsed / duration;                      // 0  1
            col.radius = Mathf.Sin(t * Mathf.PI) * maxRadius;  // 0  max  0
        }

        if (elapsed >= duration)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDamagedPlayer) return;

        if (other.CompareTag("Player"))
        {
            hasDamagedPlayer = true;
            PlayerStats.Instance.TakeDamage(damage);
        }
    }
}
