using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackMessage
{
    //public static new System.Action<AttackMessage> Receivers;

    public Character origin;
    public LayerMask hitDetection;
    public int baseDamage;

    public bool AtRisk(Character c, int damageThresholdToAvoid)
    {
        // Check if the attacking character is hostile towards the theoretical victim
        Character attacker = origin as Character;
        if (attacker != null && attacker.IsHostileTowards(c) == false) return false;

        // Checks if position is safe and the damage is actually high enough to justify dodging
        if (PositionAtRisk(c, c.transform.position, damageThresholdToAvoid, out int blah) == false) return false;

        return true;
    }
    /// <summary>
    /// Is a character in the path of an attack, is it dangerous enough it has to be avoided, and how much damage will they take?
    /// </summary>
    /// <param name="c"></param>
    /// <param name="transformPosition"></param>
    /// <param name="potentialDamage"></param>
    /// <returns></returns>
    public bool PositionAtRisk(Character c, Vector3 transformPosition, int damageThresholdToAvoid,  out int potentialDamage)
    {
        // Checks if character's hitbox bounds are inside attack zone
        // Uses offset between bounds centre and transform position to shift bounds to a hypothetical different position
        Bounds characterBounds = c.health.HitboxBounds;
        characterBounds.center -= c.transform.position;
        characterBounds.center += transformPosition;
        return PositionAtRisk(characterBounds, c.health.HitboxColliders, out potentialDamage) && potentialDamage > damageThresholdToAvoid;
    }
    /// <summary>
    /// Is a bounds in the path of an attack, and how much damage will they take?
    /// </summary>
    /// <param name="characterBounds"></param>
    /// <param name="characterHitboxes"></param>
    /// <param name="potentialDamage"></param>
    /// <returns></returns>
    public abstract bool PositionAtRisk(Bounds characterBounds, Collider[] characterHitboxes, out int potentialDamage);
}
public class DirectionalAttackMessage : AttackMessage
{
    public DirectionalAttackMessage(Character _attacker, int _baseDamage, /*AnimationCurve _damageFalloff, */Vector3 _originPoint, Vector3 _direction, float _range, float _spread, LayerMask _hitDetection)
    {
        origin = _attacker;
        baseDamage = _baseDamage;
        //damageFalloff = _damageFalloff;
        hitDetection = _hitDetection;
        originPoint = _originPoint;
        direction = _direction;
        range = _range;
        spread = _spread;
    }
    
    public Vector3 originPoint;
    public Vector3 direction;
    public float range;
    public float spread;
    //public AnimationCurve damageFalloff;

    public override bool PositionAtRisk(Bounds bounds, Collider[] characterHitboxes, out int potentialDamage)
    {
        // Check range
        float distanceToTarget = Vector3.Distance(originPoint, bounds.ClosestPoint(originPoint));
        potentialDamage = baseDamage;//Mathf.RoundToInt(baseDamage * damageFalloff.Evaluate(distanceToTarget / 1));

        if (distanceToTarget >= range) return false;

        // Check angle. A special 'closest point' position is created in case a character's transform is outside the attack zone but some of their colliders are.
        float checkDistance = Mathf.Min(range, Vector3.Distance(originPoint, bounds.center));
        Vector3 closestPointInsideAngle = bounds.ClosestPoint(originPoint + checkDistance * direction.normalized);
        float targetAngle = Vector3.Angle(direction, closestPointInsideAngle - originPoint);
        if (targetAngle >= spread) return false;

        // Check if an unobstructed linear path is available between the origin and target point
        if (AIAction.LineOfSight(originPoint, closestPointInsideAngle, hitDetection, origin.health.HitboxColliders, characterHitboxes) == false)
        {
            return false;
        }

        return true;
    }
}
public class AOEAttackMessage : AttackMessage
{
    public AOEAttackMessage(Character _attacker, int _baseDamage, AnimationCurve _damageFalloff, Vector3 _centre, float _radius, LayerMask _hitDetection)
    {
        origin = _attacker;
        baseDamage = _baseDamage;
        damageFalloff = _damageFalloff;
        centre = _centre;
        radius = _radius;
        hitDetection = _hitDetection;
    }

    public Vector3 centre;
    public float radius;
    public AnimationCurve damageFalloff;

    public override bool PositionAtRisk(Bounds bounds, Collider[] characterHitboxes, out int potentialDamage)
    {
        // Check range
        Vector3 closestPoint = bounds.ClosestPoint(centre);
        float distanceToTarget = Vector3.Distance(centre, closestPoint);
        potentialDamage = Mathf.RoundToInt(baseDamage * damageFalloff.Evaluate(distanceToTarget / 1));

        if (distanceToTarget >= radius) return false;

        // Check if an unobstructed linear path is available between the origin and target point
        if (AIAction.LineOfSight(centre, closestPoint, hitDetection, origin.health.HitboxColliders, characterHitboxes) == false)
        {
            return false;
        }

        return true;
    }
}