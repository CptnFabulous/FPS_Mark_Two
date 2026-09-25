using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AmmoRefill : ItemPickup
{
    [Header("Weapon")]
    public Weapon toPickup;
    public bool idLocked = false;
    public string pickupMessage = "Pick up";
    public string idLockedMessage = "ID-locked";

    [Header("Ammo")]
    public AmmunitionType type;
    [Tooltip("If less than one, restores all ammo")]
    public int amountToRestore;
    public string pickupMessageFormat = "Restore {0} {1} rounds";
    public string fullMessageFormat = "Max {0} rounds is {1}";
    public bool limitedSupply = true;

    bool CanPlayerPickUpThisWeapon(Player player)
    {
        if (toPickup == null) return false;

        string properName = toPickup.parentEntity.properName;
        WeaponHandler wh = player.weapons;
        if (toPickup.isOffhand)
        {
            return wh.offhandAttacks.CanAdd(toPickup);
        }
        else
        {
            return wh.CanAdd(toPickup);//. .Find((w) => w.parentEntity.properName == properName) == null;
        }

    }

    public override bool CanInteract(Player player, out string message)
    {
        message = null;

        // If this is a new weapon, always pick it up
        if (CanPlayerPickUpThisWeapon(player))
        {
            // If the weapon is ID-locked (and the player doesn't have another weapon that accepts the same ammo), no reason to pick it up
            if (idLocked)
            {
                message = idLockedMessage;
                return false;
            }

            message = pickupMessage;
            return true;
        }


        int current = Mathf.RoundToInt(player.weapons.ammo.GetStock(type));
        int max = player.weapons.ammo.GetMax(type);

        // Check if the player's ammo can be topped up
        bool ammoNotFull = current < max;

        if (ammoNotFull)
        {
            message = string.Format(pickupMessageFormat, amountToRestore, type.name);
        }
        else
        {
            message = string.Format(fullMessageFormat, type.name, max);
        }

        return ammoNotFull;
    }
    public override void OnPickup(Player player)
    {
        WeaponHandler wh = player.weapons;

        // Check if a new weapon was picked up
        bool pickedUpWeapon = CanPlayerPickUpThisWeapon(player);
        if (pickedUpWeapon)
        {
            // Check if this weapon is a mainhand or offhand ability, and add to the appropriate list
            Weapon spawnedWeapon = Instantiate(toPickup);
            if (toPickup.isOffhand)
            {
                wh.offhandAttacks.TryAdd(spawnedWeapon);
            }
            else
            {
                wh.TryAdd(spawnedWeapon, true);
            }
        }

        // Determine how much ammo needs to be added
        bool isInfinite = amountToRestore <= 0;
        int amount = isInfinite ? int.MaxValue : amountToRestore;
        // Increment player ammo and modify remainder value
        wh.ammo.Collect(type, amount, out int remainder);
        if (!isInfinite) amountToRestore = remainder;

        // If not unlimited, and weapon is picked up or all ammo is taken, delete the pickup
        if (limitedSupply && (pickedUpWeapon || limitedSupply && amountToRestore <= 0)) base.OnPickup(player);
    }
}