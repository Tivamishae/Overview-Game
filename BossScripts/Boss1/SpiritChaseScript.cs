using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundRippleProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f;                // How fast it moves
    public float overshootDistance = 5f;    // How far it goes after hitting player

    [Header("Ripple Settings")]
    public GameObject ripplePrefab;         // Effect prefab
    public float rippleInterval = 0.5f;     // How often to spawn ripples
    public float rippleLifetime = 2f;       // How long each ripple lasts

    [Header("Damage Settings")]
    public float hitRadius = 1.5f;          // Radius to damage player
    public float damage = 40f;              // Damage dealt on hit

    [Header("Lifetime Settings")]
    public float maxLifetime = 8f;          // Time before projectile despawns

    public Transform player;
    public GroundCheck playerGroundCheck;
    private bool hasDealtDamage = false;
    private Vector3 finalDirection;
    private bool followingPlayer = true;
    private Vector3 spawnPosition;

    public string spiritChaseSound = "Sounds/Bosses/Boss1/SpiritChaseMagic";
    public string spiritChaseSplashSound = "Sounds/Bosses/Boss1/SpiritChaseSplash";
    public GameObject fireResidue;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;

        if (player != null)
        {
            Transform groundCheckChild = player.Find("Ground Check");
            if (groundCheckChild != null)
            {
                playerGroundCheck = groundCheckChild.GetComponent<GroundCheck>();
            }
            else
            {
                Debug.LogWarning("GroundRippleProjectile: No child named 'GroundCheck' found under player!");
            }
        }

        spawnPosition = transform.position;

        StartCoroutine(SpawnRipples());
    }


    void Update()
    {
        if (player == null) return;

        if (followingPlayer)
        {
            // Move towards player
            Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 dir = (targetPos - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;

            // Check if close enough to hit
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist <= hitRadius)
            {
                if (!hasDealtDamage && playerGroundCheck != null && playerGroundCheck.isGrounded)
                {
                    PlayerStats.Instance.TakeDamage(damage);
                    hasDealtDamage = true;
                    Debug.Log("GroundRipple hit grounded player!");
                }

                // Save direction and continue past player
                finalDirection = dir;
                followingPlayer = false;

                // Start overshoot timer
                StartCoroutine(OvershootThenDestroy(1.5f));
            }
        }
        else
        {
            // Continue forward after hitting
            transform.position += finalDirection * speed * Time.deltaTime;
        }

        StickToGround(); // <-- keeps it glued to terrain
    }

    private IEnumerator OvershootThenDestroy(float overshootTime)
    {
        yield return new WaitForSeconds(overshootTime);
        ParticleSystem ps = fireResidue.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;   // get a reference to the main module
            main.loop = false;    // disable looping
        }
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!r.CompareTag("Effect")) // don’t hide effects
                r.enabled = false;
        }
        Destroy(gameObject);
    }


    private void StickToGround()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 2f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            if (hit.collider.CompareTag("Arena"))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
        }
    }

    private IEnumerator SpawnRipples()
    {
        while (true)
        {
            if (ripplePrefab != null)
            {
                Vector3 ripplePos = transform.position;

                Ray ray = new Ray(ripplePos + Vector3.up * 2f, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, 5f))
                {
                    if (hit.collider.CompareTag("Arena"))
                    {
                        ripplePos.y = hit.point.y;
                    }
                }

                var ripple = Instantiate(ripplePrefab, ripplePos, Quaternion.identity);
                PlaySound(spiritChaseSound, 1f, false, ripplePos);
                PlaySound(spiritChaseSplashSound, 1f, false, ripplePos);
                Destroy(ripple, rippleLifetime);
            }
            yield return new WaitForSeconds(rippleInterval);
        }
    }

    private void PlaySound(string path, float volume, bool loop, Vector3 pos)
    {
        AudioClip clip = AudioPreloader.Instance.GetClip(path);
        if (clip == null)
        {
            Debug.LogError($"GameAmbience: Could not find music at Resources/{path}");
            return;
        }

        AudioSystem.Instance.PlayClipAtPoint(clip, pos, 1f);
    }
}
