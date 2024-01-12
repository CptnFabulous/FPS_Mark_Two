using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITargetManager : MonoBehaviour
{
    public Character target;

    [Header("AI data")]
    public AI controlling;

    public ViewStatus canSeeTarget { get; private set; }
    public RaycastHit lastHit { get; private set; }
    public Vector3 lastKnownPosition { get; private set; }

    public bool targetExists => target != null && target.health.IsAlive;

    void Update()
    {
        if (target == null)
        {
            canSeeTarget = ViewStatus.NotPresent;
            return;
        }

        canSeeTarget = controlling.visionCone.VisionConeCheck(target.colliders, out RaycastHit hit);
        if (canSeeTarget == ViewStatus.Visible)
        {
            lastHit = hit;
            lastKnownPosition = target.transform.position;
        }
    }
}
