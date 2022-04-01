using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM : Action
{
    public FSM(string newName)
    {
        name = newName;
    }

    public List<Action> allStates = new List<Action>();
    public List<Transition> allTransitions = new List<Transition>();
    int currentStateIndex = 0;
    public Action currentState
    {
        get
        {
            if (allStates.Count <= 0 || currentStateIndex < 0 || currentStateIndex > allStates.Count - 1)
            {
                return null;
            }
            return allStates[currentStateIndex];
        }
        set
        {
            currentStateIndex = allStates.IndexOf(value);
        }
    }

    public override void Setup()
    {
        for (int i = 0; i < allStates.Count; i++)
        {
            allStates[i].host = host;
            allStates[i].Setup();
        }
    }
    public override void Loop()
    {
        for (int i = 0; i < allTransitions.Count; i++)
        {
            // If transition can happen from the current state (or from anywhere)
            bool transitionCanOccur = allTransitions[i].from == currentState || allTransitions[i].from == null;
            if (transitionCanOccur && allTransitions[i].conditions.Invoke() == true)
            {
                currentState?.Exit();
                currentState = allTransitions[i].to;
                currentState.Enter();
            }
        }

        currentState?.Loop();
    }

    public override void Enter() => currentState?.Enter();
    public override void LateLoop() => currentState?.LateLoop();
    public override void FixedLoop() => currentState?.FixedLoop();
    public override void Exit() => currentState?.Exit();

    public void AddState(Action newState, bool setAsDefault = false)
    {
        allStates.Add(newState);
        if (setAsDefault)
        {
            currentState = newState;
        }
    }
    public void AddTransition(Action from, Action to, Func<bool> conditions)
    {
        allTransitions.Add(new Transition(from, to, conditions));
    }

    public struct Transition
    {
        public Action from;
        public Action to;
        public Func<bool> conditions;
        public Transition(Action _from, Action _to, Func<bool> _conditions)
        {
            from = _from;
            to = _to;
            conditions = _conditions;
        }
    }
}