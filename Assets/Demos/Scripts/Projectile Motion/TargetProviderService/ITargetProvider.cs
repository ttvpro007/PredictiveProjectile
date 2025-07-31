using UnityEngine;

/// <summary>
/// Provides a target by finding the first GameObject with a given tag.
/// </summary>
public interface ITargetProvider
{
    /// <summary>
    /// Returns the Transform of the current target, or null if none found.
    /// </summary>
    Transform GetTargetTransform();

    /// <summary>
    /// Returns the current velocity of the target (NavMeshAgent or Rigidbody), or zero if unavailable.
    /// </summary>
    Vector3 GetTargetVelocity();
}
