using CptnFabulous.MiscUtility;
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
    Crushing,
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
    public bool godmode = false;

    [Header("Stats")]
    public Resource data = new Resource(100, 100, 20);
    public string deadDescription = "dead";
    public CharacterPoise poise;

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

    //static float minimumCollisionForceToDamage = 10;
    //static float damagePerCollisionForceUnit = 0.5f;
    //static float stunPerCollisionForceUnit = 1f;
    static float minTimeBetweenCollisions = 0.5f;
    static float minTimeAfterThrowBeforeCollision = 1f;
    static float multiplierForStaticCollisions = 1;

    public static System.Action<DamageMessage> onEntityDamaged;
    public static System.Action<KillMessage> onEntityKilled;

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


        if (poise != null && stun > 0)
        {
            poise.ApplyStun(damageMessage);
        }


        (isHealing ? onHeal : onDamage).Invoke(damageMessage);
        onEntityDamaged?.Invoke(damageMessage);

        // If this attack is the one that killed the entity, run death events
        if (IsAlive || !wasAlive) return;

        // Send kill message
        attachedTo.DebugLog($"Dying of {type}");
        KillMessage killMessage = new KillMessage(attacker, this, type);
        onDeath.Invoke(killMessage);
        onEntityKilled?.Invoke(killMessage);
    }
    public void Heal(int value, Entity healer, Entity toolUsed) => Damage(-value, 0, false, DamageType.Healing, healer, toolUsed, Vector3.zero);
    
    public void DamageFromPhysicsCollision(Collision collision, Hitbox hitbox)
    {
        // Don't bother with calculations if entity is already dead
        if (IsAlive == false && allowPosthumousDamage == false) return;
        // Ensure the entity can't be damaged by its own colliders
        if (attachedTo.colliders.Contains(collision.collider)) return;

        // TO DO: don't allow if some of the entity's colliders aren't supposed to register collisions under the current conditions
        // E.g. a ragdoll collider hitting a surface, while it's meant to be animated.
        if (attachedTo.ShouldPlayPhysicsEffects(collision) == false) return;

        #region Figure out what damaged this entity

        // Get the entity data of the launched object
        // TO DO: figure out if the object was launched by another entity
        Entity attacker = null;
        Entity incomingEntity = collision.gameObject.GetComponentInParent<Entity>();




        

        /*
        
        // Obtain launch data for target and incoming object
        // If only one has data assigned, use that.
        // If both exist, check which one was launched first (prioritise whichever was launched by an actual player)
        // Then register that value as the attacker.

        if (PhysicsCache.GetObjectLaunchData(incomingRigidbody, out var incoming) || PhysicsCache.GetObjectLaunchData(attachedTo.rigidbody, out var self))
        {
            PhysicsCache.ObjectLaunchData best = MiscFunctions.GetBest((toCheck) =>
            {

            }, true, incoming, self);


        }
        else
        {
            
        }
        */

        // If neither has assigned launch data, use the incoming object itself as the attacker
        attacker = incomingEntity;

        // If the thing that collided with this entity is considered an ally, don't deal damage.
        if (attacker != null && attacker.IsHostileTowards(attachedTo) == false) return;

        #endregion

        Rigidbody incomingRigidbody = collision.rigidbody;
        bool isStaticCollision = incomingRigidbody == null;

        #region Calculate collision force, cancel if too low

        DamageResistanceProfile resistances = hitbox.resistances;
        float threshold = resistances.minimumCollisionForceToDamage;//isStaticCollision ? resistances.staticCollisionForceThreshold : resistances.forceThreshold;



        // What if I just use the relative velocity magnitude?
        // Because then a heavy object would deal the same damage as a light one, even though a heavy object moving at the same speed would put way more hurt on the target.
        // Do I need to separate size and weight?
        // I know I need to separate mass and weight. Weight is dependent on gravity.

        Vector3 relativeVelocity = collision.relativeVelocity;
        float force = relativeVelocity.magnitude;

        // Multiply the force based on the angle of the normal and relative velocity.
        // This ensures that entities don't take ridiculous amounts of damage just from scrapes.
        // It also ensures that collisions are ignored if the colliders are actually meant to be moving away from each other.
        Vector3 normal = PhysicsUtility.GetAverageCollisionNormal(collision).normalized;
        float dotProduct = Vector3.Dot(relativeVelocity.normalized, normal);
        dotProduct = Mathf.Clamp01(dotProduct);
        force *= dotProduct;

        /*
        Rigidbody ownRigidbody = hitbox.rigidbody;

        // Get the two mass values to compare against.
        // Because a static collision will be connected to the environment around it, its effective mass will be so large that it cannot be moved.
        // So just use the largest max value possible (but not infinity because that screws up the mass).
        float ownMass = PhysicsCache.TotalMassOfConnectedRigidbodies(ownRigidbody);
        float incomingMass = isStaticCollision ? float.MaxValue : PhysicsCache.TotalMassOfConnectedRigidbodies(incomingRigidbody);
        
        // Figure out the mass ratio
        float total = ownMass + incomingMass;
        float damageRatio = Mathf.Clamp01(incomingMass / total);

        float magnitude = collision.relativeVelocity.magnitude;
        float force = damageRatio * magnitude;
        attachedTo.DebugLog($"Hit by {incomingEntity}, Force: clamped({incomingMass}/{total}) = {damageRatio}, * {magnitude} = {force}, threshold = {threshold}");
        //attachedTo.DebugLog($"Hit by {incomingEntity}, Force: clamped({incomingMass}/{ownMass} = {damageRatio}) * {magnitude} = {force}, threshold = {threshold}");
        */

        // Impulse based: too inconsistent
        /*
        // TO DO: figure out how to effectively calculate force transferred to rigidbody in collision

        float impulseStrength = collision.ImpulseForceStrength();
        float ourMass = PhysicsCache.TotalMassOfConnectedRigidbodies(ownRigidbody);


        float force = impulseStrength / ownMass;

        // TEMPORARY: since player mass is now 100x what it was, divide impact force by same amount and see what happens
        force *= 0.01f;

        //attachedTo.DebugLog($"Hit by {incomingEntity.name}, accumulated = {ownRigidbody.GetAccumulatedForce()}");
        //attachedTo.DebugLog($"Hit by {incomingEntity.name}, Force: {impulseStrength} / {ourMass} * 0.01f = {force}, threshold = {threshold}");
        //attachedTo.DebugLog($"Checking collision damage: {force}/{threshold}");
        */

        /*
        Vector3 relativeVelocity = GetRelativeVelocityOfPhysicsImpact(collision, hitbox);
        float force = relativeVelocity.magnitude;

        // Multiply the force based on the angle of the normal and relative velocity.
        // This ensures that entities don't take ridiculous amounts of damage just from scrapes.
        Vector3 normal = PhysicsUtility.GetAverageCollisionNormal(collision).normalized;
        float dotProduct = Vector3.Dot(relativeVelocity.normalized, normal);
        //attachedTo.DebugLog($"{attachedTo}: Hit dot product = {dotProduct}");
        dotProduct = Mathf.Clamp01(dotProduct);
        force *= dotProduct;



        // If the force isn't enough to register, cancel.
        // We don't want things constantly taking chip damage from the most miniscule impacts
        //attachedTo.DebugLog($"{hitbox} impacted with {collision.collider}, velocity = {force}/{minimumCollisionForceToDamage}");
        if (force <= threshold) return;

        // Multiply physics damage based on the incoming mass
        float massMultiplier = (!isStaticCollision) ? PhysicsCache.TotalMassOfConnectedRigidbodies(incomingRigidbody) : multiplierForStaticCollisions;
        force *= massMultiplier;
        */

        // If the force isn't enough to register, cancel.
        // We don't want things constantly taking chip damage from the most miniscule impacts
        if (force <= threshold) return;
        
        
        #endregion

        #region Check if the entity can take damage from the colliding object at this time

        // Check the root rigidbody this entity is attached to.
        GameObject damagedBy = incomingRigidbody != null ? PhysicsCache.GetRootRigidbody(incomingRigidbody).gameObject : collision.gameObject;

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

        #endregion

        //attachedTo.DebugLog($"{damagedBy} will damage {attachedTo} in {hitbox}. Force = {relativeVelocity.magnitude} * {dotProduct} * {massMultiplier} = {force}/{threshold}, on frame {Time.frameCount}");

        #region Calculate damage and stun values

        // Calculate damage and stun accordingly
        float damage = force * resistances.damagePerCollisionForceUnit;
        float stun = force * resistances.stunPerCollisionForceUnit;

        float multiplier = resistances[DamageType.PhysicsImpact];
        if (multiplier == 0) return;
        damage = Mathf.RoundToInt(damage * multiplier);
        stun = Mathf.RoundToInt(stun * multiplier);

        #endregion

        #region Deal damage and stun

        // Deal damage (use the hitbox's main damage function to calculate things like resistances)
        hitbox.Damage(Mathf.RoundToInt(damage), Mathf.RoundToInt(stun), DamageType.PhysicsImpact, attacker, incomingEntity, collision.relativeVelocity.normalized, hitbox.isCritical);
        recentPhysicsCollisions[damagedBy] = Time.time; // Update the last time hit for the next check

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