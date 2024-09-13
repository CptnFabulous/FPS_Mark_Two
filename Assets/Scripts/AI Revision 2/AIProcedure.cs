using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIProcedure : AIStateFunction
{
    public StateFunction toSwitchToOnEnd;

    Coroutine currentCoroutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentCoroutine = StartCoroutine(RunProcedure());
    }

    protected virtual void OnDisable()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
    }

    IEnumerator RunProcedure()
    {
        // Extra frame yield until everything's initialised
        yield return null;
        // Run the procedure
        yield return Procedure();
        // Switch to another state on end
        if (toSwitchToOnEnd != null) controller.SwitchToState(toSwitchToOnEnd);
    }

    protected void ResetProcedure()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(RunProcedure());
    }

    protected abstract IEnumerator Procedure();
}
