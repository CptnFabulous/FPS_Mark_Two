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
    [Multiline] public string description;
    public bool optional;
    public List<Objective> prerequisites;

    bool alreadyCompleted = false;

    ObjectiveHandler _h;


    public ObjectiveHandler handler => _h ??= GetComponentInParent<ObjectiveHandler>();
    public Player targetPlayer => handler.targetPlayer;
    public ObjectiveStatus status
    {
        get
        {
            if (alreadyCompleted) return ObjectiveStatus.Completed;

            // This objective cannot be revealed yet if any of the prerequisites aren't completed
            foreach (Objective prerequisite in prerequisites)
            {
                if (prerequisite.status != ObjectiveStatus.Completed) return ObjectiveStatus.Inactive;
            }
            // Check if passed, or failed
            if (DetermineSuccess())
            {
                alreadyCompleted = true;
                return ObjectiveStatus.Completed;
            }
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
    /// Returns a formatted string representing the progress info.
    /// </summary>
    /// <returns></returns>
    public virtual string formattedProgress => null;
    /// <summary>
    /// The specified location of the objective, for setting markers.
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

    #region
    /// <summary>
    /// Has the player met the requirements to complete the objective?
    /// </summary>
    protected abstract bool DetermineSuccess(); // TO DO: add a bit of code to permanently check it off as completed, if it's active and once the conditions are met. So that it doesn't 'uncomplete' itself (unless I enable something for that)
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
    #endregion
}