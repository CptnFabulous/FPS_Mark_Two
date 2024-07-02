using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AIStaggering : AIProcedure
{
    [Header("Stun data")]
    public float staggerTime = 3;
    [SerializeField] CharacterPoise stunHandler;

    NavMeshPath existingPath;
    /*
    private void Awake()
    {
        // Experimental code to automatically set the stun time to match the length of the animation state
        Debug.Log("Setting correct animation length");
        Animator animator = rootAI.animator;

        int layerIndex = animator.GetLayerIndex("Movement");
        AnimatorStateInfo staggerStateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

        staggerTime = staggerStateInfo.length;
    }
    */
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