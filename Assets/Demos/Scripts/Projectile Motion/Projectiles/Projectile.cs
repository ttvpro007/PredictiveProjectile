using Obvious.Soap.Example;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Projectile : MonoBehaviour, IDisplayable
{
    [SerializeField, InlineEditor] private GameplayObjectDataSO gameplayObjectData;

    public float Cooldown => cooldown;
    public GameObject UIGameObject => gameplayObjectData.UIGameObject;
    public GameObject GameplayGameObject => gameplayObjectData.GameplayGameObject;
    public string Description => gameplayObjectData.Description;

    [Tooltip("The interval at which this projectile can be spawned, in seconds.")]
    [DisplayField("Cooldown", "Icons/clock_3")]
    [SerializeField, Range(1f, 3f)] protected float cooldown = 1f;

    // Serialized Fields
    [Tooltip("The amount of damage this projectile deals upon impact.")]
    [DisplayField("Hit Damage", "Icons/bullseye")]
    [SerializeField] protected int onHitDamage;

    [Tooltip("The percentage range for damage modifiers.")]
    [MinMaxSlider(-1f, 1f, true)]
    [SerializeField] protected Vector2 modifierPercentage;

    [Tooltip("Prefab representing the point of impact.")]
    [SerializeField] private GameObject hitPointPrefab;

    [Tooltip("Prefab for the visual effect when the projectile hits something.")]
    [SerializeField] private GameObject onHitEffect;

    [Tooltip("Critical hit chance represented as a value between 0 and 1.")]
    [DisplayField("Crit Chance", "Icons/critical")]
    [Range(0f, 1f)]
    [SerializeField] protected float critChance;

    // Protected Fields
    protected Rigidbody rBody;
    protected Collider col;
    protected Transform modelTransform;
    protected TrajectoryDrawer trajectoryDrawer;

    protected Vector3 hitPointPosition;
    protected GameObject hitPointInstance;

    public GameObject SpawnGameplayObject(Transform holder)
    {
        GameObject gameObject = Instantiate(gameplayObjectData.GameplayGameObject, holder);

        gameObject.transform.SetLocalPositionAndRotation(
            gameplayObjectData.SpawnPosition,
            Quaternion.Euler(gameplayObjectData.SpawnRotation)
        );

        return gameObject;
    }

    // Public Methods
    /// <summary>
    /// Sets the hit point position for where the projectile should impact.
    /// </summary>
    /// <param name="hitPointPosition">The position of the hit point.</param>
    public void SetHitPointPosition(Vector3 hitPointPosition)
    {
        this.hitPointPosition = hitPointPosition;
    }

    /// <summary>
    /// Sets the trajectory drawer to visualize the projectile's trajectory.
    /// </summary>
    /// <param name="trajectoryDrawer">Reference to the TrajectoryDrawer component.</param>
    public void SetTrajectoryDrawer(TrajectoryDrawer trajectoryDrawer)
    {
        this.trajectoryDrawer = trajectoryDrawer;
    }

    // Unity Lifecycle Methods
    protected virtual void Awake()
    {
        // Attempt to get or add Rigidbody component
        if (!TryGetComponent(out rBody))
        {
            rBody = gameObject.AddComponent<Rigidbody>();
        }

        // Cache reference to Collider
        col = gameObject.GetComponent<Collider>();

        // Find the child Transform named "Model"
        modelTransform = transform.Find("Model");

        // Find an instance of TrajectoryDrawer in the scene
        trajectoryDrawer = FindAnyObjectByType<TrajectoryDrawer>();
    }

    protected virtual void Start()
    {
        // Instantiate the hit point marker at the calculated hit point position
        if (hitPointPrefab != null)
        {
            hitPointInstance = Instantiate(hitPointPrefab, hitPointPosition, Quaternion.identity);
        }
    }

    // Protected Methods
    protected virtual void Destroy()
    {
        // Cleanup instantiated hit point marker on destroy
        if (hitPointInstance != null)
        {
            Destroy(hitPointInstance);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Calculates the modified damage value, including critical hits and random variance.
    /// </summary>
    /// <param name="damage">The base damage value.</param>
    /// <returns>The calculated damage value.</returns>
    protected float GetCalculatedDamage(float damage)
    {
        // Apply random modifier within percentage range
        return Mathf.Round(damage * 1f + Random.Range(modifierPercentage.x, modifierPercentage.y));
    }

    /// <summary>
    /// Handles collision events to deal damage and trigger visual effects.
    /// </summary>
    /// <param name="collision">The collision information.</param>
    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Instantiate the on-hit visual effect at the projectile's position
        if (onHitEffect != null)
        {
            var onHitEffectInstance = Instantiate(onHitEffect, transform.position, transform.rotation);
            VT.ReusableSystems.Timers.Timer.Create(1f)
                .OnComplete(() => Destroy(onHitEffectInstance))
                .AutoDispose()
                .Start();
        }

        // Apply damage if the collision object has a Health component
        if (collision.gameObject.TryGetComponent<Health>(out var health))
        {
            DealDamage(health, onHitDamage);
        }
    }

    /// <summary>
    /// Deals damage to the target health component.
    /// </summary>
    /// <param name="health">The Health component of the target.</param>
    /// <param name="damage">The base damage to deal.</param>
    protected virtual void DealDamage(Health health, float damage)
    {
        if (Random.Range(0f, 1f) <= critChance)
        {
            // If a critical hit occurs, deal critical damage
            health.TakeCriticalDamage((int)GetCalculatedDamage(damage) * 2);
        }
        else
        {
            health.TakeDamage((int)GetCalculatedDamage(damage));
        }
    }

    protected void DealFlatDamage(Health health, float damage)
    {
        health.TakeDamage((int) damage);
    }

    protected void SetModelActive(bool active)
    {
        if (modelTransform)
        {
            modelTransform.gameObject.SetActive(active);
        }
    }
}

public interface IExplosive
{
    GameObject ExplosionEffect { get; }
    float ExplosionDamage { get; }
    float ExplosionRadius { get; }
    bool HasExploded { get; }

    void Explode();
}

public interface IDisplayable
{
    [System.Serializable]
    public class Displayable
    {
        public Sprite Icon;
        public string Field;
        [ReadOnly] public string Value;
    }

    GameObject UIGameObject { get; }
    GameObject GameplayGameObject { get; }
    string Description { get; }
}