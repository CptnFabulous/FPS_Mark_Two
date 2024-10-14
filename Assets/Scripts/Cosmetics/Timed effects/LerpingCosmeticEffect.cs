using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LerpingCosmeticEffect : MonoBehaviour
{
    public AnimationCurve curve = new AnimationCurve
    {
        keys = new Keyframe[]
        {
            new Keyframe(0, 0),
            new Keyframe(0.1f, 1),
            new Keyframe(1, 0),
        }
    };
    public void LerpEffects(float timer) => SetLerpDirectly(curve.Evaluate(timer));
    public abstract void SetLerpDirectly(float value);
}
