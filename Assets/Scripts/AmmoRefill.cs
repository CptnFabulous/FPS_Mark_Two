using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoRefill : MonoBehaviour
{
    public AmmunitionType type;
    public int amountToRestore;

    public void RefillAmmo(Player player)
    {
        player.weapons.ammo.Collect(type, amountToRestore, out int remainder);
        amountToRestore = remainder;
        if (amountToRestore <= 0)
        {

        }
    }
}
