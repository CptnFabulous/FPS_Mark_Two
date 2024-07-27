using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AIKnockdownState : AIProcedure
{
    [Header("Stun data")]
    [SerializeField] CharacterPoise stunHandler;
    [SerializeField] float timeBeforeStandingUp = 2;
    [SerializeField] float standUpTime = 2;
    [SerializeField] float maxVelocityToStandUp = 0.25f;
    [SerializeField] float maxAngularVelocityToStandUp = 0.25f;

    public HumanoidAnimator animationHandler;
    public StandUpFromRagdoll ragdollLerpHandler;

    NavMeshPath existingPath;

    PhysicsAffectedAI aiPhysics => rootAI.physicsHandler;
    ICharacterLookController lookController => rootAI.lookController;
    //Animator animator => ragdollLerpHandler.animator.animator;
    protected override IEnumerator Procedure()
    {
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

        // Wait while character stands back up
        // TO DO: figure out what happens if the enemy is hit by something that should knock them down, while they're in the 'standing up' animation
        // Alternatively, if the standing up animation is quick enough, I can just give them I-frames and it should be fine.
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

        if (ragdollLerpHandler != null) ragdollLerpHandler.enabled = false;

        if (existingPath != null)
        {
            navMeshAgent.path = existingPath;
            existingPath = null;
        }

        lookController.active = true;
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