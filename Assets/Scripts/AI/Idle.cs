using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : AIMovement
{
    public override void Enter(StateMachine controller)
    {
        base.Enter(controller);
        NavMeshAgent.isStopped = true;
    }

    public override void Exit(StateMachine controller)
    {
        NavMeshAgent.isStopped = false;
    }
}