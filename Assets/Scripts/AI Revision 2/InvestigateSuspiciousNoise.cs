using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateSuspiciousNoise : MonoBehaviour
{
    public InvestigateLocations locationSearchState;
    //public StateFunction onSearchUnsuccessful;
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


        //locationSearchState.TrySearchForNewPosition(sound.originPoint, priority, onSearchUnsuccessful);

        // MAYBE: Also if the position doesn't line up with a known friendly/harmless thing?
        //locationSearchState.rootAI.DebugLog($"Investigating {sound.sound.name} at {sound.originPoint}");
        /*
        StateFunction previousState = locationSearchState.controller.currentActiveStateInHierarchy;
        Debug.Log($"{locationSearchState.rootAI.name}: Investigating {sound.sound.name} at {sound.originPoint}. Will switch to {previousState.name} if failed");
        */
        locationSearchState.TrySearchForNewPosition(sound.originPoint, priority, false);
    }
}