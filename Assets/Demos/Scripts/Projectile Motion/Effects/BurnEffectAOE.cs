using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BurnEffectAOE : MonoBehaviour
{
    [Tooltip("Prefab for the burn visual effect.")]
    [SerializeField] private GameObject burnEffect;

    public event Action OnBurnEffectComplete;

    private float burningTimeElapsed = 0f;
    private float burnDuration;
    private float burnInterval;
    private float burnRadius;
    private float burnDamage;

    private SphereCollider sphereCollider;

    public void Configure(float burnDuration, float burnInterval, float burnRadius, float burnDamage)
    {
        this.burnDuration = burnDuration;
        this.burnInterval = burnInterval;
        this.burnRadius = burnRadius;
        this.burnDamage = burnDamage;
    }

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
            sphereCollider.radius = burnRadius;
        }

        StartCoroutine(Burn());
    }

    private IEnumerator Burn()
    {
        while (burningTimeElapsed < burnDuration)
        {
            burningTimeElapsed += Time.deltaTime;
            yield return null;
        }

        OnBurnEffectComplete?.Invoke();

        // Destroy game object
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        BurnEffectDealer.Attach(other.gameObject, burnDuration, burnInterval, burnDamage);
    }
}
