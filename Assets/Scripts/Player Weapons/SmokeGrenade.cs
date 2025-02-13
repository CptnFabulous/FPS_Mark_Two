using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeGrenade : Throwable
{
    public SmokeCloud smoke;

    public override void OnThrow() { }

    private void OnEnable()
    {
        smoke.emitting = true;
    }
}
