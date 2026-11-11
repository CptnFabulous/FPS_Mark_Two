using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckpointManager : MonoBehaviour
{
    //static CheckpointManager _instance;

    //public static CheckpointManager current => MiscFunctions.FindComponentSingleton(ref _instance);

    [Header("Checkpoints")]
    public Transform[] checkpoints;
    public float proximityToEstablishNewCheckpoint = 2;
    public LayerMask collisionMask;

    [Header("Respawning")]
    [Range(0.1f, 1)] public float healthRatioOnRespawn = 0.5f;
    public ParticleSystem effect;
    public UnityEvent<Player> onRespawn;

    [Header("Debug")]
    public int currentCheckpointIndex = 0;
    Player _target;

    public int deathCount { get; private set; } = 0;

    public Player targetPlayer => _target ??= FindAnyObjectByType<Player>();

    void Update()
    {
        // Check positions of later checkpoints, to see if the player is close enough to change the checkpoint
        for (int i = currentCheckpointIndex + 1; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null) continue;

            Vector3 checkpointPosition = checkpoints[i].position;
            Vector3 playerPosition = targetPlayer.transform.position;

            // Check that player is close enough
            float distance = Vector3.Distance(checkpointPosition, playerPosition);
            if (distance > proximityToEstablishNewCheckpoint) continue;

            // Line of sight check in case the checkpoint is on the other side of a thin wall
            bool lineOfSight = AIAction.LineOfSight(checkpointPosition, playerPosition, out _, collisionMask, targetPlayer.HitOwnCollider);
            if (!lineOfSight) continue;

            // Update index to represent new checkpoint
            Debug.Log("Updating checkpoint to " + checkpoints[i]);
            currentCheckpointIndex = i;
        }
    }

    public static CheckpointManager FindByPlayer(Player player)
    {
        CheckpointManager checkpointManager = FindObjectOfType<CheckpointManager>();
        /*
        if (checkpointManager == null) return null;
        if (ch)


        if (checkpointManager != null && checkpointManager.targetPlayer == player)
        {
            checkpointManager.TryRespawnAtLastCheckpoint();
            return;
        }
        */
        return checkpointManager;
    }

    public bool TryRespawnAtLastCheckpoint()
    {
        if (checkpoints.Length == 0) return false;

        targetPlayer.stateHandler.CurrentState = PlayerStateHandler.PlayerState.Active;
        RespawnAtCheckpoint(targetPlayer, checkpoints[currentCheckpointIndex]);
        return true;
    }

    public void RespawnAtCheckpoint(Player player, Transform checkpoint)
    {
        player.SetAliveState(true);


        player.transform.position = checkpoint.position;
        player.lookController.lookRotation = checkpoint.rotation;
        player.rigidbody.velocity = Vector3.zero;

        int amountToHeal = Mathf.CeilToInt(player.health.data.max * healthRatioOnRespawn);
        player.health.Heal(amountToHeal, null, null);

        deathCount += 1;

        if (effect != null)
        {
            effect.transform.position = checkpoint.position;
            effect.transform.rotation = checkpoint.rotation;
            effect.gameObject.SetActive(true);
            effect.Play();
        }
        onRespawn.Invoke(player);
    }
    
}
