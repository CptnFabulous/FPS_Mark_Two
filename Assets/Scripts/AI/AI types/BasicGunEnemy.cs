using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGunEnemy : Combatant
{
    [Header("Stats")]
    public EngageTargetAtDistance engagementMovement;
    public AimAtTarget targetingStats;




    public StateMachine.TestState firstState;
    public StateMachine.TestState secondState;

    private void Awake()
    {
        //movementStates.AddTransition(firstState, secondState, firstState.TimeLimitExceeded());
        //movementStates.AddTransition(secondState, firstState, secondState.TimeLimitExceeded());

        //movementStates.AddState(firstState, true);
        //movementStates.AddState(secondState);


        movementStates.AddState(engagementMovement, true);
        actionStates.AddState(targetingStats);
    }
}
