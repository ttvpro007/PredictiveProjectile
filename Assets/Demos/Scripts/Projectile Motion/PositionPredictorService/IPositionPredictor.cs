using UnityEngine;

/// <summary>
/// Predicts the future position of a target by simple linear extrapolation.
/// Encapsulates its own time-to-predict configuration.
/// </summary>
public interface IPositionPredictor
{
    /// <summary>
    /// Predicts where the target will be after the configured time, based on current velocity.
    /// </summary>
    /// <param name="target">Transform of the current target.</param>
    /// <param name="velocity">Current velocity of the target.</param>
    Vector3 PredictPosition(Transform target, Vector3 velocity);
}
