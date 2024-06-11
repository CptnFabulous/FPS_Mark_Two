using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsCache
{
    static Dictionary<Rigidbody, Rigidbody> rootDictionary = new Dictionary<Rigidbody, Rigidbody>();

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
}