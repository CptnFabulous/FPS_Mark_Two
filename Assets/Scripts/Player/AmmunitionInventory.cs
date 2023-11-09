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
    public bool startEmpty;
    public Resource[] ammunitionTypes;

    private void Start()
    {
        for (int i = 0; i < ammunitionTypes.Length; i++)
        {
            ammunitionTypes[i].current = startEmpty ? 0 : ammunitionTypes[i].max;
        }
    }

    public Resource GetValues(AmmunitionType type)
    {
        return ammunitionTypes[AmmunitionType.GetIndex(type)];
    }
    public float GetStock(AmmunitionType type)
    {
        return GetValues(type).current;
    }
    public int GetMax(AmmunitionType type)
    {
        return GetValues(type).max;
    }
    public void Collect(AmmunitionType type, int amount, out int remainder)
    {
        ammunitionTypes[AmmunitionType.GetIndex(type)].Increment(amount, out float extra);
        remainder = Mathf.RoundToInt(extra);
    }
    public void Spend(AmmunitionType type, int amount)
    {
        ammunitionTypes[AmmunitionType.GetIndex(type)].Increment(-amount);
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
}
