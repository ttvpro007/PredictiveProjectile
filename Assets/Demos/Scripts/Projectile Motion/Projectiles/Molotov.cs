using Obvious.Soap.Example;
using UnityEngine;

public class Molotov : Projectile, IExplosive
{
    [Tooltip("Prefab for the explosion visual effect.")]
    [SerializeField] private GameObject explosionEffect;

    [Tooltip("Damage dealt by the explosion.")]
    [DisplayField("Explosion Damage", "Icons/boom")]
    [SerializeField] private float explosionDamage = 3f;

    [Tooltip("Radius of the explosion effect.")]
    [DisplayField("Explosion Radius", "Icons/radius")]
    [SerializeField] private float explosionRadius = 3f;

    [Tooltip("Prefab for the burn visual effect.")]
    [SerializeField] private GameObject trailEffect;

    [Tooltip("Prefab for the burn effect that appears after explosion.")]
    [SerializeField] private GameObject burnEffectPrefab;

    [Tooltip("Damage dealt over time by burning.")]
    [DisplayField("Burn Damage", "Icons/fire")]
    [SerializeField] private float burnDamage = 3f;

    [Tooltip("Duration of the burn effect.")]
    [DisplayField("Burn Duration", "Icons/clock")]
    [SerializeField] private float burnDuration = 5f;

    [Tooltip("Interval for applying burn damage.")]
    [DisplayField("Burn Interval", "Icons/clock_2")]
    [SerializeField] private float burnInterval = 1f;

    private bool hasExploded = false;
    private GameObject instantiatedTrailEffect;

    private int maxEffectsCount = 1;
    private int currentEffectsCount = 0;

    // Properties from IExplosive
    public GameObject ExplosionEffect => explosionEffect;
    public float ExplosionDamage => explosionDamage;
    public float ExplosionRadius => explosionRadius;
    public bool HasExploded => hasExploded;

    protected override void Start()
    {
        base.Start();

        if (trailEffect != null)
        {
            instantiatedTrailEffect = Instantiate(trailEffect, transform.position, Quaternion.identity, transform);
        }
    }

    public void Explode()
    {
        // Instantiate explosion visual effect
        if (currentEffectsCount < maxEffectsCount)
        {
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            if (burnEffectPrefab != null)
            {
                if (Instantiate(burnEffectPrefab, transform.position, Quaternion.identity).TryGetComponent<BurnEffectAOE>(out var burnEffect))
                {
                    burnEffect.Configure(burnDuration, burnInterval, explosionRadius, burnDamage);
                    burnEffect.OnBurnEffectComplete += Destroy;
                }
            }

            currentEffectsCount++;
        }

        int maxColliders = 10;
        Collider[] colliders = new Collider[maxColliders];
        // Apply explosion damage to nearby objects and initiate burn effect
        int numberOfCollisions = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, colliders);
        for (int i = 0; i < maxColliders; i++)
        {
            if (colliders[i] == null) continue; // Skip if collider is null or exceeds the number of collisions

            if (colliders[i].TryGetComponent<Health>(out var health))
            {
                DealDamage(health, explosionDamage);
            }
        }

        // Disable the molotov model
        SetModelActive(false);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ignore Collision")) return;

        base.OnCollisionEnter(collision);

        // Stop physics and disable the trail effect
        rBody.isKinematic = true;
        if (col) col.isTrigger = true;

        if (instantiatedTrailEffect != null)
        {
            Destroy(instantiatedTrailEffect);
        }

        if (!hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }
}
