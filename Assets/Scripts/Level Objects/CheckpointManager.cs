using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CheckpointManager : MonoBehaviour
{
    public Player targetPlayer;

    [Header("Checkpoints")]
    public Transform[] checkpoints;
    public float proximityToEstablishNewCheckpoint = 2;
    public LayerMask collisionMask;

    [Header("Respawning")]
    [Range(0.1f, 1)] public float healthRatioOnRespawn = 0.5f;
    public ParticleSystem effect;
    public UnityEvent<Player> onRespawn;

    int currentCheckpointIndex = 0;
    public int deathCount { get; private set; } = 0;

    void Update()
    {
        // Check positions of later checkpoints, to see if the player is close enough to change the checkpoint
        for (int i = currentCheckpointIndex + 1; i < checkpoints.Length; i++)
        {
            Vector3 checkpointPosition = checkpoints[i].position;
            Vector3 playerPosition = targetPlayer.transform.position;

            // Check that player is close enough
            float distance = Vector3.Distance(checkpointPosition, playerPosition);
            if (distance > proximityToEstablishNewCheckpoint) continue;

            // Line of sight check in case the checkpoint is on the other side of a thin wall
            bool lineOfSight = AIAction.LineOfSight(checkpointPosition, playerPosition, collisionMask, targetPlayer.colliders);
            if (!lineOfSight) continue;

            // Update index to represent new checkpoint
            Debug.Log("Updating checkpoint to " + checkpoints[i]);
            currentCheckpointIndex = i;
        }
    }

    public void RespawnAtLastCheckpoint() => RespawnAtCheckpoint(targetPlayer, checkpoints[currentCheckpointIndex]);

    public void RespawnAtCheckpoint(Player player, Transform checkpoint)
    {
        player.transform.position = checkpoint.position;
        player.lookController.lookRotation = checkpoint.rotation;

        int amountToHeal = Mathf.CeilToInt(player.health.data.max * healthRatioOnRespawn);
        player.health.Heal(amountToHeal, null, null);

        deathCount += 1;

        if (effect != null)
        {
            effect.transform.position = checkpoint.position;
            effect.transform.rotation = checkpoint.rotation;
            effect.Play();
        }
        onRespawn.Invoke(player);
    }
    
}
