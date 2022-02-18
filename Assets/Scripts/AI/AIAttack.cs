using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AttackPhase
{
    Ready,
    Telegraphing,
    Attacking,
    CoolingDown
}
public class AIAttack : MonoBehaviour
{
    public float telegraphDelay = 0.5f;
    public AIAim.AimValues aimStatsWhileTelegraphing;
    public UnityEvent onTelegraph;

    public float attacksPerMinute = 300;
    public int minAttackCount = 1;
    public int maxAttackCount = 2;
    public AIAim.AimValues aimStatsWhileAttacking;
    public UnityEvent onAttack;

    public float cooldownDuration = 1;
    public UnityEvent onCooldown;

    public AimAtTarget behaviourUsingThis { get; set; }

    public AttackPhase CurrentPhase { get; private set; }
    IEnumerator currentAttack;

    public IEnumerator AttackSequence()
    {
        CurrentPhase = AttackPhase.Telegraphing;
        behaviourUsingThis.AI.aiming.Stats = aimStatsWhileTelegraphing;
        onTelegraph.Invoke();
        yield return new WaitForSeconds(telegraphDelay);

        CurrentPhase = AttackPhase.Attacking;
        behaviourUsingThis.AI.aiming.Stats = aimStatsWhileAttacking;
        for (int i = 0; i < maxAttackCount; i++)
        {
            onAttack.Invoke();
            yield return new WaitForSeconds(60 / attacksPerMinute);

            if (behaviourUsingThis.TargetAcquired == false && i >= minAttackCount)
            {
                End();
            }

            if (CurrentPhase == AttackPhase.CoolingDown) // If attack has been ended, break the loop 
            {
                break;
            }
        }

        CurrentPhase = AttackPhase.CoolingDown;
        behaviourUsingThis.AI.aiming.Stats = behaviourUsingThis.stats;
        onCooldown.Invoke();
        yield return new WaitForSeconds(cooldownDuration);

        CurrentPhase = AttackPhase.Ready;
        currentAttack = null;
    }
    public void Initiate()
    {
        if (CurrentPhase != AttackPhase.Ready)
        {
            return;
        }
        currentAttack = AttackSequence();
        StartCoroutine(currentAttack);
    }
    public void End()
    {
        CurrentPhase = AttackPhase.CoolingDown;
    }
    public void Cancel()
    {
        if (currentAttack != null)
        {
            StopCoroutine(currentAttack);
            currentAttack = null;
        }
        CurrentPhase = AttackPhase.Ready;
    }

    public void ShootGun(GunGeneralStats stats)
    {
        stats.Shoot(behaviourUsingThis.AI.character, behaviourUsingThis.AimData.LookOrigin, behaviourUsingThis.AimData.AimDirection, behaviourUsingThis.AimData.LookUp);
    }
}
