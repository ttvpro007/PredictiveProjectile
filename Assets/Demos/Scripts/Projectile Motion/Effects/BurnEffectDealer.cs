using Obvious.Soap.Example;
using System.Collections;
using UnityEngine;

public class BurnEffectDealer : MonoBehaviour
{
    public static BurnEffectDealer Attach(GameObject target, float burnDuration, float burnInterval, float burnDamage)
    {
        if (target == null)
        {
            Debug.LogError("Target GameObject is null. Cannot attach BurnEffectDealer.");
            return null;
        }

        BurnEffectDealer existingDealer = target.GetComponent<BurnEffectDealer>();

        if (existingDealer != null)
        {
            return existingDealer;
        }

        // Create a new BurnEffectDealer component and configure it
        BurnEffectDealer dealer = target.AddComponent<BurnEffectDealer>();
        dealer.Configure(target, burnDuration, burnInterval, burnDamage);
        dealer.Burn();

        return dealer;
    }

    private GameObject target;
    private float burningTimeElapsed;
    private float nextBurnDamageTime;
    private float burnDuration;
    private float burnInterval;
    private float burnDamage;

    private bool configured = false;

    public void Configure(GameObject target, float burnDuration, float burnInterval, float burnDamage)
    {
        this.target = target;
        this.burnDuration = burnDuration;
        this.burnInterval = burnInterval;
        this.burnDamage = burnDamage;

        configured = true;
    }

    public void Burn()
    {
        if (configured)
        {
            StartCoroutine(BurnCycle());
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Destroy(this);
    }

    private IEnumerator BurnCycle()
    {
        if (target == null)
        {
            Debug.LogError("Target GameObject is null. Cannot start burn effect.");
            yield break;
        }

        while (burningTimeElapsed < burnDuration)
        {
            if (nextBurnDamageTime >= burnInterval && target.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage((int) burnDamage);
                nextBurnDamageTime = 0f;
            }

            burningTimeElapsed += Time.deltaTime;
            nextBurnDamageTime += Time.deltaTime;

            Debug.Log($"Applying burn damage to {target.name}. Total time elapsed: {burningTimeElapsed:F2}s");

            yield return null;
        }

        // Destroy the BurnEffectDealer component after the burn duration ends
        if (target != null)
        {
            Destroy(this);
        }
        else
        {
            Debug.LogWarning("Target GameObject is null. Cannot destroy BurnEffectDealer.");
        }
    }
}
