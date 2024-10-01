using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AIGunAttack : MonoBehaviour
{
    public AI rootAI;
    
    public RangedAttack weapon;
    
    [Header("Telegraph")]
    public float telegraphDelay = 0.25f;
    public float telegraphMoveSpeedMultiplier = 0.5f;
    public float aimSpeedWhileTelegraphing = 3;
    public UnityEvent onTelegraph;
    public UnityEvent onTelegraphEnd;

    [Header("Attack")]
    public int attackNumberMin = 1;
    public int attackNumberMax = 3;
    public float attackMoveSpeedMultiplier = 0.5f;
    public UnityEvent onAttack;

    [Header("Cooldown")]
    public float cooldownMin = 0.05f;
    public float cooldownMax = 0.2f;
    public UnityEvent onCooldown;

    //IEnumerator currentAttackSequence;

    bool inAttack;
    Vector3 currentAimTarget;

    AIAim aim => rootAI.aiming;
    Character target => rootAI.target;
    Vector3 targetPosition => target.bounds.center;
    bool aimIsCorrect => aim.IsLookingAt(currentAimTarget);

    private void Update()
    {
        if (target == null) return;
        if (target.health.IsAlive == false) return;


        // While attacking, rotate aim towards target
        // While telegraphing and attacking, shift aim towards target at a speed roughly equivalent to their standard movement speed
        // While cooling down, rotate aim normally


        

        bool canSee = rootAI.targeting.canSeeTarget == ViewStatus.Visible;
        currentAimTarget = rootAI.targeting.lastValidHit.point;
        bool canShoot = AIAction.LineOfSight(aim.LookOrigin, currentAimTarget, weapon.attackMask, rootAI.colliders, target.colliders);
        bool canTarget = canSee && canShoot;
        Debug.DrawLine(aim.LookOrigin, currentAimTarget, Color.magenta);

        aim.lookingInDefaultDirection = false;
        if (inAttack)
        {
            /*
            // Shift aim target position linearly
            // (at a speed proportional to the player's movement speed, so it's more difficult when moving normally but easier when sprinting)
            float speed = 0;
            if (target is Player p) speed = p.movement.defaultSpeed * 0.5f; // Should I add a property for the multiplier?
            aim.ShiftLookTowards(targetPosition, speed);
            */
            aim.ShiftLookTowards(targetPosition, aimSpeedWhileTelegraphing);
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
    }


    IEnumerator AttackSequence()
    {
        inAttack = true;

        rootAI.DebugLog($"Telegraphing {weapon}");

        //currentAimTarget = targetPosition;
        rootAI.agent.speed = rootAI.baseMovementSpeed * telegraphMoveSpeedMultiplier;
        onTelegraph.Invoke();
        yield return new WaitForSeconds(telegraphDelay);

        rootAI.agent.speed = rootAI.baseMovementSpeed * attackMoveSpeedMultiplier;
        onTelegraphEnd.Invoke();

        rootAI.DebugLog($"Executing attack with {weapon}");

        int max = weapon.controls.maxBurst;
        max = max > 0 ? max : int.MaxValue - 1;
        max = Mathf.Min(attackNumberMax, max) + 1;
        int numberOfAttacks = Random.Range(attackNumberMin, max);
        for (int i = 0; i < numberOfAttacks; i++)
        {
            yield return weapon.SingleShot();
            onAttack.Invoke();
        }

        rootAI.DebugLog($"Cooling down from attack with {weapon}");

        rootAI.agent.speed = rootAI.baseMovementSpeed;
        float cooldown = Random.Range(cooldownMin, cooldownMax);
        onCooldown.Invoke();
        yield return new WaitForSeconds(cooldown);
        inAttack = false;
    }

}
