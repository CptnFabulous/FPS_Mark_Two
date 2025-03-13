using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachLocation : Objective
{
    public GameObject area;

    LevelArea level;
    Collider collider;
    Renderer renderer;

    Bounds GetAreaBounds()
    {
        Bounds b;

        if (area.TryGetComponent(out level))
        {
            b = level.bounds;
        }
        else if (area.TryGetComponent(out collider))
        {
            b = collider.bounds;
        }
        else if (area.TryGetComponent(out renderer))
        {
            b = renderer.bounds;
        }
        else
        {
            b = new Bounds(area.transform.position, Vector3.zero);
        }

        return b;
    }

    /// <summary>
    /// Is the player inside the bounds for success?
    /// </summary>
    protected override bool DetermineSuccess() => targetPlayer != null && GetAreaBounds().Intersects(targetPlayer.bounds);
    protected override string GetSerializedProgress()
    {
        return "";
        //throw new System.NotImplementedException();
    }

    protected override void Setup(string progress)
    {
        //throw new System.NotImplementedException();
    }

    public override Vector3? location => GetAreaBounds().center;
}
