using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EditableMatrix))]
public class MatrixDrawer : PropertyDrawer
{
    private const float LabelHeight = 18f;
    private const float Padding = 4f;
    private const float CellWidth = 35f;
    private const float CellHeight = 20f;
    private const float MatrixSpacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, LabelHeight), property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float currentY = position.y + LabelHeight + Padding;

            SerializedProperty widthProp = property.FindPropertyRelative("width");
            SerializedProperty heightProp = property.FindPropertyRelative("height");
            SerializedProperty rowsProp = property.FindPropertyRelative("rows");

            EditorGUI.BeginChangeCheck();

            Rect widthRect = new Rect(position.x, currentY, position.width * 0.5f - Padding, LabelHeight);
            widthProp.intValue = EditorGUI.IntField(widthRect, "Width", widthProp.intValue);
            widthProp.intValue = Mathf.Max(1, widthProp.intValue);

            Rect heightRect = new Rect(position.x + position.width * 0.5f + Padding, currentY, position.width * 0.5f - Padding, LabelHeight);
            heightProp.intValue = EditorGUI.IntField(heightRect, "Height", heightProp.intValue);
            heightProp.intValue = Mathf.Max(1, heightProp.intValue);

            currentY += LabelHeight + Padding;

            if (EditorGUI.EndChangeCheck())
            {
                EditableMatrix matrix = fieldInfo.GetValue(property.serializedObject.targetObject) as EditableMatrix;
                if (matrix != null)
                {
                    matrix.Resize(widthProp.intValue, heightProp.intValue);
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }

            int width = widthProp.intValue;
            int height = heightProp.intValue;

            for (int r = 0; r < height; r++)
            {
                SerializedProperty rowProp = rowsProp.GetArrayElementAtIndex(r);
                SerializedProperty valuesProp = rowProp.FindPropertyRelative("values");

                if (valuesProp.arraySize != width)
                {
                    valuesProp.arraySize = width;
                }

                for (int c = 0; c < width; c++)
                {
                    SerializedProperty valueProp = valuesProp.GetArrayElementAtIndex(c);

                    Rect cellRect = new Rect(
                        position.x + c * (CellWidth + MatrixSpacing),
                        currentY + r * (CellHeight + MatrixSpacing),
                        CellWidth,
                        CellHeight
                    );

                    valueProp.intValue = EditorGUI.IntField(cellRect, GUIContent.none, valueProp.intValue);
                }
            }

            currentY += height * (CellHeight + MatrixSpacing) + Padding;

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = LabelHeight + Padding;

        if (property.isExpanded)
        {
            SerializedProperty heightProp = property.FindPropertyRelative("height");
            int height = heightProp != null ? Mathf.Max(1, heightProp.intValue) : 1;

            totalHeight += LabelHeight + Padding;

            totalHeight += height * (CellHeight + MatrixSpacing) + Padding;
        }

        return totalHeight;
    }
}