using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CombatArea : MonoBehaviour
{
    //public bool lockExitsUntilCompleted;
    
    public List<Combatant> remainingEnemies;
    public UnityEvent onPlayerEnter;
    public UnityEvent onAllEnemiesDefeated;
    public UnityEvent onPlayerExit;

    Player playerFighting;

    

    private void Awake()
    {
        remainingEnemies = new List<Combatant>(GetComponentsInChildren<Combatant>());
        EventHandler.Subscribe(CheckKills, true);
    }


    public void Activate(Player entering)
    {
        if (remainingEnemies.Count <= 0) // If no enemies are left, no need to do anything
        {
            return;
        }

        playerFighting = entering;

        // Aggro enemies towards player
        for (int i = 0; i < remainingEnemies.Count; i++)
        {
            if (remainingEnemies[i].controlling.IsHostileTowards(playerFighting) && remainingEnemies[i].target == null)
            {
                remainingEnemies[i].target = playerFighting;
            }
        }

        onPlayerEnter.Invoke();
    }

    public void PauseEncounter(Player leaving)
    {
        // Deregister player from combat zone
        if (leaving == playerFighting)
        {
            playerFighting = null;
        }

        // De-aggro enemies
        for (int i = 0; i < remainingEnemies.Count; i++)
        {
            if (remainingEnemies[i].target == leaving)
            {
                remainingEnemies[i].target = null;
            }
        }

        onPlayerExit.Invoke();
    }

    void CheckKills(KillMessage message)
    {
        // Remove empty entries and entries where enemy is already dead
        remainingEnemies.RemoveAll((c) => c == null || c.controlling.health.IsAlive == false);
        if (remainingEnemies.Count <= 0) // If all enemies are defeated
        {
            onAllEnemiesDefeated.Invoke();
            playerFighting = null;
        }
    }
}
