using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AIGunAttack : MonoBehaviour
{
    public enum AttackState
    {
        None,
        Telegraph,
        Attack,
        Cooldown
    }
    
    
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
    public float delayBetweenAttacks = 0;

    [Header("Cooldown")]
    public float cooldownMin = 0.05f;
    public float cooldownMax = 0.2f;
    public UnityEvent onCooldown;

    AttackState attackState;
    IEnumerator currentAttackSequence;

    AIAim aim => rootAI.aiming;
    Character target => rootAI.target;
    bool canTarget => rootAI.targeting.viewStatus == ViewStatus.Visible;
    public Vector3 targetPosition
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

    /// <summary>
    /// Needs to be run every frame to execute attack behaviours.
    /// </summary>
    /// <returns>True if a valid action was performed this frame, false if nothing could be done.</returns>
    public bool RunAttackRoutine()
    {
        if (target == null) return false;
        if (target.health.IsAlive == false) return false;

        // Check that the AI can get line of sight to its target, and that the current attack won't be blocked.
        bool attackNotBlocked = AttackNotBlocked(aim.LookOrigin);

        // If attack has already started, shift aim linearly towards target
        if (attackState != AttackState.None)
        {
            // If in telegraph, check if target is lost. If so, cancel attack.
            if (attackState == AttackState.Telegraph && canTarget == false)
            {
                CancelAttack();
                return false;
            }
            
            aim.ShiftFreeLookTowards(targetPosition, aimSpeedWhileTelegraphing);
            return true;
        }
        else if (canTarget && attackNotBlocked)
        {
            // if the AI can see the target and its attack isn't blocked, rotate look to fix aim on target.
            aim.RotateFreeLookTowards(targetPosition);
            if (onTarget)
            {
                currentAttackSequence = null;
                currentAttackSequence = AttackSequence();
                StartCoroutine(currentAttackSequence);
            }
            return true;
        }

        return false;
    }
    public void CancelAttack()
    {
        if (currentAttackSequence == null) return;

        // Prematurely cancel attack
        rootAI.DebugLog("Cancelling attack");
        StopCoroutine(currentAttackSequence);
        currentAttackSequence = null;
        onTelegraphEnd.Invoke();
        SetSpeedMultiplier(1);
        onCooldown.Invoke();
        attackState = AttackState.None;
        aim.CancelAsyncRoutines();
    }

    IEnumerator AttackSequence()
    {
        rootAI.DebugLog($"Telegraphing {weapon}");
        attackState = AttackState.Telegraph;
        // TO DO: set a fire-and-forget routine for shifting aim towards target
        SetSpeedMultiplier(telegraphMoveSpeedMultiplier);
        onTelegraph.Invoke();
        yield return new WaitForSeconds(telegraphDelay);
        //yield return CancellableYield(CancellableYieldForSeconds(telegraphDelay), () => canTarget == false, CancelAttack);

        rootAI.DebugLog($"Executing attack with {weapon}");
        attackState = AttackState.Attack;
        SetSpeedMultiplier(attackMoveSpeedMultiplier);
        onTelegraphEnd.Invoke();

        int max = weapon.controls.maxBurst;
        max = max > 0 ? max : int.MaxValue - 1;
        max = Mathf.Min(attackNumberMax, max) + 1;
        int numberOfAttacks = Random.Range(attackNumberMin, max);
        for (int i = 0; i < numberOfAttacks; i++)
        {
            yield return weapon.SingleShotAsync();
            onAttack.Invoke();
            if (delayBetweenAttacks > 0) yield return new WaitForSeconds(delayBetweenAttacks);
        }

        rootAI.DebugLog($"Cooling down from attack with {weapon}");
        attackState = AttackState.Cooldown;
        SetSpeedMultiplier(1); // Revert to default speed
        float cooldown = Random.Range(cooldownMin, cooldownMax);
        onCooldown.Invoke();
        yield return new WaitForSeconds(cooldown);

        // End attack
        attackState = AttackState.None;
    }

    void SetSpeedMultiplier(float multiplier)
    {
        NavMeshAgent agent = rootAI.agent;
        if (agent != null) agent.speed = rootAI.baseMovementSpeed * multiplier;
    }
    public bool AttackNotBlocked(Vector3 aimOrigin) => AIAction.LineOfSight(aimOrigin, targetPosition, rootAI, target, weapon.attackMask);

    public static IEnumerator CancellableYieldForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    public static IEnumerator CancellableYield(IEnumerator ienumerator, System.Func<bool> cancelCondition, System.Action onCancel)
    {
        while (ienumerator.MoveNext())
        {
            // If values indicate the function needs to be cancelled, do so and invoke the appropriate event
            if (cancelCondition.Invoke())
            {
                onCancel.Invoke();
                yield break;
            }
            yield return ienumerator.Current;
        }
    }
}
