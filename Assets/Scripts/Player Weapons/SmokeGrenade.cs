using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeGrenade : Throwable
{
    public SmokeCloud smoke;
    public float remainingDuration = 10;

    public override void OnThrow() { }

    private void OnEnable()
    {
        //Debug.Log($"{this}: enabled at time {Time.time}");
        smoke.emitting = true;
    }
    private void OnDisable()
    {
        //Debug.Log($"{this}: disabled at time {Time.time}");
        //smoke.activelyEmitting = false;
    }
    void Update()
    {
        if (smoke.emitting == false)
        {
            enabled = false;
            return;
        }

        remainingDuration = Mathf.Max(remainingDuration - Time.deltaTime, 0);
        //Debug.Log("Remaining duration = " + remainingDuration);
        smoke.emitting = remainingDuration > 0;
    }
}
