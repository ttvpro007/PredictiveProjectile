using System.Collections.Generic;
using UnityEngine;

public abstract class TargetProvider
{
    /// <summary>Return a target Transform from the collection, or null if none.</summary>
    protected abstract Transform IssueTarget(ICollection<GameObject> targets);

    public Transform GetTarget(ICollection<GameObject> targets)
    {
        if (targets == null || targets.Count == 0) return null;
        return IssueTarget(targets);
    }
}

public class FirstTargetProvider : TargetProvider
{
    protected override Transform IssueTarget(ICollection<GameObject> targets)
    {
        foreach (var go in targets)
        {
            if (go) return go.transform;
        }
        return null;
    }
}

public class RandomTargetProvider : TargetProvider
{
    protected override Transform IssueTarget(ICollection<GameObject> targets)
    {
        Transform choice = null;
        int seen = 0;

        foreach (var go in targets)
        {
            if (go == null) continue;

            // reservoir sampling: replace current choice with probability 1/seen
            seen++;
            if (Random.Range(0, seen) == 0)
                choice = go.transform;
        }

        return choice; // may be null if all entries were null/inactive
    }
}

public class NearestTargetProvider : TargetProvider
{
    public Transform referencePoint; // where to measure distance from

    protected override Transform IssueTarget(ICollection<GameObject> targets)
    {
        Transform nearest = null;
        float minDistance = float.MaxValue;
        foreach (var go in targets)
        {
            if (go == null) continue;
            float distance = Vector3.Distance(go.transform.position, referencePoint.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = go.transform;
            }
        }
        return nearest; // may be null if all entries were null/inactive
    }
}

public class FarthestTargetProvider : TargetProvider
{
    public Transform referencePoint; // where to measure distance from

    protected override Transform IssueTarget(ICollection<GameObject> targets)
    {
        Transform nearest = null;
        float maxDistance = float.MinValue;
        foreach (var go in targets)
        {
            if (go == null) continue;
            float distance = Vector3.Distance(go.transform.position, referencePoint.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                nearest = go.transform;
            }
        }
        return nearest; // may be null if all entries were null/inactive
    }
}