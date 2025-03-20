using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeGrenade : Throwable
{
    public SmokeCloud smoke;

    private void Awake()
    {
        smoke.emitting = false;
    }
    private void OnDisable()
    {
        smoke.emitting = false;
    }
    public override void OnThrow()
    {
        smoke.emitting = true;
    }
}
