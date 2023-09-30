using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITargetManager : MonoBehaviour
{
    public Character target;

    [Header("Stats")]
    public float timeToWaitBeforeReacquiringTarget = 5;

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


    public IEnumerator QuickLookForLostTarget(System.Action onFailedToReacquireTarget)
    {
        Debug.Log($"{this}: target lost, checking if outside field of view");
        // If outside the view angle, rotate to see where the player is last.
        // If the AI already knows the target is behind cover (or inexplicably not present), skip this step
        while (canSeeTarget == ViewStatus.OutsideViewAngle)
        {
            while (aim.IsLookingAt(lastHit.point) == false)
            {
                yield return null;
                aim.RotateLookTowards(lastHit.point);
                // If the AI can see the target now, cancel
                if (canSeeTarget == ViewStatus.Visible) yield break;
            }
        }

        // The AI now knows the target is blocked by cover. Wait for several seconds
        // (in case the target has simply taken cover, or to allow the player to perform a flanking maneuver) 
        Debug.Log($"{this}: target is behind cover, waiting cautiously");
        float timer = 0;
        while (timer < timeToWaitBeforeReacquiringTarget)
        {
            if (canSeeTarget == ViewStatus.Visible) yield break;
            yield return null;
            timer += Time.deltaTime;
            Debug.Log($"{this}: waited for {timer}/{timeToWaitBeforeReacquiringTarget} seconds");
        }

        // Seek new target position
        Debug.Log($"{this}: cannot find target");
        onFailedToReacquireTarget?.Invoke();
    }
}
