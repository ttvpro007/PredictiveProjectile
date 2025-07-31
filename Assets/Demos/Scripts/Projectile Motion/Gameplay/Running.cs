using Obvious.Soap;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Running : MonoBehaviour
{
    [Header("Running Settings")]
    [Tooltip("Boolean that determines whether the object is currently running.")]
    [SerializeField] private bool run;

    [Tooltip("Speed of the object when running, controlled by the NavMeshAgent.")]
    [SerializeField] private FloatVariable speed;

    [Tooltip("Radius around the map center where the object will randomly pick new destinations.")]
    [SerializeField] private float radius = 10f;

    [Tooltip("Distance tolerance to the destination. If the object is within this distance, it will pick a new destination.")]
    [SerializeField] private float destinationTolerance = 2f;

    private NavMeshAgent agent;
    private Transform mapCenter;
    public Vector3 CurrentDestination { get; private set; }
    public Vector3 CurrentVelocity { get; private set; }
    public event Action<Vector3> OnDestinationChanged;
    private Rigidbody rb;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing from this GameObject.");
            enabled = false;
            return;
        }
        rb = GetComponent<Rigidbody>();
        mapCenter = GameObject.FindGameObjectWithTag("MapCenter")?.transform;
    }

    private void Start()
    {
        if (mapCenter == null)
        {
            Debug.LogError("Map Center not found. Make sure there's a GameObject tagged 'MapCenter'.");
            enabled = false;
            return;
        }

        // If not grounded, let physics drop the object before starting nav movement
        StartCoroutine(InitializeMovement());
    }

    private void Update()
    {
        if (!run || !agent.enabled) return;

        agent.speed = speed.Value;
        CurrentVelocity = agent.velocity;

        if ((transform.position - CurrentDestination).sqrMagnitude <= destinationTolerance * destinationTolerance)
            SetDestination();
    }

    private IEnumerator InitializeMovement()
    {
        // If not grounded, disable agent and kinematic to let gravity apply
        if (!IsGrounded())
        {
            agent.enabled = false;
            rb.isKinematic = false;

            // Wait until object hits the ground
            while (!IsGrounded())
                yield return null;

            // Once grounded, restore kinematic and enable the agent
            rb.isKinematic = true;
            agent.enabled = true;
        }

        // Start running
        SetDestination();
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    private void SetDestination()
    {
        CurrentDestination = GenerateRandomPosition();
        agent.SetDestination(CurrentDestination);
        OnDestinationChanged?.Invoke(CurrentDestination);
    }

    private Vector3 GenerateRandomPosition()
    {
        Vector2 rand = UnityEngine.Random.insideUnitCircle * radius;
        Vector3 pos = new Vector3(rand.x, 0, rand.y);
        return mapCenter.position + pos;
    }

    public void AddExplosionForce(float force, Vector3 origin, float radius)
    {
        if (agent == null || rb == null) return;
        agent.enabled = false;
        rb.isKinematic = false;
        rb.AddExplosionForce(force, origin, radius);
        StartCoroutine(EnableAgentWhenGrounded());
    }

    private IEnumerator EnableAgentWhenGrounded()
    {
        yield return new WaitForSeconds(1f);
        while (!IsGrounded())
            yield return null;
        rb.isKinematic = true;
        agent.enabled = true;
    }
}
