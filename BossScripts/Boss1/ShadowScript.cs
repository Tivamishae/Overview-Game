using UnityEngine;
using System.Collections;

public class ShadowScript : MonoBehaviour
{
    public float shadowDashDelay = 2f;       // how long the shadow waits before dashing
    public float shadowDashSpeed = 25f;      // speed of shadow dash
    public float damageRadius = 2f;
    public GameObject player;
    public bool ShouldFacePlayer = true;
    public string shadowDashSound = "Sounds/Bosses/Boss1/ShadowSpiritDash";
    private Vector3 dashStart;

    private bool hasDealtDamage = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        dashStart = transform.position;
    }

    void Update()
    {
        if (ShouldFacePlayer)
        {
            FacePlayer();
        }
    }

    public void SpawnShadow()
    {
        // Entry point, if needed
    }

    private void DashToTarget()
    {
        ShouldFacePlayer = false;
        Animator animator = GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;
        StartCoroutine(ShadowDashCoroutine());

        var smokePrefab = Resources.Load<GameObject>("3D/Bosses/Boss1/Boss1Spawns/SpiritDashResidue");
        if (smokePrefab != null)
        {
            // Parent smoke to the shadow so it follows
            var smokeEffect = Instantiate(smokePrefab, transform.position, Quaternion.identity, transform);
            // (optional) auto destroy handled by smoke prefab itself
        }
    }

    private IEnumerator ShadowDashCoroutine()
    {
        yield return new WaitForSeconds(shadowDashDelay);
        Vector3 target = BossSystem.Instance.currentBoss.transform.position;
        PlaySound(shadowDashSound, 1f, false);

        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            // Move towards target
            transform.position = Vector3.MoveTowards(transform.position, target, shadowDashSpeed * Time.deltaTime);

            // Damage check (only once)
            if (!hasDealtDamage && player != null)
            {
                float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distToPlayer <= damageRadius)
                {
                    hasDealtDamage = true;
                    PlayerStats.Instance.TakeDamage(50f);
                    Debug.Log("Shadow hit player for 50 damage!");
                }
            }

            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        // Shadow reached destination  make it invisible
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!r.CompareTag("Effect")) // don’t hide effects
                r.enabled = false;
        }

        // Let smoke and other effects play out
        Destroy(gameObject, 10f);
    }

    private void FacePlayer()
    {
        if (player == null) return;

        // Direction from this object to player
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f; // ignore vertical difference

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void PlaySound(string path, float volume, bool loop)
    {
        AudioClip clip = AudioPreloader.Instance.GetClip(path);
        if (clip == null)
        {
            Debug.LogError($"GameAmbience: Could not find music at Resources/{path}");
            return;
        }

        Transform followTarget = this.transform;

        AudioSystem.Instance.PlayClipFollow(clip, followTarget, 1f);
    }
}
