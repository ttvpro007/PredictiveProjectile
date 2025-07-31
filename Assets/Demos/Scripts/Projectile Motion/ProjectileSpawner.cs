using Obvious.Soap;
using UnityEngine;

/// <summary>
/// Base spawner for projectiles, handling trajectory drawing, rotation, and spawning.
/// </summary>
public class ProjectileSpawner : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("The projectile prefab to spawn.")]
    [SerializeField] public GameObject projectilePrefab;

    [Tooltip("The point where the projectile will spawn.")]
    [SerializeField] public Transform spawnPoint;

    [Header("Trajectory Visualization")]
    [Tooltip("Component responsible for drawing the trajectory arc.")]
    [SerializeField] private TrajectoryDrawer trajectoryDrawer;

    [Tooltip("Time step used for trajectory calculation in DrawTrajectory.")]
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    [Tooltip("Transform used to visualize the maximum height of the projectile arc.")]
    [SerializeField] public Transform curveMaxHeightTransform;

    [Header("Thrower Rotation")]
    [Tooltip("Speed at which the thrower rotates towards the launch direction.")]
    [SerializeField] private float rotationSpeed = 5f;

    // The currently calculated initial velocity vector
    protected Vector3 initialVelocity;

    public void SetProjectile(GameObject projectilePrefab)
    {
        this.projectilePrefab = projectilePrefab;
    }

    /// <summary>
    /// Sets the initial velocity for this projectile spawn.
    /// Also orients the spawnPoint to face that direction.
    /// </summary>
    public void SetInitialVelocity(Vector3 velocity)
    {
        initialVelocity = velocity;
        if (spawnPoint != null && velocity.sqrMagnitude > 0f)
        {
            spawnPoint.forward = velocity.normalized;
        }
    }

    /// <summary>
    /// Instantiates the projectile and applies the initial velocity.
    /// </summary>
    public void Spawn()
    {
        if (projectilePrefab == null || spawnPoint == null)
            return;

        GameObject projGO = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        // If there's a Projectile component, set its hit point (optional)
        if (projGO.TryGetComponent<Projectile>(out var proj))
        {
            if (trajectoryDrawer != null)
                proj.SetHitPointPosition(trajectoryDrawer.HitPointPosition);
        }

        // Apply impulse force
        if (projGO.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(initialVelocity, ForceMode.Impulse);
        }
    }

    private void Update()
    {
        DrawTrajectory();
        RotateThrower();
    }

    /// <summary>
    /// Draws the predicted trajectory using the configured drawer.
    /// </summary>
    private void DrawTrajectory()
    {
        if (trajectoryDrawer == null || spawnPoint == null)
            return;

        trajectoryDrawer.DrawTrajectory(
            spawnPoint.position,
            initialVelocity,
            trajectoryTimeStep,
            Physics.gravity);
    }

    /// <summary>
    /// Smoothly rotates this GameObject to face the launch direction.
    /// </summary>
    private void RotateThrower()
    {
        if (initialVelocity.sqrMagnitude < 0.01f)
            return;

        Vector3 dir = initialVelocity.normalized;
        dir.y = 0f; // Keep rotation in XZ plane
        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime);
    }
}
