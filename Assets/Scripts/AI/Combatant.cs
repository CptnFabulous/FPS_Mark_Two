using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combatant : AI
{
    [Header("Target")]
    public Character target;

    /// <summary>
    /// Checks if a target has been found and is not dead
    /// </summary>
    /// <returns></returns>
    public System.Func<bool> TargetAcquired() => () =>
    {
        return target != null && target.health.IsAlive == false;
    };
}
