using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITargetManager : MonoBehaviour
{
    public Character target;

    [Header("AI data")]
    public AI controlling;
    public FieldOfView visionCone;

    public ViewStatus canSeeTarget { get; private set; }
    public RaycastHit lastHit { get; private set; }
    public Vector3 lastKnownPosition { get; private set; }

    AIAim aim => controlling.aiming;
    public bool noTarget => target == null || target.health.IsAlive == false;

    void Update()
    {
        if (target == null)
        {
            canSeeTarget = ViewStatus.NotPresent;
            return;
        }

        canSeeTarget = visionCone.VisionConeCheck(target.colliders, out RaycastHit hit);
        if (canSeeTarget == ViewStatus.Visible)
        {
            lastHit = hit;
            lastKnownPosition = target.transform.position;
        }
    }
}
