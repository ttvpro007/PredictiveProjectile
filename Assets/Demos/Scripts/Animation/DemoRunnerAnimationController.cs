using Obvious.Soap.Example;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DemoRunnerAnimationController : MonoBehaviour
{
    private Animator animator;
    private Running runner;
    private Health health;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        runner = GetComponentInParent<Running>();
        health = GetComponentInParent<Health>();
    }

    private void OnEnable()
    {
        if (runner != null)
        {
            runner.OnSpeedChanged -= HandleOnSpeedChanged;
            runner.OnSpeedChanged += HandleOnSpeedChanged;
            runner.OnGroundedChanged -= HandleOnGroundedChanged;
            runner.OnGroundedChanged += HandleOnGroundedChanged;
            runner.OnStunned -= HandleOnStunned;
            runner.OnStunned += HandleOnStunned;
        }

        if (runner != null)
        {
            HandleOnSpeedChanged(runner.Speed.Value);
        }

        if (health != null)
        {
            health.OnDamaged -= HandleOnDamaged;
            health.OnDamaged += HandleOnDamaged;
            health.OnCriticalDamaged -= HandleOnDamaged;
            health.OnCriticalDamaged += HandleOnDamaged;
        }
    }

    private void OnDisable()
    {
        if (runner != null)
        {
            runner.OnSpeedChanged -= HandleOnSpeedChanged;
            runner.OnGroundedChanged -= HandleOnGroundedChanged;
            runner.OnStunned -= HandleOnStunned;
        }

        if (health != null)
        {
            health.OnDamaged -= HandleOnDamaged;
            health.OnCriticalDamaged -= HandleOnDamaged;
        }
    }

    private void HandleOnStunned(bool isStunned)
    {
        if (animator)
        {
            animator.SetBool("IsStunned", isStunned);
        }
    }

    private void HandleOnDamaged(int obj)
    {
        if (animator)
        {
            animator.SetTrigger("Damaged");
        }
    }

    private void HandleOnSpeedChanged(float speed)
    {
        if (animator)
        {
            animator.SetFloat("SpeedNormalized", speed / runner.Speed.Max);
        }
    }

    private void HandleOnGroundedChanged(bool isGrounded)
    {
        if (animator)
        {
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
}
