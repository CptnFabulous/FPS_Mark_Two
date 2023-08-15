using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CptnFabulous.StateMachines;

public abstract class AIState : State
{
    public AI rootAI => ai ??= controller.GetComponent<AI>();
    public NavMeshAgent navMeshAgent => rootAI.agent;
    public AIAim aim => rootAI.aiming;

    AI ai;
}