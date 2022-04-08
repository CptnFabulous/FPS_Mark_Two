using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PriorityActionController : Action
{
    public struct ListedAction
    {
        public Action action;
        public Func<bool> condition;

        public ListedAction(Action newAction, Func<bool> newCondition)
        {
            action = newAction;
            condition = newCondition;
        }
    }
    public PriorityActionController(string newName)
    {
        name = newName;
        allActions = new List<ListedAction>();
        index = -1;
    }

    #region List of actions
    public List<ListedAction> allActions { get; private set; }
    public Action defaultAction;
    int index;
    public void AddAction(Action action, Func<bool> condition) => allActions.Add(new ListedAction(action, condition));
    public void AddAction(IPriorityAction pa) => AddAction(pa.actionToRun, pa.prerequisites);
    public void InsertAction(Action action, Func<bool> condition, int index) => allActions.Insert(index, new ListedAction(action, condition));
    public void InsertAction(IPriorityAction pa, int index) => InsertAction(pa.actionToRun, pa.prerequisites, index);

    #endregion

    #region Properties
    /// <summary>
    /// Can any actions in this controller be performed?
    /// </summary>
    public Func<bool> CanPerform() => () => CurrentAction == null; // If CurrentAction is null, that means none of the actions in the list are valid and there's no assigned default
    public int CurrentIndex
    {
        get => index;
        private set
        {
            if (index != value)
            {
                CurrentAction?.Exit();
                index = value;
                CurrentAction?.Enter();
            }
        }
    }
    public Action CurrentAction
    {
        get
        {
            // Check if none of the actions in the list can be performed (or if there are none)
            if (CurrentIndex < 0 || CurrentIndex >= allActions.Count)
            {
                return defaultAction;
            }
            return allActions[CurrentIndex].action;
        }
    }
    #endregion

    public override void Setup()
    {
        for (int i = 0; i < allActions.Count; i++)
        {
            allActions[i].action.host = host;
            allActions[i].action.Setup();
        }
        
        if (defaultAction != null)
        {
            defaultAction.host = host;
            defaultAction.Setup();
        }
    }
    public override void Loop()
    {
        // Scrolls through each action in priority order
        bool actionFound = false;
        for (int i = 0; i < allActions.Count; i++)
        {
            // The first state with valid prerequisites, whose prerequisites are met, has its index assigned as the one to run
            bool canPerformState = allActions[i].condition != null && allActions[i].condition.Invoke();
            if (canPerformState)
            {
                CurrentIndex = i;
                actionFound = true;
                break;
            }
        }

        if (actionFound == false)
        {
            CurrentIndex = -1;
        }

        CurrentAction?.Loop();
    }

    #region Running regular functions on current action
    public override void Enter() => CurrentAction?.Enter();
    public override void Exit() => CurrentAction?.Exit();
    public override void FixedLoop() => CurrentAction?.FixedLoop();
    public override void LateLoop() => CurrentAction?.LateLoop();
    #endregion
}


public interface IPriorityAction
{
    public Action actionToRun { get; }
    public Func<bool> prerequisites { get; }
}