using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Transform projectileSpawnPoint;
    public Transform ProjectileSpawnPoint => projectileSpawnPoint;
}
