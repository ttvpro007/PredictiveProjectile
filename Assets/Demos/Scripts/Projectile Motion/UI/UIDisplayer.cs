using Sirenix.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDisplayer : MonoBehaviour
{
    // A list to hold any projectile types (Arrow, Grenade, Molotov, etc.)
    [SerializeField] private List<Projectile> Projectiles;
    [SerializeField] private RectTransform ObjectHolderTransform;
    [SerializeField] private TMP_Text DescriptionTextField;
    [SerializeField] private GameObject StatRow;

    public List<GameObject> StatRows = new();

    private List<GameObject> projectilePrefabs = new();
    private readonly Dictionary<Projectile, IReadOnlyCollection<IDisplayable.Displayable>> projectileDisplayFields = new();
    private readonly Dictionary<Projectile, GameObject> runtimeUIGameObjects = new();

    private void UpdateProjectileDisplayFields(Projectile projectile)
    {
        if (ReflectionHelper.CastProjectile<Arrow>(projectile, out var arrow))
        {
            projectile.UpdateDisplayFieldsInfo(arrow);
        }
        else if (ReflectionHelper.CastProjectile<Grenade>(projectile, out var grenade))
        {
            projectile.UpdateDisplayFieldsInfo(grenade);
        }
        else if (ReflectionHelper.CastProjectile<Molotov>(projectile, out var molotov))
        {
            projectile.UpdateDisplayFieldsInfo(molotov);
        }
    }

    public void Init(List<GameObject> projectilePrefabs)
    {
        this.projectilePrefabs = projectilePrefabs;

        // Iterate over each projectile in the list
        for (int i = 0; i < projectilePrefabs.Count; i++)
        {
            if (projectilePrefabs[i].TryGetComponent<Projectile>(out var projectile))
            {
                UpdateProjectileDisplayFields(projectile);

                // Store the display fields in the dictionary for easy access
                projectileDisplayFields[projectile] = projectile.DisplayFields;

                // Store the runtime game objects in the dictionary for easy access
                var runtimeUIGameObject = Instantiate(projectile.UIGameObject, ObjectHolderTransform);
                runtimeUIGameObject.SetActive(false);
                runtimeUIGameObjects[projectile] = runtimeUIGameObject;
            }
        }

        if (!projectilePrefabs.IsNullOrEmpty())
            SwitchTo(0);
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
            && runtimeUIGameObjects.ContainsKey(projectile))
        {
            UpdateDisplayForProjectile(projectile);
        }
    }

    public void UpdateDisplayForProjectile(Projectile projectile)
    {
        if (ReflectionHelper.CastProjectile<Arrow>(projectile, out var arrow))
        {
            SetDisplay(arrow);
        }
        else if (ReflectionHelper.CastProjectile<Grenade>(projectile, out var grenade))
        {
            SetDisplay(grenade);
        }
        else if (ReflectionHelper.CastProjectile<Molotov>(projectile, out var molotov))
        {
            SetDisplay(molotov);
        }
    }

    public void SetDisplay<T>(T projectile) where T : Projectile
    {
        TurnOffAllUIGameObject();
        RemoveAllStatRows();

        runtimeUIGameObjects[projectile].SetActive(true);
        DescriptionTextField.text = projectile.Description;

        foreach (var displayableField in projectileDisplayFields[projectile])
        {
            var statRow = Instantiate(StatRow, transform).GetComponent<StatRow>();
            statRow.IconDisplayer.sprite = displayableField.Icon;
            statRow.LabelField.text = displayableField.Field;
            statRow.ValueField.text = displayableField.Value;
            StatRows.Add(statRow.gameObject);
        }
    }

    private void TurnOffAllUIGameObject()
    {
        foreach (var go in runtimeUIGameObjects.Values)
        {
            go.SetActive(false);
        }
    }

    private void RemoveAllStatRows()
    {
        for (int i = 0; i < StatRows.Count; i++)
        {
            Destroy(StatRows[i]);
        }

        StatRows.Clear();
    }
}
