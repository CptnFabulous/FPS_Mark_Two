using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AIAttackBehaviour : AIAction
{
    public Character Target
    {
        get
        {
            return CombatAI?.target;
        }
    }
    
    // Attack stats
    [Header("Attack speed")]
    public float attacksPerMinute;
    public float minAttackCount;
    public float maxAttackCount;
    public float delayBetweenAttacks
    {
        get
        {
            return 60 / attacksPerMinute;
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
    public abstract void WhileWaitingToAttack();
    /// <summary>
    /// <summary>
    /// Can the AI can viably attack the target?
    /// </summary>
    /// <returns></returns>
    public virtual bool CanAttackTarget()
    {
        return currentAttack == null && Time.time - timeOfLastAttackEnd >= cooldownDuration;
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
        AI.StopCoroutine(currentAttack);
        currentAttack = null;
        currentPhase = AttackPhase.CoolingDown;
        onCooldown.Invoke();
        timeOfLastAttackEnd = Time.time;
    }
    public AttackPhase currentPhase { get; private set; }
    IEnumerator currentAttack;
    int counter;
    float timeOfLastAttackEnd;


    public override void Update(StateMachine controller)
    {
        // Perform default functions until a target is found
        WhileWaitingToAttack();

        // If conditions ideal to attack the target
        // If attack is off cooldown
        // If AI is not already performing the attack
        if (CanAttackTarget())
        {
            // Start attack sequence
            currentAttack = AttackSequence();
            AI.StartCoroutine(currentAttack);
        }
        
    }





    void WhileTelegraphing()
    {

    }
    void WhileAttacking()
    {

    }
    void WhileCoolingDown()
    {

    }


}
