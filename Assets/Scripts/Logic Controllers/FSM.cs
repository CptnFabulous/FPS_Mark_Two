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

    public List<Action> states = new List<Action>();
    public List<Transition> transitions = new List<Transition>();
    int currentStateIndex = 0;
    public Action currentState
    {
        get
        {
            if (states.Count <= 0 || currentStateIndex < 0 || currentStateIndex > states.Count - 1)
            {
                return null;
            }
            return states[currentStateIndex];
        }
        set
        {
            currentStateIndex = states.IndexOf(value);
        }
    }

    public override void Setup()
    {
        for (int i = 0; i < states.Count; i++)
        {
            states[i].host = host;
            states[i].Setup();
        }
    }
    public override void Loop()
    {
        for (int i = 0; i < transitions.Count; i++)
        {
            // If transition can happen from the current state (or from anywhere)
            bool transitionCanOccur = transitions[i].from == currentState || transitions[i].from == null;
            if (transitionCanOccur && transitions[i].conditions.Invoke() == true)
            {
                currentState?.Exit();
                currentState = transitions[i].to;
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
        states.Add(newState);
        if (setAsDefault)
        {
            currentState = newState;
        }
    }
    public void AddTransition(Action from, Action to, Func<bool> conditions)
    {
        transitions.Add(new Transition(from, to, conditions));
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