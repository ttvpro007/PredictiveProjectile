using UnityEngine;

[CreateAssetMenu(fileName = "New Gameplay Object Data", menuName = "Demo/Gameplay Object Data")]
public class GameplayObjectDataSO : ScriptableObject
{
    [SerializeField] private GameObject uiGameObject;
    [SerializeField] private GameObject gameplayGameObject;
    [SerializeField] private float spawnDelay;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private Vector3 spawnRotation;
    [SerializeField] private string description;
    [SerializeField] private string animationTriggerName;

    public GameObject UIGameObject => uiGameObject;
    public GameObject GameplayGameObject => gameplayGameObject;
    public float SpawnDelay => spawnDelay;
    public Vector3 SpawnPosition => spawnPosition;
    public Vector3 SpawnRotation => spawnRotation;
    public string Description => description;
    public string AnimationTriggerName => animationTriggerName;
}
