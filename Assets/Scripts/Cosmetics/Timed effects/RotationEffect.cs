using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationEffect : LerpingCosmeticEffect
{
    public Transform altering;
    public Space space = Space.Self;
    public Vector3 oldEulerAngles;
    public Vector3 newEulerAngles;
    public override void SetLerpDirectly(float value)
    {
        Vector3 eulerAngles = Quaternion.Lerp(Quaternion.Euler(oldEulerAngles), Quaternion.Euler(newEulerAngles), value).eulerAngles;
        if (space == Space.Self)
        {
            altering.localEulerAngles = eulerAngles;
        }
        else
        {
            altering.eulerAngles = eulerAngles;
        }
    }
}
