using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectiveStatus
{
    Inactive,
    Active,
    Completed,
    Failed
}

public abstract class Objective : MonoBehaviour
{
    public bool optional;
    public List<Objective> prerequisites;

    ObjectiveHandler _h;

    public ObjectiveHandler handler => _h ??= GetComponentInParent<ObjectiveHandler>();
    public Player targetPlayer => handler.targetPlayer;
    public ObjectiveStatus status
    {
        get
        {
            // This objective cannot be revealed yet if any of the prerequisites aren't completed
            foreach (Objective prerequisite in prerequisites)
            {
                if (prerequisite.status != ObjectiveStatus.Completed) return ObjectiveStatus.Inactive;
            }
            // Check if passed, or failed
            if (DetermineSuccess()) return ObjectiveStatus.Completed;
            if (DetermineFailure()) return ObjectiveStatus.Failed;
            // Otherwise is active
            return ObjectiveStatus.Active;
        }
    }
    public string serialisedProgress
    {
        get => GetSerializedProgress();
        set => Setup(value);
    }
    /// <summary>
    /// Returns a formatted string representing the player's progress on this objective.
    /// </summary>
    /// <returns></returns>
    public virtual string formattedProgress => null;
    /// <summary>
    /// The location of the objective, for setting markers.
    /// </summary>
    public abstract Nullable<Vector3> location { get; }

    /// <summary>
    /// Updates the current objective to reflect the player's saved progress.
    /// </summary>
    protected abstract void Setup(string progress);
    /// <summary>
    /// Obtains whatever progress the player has made, in a serialisable value.
    /// </summary>
    protected abstract string GetSerializedProgress();

    /// <summary>
    /// Has the player met the requirements to complete the objective?
    /// </summary>
    protected abstract bool DetermineSuccess();
    /// <summary>
    /// Has the player done something that renders the objective unable to be completed?
    /// </summary>
    protected virtual bool DetermineFailure() => false;
    /// <summary>
    /// What needs to happen when the player completes the objective?
    /// </summary>
    public virtual void OnCompleted() { }
    /// <summary>
    /// What needs to happen if the player fails the objective?
    /// </summary>
    public virtual void OnFailed() { }

}