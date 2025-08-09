using Obvious.Soap;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Running : MonoBehaviour
{
    [Header("Running Settings")]
    [Tooltip("Enable/disable autonomous running.")]
    [SerializeField] private bool run = true;

    [Tooltip("Speed of the object when running, applied to NavMeshAgent.speed.")]
    [SerializeField] private FloatVariable speed;

    [Tooltip("Radius around the map center to pick new destinations.")]
    [SerializeField] private float radius = 10f;

    [Tooltip("Arrival tolerance (meters).")]
    [SerializeField] private float destinationTolerance = 2f;

    [Header("Grounding")]
    [Tooltip("Layers considered ground.")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Tooltip("Frames required to confirm grounded.")]
    [SerializeField] private int groundedFrameThreshold = 2;

    [Tooltip("Frames required to confirm airborne.")]
    [SerializeField] private int airborneFrameThreshold = 2;


    [Header("Injection")]
    [SerializeReference] private IGroundChecker groundChecker = new SphereRayGroundChecker();
    [SerializeReference] private IGroundDebugDrawer groundDebugDrawer = new GizmosGroundDebugDrawer();

    // Components
    private Rigidbody rb;
    private NavMeshAgent agent;
    private Transform mapCenter;    // GameObject tagged "MapCenter"
    private Collider myCol;

    // State
    private bool wasGrounded;
    private int groundedFrames;
    private int airborneFrames;
    private GroundCheckResult lastGround;

    // Events
    public event Action<bool> OnGroundedChanged;
    public event Action<Vector3> OnDestinationChanged;
    public event Action<float> OnSpeedChanged
    {
        add
        {
            if (speed != null)
                value?.Invoke(speed.Value);
            speed.OnValueChanged += value;
        }
        remove
        {
            speed.OnValueChanged -= value;
        }
    }

    // Public read-only state
    public Vector3 CurrentDestination { get; private set; }
    public Vector3 CurrentVelocity { get; private set; }
    public FloatVariable Speed => speed;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();

        if (!agent)
        {
            Debug.LogError("[Running] NavMeshAgent is required.");
            enabled = false;
            return;
        }

        mapCenter = GameObject.FindGameObjectWithTag("MapCenter")?.transform;
    }

    private void OnEnable()
    {
        if (mapCenter == null)
        {
            Debug.LogError("[Running] Map Center not found. Add a GameObject tagged 'MapCenter'.");
            enabled = false;
            return;
        }

        // Initial ground probe
        lastGround = groundChecker?.Check(transform, myCol, groundMask) ?? default;
        wasGrounded = lastGround.grounded;
        groundedFrames = airborneFrames = 0;

        // Start in nav state (we'll disable on airborne)
        rb.isKinematic = false;
        agent.enabled = false;
        //agent.Warp(transform.position); // sync agent's internal nav position

        // Announce initial state
        OnGroundedChanged?.Invoke(wasGrounded);

        if (run)
            SetDestination();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

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

        agent.speed = speed ? speed.Value : agent.speed;
        CurrentVelocity = agent.velocity;

        // Robust arrival condition
        if (!agent.pathPending &&
            agent.remainingDistance <= destinationTolerance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude < 0.001f))
        {
            SetDestination();
        }
    }

    private void FixedUpdate()
    {
        // Probe ground via injected checker
        lastGround = groundChecker?.Check(transform, myCol, groundMask) ?? default;
        bool grounded = lastGround.grounded;

        if (grounded)
        {
            airborneFrames = 0;
            groundedFrames++;

            if (!wasGrounded && groundedFrames >= Mathf.Max(1, groundedFrameThreshold))
            {
                // Became grounded
                wasGrounded = true;
                OnGroundedChanged?.Invoke(true);

                Debug.Log($"[Running] Grounded at {transform.position} after {groundedFrames} frames.");

                rb.isKinematic = true;
                // Warp BEFORE enabling to keep agent in sync with current pose
                if (!agent.enabled) agent.Warp(transform.position);
                agent.enabled = true;

                if (run && (!agent.hasPath || agent.remainingDistance <= destinationTolerance))
                    SetDestination();
            }
        }
        else
        {
            groundedFrames = 0;
            airborneFrames++;

            if (wasGrounded && airborneFrames >= Mathf.Max(1, airborneFrameThreshold))
            {
                // Left ground
                wasGrounded = false;
                OnGroundedChanged?.Invoke(false);

                Debug.Log($"[Running] Airborne at {transform.position} after {airborneFrames} frames.");

                if (agent.enabled)
                {
                    agent.ResetPath();
                    agent.enabled = false;
                }
                rb.isKinematic = false;
            }
        }
    }

    private void SetDestination()
    {
        if (!agent.enabled) return;

        // Try a handful of random points; snap to NavMesh
        for (int i = 0; i < 8; i++)
        {
            Vector2 rand = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 candidate = mapCenter.position + new Vector3(rand.x, 0f, rand.y);

            if (NavMesh.SamplePosition(candidate, out var hit, radius, NavMesh.AllAreas))
            {
                CurrentDestination = hit.position;
                agent.SetDestination(CurrentDestination);
                OnDestinationChanged?.Invoke(CurrentDestination);
                return;
            }
        }

        // Fallback: hold position
        CurrentDestination = transform.position;
        agent.ResetPath();
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

    private IEnumerator EnableAgentWhenGrounded()
    {
        // Let physics tick first
        yield return new WaitForFixedUpdate();

        int need = Mathf.Max(1, groundedFrameThreshold);
        int count = 0;

        while (count < need)
        {
            var res = groundChecker?.Check(transform, myCol, groundMask) ?? default;
            if (res.grounded) count++; else count = 0;
            yield return new WaitForFixedUpdate();
        }

        rb.isKinematic = true;

        // Always warp before enabling to sync nav internal position
        agent.Warp(transform.position);
        agent.enabled = true;

        if (run) SetDestination();
    }

    private void OnDrawGizmos()
    {
        // Draw last probe (works in Edit/Play; during Edit, lastGround will be default until first probe)
        groundDebugDrawer?.Draw(lastGround);
    }
}
