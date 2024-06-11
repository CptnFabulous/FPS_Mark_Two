using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsCache
{
    static Dictionary<Rigidbody, Rigidbody> rootDictionary = new Dictionary<Rigidbody, Rigidbody>();
    static Dictionary<Rigidbody, Character> characterDictionary = new Dictionary<Rigidbody, Character>();

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
    public static Character GetRootCharacter(Rigidbody target)
    {
        if (characterDictionary.TryGetValue(target, out Character c)) return c;

        // Check for an entity first, so if 'target' is a child of an entity that's a child of a Character, it doesn't return that one by mistake
        Entity e = target.GetComponentInParent<Entity>();
        Character character = e as Character;
        if (character == null)
        {
            // If a 'character' script can't be found, check if the collider is on a ragdoll
            // (since ragdolls are separated from their original entities to prevent wonky physics)
            Ragdoll ragdoll = target.GetComponentInParent<Ragdoll>();
            if (ragdoll != null) character = ragdoll.attachedTo;
        }
        characterDictionary[target] = character;

        return character;
    }
    public static bool PhysicsObjectCharacterCheck(Rigidbody target, out Character character)
    {
        character = GetRootCharacter(target);
        return character != null;
    }
}