using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : ItemPickup
{
    public int healthRestored;
    public bool deleteOnPickup;
    public Entity attachedTo;

    public override bool CanInteract(Player player) => player.health.data.isFull == false;
    public override void OnPickup(Player player)
    {
        player.health.Heal(healthRestored, attachedTo);
        if (deleteOnPickup) base.OnPickup(player);
    }
}
