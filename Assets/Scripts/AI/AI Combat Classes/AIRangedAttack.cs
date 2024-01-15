using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRangedAttack : AIAttackBehaviour
{
    [Header("Aim stats")]
    public AIAim.AimValues aimStatsWhileTelegraphing;
    public AIAim.AimValues aimStatsWhileAttacking;
    public float aimBreakThreshold;
    public float minRange = 10;
    public float maxRange = 30;

    Vector3 targetLocation;
    bool lineOfSightEstablished;
    bool aimAlreadyLocked;
    
    
    public void Awake()
    {
        onTelegraph.AddListener(() => actionRunning.Aim.Stats = aimStatsWhileTelegraphing);
        onTelegraph.AddListener(() => actionRunning.Aim.Stats = aimStatsWhileAttacking);
        onTelegraph.AddListener(() => actionRunning.Aim.ResetStatsToDefault());
    }
    public override void Enter()
    {
        actionRunning.Aim.lookingInDefaultDirection = false;
    }
    public override void Exit()
    {
        base.Exit();
        actionRunning.Aim.lookingInDefaultDirection = true;
    }

    public override void AcquireTarget()
    {
        targetLocation = GetTargetLocation();
        lineOfSightEstablished = AIAction.LineOfSight(actionRunning.Aim.LookOrigin, targetLocation, actionRunning.AI.attackMask, actionRunning.AI.health.HitboxColliders, actionRunning.CombatAI.target.health.HitboxColliders);
        
        if (lineOfSightEstablished) // If AI has a line of sight to attack the target, shift aim towards target
        {
            actionRunning.Aim.RotateLookTowards(targetLocation);
        }
        else
        {
            actionRunning.Aim.LookInNeutralDirection();
        }
    }

    public virtual Vector3 GetTargetLocation()
    {
        return actionRunning.CombatAI.target.bounds.center;
    }
    public override bool CanAttackTarget()
    {
        // Checks prematurely for line of sight (already calculated)
        if (lineOfSightEstablished == false) return false;

        // Checks prematurely for base criteria (simple to calculate)
        if (base.CanAttackTarget() == false) return false;

        // Checks if distance is correct
        float distanceToTarget = Vector3.Distance(actionRunning.Aim.LookOrigin, targetLocation);
        if (distanceToTarget < minRange || distanceToTarget > maxRange) return false;

        // Check if aim is on target
        Vector3 boundsExtents = actionRunning.CombatAI.target.bounds.extents;
        float aimThreshold = MiscFunctions.Min(boundsExtents.x, boundsExtents.y, boundsExtents.z);
        if (aimAlreadyLocked) aimThreshold += aimBreakThreshold;
        aimAlreadyLocked = actionRunning.Aim.LookCheckDistance(targetLocation, aimThreshold, true);
        return aimAlreadyLocked;
    }
}
