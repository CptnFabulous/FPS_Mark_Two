using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Faction", menuName = "ScriptableObjects/Faction", order = 0)]
public class Faction : ScriptableObject
{
    public Sprite icon;
    public Color colour = Color.white;
    [Multiline]
    public string description = "A new faction.\nNo additional information is available.";

    public bool IsHostileTowards(Faction other)
    {
        return IsHostileTowards(this, other);
    }

    /// <summary>
    /// Is the first faction hostile towards the second?
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool IsHostileTowards(Faction a, Faction b)
    {
        // Currently only checks if they're different.
        // I gave the check its own function anyway in case I decide to make the criteria more elaborate, e.g. having alliances
        return a != b;
    }
}
