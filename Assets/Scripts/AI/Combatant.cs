using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combatant : MonoBehaviour
{
    [Header("Target")]
    public Character target;

    [Header("States")]
    public AvoidAttack evasion;
    public PriorityActionController eliminateTarget;
    public PriorityActionController outOfCombat;


    public Idle idleState = new Idle();

    public AI controlling { get; private set; }

    public void Awake()
    {
        controlling = GetComponent<AI>();

        eliminateTarget = new PriorityActionController("Eliminate target");
        outOfCombat = new PriorityActionController("Out of combat");

        SetupLogicPatterns();
    }

    public virtual void SetupLogicPatterns()
    {
        PriorityActionController mainController = new PriorityActionController("Main Controller");
        mainController.AddAction(evasion);
        mainController.AddAction(eliminateTarget, TargetAcquired());
        mainController.AddAction(outOfCombat, null);
        mainController.defaultAction = idleState;

        //controlling.actions.SetBaseAction(mainController);
    }

    /// <summary>
    /// Checks if a target has been found and is not dead.
    /// </summary>
    /// <returns></returns>
    public System.Func<bool> TargetAcquired() => () => target != null && target.health.IsAlive == true;
}
