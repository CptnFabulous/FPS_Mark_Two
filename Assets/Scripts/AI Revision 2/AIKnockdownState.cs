using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AIKnockdownState : AIProcedure
{
    [Header("Standing up")]
    [SerializeField] float timeBeforeStandingUp = 2;
    [SerializeField] float standUpTime = 2;
    [SerializeField] float maxVelocityToStandUp = 0.25f;
    [SerializeField] float maxAngularVelocityToStandUp = 0.25f;

    [Header("Re-ragdollising")]
    [SerializeField] int stunlockThreshold = 10;

    [Header("Stun data")]
    [SerializeField] CharacterPoise stunHandler;

    public HumanoidAnimator animationHandler;
    public StandUpFromRagdoll ragdollLerpHandler;

    NavMeshPath existingPath;
    bool currentlyStandingUp;

    PhysicsAffectedAI aiPhysics => rootAI.physicsHandler;
    ICharacterLookController lookController => rootAI.lookController;

    void Awake()
    {
        stunHandler.onStunApplied.AddListener(CheckToStunlock);
    }

    protected override IEnumerator Procedure()
    {
        currentlyStandingUp = false;
        if (ragdollLerpHandler != null) ragdollLerpHandler.enabled = false;

        Debug.Log($"{rootAI} ({rootAI.GetInstanceID()}): Knockdown start");
        existingPath = navMeshAgent.path;
        navMeshAgent.ResetPath();

        // Ragdollise enemy
        aiPhysics.ragdollActive = true;
        lookController.active = false;

        #region Wait until it's appropriate to stand up again
        // Wait for minimum stun time
        // Then wait until the character is in a position to stand back up (in case they tumbled off a cliff or something)
        Debug.Log("Waiting until enemy can stand up");
        yield return new WaitForSeconds(timeBeforeStandingUp);
        NavMeshHit solidGround = new NavMeshHit();
        yield return new WaitUntil(() => RagdollCanStandUp(out solidGround));
        #endregion

        Debug.Log("Standing up");

        if (animationHandler != null) animationHandler.RecoverFromRagdoll();

        // Disable ragdoll and realign agent with NavMesh
        aiPhysics.SetPositionWithoutAdjustingRagdoll(solidGround.position);
        yield return aiPhysics.SetRagdollActiveState(false);
        /*
        // This code should theoretically result in more accurate positioning, but it usually results in the ragdoll slightly clipping into the ground at the start of the animation
        yield return aiPhysics.SetRagdollActiveState(false);
        aiPhysics.SetPositionWithoutAdjustingRagdoll(solidGround.position);
        */

        // If lerping code is present, start that up
        if (ragdollLerpHandler != null) ragdollLerpHandler.StartTransition();

        // Reset stun value so the class can check for stunlocks
        Debug.Log($"{rootAI}: resetting stun meter for stunlocking");
        stunHandler.currentStun = 0;

        // Wait while character stands back up
        currentlyStandingUp = true;
        yield return new WaitForSeconds(standUpTime);

        stunHandler.ReturnToNormalFunction();
    }
    protected override void OnDisable()
    {
        Debug.Log($"{rootAI}: exiting knockdown state");
        base.OnDisable();
        EndKnockdown();
    }

    void EndKnockdown()
    {
        if (aiPhysics.ragdollActive)
        {
            aiPhysics.ragdollActive = false;
        }

        currentlyStandingUp = false;
        if (ragdollLerpHandler != null) ragdollLerpHandler.enabled = false;

        if (existingPath != null)
        {
            navMeshAgent.path = existingPath;
            existingPath = null;
        }

        lookController.active = true;
    }
    void CheckToStunlock(DamageMessage dm)
    {
        // Check that the entity is in this state, and trying to stand up from a knockdown
        if (!enabled) return;
        if (!currentlyStandingUp) return;

        // If the enemy was stunned enough while trying to stand up, knock them back down again
        if (stunHandler.currentStun < stunlockThreshold) return;

        Debug.Log($"{rootAI}: resetting knockdown due to stunlock");
        ResetProcedure();
    }
    bool RagdollCanStandUp(out NavMeshHit solidGround)
    {
        solidGround = new NavMeshHit();

        // Check that the ragdoll velocity has slowed enough for the AI to realistically gain control of its momentum
        Ragdoll ragdoll = aiPhysics.ragdoll;
        if (ragdoll.totalVelocity.magnitude > maxVelocityToStandUp) return false;
        if (ragdoll.totalAngularVelocity.magnitude > maxAngularVelocityToStandUp) return false;

        // Check the position of the ragdoll is on solid ground
        Bounds bounds = rootAI.bounds;
        bool onNavMesh = NavMesh.SamplePosition(bounds.center, out solidGround, bounds.extents.magnitude, rootAI.agent.areaMask);
        if (onNavMesh == false) return false;

        return true;
    }
}