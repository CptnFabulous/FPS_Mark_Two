using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A MonoBehaviour class designed to have an action or switching group of actions added to it.
/// </summary>
public class ActionExecutor : MonoBehaviour
{
    public Action baseAction { get; private set; }
    public void SetBaseAction(Action newBaseAction)
    {
        baseAction = newBaseAction;
        baseAction.host = this;
    }

    void Start() => baseAction?.Setup();
    void OnEnable() => baseAction?.Enter();
    void OnDisable() => baseAction?.Exit();
    void Update() => baseAction?.Loop();
    void FixedUpdate() => baseAction?.FixedLoop();
    void LateUpdate() => baseAction?.LateLoop();
}

/// <summary>
/// Action with enter, exit and loop states, designed to be switched to and from in a system such as a state machine.
/// </summary>
public abstract class Action
{
    public string name = "New Action";
    public ActionExecutor host { get; set; }

    /// <summary>
    /// Equivalent of Start().
    /// </summary>
    public virtual void Setup() { }
    /// <summary>
    /// Activates when the state is first entered.
    /// </summary>
    public virtual void Enter() { }
    /// <summary>
    /// Runs just before switching away from this state.
    /// </summary>
    public virtual void Exit() { }
    /// <summary>
    /// Runs continuously every frame.
    /// </summary>
    public virtual void Loop() { }
    /// <summary>
    /// Runs at the same time as FixedUpdate().
    /// </summary>
    public virtual void FixedLoop() { }
    /// <summary>
    /// Runs at the same time as LateUpdate().
    /// </summary>
    public virtual void LateLoop() { }

    public override string ToString()
    {
        return name + " (" + base.ToString() + ")";
    }
}

/// <summary>
/// Runs multiple actions simultaneously.
/// </summary>
public class MultiAction : Action
{
    public MultiAction(string newName)
    {
        name = newName;
        allActions = new List<Action>();
    }
    
    public List<Action> allActions;

    public override void Setup()
    {
        // Scrolls through each action
        for (int i = 0; i < allActions.Count; i++)
        {
            allActions[i].host = host;
            allActions[i].Setup();
        }
    }
    public override void Enter()
    {
        // Scrolls through each action
        for (int i = 0; i < allActions.Count; i++)
        {
            allActions[i].Enter();
        }
    }
    public override void Exit()
    {
        // Scrolls through each action
        for (int i = 0; i < allActions.Count; i++)
        {
            allActions[i].Exit();
        }
    }
    public override void Loop()
    {
        // Scrolls through each action
        for (int i = 0; i < allActions.Count; i++)
        {
            allActions[i].Loop();
        }
    }
    public override void FixedLoop()
    {
        // Scrolls through each action
        for (int i = 0; i < allActions.Count; i++)
        {
            allActions[i].FixedLoop();
        }
    }
    public override void LateLoop()
    {
        // Scrolls through each action
        for (int i = 0; i < allActions.Count; i++)
        {
            allActions[i].LateLoop();
        }
    }
}
