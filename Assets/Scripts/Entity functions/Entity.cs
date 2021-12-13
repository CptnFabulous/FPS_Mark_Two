using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string properName = "New Entity";
    public string description = "A new entity.";
    public bool isUnique;

    /*
    public float timeScale = 1;
    public Vector3 gravity = Physics.gravity;

    public float ActiveTime
    {
        get
        {
            return Time.time * timeScale;
        }
    }
    public float CurrentTimeScale
    {
        get
        {
            return Time.timeScale * timeScale;
        }
    }
    public float DeltaTime
    {
        get
        {
            return Time.deltaTime * timeScale;
        }
    }
    public float FixedDeltaTime
    {
        get
        {
            return Time.fixedDeltaTime * timeScale;
        }
    }
    */
    public virtual void Delete()
    {
        //Debug.Log("Destroying " + name + " on frame " + Time.frameCount);
        Destroy(gameObject);
    }
}
