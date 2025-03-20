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
    [SerializeField] LayerMask hitDetection = ~0;
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

    public override LayerMask attackMask => hitDetection;

    protected override void OnSecondaryInputChanged(bool held)
    {
        // Block/parry
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
        Vector3 origin = User.LookTransform.position;
        Vector3 direction = User.aimDirection;
        List<Character> targets = WeaponUtility.MeleeDetectMultiple<Character>(origin, direction, range, angle, hitDetection);
        targets.RemoveAll((e) => User.IsHostileTowards(e) == false);
        MiscFunctions.SortListWithOnePredicate(targets, (e) =>
        {
            Vector3 hitLocation = e.bounds.ClosestPoint(origin);
            return Vector3.Angle(direction, hitLocation - origin);
        });
        Character target = (targets.Count > 0) ? targets[0] : null;
        //Debug.Log($"{this}: commencing attack, target = {target}");
        #endregion

        #region Play attack animation
        if (animator != null) animator.SetTrigger(attackTrigger);

        // Wait for attack. If target is acquired, shift movement towards target
        if (target != null && snapTowardsTarget)
        {
            #region Shift the character's position/rotation towards the target

            Quaternion startingRotation = User.lookController.lookRotation;
            AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            // TO DO: disable standard look controls
            yield return MiscFunctions.WaitOnLerp(attackTime, (ref float t) =>
            {
                Vector3 aimDirection = target.bounds.center - User.LookTransform.position;
                Quaternion desiredRotation = Quaternion.LookRotation(aimDirection, User.transform.up);
                User.lookController.lookRotation = Quaternion.Lerp(startingRotation, desiredRotation, curve.Evaluate(t));
            });
            // TO DO: reenable look controls
            #endregion
        }
        else
        {
            yield return new WaitForSeconds(attackTime);
        }
        #endregion

        #region Deal damage to target (if the attack hits something)
        GameObject targetObject = null;
        Vector3 point = Vector3.zero;
        Vector3 normal = -direction;
        Vector3 hitDirection = direction;
        if (target != null)
        {
            //Debug.Log($"{this}: dealing damage");

            targetObject = target.gameObject;
            point = target.bounds.center;
            hitDirection = point - origin;
            normal = -hitDirection;
            //hitData.AttackObject(target.gameObject, User, User, point, hitDirection, -hitDirection);
        }
        else if (Physics.SphereCast(origin, backupCastRadius, direction, out RaycastHit rh, range, hitDetection))
        {
            targetObject = rh.collider.gameObject;
            point = rh.point;
            normal = rh.normal;
            hitDirection = direction;
            // Casts a secondary check
            //Debug.Log("Hit something that isn't an entity");
        }

        if (targetObject != null)
        {
            Vector3 attackDirection = hitDirection;

            if (directionOffsetAngles != Vector3.zero)
            {
                //Debug.DrawRay(point, attackDirection, Color.red, 5);
                Quaternion q = Quaternion.LookRotation(attackDirection, transform.up);
                q *= Quaternion.Euler(directionOffsetAngles);
                attackDirection = q * Vector3.forward;
            }
            //Debug.DrawRay(point, attackDirection, Color.green, 5);

            hitData.AttackObject(targetObject, User, User, point, attackDirection, normal);
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
        if (animator != null) animator.SetTrigger(interruptTrigger);
    }
}
