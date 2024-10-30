using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimAssist : MonoBehaviour
{
    [SerializeField] LookController lookController;
    [SerializeField] WeaponHandler weaponHandler;
    [SerializeField] bool enabledForController;
    [SerializeField] bool enabledForMouseAndKeyboard;
    [SerializeField] bool disableWhileHipfiring;
    [SerializeField] float checkRange = 50;
    [SerializeField] float magnetiseAngle = 10;
    [SerializeField] float magnetiseSpeed = 10;

    [SerializeField] float parallelDistanceCheck = 0.5f; // TO DO: figure out better name for this




    [SerializeField] AnimationCurve magnetiseRangeCurve = AnimationCurve.Linear(0, 1, 1, 0);
    [SerializeField] AnimationCurve magnetiseAngleCurve = AnimationCurve.Constant(0, 1, 1);








    [SerializeField] float magnetiseTime = 0.5f;
    [SerializeField] AnimationCurve magnetiseTimeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Hitbox targeted;
    RaycastHit rh;
    bool snapInProgress;


    //Quaternion snapStartRotation;
    //float snapTimer;

    Vector3 aimOrigin => swayHandler.aimOrigin;
    AimSwayHandler swayHandler => weaponHandler.swayHandler;
    RangedAttack currentAttack => weaponHandler.CurrentWeapon.CurrentMode as RangedAttack;
    LayerMask mask => currentAttack.attackMask;

    public void Update()
    {
        #region Check that snapping should occur
        // Don't bother if the player isn't currently holding a gun
        if (weaponHandler.weaponDrawn == false) return;
        if (currentAttack == null) return;

        // Check if aim assist should be active, based on what input device the player is using
        bool shouldUse = lookController.usingGamepad ? enabledForController : enabledForMouseAndKeyboard;
        if (shouldUse == false) return;

        // Check if the user is hip-firing. If the sights are set to only activate during ADS, cancel.
        bool usingAds = currentAttack.optics != null && currentAttack.optics.IsAiming;
        if (disableWhileHipfiring && !usingAds) return;
        #endregion

        #region Find hitbox to snap to
        // Determine visible look direction
        // If hip-firing, use the centre of the reticle to prevent aim shifting away from the intended target.
        // If aiming down sights, shift the true aim direction (to account for sway).
        Vector3 direction = usingAds ? swayHandler.aimDirection : lookController.aimAxis.forward;

        // Check for closest hitbox
        Hitbox newHitbox = null;
        bool angleCheck = AngleCheck.CheckForObjectsInCone(aimOrigin, direction, magnetiseAngle, checkRange, mask, out newHitbox, out rh, (Collider c, out Hitbox h) => ValidHitbox(c, direction, out h));
        if (angleCheck == false) return;
        #endregion



        // Check if closest hitbox has changed. If so, trigger new snap operation
        if (newHitbox != targeted)
        {
            targeted = newHitbox;
            //snapStartRotation = lookController.lookRotation;
            snapInProgress = true;
            //snapTimer = 0;
        }

        // Don't adjust aim if snap has finished
        if (snapInProgress == false) return;
        /*
        if (snapTimer >= 1) return;

        snapTimer += Time.deltaTime / magnetiseTime;
        snapTimer = Mathf.Clamp01(snapTimer);
        */



        // Obtain the desired aim rotation. If aiming down sights, account for difference between look and aim direction
        Vector3 point = targeted.collider.bounds.center;
        Quaternion desiredRotation = Quaternion.LookRotation(point - aimOrigin, lookController.mainBodyTransform.up);
        if (usingAds)
        {
            Quaternion difference = desiredRotation *= Quaternion.Inverse(swayHandler.aimRotation);
            desiredRotation = lookController.lookRotation * difference;
        }

        // Slowly shift aim towards closest hitbox
        float speed = magnetiseSpeed;

        // Change strength based on range
        speed *= magnetiseRangeCurve.Evaluate(rh.distance / checkRange);

        // Change strength based on angle towards target
        float angleRatio = Quaternion.Angle(lookController.lookRotation, desiredRotation) / magnetiseAngle;
        speed *= magnetiseAngleCurve.Evaluate(angleRatio);

        // Should the magnetise strength be influenced by the player's current sensitivity?
        // Since if the player can adjust their aim more with a smaller movement, the aim assist might need to be stronger to ensure it has any effect.

        // Version A: shift continuously towards target
        lookController.lookRotation = Quaternion.RotateTowards(lookController.lookRotation, desiredRotation, speed * Time.deltaTime);

        // Version B: cache position then lerp towards target
        // Feels extremely jerky because you have no control while snapping
        //lookController.lookRotation = Quaternion.Lerp(snapStartRotation, desiredRotation, magnetiseTimeCurve.Evaluate(snapTimer));

        // Version C: lerp towards target but from current position (so it accounts for external changes)
        // Allows for some adjustment, and feels smoother, but still feels a bit jerky
        //lookController.lookRotation = Quaternion.Lerp(lookController.lookRotation, desiredRotation, magnetiseTimeCurve.Evaluate(snapTimer));


        // Check if snapping is finished
        if (lookController.lookRotation == desiredRotation) snapInProgress = false;
    }

    private void OnDrawGizmosSelected()
    {
        float range = targeted != null ? rh.distance : checkRange;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(aimOrigin, swayHandler.aimDirection * range);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(aimOrigin, lookController.aimAxis.forward * range);
        
        if (targeted == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(aimOrigin, targeted.collider.bounds.center);
    }
    
    bool ValidHitbox(Collider c, Vector3 direction, out Hitbox target)
    {
        // Check that this component has a hitbox on it
        target = c.GetComponentInParent<Hitbox>();
        if (target == null) return false;

        // Check that the hitbox belongs to an enemy the player is hostile to
        Entity e = target.attachedTo;
        if (weaponHandler.controller.IsHostileTowards(e) == false) return false;

        // Ensure that said entity is still alive
        if (e.health == null) return false;
        if (e.health.IsAlive == false) return false;

        // Exclude colliders that are too far away in terms of distance (otherwise the aim assist reaches way too far at longer distances)
        float distanceAtRange = ParallelDistance(c.bounds.center, aimOrigin, direction);
        if (distanceAtRange > parallelDistanceCheck) return false;

        return true;
    }

    static float ParallelDistance(Vector3 point, Vector3 origin, Vector3 direction)
    {
        float range = Vector3.Distance(point, origin);
        Vector3 pointInDirection = origin + (range * direction);
        return Vector3.Distance(point, pointInDirection);
    }
}