using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DemoRunnerAnimationController : MonoBehaviour
{
    [SerializeField] private GameObject runnerRuntimeGameObject;

    private Animator animator;
    private Running runner;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        runner = runnerRuntimeGameObject.GetComponent<Running>();
    }

    private void OnEnable()
    {
        if (runner != null)
        {
            runner.OnSpeedChanged -= HandleSpeedChanged;
            runner.OnSpeedChanged += HandleSpeedChanged;
            runner.OnGroundedChanged -= HandleGroundedChanged;
            runner.OnGroundedChanged += HandleGroundedChanged;
        }

        if (runner != null)
        {
            HandleSpeedChanged(runner.Speed.Value);
        }
    }

    private void OnDisable()
    {
        if (runner != null)
        {
            runner.Speed.OnValueChanged -= HandleSpeedChanged;
            runner.OnGroundedChanged -= HandleGroundedChanged;
        }
    }

    private void HandleSpeedChanged(float speed)
    {
        if (runnerRuntimeGameObject && animator)
        {
            animator.SetFloat("SpeedNormalized", speed / runner.Speed.Max);
        }
    }

    private void HandleGroundedChanged(bool isGrounded)
    {
        if (runnerRuntimeGameObject && animator)
        {
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
}
