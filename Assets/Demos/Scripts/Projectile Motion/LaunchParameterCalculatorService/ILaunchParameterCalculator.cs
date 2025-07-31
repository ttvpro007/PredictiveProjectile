using UnityEngine;

/// <summary>
/// Defines methods for calculating projectile launch parameters.
/// </summary>
public interface ILaunchParameterCalculator
{
    /// <summary>
    /// Calculates the initial velocity vector needed to hit a target based on displacement.
    /// </summary>
    Vector3 CalculateInitialVelocity(Vector3 displacement);

    /// <summary>
    /// Calculates the world position at maximum height for trajectory visualization.
    /// </summary>
    Vector3 CalculateMaxHeightPosition(Vector3 displacement, Vector3 spawnPosition);
}
