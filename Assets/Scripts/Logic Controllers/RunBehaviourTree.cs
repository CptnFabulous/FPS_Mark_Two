using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunBehaviourTree : StateFunction
{
    public BehaviourTree behaviourTree;
    public StateFunction onSuccess;
    public StateFunction onFailure;

    public override IEnumerator AsyncProcedure()
    {
        behaviourTree.enabled = true;

        // Run through the whole behaviour tree, and figure out if the task was a success
        yield return behaviourTree.StartTask(null);
        bool success = behaviourTree.Result();
        // Switch to an appropriate state based on success or failure.
        // Force an override, that way this state can be assigned to one of these fields, to force the state to continue looping if certain criteria if met
        controller.SwitchToState(success ? onSuccess : onFailure, true);
    }
    public override IEnumerator AsyncExit()
    {
        yield return behaviourTree.OnCancelTask();
        behaviourTree.enabled = false;
    }
}