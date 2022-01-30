using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGunEnemy : Combatant
{
    [Header("Stats")]
    public EngageTargetAtDistance engagementMovement;
    public AimAtTarget targetingStats;

    public override void Awake()
    {
        base.Awake();

        inCombat.allStates.Add(engagementMovement);
        inCombat.allStates.Add(targetingStats);

    }
}
