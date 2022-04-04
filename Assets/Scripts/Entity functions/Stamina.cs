using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Stamina : MonoBehaviour
{
    public Resource stamina = new Resource(25, 25, 5);
    public float staminaRegenRate = 10;
    [Header("Quick stun")]
    public int thresholdForQuickStun;
    public float quickStunDuration = 0.5f;
    public UnityEvent onStun;
    [Header("Stumble")]
    public float staggerDuration = 3;
    public float immunityDurationAfterStaggering = 2;
    public UnityEvent onStagger;
    public UnityEvent onRecover;

    IEnumerator CurrentCoroutine
    {
        get
        {
            return action;
        }
        set
        {
            // Automatically stops the previous coroutine before starting the new one
            
            if (action != null)
            {
                StopCoroutine(action);
            }

            action = value;

            if (action != null)
            {
                StartCoroutine(action);
            }
        }
    }
    IEnumerator action;
    bool isImmune;

    public void Update()
    {
        stamina.current = Mathf.Clamp(stamina.current + Time.deltaTime * staminaRegenRate, 0, stamina.max);
    }
    private void OnDisable()
    {
        CurrentCoroutine = null;
    }

    public void WearDown(int amount)
    {
        if (isImmune)
        {
            return;
        }

        stamina.Increment(-amount);

        // If stamina runs out, enemy is staggered. If enemy is in the middle of a quick stun when this happens, cancel it
        if (stamina.isDepleted)
        {
            CurrentCoroutine = StaggerState();
        }
        else if (amount > thresholdForQuickStun) // Otherwise, check if the attack is strong enough to cause a quick stun
        {
            CurrentCoroutine = StunState();
        }
    }
    IEnumerator StunState()
    {
        onStun.Invoke();
        yield return new WaitForSeconds(quickStunDuration);
        onRecover.Invoke();
    }
    IEnumerator StaggerState()
    {
        isImmune = true; // Temporarily become immune to stunning, to provide a chance to bounce back
        onStagger.Invoke();
        stamina.current = stamina.max; // Restore stamina to full

        yield return new WaitForSeconds(staggerDuration);

        onRecover.Invoke();

        yield return new WaitForSeconds(immunityDurationAfterStaggering);

        isImmune = false;
    }

    /*

    What to do on stun and stagger:
    If stunned:
    Temporarily jolt accuracy
    Offset or slow their movement speed slightly (what's a not awful way to do this?)

    If staggered:
    Interrupt attacks
    Temporarily disable movement
    Temporarily enable rigidbody so enemy can be knocked around

    idea for how to implement knockdown:
    In hitbox's OnCollisionEnter, check if the attached health object has a Stamina class like this present
    Check if force magnitude is over the threshold required for knockdown
    If so, run knockdown functions
    
    Knockdown state
    Enable ragdoll physics, disable all logic code
    Wait until
    * Sufficient time has elapsed
    * Total velocity is below the threshold required for standing back up
    * AI is standing on valid ground to run its usual functions
    Disable ragdoll physics, re-enable logic code, run 'standing up' animation/state

    */
}
