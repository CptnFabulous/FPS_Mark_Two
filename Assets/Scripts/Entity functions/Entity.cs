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
    public virtual Bounds bounds => new Bounds();
    public Vector3 CentreOfMass => bounds.center;

    public virtual IList<Collider> colliders => null;

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
