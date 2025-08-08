using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class DisplayFieldAttribute : Attribute
{
    /// <summary>
    /// What shows up in the UI (emoji, units, etc).
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// A Resources folder path (or Addressables key) for the Sprite.
    /// E.g. "Icons/DamageIcon" if you have Assets/Resources/Icons/DamageIcon.png
    /// </summary>
    public string IconPath { get; }

    public DisplayFieldAttribute(string label, string iconPath = null)
    {
        this.Label = label;
        this.IconPath = iconPath;
    }
}