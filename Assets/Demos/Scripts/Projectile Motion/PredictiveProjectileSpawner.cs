using UnityEngine;

/// <summary>
/// Generic spawner that predicts and launches projectiles using injected modules.
/// All module configuration lives in their own classes, so this spawner is fully generic.
/// </summary>
[RequireComponent(typeof(ProjectileSpawner))]
public class PredictiveProjectileSpawner : ProjectileSpawner
{
    [Header("Modules (assign implementations via SerializeReference)")]
    [Tooltip("Provides the target Transform and its velocity.")]
    [SerializeReference] private ITargetProvider targetProvider;

    [Tooltip("Predicts the target's future position.")]
    [SerializeReference] private IPositionPredictor positionPredictor;

    [Tooltip("Calculates launch velocity and trajectory peaks.")]
    [SerializeReference] private ILaunchParameterCalculator launchCalculator;

    private void Awake()
    {
        if (targetProvider == null)
            Debug.LogError("PredictiveProjectileSpawner: ITargetProvider is not assigned.");
        if (positionPredictor == null)
            Debug.LogError("PredictiveProjectileSpawner: IPositionPredictor is not assigned.");
        if (launchCalculator == null)
            Debug.LogError("PredictiveProjectileSpawner: ILaunchParameterCalculator is not assigned.");
    }

    /// <summary>
    /// Performs target lookup, prediction, launch calculation, and spawning.
    /// </summary>
    public void LaunchPredictive()
    {
        // 1. Acquire current target
        Transform target = targetProvider.GetTargetTransform();
        if (target == null) return;

        // 2. Estimate future position
        Vector3 currentVel = targetProvider.GetTargetVelocity();
        Vector3 futurePos = positionPredictor.PredictPosition(target, currentVel);

        // 3. Compute launch velocity
        Vector3 displacement = futurePos - spawnPoint.position;
        Vector3 initialVelocity = launchCalculator.CalculateInitialVelocity(displacement);
        SetInitialVelocity(initialVelocity);

        // 4. Place trajectory peak indicator
        Vector3 peakPosition = launchCalculator.CalculateMaxHeightPosition(displacement, spawnPoint.position);
        if (curveMaxHeightTransform != null)
            curveMaxHeightTransform.position = peakPosition;

        // 5. Fire the projectile
        Spawn();
    }
}
