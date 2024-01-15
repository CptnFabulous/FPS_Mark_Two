using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string properName = "New Entity";
    public string description = "A new entity.";
    public bool isUnique;

    /// <summary>
    /// The bounds. I might need to make this abstract, once I figure out what type of entity to make the bullets.
    /// </summary>
    public Bounds bounds
    {
        get
        {
            if (colliders != null) return MiscFunctions.CombinedBounds(colliders);
            return new Bounds(transform.position, Vector3.zero);
        }
    }
    public Vector3 CentreOfMass => bounds.center;

    public virtual IList<Collider> colliders => null;

    protected virtual void Awake()
    {
        if (isUnique == false && string.IsNullOrEmpty(properName) == false) gameObject.name = properName;
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
    public virtual void Delete()
    {
        //Debug.Log("Destroying " + name + " on frame " + Time.frameCount);
        Destroy(gameObject);
    }
}
