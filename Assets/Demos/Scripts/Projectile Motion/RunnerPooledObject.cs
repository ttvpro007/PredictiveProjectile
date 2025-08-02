using Obvious.Soap.Example;
using UnityEngine;
using VT.Patterns.ObjectPoolPattern;

[RequireComponent(typeof(Health))]
public class RunnerPooledObject : PooledObject
{
    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    public void ReturnToPoolIfHealthAvailable()
    {
        if (_health != null)
        {
            ReturnToPool();
        }
    }
}
