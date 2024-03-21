using Newtonsoft.Json.Linq;
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
    
    public UnityEvent<DamageMessage> onDamage;
    public UnityEvent<DamageMessage> onHeal;
    public UnityEvent<KillMessage> onDeath;
    public bool allowPosthumousDamage;
    public bool IsAlive => data.current > 0;

    public Character attachedTo => c ??= GetComponentInParent<Character>();
    public Hitbox[] hitboxes
    {
        get
        {
            if (hb != null) return hb;

            hb = GetComponentsInChildren<Hitbox>();
            for (int i = 0; i < hitboxes.Length; i++)
            {
                hitboxes[i].sourceHealth = this;
            }

            return hb;
        }
    }

    Character c;
    Hitbox[] hb;

    /// <summary>
    /// Deals damage to this health script. Don't directly call this except in fringe circumstances - instead call Damage() on one of its Hitbox classes.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="type"></param>
    /// <param name="attacker"></param>
    public void Damage(int damage, int stun, bool isCritical, DamageType type, Entity attacker)
    {
        if (IsAlive == false && allowPosthumousDamage == false) return;

        bool isHealing = damage < 0;
        if (isHealing) type = DamageType.Healing;

        data.Increment(-damage);

        DamageMessage damageMessage = new DamageMessage(attacker, this, type, damage, isCritical, stun);
        (isHealing ? onHeal : onDamage).Invoke(damageMessage);
        Notification<DamageMessage>.Transmit(damageMessage);

        if (data.current <= 0)
        {
            KillMessage killMessage = new KillMessage(attacker, this, type);
            onDeath.Invoke(killMessage);
            Notification<KillMessage>.Transmit(killMessage);
        }
    }
    public void Heal(int value, Entity healer) => Damage(-value, 0, false, DamageType.Healing, healer);

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
        onHeal.Invoke(null);
    }
    #endregion
}
