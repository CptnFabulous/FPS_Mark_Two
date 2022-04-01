using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combatant : MonoBehaviour
{
    [Header("Target")]
    public Character target;

    [Header("States")]

    public PriorityActionController eliminateTarget = new PriorityActionController("Eliminate target");
    public PriorityActionController outOfCombat = new PriorityActionController("Out of combat");

    public Idle idleState = new Idle();

    public AI controlling { get; private set; }

    public virtual void Awake()
    {
        controlling = GetComponent<AI>();
        

        PriorityActionController mainController = new PriorityActionController("Main Controller");
        mainController.AddAction(eliminateTarget, TargetAcquired());
        mainController.AddAction(outOfCombat, null);
        mainController.defaultAction = idleState;

        controlling.actions.SetBaseAction(mainController);
    }

    /// <summary>
    /// Checks if a target has been found and is not dead.
    /// </summary>
    /// <returns></returns>
    public System.Func<bool> TargetAcquired() => () => target != null && target.health.IsAlive == true;
}
