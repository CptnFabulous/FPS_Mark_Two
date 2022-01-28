using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    IEnumerator currentAttack;
    bool inAttack;

    public IEnumerator AttackSequence()
    {
        inAttack = true;
        //Debug.Log("Telegraphing on frame " + Time.frameCount);
        behaviourUsingThis.AI.aiming.Stats = aimStatsWhileTelegraphing;
        onTelegraph.Invoke();
        yield return new WaitForSeconds(telegraphDelay);


        behaviourUsingThis.AI.aiming.Stats = aimStatsWhileAttacking;
        for (int i = 0; i < maxAttackCount; i++)
        {
            //Debug.Log("Attacking, #" + (i + 1) + "/" + maxAttackCount + " on " + Time.frameCount);
            onAttack.Invoke();
            yield return new WaitForSeconds(60 / attacksPerMinute);

            if (behaviourUsingThis.TargetAcquired == false && i >= minAttackCount)
            {
                End();
            }

            if (inAttack == false) // If attack has been ended, break the loop 
            {
                break;
            }
        }

        inAttack = false;
        //Debug.Log("Ending attack on " + Time.frameCount);
        behaviourUsingThis.AI.aiming.Stats = behaviourUsingThis.stats;
        onCooldown.Invoke();
        yield return new WaitForSeconds(cooldownDuration);

        currentAttack = null;
    }

    /// <summary>
    /// Is this behaviour currently running an attack sequence? If false, set to true to start a new one, or set to false to cancel an in-progress sequence
    /// </summary>

    public bool InAttack
    {
        get
        {
            return currentAttack != null && inAttack == true;
        }
        private set
        {
            if (InAttack != value) // If value has been changed
            {
                if (value == true)
                {
                    currentAttack = AttackSequence();
                    StartCoroutine(currentAttack);
                    inAttack = true;
                    Debug.Log("Starting attack on frame " + Time.frameCount);
                }
                else
                {
                    inAttack = false;
                    Debug.Log("Ending attack on frame " + Time.frameCount);
                }
            }
        }
    }

    public void StartSequence()
    {
        InAttack = true;
    }
    public void End()
    {
        InAttack = false;
    }
    public void CancelSequence()
    {
        if (currentAttack != null)
        {
            StopCoroutine(currentAttack);
            currentAttack = null;
        }
        inAttack = false;
        Debug.Log("Cancelling attack on frame " + Time.frameCount);
    }



    public void ShootGun(GunGeneralStats stats)
    {
        stats.Shoot(behaviourUsingThis.AI.character, behaviourUsingThis.AimData.LookOrigin, behaviourUsingThis.AimData.AimDirection, behaviourUsingThis.AimData.LookUp);
    }
}
