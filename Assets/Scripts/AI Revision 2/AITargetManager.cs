using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITargetManager : MonoBehaviour
{
    public Character target;

    [Header("AI data")]
    public AI controlling;
    public AIStateFunction onTargetFound;

    public ViewStatus canSeeTarget { get; private set; }
    public RaycastHit lastHit { get; private set; }
    public Vector3 lastKnownPosition { get; private set; }

    public bool targetExists => target != null && target.health.IsAlive;

    void Update()
    {
        #region If no target is assigned, look for one
        if (targetExists == false)
        {
            target = controlling.visionCone.FindObjectInFieldOfView<Character>(controlling.IsHostileTowards, out _);
            if (targetExists)
            {
                // If a new target has been found, switch to the appropriate AI state
                controlling.stateController.SwitchToState(onTargetFound);
            }
            else // No target has been found, cancel function
            {
                canSeeTarget = ViewStatus.NotPresent;
                return;
            }
        }
        #endregion

        #region Update info on current target
        canSeeTarget = controlling.visionCone.VisionConeCheck(target, out RaycastHit hit);
        if (canSeeTarget == ViewStatus.Visible)
        {
            lastHit = hit;
            lastKnownPosition = target.transform.position;
        }
        #endregion
    }
}
