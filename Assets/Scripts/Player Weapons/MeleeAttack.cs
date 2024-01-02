using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : WeaponMode//, IInterruptableAction
{
    [Header("Damage")]
    [SerializeField] int damage = 15;
    [SerializeField] int stun = 15;
    [SerializeField] DamageType damageType = DamageType.Slashing;

    [Header("Timing")]
    [SerializeField] float windupTime = 0.25f;
    [SerializeField] float attackTime = 0.25f;
    [SerializeField] float cooldownTime = 0.5f;

    [Header("Detection")]
    [SerializeField] float range = 2;
    [SerializeField] float angle = 45;
    [SerializeField] LayerMask hitDetection = ~0;
    [SerializeField] bool snapTowardsTarget;

    [Header("Animations")]
    public Animator animator;
    public string windupTrigger = "Windup";
    public string attackTrigger = "Attack";
    public string cooldownTrigger = "Cooldown";
    public string interruptTrigger = "Interrupted";

    Coroutine currentAttack;

    public override LayerMask attackMask => hitDetection;
    public override bool InAction => currentAttack != null;

    public override void OnSwitchFrom() { }
    public override void OnSwitchTo() { }

    protected override void OnPrimaryInputChanged(bool held)
    {
        Debug.Log($"{this}: input changed to {held}");
        if (held == false) return;
        if (currentAttack != null) return;
        // Attack
        currentAttack = StartCoroutine(AttackSequence());
    }
    protected override void OnSecondaryInputChanged()
    {
        // Block/parry
    }

    public static IEnumerator WaitOnLerp(float secondsToWait, System.Action<float> frameAction)
    {
        float t = 0;
        do
        {
            t += Time.deltaTime / secondsToWait;
            t = Mathf.Clamp01(t);
            frameAction.Invoke(t);
            yield return null;
        }
        while (t < 1);
    }

    IEnumerator AttackSequence()
    {
        Debug.Log($"{this}: winding up");
        // Play windup animation
        if (animator != null) animator.SetTrigger(windupTrigger);
        // TO DO: Add a thing here to send an attack message, so enemies can dodge attacks
        yield return new WaitForSeconds(windupTime);

        // Acquire target
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
        Debug.Log($"{this}: commencing attack, target = {target}");

        // Play swing animation
        if (animator != null) animator.SetTrigger(attackTrigger);

        // Wait for attack. If target is acquired, shift movement towards target
        if (target != null && snapTowardsTarget)
        {
            // Shift the character's position/rotation towards the target

            Quaternion startingRotation = User.lookController.lookRotation;
            AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            // TO DO: disable standard look controls
            yield return WaitOnLerp(attackTime, (t) =>
            {
                Vector3 aimDirection = target.bounds.center - User.LookTransform.position;
                Quaternion desiredRotation = Quaternion.LookRotation(aimDirection, User.transform.up);
                User.lookController.lookRotation = Quaternion.Lerp(startingRotation, desiredRotation, curve.Evaluate(t));
            });
            // TO DO: reenable look controls

        }
        else
        {
            yield return new WaitForSeconds(attackTime);
        }

        // Deal damage to target (if acquired)
        if (target != null)
        {
            Debug.Log($"{this}: dealing damage");
            target.health.Damage(damage, stun, false, damageType, User);
        }
        else
        {
            Debug.Log($"{this}: missed");
        }

        Debug.Log($"{this}: cooling down");
        // Play cooldown/return animation
        if (animator != null) animator.SetTrigger(cooldownTrigger);
        yield return new WaitForSeconds(cooldownTime);

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
