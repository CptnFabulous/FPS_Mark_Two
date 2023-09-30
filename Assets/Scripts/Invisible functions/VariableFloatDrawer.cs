using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class VariableFloat
{
    [SerializeField] float _baseValue;
    Dictionary<string, float> multipliers = new Dictionary<string, float>();

    //public System.Func<float> multiplierFunctions;

    public float baseValue => _baseValue;
    public float calculatedValue
    {
        get
        {
            float totalMultiplier = 1;
            foreach (var kvp in multipliers)
            {
                totalMultiplier += kvp.Value;
            }

            return baseValue * totalMultiplier;
        }
    }
    public float this[string key]
    {
        get => multipliers[key];
        set => multipliers[key] = value;
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(VariableFloat))]
public class VariableFloatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_baseValue"), label);
    }
}
#endif
