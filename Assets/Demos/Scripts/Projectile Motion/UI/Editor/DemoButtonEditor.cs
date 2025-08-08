// Place this script in an Editor folder!
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(DemoButton))]
[CanEditMultipleObjects]
public class DemoButtonEditor : ButtonEditor
{
    SerializedProperty progressImageProp;
    SerializedProperty throwerProp;

    protected override void OnEnable()
    {
        base.OnEnable();
        progressImageProp = serializedObject.FindProperty("progressImage");
        throwerProp       = serializedObject.FindProperty("thrower");
    }

    public override void OnInspectorGUI()
    {
        // Draw the default Button inspector first:
        base.OnInspectorGUI();

        // Now draw your subclass fields:
        GUILayout.Space(8);
        EditorGUILayout.LabelField("DemoButton Extras", EditorStyles.boldLabel);
        serializedObject.Update();
        EditorGUILayout.PropertyField(progressImageProp);
        EditorGUILayout.PropertyField(throwerProp);
        serializedObject.ApplyModifiedProperties();
    }
}
