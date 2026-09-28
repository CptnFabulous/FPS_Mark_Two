using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simultaneously acts like a coroutine and a WaitUntil, waiting until either the coroutine finishes or the cancel condition has been met.
/// </summary>
public class WaitForCoroutineOrCancel : CustomYieldInstruction
{
    public MonoBehaviour host;
    public System.Func<bool> cancelConditions;

    Coroutine coroutine;
    bool finished = false;

    public override bool keepWaiting
    {
        get
        {
            // Keep waiting if coroutine hasn't finished
            if (finished) return false;
            // Or if there are no cancel conditions to check for
            if (cancelConditions == null) return false;
            // Or if there are but they haven't been met yet
            if (cancelConditions.Invoke()) return false;

            return true;
        }
    }

    public WaitForCoroutineOrCancel(MonoBehaviour host, System.Func<bool> cancelConditions)
    {
        this.host = host;
        this.cancelConditions = cancelConditions;
    }

    /// <summary>
    /// Actually starts the coroutine. Can be referenced to yield repeatedly without making a new WaitForCoroutineOrCancel every time
    /// </summary>
    /// <param name="enumerator"></param>
    /// <returns></returns>
    public WaitForCoroutineOrCancel RunCoroutine(IEnumerator enumerator)
    {
        finished = false;
        coroutine = host.StartCoroutine(CoroutineWithEndConfirmation(enumerator));
        return this;
    }
    /// <summary>
    /// Immediately cancels the current coroutine and allows it to proceed
    /// </summary>
    public void CancelImmediately()
    {
        host.StopCoroutine(coroutine);
        finished = true;
    }

    IEnumerator CoroutineWithEndConfirmation(IEnumerator enumerator)
    {
        finished = false;
        yield return enumerator;
        CancelImmediately();
    }
}