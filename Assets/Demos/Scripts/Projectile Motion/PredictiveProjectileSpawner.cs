using Obvious.Soap;
using Obvious.Soap.Example;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PredictiveProjectileSpawner : ProjectileSpawner
{
    #region ===== Config: Targeting & Ballistics =====
    [Header("Predictive Target Settings")]
    [Tooltip("The target the projectile is aimed at.")]
    [SerializeField] private Transform target;

    [Tooltip("The NavMeshAgent of the target to read velocity from.")]
    [SerializeField] private NavMeshAgent targetNavMeshAgent;

    [Tooltip("Time to impact used to predict the target's future position.")]
    [SerializeField] private FloatVariable timeToImpact;

    [Tooltip("Fixed launch speed of the projectile (used when 'useFixedLaunchSpeed' is true).")]
    [SerializeField] private FloatVariable fixedLaunchSpeed;

    [Tooltip("If true, use a fixed launch speed and solve for angle; otherwise use time-to-impact.")]
    [SerializeField] private bool useFixedLaunchSpeed = false;

    [Tooltip("Vertical offset added to the predicted target position.")]
    [SerializeField] private float yOffset = 0f;
    #endregion

    [SerializeReference] private TargetProvider targetProvider;

    #region ===== Dependencies =====
    [SerializeField] private WeaponSwitcher weaponSwitcher;
    private CameraController cameraController;
    #endregion

    #region ===== Tuning =====
    [SerializeField, Tooltip("How often to poll for a target if none is assigned.")]
    private float pollInterval = 0.5f;
    private float nextPollTime = 0f;
    #endregion

    #region ===== State =====
    private Vector3 futureTargetPosition;
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────

    #region ===== Unity Lifecycle =====
    private void Awake()
    {
        Camera mainCam = Camera.main;
        cameraController = mainCam ? mainCam.GetComponent<CameraController>() : null;
    }

    private void OnEnable()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.OnProjectileSwitched -= HandleProjectileSwitched;
            weaponSwitcher.OnProjectileSwitched += HandleProjectileSwitched;
        }
    }

    private void OnDisable()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.OnProjectileSwitched -= HandleProjectileSwitched;
        }
    }

    protected override void Update()
    {
        base.Update();

        // Lazy acquire a target if none assigned
        if (target == null && Time.time >= nextPollTime)
        {
            FindTarget();
            nextPollTime = Time.time + pollInterval;
        }
    }
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────

    #region ===== Overrides =====
    protected override void CalculateLaunchParameters()
    {
        if (useFixedLaunchSpeed)
            CalculateLaunchParametersByLaunchSpeed();
        else
            CalculateLaunchParametersByTimeToImpact();
    }
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────

    #region ===== Public / External API =====
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────

    #region ===== Helpers: Prediction & Ballistics =====
    private float GetTimeToImpact()
    {
        return currentProjectile ? timeToImpact.Value + currentProjectile.SpawnDelay : timeToImpact.Value;
    }

    /// <summary> Predicts future target position = current position + velocity * T. </summary>
    private Vector3 PredictFuturePosition()
    {
        if (!target || !targetNavMeshAgent)
            return Vector3.zero;

        return target.position + targetNavMeshAgent.velocity * GetTimeToImpact();
    }

    /// <summary>
    /// Calculate initial velocity using a known time-to-impact.
    /// Solves v0x and v0y from kinematics to reach predicted position at time T.
    /// </summary>
    private void CalculateLaunchParametersByTimeToImpact()
    {
        if (!target || !targetNavMeshAgent)
        {
            // Fallback if we have no target info
            SetInitialVelocity(transform.forward * 20f);
            return;
        }

        // Predict + apply vertical offset
        futureTargetPosition = PredictFuturePosition();
        futureTargetPosition.y += yOffset;

        Vector3 displacement = futureTargetPosition - spawnPoint.position;

        float x = new Vector2(displacement.x, displacement.z).magnitude; // horizontal distance
        float y = displacement.y;                                         // vertical distance
        float t = Mathf.Max(0.0001f, GetTimeToImpact());
        float g = Mathf.Abs(Physics.gravity.y);

        float v0x = x / t;
        float v0y = (y + 0.5f * g * t * t) / t;

        Vector3 horizontalDir = new Vector3(displacement.x, 0f, displacement.z).normalized;
        Vector3 velocity = horizontalDir * v0x + Vector3.up * v0y;

        SetInitialVelocity(velocity);
        PlaceMaxHeightMarker(spawnPoint.position, horizontalDir, v0x, v0y, g);
    }

    /// <summary>
    /// Calculate initial velocity for a moving target with a fixed launch speed.
    /// Brute-forces theta to fit vertical displacement within a tolerance.
    /// </summary>
    private void CalculateLaunchParametersByLaunchSpeed()
    {
        if (!target)
        {
            SetInitialVelocity(transform.forward * 20f);
            return;
        }

        futureTargetPosition = PredictFuturePosition();
        futureTargetPosition.y += yOffset; // keep consistent with time-to-impact path

        Vector3 displacement = futureTargetPosition - spawnPoint.position;
        float x = new Vector2(displacement.x, displacement.z).magnitude;
        float y = displacement.y;

        float g = Mathf.Abs(Physics.gravity.y);
        float v0 = fixedLaunchSpeed ? fixedLaunchSpeed.Value : 20f;

        // Quick range check using optimal 45° angle on flat terrain (approximate)
        float maxRange = (v0 * v0 * Mathf.Sin(2f * 45f * Mathf.Deg2Rad)) / g;
        if (x > maxRange)
        {
            Debug.LogWarning($"[PredictiveProjectileSpawner] Target is out of range (max ~{maxRange:F1}m, need {x:F1}m).");
            return;
        }

        // Brute force theta in [0, 90] to match vertical displacement
        const float tol = 0.5f;
        bool found = false;
        float thetaRad = 0f;
        float v0x = 0f, v0y = 0f;

        for (float deg = 0f; deg <= 90f; deg += 0.1f)
        {
            thetaRad = deg * Mathf.Deg2Rad;
            v0x = v0 * Mathf.Cos(thetaRad);
            v0y = v0 * Mathf.Sin(thetaRad);

            if (Mathf.Abs(v0x) < 1e-3f) continue;

            float t = x / v0x;
            float yCalc = v0y * t - 0.5f * g * t * t;

            if (Mathf.Abs(yCalc - y) <= tol)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning("[PredictiveProjectileSpawner] No feasible launch angle found to hit the target.");
            return;
        }

        Vector3 horizontalDir = new Vector3(displacement.x, 0f, displacement.z).normalized;
        Vector3 velocity = horizontalDir * v0x + Vector3.up * v0y;

        SetInitialVelocity(velocity);
        PlaceMaxHeightMarker(spawnPoint.position, horizontalDir, v0x, v0y, g);
    }

    private void PlaceMaxHeightMarker(Vector3 start, Vector3 horizontalDir, float v0x, float v0y, float g)
    {
        if (!curveMaxHeightTransform) return;

        float tMax = v0y / g;
        float hMax = (v0y * v0y) / (2f * g);
        float horizontalAtMax = v0x * tMax;

        Vector3 pos = start + horizontalDir * horizontalAtMax;
        pos.y = start.y + hMax;

        curveMaxHeightTransform.position = pos;
    }
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────

    #region ===== Target Acquisition & Events =====
    private void HandleProjectileSwitched(Projectile projectile)
    {
        SetProjectile(projectile.gameObject);

        Weapon weapon = weaponSwitcher.GetWeapon(projectile);
        if (!weapon)
        {
            Debug.LogWarning("[PredictiveProjectileSpawner] Weapon is null. Cannot set spawn point.");
            return;
        }

        spawnPoint.SetParent(weapon.ProjectileSpawnPoint);
        spawnPoint.localPosition = Vector3.zero;
    }

    private Transform GetTarget(ICollection<GameObject> targets)
    {
        if (targets.IsNullOrEmpty())
        {
            Debug.LogWarning("[PredictiveProjectileSpawner] No targets available.");
            return null;
        }

        if (targetProvider == null)
        {
            Debug.LogWarning("[PredictiveProjectileSpawner] No TargetProvider assigned. Using first target by default.");
            targetProvider = new FirstTargetProvider(); // Fallback to first target if none provided
        }

        return targetProvider.GetTarget(targets);
    }

    private void FindTarget()
    {
        if (target) return;

        GameObject[] candidates = GameObject.FindGameObjectsWithTag("Enemy");
        if (candidates.IsNullOrEmpty())
        {
            Debug.LogWarning("[PredictiveProjectileSpawner] No target found. Assign in inspector or add an 'Enemy'-tagged object.");
            return;
        }

        target = GetTarget(candidates);

        if (target.TryGetComponent<Health>(out var health))
        {
            health.OnDeath -= HandleTargetDeath; // de-dupe
            health.OnDeath += HandleTargetDeath;
        }

        if (target.TryGetComponent<RunningVisual>(out var runningVisual))
        {
            runningVisual.ShowTargetIndicator(true);
        }

        targetNavMeshAgent = target.GetComponent<NavMeshAgent>();
        cameraController?.AddPointToTrack(target);
    }

    private void HandleTargetDeath()
    {
        if (target)
        {
            cameraController?.RemovePointToTrack(target);
            if (target.TryGetComponent<RunningVisual>(out var runningVisual))
            {
                runningVisual.ShowTargetIndicator(false);
            }
        }

        target = null;
        targetNavMeshAgent = null;

        Debug.Log("[PredictiveProjectileSpawner] Target died. Searching for a new one...");
        FindTarget();
    }
    #endregion
}

public static class ICollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
    {
        return collection == null || collection.Count == 0;
    }
}
