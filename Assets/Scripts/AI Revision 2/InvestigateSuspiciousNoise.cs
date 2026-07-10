using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateSuspiciousNoise : MonoBehaviour
{
    public InvestigateLocations locationSearchState;
    //public StateFunction onSearchUnsuccessful;

    [Header("Sounds")]
    public int basePriority = 0;
    public float soundWaitDuration = 0;
    public List<DiegeticSound> suspiciousNoises;

    [Header("Damage")]
    public float damageWaitDuration = 2;

    AI rootAI => locationSearchState.rootAI;

    private void Awake()
    {
        rootAI.hearing.onSoundHeard.AddListener(CheckIfNoiseIsWorthInvestigating);
        rootAI.health.onDamage.AddListener(RespondToDamage);
    }
    void CheckIfNoiseIsWorthInvestigating(HeardSound sound)
    {
        // Check if the sound is suspicious and figure out the priority value
        float priority = suspiciousNoises.FindIndex((s) => s == sound.sound);
        if (priority < 0) return;

        // Convert value to 0-1 range and add onto base priority.
        // This way sounds can be compared to each other, but all sounds can be grouped together and compared against other suspicious things.
        priority /= suspiciousNoises.Count;
        priority = basePriority + priority;


        // MAYBE: Also if the position doesn't line up with a known friendly/harmless thing?
        //locationSearchState.rootAI.DebugLog($"Investigating {sound.sound.name} at {sound.originPoint}");

        StateFunction previousState = GetPreviousState();
        if (locationSearchState.TrySearchForNewPosition(sound.originPoint, soundWaitDuration, previousState, priority, false, false))
        {
            rootAI.DebugLog($"Investigating {sound.sound.name} at {sound.originPoint}. Will switch to {previousState.name} if failed");
        }
    }

    void RespondToDamage(DamageMessage dm)
    {
        // The enemy won't necessarily know where the damage came from, 
        // Look in the direction the damage came from, and wait a couple of seconds.
        // Go over to investigate that position.

        FieldOfView visionCone = rootAI.visionCone;
        Vector3 directionToLook = -dm.direction;
        float range = visionCone.viewRange;

        Vector3 possibleOrigin;
        if (Physics.Raycast(visionCone.transform.position, directionToLook, out RaycastHit rh, range, visionCone.viewDetection.mask))
        {
            possibleOrigin = rh.point;
        }
        else
        {
            possibleOrigin = range * directionToLook.normalized;
        }

        // Check possible origin of hit
        if (locationSearchState.TrySearchForNewPosition(possibleOrigin, damageWaitDuration, GetPreviousState(), 2, false, true))
        {
            rootAI.DebugLog($"Reacting to damage by {dm.method}");
        }
    }

    StateFunction GetPreviousState() => locationSearchState.controller.currentStateInHierarchy;
}