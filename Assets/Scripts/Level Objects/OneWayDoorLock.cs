using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayDoorLock : DoorLock
{
    public override bool CanOpen(Player player)
    {
        // Check player position against transform direction
        Vector3 direction = transform.position - player.bounds.center;
        float dot = Vector3.Dot(transform.forward, direction);
        // If the player is approaching in the correct direction, allow unlocking
        return dot > 0;
    }
}
