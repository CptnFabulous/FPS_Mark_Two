using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AIAttackBehaviour : MonoBehaviour
{
    [HideInInspector] public ExecuteAttack actionRunning;
    
    // Attack stats
    [Header("Attack speed")]
    public float attacksPerMinute;
    public int minAttackCount;
    public int maxAttackCount;
    public float delayBetweenAttacks
    {
        get
        {
            return 60 / attacksPerMinute;
        }
    }
    public bool attackInProgress
    {
        get
        {
            return currentAttack != null;
        }
    }

    [Header("Timing")]
    public float telegraphDelay;
    public float cooldownDuration;

    [Header("Effects")]
    public UnityEvent onTelegraph;
    public UnityEvent onAttack;
    public UnityEvent onCooldown;

    /// <summary>
    /// Actions to perform until a target is found.
    /// </summary>
    public abstract void AcquireTarget();
    /// <summary>
    /// <summary>
    /// Can the AI can viably attack the target?
    /// </summary>
    /// <returns></returns>
    public virtual bool CanAttackTarget()
    {
        // If attack is off cooldown
        // If AI is not already performing the attack
        return attackInProgress == false && Time.time - timeOfLastAttackEnd >= cooldownDuration;
    }
    /// <summary>
    /// The IEnumerator controlling the telegraph and attack timing.
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackSequence()
    {
        currentPhase = AttackPhase.Telegraphing;
        onTelegraph.Invoke();
        yield return new WaitForSeconds(telegraphDelay);

        currentPhase = AttackPhase.Attacking;
        for (counter = 0; counter < maxAttackCount; counter++)
        {
            onAttack.Invoke();
            yield return new WaitForSeconds(delayBetweenAttacks);
        }

        EndAttack();
    }
    public void EndAttack()
    {
        if (attackInProgress == false)
        {
            return;
        }
        StopCoroutine(currentAttack);
        currentAttack = null;
        currentPhase = AttackPhase.CoolingDown;
        onCooldown.Invoke();
        timeOfLastAttackEnd = Time.time;
    }
    public AttackPhase currentPhase { get; private set; }
    IEnumerator currentAttack;
    int counter;
    float timeOfLastAttackEnd;

    public virtual void Enter() { }
    public void Loop()
    {
        // Perform default functions until a target is found
        AcquireTarget();

        // If conditions ideal to attack the target
        if (CanAttackTarget())
        {
            // Start attack sequence
            currentAttack = AttackSequence();
            StartCoroutine(currentAttack);
        }
    }
    public virtual void Exit()
    {
        EndAttack();
    }






    public void ShootGun(GunGeneralStats stats)
    {
        stats.Shoot();
        //stats.Shoot(actionRunning.AI, actionRunning.Aim.LookOrigin, actionRunning.Aim.AimDirection, actionRunning.Aim.LookUp);
    }


}

[System.Serializable]
public class ExecuteAttack : AIAction
{
    public AIAttackBehaviour attack;
    public override void Enter()
    {
        attack.actionRunning = this;
        attack.Enter();
    }
    public override void Loop() => attack.Loop();
    public override void Exit() => attack.Exit();
}

public enum AttackPhase
{
    Ready,
    Telegraphing,
    Attacking,
    CoolingDown
}
