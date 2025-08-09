using System;
using UnityEngine;

[Serializable]
public class SphereRayGroundChecker : IGroundChecker
{
    [SerializeField] public float groundCheckRadius = 0.2f;
    [SerializeField] public float groundCheckExtra  = 0.05f;

    // temp buffer (small) to avoid allocs when gathering debug overlaps
    private readonly Collider[] _overlapBuf = new Collider[8];

    public GroundCheckResult Check(Transform t, Collider col, LayerMask mask)
    {
        var r = new GroundCheckResult();

        Vector3 origin;
        float downDistance;

        if (col != null)
        {
            var b = col.bounds;
            origin = b.center;
            origin.y = b.min.y + groundCheckRadius + groundCheckExtra;
            downDistance = groundCheckRadius + groundCheckExtra * 2f;
        }
        else
        {
            origin = t.position + Vector3.up * 0.1f;
            downDistance = 0.2f + groundCheckExtra;
        }

        r.sphereCenter = origin;
        r.sphereRadius = groundCheckRadius;
        r.rayOrigin    = origin;
        r.rayLength    = downDistance;

        r.sphereHit = Physics.CheckSphere(origin, groundCheckRadius, mask, QueryTriggerInteraction.Ignore);

        // capture overlaps for debug visualization
        r.overlapCount = Physics.OverlapSphereNonAlloc(origin, groundCheckRadius, _overlapBuf, mask, QueryTriggerInteraction.Ignore);
        r.overlaps     = _overlapBuf;

        r.rayHit   = Physics.Raycast(origin, Vector3.down, out var hit, downDistance, mask, QueryTriggerInteraction.Ignore);
        if (r.rayHit)
        {
            r.hitPoint  = hit.point;
            r.hitNormal = hit.normal;
        }

        r.grounded = r.sphereHit || r.rayHit;
        return r;
    }
}
