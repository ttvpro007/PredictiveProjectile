using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class TagBasedTargetProvider : ITargetProvider
{
    [SerializeField] private string tag;
    private Transform cachedTransform;

    public Transform GetTargetTransform()
    {
        if (cachedTransform == null)
            FindAndCacheTarget();
        return cachedTransform;
    }

    public Vector3 GetTargetVelocity()
    {
        var target = GetTargetTransform();
        if (target == null)
            return Vector3.zero;

        // Prefer NavMeshAgent velocity
        if (target.TryGetComponent<NavMeshAgent>(out var agent))
            return agent.velocity;

        // Fallback to Rigidbody
        if (target.TryGetComponent<Rigidbody>(out var rb))
            return rb.linearVelocity;

        return Vector3.zero;
    }

    private void FindAndCacheTarget()
    {
        var objs = GameObject.FindGameObjectsWithTag(tag);
        if (objs.Length > 0)
        {
            cachedTransform = objs[0].transform;
        }
        else
        {
            Debug.LogWarning($"TagBasedTargetProvider: No GameObjects found with tag '{tag}'.");
        }
    }
}
