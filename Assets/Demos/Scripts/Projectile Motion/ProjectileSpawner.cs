using Obvious.Soap;
using System;
using System.Collections;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Projectile Settings")]

    /// <summary>
    /// The projectile prefab to spawn.
    /// </summary>
    [Tooltip("The projectile prefab to spawn.")]
    [SerializeField] protected GameObject projectilePrefab;

    /// <summary>
    /// The point where the projectile will spawn.
    /// </summary>
    [Tooltip("The point where the projectile will spawn.")]
    [SerializeField] protected Transform spawnPoint;

    /// <summary>
    /// Adjustable launch force.
    /// </summary>
    [Tooltip("Adjustable launch force.")]
    [SerializeField] protected FloatVariable launchForce;

    /// <summary>
    /// Adjustable angle in degrees for upward launch.
    /// </summary>
    [Tooltip("Adjustable angle in degrees for upward launch.")]
    [SerializeField] protected FloatVariable upwardAngle;

    /// <summary>
    /// Time step for trajectory calculation.
    /// </summary>
    [Tooltip("Time step for trajectory calculation.")]
    [SerializeField] protected float trajectoryTimeStep = 5f;

    [Header("References")]
    /// <summary>
    /// Reference to the TrajectoryDrawer component.
    /// </summary>
    [Tooltip("Reference to the TrajectoryDrawer component.")]
    [SerializeField] protected TrajectoryDrawer trajectoryDrawer;

    /// <summary>
    /// Speed at which the thrower rotates.
    /// </summary>
    [Tooltip("Speed at which the thrower rotates.")]
    [SerializeField] protected float rotationSpeed = 5f;  // Speed at which the thrower rotates to face the runner

    [SerializeField] protected Transform curveMaxHeightTransform;

    public event Action<Projectile> OnProjectileShot;
    public event Action<Projectile> OnProjectileSpawned; // Event triggered before a projectile is shot for the delay logic

    protected Vector3 launchDirection;
    private Vector3 initialVelocity;

    protected bool CanSpawn => Time.time >= nextSpawnTime;
    protected float nextSpawnTime;

    /// <summary>
    /// Sets the launch force.
    /// </summary>
    /// <param name="launchForce">The desired launch force.</param>
    public void SetLaunchForce(float launchForce)
    {
        this.launchForce.Value = launchForce;
    }

    protected virtual void Update()
    {
        // Update the launch parameters each frame to account for changes in force or angle
        CalculateLaunchParameters();

        // Draw the trajectory in real-time
        DrawTrajectory();

        RotateThrower();
    }

    public void SetInitialVelocity(Vector3 initialVelocity)
    {
        this.initialVelocity = initialVelocity;

        spawnPoint.LookAt(initialVelocity);
    }

    public void SetProjectile(GameObject projectilePrefab)
    {
        this.projectilePrefab = projectilePrefab;
    }

    /// <summary>
    /// Calculates the launch direction and initial velocity based on the current parameters.
    /// </summary>
    protected virtual void CalculateLaunchParameters()
    {
        // Calculate launch direction with the upward angle
        launchDirection = Quaternion.AngleAxis(-upwardAngle.Value, spawnPoint.right) * spawnPoint.forward;

        // Calculate initial velocity based on the launch force and direction
        SetInitialVelocity(launchDirection.normalized * launchForce.Value);
    }

    /// <summary>
    /// Draws the trajectory in real-time using the TrajectoryDrawer component.
    /// </summary>
    protected virtual void DrawTrajectory()
    {
        if (trajectoryDrawer == null || spawnPoint == null) return;

        // Call the DrawTrajectory method on the TrajectoryDrawer component
        trajectoryDrawer.DrawTrajectory(spawnPoint.position, initialVelocity, trajectoryTimeStep, Physics.gravity);
    }

    // Function to rotate the thrower to face the runner
    private void RotateThrower()
    {
        Vector3 directionToTarget = initialVelocity.normalized;
        directionToTarget.y = 0f;  // Keep the direction strictly in the XZ plane

        // Calculate the target rotation to look at the runner
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Smoothly rotate the thrower towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private Projectile cachedProjectile;

    /// <summary>
    /// Schedules a spawn: fires OnBefore immediately, then does the real spawn after `delay` seconds.
    /// </summary>
    public void SpawnProjectile()
    {
        if (!CanSpawn) return;

        cachedProjectile = CreateProjectile();

        if (cachedProjectile == null) return;

        OnProjectileSpawned?.Invoke(cachedProjectile);

        if (cachedProjectile.SpawnDelay <= 0f)
        {
            ShootProjectile();
            return;
        }

        cachedProjectile.gameObject.SetActive(false);
        StartCoroutine(DoSpawnAfterDelay(cachedProjectile.SpawnDelay));
    }

    private IEnumerator DoSpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShootProjectile();
    }

    /// <summary>
    /// Spawns the projectile and applies the calculated launch force to it.
    /// </summary>
    public void ShootProjectile()
    {
        if (!CanSpawn) return;

        if (projectilePrefab == null || spawnPoint == null) return;

        //Projectile projectile = CreateProjectile();

        if (!cachedProjectile)
        {
            Debug.LogWarning("No projectile to spawn. Please call RequestSpawn first.");
            return;
        }

        cachedProjectile.gameObject.SetActive(true); // Activate the projectile game object
        // Get the Rigidbody component of the spawned projectile
        if (cachedProjectile.TryGetComponent<Rigidbody>(out var rb))
        {
            // Apply force in the calculated direction
            rb.AddForce(initialVelocity, ForceMode.Impulse);
        }

        nextSpawnTime = Time.time + cachedProjectile.Cooldown; // Set the next spawn time
        OnProjectileShot?.Invoke(cachedProjectile);

        cachedProjectile = null; // Clear the cached projectile after shooting
    }

    private Projectile CreateProjectile()
    {
        // Instantiate the projectile at the spawn point's position and rotation
        GameObject spawnedProjectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        Projectile projectile = spawnedProjectile.GetComponent<Projectile>();
        projectile.SetHitPointPosition(trajectoryDrawer.HitPointPosition);

        return projectile;
    }
}
