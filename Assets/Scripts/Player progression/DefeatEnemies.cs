using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DefeatEnemies : Objective
{
    [Header("Enemy data")]
    public List<Character> enemies;
    [Tooltip("If assigned, will check in this transform for more enemies")]
    public Transform parentForFindingMoreEnemies;
    public string counterFormat = "{0} remaining";

    protected override bool DetermineSuccess()
    {
        foreach (Character c in enemies)
        {
            if (c.health.IsAlive) return false;
        }
        return true;
    }

    protected override string GetSerializedProgress()
    {
        throw new System.NotImplementedException();
    }

    protected override void Setup(string progress)
    {
        if (parentForFindingMoreEnemies != null)
        {
            IEnumerable<Character> moreEnemies = parentForFindingMoreEnemies.GetComponentsInChildren<Character>();
            moreEnemies = moreEnemies.Where(targetPlayer.IsHostileTowards); // Ignore allies
            moreEnemies = moreEnemies.Where((e) => e.health.IsAlive); // Ignore already-dead enemies
            enemies.AddRange(moreEnemies);
        }
        
        foreach (Character c in enemies)
        {
            c.gameObject.SetActive(true);
        }
    }

    public override string formattedProgress
    {
        get
        {
            if (string.IsNullOrEmpty(counterFormat)) return base.formattedProgress;

            int remaining = 0;
            foreach (Character c in enemies)
            {
                if (c.health.IsAlive) remaining++;
            }

            //int total = enemies.Count;
            //int killed = total - remaining;

            return string.Format(counterFormat, remaining);
        }
    }

    public override Vector3? location
    {
        get
        {
            return null;

            // TO DO: add bounds data later
            Bounds b = enemies[0].bounds;
            for (int i = 1; i < enemies.Count; i++)
            {
                if (enemies[i].health.IsAlive == false) continue;
                b.Encapsulate(enemies[i].bounds);
            }
            return b.center;
        }
    }
}
