using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachLocation : Objective
{
    public GameObject area;
    public float defaultDistanceToRegister = 5;

    LevelArea level;
    Collider collider;
    Renderer renderer;

    Bounds GetAreaBounds()
    {
        if (area.TryGetComponent(out level))
        {
            return level.bounds;
        }
        else if (area.TryGetComponent(out collider))
        {
            return collider.bounds;
        }
        else if (area.TryGetComponent(out renderer))
        {
            return renderer.bounds;
        }
        else
        {
            return new Bounds(area.transform.position, defaultDistanceToRegister * Vector3.one);
        }
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
