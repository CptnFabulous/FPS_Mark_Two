using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGunEnemy : Combatant
{
    [Header("Basic attack stats")]
    MultiAction combatActions = new MultiAction("Attack target");
    public EngageTargetAtDistance engagementMovement;
    public ExecuteAttack attackStats;

    public override void Awake()
    {
        
        combatActions.allActions.Add(engagementMovement);
        combatActions.allActions.Add(attackStats);

        // Change this to be an actual state with conditions (e.g. if exact position is known)
        eliminateTarget.defaultAction = combatActions;


        base.Awake();
    }
}
