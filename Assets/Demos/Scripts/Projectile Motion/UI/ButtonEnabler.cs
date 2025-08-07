using UnityEngine;
using UnityEngine.UI;
using VT.ReusableSystems.Timers;

[RequireComponent(typeof(Button))]
public class ButtonEnabler : MonoBehaviour
{
    [SerializeField] private Image progressImage;
    [SerializeField] private PredictiveProjectileSpawner thrower;

    private Button button;
    private Timer timer;

    private void Awake()
    {
        button = GetComponent<Button>();
        timer = Timer.Create();
        UpdateProgress(1f);
    }

    private void OnEnable()
    {
        thrower.OnProjectileSpawned -= Thrower_OnProjectileSpawned;
        thrower.OnProjectileSpawned += Thrower_OnProjectileSpawned;
    }

    private void OnDisable()
    {
        timer?.Stop();
        thrower.OnProjectileSpawned -= Thrower_OnProjectileSpawned;
    }

    private void Thrower_OnProjectileSpawned(Projectile projectile)
    {
        DisableAndEnable(projectile.SpawnInterval + 0.1f);
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

        if (button != null)
        {
            button.interactable = true;
        }
    }

    private void DisableButton()
    {
        UpdateProgress(0f);

        if (button != null)
        {
            button.interactable = false;
        }
    }
}
