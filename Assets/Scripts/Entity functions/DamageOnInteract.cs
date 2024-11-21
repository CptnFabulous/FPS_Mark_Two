using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnInteract : MonoBehaviour
{
    public Interactable interactable;
    public Health health;

    [Header("Damage stats")]
    public DamageType damageType = DamageType.Piercing;
    public int damage = 1;
    public int stun = 1;

    private void Awake()
    {
        interactable.canInteract += (Player player, out string msg) =>
        {
            msg = null;
            return health.IsAlive;
        };
        interactable.onInteract.AddListener(DamageObject);
    }

    void DamageObject(Player player)
    {
        Vector3 direction = interactable.collider.bounds.center - player.bounds.center;
        health.Damage(damage, stun, false, damageType, player, null, direction);
    }
}
