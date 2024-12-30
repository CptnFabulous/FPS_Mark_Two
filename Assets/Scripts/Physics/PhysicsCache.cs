using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsCache
{
    static Dictionary<Rigidbody, Rigidbody> rootDictionary = new Dictionary<Rigidbody, Rigidbody>();
    static Dictionary<Rigidbody, float> massDictionary = new Dictionary<Rigidbody, float>();
    static Dictionary<Rigidbody, Rigidbody[]> childDictionary = new Dictionary<Rigidbody, Rigidbody[]>();
    static Dictionary<Rigidbody, Collider[]> colliderDictionary = new Dictionary<Rigidbody, Collider[]>();

    public static Rigidbody GetRootRigidbody(Rigidbody rb)
    {
        if (rb == null) return null;

        if (rootDictionary.TryGetValue(rb, out Rigidbody r)) return r;

        // Check for a joint, and get its connected body.
        // If neither are found, break the loop.
        // Whatever was last assigned to 'root' is what we need.
        Rigidbody root = rb;
        while (root.TryGetComponent(out Joint j) && j.connectedBody != null)
        {
            // If we found a connected body, re-iterate the check until we reach the end of the chain.
            root = j.connectedBody;
        }
        rootDictionary[rb] = root;

        return root;
    }
    public static float TotalMassOfConnectedRigidbodies(Rigidbody rb)
    {
        if (rb == null) return 1;

        rb = GetRootRigidbody(rb);

        float mass;
        // If a value is already stored, get that
        if (massDictionary.TryGetValue(rb, out mass)) return mass;

        // Calculate total mass from child rigidbodies, and cache it to save processing
        foreach (Rigidbody child in GetChildRigidbodies(rb))
        {
            mass += child.mass;
        }
        massDictionary[rb] = mass;

        return mass;
    }
    public static Rigidbody[] GetChildRigidbodies(Rigidbody target)
    {
        target = GetRootRigidbody(target);
        // Check for pre-cached value
        if (childDictionary.TryGetValue(target, out var array)) return array;
        // Find and cache value
        childDictionary[target] = target.GetComponentsInChildren<Rigidbody>();
        return childDictionary[target];
    }
    public static Collider[] GetChildColliders(Rigidbody target)
    {
        target = GetRootRigidbody(target);
        // Check for pre-cached value
        if (colliderDictionary.TryGetValue(target, out var array)) return array;
        // Find and cache value
        colliderDictionary[target] = target.GetComponentsInChildren<Collider>();
        return colliderDictionary[target];
    }

}

public static class ComponentCache<T>
{
    static Dictionary<GameObject, T> cache = new Dictionary<GameObject, T>();
    
    public static T Get(GameObject target)
    {
        // If a value is already cached, reference that
        if (cache.TryGetValue(target, out T e)) return e;

        // Check upwards in hierarchy
        cache[target] = target.GetComponentInParent<T>();
        if (cache[target] != null) return cache[target];

        // If a component can't be found, check if the target is on a ragdoll
        // (since ragdolls are separated from their original entities to prevent wonky physics)
        Ragdoll r = target.GetComponentInParent<Ragdoll>();
        if (r == null) return cache[target];

        // If the attached entity is the desired type, assign that
        if (r.attachedTo is T t)
        {
            cache[target] = t;
            return cache[target];
        }

        // Otherwise perform a recursive check upwards from the attached entity
        cache[target] = Get(r.attachedTo.gameObject);
        return cache[target];
    }

    
}

/// <summary>
/// An older version of Cache<T> that only works for entities
/// </summary>
/// <typeparam name="T"></typeparam>
public static class EntityCache<T> where T : Entity
{
    static Dictionary<GameObject, T> dictionary = new Dictionary<GameObject, T>();

    public static T GetEntity(GameObject g)
    {
        if (dictionary.TryGetValue(g, out T e)) return e;

        // Check in parent
        // If that didn't work, check for a ragdoll and get the attached entity instead
        T toAssign = g.GetComponentInParent<T>();
        if (toAssign == null)
        {
            // If a root entity can't be found, check if the collider is on a ragdoll
            // (since ragdolls are separated from their original entities to prevent wonky physics)
            // If the desired entity type is found, assign it
            Ragdoll r = g.GetComponentInParent<Ragdoll>();
            if (r != null && r.attachedTo is T t) toAssign = t;
        }

        dictionary[g] = toAssign;
        return dictionary[g];
    }
}