using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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

    bool inAttack;
    IEnumerator currentAttackSequence;

    AIAim aim => rootAI.aiming;
    Character target => rootAI.target;
    bool canTarget => rootAI.targeting.viewStatus == ViewStatus.Visible;
    Vector3 targetPosition
    {
        get
        {
            // Check if line of sight is established. If so, aim at the point the AI can see.
            if (canTarget) return rootAI.targeting.lastValidHit.point;
            // If not, just aim in the target's general direction.
            return target.bounds.center;
        }
    }
    bool onTarget => aim.IsLookingAt(targetPosition);

    private void Update()
    {
        if (target == null) return;
        if (target.health.IsAlive == false) return;

        // If attack has already started, shift aim linearly towards target
        if (inAttack)
        {
            aim.ShiftLookTowards(targetPosition, aimSpeedWhileTelegraphing);
        }
        else
        {
            // Check that the AI can get line of sight to its target, and that the current attack won't be blocked.
            bool attackNotBlocked = AttackNotBlocked();
            if (canTarget && attackNotBlocked)
            {
                // if the AI can see the target and its attack isn't blocked, rotate look to fix aim on target.
                aim.RotateFreeLookTowards(targetPosition);
                if (onTarget)
                {
                    currentAttackSequence = null;
                    currentAttackSequence = AttackSequence();
                    StartCoroutine(currentAttackSequence);
                }
            }
            else
            {
                // Otherwise, the AI looks straight forward as it moves to its desired vantage point.
                aim.LookInNeutralDirection();
            }
        }
    }
    private void OnDisable()
    {
        if (currentAttackSequence != null)
        {
            // Prematurely cancel attack
            StopCoroutine(currentAttackSequence);
            currentAttackSequence = null;
            onTelegraphEnd.Invoke();
            onCooldown.Invoke();
            inAttack = false;
            aim.CancelAsyncRoutines();
        }
    }

    IEnumerator AttackSequence()
    {
        inAttack = true;

        rootAI.DebugLog($"Telegraphing {weapon}");

        //currentAimTarget = targetPosition;
        SetSpeed(telegraphMoveSpeedMultiplier);
        //rootAI.agent.speed = rootAI.baseMovementSpeed * telegraphMoveSpeedMultiplier;
        onTelegraph.Invoke();
        yield return new WaitForSeconds(telegraphDelay);

        SetSpeed(attackMoveSpeedMultiplier);
        //rootAI.agent.speed = rootAI.baseMovementSpeed * attackMoveSpeedMultiplier;
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

        // Revert to default speed
        SetSpeed(1);
        //rootAI.agent.speed = rootAI.baseMovementSpeed;
        float cooldown = Random.Range(cooldownMin, cooldownMax);
        onCooldown.Invoke();
        yield return new WaitForSeconds(cooldown);
        inAttack = false;
    }

    void SetSpeed(float multipler)
    {
        if (rootAI.agent != null) rootAI.agent.speed = rootAI.baseMovementSpeed * multipler;
    }
    public bool AttackNotBlocked() => AIAction.LineOfSight(aim.LookOrigin, targetPosition, rootAI, target, weapon.attackMask);
}
