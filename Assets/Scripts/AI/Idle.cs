using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : AIMovement
{
    public Idle()
    {
        name = "Idle";
    }

    public override void Enter()
    {
        base.Enter();
        NavMeshAgent.isStopped = true;
    }
    public override void Exit()
    {
        NavMeshAgent.isStopped = false;
    }
}