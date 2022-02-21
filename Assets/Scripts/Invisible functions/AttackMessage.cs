using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackMessage
{
    public Character attacker;
    public List<Character> atRisk;

    // Projectile or melee attack
    // Damage
    // Direction
    // Angle

    // Explosive attack
    // Max damage
    // Centre
    // Blast radius


    public abstract bool AtRisk(Character character, int damageThreshold = 0);
    public abstract bool AtRisk(Bounds characterBounds, Collider[] characterColliders);
}

public class DirectionalAttackMessage : AttackMessage
{
    public int damage;
    public Vector3 origin;
    public Vector3 direction;
    public float angle;
    public float range;
    public LayerMask detection;

    public DirectionalAttackMessage(Character _attacker, Vector3 _origin, Vector3 _direction, float _angle, float _range, LayerMask _detection)
    {
        attacker = _attacker;
        origin = _origin;
        direction = _direction;
        angle = _angle;
        range = _range;
        detection = _detection;

        // Find characters in level
        List<Character> characters = new List<Character>(Object.FindObjectsOfType<Character>());
        // Remove enemies not at risk
        characters.RemoveAll(c => AtRisk(c) == false);

    }

    public override bool AtRisk(Character characterData, int damageThreshold = 0)
    {
        // Checks are ran in order from least to most processor intensive, so processing power is not wasted on large checks when a faster check returns a valid answer
        
        // Exclude allies
        if (attacker.IsHostileTowards(characterData) == false)
        {
            return false;
        }

        if (damage <= damageThreshold)
        {
            return false;
        }

        // Exclude enemies outside range, outside angle or behind cover
        // I was originally going to include separate checks for all of these, but I realised that my complex line of sight code accounts for all three values
        if (FieldOfView.ComplexDetectionConeCheck(characterData.health.HitboxColliders, origin, direction, angle, range, out RaycastHit rh, detection) == false)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Is a particular bounds at risk? Since the parameter is just a struct where the centre value can be easily changed, this can be used to detect positions a character is not presently standing on
    /// </summary>
    /// <param name="characterBounds"></param>
    /// <returns></returns>
    public override bool AtRisk(Bounds characterBounds, Collider[] characterColliders)
    {
        // Exclude if outside range
        float maxBoundsExtents = Mathf.Max(MiscFunctions.Vector3Array(characterBounds.extents));
        float distanceToTargetCentre = Vector3.Distance(origin, characterBounds.center);
        if (distanceToTargetCentre - maxBoundsExtents > range) // If distance to target skin is more than the attack's range
        {
            return false;
        }

        // Exclude if outside angle
        Vector3 sameDistanceMiddleOfShot = origin + (direction.normalized * distanceToTargetCentre);
        Vector3 closestPointToAttackCone = characterBounds.ClosestPoint(sameDistanceMiddleOfShot);
        float angleFromAttackPath = Vector3.Angle(closestPointToAttackCone - origin, direction);
        if (angleFromAttackPath > angle) // If the closest point on the character is from at an angle greater than the attack's angle
        {
            return false;
        }

        // Exclude if line of sight is broken
        List<Collider> exceptions = new List<Collider>(attacker.health.HitboxColliders);
        exceptions.AddRange(characterColliders);
        if (AIAction.LineOfSightCheck(origin, characterBounds.center, 0, detection, exceptions) == false)
        {
            return false;
        }

        return true;
    }
}
