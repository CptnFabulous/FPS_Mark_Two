using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    Electrocution,
    //Corrosive,
    //Poison,
    Asphyxiation,
    Healing, // used for healing processes, since healing and taking damage are both altering a health value
    DeletionByGame // e.g. falling out of the level or similar non-diegetic game process
}

public class Health : MonoBehaviour
{
    [Header("Stats")]
    public Resource data = new Resource(100, 100, 20);
    public bool godmode = false;

    public UnityEvent<DamageMessage> onDamage;
    public UnityEvent<DamageMessage> onHeal;
    public UnityEvent<KillMessage> onDeath;
    public bool allowPosthumousDamage;

    [Header("Other")]
    [Tooltip("For ensuring the entity can't be damaged by a physics object they're holding, if it glitches out"),
     SerializeField] ThrowHandler throwHandler;

    Entity e;
    Hitbox[] hb;
    Dictionary<GameObject, float> recentPhysicsCollisions = new Dictionary<GameObject, float>();
    // Any time the attached entity manually exerts force on a physics object (e.g. with AddForce()), register it and the time here
    public Dictionary<GameObject, float> timesPhysicsObjectsWereLaunchedByThisEntity = new Dictionary<GameObject, float>();

    static float minimumCollisionForceToDamage = 10;
    static float damagePerCollisionForceUnit = 0.5f;
    static float stunPerCollisionForceUnit = 1f;
    static float minTimeBetweenCollisions = 0.5f;
    static float minTimeAfterThrowBeforeCollision = 1f;

    public bool IsAlive => data.current > 0;
    public Entity attachedTo => e ??= GetComponentInParent<Entity>();
    public Hitbox[] hitboxes => hb ??= GetComponentsInChildren<Hitbox>().Where((hb) => hb.attachedTo == attachedTo).ToArray();
    
    /// <summary>
    /// Deals damage to this health script. Don't directly call this except in fringe circumstances - instead call Damage() on one of its Hitbox classes.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="type"></param>
    /// <param name="attacker"></param>
    public void Damage(int damage, int stun, bool isCritical, DamageType type, Entity attacker, Vector3 direction)
    {
        if (IsAlive == false && allowPosthumousDamage == false) return;

        bool isHealing = damage < 0;
        if (isHealing)
        {
            type = DamageType.Healing;
        }
        else if (godmode && type != DamageType.DeletionByGame)
        {
            // If godmode is enabled, set damage to zero
            damage = 0;
            stun = 0;
        }

        Debug.Log($"{this} ({data.current} health) took{(isCritical ? " a critical" : "")} {damage} damage and {stun} stun");
        data.Increment(-damage);

        DamageMessage damageMessage = new DamageMessage(attacker, this, type, damage, isCritical, stun, direction);
        (isHealing ? onHeal : onDamage).Invoke(damageMessage);
        Notification<DamageMessage>.Transmit(damageMessage);

        if (data.current <= 0)
        {
            KillMessage killMessage = new KillMessage(attacker, this, type);
            onDeath.Invoke(killMessage);
            Notification<KillMessage>.Transmit(killMessage);
        }
    }
    public void Heal(int value, Entity healer) => Damage(-value, 0, false, DamageType.Healing, healer, Vector3.zero);
    public void DamageFromPhysicsCollision(Collision collision, Hitbox hitbox)
    {
        // Don't bother with calculations if entity is already dead
        if (IsAlive == false && allowPosthumousDamage == false) return;

        #region Calculate collision force, cancel if too low
        // Figure out impact force from velocity. If a rigidbody is present, multiply the mass accordingly
        float force = collision.relativeVelocity.magnitude;
        Rigidbody rb = collision.rigidbody;
        if (rb != null)
        {
            force *= PhysicsCache.TotalMassOfConnectedRigidbodies(rb);
        }
        /*
        else
        {
            // How do I determine the impact force if the entity collides with terrain?
        }
        */

        // If the force isn't enough to register, cancel.
        // We don't want things constantly taking chip damage from the most miniscule impacts
        if (force <= minimumCollisionForceToDamage) return;
        #endregion

        #region Check if the entity can take damage from the colliding object at this time

        // Ensure the entity can't be damaged by its own colliders
        if (attachedTo.colliders.Contains(collision.collider)) return;

        // Check the root rigidbody this entity is attached to.
        GameObject damagedBy = rb != null ? PhysicsCache.GetRootRigidbody(rb).gameObject : collision.gameObject;

        // Don't deal damage if the attached entity is deliberately holding it
        if (throwHandler != null && throwHandler.holding != null && throwHandler.holding.gameObject == damagedBy) return;

        // If this character applied a physics force to this object a short time ago, don't register a hit
        if (timesPhysicsObjectsWereLaunchedByThisEntity.TryGetValue(damagedBy, out float timeOfHit))
        {
            if ((Time.time - timeOfHit) < minTimeAfterThrowBeforeCollision) return;
        }

        // If the root object just dealt physics damage previously, don't count
        // (So that damage doesn't happen multiple times due to a single object hitting multiple hitboxes at once)
        if (recentPhysicsCollisions.TryGetValue(damagedBy, out float hitTime) && (Time.time - hitTime) < minTimeBetweenCollisions) return;
        recentPhysicsCollisions[damagedBy] = Time.time; // Update the last time hit for the next check

        #endregion

        #region Deal damage and stun
        Debug.Log($"{damagedBy} will damage {attachedTo} in {this}, force = {force}/{minimumCollisionForceToDamage}, on frame {Time.frameCount}");
        //Debug.Log($"{damagedBy} {(willTakeDamage ? "will" : "won't")} damage {attachedTo} in {this}, force = {force}/{minimumCollisionForceToDamage}, on frame {Time.frameCount}");

        // Calculate damage and stun accordingly
        float damage = force * damagePerCollisionForceUnit;
        float stun = force * stunPerCollisionForceUnit;
        Entity thingThatDamagedThisHitbox = collision.gameObject.GetComponentInParent<Entity>();

        // Deal damage (use the hitbox's main damage function to calculate things like resistances)
        hitbox.Damage(Mathf.RoundToInt(damage), Mathf.RoundToInt(stun), DamageType.Impact, thingThatDamagedThisHitbox, collision.relativeVelocity.normalized, hitbox.isCritical);
        #endregion
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
        onHeal.Invoke(null);
    }
    #endregion
}