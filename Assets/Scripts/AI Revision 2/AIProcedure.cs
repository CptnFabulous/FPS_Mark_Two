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

    private void OnDisable()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
    }

    IEnumerator RunProcedure()
    {
        yield return Procedure();
        controller.SwitchToState(toSwitchToOnEnd);
    }

    protected abstract IEnumerator Procedure();
}
