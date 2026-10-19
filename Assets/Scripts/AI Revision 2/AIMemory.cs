using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class containing miscellaneous data that an AI may need to reference, across different states.
/// </summary>
public class AIMemory : MonoBehaviour
{
    [SerializeField] AI rootAI;

    // Co-operation
    [NonSerialized, HideInInspector] List<AI> _allies;

    public List<AI> allies
    {
        get
        {
            if (_allies == null || _allies.Count == 0)
            {
                _allies = new List<AI>();
                _allies.AddRange(FindObjectsOfType<AI>());
                _allies.RemoveAll(x => x.affiliation != rootAI.affiliation);
            }

            return _allies;
        }
    }
}
