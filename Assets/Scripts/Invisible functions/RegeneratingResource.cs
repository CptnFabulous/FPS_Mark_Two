using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RegeneratingResource : MonoBehaviour
{
    public Resource values = new Resource(3, 3, 1);

    public float regenDelay = 2;
    public float regenTime = 2;

    public ResourceMeter guiMeter;

    float regenDelayTimer;

    public void Deplete(float value)
    {
        values.Increment(-value);
        regenDelayTimer = 0;
    }

    private void Update()
    {
        if (regenDelayTimer < regenDelay)
        {
            regenDelayTimer += Time.deltaTime;
        }
        else if (values.isFull == false)
        {
            values.Increment(Time.deltaTime / regenTime);
        }
    }
    private void LateUpdate()
    {
        if (guiMeter != null) guiMeter.Refresh(values);
    }
}