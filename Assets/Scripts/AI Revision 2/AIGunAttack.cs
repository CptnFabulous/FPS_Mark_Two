using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AIGunAttack : MonoBehaviour
{
    public AI rootAI;
    
    public RangedAttack weapon;

    [Header("Aiming")]
    public float aimAngleThreshold = 0f;

    [Header("Telegraph")]
    public float telegraphDelay = 0.25f;
    public float telegraphMoveSpeedMultiplier = 0.5f;

    [Header("Attack")]
    public int attackNumberMin = 1;
    public int attackNumberMax = 3;
    public float attackMoveSpeedMultiplier = 0.5f;

    [Header("Cooldown")]
    public float cooldownMin = 0.05f;
    public float cooldownMax = 0.2f;

    //IEnumerator currentAttackSequence;

    bool inAttack;
    Vector3 currentAimTarget;

    AIAim aim => rootAI.aiming;
    Character target => rootAI.target;
    Vector3 targetPosition => target.bounds.center;
    bool aimIsCorrect => aim.IsLookingAt(currentAimTarget, aimAngleThreshold);

    private void Update()
    {
        if (target == null) return;
        if (target.health.IsAlive == false) return;


        // While attacking, rotate aim towards target
        // While telegraphing and attacking, shift aim towards target at a speed roughly equivalent to their standard movement speed
        // While cooling down, rotate aim normally

        currentAimTarget = targetPosition;
        bool canTarget = AIAction.LineOfSight(rootAI.LookTransform.position, currentAimTarget, rootAI.attackMask, rootAI.health.HitboxColliders, target.health.HitboxColliders);

        aim.lookingInDefaultDirection = false;
        if (inAttack)
        {
            // Shift aim target position linearly
            // (at a speed proportional to the player's movement speed, so it's more difficult when moving normally but easier when sprinting)
            float speed = 0;
            if (target is Player p) speed = p.movement.defaultSpeed * 0.5f; // Should I add a property for the multiplier?
            aim.ShiftLookTowards(targetPosition, speed);
        }
        else if (canTarget)
        {
            // Rotate aim
            aim.RotateLookTowards(currentAimTarget);
            if (aimIsCorrect)
            {
                StartCoroutine(AttackSequence());
            }
        }
        else
        {
            aim.lookingInDefaultDirection = true; // Look in default direction
        }
        
        /*
        bool aiming = inAttack || AIAction.LineOfSight(rootAI.LookTransform.position, targetPosition, rootAI.attackMask, rootAI.health.HitboxColliders, target.health.HitboxColliders);
        aim.lookingInDefaultDirection = !aiming;
        if (aiming)
        {
            if (inAttack == false)
            {
                currentAimTarget = targetPosition;
            }
            aim.RotateLookTowards(currentAimTarget);
            if (!inAttack && aimIsCorrect)
            {
                StartCoroutine(AttackSequence());
            }
        }
        */
    }


    IEnumerator AttackSequence()
    {
        inAttack = true;

        //currentAimTarget = targetPosition;
        rootAI.agent.speed = rootAI.baseMovementSpeed * telegraphMoveSpeedMultiplier;
        yield return new WaitForSeconds(telegraphDelay);

        rootAI.agent.speed = rootAI.baseMovementSpeed * attackMoveSpeedMultiplier;

        int max = weapon.controls.maxBurst;
        max = max > 0 ? max : int.MaxValue - 1;
        max = Mathf.Min(attackNumberMax, max) + 1;
        int numberOfAttacks = Random.Range(attackNumberMin, max);
        for (int i = 0; i < numberOfAttacks; i++)
        {
            yield return weapon.SingleShot();
        }

        rootAI.agent.speed = rootAI.baseMovementSpeed;
        float cooldown = Random.Range(cooldownMin, cooldownMax);
        yield return new WaitForSeconds(cooldown);
        inAttack = false;
    }

}
