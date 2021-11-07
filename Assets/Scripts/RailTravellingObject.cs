using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailTravellingObject : MonoBehaviour
{
    public ObjectRail currentRail;
    public int index;

    float timer;
    void Update()
    {
        if (currentRail == null)
        {
            enabled = false;
        }

        ObjectRail.RailSegment segment = currentRail.segments[index];
        Transform start = currentRail.StartOfSegment(index);
        float distance = Vector3.Distance(start.position, segment.end.position);

        timer += Time.deltaTime / distance * currentRail.baseSpeed * segment.speedMultiplier;
        timer = Mathf.Clamp01(timer);
        //Debug.Log(timer);
        transform.position = Vector3.Lerp(start.position, segment.end.position, segment.curve.Evaluate(timer));
        transform.rotation = Quaternion.Lerp(start.rotation, segment.end.rotation, segment.curve.Evaluate(timer));

        if (timer == 1)
        {
            index += 1;
            if (index >= currentRail.segments.Length - 1)
            {
                index = 0;
            }
            timer = 0;
            
        }
    }
}