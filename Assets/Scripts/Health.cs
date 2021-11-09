using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum DamageType
{
    Piercing, // e.g. gunshots and stabs
    Slashing, // e.g. shallow sword cuts
    //Severing, // e.g. body part removal
    Bludgeoning, // e.g. blunt force attacks
    Impact, // e.g. slamming into a wall or floor
    Explosive,
    Burning,
    //Freezing,
    //Electric,
    //Corrosive,
    //Poison,
    Asphyxiation,
    Healing, // used for healing processes, since healing and taking damage are both altering a health value
    DeletionByGame // e.g. falling out of the level or similar non-diegetic game process
}

public class Health : MonoBehaviour
{
    public Resource data = new Resource(100, 100, 20);
    public Hitbox[] hitboxes { get; private set; }
    public UnityEvent onDamage;
    public UnityEvent onHeal;
    public UnityEvent onDeath;
    public bool allowPosthumousDamage;
    public bool IsAlive
    {
        get
        {
            return data.current > 0;
        }
    }
    public Bounds HitboxBounds
    {
        get
        {
            Bounds entityBounds = hitboxes[0].collider.bounds;
            for (int i = 1; i < hitboxes.Length; i++)
            {
                entityBounds.Encapsulate(hitboxes[i].collider.bounds);
            }
            return entityBounds;
        }
    }
    public void Damage(int amount, DamageType type, Entity attacker)
    {
        if (IsAlive == false && allowPosthumousDamage == false)
        {
            return;
        }
        
        data.current -= amount;

        EventHandler.Transmit(new DamageMessage(attacker, this, type, amount));

        if (data.current <= 0)
        {
            onDeath.Invoke();
        }
        else if (amount < 0)
        {
            onHeal.Invoke();
        }
        else
        {
            onDamage.Invoke();
        }
    }

    private void Awake()
    {
        hitboxes = GetComponentsInChildren<Hitbox>();
        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].sourceHealth = this;
        }
    }
}
