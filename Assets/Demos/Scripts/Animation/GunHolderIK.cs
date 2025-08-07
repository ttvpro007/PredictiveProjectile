using UnityEngine;

public class GunHolderIK : MonoBehaviour
{
    [SerializeField] private Transform otherHandGrabPoint;
    [SerializeField] private Transform otherHandElbowHint;

    public Transform GrabPoint => otherHandGrabPoint;
    public Transform ElbowHint => otherHandElbowHint;
}
