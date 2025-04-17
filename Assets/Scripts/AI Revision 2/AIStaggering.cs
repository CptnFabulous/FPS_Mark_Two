using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStaggering : AIStateFunction
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
    public override IEnumerator AsyncProcedure()
    {
        // Disable agent pathing (but save original path)
        existingPath = navMeshAgent.path;
        navMeshAgent.ResetPath();

        yield return new WaitForSeconds(staggerTime);

        // Re-assign path now that stun is complete
        navMeshAgent.path = existingPath;
        existingPath = null;

        // Switch back to the AI's previous state
        stunHandler.ReturnToNormalFunction();
    }
}