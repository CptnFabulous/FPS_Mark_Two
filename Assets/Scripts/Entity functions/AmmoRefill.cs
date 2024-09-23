using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoRefill : ItemPickup
{
    [Header("Weapon")]
    public Weapon toPickup;

    [Header("Ammo")]
    public AmmunitionType type;
    [Tooltip("If less than one, restores all ammo")]
    public int amountToRestore;
    public bool limitedSupply = true;

    bool CanPlayerPickUpThisWeapon(Player player)
    {
        if (toPickup == null) return false;

        string properName = toPickup.parentEntity.properName;
        return player.weapons.equippedWeapons.Find((w) => w.parentEntity.properName == properName) == null;
    }

    public override bool CanInteract(Player player)
    {
        // If this is a new weapon, always pick it up
        if (CanPlayerPickUpThisWeapon(player)) return true;

        // Otherwise check if the player's ammo can be topped up
        return player.weapons.ammo.GetStock(type) < player.weapons.ammo.GetMax(type);
    }
    public override void OnPickup(Player player)
    {
        // Check to provide weapon
        if (CanPlayerPickUpThisWeapon(player))
        {
            Weapon spawnedWeapon = Instantiate(toPickup);
            player.weapons.AddWeapon(spawnedWeapon, true);
        }

        int amount = amountToRestore;
        if (amount <= 0)
        {
            amount = int.MaxValue;
        }
        player.weapons.ammo.Collect(type, amount, out amount);

        if (limitedSupply) base.OnPickup(player);
    }
}