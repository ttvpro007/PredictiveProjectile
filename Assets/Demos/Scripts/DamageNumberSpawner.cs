using UnityEngine;
using DamageNumbersPro;
using Obvious.Soap.Example;

public class DamageNumberSpawner : MonoBehaviour
{
    //Assign prefab in inspector.
    [SerializeField] private DamageNumber numberPrefab;
    [SerializeField] private DamageNumber critNumberPrefab;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += SpawnNumber;
            health.OnCriticalDamaged += SpawnCritNumber;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= SpawnNumber;
            health.OnCriticalDamaged -= SpawnCritNumber;
        }
    }

    public void SpawnNumber(int value)
    {
        //Spawn new popup at transform.position.
        numberPrefab?.Spawn(transform.position, value);
    }

    public void SpawnCritNumber(int value)
    {
        //Spawn new popup at transform.position.
        critNumberPrefab?.Spawn(transform.position, value);
    }
}