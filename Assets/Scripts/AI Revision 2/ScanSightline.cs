using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanSightline : AIStateFunction
{
    [Header("Stats")]
    [Tooltip("Represents the position to stand in and the direction to aim in.")]
    public Transform sightline;
    [Range(0, 360)]
    public float horizontalAngle;
    [Range(0, 180)]
    public float verticalAngle;
    public float delayBetweenSweeps = 0.5f;

    public override IEnumerator AsyncProcedure()
    {
        // Move to location of sightline
        yield return rootAI.TravelToDestination(sightline.position);

        // Once at sightline, continually scan it for targets
        Vector2 angles = new Vector2(horizontalAngle, verticalAngle);
        yield return aim.SweepSightlineAsync(() => sightline.forward, angles, delayBetweenSweeps);
    }
}
