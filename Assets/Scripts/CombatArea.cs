using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatArea : MonoBehaviour
{
    //public bool lockExitsUntilCompleted;
    
    public List<Combatant> allEnemies;
    Player playerFighting;

    Collider zoneCollider;
    Rigidbody zoneRigidbody;


    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
        zoneRigidbody = GetComponent<Rigidbody>();
        if (zoneRigidbody == null)
        {
            zoneRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        zoneRigidbody.isKinematic = true;

        allEnemies = new List<Combatant>(GetComponentsInChildren<Combatant>());
    }

    private void OnTriggerEnter(Collider other)
    {
        Player entering = other.GetComponentInParent<Player>();
        if (entering == null) // Collider entering zone was not a player
        {
            return;
        }

        playerFighting = entering;

        // Aggro enemies towards player
        allEnemies.RemoveAll((c) => c == null || c.character.health.IsAlive == false); // Remove empty entries and entries where enemy is already dead
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].character.IsHostileTowards(playerFighting) && allEnemies[i].target == null)
            {
                allEnemies[i].target = playerFighting;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player leaving = other.GetComponentInParent<Player>();
        if (leaving == null) // Collider entering zone was not a player
        {
            return;
        }

        // Deregister player from combat zone
        if (leaving == playerFighting)
        {
            playerFighting = null;
        }

        // De-aggro enemies
        allEnemies.RemoveAll((c) => c == null || c.character.health.IsAlive == false); // Remove empty entries and entries where enemy is already dead
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].target == leaving)
            {
                allEnemies[i].target = null;
            }
        }
    }


}
