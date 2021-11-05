using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleWeaponAnimation : MonoBehaviour
{
    public Transform older;
    public Transform newer;
    public float time = 0.25f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}
