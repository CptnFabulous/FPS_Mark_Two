using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CharacterPoise : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float staggerThreshold = 10;
    [SerializeField] float knockdownThreshold = 20;
    [SerializeField] float recoverSpeed = 3.33f;
    public UnityEvent<DamageMessage> onStunApplied;

    [Header("State transitions")]
    [SerializeField] Character attachedTo;
    [SerializeField] StateController stateController;
    [SerializeField] StateFunction stunState;
    [SerializeField] StateFunction knockdownState;
    
    float currentStunAmount;
    StateFunction lastNonStunState;

    Health health => attachedTo.health;
    float currentStun
    {
        get => currentStunAmount;
        set => SetStun(value);
    }
    
    void Awake()
    {
        health.onDamage.AddListener(ApplyStun);
    }
    void Update()
    {
        if (currentStun > 0)
        {
            float newStun = currentStun - (recoverSpeed * Time.deltaTime);
            currentStun = Mathf.Max(newStun, 0);
        }
    }

    public void ApplyStun(DamageMessage dm)
    {
        currentStun += dm.stun;
        onStunApplied.Invoke(dm);
    }
    void SetStun(float newStunValue)
    {
        if (health.IsAlive == false) return;

        //Debug.Log("Setting stun to " + newStunValue);
        currentStunAmount = newStunValue;

        // Determine what the last non-stun state was
        StateFunction currentState = stateController.currentState;
        if (currentState != knockdownState && currentState != stunState) lastNonStunState = currentState;
        
        // Check if stun is high enough to go to knockdown state
        if (stateController.currentState == knockdownState) return;
        if (currentStunAmount >= knockdownThreshold)
        {
            stateController.SwitchToState(knockdownState);
            return;
        }

        // If able to, check if stun is high enough for stagger state
        if (stateController.currentState == stunState) return;
        if (currentStunAmount >= staggerThreshold)
        {
            stateController.SwitchToState(stunState);
            return;
        }
    }
    public void ReturnToNormalFunction()
    {
        // Rather than preventing stun from increasing at all while staggered or knocked down, just reset stun to zero once the enemy leaves the stun state.
        stateController.SwitchToState(lastNonStunState);
        SetStun(0);
    }
}