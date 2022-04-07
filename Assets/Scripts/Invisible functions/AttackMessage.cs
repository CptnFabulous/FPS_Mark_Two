using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackMessage
{
    //public static new System.Action<AttackMessage> Receivers;

    public Character origin;
    public LayerMask hitDetection;

    public bool AtRisk(Character c) => AtRisk(c, c.transform.position);
    public bool AtRisk(Character c, Vector3 transformPosition)
    {
        // Check if the attacking character is hostile towards the theoretical victim
        Character attacker = origin as Character;
        if (attacker != null && attacker.IsHostileTowards(c) == false) return false;

        // Checks if character's hitbox bounds are inside attack zone
        // Uses offset between bounds centre and transform position to shift bounds to a hypothetical different position
        Bounds characterBounds = c.health.HitboxBounds;
        characterBounds.center -= c.transform.position;
        characterBounds.center += transformPosition;
        if (PositionAtRisk(characterBounds, c.health.HitboxColliders) == false) return false;

        return true;
    }
    
    /// <summary>
    /// Is a set of bounds within the attack zone?
    /// </summary>
    /// <param name="characterBounds"></param>
    /// <param name="characterHitboxes"></param>
    /// <returns></returns>
    public abstract bool PositionAtRisk(Bounds characterBounds, Collider[] characterHitboxes);
}
public class DirectionalAttackMessage : AttackMessage
{
    public DirectionalAttackMessage(Character _attacker, Vector3 _originPoint, Vector3 _direction, float _range, float _spread, LayerMask _hitDetection)
    {
        origin = _attacker;
        originPoint = _originPoint;
        direction = _direction;
        range = _range;
        spread = _spread;
        hitDetection = _hitDetection;
    }
    
    public Vector3 originPoint;
    public Vector3 direction;
    public float range;
    public float spread;

    public override bool PositionAtRisk(Bounds bounds, Collider[] characterHitboxes)
    {
        // Check range
        float distanceToTarget = Vector3.Distance(originPoint, bounds.ClosestPoint(originPoint));
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
    public AOEAttackMessage(Character _attacker, Vector3 _centre, float _radius, LayerMask _hitDetection)
    {
        origin = _attacker;
        centre = _centre;
        radius = _radius;
        hitDetection = _hitDetection;
    }

    public Vector3 centre;
    public float radius;

    public override bool PositionAtRisk(Bounds bounds, Collider[] characterHitboxes)
    {
        // Check range
        Vector3 closestPoint = bounds.ClosestPoint(centre);
        float distanceToTarget = Vector3.Distance(centre, closestPoint);
        if (distanceToTarget >= radius) return false;

        // Check if an unobstructed linear path is available between the origin and target point
        if (AIAction.LineOfSight(centre, closestPoint, hitDetection, origin.health.HitboxColliders, characterHitboxes) == false)
        {
            return false;
        }

        return true;
    }
}