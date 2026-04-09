using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public static IReadOnlyList<Entity> existing => _existing;
    static List<Entity> _existing = new List<Entity>();
    
    public bool showDebugData = false;

    public string properName = "New Entity";
    public string description = "[PLACEHOLDER]";
    public bool isUnique;

    public Health health;
    public AudioSource audioSource;
    public Ragdoll ragdoll;

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
        _existing.Add(this);
    }
    private void OnDestroy()
    {
        _existing.Remove(this);
    }

    protected virtual void Die()
    {
        DebugLog($"{this} is now dying");
    }

    public bool IsHostileTowards(Entity target)
    {
        Character attackerChar = this as Character;
        Character targetChar = target as Character;

        if (attackerChar == null || targetChar == null) return true;
        
        return attackerChar.affiliation.IsHostileTowards(targetChar.affiliation);
    }
    /// <summary>
    /// Quickly checks if a raycast was on one of this entity's colliders.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    public bool HitOwnCollider(RaycastHit hit) => MiscFunctions.ArrayContains(colliders, hit.collider);

    /*
    public float timeScale = 1;
    public Vector3 gravity = Physics.gravity;

    public float ActiveTime => Time.time * timeScale;
    public float CurrentTimeScale => Time.timeScale * timeScale;
    public float DeltaTime => Time.deltaTime * timeScale;
    public float FixedDeltaTime => Time.fixedDeltaTime * timeScale;
    */
    public void Delete()
    {
        // If this entity has health, pre-emptively kill it to ensure 'on death' events occur properly
        if (health != null)
        {
            int killDamage = health.data.max * 999;
            health.Damage(killDamage, 0, false, DamageType.DeletionByGame, null, null, Vector3.zero);
        }

        // Delete the actual object.
        // (Unless it's a player, don't delete them so the game over screen can play)
        if (this is Player player == false) Destroy(gameObject);
    }









    public void DebugLog(string message)
    {
        if (showDebugData) Debug.Log($"{this}: {message}, frame {Time.frameCount}");
    }
    public void DebugLogError(string message)
    {
        if (showDebugData) Debug.LogError($"{this}: {message}, frame {Time.frameCount}");
    }
    public void DebugLog(object target)
    {
        if (showDebugData) Debug.Log($"{this}: {target}, frame {Time.frameCount}");
    }
}
