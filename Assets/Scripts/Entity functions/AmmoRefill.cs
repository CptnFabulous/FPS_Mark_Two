using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoRefill : MonoBehaviour
{
    public Interactable interactable;
    public AmmunitionType type;
    public int amountToRestore;


    private void Awake()
    {
        interactable.onInteract.AddListener(RefillAmmo);
        interactable.canInteract += CanInteract;
    }
    public bool CanInteract(Player player) => player.weapons.ammo.GetStock(type) < player.weapons.ammo.GetMax(type);
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
