using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class AimAtTarget : AIAction
{
    public AIAttack attack;

    public AIAim.AimValues stats;
    public float aimLockThreshold;
    public float aimBreakThreshold;
    public virtual Vector3 GetTargetPoint()
    {
        Bounds targetBounds = CombatAI.target.health.HitboxBounds;
        return targetBounds.center;
    }
    public virtual bool CheckTargetStatus()
    {
        Bounds targetBounds = CombatAI.target.health.HitboxBounds;
        float threshold = Mathf.Min(MiscFunctions.Vector3Array(targetBounds.extents));
        if (TargetAcquired)
        {
            // Aim threshold is larger if a target is already acquired, for a bit of sluggishness in the enemy realising they've lost their aim
            threshold += aimBreakThreshold;
        }

        return AI.aiming.LookCheckDistance(AI.aiming.AimDirection, TargetPoint, threshold);
    }
    public Vector3 TargetPoint { get; private set; }
    public bool TargetAcquired { get; private set; }


    public override void Enter(StateMachine controller)
    {
        base.Enter(controller);

        AimData.Stats = stats; // Set new stats
        AimData.lookingInDefaultDirection = false;

        attack.behaviourUsingThis = this; // Assign a reference to the attack being triggered by this behaviour
    }
    public override void Update(StateMachine controller)
    {
        // Checks if it's presently possible to aim at the target
        bool lineOfSightPossible = LineOfSightCheck(AimData.LookOrigin, CombatAI.target.health.HitboxColliders, stats.lookDetection, stats.diameterForUnobstructedSight, Character.health.HitboxColliders);
        if (lineOfSightPossible)
        {
            TargetPoint = GetTargetPoint(); // Determine current point in world space to aim at
            AimData.RotateLookTowards(TargetPoint); // Shift aim towards target point
            TargetAcquired = CheckTargetStatus(); // Check if target is close enough to be within aim threshold

        }
        else // If not, return to neutral pose until target is found again
        {
            if (TargetAcquired)
            {
                TargetAcquired = false; // Enemy obviously doesn't have a target if line of sight is broken
                attack.End();
            }

            AimData.LookInNeutralDirection();
        }

        // If aim is on target, attack
        if (TargetAcquired)
        {
            attack.StartSequence();
        }
    }
    public override void Exit(StateMachine controller)
    {
        attack.CancelSequence();
        AimData.lookingInDefaultDirection = true;
    }
}
