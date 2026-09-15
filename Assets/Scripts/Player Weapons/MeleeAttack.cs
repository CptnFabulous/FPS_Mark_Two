using CptnFabulous.MiscUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeAttack : WeaponMode//, IInterruptableAction
{
    [Header("Timing")]
    [SerializeField] float windupTime = 0.25f;
    [SerializeField] float attackTime = 0.25f;
    [SerializeField] float cooldownTime = 0.5f;
    public UnityEvent onAttack;

    [Header("Detection")]
    [SerializeField] float range = 2;
    [SerializeField] float angle = 45;
    [SerializeField] DetectionProfile hitDetection;
    [SerializeField] float backupCastRadius = 0.5f;
    [SerializeField] bool snapTowardsTarget;

    [Header("Damage")]
    [SerializeField] DamageDealer hitData;
    [SerializeField] Vector3 directionOffsetAngles = Vector3.zero;
    [SerializeField] float staminaConsumption = 1;

    [Header("Animations")]
    [SerializeField] Animator animator;
    [SerializeField] string windupTrigger = "Windup";
    [SerializeField] string attackTrigger = "Attack";
    [SerializeField] string cooldownTrigger = "Cooldown";
    [SerializeField] string interruptTrigger = "Interrupted";

    public override LayerMask attackMask => hitDetection.mask;
    public override string hudInfo => null;
    Vector3 attackOrigin => User.LookTransform.position;

    protected override void OnSecondaryInputChanged(bool held)
    {
        // Block/parry?
    }

    public override bool CanAttack() => User.stamina.values.current > staminaConsumption;
    public override void OnAttack() => User.stamina.Deplete(staminaConsumption);

    protected override IEnumerator AttackSequence()
    {
        #region Windup
        //Debug.Log($"{this}: winding up");
        // Play windup animation
        OnAttack();
        if (animator != null) animator.SetTrigger(windupTrigger);
        onAttack.Invoke();
        // TO DO: Add a thing here to send an attack message, so enemies can dodge attacks
        yield return new WaitForSeconds(windupTime);
        #endregion

        #region Acquire target

        Vector3 direction = User.aimDirection;

        RaycastHit hit;
        Collider targetCollider;

        // Detect closest collider that meets requirements (reuse interaction code?)
        // TO DO: Do I prioritise hitboxes over rigidbodies, or do I just prioritise whichever has either and is closer to the reticle?
        bool hitboxFound = AngleCheck.CheckForObjectsInCone(attackOrigin, direction, angle, range, attackMask, out targetCollider, out hit, (Collider collider, out Collider c) =>
        {
            // Mark the 'out' value, even though we're doing GetComponent() calls
            c = collider;

            // Check for a hitbox
            // Make sure hitbox isn't an ally
            Hitbox hb = collider.GetComponentInParent<Hitbox>();
            if (hb != null && User.IsHostileTowards(hb.attachedTo)) return true;

            // If no hitbox is found, check for a non-kinematic rigidbody
            Rigidbody rb = ComponentUtility.GetComponentInParentWhere<Rigidbody>(collider.transform, (rb) => rb.isKinematic == false);
            if (rb != null) return true;

            return false;
        });

        // If a hitbox is not found, use a raycast to hit whatever collider the user is directly aiming at
        if (!hitboxFound && Physics.Raycast(attackOrigin, direction, out hit, range, attackMask))
        {
            targetCollider = hit.collider;
        }
        
        #endregion

        #region Play attack animation
        if (animator != null) animator.SetTrigger(attackTrigger);

        // Wait for attack. If target is acquired, shift movement towards target
        if (targetCollider != null && snapTowardsTarget)
        {
            Quaternion startingRotation = User.lookController.lookRotation;
            AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            // TO DO: disable standard look controls
            yield return MiscFunctions.WaitOnLerp(attackTime, (ref float t) =>
            {
                Vector3 aimDirection = hit.point - attackOrigin;
                Quaternion desiredRotation = Quaternion.LookRotation(aimDirection, User.transform.up);
                User.lookController.lookRotation = Quaternion.Lerp(startingRotation, desiredRotation, curve.Evaluate(t));
            });
            // TO DO: reenable look controls
        }
        else
        {
            yield return new WaitForSeconds(attackTime);
        }
        #endregion

        #region Deal damage to target (if the attack hits something)

        if (targetCollider != null)
        {
            Vector3 attackDirection = hit.point - attackOrigin;

            if (directionOffsetAngles != Vector3.zero)
            {
                //Debug.DrawRay(point, attackDirection, Color.red, 5);
                Quaternion q = Quaternion.LookRotation(attackDirection, transform.up);
                q *= Quaternion.Euler(directionOffsetAngles);
                attackDirection = q * Vector3.forward;
            }
            //Debug.DrawRay(point, attackDirection, Color.green, 5);

            hitData.AttackObject(targetCollider.gameObject, User, User, hit.point, attackDirection, hit.normal);
        }

        #endregion

        #region Cooldown
        //Debug.Log($"{this}: cooling down");
        // Play cooldown/return animation
        if (animator != null) animator.SetTrigger(cooldownTrigger);
        yield return new WaitForSeconds(cooldownTime);
        #endregion

        currentAttack = null;
    }


    


    public override void OnTertiaryInput() { }

    protected override void OnDisable()
    {
        base.OnDisable();
        AnimationUtility.TrySetAnimatorTrigger(animator, interruptTrigger);
    }
}
