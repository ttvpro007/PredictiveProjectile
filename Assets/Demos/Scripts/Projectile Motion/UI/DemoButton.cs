using UnityEngine;
using UnityEngine.UI;
using VT.ReusableSystems.Timers;

public class DemoButton : Button
{
    [SerializeField] private Image progressImage;
    [SerializeField] private PredictiveProjectileSpawner thrower;

    private Timer timer;

    protected override void Awake()
    {
        base.Awake();

        timer = Timer.Create();
        UpdateProgress(1f);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        thrower.OnProjectileSpawned -= HandleOnProjectileSpawned;
        thrower.OnProjectileSpawned += HandleOnProjectileSpawned;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        timer?.Stop();
        thrower.OnProjectileSpawned -= HandleOnProjectileSpawned;
    }

    private void HandleOnProjectileSpawned(Projectile projectile)
    {
        DisableAndEnable(projectile.Cooldown + 0.1f);
    }

    public void DisableAndEnable(float enableDelay)
    {
        if (timer != null)
        {
            timer.Stop();

            timer.SetDuration(enableDelay)
                .OnStart(DisableButton)
                .OnUpdate(UpdateProgress)
                .OnComplete(EnableButton)
                .Start();
        }
    }

    private void UpdateProgress(float progress)
    {
        if (progressImage != null)
        {
            progressImage.transform.localScale = new(1f, 1f - progress, 1f);
        }
    }

    private void EnableButton()
    {
        UpdateProgress(1f);
        interactable = true;
    }

    private void DisableButton()
    {
        UpdateProgress(0f);
        interactable = false;
    }
}
