using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
    public class IndividualObjectPool
    {
        public IndividualObjectPool(Component prefab)
        {
            originalPrefab = prefab;
            standby = new Queue<Component>();
            active = new List<Component>();
            // Set up pool parent
            poolParent = new GameObject($"Object Pool Parent ({originalPrefab})").transform;
            Object.DontDestroyOnLoad(poolParent);
        }

        Component originalPrefab;
        public int maxPrefabs = 100;

        Transform poolParent;
        List<Component> active;
        Queue<Component> standby;

        public Component RequestObject(bool activeByDefault = true)
        {
            // Clear entries for accidentally-destroyed objects
            active.RemoveAll((x) => x == null);

            Component value;
            if (standby.Count > 0) // Check if there are any on standby in the pool
            {
                // Load an existing one
                value = standby.Dequeue();
            }
            else if (maxPrefabs > 0 && active.Count >= maxPrefabs) // Otherwise, check if we're allowed to spawn more or if we've reached the limit and need to re-use existing ones
            {
                // 'Deactivate' the oldest already-active one and re-use it
                value = active[0];
                active.RemoveAt(0);
            }
            else // Otherwise, spawn a brand new one
            {
                value = Object.Instantiate(originalPrefab);
            }

            // Add the value to the list so we know what order it was spawned in
            active.Add(value);
            value.gameObject.SetActive(activeByDefault);
            return value;
        }
        public bool DismissObject(Component toDismiss)
        {
            if (active.Contains(toDismiss) == false) return false;

            // Remove from active list, add to standby queue
            active.Remove(toDismiss);
            standby.Enqueue(toDismiss);
            // Disable object and shuffle it back in with the pool parent
            toDismiss.gameObject.SetActive(false);
            toDismiss.transform.SetParent(poolParent);

            return true;
        }
    }

    // Keeps track of different pools for different prefabs
    static Dictionary<Component, IndividualObjectPool> dictionary;

    /// <summary>
    /// Registers a pool for a prefab, and requests a copy (or creates one if all are currently being used).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefab">The prefab you want to spawn a copy of.</param>
    /// <param name="activeByDefault">Does the object spawn as active or inactive?</param>
    /// <param name="maxPrefabs">Sets how many prefabs can spawn at a time before existing ones start being re-assigned. Zero or less means an unlimited number.</param>
    /// <returns></returns>
    public static T RequestObject<T>(T prefab, bool activeByDefault = true, int maxPrefabs = 0) where T : Component
    {
        // Only this function needs to have a generic type, because each prefab is separated by being dictionary keys anyway.
        // I tried setting it up to use completely generic types, but this meant having to declare the type each time ObjectPool is referenced.

        // Don't do anything if there's no prefab specified
        if (prefab == null) return null;

        // Make sure a dictionary actually exists
        if (dictionary == null) dictionary = new Dictionary<Component, IndividualObjectPool>();

        // TO DO: delete pools whose original prefabs have been destroyed

        // Check if a pool already exists for this prefab
        if (dictionary.ContainsKey(prefab) == false)
        {
            // If not, create one
            dictionary.Add(prefab, new IndividualObjectPool(prefab));
        }

        // Set max number of prefabs (might as well do it here so we can set different max sizes for different prefabs)
        dictionary[prefab].maxPrefabs = maxPrefabs;
        // Request the desired object from that pool.
        return dictionary[prefab].RequestObject(activeByDefault) as T;
    }
    /// <summary>
    /// Use this instead of 'Destroy' to get rid of objects that were spawned from a pool.
    /// </summary>
    /// <param name="toDismiss">The component whose GameObject you want to get rid of.</param>
    public static void DismissObject(Component toDismiss)
    {
        // Don't proceed if there's nothing to dismiss
        if (toDismiss == null) return;

        // Iterate through the pools to see if it's part of one of them. If so, delete and end the function
        foreach (IndividualObjectPool pool in dictionary.Values)
        {
            if (pool.DismissObject(toDismiss)) return;
        }

        // If it's not recognised by one of the pools, just destroy it since it still needs to be gotten rid of
        Object.Destroy(toDismiss.gameObject);
    }
}