using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField] private Rig animRig;
    [SerializeField] private CharacterGrabGunIK characterGrabGunIK;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private UIDisplayer UIDisplayer;
    [SerializeField] private List<GameObject> ProjectilePrefabs;

    private PredictiveProjectileSpawner thrower;
    private readonly Dictionary<Projectile, GameObject> RuntimeWeaponGameObject = new();

    private TwoBoneIKConstraint leftArmIKConstraint;

    private void Awake()
    {
        thrower = GetComponent<PredictiveProjectileSpawner>();
        leftArmIKConstraint = animRig.GetRigConstraint<TwoBoneIKConstraint>("LeftArmIK");
    }

    // Start is called before the first frame update
    void Start()
    {
        // Iterate over each projectile in the list
        for (int i = 0; i < ProjectilePrefabs.Count; i++)
        {
            if (ProjectilePrefabs[i].TryGetComponent<Projectile>(out var projectile))
            {
                //Store the runtime game objects in the dictionary for easy access
                var runtimeGameplayGameObject = projectile.SpawnGameplayObject(weaponHolder);
                runtimeGameplayGameObject.SetActive(false);
                RuntimeWeaponGameObject[projectile] = runtimeGameplayGameObject;
            }
        }

        UIDisplayer.Init(ProjectilePrefabs);

        if (!ProjectilePrefabs.IsNullOrEmpty())
            SwitchTo(0);
    }

    private void UpdateDisplayForProjectile(Projectile projectile)
    {
        if (ReflectionHelper.CastBehaviourAs<Projectile, Arrow>(projectile, out var arrow))
        {
            SwitchTo(arrow);
        }
        else if (ReflectionHelper.CastBehaviourAs<Projectile, Grenade>(projectile, out var grenade))
        {
            SwitchTo(grenade);
        }
        else if (ReflectionHelper.CastBehaviourAs<Projectile, Molotov>(projectile, out var molotov))
        {
            SwitchTo(molotov);
        }
    }

    private void SwitchTo<T>(T projectile) where T : Projectile
    {
        if (!RuntimeWeaponGameObject.ContainsKey(projectile)) return;

        TurnOffAllGameplayGameObject();
        GameObject runtimeGO = RuntimeWeaponGameObject[projectile];
        runtimeGO.SetActive(true);

        if (!leftArmIKConstraint)
        {
            Debug.LogWarning("Left Arm IK Constraint is not set in the Rig.");
            return;
        }

        if (runtimeGO.TryGetComponent<GunHolderIK>(out var gunHolderIK))
        {
            if (gunHolderIK.GrabPoint != null && gunHolderIK.ElbowHint != null)
            {
                leftArmIKConstraint.weight = 1;

                // TODO Set the IK target for the ik constraint
                characterGrabGunIK.SetPositionAndRotation(
                    gunHolderIK.GrabPoint,
                    gunHolderIK.ElbowHint);
            }
            else
            {
                leftArmIKConstraint.weight = 0;
            }
        }
    }

    private void TurnOffAllGameplayGameObject()
    {
        foreach (var go in RuntimeWeaponGameObject.Values)
        {
            go.SetActive(false);
        }
    }

    public void SwitchTo(int index)
    {
        if (index < 0 || index >= ProjectilePrefabs.Count)
        {
            Debug.LogWarning("Index out of range for ProjectilePrefabs.");
            return;
        }

        var projectilePrefab = ProjectilePrefabs[index];

        if (projectilePrefab.TryGetComponent(out Projectile projectile)
            && RuntimeWeaponGameObject.ContainsKey(projectile))
        {
            thrower.SetProjectile(projectilePrefab);
            UpdateDisplayForProjectile(projectile);
            UIDisplayer.UpdateDisplayForProjectile(projectile);
        }
    }
}

public static class RigExtension
{
    /// <summary>
    /// Finds a child by name under this Rig and returns its 
    /// constraint component T (if any).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rig"></param>
    /// <param name="childName"></param>
    /// <returns></returns>
    public static T GetRigConstraint<T>(this Rig rig, string childName)
        where T : Component, IRigConstraint
    {
        if (rig == null) return null;
        var child = rig.transform.Find(childName);
        return child ? child.GetComponent<T>() : null;
    }

    // Try get the constraint component of type T from the Rig
    /// <summary>
    /// Try to find a child by name under this Rig and returns its 
    /// constraint component T (if any).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rig"></param>
    /// <param name="childName"></param>
    /// <param name="constraint"></param>
    /// <returns></returns>
    public static bool TryGetRigConstraint<T>(this Rig rig, string childName, out T constraint)
        where T : Component, IRigConstraint
    {
        constraint = rig.GetRigConstraint<T>(childName);
        return constraint != null;
    }
}