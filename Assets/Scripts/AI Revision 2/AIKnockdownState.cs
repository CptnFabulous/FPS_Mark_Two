using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AIKnockdownState : AIProcedure
{
    [SerializeField] CharacterPoise stunHandler;

    [Header("Stun data")]
    [SerializeField] float timeBeforeStandingUp = 2;
    [SerializeField] float standUpTime = 2;
    [SerializeField] float maxVelocityToStandUp = 0.25f;
    [SerializeField] float maxAngularVelocityToStandUp = 0.25f;

    public UnityEvent onKnockdownStart;
    public UnityEvent onStandUpStart;

    NavMeshPath existingPath;

    PhysicsAffectedAI aiPhysics => rootAI.physicsHandler;

    protected override IEnumerator Procedure()
    {
        Debug.Log($"{rootAI} ({rootAI.GetInstanceID()}): Knockdown start");
        existingPath = navMeshAgent.path;
        navMeshAgent.ResetPath();

        // Ragdollise enemy
        onKnockdownStart.Invoke();
        aiPhysics.ragdollActive = true;

        // Wait for minimum stun time
        // Then wait until the character is in a position to stand back up (in case they tumbled off a cliff or something)
        Debug.Log("Waiting until enemy can stand up");
        yield return new WaitForSeconds(timeBeforeStandingUp);
        NavMeshHit solidGround = new NavMeshHit();
        yield return new WaitUntil(() => RagdollCanStandUp(out solidGround));

        Debug.Log("Standing up");
        // Re-enable regular physics and animations
        onStandUpStart.Invoke();
        aiPhysics.ragdollActive = false;
        // Teleport AI position to sampled position (don't disturb ragdoll position)
        aiPhysics.SetPositionWithoutAdjustingRagdoll(solidGround.position);

        // Wait while character stands back up (animation should be triggered through onStandUpStart)
        yield return new WaitForSeconds(standUpTime);

        // TO DO: figure out what happens if the enemy is hit by something that should knock them down, while they're in the 'standing up' animation
        // Alternatively, if the standing up animation is quick enough, I can just give them I-frames and it should be fine.

        navMeshAgent.path = existingPath;
        existingPath = null;

        stunHandler.ReturnToNormalFunction();
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