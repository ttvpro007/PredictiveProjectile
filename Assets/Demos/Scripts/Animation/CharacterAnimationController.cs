using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [Tooltip("The spawner that fires the OnProjectileSpawned event")]
    [SerializeField] private PredictiveProjectileSpawner thrower;

    [Tooltip("The Animator component responsible for character animations")]
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        // Subscribe safely
        thrower.OnProjectileSpawned -= HandleOnProjectileSpawned;
        thrower.OnProjectileSpawned += HandleOnProjectileSpawned;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        thrower.OnProjectileSpawned -= HandleOnProjectileSpawned;
    }

    private void HandleOnProjectileSpawned(Projectile projectile)
    {
        // Trigger the corresponding animation trigger
        animator.SetTrigger(projectile.AnimationTriggerName);
    }
}
