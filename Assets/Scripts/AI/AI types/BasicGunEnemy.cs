using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGunEnemy : Combatant
{
    [Header("Basic attack stats")]
    public EngageTargetAtDistance engagementMovement;
    public ExecuteAttack attackStats;

    MultiAction combatActions;

    public override void SetupLogicPatterns()
    {
        combatActions = new MultiAction("Attack target");

        combatActions.allActions.Add(engagementMovement);
        combatActions.allActions.Add(attackStats);

        // Change this to be an actual state with conditions (e.g. if exact position is known)
        eliminateTarget.defaultAction = combatActions;


        base.SetupLogicPatterns();
    }
}
