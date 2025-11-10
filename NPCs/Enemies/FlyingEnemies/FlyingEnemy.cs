/* using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class FlyingEnemy : Enemy
{
    [Header("Flying Logic")]
    private bool _isInAir;
    public float groundOffset = 0f;
    public float flyingOffset = 3f; // example
    public float deathSpeed = 1f;
    private Coroutine heightRoutine;
    private Coroutine deathRoutine;
    [SerializeField] private float heightChangeSpeed = 2f;

    [Header("Flight Cycle")]
    public bool useFlightCycle = true;
    public Vector2 airTimeRange = new Vector2(40f, 100f);
    public Vector2 groundTimeRange = new Vector2(20f, 40f);
    public float flightCycleTimer;

    #region FlightCycle Logic

    private void Update()
    {
        base.Update();
        UpdateFlightCycle();
    }

    private void UpdateFlightCycle()
    {
        if (!useFlightCycle) return;

        // Only cycle when NOT in combat & NOT dead
        if (currentState == EnemyState.Combat || currentState == EnemyState.Dead)
            return;

        flightCycleTimer -= Time.deltaTime;
        if (flightCycleTimer <= 0f)
        {
            // Toggle flying state
            isInAir = !isInAir;
            StartFlightCycle();
        }
    }

    private void StartFlightCycle()
    {
        // If currently flying  schedule landing
        if (isInAir)
            flightCycleTimer = Random.Range(groundTimeRange.x, groundTimeRange.y);
        else
            flightCycleTimer = Random.Range(airTimeRange.x, airTimeRange.y);
    }

    #endregion

    #region Air Logic
    protected bool isInAir
    {
        get => _isInAir;
        set
        {
            if (_isInAir == value) return; // no change, do nothing

            _isInAir = value;

            if (_isInAir)
                TakeOff();
            else
                Land();
        }
    }

    protected void TakeOff()
    {
        PlayBool("isFlying", true);

        if (heightRoutine != null) StopCoroutine(heightRoutine);
        heightRoutine = StartCoroutine(AdjustHeight(flyingOffset));
    }

    protected void Land()
    {
        PlayBool("isFlying", false);

        if (heightRoutine != null) StopCoroutine(heightRoutine);
        heightRoutine = StartCoroutine(AdjustHeight(groundOffset));
    }

    private IEnumerator AdjustHeight(float targetOffset)
    {
        while (!Mathf.Approximately(agent.baseOffset, targetOffset))
        {
            agent.baseOffset = Mathf.MoveTowards(agent.baseOffset, targetOffset, heightChangeSpeed * Time.deltaTime);
            yield return null;
        }

        heightRoutine = null;
    }

    #endregion

    #region Combat

    protected override void EnterCombat()
    {
        base.EnterCombat();

        // If not already flying, take off
        if (!isInAir)
            isInAir = true;
    }

    #endregion

    #region Death

    protected override void EnterDead()
    {
        // Stop NavMesh movement but keep agent enabled for descent
        agent.isStopped = true;
        agent.ResetPath();

        // Stop any active flying height changes
        if (heightRoutine != null) StopCoroutine(heightRoutine);

        // Start descent
        deathRoutine = StartCoroutine(DescentOnDeath());

        // Play death animation
        PlayTrigger("Death");
    }

    private IEnumerator DescentOnDeath()
    {
        // Smoothly descend while still using agent.baseOffset
        while (agent.baseOffset > groundOffset)
        {
            agent.baseOffset = Mathf.MoveTowards(agent.baseOffset, groundOffset, deathSpeed * Time.deltaTime);
            yield return null;
        }

        // Now disable agent after reaching ground
        agent.enabled = false;

        // Apply physics to settle body
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        deathRoutine = null;
    }

    public override void ResetEnemy()
    {
        base.ResetEnemy();

        // Stop any height-related coroutines from previous life
        if (heightRoutine != null) StopCoroutine(heightRoutine);
        if (deathRoutine != null) StopCoroutine(deathRoutine);

        // Reset flight state WITHOUT triggering animations
        _isInAir = false;                // silent reset of flag
        agent.baseOffset = groundOffset; // snap to ground height

        // Re-enable the agent (important if disabled on death)
        agent.enabled = true;
    }

    #endregion
}

*/