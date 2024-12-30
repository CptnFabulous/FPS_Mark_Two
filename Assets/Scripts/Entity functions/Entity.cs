using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool showDebugData = false;

    public string properName = "New Entity";
    public string description = "[PLACEHOLDER]";
    public bool isUnique;

    public Health health;
    public AudioSource audioSource;

    IList<Collider> _colliders;
    Rigidbody _rb;

    Renderer[] _renderers;

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
            // If colliders are already cached, return those
            if (_colliders != null) return _colliders;

            // If no health script is present, just return all child colliders
            if (health == null)
            {
                _colliders = GetComponentsInChildren<Collider>(false);
                return _colliders;
            }

            // If health script exists, generate a list from the hitbox colliders
            _colliders = new Collider[health.hitboxes.Length];
            for (int i = 0; i < _colliders.Count; i++)
            {
                _colliders[i] = health.hitboxes[i].collider;
            }
            return _colliders;
        }
    }
    public Rigidbody rigidbody => _rb ??= GetComponentInChildren<Rigidbody>();

    public Renderer[] renderers => MiscFunctions.GetImmediateComponentsInChildren<Renderer, Entity>(this, ref _renderers);

    protected virtual void Awake()
    {
        if (!isUnique && !string.IsNullOrEmpty(properName)) gameObject.name = properName;
        if (health != null) health.onDeath.AddListener((_) => Die());
    }


    protected virtual void Die()
    {
        Debug.Log($"{this} is now dying");
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









    public void DebugLog(string message)
    {
        if (showDebugData) Debug.Log($"{this}: {message}, frame {Time.frameCount}");
    }
    public void DebugLog(object target)
    {
        if (showDebugData) Debug.Log($"{this}: {target}, frame {Time.frameCount}");
    }
}
