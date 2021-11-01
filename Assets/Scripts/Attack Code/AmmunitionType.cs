using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ammunition Type", menuName = "ScriptableObjects/Ammunition Type", order = 0)]
public class AmmunitionType : ScriptableObject
{
    public string description;
    public Sprite icon;

    public static AmmunitionType[] All
    {
        get
        {
            if (allTypes == null)
            {
                allTypes = Resources.LoadAll<AmmunitionType>("");
            }
            return allTypes;
        }
    }
    static AmmunitionType[] allTypes;

    public static int GetIndex(AmmunitionType type)
    {
        for (int i = 0; i < All.Length; i++)
        {
            if (All[i] == type)
            {
                return i;
            }
        }
        return -1;
    }
}
