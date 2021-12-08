using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoRefill : MonoBehaviour
{
    public AmmunitionType type;
    public int amountToRestore;

    public void RefillAmmo(Player player)
    {
        int amount = amountToRestore;
        if (amount <= 0)
        {
            amount = int.MaxValue;
        }
        player.weapons.ammo.Collect(type, amount, out amount);
    }
}
