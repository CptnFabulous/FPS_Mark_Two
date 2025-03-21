using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AmmunitionInventory : MonoBehaviour
{
    public bool startEmpty;
    [SerializeField] Resource[] ammunitionTypes;

    public UnityEvent<AmmunitionType, int, Resource> onResourceUpdated;

    private void Start()
    {
        for (int i = 0; i < ammunitionTypes.Length; i++)
        {
            ammunitionTypes[i].current = startEmpty ? 0 : ammunitionTypes[i].max;
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
