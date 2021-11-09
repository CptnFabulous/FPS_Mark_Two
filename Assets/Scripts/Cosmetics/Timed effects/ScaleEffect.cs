using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleEffect : LerpingCosmeticEffect
{
    public Transform altering;
    public Space space = Space.Self;
    public Vector3 oldScale = Vector3.one;
    public Vector3 newScale = Vector3.one;
    public override void SetLerpDirectly(float value)
    {
        Vector3 scale = Vector3.Lerp(oldScale, newScale, value);
        if (space == Space.Self)
        {
            altering.localScale = scale;
        }
        else
        {
            Transform parent = altering.parent;
            altering.parent = null;
            altering.localScale = scale;
            altering.parent = parent;
        }
    }
}
