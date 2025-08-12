using UnityEngine;
using DamageNumbersPro;
using Obvious.Soap.Example;

public class DamageNumberSpawner : MonoBehaviour
{
    //Assign prefab in inspector.
    [SerializeField] private DamageNumber numberPrefab;
    [SerializeField] private DamageNumber critNumberPrefab;
    [SerializeField] private DamageNumber textPrefab;

    private Health health;
    private Running runner;

    private void Awake()
    {
        health = GetComponent<Health>();
        runner = GetComponent<Running>();
    }

    private void OnEnable()
    {
        RegisterEvents();
    }

    private void OnDisable()
    {
        UnregisterEvents();
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

    public void SpawnWord(string word)
    {
        //Spawn new popup at transform.position.
        textPrefab?.Spawn(transform.position, word);
    }

    private void RegisterEvents()
    {
        if (health != null)
        {
            health.OnDamaged -= SpawnNumber;
            health.OnDamaged += SpawnNumber;
            health.OnCriticalDamaged -= SpawnCritNumber;
            health.OnCriticalDamaged += SpawnCritNumber;
        }

        if (runner != null)
        {
            runner.OnStunned += HandleOnStnned;
        }
    }

    private void UnregisterEvents()
    {
        if (health != null)
        {
            health.OnDamaged -= SpawnNumber;
            health.OnCriticalDamaged -= SpawnCritNumber;
        }

        if (runner != null)
        {
            runner.OnStunned -= HandleOnStnned;
        }
    }

    private void HandleOnStnned(bool isStunned)
    {
        if (isStunned)
        {
            SpawnWord("Stunned");
        }
    }
}