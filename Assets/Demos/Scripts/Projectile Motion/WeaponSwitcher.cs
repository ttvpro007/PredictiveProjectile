using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField] private Rig animRig;
    [SerializeField] private CharacterGrabGunIK characterGrabGunIK;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private UIDisplayer uiDisplayer;
    [SerializeField] private List<GameObject> projectilePrefabs;

    public event Action<Projectile> OnProjectileSwitched;

    private readonly Dictionary<Projectile, GameObject> runtimeWeaponGameObject = new();
    public IReadOnlyDictionary<Projectile, GameObject> RuntimeWeaponGameObjects => runtimeWeaponGameObject;

    private MultiAimConstraint aimIKConstraint;
    private TwoBoneIKConstraint leftArmIKConstraint;
    private DampedTransform dampedHandAimSpine3IKConstraint;

    private void Awake()
    {
        aimIKConstraint = animRig.GetRigConstraint<MultiAimConstraint>("Aim");
        leftArmIKConstraint = animRig.GetRigConstraint<TwoBoneIKConstraint>("LeftArmIK");
        dampedHandAimSpine3IKConstraint = animRig.GetRigConstraint<DampedTransform>("DampedHandAimSpine3");
    }

    private void OnEnable()
    {
        OnProjectileSwitched -= HandleProjectileSwitched;
        OnProjectileSwitched += HandleProjectileSwitched;
    }

    private void OnDisable()
    {
        OnProjectileSwitched -= HandleProjectileSwitched;
    }

    private void HandleProjectileSwitched(Projectile projectile)
    {
        UpdateDisplayForProjectile(projectile);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Iterate over each projectile in the list
        for (int i = 0; i < projectilePrefabs.Count; i++)
        {
            if (projectilePrefabs[i].TryGetComponent<Projectile>(out var projectile))
            {
                //Store the runtime game objects in the dictionary for easy access
                var runtimeGameplayGameObject = projectile.SpawnGameplayObject(weaponHolder);
                runtimeGameplayGameObject.SetActive(false);
                runtimeWeaponGameObject[projectile] = runtimeGameplayGameObject;
            }
        }

        uiDisplayer.Init(projectilePrefabs);

        if (!projectilePrefabs.IsNullOrEmpty())
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

    private void SetIKConstraintWeight(float weight)
    {
        weight = Mathf.Clamp01(weight);

        if (aimIKConstraint != null)
        {
            aimIKConstraint.weight = weight;
        }
        else
        {
            Debug.LogWarning("Aim IK Constraint is not set in the Rig.");
        }

        if (leftArmIKConstraint != null)
        {
            leftArmIKConstraint.weight = weight;
        }
        else
        {
            Debug.LogWarning("Left Arm IK Constraint is not set in the Rig.");
        }

        if (dampedHandAimSpine3IKConstraint != null)
        {
            dampedHandAimSpine3IKConstraint.weight = weight;
        }
        else
        {
            Debug.LogWarning("Damped Hand Aim Spine3 IK Constraint is not set in the Rig.");
        }
    }

    private void SwitchTo<T>(T projectile) where T : Projectile
    {
        if (!runtimeWeaponGameObject.ContainsKey(projectile)) return;

        TurnOffAllGameplayGameObject();
        GameObject runtimeGO = runtimeWeaponGameObject[projectile];
        runtimeGO.SetActive(true);

        if (runtimeGO.TryGetComponent<GunHolderIK>(out var gunHolderIK))
        {
            if (gunHolderIK.GrabPoint != null && gunHolderIK.ElbowHint != null)
            {
                SetIKConstraintWeight(1f);

                // TODO Set the IK target for the ik constraint
                characterGrabGunIK.SetPositionAndRotation(
                    gunHolderIK.GrabPoint,
                    gunHolderIK.ElbowHint);
            }
            else
            {
                SetIKConstraintWeight(0f);
            }
        }
    }

    private void TurnOffAllGameplayGameObject()
    {
        foreach (var go in runtimeWeaponGameObject.Values)
        {
            go.SetActive(false);
        }
    }

    public void SwitchTo(int index)
    {
        if (index < 0 || index >= projectilePrefabs.Count)
        {
            Debug.LogWarning("Index out of range for ProjectilePrefabs.");
            return;
        }

        var projectilePrefab = projectilePrefabs[index];

        if (projectilePrefab.TryGetComponent(out Projectile projectile)
            && runtimeWeaponGameObject.ContainsKey(projectile))
        {
            OnProjectileSwitched?.Invoke(projectile);

            //thrower.SetProjectile(projectilePrefab);
            //UpdateDisplayForProjectile(projectile);
            //uiDisplayer.UpdateDisplayForProjectile(projectile);
        }
    }

    public Weapon GetWeapon(Projectile projectile)
    {
        if (runtimeWeaponGameObject.TryGetValue(projectile, out var weaponGO))
        {
            return weaponGO.GetComponent<Weapon>();
        }
        return null;
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