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
    
    public UnityEvent onDamage;
    public UnityEvent onHeal;
    public UnityEvent onDeath;
    public bool allowPosthumousDamage;
    public Stamina stunData;
    public bool IsAlive => data.current > 0;

    public Hitbox[] hitboxes { get; private set; }
    public Bounds HitboxBounds
    {
        get
        {
            return MiscFunctions.CombinedBounds(hitboxes);
        }
    }
    public Collider[] HitboxColliders
    {
        get
        {
            if (hitboxColliders == null)
            {
                hitboxColliders = new Collider[hitboxes.Length];
                for (int i = 0; i < hitboxColliders.Length; i++)
                {
                    hitboxColliders[i] = hitboxes[i].collider;
                }
            }
            return hitboxColliders;
        }
    }
    Collider[] hitboxColliders;

    

    private void Awake()
    {
        hitboxes = GetComponentsInChildren<Hitbox>();
        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].sourceHealth = this;
        }
    }


    /// <summary>
    /// Deals damage to this health script. Don't directly call this except in fringe circumstances - instead call Damage() on one of its Hitbox classes.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="type"></param>
    /// <param name="attacker"></param>
    public void Damage(int damage, int stun, bool isCritical, DamageType type, Entity attacker)
    {
        if (IsAlive == false && allowPosthumousDamage == false)
        {
            return;
        }

        data.Increment(-damage);

        if (damage < 0)
        {
            onHeal.Invoke();
        }
        else
        {
            onDamage.Invoke();

            if (stun > 0 && stunData != null && stunData.enabled)
            {
                stunData.WearDown(stun);
            }

            DamageMessage damageMessage = new DamageMessage(attacker, this, type, damage, isCritical, stun);
            Notification<DamageMessage>.Transmit(damageMessage);
        }

        if (data.current <= 0)
        {
            onDeath.Invoke();
            KillMessage killMessage = new KillMessage(attacker, this, type);
            Notification<KillMessage>.Transmit(killMessage);
        }
    }

    #region Miscellaneous functions
    public void DestroyOnDeath()
    {
        Destroy(gameObject);
    }
    public void Resurrect(float delay)
    {
        StartCoroutine(ResurrectSequence(delay));
    }
    IEnumerator ResurrectSequence(float delay)
    {
        yield return new WaitForSeconds(delay);
        data.current = data.max;
        onHeal.Invoke();
    }
    #endregion
}
