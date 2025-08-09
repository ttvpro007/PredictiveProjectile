using System;
using UnityEngine;

[Serializable]
public class GizmosGroundDebugDrawer : IGroundDebugDrawer
{
    [SerializeField] public bool  enabled      = true;
    [SerializeField] public bool  drawInGame   = true;
    [SerializeField] public Color groundedCol  = new Color(0f, 1f, 0f, 0.35f);
    [SerializeField] public Color airborneCol  = new Color(1f, 0f, 0f, 0.35f);
#if UNITY_EDITOR
    [SerializeField] public bool drawOverlapBounds = true;
#endif

    public void Draw(GroundCheckResult r)
    {
        if (!enabled) return;

        var col = r.grounded ? groundedCol : airborneCol;

        Gizmos.color = col;
        Gizmos.DrawWireSphere(r.sphereCenter, r.sphereRadius);
        Gizmos.DrawLine(r.rayOrigin, r.rayOrigin + Vector3.down * r.rayLength);

        if (r.rayHit)
        {
            Gizmos.DrawSphere(r.hitPoint, Mathf.Max(0.025f, r.sphereRadius * 0.2f));
            Gizmos.DrawLine(r.hitPoint, r.hitPoint + r.hitNormal * Mathf.Max(0.1f, r.sphereRadius * 0.6f));
        }

        if (drawInGame)
            Debug.DrawLine(r.rayOrigin, r.rayOrigin + Vector3.down * r.rayLength, col, 0f, false);

#if UNITY_EDITOR
        if (drawOverlapBounds && r.overlaps != null)
        {
            UnityEditor.Handles.color = col;
            for (int i = 0; i < r.overlapCount; i++)
            {
                var c = r.overlaps[i];
                if (!c) continue;
                var b = c.bounds;
                UnityEditor.Handles.DrawWireCube(b.center, b.size);
            }
            UnityEditor.Handles.Label(r.rayOrigin + Vector3.right * 0.1f, r.grounded ? "Grounded" : "Airborne");
        }
#endif
    }
}
