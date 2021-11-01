using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public enum AmmoTypeEnum
{
    Pistol,
    Buckshot,
    Rifle,
    Grenade
}
*/
public class AmmunitionInventory : MonoBehaviour
{
    public Resource[] ammunitionTypes;
    //public 
    public void Collect(AmmunitionType type, int amount)
    {
        ammunitionTypes[AmmunitionType.GetIndex(type)].Change(amount);
    }

    public void Spend(AmmunitionType type, int amount)
    {
        ammunitionTypes[AmmunitionType.GetIndex(type)].Change(-amount);
    }

    void Reset()
    {
        OnValidate();
    }
    void OnValidate()
    {
        Resource[] newAmmoTypes = new Resource[AmmunitionType.All.Length]; // Then creates an appropriately sized array of resource variables
        for (int i = 0; i < newAmmoTypes.Length; i++) // For each required ammo type
        {
            // Check if a previous field still exists for the type, and update it if so
            bool typeIsNew = true;
            for (int o = 0; o < ammunitionTypes.Length; o++)
            {
                if (ammunitionTypes[i].name == AmmunitionType.All[i].name)
                {
                    newAmmoTypes[i] = ammunitionTypes[i];
                    typeIsNew = false;
                    break;
                }
            }

            if (typeIsNew) // If old field could not be found, create one
            {
                // Create a new field for the current ammo type
                newAmmoTypes[i] = new Resource(100, 100, 20);
                newAmmoTypes[i].name = AmmunitionType.All[i].name;
            }

            // Clamps minimum amount to make sure it doesn't violate the carrying stats
            newAmmoTypes[i].current = Mathf.Clamp(newAmmoTypes[i].current, 0, newAmmoTypes[i].max);
        }
        ammunitionTypes = newAmmoTypes;
    }

    /*
    public void Collect(AmmoTypeEnum type, int amount)
    {
        ammunitionTypes[(int)type].Change(amount);
    }

    public void Spend(AmmoTypeEnum type, int amount)
    {
        ammunitionTypes[(int)type].Change(-amount);
    }

    void Reset()
    {
        OnValidate();
    }
    void OnValidate()
    {
        string[] names = System.Enum.GetNames(typeof(AmmunitionType)); // Obtains name strings for all ammunition types
        Resource[] newAmmoTypes = new Resource[names.Length]; // Then creates an appropriately sized array of resource variables
        for (int i = 0; i < newAmmoTypes.Length; i++) // For each required ammo type
        {
            // Check if a previous field still exists for the type, and update it if so
            bool typeIsNew = true;
            for (int o = 0; o < ammunitionTypes.Length; o++)
            {
                if (ammunitionTypes[i].name == names[i])
                {
                    newAmmoTypes[i] = ammunitionTypes[i];
                    typeIsNew = false;
                    break;
                }
            }

            if (typeIsNew) // If old field could not be found, create one
            {
                // Create a new field for the current ammo type
                newAmmoTypes[i] = new Resource(100, 100, 20);
                newAmmoTypes[i].name = names[i];
            }

            // Clamps minimum amount to make sure it doesn't violate the carrying stats
            newAmmoTypes[i].current = Mathf.Clamp(newAmmoTypes[i].current, 0, newAmmoTypes[i].max);
        }
        ammunitionTypes = newAmmoTypes;
    }
    */
}
