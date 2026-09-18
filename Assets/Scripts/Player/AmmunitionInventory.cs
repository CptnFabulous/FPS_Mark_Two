using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AmmunitionInventory : MonoBehaviour
{
    public WeaponHandler weaponHandler;
    public bool startEmpty;
    [SerializeField] Resource[] ammunitionTypes;

    public UnityEvent<AmmunitionType, int, Resource> onResourceUpdated;

    private void Start()
    {
        for (int i = 0; i < ammunitionTypes.Length; i++)
        {
            // Unless set to start completely empty, fill each ammo type if the player has a weapon or gadget that uses it
            bool shouldFill = !startEmpty && PlayerUsesAmmoType(AmmunitionType.All[i]);
            ammunitionTypes[i].current = shouldFill ? ammunitionTypes[i].max : 0;
        }
    }

    

    public Resource GetValues(AmmunitionType type) => ammunitionTypes[AmmunitionType.GetIndex(type)];
    public float GetStock(AmmunitionType type) => GetValues(type).current;
    public int GetMax(AmmunitionType type) => GetValues(type).max;
    public void Collect(AmmunitionType type, int amount, out int remainder)
    {
        int index = AmmunitionType.GetIndex(type);
        ammunitionTypes[index].Increment(amount, out float extra);
        remainder = Mathf.RoundToInt(extra);

        onResourceUpdated.Invoke(type, amount - remainder, ammunitionTypes[index]);
    }
    public void Spend(AmmunitionType type, int amount)
    {
        int index = AmmunitionType.GetIndex(type);
        ammunitionTypes[index].Increment(-amount);

        onResourceUpdated.Invoke(type, -amount, ammunitionTypes[index]);
    }

#if UNITY_EDITOR
    [ContextMenu("Validate types")]
    void Validate()
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
#endif

    #region Checking ammo against current weapons

    bool PlayerUsesAmmoType(AmmunitionType type)
    {
        // Check if the player has a weapon or gadget equipped with this ammo type.
        if (weaponHandler.equippedWeapons.Find((w) => WeaponUsesAmmoType(w, type)) != null) return true;
        if (weaponHandler.offhandAttacks.allModes.FirstOrDefault((m) => WeaponModeUsesAmmoType(m, type)) != null) return true;

        return false;
    }
    bool WeaponUsesAmmoType(Weapon w, AmmunitionType ammoType)
    {
        return w.modes.FirstOrDefault((m) => WeaponModeUsesAmmoType(m, ammoType)) != null;
    }
    bool WeaponModeUsesAmmoType(WeaponMode mode, AmmunitionType ammoType)
    {
        if (mode is RangedAttack r)
        {
            return r.stats.ammoType == ammoType;
        }
        if (mode is ThrowObject t)
        {
            return t.ammunitionType == ammoType;
        }

        return false;
    }

    #endregion
}
