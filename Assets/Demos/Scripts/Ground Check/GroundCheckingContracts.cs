using UnityEngine;

public interface IGroundChecker
{
    GroundCheckResult Check(Transform subject, Collider subjectCollider, LayerMask groundMask);
}

public interface IGroundDebugDrawer
{
    void Draw(GroundCheckResult r);
}

public struct GroundCheckResult
{
    public bool grounded;

    // probe data
    public Vector3 sphereCenter;
    public float   sphereRadius;
    public Vector3 rayOrigin;
    public float   rayLength;

    // hits
    public bool     sphereHit;
    public bool     rayHit;
    public Vector3  hitPoint;
    public Vector3  hitNormal;

    // optional debug
    public int       overlapCount;
    public Collider[] overlaps; // non-alloc temp from checker
}
