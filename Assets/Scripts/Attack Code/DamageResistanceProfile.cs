using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Resistance Profile", menuName = "ScriptableObjects/Damage Resistance Profile")]
public class DamageResistanceProfile : ScriptableObject
{
    [System.Serializable]
    struct DamageMultiplier
    {
        public DamageType type;
        public float multiplier;

        public DamageMultiplier(DamageType type, float multiplier)
        {
            this.type = type;
            this.multiplier = multiplier;
        }
    }

    [SerializeField] List<DamageMultiplier> _multipliers;
    [Tooltip("If enabled, only allow damage specified here. Otherwise, any damage types not specified here will take normal damage")]
    [SerializeField] bool isWhitelist;

    Dictionary<DamageType, float> _md;

    Dictionary<DamageType, float> multipliers
    {
        get
        {
            if (_md != null) return _md;

            // Create a new dictionary and populate with values based on the tuples
            _md = new Dictionary<DamageType, float>();
            foreach (var m in _multipliers) _md[m.type] = m.multiplier;

            return _md;
        }
    }

    public float this[DamageType type] => GetMultiplier(type);

    public float GetMultiplier(DamageType type)
    {
        // If a value is present, use that
        if (multipliers.TryGetValue(type, out float value)) return value;
        // If whitelist, only allow damage if specified
        if (isWhitelist) return 0;
        // If blacklist, inflict normal damage unless specified otherwise
        return 1;
    }
}