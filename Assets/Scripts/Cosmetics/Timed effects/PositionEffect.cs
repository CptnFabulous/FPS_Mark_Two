using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionEffect : LerpingCosmeticEffect
{
    public Transform altering;
    public Space space = Space.Self;
    public Vector3 oldPosition;
    public Vector3 newPosition;
    public override void SetLerpDirectly(float value)
    {
        Vector3 position = Vector3.Lerp(oldPosition, newPosition, value);
        if (space == Space.Self)
        {
            altering.localPosition = position;
        }
        else
        {
            altering.position = position;
        }
    }
}
