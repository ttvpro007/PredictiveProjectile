using UnityEngine;

public class CharacterGrabGunIK : MonoBehaviour
{
    [SerializeField] private Transform handIKTarget;
    [SerializeField] private Transform elbowIKTarget;

    public void SetPositionAndRotation(Transform handIKTarget, Transform elbowIKTarget)
    {
        this.handIKTarget?.SetPositionAndRotation(handIKTarget.position, handIKTarget.rotation);
        this.elbowIKTarget?.SetPositionAndRotation(elbowIKTarget.position, elbowIKTarget.rotation);
    }
}
