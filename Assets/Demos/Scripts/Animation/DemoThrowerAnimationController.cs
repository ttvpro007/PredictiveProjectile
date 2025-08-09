using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DemoThrowerAnimationController : MonoBehaviour
{
    [Tooltip("If true, the animation will be triggered when the projectile is shot. If false, it will be triggered when the projectile is spawned.")]
    [SerializeField] private bool animateOnShot = true;

    private PredictiveProjectileSpawner thrower;
    private Animator animator;

    private void Awake()
    {
        thrower = GetComponentInParent<PredictiveProjectileSpawner>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // Subscribe
        if (thrower != null)
        {
            if (animateOnShot)
            {
                thrower.OnProjectileShot -= HandleOnProjectileShot;
                thrower.OnProjectileShot += HandleOnProjectileShot;
            }
            else
            {
                thrower.OnProjectileSpawned -= HandleOnProjectileSpawned;
                thrower.OnProjectileSpawned += HandleOnProjectileSpawned;
            }
        }
    }

    private void OnDisable()
    {
        // Unsubscribe
        if (thrower != null)
        {
            if (animateOnShot)
            {
                thrower.OnProjectileShot -= HandleOnProjectileShot;
            }
            else
            {
                thrower.OnProjectileSpawned -= HandleOnProjectileSpawned;
            }
        }
    }

    private void HandleOnProjectileSpawned(Projectile projectile)
    {
        // Trigger the corresponding animation trigger
        if (animator != null)
            animator.SetTrigger(projectile.AnimationTriggerName);
    }

    private void HandleOnProjectileShot(Projectile projectile)
    {
        // Trigger the corresponding animation trigger
        if (animator != null)
            animator.SetTrigger(projectile.AnimationTriggerName);
    }
}
