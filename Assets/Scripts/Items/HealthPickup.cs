using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healthRestored;
    public bool deleteOnPickup;
    public Entity attachedTo;
    public Interactable interactable;

    private void Awake()
    {
        interactable.onInteract.AddListener(RestoreHealth);
        interactable.canInteract += CanInteract;
    }
    bool CanInteract(Player player) => player.health.data.isFull == false;
    void RestoreHealth(Player player)
    {
        player.health.Heal(healthRestored, attachedTo);
        if (deleteOnPickup) Destroy(gameObject);
    }
}
