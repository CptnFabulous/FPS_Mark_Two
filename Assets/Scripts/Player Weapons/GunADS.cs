using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunADS : MonoBehaviour
{
    [Header("Stats")]
    public float magnification = 1;
    public float transitionTime = 0.25f;
    public float hipfireSwayMultiplier = 1;
    public bool hideMainReticle;
    public UnityEvent onSwitchToADS;
    public UnityEvent onSwitchToHipfire;
    public UnityEvent<ADSHandler, float> onADSLerp;

    [Header("Animations")]
    public Transform hipFireOrientation;
    public Transform modelOrientationTransform;
    public Transform modelPivot;
    public float distanceBetweenReticleAxisAndHead = 1f;
    public AnimationCurve modelMovementCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Turn sway")]
    public Transform reticleAxis;
    public float lookSwayDegrees = 2;
    public float speedForMaxSway = 120;
    public float swayUpdateTime = 0.1f;
}