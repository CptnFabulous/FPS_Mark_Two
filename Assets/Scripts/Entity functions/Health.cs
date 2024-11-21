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
    PhysicsImpact,
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
    public string deadDescription = "dead";

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

    public DamageMessage lastSourceOfDamage { get; private set; } = null;

    static float minimumCollisionForceToDamage = 10;
    static float damagePerCollisionForceUnit = 0.5f;
    static float stunPerCollisionForceUnit = 1f;
    static float minTimeBetweenCollisions = 0.5f;
    static float minTimeAfterThrowBeforeCollision = 1f;
    static float multiplierForStaticCollisions = 4;

    public bool IsAlive => data.current > 0;
    public Entity attachedTo => e ??= GetComponentInParent<Entity>();
    public Hitbox[] hitboxes
    {
        get
        {
            if (hb != null) return hb;
            // Look for hitboxes in child components
            hb = GetComponentsInChildren<Hitbox>();
            // If entity has a child attached, exclude hitboxes that are part of that child
            hb = hb.Where((hb) => hb.attachedTo == attachedTo).ToArray();
            return hb;
        }
    }
    
    /// <summary>
    /// Deals damage to this health script. Don't directly call this except in fringe circumstances - instead call Damage() on one of its Hitbox classes.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="type"></param>
    /// <param name="attacker"></param>
    public void Damage(int damage, int stun, bool isCritical, DamageType type, Entity attacker, Entity weaponUsed, Vector3 direction)
    {

        bool wasAlive = IsAlive;

        bool isHealing = damage < 0;


        if (IsAlive == false && allowPosthumousDamage == false && isHealing == false) return;

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

        attachedTo.DebugLog($"{this} ({data.current} health) took{(isCritical ? " a critical" : "")} {damage} damage and {stun} stun");
        data.Increment(-damage);

        DamageMessage damageMessage = new DamageMessage(attacker, weaponUsed, this, type, damage, isCritical, stun, direction);
        // At this point I think I'd need to delete the previous 'lastSourceOfDamage' if C# meant I didn't have to worry about garbage collection
        lastSourceOfDamage = damageMessage;
        (isHealing ? onHeal : onDamage).Invoke(damageMessage);
        Notification<DamageMessage>.Transmit(damageMessage);

        /*
        // If this attack is the one that killed the entity, run death events
        if (!IsAlive && wasAlive)
        {
            attachedTo.DebugLog($"Dying of {type}");
            KillMessage killMessage = new KillMessage(attacker, this, type);
            onDeath.Invoke(killMessage);
            Notification<KillMessage>.Transmit(killMessage);
        }
        */

        // If this attack is the one that killed the entity, run death events
        if (IsAlive || !wasAlive) return;

        
        // If the player, check that they can respawn at a checkpoint
        if (attachedTo is Player player)
        {
            CheckpointManager checkpointManager = FindObjectOfType<CheckpointManager>();
            if (checkpointManager != null && checkpointManager.targetPlayer == player)
            {
                checkpointManager.RespawnAtLastCheckpoint();
                return;
            }
        }
        
        

        attachedTo.DebugLog($"Dying of {type}");
        KillMessage killMessage = new KillMessage(attacker, this, type);
        onDeath.Invoke(killMessage);
        Notification<KillMessage>.Transmit(killMessage);
    }
    public void Heal(int value, Entity healer, Entity toolUsed) => Damage(-value, 0, false, DamageType.Healing, healer, toolUsed, Vector3.zero);
    
    public void DamageFromPhysicsCollision(Collision collision, Hitbox hitbox)
    {
        // Don't bother with calculations if entity is already dead
        if (IsAlive == false && allowPosthumousDamage == false) return;
        // Ensure the entity can't be damaged by its own colliders
        if (attachedTo.colliders.Contains(collision.collider)) return;

        #region Calculate collision force, cancel if too low
        Vector3 relativeVelocity = GetRelativeVelocityOfPhysicsImpact(collision, hitbox);
        float force = relativeVelocity.magnitude;
        Rigidbody rb = collision.rigidbody;

        // Multiply the force based on the angle of the normal and relative velocity.
        // This ensures that entities don't take ridiculous amounts of damage just from scrapes.
        Vector3 normal = MiscFunctions.GetAverageCollisionNormal(collision);
        float dotProduct = Vector3.Dot(relativeVelocity, normal);
        //Debug.Log($"{attachedTo}: Hit dot product = {dotProduct}");
        dotProduct = Mathf.Clamp01(dotProduct);
        force *= dotProduct;

        // If the force isn't enough to register, cancel.
        // We don't want things constantly taking chip damage from the most miniscule impacts
        //attachedTo.DebugLog($"{hitbox} impacted with {collision.collider}, velocity = {force}/{minimumCollisionForceToDamage}");
        if (force <= minimumCollisionForceToDamage) return;

        // Multiply physics damage based on the incoming mass
        if (rb != null)
        {
            force *= PhysicsCache.TotalMassOfConnectedRigidbodies(rb);
        }
        else
        {
            force *= multiplierForStaticCollisions;
        }

        /*
        // If the force isn't enough to register, cancel.
        // We don't want things constantly taking chip damage from the most miniscule impacts
        if (force <= minimumCollisionForceToDamage) return;
        */
        #endregion

        #region Check if the entity can take damage from the colliding object at this time


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
        attachedTo.DebugLog($"{damagedBy} will damage {attachedTo} in {hitbox}, force = {force}/{minimumCollisionForceToDamage}, on frame {Time.frameCount}");

        // Calculate damage and stun accordingly
        float damage = force * damagePerCollisionForceUnit;
        float stun = force * stunPerCollisionForceUnit;

        Entity thingThatDamagedThisHitbox = collision.gameObject.GetComponentInParent<Entity>();


























        /*
        // Obtain launch data for target and incoming object
        // If only one has data assigned, use that.
        // If both exist, check which one was launched first (prioritise whichever was launched by an actual player)
        // Then register that value as the attacker.

        // If neither has assigned launch data, use the incoming object itself as the attacker






        if (PhysicsCache.GetObjectLaunchData(incomingRigidbody, out var incoming) || PhysicsCache.GetObjectLaunchData(attachedTo.rigidbody, out var self))
        {
            PhysicsCache.ObjectLaunchData best = MiscFunctions.GetBest((toCheck) =>
            {

            }, true, incoming, self);


        }
        */

        // Deal damage (use the hitbox's main damage function to calculate things like resistances)
        hitbox.Damage(Mathf.RoundToInt(damage), Mathf.RoundToInt(stun), DamageType.PhysicsImpact, thingThatDamagedThisHitbox, thingThatDamagedThisHitbox, collision.relativeVelocity.normalized, hitbox.isCritical);
        #endregion
    }

    /// <summary>
    /// Obtains the relative velocity of a collision, but accounting for kinematic child bodies (in case they register the hit but are attached to a non-kinematic parent rigidbody).
    /// </summary>
    /// <param name="collision"></param>
    /// <param name="hitbox"></param>
    /// <returns></returns>
    static Vector3 GetRelativeVelocityOfPhysicsImpact(Collision collision, Hitbox hitbox)
    {
        // First, get it the normal way.
        Vector3 relativeVelocity = collision.relativeVelocity;
        if (relativeVelocity.magnitude > 0.01f) return relativeVelocity;

        // If force is zero or basically zero, that might be because the hitbox is part of a kinematic child collider compared to an actually active rigidbody.
        // Check if at least one rigidbody exists and is kinematic
        Rigidbody incomingRigidbody = collision.rigidbody;
        Rigidbody hitRigidbody = hitbox.rigidbody;
        if (hitRigidbody != null && hitRigidbody.isKinematic == false) return relativeVelocity;
        if (hitRigidbody != null && hitRigidbody.isKinematic == false) return relativeVelocity;

        // If not, check in the parent values
        //attachedTo.DebugLog($"Miniscule collision detected, check for non-kinematic parents. {hitRigidbody}, {incomingRigidbody}, {force}");
        Rigidbody hit = MiscFunctions.GetComponentInParentWhere<Rigidbody>(hitbox.transform, (rb) => rb.isKinematic == false);
        Rigidbody incoming = MiscFunctions.GetComponentInParentWhere<Rigidbody>(collision.collider.transform, (rb) => rb.isKinematic == false);
        bool hitExists = hit != null;
        bool incomingExists = incoming != null;

        // If both values exist, just subtract incoming from hit to get the relative velocity!
        if (hitExists && incomingExists) return hit.velocity - incoming.velocity;

        // If neither exists (somehow), there's no physics occurring in the first place.
        if (!hitExists && !incomingExists) return Vector3.zero;

        // If hit is stationary, relative velocity is just incoming velocity (incoming - zero)
        // If incoming is stationary, relative velocity is just opposite of hit velocity (zero - hit)
        Vector3 velocity = Vector3.zero;
        if (incomingExists) velocity += incoming.velocity;
        else if (hitExists) velocity -= hit.velocity;

        // Check the directions of the relative velocity and collision normal.
        // If the angle is obtuse (dot product is less than zero), the two objects are moving away from each other and therefore not colliding.
        Vector3 normal = MiscFunctions.GetAverageCollisionNormal(collision);
        float dotProduct = Vector3.Dot(velocity, normal);
        if (dotProduct < 0) return Vector3.zero;

        // Return the functional relative velocity
        return velocity;
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