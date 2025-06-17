using CodeRebirthLib.ConfigManagement;
using UnityEditor;
using UnityEngine;

namespace CodeRebirthLib.Editor.PropertyDrawers;

[CustomPropertyDrawer(typeof(CRDynamicConfig))]
public class DynamicConfigDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // Calculate rects for each line.
        Rect settingNameRect = new Rect(position.x, position.y, position.width, lineHeight);
        Rect typeRect = new Rect(position.x, position.y + (lineHeight + spacing), position.width, lineHeight);
        Rect defaultRect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, position.width, lineHeight);
        Rect descRect = new Rect(position.x, position.y + (lineHeight + spacing) * 3, position.width, lineHeight);

        // Draw key and type fields.
        EditorGUI.PropertyField(settingNameRect, property.FindPropertyRelative("settingName"), new GUIContent("settingName"));
        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("DynamicConfigType"), new GUIContent("Type"));

        // Depending on the type, show the correct default value field.
        SerializedProperty dynamicTypeProp = property.FindPropertyRelative("DynamicConfigType");
        CRDynamicConfigType configType = (CRDynamicConfigType)dynamicTypeProp.enumValueIndex;
        switch (configType)
        {
            case CRDynamicConfigType.String:
                EditorGUI.PropertyField(defaultRect, property.FindPropertyRelative("defaultString"), new GUIContent("Default Value"));
                break;
            case CRDynamicConfigType.Int:
                EditorGUI.PropertyField(defaultRect, property.FindPropertyRelative("defaultInt"), new GUIContent("Default Value"));
                break;
            case CRDynamicConfigType.Float:
                EditorGUI.PropertyField(defaultRect, property.FindPropertyRelative("defaultFloat"), new GUIContent("Default Value"));
                break;
            case CRDynamicConfigType.Bool:
                EditorGUI.PropertyField(defaultRect, property.FindPropertyRelative("defaultBool"), new GUIContent("Default Value"));
                break;
        }

        EditorGUI.PropertyField(descRect, property.FindPropertyRelative("Description"), new GUIContent("Description"));
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        // Four lines (Key, Type, Default Value, Description) plus spacing.
        return (lineHeight * 4) + (spacing * 3);
    }
}