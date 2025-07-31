using Obvious.Soap;
using UnityEngine;

[System.Serializable]

/// <summary>
/// Linear position predictor: position + velocity * time.
/// </summary>
public class LinearPositionPredictor : IPositionPredictor
{
    [Header("Prediction Settings")]
    [Tooltip("Time in seconds to predict ahead.")]
    [SerializeField] private FloatVariable timeToPredict;

    /// <inheritdoc />
    public Vector3 PredictPosition(Transform target, Vector3 velocity)
    {
        if (target == null)
        {
            Debug.LogWarning("LinearPositionPredictor: target is null.");
            return Vector3.zero;
        }
        float t = timeToPredict.Value;
        return target.position + velocity * t;
    }
}
