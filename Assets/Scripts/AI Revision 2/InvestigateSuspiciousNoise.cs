using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateSuspiciousNoise : MonoBehaviour
{
    public InvestigateLocations locationSearchState;
    public StateFunction onSearchUnsuccessful;
    public List<DiegeticSound> suspiciousNoises;

    private void Awake()
    {
        locationSearchState.rootAI.hearing.onSoundHeard.AddListener(CheckIfNoiseIsWorthInvestigating);
    }
    void CheckIfNoiseIsWorthInvestigating(HeardSound sound)
    {
        // Check if the sound is suspicious and figure out the priority value
        float priority = suspiciousNoises.FindIndex((s) => s == sound.sound);
        if (priority < 0) return;

        // MAYBE: Also if the position doesn't line up with a known friendly/harmless thing?

        locationSearchState.TrySearchForNewPosition(sound.originPoint, priority, onSearchUnsuccessful);
    }
}