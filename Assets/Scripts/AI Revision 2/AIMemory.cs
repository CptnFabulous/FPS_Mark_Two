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
                _allies.AddRange(FindObjectsOfType<AI>(true));
                _allies.RemoveAll(x => x.affiliation != rootAI.affiliation);
            }

            return _allies;
        }
    }








    public bool AreEnoughAlliesInvestigating(Vector3 position, float distanceThreshold, int sufficientAllies, out int allyCount)
    {
        allyCount = 0;

        float squaredThreshold = distanceThreshold * distanceThreshold;
        rootAI.DebugLog($"Checking if any allies are too close. Ally count = {allies.Count}");
        foreach (AI ally in allies)
        {
            if (ally.isActiveAndEnabled == false) continue;
            if (ally.health.IsAlive == false) continue;
            if (ally.agent == null) continue;

            Vector3 desiredAllyPosition = ally.movement.navMeshAgent.destination;

            // TO DO: check if position is in the same general area?

            Vector3 direction = desiredAllyPosition - position;
            float allyDistanceSquared = direction.sqrMagnitude;
            if (allyDistanceSquared >= squaredThreshold) continue;

            allyCount++;
            if (allyCount >= sufficientAllies) return true;
        }

        return false;
    }

}
