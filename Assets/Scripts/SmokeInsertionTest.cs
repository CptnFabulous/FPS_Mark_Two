using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeInsertionTest : MonoBehaviour
{
    public SimulatedSmokeGrid smokeGrid;
    public float smokePerSecond = 1;
    public float burstAmount = 100;


    private void Update()
    {
        smokeGrid.IntroduceSmoke(transform.position, smokePerSecond * Time.deltaTime);
    }

    [ContextMenu("Burst of smoke")]
    public void SmokeBurst()
    {
        smokeGrid.IntroduceSmoke(transform.position, burstAmount);
    }
    [ContextMenu("Clear smoke")]
    public void Clear() => smokeGrid.Clear();
}
