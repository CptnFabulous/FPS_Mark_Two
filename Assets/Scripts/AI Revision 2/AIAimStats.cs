using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAimStats : MonoBehaviour
{
    public float lookSpeed = 360;
    public AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);
    public float swayAngle;
    public float swaySpeed;
    public float diameterForUnobstructedSight;

    public float SpeedBasedOnAngle(Vector3 currentAimDirection, Vector3 desiredDirection)
    {
        if (speedCurve == null) return lookSpeed;

        float angle = Vector3.Angle(currentAimDirection, desiredDirection);
        return lookSpeed * speedCurve.Evaluate(angle / 180);
    }
}
