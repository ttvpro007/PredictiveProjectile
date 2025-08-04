using Obvious.Soap.Example;
using UnityEngine;
using VT.Patterns.ObjectPoolPattern;

[RequireComponent(typeof(Health))]
public class RunnerPooledObject : PooledObject
{
    private Health _health;
    private CameraController _cameraController;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _cameraController = Camera.main.GetComponent<CameraController>();
    }

    public override void OnSpawned()
    {
        base.OnSpawned();

        _health.ResetHealth();
        _health.OnDeath += ReturnToPool;
        _cameraController?.AddPointToTrack(transform);
    }

    public override void OnReturned()
    {
        base.OnReturned();

        _health.OnDeath -= ReturnToPool;
        _cameraController?.RemovePointToTrack(transform);
    }
}
