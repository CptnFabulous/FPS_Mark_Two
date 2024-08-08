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
    [SerializeField] float staminaConsumption = 1;

    [Header("Animations")]
    [SerializeField] Animator animator;
    [SerializeField] string windupTrigger = "Windup";
    [SerializeField] string attackTrigger = "Attack";
    [SerializeField] string cooldownTrigger = "Cooldown";
    [SerializeField] string interruptTrigger = "Interrupted";

    Coroutine currentAttack;

    public override LayerMask attackMask => hitDetection;
    public override bool InAction => currentAttack != null;

    public override void OnSwitchFrom() { }
    public override void OnSwitchTo() { }

    protected override void OnPrimaryInputChanged(bool held)
    {
        //Debug.Log($"{this}: input changed to {held}");
        if (held == false) return;
        if (currentAttack != null) return;
        // Attack
        currentAttack = StartCoroutine(AttackSequence());
    }
    protected override void OnSecondaryInputChanged()
    {
        // Block/parry
    }

    public override bool CanAttack() => User.stamina.values.current > staminaConsumption;
    public override void OnAttack() => User.stamina.Deplete(staminaConsumption);

    IEnumerator AttackSequence()
    {
        if (CanAttack() == false)
        {
            currentAttack = null;
            yield break;
        }

        
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
            yield return MiscFunctions.WaitOnLerp(attackTime, (t) =>
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
        if (target != null)
        {
            //Debug.Log($"{this}: dealing damage");

            Vector3 point = target.bounds.center;
            Vector3 hitDirection = point - origin;
            hitData.AttackObject(target.gameObject, User, User, point, hitDirection, -hitDirection);
            //target.health.Damage(damage, stun, false, damageType, User);
        }
        else if (Physics.SphereCast(origin, backupCastRadius, direction, out RaycastHit rh, range, hitDetection))
        {
            // Casts a secondary check
            //Debug.Log("Hit something that isn't an entity");
            hitData.AttackObject(rh.collider.gameObject, User, User, rh.point, direction, rh.normal);
        }
        /*
        else
        {
            Debug.Log($"{this}: missed");
        }
        */
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

    protected override void OnInterrupt()
    {
        StopCoroutine(currentAttack);
        currentAttack = null;
        if (animator != null) animator.SetTrigger(interruptTrigger);
    }
}
