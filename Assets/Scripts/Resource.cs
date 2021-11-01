using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Resource
{
    public Resource(int _max, float _current, float _critical)
    {
        name = "New Resource";
        max = _max;
        current = _current;
        criticalLevel = _critical;
    }

    [HideInInspector] public string name;
    public int max;
    public float current;
    public float criticalLevel;
    bool isCritical
    {
        get
        {
            return current <= criticalLevel;
        }
    }



    public void Change(int amount)
    {
        current += amount;
        current = Mathf.Clamp(current, 0, max);
    }
    public void Change(int amount, out float leftover)
    {
        leftover = 0;
        current += amount;
        if (current > max)
        {
            leftover = current - max;
        }
        else if (current < 0)
        {
            leftover = -current;
        }
        current = Mathf.Clamp(current, 0, max);
    }
}
