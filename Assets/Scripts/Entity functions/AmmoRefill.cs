using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoRefill : MonoBehaviour
{
    public Interactable interactable;
    public AmmunitionType type;
    [Tooltip("If less than one, restores all ammo")]
    public int amountToRestore;
    public bool limitedSupply;


    private void Awake()
    {
        interactable.onInteract.AddListener(RefillAmmo);
        interactable.canInteract += CanInteract;
    }
    public bool CanInteract(Player player) => player.weapons.ammo.GetStock(type) < player.weapons.ammo.GetMax(type);
    // public bool CanInteract(Player player) => player.weapons.ammo[type].current < player.weapons.ammo[type].max;
    public void RefillAmmo(Player player)
    {
        int amount = amountToRestore;
        if (amount <= 0)
        {
            amount = int.MaxValue;
        }
        player.weapons.ammo.Collect(type, amount, out amount);
        // player.weapons.ammo[type].Restore(amount, out amount);

        if (limitedSupply) Destroy(gameObject);
    }
}
