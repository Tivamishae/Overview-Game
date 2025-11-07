using UnityEngine;
using UnityEngine.AI;

public class DrumCallerTotem : Enemy
{
    [Header("Totem Settings")]
    public float drainRange = 20f;
    public float drainDamagePerSecond = 10f;
    private float nextDrumTime;
    private bool isDraining = false;

    [Header("Audio")]
    private AudioClip drainClip;
    private AudioClip totemAppearClip;

    [Header("Drain VFX")]
    public GameObject drainBeamPrefab;      // Prefab spawns at player and travels to totem
    public GameObject activeBeam;           // Instance of beam
    public Collider drainDestroyer;         // Collider at totem used to detect when beam hits
    private ParticleSystem activeBeamParticles;

    private void Start()
    {
        ScheduleNextDrum();

        drainClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/DrumCaller/Drain");
        totemAppearClip = AudioPreloader.Instance.GetClip("Sounds/Enemies/DrumCaller/TotemAppear");
        despawnTimer = 2.5f;
        removeColliderTimer = 1f;
    }

    #region Update

    protected override void Update()
    {
        base.Update();

        // Always idle state only – no combat, no wandering
        if (currentState != EnemyState.Idle)
            SetState(EnemyState.Idle);

        // Trigger Drum animation periodically
        if (Time.time >= nextDrumTime)
        {
            PlayTrigger("Drum");
            ScheduleNextDrum();
        }

        // Drain logic
        if (isDraining && activeBeam)
        {
            // Deal damage over time when draining
            PlayerStats.Instance.TakeDamage(drainDamagePerSecond * Time.deltaTime);

            // Beam follows player and aims toward totem
            activeBeam.transform.position = player.transform.position;
            activeBeamParticles?.Emit(1);

            Vector3 target = transform.position + Vector3.up * 0.5f;
            activeBeam.transform.rotation = Quaternion.LookRotation(target - player.transform.position);
        }
    }

    #endregion

    #region Drumming and Draining

    private void ScheduleNextDrum() =>
        nextDrumTime = Time.time + Random.Range(attackCooldownMin, attackCooldownMax);

    // Called from animation event
    public void StartDrain()
    {
        isDraining = true;

        if (activeBeam == null)
        {
            activeBeam = Instantiate(drainBeamPrefab, player.transform.position, Quaternion.identity);
            activeBeamParticles = activeBeam.GetComponent<ParticleSystem>();

            Vector3 dir = (transform.position - player.transform.position).normalized;
            activeBeam.transform.rotation = Quaternion.LookRotation(dir);

            activeBeamParticles.Clear(true);
            activeBeamParticles.Play();

            var trigger = activeBeamParticles.trigger;
            trigger.enabled = true;
            trigger.SetCollider(0, drainDestroyer);
        }

        AudioSystem.Instance.PlayClipFollow(totemAppearClip, transform, 1f);
    }

    // Called from animation event
    public void EndDrain()
    {
        isDraining = false;

        if (activeBeam != null)
        {
            var ps = activeBeam.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();

            Destroy(activeBeam, 1.5f);
            activeBeam = null;
        }
    }

    #endregion

    #region Unncessary Overrides

    protected override void EnterCombat() { }   // Totem never enters combat
    protected override void UpdateCombat() { }  // Disable combat logic
    protected override void EnterWander() { }   // Disable wandering
    protected override void UpdateWander() { }  // Disable wandering
    protected override void EnterSearching() { }
    protected override void UpdateSearching() { }

    #endregion

    #region Death

    public override void ResetEnemy()
    {
        base.ResetEnemy();

        // Clear beam if any
        EndDrain();

        ScheduleNextDrum();
        isDraining = false;
    }

    public void KillTotem()
    {
        if (currentState == EnemyState.Dead) return; // already dying
        SetState(EnemyState.Dead);
    }

    #endregion

    #region Additional Functions

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        // Optional: play heal VFX or animation
    }

    public void Appear() => AudioSystem.Instance.PlayClipFollow(totemAppearClip, transform, 1f);

    #endregion
}
