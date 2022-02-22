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
        onTelegraph.AddListener(() => aim.Stats = aimStatsWhileTelegraphing);
        onTelegraph.AddListener(() => aim.Stats = aimStatsWhileAttacking);
        onTelegraph.AddListener(() => aim.ResetStatsToDefault());
    }
    public override void Enter()
    {
        aim.lookingInDefaultDirection = false;
    }
    public override void Exit()
    {
        base.Exit();
        aim.lookingInDefaultDirection = true;
    }

    public override void AcquireTarget()
    {
        targetLocation = GetTargetLocation();
        lineOfSightEstablished = AIAction.LineOfSightCheck(aim.LookOrigin, user.target.health.HitboxColliders, aim.Stats.lookDetection, aim.Stats.diameterForUnobstructedSight, user.character.health.HitboxColliders);

        if (lineOfSightEstablished) // If AI has a line of sight to attack the target, shift aim towards target
        {
            aim.RotateLookTowards(targetLocation);
        }
        else
        {
            aim.LookInNeutralDirection();
        }
    }

    public virtual Vector3 GetTargetLocation()
    {
        return user.target.health.HitboxBounds.center;
    }
    public override bool CanAttackTarget()
    {
        // Checks prematurely for line of sight (already calculated)
        if (lineOfSightEstablished == false)
        {
            return false;
        }

        // Checks prematurely for base criteria (simple to calculate)
        if (base.CanAttackTarget() == false)
        {
            return false;
        }

        // Checks if distance is correct
        float distanceToTarget = Vector3.Distance(aim.LookOrigin, targetLocation);
        if (distanceToTarget < minRange || distanceToTarget > maxRange)
        {
            return false;
        }

        // Check if aim is on target
        Vector3 boundsExtents = user.target.health.HitboxBounds.extents;
        float aimThreshold = MiscFunctions.Vector3Min(boundsExtents);
        if (aimAlreadyLocked)
        {
            aimThreshold += aimBreakThreshold;
        }
        aimAlreadyLocked = aim.LookCheckDistance(targetLocation, aimThreshold, true);
        return aimAlreadyLocked;
    }
}
