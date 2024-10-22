using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    [Tooltip("These must be completed before this objective can start being progressed towards.")]
    public List<Objective> prerequisites;
    [Tooltip("If filled in, these objectives must be completed before this one can be permanently marked as completed.")]
    public List<Objective> completionPrerequisites;

    public UnityEvent onBegin;
    public UnityEvent onCompleted;
    public UnityEvent onFailed;

    ObjectiveStatus _status = ObjectiveStatus.Inactive;
    ObjectiveHandler _handler;

    public ObjectiveHandler handler => _handler ??= GetComponentInParent<ObjectiveHandler>();
    public Player targetPlayer => handler.targetPlayer;
    public ObjectiveStatus status => _status;
    public string serialisedProgress
    {
        get => GetSerializedProgress();
        set => Setup(value);
    }

    public void Update()
    {
        // If already completed or failed, return
        if (_status == ObjectiveStatus.Completed || _status == ObjectiveStatus.Failed)
        {
            enabled = false;
            return;
        }

        // If inactive, confirm that the prerequisite objectives have been completed
        if (_status == ObjectiveStatus.Inactive)
        {
            // If a prerequisite is not completed, cancel
            foreach (Objective prerequisite in prerequisites)
            {
                if (prerequisite.status != ObjectiveStatus.Completed) return;
            }

            // If so, switch state to active and invoke begin event
            _status = ObjectiveStatus.Active;
            OnBegin();
            onBegin.Invoke();
        }

        // If active, check success and failure states
        if (_status == ObjectiveStatus.Active)
        {
            // If objective is undo-able, also check that the necessary later objectives are completed to lock success
            if (DetermineSuccess() && ReadyToMarkCompleted())
            {
                // If successful, change state to success and invoke success event
                _status = ObjectiveStatus.Completed;
                OnCompleted();
                onCompleted.Invoke();
            }
            else if (DetermineFailure())
            {
                // Otherwise if failed, change state to failed and invoke failure event
                _status = ObjectiveStatus.Failed;
                OnFailed();
                onFailed.Invoke();
            }
        }
    }

    bool ReadyToMarkCompleted()
    {
        // Wait before all prerequisites are completed, until permanently locking state
        foreach (Objective prerequisite in completionPrerequisites)
        {
            if (prerequisite.status != ObjectiveStatus.Completed) return false;
        }
        return true;
    }


    /// <summary>
    /// Returns a formatted string representing the progress info.
    /// </summary>
    /// <returns></returns>
    public virtual string formattedProgress => null;
    /// <summary>
    /// The specified location of the objective, for setting markers.
    /// </summary>
    public abstract Vector3 location { get; }

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
    protected abstract bool DetermineSuccess();
    /// <summary>
    /// Has the player done something that renders the objective unable to be completed?
    /// </summary>
    protected virtual bool DetermineFailure() => false;
    protected virtual void OnBegin() { }
    protected virtual void OnCompleted() { }
    protected virtual void OnFailed() { }
    #endregion
}