using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGunEnemy : Combatant
{
    [Header("Stats")]
    public EngageTargetAtDistance engagementMovement;
    public AimAtTarget targetingStats;

    

    [Header("Testing stuff")]
    public StateMachine.TestState firstState;
    public StateMachine.TestState secondState;

    public override void Awake()
    {


        base.Awake();
        //movementStates.AddTransition(firstState, secondState, firstState.TimeLimitExceeded());
        //movementStates.AddTransition(secondState, firstState, secondState.TimeLimitExceeded());

        //movementStates.AddState(firstState, true);
        //movementStates.AddState(secondState);

        inCombat.allStates.Add(engagementMovement);
        inCombat.allStates.Add(targetingStats);

    }
}
