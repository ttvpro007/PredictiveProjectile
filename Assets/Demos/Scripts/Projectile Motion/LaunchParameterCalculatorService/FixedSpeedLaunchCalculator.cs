using Obvious.Soap;
using UnityEngine;

/// <summary>
/// Launch parameter calculator that uses a fixed launch speed to derive
/// the optimal launch angle and initial velocity components.
/// </summary>
[System.Serializable]
public class FixedSpeedLaunchCalculator : ILaunchParameterCalculator
{
    [SerializeField] private FloatVariable fixedLaunchSpeed;
    [SerializeField] private float yOffset;
    [SerializeField] private float angleStep;
    [SerializeField] private float tolerance;

    public Vector3 CalculateInitialVelocity(Vector3 displacement)
    {
        float g = Mathf.Abs(Physics.gravity.y);
        float v0 = fixedLaunchSpeed.Value;

        // Horizontal plane distance
        float x = new Vector2(displacement.x, displacement.z).magnitude;
        // Vertical displacement including offset
        float y = displacement.y + yOffset;

        // Maximum possible range at 45°
        float maxRange = (v0 * v0 * Mathf.Sin(2f * 45f * Mathf.Deg2Rad)) / g;
        if (x > maxRange)
        {
            Debug.LogWarning($"Target out of range: horizontal {x:F2}m > max {maxRange:F2}m.");
            // Fire horizontally at max range direction
            Vector3 dir = new Vector3(displacement.x, 0f, displacement.z).normalized;
            return dir * v0;
        }

        bool found = false;
        float bestTheta = 0f;

        // Search for a viable launch angle
        for (float deg = 0f; deg <= 90f; deg += angleStep)
        {
            float theta = deg * Mathf.Deg2Rad;
            float v0x = v0 * Mathf.Cos(theta);
            float v0y = v0 * Mathf.Sin(theta);

            // Time based on horizontal component
            if (v0x < 1e-3f) continue;
            float t = x / v0x;

            // Predicted vertical displacement
            float yCalc = v0y * t - 0.5f * g * t * t;
            if (Mathf.Abs(yCalc - y) <= tolerance)
            {
                found = true;
                bestTheta = theta;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning("No suitable launch angle found; using 45° fallback.");
            bestTheta = 45f * Mathf.Deg2Rad;
        }

        // Build initial velocity vector
        float finalV0x = v0 * Mathf.Cos(bestTheta);
        float finalV0y = v0 * Mathf.Sin(bestTheta);
        Vector3 horizDir = new Vector3(displacement.x, 0f, displacement.z).normalized;
        return horizDir * finalV0x + Vector3.up * finalV0y;
    }

    public Vector3 CalculateMaxHeightPosition(Vector3 displacement, Vector3 spawnPosition)
    {
        // Use the same velocity to compute peak
        Vector3 v0 = CalculateInitialVelocity(displacement);
        float g = Mathf.Abs(Physics.gravity.y);
        float v0y = v0.y;

        // Peak height relative to spawn
        float hMax = (v0y * v0y) / (2f * g);
        float tPeak = v0y / g;

        // Horizontal travel to peak
        Vector3 horizDir = new Vector3(displacement.x, 0f, displacement.z).normalized;
        float horizSpeed = new Vector2(v0.x, v0.z).magnitude;
        float horizDist = horizSpeed * tPeak;

        Vector3 peak = spawnPosition + horizDir * horizDist;
        peak.y = spawnPosition.y + hMax;
        return peak;
    }
}
