using UnityEngine;
using System;

public class EnemyProjectile : MonoBehaviour
{
    public Quaternion rotationOffset = Quaternion.identity;
    public float damage;
    public AudioClip impactSound; // Assigned dynamically
    private Rigidbody rb;
    private Collider col;

    // Debuff settings
    public bool hasDebuff;
    public DebuffStat debuffStat;   // which stat to affect
    public float debuffPercent;     // 0.40f = 40% reduction
    public float debuffDuration;    // in seconds
    public float SelfDestructDelay = 15f; // seconds before auto-destroy
    private float collisionEnableDelay = 0.5f;

    public Action<Collision> onImpact;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        col.enabled = false;

        Invoke(nameof(EnableCollision), collisionEnableDelay);
    }

    private void EnableCollision()
    {
        col.enabled = true;
    }

    private void Update()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity.normalized) * rotationOffset;

        SelfDestructDelay -= Time.deltaTime;
        if (SelfDestructDelay <= 0f)
        {
            Destroy(gameObject);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        bool hitPlayer = collision.gameObject.CompareTag("Player");

        // Stop physics immediately so it doesn't move during the delay
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        if (hitPlayer)
        {
            // Apply damage & debuff
            PlayerStats.Instance.TakeDamage(damage);
            if (hasDebuff)
            {
                PlayerStats.Instance.ApplyTemporaryReduction(debuffStat, debuffPercent, debuffDuration);
            }
        }

        // Play impact sound (only once)
        if (impactSound && AudioSystem.Instance)
            AudioSystem.Instance.PlayClipAtPoint(impactSound, transform.position, 1f);

        // Trigger any impact event effects (AOE, VFX, etc.)
        onImpact?.Invoke(collision);

        // Destroy projectile after short delay so effects can spawn
        Destroy(gameObject);
    }


}
