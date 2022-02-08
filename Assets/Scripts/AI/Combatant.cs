using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combatant : AI
{
    [Header("Target")]
    public Character target;

    [Header("While out of combat")]
    public StateMachine.SubStateMachine outOfCombat;
    public Idle idleState;

    public StateMachine.MultiState inCombat;

    public virtual void Awake()
    {
        outOfCombat = new StateMachine.SubStateMachine("Out of combat");
        inCombat = new StateMachine.MultiState("In combat");
        
        outOfCombat.AddState(idleState);

        stateMachine.AddState(outOfCombat, true);
        stateMachine.AddTransition(outOfCombat, inCombat, TargetAcquired(true));

        stateMachine.AddState(inCombat);
        stateMachine.AddTransition(inCombat, outOfCombat, TargetAcquired(false));

    }



    /// <summary>
    /// Checks if a target has been found and is not dead
    /// </summary>
    /// <returns></returns>
    public System.Func<bool> TargetAcquired(bool condition) => () =>
    {
        return (target != null && target.health.IsAlive == true) == condition;
    };
}
