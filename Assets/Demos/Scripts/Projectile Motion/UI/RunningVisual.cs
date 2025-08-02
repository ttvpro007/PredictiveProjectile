using UnityEngine;

[RequireComponent(typeof(Running))]
public class RunningVisual : MonoBehaviour
{
    [Header("Indicator Prefabs")]
    [Tooltip("Prefab for the direction indicator (e.g., an arrow mesh).")]
    [SerializeField] private GameObject directionIndicatorPrefab;

    [Tooltip("Prefab for the target destination indicator (e.g., a marker or sphere).")]
    [SerializeField] private GameObject targetIndicatorPrefab;

    private Transform directionIndicator;
    private Transform targetIndicator;
    private Running runner;

    private void Awake()
    {
        runner = GetComponent<Running>();
    }

    private void OnEnable()
    {
        // Create direction indicator
        if (directionIndicatorPrefab != null)
        {
            GameObject dirGO = Instantiate(directionIndicatorPrefab);
            directionIndicator = dirGO.transform;
            directionIndicator.SetParent(transform, false);
            directionIndicator.localPosition = Vector3.forward + Vector3.up * 0.1f;
        }

        // Create target indicator
        if (targetIndicatorPrefab != null)
        {
            GameObject tgtGO = Instantiate(targetIndicatorPrefab);
            targetIndicator = tgtGO.transform;
        }

        // Subscribe to destination-changed to update target marker
        runner.OnDestinationChanged += SetTargetIndicator;

        // Initialize target marker position immediately
        if (targetIndicator != null)
            SetTargetIndicator(runner.CurrentDestination);
    }
    
    private void OnDisable()
    {
        // Unsubscribe from destination-changed
        if (runner != null)
            runner.OnDestinationChanged -= SetTargetIndicator;
        // Destroy indicators
        if (directionIndicator != null)
            Destroy(directionIndicator.gameObject);
        if (targetIndicator != null)
            Destroy(targetIndicator.gameObject);
    }

    private void Update()
    {
        // Rotate direction indicator to match current movement
        if (directionIndicator != null)
        {
            Vector3 vel = runner.CurrentVelocity;
            if (vel.sqrMagnitude > 0.01f)
                directionIndicator.forward = vel.normalized;
        }
    }

    private void SetTargetIndicator(Vector3 newPos)
    {
        if (targetIndicator != null)
            targetIndicator.position = newPos;
    }
}
