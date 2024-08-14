using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Resistance Profile", menuName = "ScriptableObjects/Damage Resistance Profile")]
public class DamageResistanceProfile : ScriptableObject
{
    [SerializeField] List<KeyValuePair<DamageType, float>> _multipliers;

    Dictionary<DamageType, float> _md;

    public Dictionary<DamageType, float> multipliers
    {
        get
        {
            if (_md != null) return _md;

            // Create a new dictionary
            _md = new Dictionary<DamageType, float>();
            // Populate with values based on the tuples
            foreach (var m in _multipliers) _md[m.Key] = m.Value;

            return _md;
        }
    }
}