using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIStateFunction : StateFunction
{
    public Sprite icon; // The icon that appears as a visual shorthand to show the enemy's state has changed
    public DiegeticSound soundCue; // Sounds that the enemy makes to signal that it's switched to the new state

    AI _root;
    public AI rootAI => _root ??= GetComponentInParent<AI>();
    public NavMeshAgent navMeshAgent => rootAI.agent;
    public AIAim aim => rootAI.aiming;
    public FieldOfView visionCone => rootAI.visionCone;
    public DiegeticAudioListener hearing => rootAI.hearing;
    public AITargetManager targetManager => rootAI.targeting;
    public Vector3 standingPosition => navMeshAgent.transform.position;

    protected virtual void OnEnable()
    {
        // Play effects to indicate the AI has switched to a new action
        rootAI.statusIcon.TriggerAnimation(icon);
        // Include a thing here that plays the sound effect
        soundCue?.Play(rootAI.transform.position, rootAI);

    }
}
