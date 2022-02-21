using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRangedAttack : AIAttackBehaviour
{
    [Header("Aim stats")]
    public AIAim.AimValues aimStatswhileTelegraphing;
    public AIAim.AimValues aimStatswhileAttacking;
    public float aimBreakThreshold;
    public float minRange = 10;
    public float maxRange = 30;

    Vector3 targetLocation;
    bool lineOfSightEstablished;
    bool aimAlreadyLocked;
    
    /*
    public void Setup(StateMachine controller)
    {
        onTelegraph.AddListener(() => AimData.Stats = aimStatswhileTelegraphing);
        onTelegraph.AddListener(() => AimData.Stats = aimStatswhileAttacking);
        onTelegraph.AddListener(AimData.ResetStatsToDefault);
    }
    */
    public override void WhileWaitingToAttack()
    {
        targetLocation = GetTargetLocation();
        lineOfSightEstablished = LineOfSightCheck(AimData.LookOrigin, Target.health.HitboxColliders, AimData.Stats.lookDetection, AimData.Stats.diameterForUnobstructedSight, Character.health.HitboxColliders);
        
        if (lineOfSightEstablished) // If AI has a line of sight to attack the target, shift aim towards target
        {
            AimData.RotateLookTowards(targetLocation);
        }
        else
        {
            AimData.LookInNeutralDirection();
        }
    }

    public virtual Vector3 GetTargetLocation()
    {
        return Target.health.HitboxBounds.center;
    }
    public override bool CanAttackTarget()
    {
        Vector3 boundsExtents = CombatAI.target.health.HitboxBounds.extents;
        float aimThreshold = MiscFunctions.Vector3Min(boundsExtents);
        if (aimAlreadyLocked)
        {
            aimThreshold += aimBreakThreshold;
        }
        aimAlreadyLocked = AimData.LookCheckDistance(targetLocation, aimThreshold, true);

        float distanceToTarget = Vector3.Distance(AimData.LookOrigin, targetLocation);
        bool correctDistance = distanceToTarget >= minRange && distanceToTarget <= maxRange;

        // Check if line of sight is established and aim is locked on
        return base.CanAttackTarget() && aimAlreadyLocked && lineOfSightEstablished && correctDistance;
    }
}
