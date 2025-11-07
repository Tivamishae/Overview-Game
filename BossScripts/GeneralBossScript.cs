using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GeneralBossScript : MonoBehaviour
{
    [Header("Boss Stats")]
    public float health = 400f;
    public bool isInvulnerable = false;
    public bool isDead = false;   // NEW: prevents any actions after death
    public float MaxHealth { get; private set; }
    public string name;
    public string bossID;
    public bool ShouldFacePlayer = true;

    [Header("Boss Drops")]
    public int moneyReward;
    public float xpReward;
    public int itemReward;

    // Health change event (current, max)
    public event Action<float, float> OnHealthChanged;
    public string bossTheme = "Sounds/Bosses/Boss1/BossTheme";

    [Header("Components")]
    public Animator animator { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public Transform player { get; private set; }

    [Header("Boss Attacks")]
    public List<BossAttack> bossAttacks = new List<BossAttack>();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player")?.transform;

        if (animator == null) Debug.LogError("GeneralBossScript: Animator component not found.");
        if (agent == null) Debug.LogError("GeneralBossScript: NavMeshAgent component not found.");
        if (player == null) Debug.LogError("GeneralBossScript: Player not found with tag 'Player'.");

        MaxHealth = Mathf.Max(health, 1f);
        OnHealthChanged?.Invoke(health, MaxHealth); // initial push
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable || isDead) return; // Prevents damage after death

        health = Mathf.Max(0f, health - Mathf.Max(0f, damage));
        OnHealthChanged?.Invoke(health, MaxHealth);

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || isDead) return;

        float max = MaxHealth;
        health = Mathf.Min(health + amount, max);

        OnHealthChanged?.Invoke(health, MaxHealth);
    }

    private void Die()
    {
        if (isDead) return; // safety
        isDead = true;

        Debug.Log("Boss has died.");
        BossSystem.Instance?.UnregisterBoss();
        GrantXPAndMoney();

        // Stop movement & attacks
        if (agent != null) agent.isStopped = true;

        // Delay destruction
        StartCoroutine(DespawnAfterDelay(10f));
    }

    private System.Collections.IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void GrantXPAndMoney()
    {
        PlayerStats.Instance.Money += moneyReward;
        PlayerStats.Instance.XP += xpReward;

        Inventory.Instance.AddItem(itemReward, 1);
    }

    public void FacePlayer()
    {
        if (player == null) return;

        // Direction from this object to player
        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // ignore vertical difference

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
