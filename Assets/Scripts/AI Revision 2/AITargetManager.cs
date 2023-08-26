using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITargetManager : MonoBehaviour
{
    public Character target;

    [Header("AI data")]
    public AI controlling;
    public FieldOfView visionCone;

    public bool canSeeTarget { get; private set; }
    public RaycastHit targetHit { get; private set; }
    public Vector3 lastKnownPosition { get; private set; }

    void Update()
    {
        if (target == null)
        {
            canSeeTarget = false;
            return;
        }

        canSeeTarget = visionCone.VisionConeCheck(target.colliders, out RaycastHit hit);
        targetHit = hit;
        if (canSeeTarget)
        {
            lastKnownPosition = target.transform.position;
        }
    }
}
