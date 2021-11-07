using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRail : MonoBehaviour
{
    [System.Serializable]
    public struct RailSegment
    {
        public Transform end;
        public AnimationCurve curve;// = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float speedMultiplier;// = 1;
    }

    public Transform start;
    public RailSegment[] segments = new RailSegment[]
    {
        new RailSegment
        {
            curve = AnimationCurve.Linear(0, 0, 1, 1),
            speedMultiplier = 1
        }
    };
    public float baseSpeed;

    public Transform StartOfSegment(int currentSegmentIndex)
    {
        if (currentSegmentIndex <= 0)
        {
            return start;
        }
        return segments[currentSegmentIndex - 1].end;
    }
}
