using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class DrumCaller : Enemy
{
    [Header("Teleport Settings")]
    public float teleportRadius = 5f;           // landing distance from player
    public int teleportSampleTries = 10;         // tries on back half
    public int teleportFallbackTries = 10;       // tries anywhere
    public float navSampleMaxDistance = 5f;

    [Header("Summon Settings")]
    public GameObject totemPrefab;
    public float totemSpawnDistance = 3f;
    public float totemHealAmount = 50f;
    public int maxActiveTotems = 2;
    private readonly List<GameObject> activeTotems = new();
    private bool teleportQueued = false;

    [Header("Audio")]
    private AudioClip idleSoundClip;
    private AudioClip summonClip;
    private AudioClip teleportClip;
    private int idleCountDown = 0;

    private void Start()
    {
        idleSoundClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/DrumCaller/IdleGrunt");
        summonClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/DrumCaller/Summon");
        teleportClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/DrumCaller/Teleport");
    }

    #region Combat

    protected override void EnterCombat()
    {
        base.EnterCombat();

        // Turret caster: no movement in combat
        agent.isStopped = true;
        agent.ResetPath();
    }

    protected override void UpdateCombat()
    {
        if (isAttacking) return;
        agent.isStopped = true;

        attackCooldownTimer -= Time.deltaTime;
        if (attackCooldownTimer <= 0f)
        {
            StartAttack();
        }
        FacePlayer();
    }

    #endregion

    #region Teleport Logic

    public void PerformTeleport()
    {
        if (TryTeleportBehindPlayer(out Vector3 tp))
        {
            if (agent && agent.enabled) agent.Warp(tp);
            else transform.position = tp;

            TeleportSound();
        }
    }

    private bool TryTeleportBehindPlayer(out Vector3 pos)
    {
        pos = transform.position;
        if (!player) return false;

        Vector3 playerPos = player.transform.position;
        Vector3 back = -player.transform.forward;

        // Back half first
        for (int i = 0; i < teleportSampleTries; i++)
        {
            float angle = Random.Range(-90f, 90f);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * back;
            Vector3 candidate = playerPos + dir.normalized * teleportRadius;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleMaxDistance, NavMesh.AllAreas))
            {
                pos = hit.position;
                return true;
            }
        }

        // Fallback anywhere on the circle
        for (int i = 0; i < teleportFallbackTries; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 candidate = playerPos + dir.normalized * teleportRadius;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleMaxDistance, NavMesh.AllAreas))
            {
                pos = hit.position;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Summon Logic

    public void DoSummon()
    {
        // Clean nulls
        activeTotems.RemoveAll(t => t == null);

        if (activeTotems.Count < maxActiveTotems)
        {
            TrySpawnTotem();
        }
        else
        {
            // Heal existing totems
            foreach (var totem in activeTotems)
            {
                if (totem != null && totem.TryGetComponent(out DrumCallerTotem th))
                {
                    th.Heal(totemHealAmount);
                }
            }
        }
    }

    private void TrySpawnTotem()
    {
        Vector3 spawnDir = (player.transform.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + spawnDir * totemSpawnDistance;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            GameObject totem = Instantiate(totemPrefab, hit.position, Quaternion.identity);

            // Face toward the player
            Vector3 lookPos = player.transform.position;
            lookPos.y = totem.transform.position.y;
            totem.transform.LookAt(lookPos);

            activeTotems.Add(totem);
        }
    }

    #endregion

    #region Audio

    public void IdleGrunt()
    {
        idleCountDown -= 1;
        if (idleCountDown <= 0)
        {
            AudioSystem.Instance.PlayClipFollow(idleSoundClip, transform, 1f);
            idleCountDown = Random.Range(5, 10);
        }
    }

    private void TeleportSound() => AudioSystem.Instance.PlayClipFollow(teleportClip, transform, 1f);
    private void Drum() => AudioSystem.Instance.PlayClipFollow(summonClip, transform, 1f);

    #endregion

    #region Death

    public override void ResetEnemy()
    {
        base.ResetEnemy();

        // Clean up spawned totems (or return to pool if you add pooling later)
        for (int i = 0; i < activeTotems.Count; i++)
        {
            if (activeTotems[i] != null)
                Destroy(activeTotems[i]);
        }
        activeTotems.Clear();
    }

    protected override void EnterDead()
    {
        base.EnterDead();

        // Trigger proper death sequence on all active totems
        foreach (var totem in activeTotems)
        {
            if (totem != null && totem.TryGetComponent(out DrumCallerTotem dt))
            {
                dt.KillTotem(); // Proper death flow (EnterDead etc.)
            }
        }

        activeTotems.Clear();
    }

    #endregion
}
