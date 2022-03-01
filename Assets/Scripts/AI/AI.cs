using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : Character
{
    public override Vector3 LookOrigin => aiming.LookOrigin;
    public override LayerMask lookMask => view.viewDetection;
    public override LayerMask attackMask => aiming.Stats.lookDetection;

    [Header("AI-specific components")]
    public NavMeshAgent agent;
    public AIAim aiming;
    public FieldOfView view;

    [Header("Behaviours")]
    public StateMachine stateMachine;
}
