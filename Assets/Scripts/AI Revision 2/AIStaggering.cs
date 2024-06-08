using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStaggering : AIProcedure
{
    [Header("Stun data")]
    [SerializeField] float staggerTime = 3;
    [SerializeField] CharacterPoise stunHandler;

    NavMeshPath existingPath;

    protected override IEnumerator Procedure()
    {
        // Ensure this code switches to the AI's previous state
        toSwitchToOnEnd = controller.previousState;

        // Disable agent pathing (but save original path)
        existingPath = navMeshAgent.path;
        navMeshAgent.ResetPath();

        yield return new WaitForSeconds(staggerTime);

        // Re-assign path now that stun is complete
        navMeshAgent.path = existingPath;
        existingPath = null;

        stunHandler.ReturnToNormalFunction();
    }
}