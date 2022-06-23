using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ColorPoint))]
public class ColorPointDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);

        var contentPosition = EditorGUI.PrefixLabel(position, label);
        contentPosition.width *= 0.75f;

        EditorGUI.indentLevel = 0;

        EditorGUI.PropertyField(contentPosition, 
            property.FindPropertyRelative("Position"), GUIContent.none);

        contentPosition.x += contentPosition.width;
        contentPosition.width /= 3f;

        EditorGUIUtility.labelWidth = 14f;

        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("Color"),
            new GUIContent("C"));

        EditorGUI.EndProperty();
    }
}
