using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIStateFunction : StateFunction
{
    AI _root;
    public AI rootAI => _root ??= GetComponentInParent<AI>();
    public NavMeshAgent navMeshAgent => rootAI.agent;
    public AIAim aim => rootAI.aiming;
    public FieldOfView visionCone => targetManager.visionCone;
    public AITargetManager targetManager => rootAI.targeting;
    public Vector3 standingPosition => navMeshAgent.transform.position;
}
