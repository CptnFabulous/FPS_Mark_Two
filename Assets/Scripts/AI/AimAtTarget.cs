using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class AimAtTarget : AIAction
{
    public AIAim.AimValues stats;

    public float aimLockThreshold;
    public float aimBreakThreshold;
    public UnityEvent onTargetAcquired;
    public UnityEvent whileTargetAcquired;
    public UnityEvent onTargetLost;

    public bool TargetAcquired
    {
        get
        {
            return targetLocked;
        }
        set
        {
            if (value != targetLocked)
            {
                if (value == true)
                {
                    onTargetAcquired.Invoke();
                }
                else
                {
                    onTargetLost.Invoke();
                }
            }
            targetLocked = value;
        }
    }
    bool targetLocked;
    public Vector3 targetPoint { get; private set; }

    public override void Enter(StateMachine controller)
    {
        base.Enter(controller);

        AimData.Stats = stats;
    }

    public override void Update(StateMachine controller)
    {
        // Checks if it's presently possible to aim at the target
        bool lineOfSightPossible = LineOfSightCheck(AimData.LookOrigin, CombatAI.target.health.HitboxColliders, stats.lookDetection, stats.diameterForUnobstructedSight, Character.health.HitboxColliders);
        if (lineOfSightPossible)
        {
            Bounds targetBounds = CombatAI.target.health.HitboxBounds;
            targetPoint = targetBounds.center;
            AimData.RotateLookTowards(targetPoint, stats.SpeedBasedOnAngle(AimData.AimDirection, targetPoint - AimData.LookOrigin));

            //targetAcquired = AimData.LookCheckAngle(AimData.AimDirection, targetPoint, threshold);
            //float distanceLockThreshold = Mathf.Min(/* axes of targetBounds.extents.magnitude */);
            float threshold = TargetAcquired ? aimBreakThreshold : aimLockThreshold;
            TargetAcquired = AimData.LookCheckDistance(AimData.AimDirection, targetPoint, threshold);
            if (TargetAcquired)
            {
                whileTargetAcquired.Invoke();
            }
        }
        else // If not, return to neutral pose until target is found again
        {
            TargetAcquired = false; // Enemy obviously doesn't have a target if line of sight is broken
            AimData.ReturnToNeutralLookPosition(stats.lookSpeed);
        }
        



        
    }

    public void OnTargetAcquired()
    {
        
    }

    public override void LateUpdate(StateMachine controller)
    {
        Color rayColour = TargetAcquired ? Color.green : Color.red;
        Debug.DrawRay(AimData.LookOrigin, AimData.AimDirection * Vector3.Distance(AimData.LookOrigin, targetPoint), rayColour);
        Debug.DrawRay(AimData.LookOrigin, AimData.LookDirection * Vector3.Distance(AimData.LookOrigin, targetPoint), Color.white);
    }
}
