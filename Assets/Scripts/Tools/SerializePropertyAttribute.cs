using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tools
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializePropertyAttribute : PropertyAttribute
    {
        public string PropertyName { get; private set; }

        public SerializePropertyAttribute(string name)
        {
            PropertyName = name;
        }

        public SerializePropertyAttribute()
        {
            PropertyName = null;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializePropertyAttribute))]
    public class SerializePropertyAttributeDrawer : PropertyDrawer
    {
        private PropertyInfo _propertyFieldInfo = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var target = property.serializedObject.targetObject;

            var propertyName = ((Tools.SerializePropertyAttribute)attribute).PropertyName;
            if (string.IsNullOrEmpty(propertyName))
            {
                propertyName = property.name;
                if (propertyName.StartsWith("_"))
                    propertyName = propertyName[1..];
                propertyName = char.ToUpper(propertyName[0]) + propertyName[1..];
            }

            // Find the property field using reflection, in order to get access to its getter/setter.
            if (_propertyFieldInfo == null)
                _propertyFieldInfo = target.GetType().GetProperty(propertyName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (_propertyFieldInfo != null)
            {
                // Retrieve the value using the property getter:
                var value = _propertyFieldInfo.GetValue(target, null);

                // Draw the property, checking for changes:
                EditorGUI.BeginChangeCheck();
                value = DrawProperty(position, property.propertyType, _propertyFieldInfo.PropertyType, value, label);

                // If any changes were detected, call the property setter:
                if (!EditorGUI.EndChangeCheck() || _propertyFieldInfo == null) return;
                // Record object state for undo:
                Undo.RecordObject(target, "Inspector");

                // Call property setter:
                _propertyFieldInfo.SetValue(target, value, null);
            }
            else
            {
                EditorGUI.LabelField(position, "Error: could not retrieve property.");
            }
        }

        private object DrawProperty(Rect position, SerializedPropertyType propertyType, Type type, object value,
            GUIContent label)
        {
            return propertyType switch
            {
                SerializedPropertyType.Integer => EditorGUI.IntField(position, label, (int)value),
                SerializedPropertyType.Boolean => EditorGUI.Toggle(position, label, (bool)value),
                SerializedPropertyType.Float => EditorGUI.FloatField(position, label, (float)value),
                SerializedPropertyType.String => EditorGUI.TextField(position, label, (string)value),
                SerializedPropertyType.Color => EditorGUI.ColorField(position, label, (Color)value),
                SerializedPropertyType.ObjectReference or SerializedPropertyType.ExposedReference =>
                    EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, type, true),
                SerializedPropertyType.LayerMask => EditorGUI.LayerField(position, label, (int)value),
                SerializedPropertyType.Enum => EditorGUI.EnumPopup(position, label, (Enum)value),
                SerializedPropertyType.Vector2 => EditorGUI.Vector2Field(position, label, (Vector2)value),
                SerializedPropertyType.Vector3 => EditorGUI.Vector3Field(position, label, (Vector3)value),
                SerializedPropertyType.Vector4 => EditorGUI.Vector4Field(position, label, (Vector4)value),
                SerializedPropertyType.Rect => EditorGUI.RectField(position, label, (Rect)value),
                SerializedPropertyType.AnimationCurve => EditorGUI.CurveField(position, label, (AnimationCurve)value),
                SerializedPropertyType.Bounds => EditorGUI.BoundsField(position, label, (Bounds)value),
                _ => throw new NotImplementedException("Unimplemented propertyType " + propertyType + ".")
            };
        }
    }
}
#endif