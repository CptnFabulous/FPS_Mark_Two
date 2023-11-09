using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoRefill : ItemPickup
{
    public AmmunitionType type;
    [Tooltip("If less than one, restores all ammo")]
    public int amountToRestore;
    public bool limitedSupply = true;

    public override bool CanInteract(Player player) => player.weapons.ammo.GetStock(type) < player.weapons.ammo.GetMax(type);
    public override void OnPickup(Player player)
    {
        int amount = amountToRestore;
        if (amount <= 0)
        {
            amount = int.MaxValue;
        }
        player.weapons.ammo.Collect(type, amount, out amount);

        if (limitedSupply) base.OnPickup(player);
    }
}
