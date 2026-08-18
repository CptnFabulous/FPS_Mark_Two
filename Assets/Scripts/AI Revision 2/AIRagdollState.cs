using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIRagdollState : AIStateFunction
{
    public PuppetmasterRagdollHandler ragdollHandler;
    [SerializeField] CharacterPoise stunHandler;
    [SerializeField] int stunlockThreshold = 10;

    [Header("Initial knockdown")]
    public float minDuration = 2f;

    //NavMeshPath existingPath;
    bool currentlyStandingUp;

    void Awake()
    {
        stunHandler.onStunApplied.AddListener(CheckToStunlock);
    }

    public override IEnumerator AsyncProcedure()
    {
        currentlyStandingUp = false;

        /*
        // TO DO: save current navmesh path?
        existingPath = navMeshAgent.path;
        navMeshAgent.ResetPath();
        */

        // Ragdoll collapses
        yield return ragdollHandler.CollapseAsync();
        // Wait minimum amount of time
        yield return new WaitForSeconds(minDuration - ragdollHandler.collapseTime);

        // Wait until it's right for the ragdoll to stand up
        Vector3 newBasePosition = Vector3.zero;
        Quaternion newBaseRotation = Quaternion.identity;
        bool navMeshPointFound = false;
        NavMeshHit navMeshHit = new NavMeshHit();
        float dot = 0;
        yield return new WaitUntil(() =>
        {
            ragdollHandler.CheckConditionsToStandUp(out newBasePosition, out newBaseRotation, out navMeshPointFound, out navMeshHit, out dot);
            return navMeshPointFound;
        });

        // Stand up
        currentlyStandingUp = true;
        yield return ragdollHandler.StandUpAsync(newBasePosition, newBaseRotation, dot);

        stunHandler.ReturnToNormalFunction();
    }

    void CheckToStunlock(DamageMessage dm)
    {
        // Check that the entity is in this state, and trying to stand up from a knockdown
        if (!enabled) return;
        if (!currentlyStandingUp) return;

        // If the enemy was stunned enough while trying to stand up, knock them back down again
        if (stunHandler.currentStun < stunlockThreshold) return;

        rootAI.DebugLog("Resetting knockdown due to stunlock");
        controller.RestartCurrentState();
    }

    private void OnDisable()
    {
        // TO DO: forcibly switch everything back to normal
        ragdollHandler.ForceAllStandUpValues();
        currentlyStandingUp = false;

        /*
        // TO DO: reapply previous navmesh path?
        if (existingPath != null && navMeshAgent.enabled)
        {
            navMeshAgent.path = existingPath;
            existingPath = null;
        }
        */
    }

}