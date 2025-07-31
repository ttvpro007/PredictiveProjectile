using Obvious.Soap;
using UnityEngine;

/// <summary>
/// Launch parameter calculator that uses a fixed time-to-impact to derive
/// both horizontal and vertical velocity components.
/// </summary>
[System.Serializable]
public class TimeToImpactLaunchCalculator : ILaunchParameterCalculator
{
    [SerializeField] private FloatVariable timeToImpact;
    [SerializeField] private float yOffset;

    public Vector3 CalculateInitialVelocity(Vector3 displacement)
    {
        float t = timeToImpact.Value;
        float g = Mathf.Abs(Physics.gravity.y);

        // Horizontal distance on XZ plane
        float x = new Vector2(displacement.x, displacement.z).magnitude;
        // Vertical displacement including offset
        float y = displacement.y + yOffset;

        // Compute horizontal and vertical components
        float v0x = x / t;
        float v0y = (y + 0.5f * g * t * t) / t;

        Vector3 horizontalDir = new Vector3(displacement.x, 0f, displacement.z).normalized;
        return horizontalDir * v0x + Vector3.up * v0y;
    }

    public Vector3 CalculateMaxHeightPosition(Vector3 displacement, Vector3 spawnPosition)
    {
        // Reuse initial velocity to compute peak
        Vector3 v0 = CalculateInitialVelocity(displacement);
        float v0y = v0.y;
        float g = Mathf.Abs(Physics.gravity.y);

        // Peak height relative to spawn
        float hMax = (v0y * v0y) / (2f * g);
        float tToPeak = v0y / g;

        // Horizontal travel to peak
        Vector3 horizontalDir = new Vector3(displacement.x, 0f, displacement.z).normalized;
        float horizontalSpeed = new Vector2(v0.x, v0.z).magnitude;
        float horizontalDistance = horizontalSpeed * tToPeak;

        Vector3 peakPos = spawnPosition + horizontalDir * horizontalDistance;
        peakPos.y = spawnPosition.y + hMax;
        return peakPos;
    }
}
