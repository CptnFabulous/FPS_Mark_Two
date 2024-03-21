using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string properName = "New Entity";
    public string description = "A new entity.";
    public bool isUnique;

    public Health health;

    IList<Collider> _colliders;
    Rigidbody _rb;

    /// <summary>
    /// The bounds. I might need to make this abstract, once I figure out what type of entity to make the bullets.
    /// </summary>
    public Bounds bounds
    {
        get
        {
            if (colliders != null && colliders.Count > 0) return MiscFunctions.CombinedBounds(colliders);
            return new Bounds(transform.position, Vector3.zero);
        }
    }
    public Vector3 CentreOfMass => bounds.center;

    public virtual IList<Collider> colliders
    {
        get
        {
            if (_colliders != null) return _colliders;

            // Check if health script is present
            if (health != null)
            {
                // If so, generate a list from the hitbox colliders
                _colliders = new Collider[health.hitboxes.Length];
                for (int i = 0; i < _colliders.Count; i++)
                {
                    _colliders[i] = health.hitboxes[i].collider;
                }
            }
            else
            {
                // Otherwise, just obtain a list of child colliders
                _colliders = GetComponentsInChildren<Collider>(false);
            }

            return _colliders;
        }
    }
    public Rigidbody rigidbody => _rb ??= GetComponentInChildren<Rigidbody>();

    protected virtual void Awake()
    {
        if (!isUnique && !string.IsNullOrEmpty(properName)) gameObject.name = properName;
    }

    public bool IsHostileTowards(Entity target)
    {
        Character attackerChar = this as Character;
        Character targetChar = target as Character;

        if (attackerChar == null || targetChar == null) return true;
        
        return attackerChar.affiliation.IsHostileTowards(targetChar.affiliation);
    }

    /*
    public float timeScale = 1;
    public Vector3 gravity = Physics.gravity;

    public float ActiveTime => Time.time * timeScale;
    public float CurrentTimeScale => Time.timeScale * timeScale;
    public float DeltaTime => Time.deltaTime * timeScale;
    public float FixedDeltaTime => Time.fixedDeltaTime * timeScale;
    */
    public virtual void Delete() => Destroy(gameObject);
}
