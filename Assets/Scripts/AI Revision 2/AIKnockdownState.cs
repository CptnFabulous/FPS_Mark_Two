using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIKnockdownState : AIProcedure
{
    [Header("Stun data")]
    [SerializeField] float duration = 3;
    [SerializeField] CharacterPoise stunHandler;
    [SerializeField] PhysicsAffectedAI aiPhysics;
    [SerializeField] float maxVelocityToStandUp = 0.25f;
    [SerializeField] float maxAngularVelocityToStandUp = 0.25f;

    NavMeshPath existingPath;

    protected override IEnumerator Procedure()
    {
        Debug.Log("Knockdown start");
        existingPath = navMeshAgent.path;
        navMeshAgent.ResetPath();

        // Ragdollise enemy
        aiPhysics.ragdollActive = true;

        // Wait for minimum stun time
        yield return new WaitForSeconds(duration);



        Debug.Log("Waiting until enemy can stand up");
        // Then wait until the character is in a position to stand back up (in case they tumbled off a cliff or something)
        NavMeshHit solidGround = new NavMeshHit();
        yield return new WaitUntil(() => RagdollCanStandUp(out solidGround));

        Vector3 aiUp = rootAI.transform.up;

        // Re-enable regular physics and animations
        aiPhysics.ragdollActive = false;
        // Teleport AI position to sampled position
        rootAI.transform.position = solidGround.position;


        // TO DO: play standing up animation



        // Wait while character stands back up
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