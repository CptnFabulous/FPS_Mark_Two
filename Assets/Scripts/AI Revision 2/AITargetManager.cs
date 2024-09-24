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
    public RaycastHit lastValidHit { get; private set; }
    public Vector3 lastKnownPosition { get; private set; }
    public float lastTimeSeenTarget { get; private set; }

    public bool targetExists => target != null && target.health.IsAlive;

    void Update()
    {
        ViewStatus viewStatusLastFrame = canSeeTarget;

        // Update hit data of target (and look for one if one isn't assigned)
        RaycastHit hit;
        if (targetExists == false)
        {
            target = controlling.visionCone.FindObjectInFieldOfView<Character>(controlling.IsHostileTowards, out hit);
            canSeeTarget = targetExists ? ViewStatus.Visible : ViewStatus.NotPresent;
        }
        else
        {
            canSeeTarget = controlling.visionCone.VisionConeCheck(target, out hit);
        }
        lastHit = hit;

        // If no target is visible, no need to proceed
        if (canSeeTarget != ViewStatus.Visible) return;

        // Update info on target
        lastValidHit = hit;
        lastKnownPosition = target.transform.position;
        lastTimeSeenTarget = Time.time;

        // If the target was null or not visible last frame (and so a change has occurred), switch to the appropriate AI state
        if (canSeeTarget != viewStatusLastFrame)
        {
            Debug.Log($"{controlling}: target found");
            controlling.stateController.SwitchToState(onTargetFound);
        }
    }
}
