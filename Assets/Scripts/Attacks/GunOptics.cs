using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunOptics : MonoBehaviour
{
    public float magnification;
    public float transitionTime;

    public UnityEvent onSwitchToADS;
    public UnityEvent onSwitchToHipfire;


    


    
    /// <summary>
    /// Is the player currently using ADS? Change this value to trigger ADS changing code
    /// </summary>
    public bool IsAiming
    {
        get
        {
            return currentlyAiming;
        }
        set
        {
            if (currentlyAiming == value)
            {
                return;
            }

            StopCoroutine(transitionSequence);
            if (value == true)
            {
                transitionSequence = Enable();
            }
            else
            {
                transitionSequence = Disable();
            }
            StartCoroutine(transitionSequence);
            currentlyAiming = value;
        }
    }
    bool currentlyAiming;
    float timer;
    IEnumerator transitionSequence;


    public IEnumerator Enable()
    {
        onSwitchToADS.Invoke();
        while (timer <= 1)
        {
            // Do lerp stuff using the timer value
            LerpADS(timer);
            timer += Time.deltaTime / transitionTime;
            yield return null;
        }
    }
    public IEnumerator Disable()
    {
        onSwitchToHipfire.Invoke();
        while (timer >= 0)
        {
            // Do lerp stuff using the timer value
            LerpADS(timer);
            timer -= Time.deltaTime / transitionTime;
            yield return null;
        }
    }
    public void LerpADS(float value)
    {

    }
}
