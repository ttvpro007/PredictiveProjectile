using Obvious.Soap;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Running_Refactored : MonoBehaviour
{
    #region ===== Config: Running =====
    [Header("Running Settings")]
    [Tooltip("Enable/disable autonomous running.")]
    [SerializeField] private bool run = true;

    [Tooltip("Speed of the object when running, applied to NavMeshAgent.speed.")]
    [SerializeField] private FloatVariable speed;

    [Tooltip("Arrival tolerance (meters).")]
    [SerializeField] private float destinationTolerance = 2f;
    #endregion

    #region ===== Config: Grounding =====
    [Header("Grounding")]
    [Tooltip("Layers considered ground.")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Tooltip("Frames required to confirm grounded.")]
    [SerializeField] private int groundedFrameThreshold = 2;

    [Tooltip("Frames required to confirm airborne.")]
    [SerializeField] private int airborneFrameThreshold = 2;
    #endregion

    #region ===== Injection =====
    [Header("Injection")]
    [SerializeReference] private IGroundChecker groundChecker = new SphereRayGroundChecker();
    [SerializeReference] private IGroundDebugDrawer groundDebugDrawer = new GizmosGroundDebugDrawer();

    [Tooltip("Source of destinations (enemy brain/AI).")]
    [SerializeReference] private IDestinationProvider destinationProvider;
    #endregion

    #region ===== Components (cached) =====
    private Rigidbody rb;
    private NavMeshAgent agent;
    private Collider col;
    #endregion

    #region ===== State =====
    private bool wasGrounded;
    private int groundedFrames;
    private int airborneFrames;
    private GroundCheckResult lastGround;

    // Coroutine handles
    private Coroutine stunCoroutine;

    // Public read-only state
    public Vector3 CurrentDestination { get; private set; }
    public Vector3 CurrentVelocity { get; private set; }
    public FloatVariable Speed => speed;
    #endregion

    #region ===== Events =====
    public event Action<bool> OnGroundedChanged;
    public event Action<Vector3> OnDestinationChanged;

    // Bridge external listeners to the FloatVariable's change event.
    public event Action<float> OnSpeedChanged
    {
        add { speed.OnValueChanged += value; }
        remove { speed.OnValueChanged -= value; }
    }

    public event Action<bool> OnStunned;
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────

    #region ===== Unity Lifecycle =====
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (!agent)
        {
            Debug.LogError("[Running] NavMeshAgent is required.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        RegisterEvents();

        // Initial ground probe & counters
        lastGround = groundChecker?.Check(transform, col, groundMask) ?? default;
        wasGrounded = lastGround.grounded;
        groundedFrames = airborneFrames = 0;

        // Start in physics (airborne-safe); we'll promote to NavMesh when grounded
        rb.isKinematic = false;
        agent.enabled = false;
        if (speed != null) agent.speed = speed.Value;

        // Announce initial state
        OnGroundedChanged?.Invoke(wasGrounded);

        if (run)
            RequestAndSetDestination();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        UnregisterEvents();

        if (agent)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        if (rb) rb.isKinematic = false;

        CurrentDestination = Vector3.zero;
        CurrentVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (!run || !agent.enabled) return;

        CurrentVelocity = agent.velocity;

        // Robust arrival condition
        if (!agent.pathPending &&
            agent.remainingDistance <= destinationTolerance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude < 0.001f))
        {
            RequestAndSetDestination();
        }
    }

    private void FixedUpdate()
    {
        // Probe ground via injected checker
        lastGround = groundChecker?.Check(transform, col, groundMask) ?? default;
        bool grounded = lastGround.grounded;

        if (grounded)
        {
            airborneFrames = 0;
            groundedFrames++;

            if (!wasGrounded && groundedFrames >= Mathf.Max(1, groundedFrameThreshold))
            {
                // Became grounded → hand control to NavMesh
                wasGrounded = true;
                OnGroundedChanged?.Invoke(true);

                rb.isKinematic = true;

                // Warp BEFORE enabling to keep agent internal state synced
                if (!agent.enabled) agent.Warp(transform.position);
                agent.enabled = true;

                if (run && (!agent.hasPath || agent.remainingDistance <= destinationTolerance))
                    RequestAndSetDestination();
            }
        }
        else
        {
            groundedFrames = 0;
            airborneFrames++;

            if (wasGrounded && airborneFrames >= Mathf.Max(1, airborneFrameThreshold))
            {
                // Left ground → return control to physics
                wasGrounded = false;
                OnGroundedChanged?.Invoke(false);

                if (agent.enabled)
                {
                    agent.ResetPath();
                    agent.enabled = false;
                }
                rb.isKinematic = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw last probe (Edit/Play; before first probe this will be default)
        groundDebugDrawer?.Draw(lastGround);
    }
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────

    #region ===== Public API =====
    public void InjectDestinationProvider(IDestinationProvider provider)
    {
        destinationProvider = provider;
    }

    public void AddExplosionForce(float force, Vector3 origin, float radius)
    {
        if (!rb) return;

        if (agent && agent.enabled)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        rb.isKinematic = false;
        rb.AddExplosionForce(force, origin, radius);

        StartCoroutine(EnableAgentWhenGrounded());
    }

    public void ApplyStun(float stunDuration)
    {
        if (!isActiveAndEnabled) return;

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunCoroutine(stunDuration));
    }
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────

    #region ===== Helpers =====
    private void RegisterEvents()
    {
        if (speed != null)
        {
            speed.OnValueChanged -= HandleSpeedChanged; // de-dupe
            speed.OnValueChanged += HandleSpeedChanged;
        }
    }

    private void UnregisterEvents()
    {
        if (speed != null)
        {
            speed.OnValueChanged -= HandleSpeedChanged;
        }
    }

    private void HandleSpeedChanged(float newSpeed)
    {
        if (agent)
            agent.speed = newSpeed;
    }

    private void RequestAndSetDestination()
    {
        if (!agent.enabled) return;

        if (destinationProvider == null) return;

        if (!destinationProvider.TryGetNext(out var candidate))
        {
            // Fallback: hold position
            CurrentDestination = transform.position;
            agent.ResetPath();
            return;
        }

        // Snap to NavMesh
        if (NavMesh.SamplePosition(candidate, out var hit, 2f, NavMesh.AllAreas))
        {
            CurrentDestination = hit.position;
            agent.SetDestination(CurrentDestination);
            OnDestinationChanged?.Invoke(CurrentDestination);
        }
        else
        {
            CurrentDestination = transform.position;
            agent.ResetPath();
        }
    }
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────

    #region ===== Coroutines =====
    private IEnumerator EnableAgentWhenGrounded()
    {
        // Let physics tick first
        yield return new WaitForFixedUpdate();

        int need = Mathf.Max(1, groundedFrameThreshold);
        int count = 0;

        while (count < need)
        {
            var res = groundChecker?.Check(transform, col, groundMask) ?? default;
            if (res.grounded) count++; else count = 0;
            yield return new WaitForFixedUpdate();
        }

        rb.isKinematic = true;

        // Always warp before enabling to sync nav internal position
        agent.Warp(transform.position);
        agent.enabled = true;

        if (run) RequestAndSetDestination();
    }

    private IEnumerator StunCoroutine(float duration)
    {
        OnStunned?.Invoke(true);

        float endTime = Time.time + duration;
        float originalSpeed = agent ? agent.speed : 0f;

        if (agent) agent.speed = 0f; // Stop movement

        while (Time.time < endTime)
            yield return null;

        if (agent && speed != null)
            agent.speed = speed.Value;
        else if (agent)
            agent.speed = originalSpeed;

        OnStunned?.Invoke(false);
    }
    #endregion
}
