using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

[CustomEditor(typeof(ActionExecutor))]
public class ActionExecutorEditor : Editor
{
    ActionExecutor objectBeingEdited;
    
    GUIStyle inactiveStyle;
    GUIStyle activeStyle;
    GUIStyle CorrectStyle(bool active) => active ? activeStyle : inactiveStyle;

    private void Awake()
    {
        inactiveStyle = new GUIStyle();
        inactiveStyle.normal.textColor = Color.white;
        activeStyle = new GUIStyle();
        activeStyle.normal.textColor = Color.green;
    }

    private void OnEnable()
    {
        objectBeingEdited = serializedObject.targetObject as ActionExecutor;
    }

    public override void OnInspectorGUI()
    {
        if (objectBeingEdited.baseAction != null)
        {
            ListAction(objectBeingEdited.baseAction, true, 0);
        }
        else
        {
            EditorGUILayout.HelpBox("No action is assigned.", MessageType.Warning);
        }
    }

    public void ListAction(Action action, bool active, int layer)
    {
        if (action as MultiAction != null)
        {
            ListAction(action as MultiAction, active, layer);
        }
        else if (action as PriorityActionController != null)
        {
            ListAction(action as PriorityActionController, active, layer);
        }
        else if (action as FSM != null)
        {
            ListAction(action as FSM, active, layer);
        }
        else
        {
            EditorGUILayout.LabelField(Spacing(layer) + action.ToString(), CorrectStyle(active));
        }
    }
    public void ListAction(MultiAction controller, bool active, int layer)
    {
        if (EditorGUILayout.Foldout(true, Spacing(layer) + controller.ToString(), CorrectStyle(active)) == false) return;

        for (int i = 0; i < controller.allActions.Count; i++)
        {
            ListAction(controller.allActions[i], active, layer); // For some reason with this function I don't need to add 1 to the layer. Doing so causes the text to be shifted too far.
        }
    }
    public void ListAction(PriorityActionController controller, bool active, int layer)
    {
        if (EditorGUILayout.Foldout(true, Spacing(layer) + controller.ToString(), CorrectStyle(active)) == false) return;

        bool activeInController;
        for (int i = 0; i < controller.allActions.Count; i++)
        {
            activeInController = active && controller.CurrentAction == controller.allActions[i].action;
            ListAction(controller.allActions[i].action, activeInController, layer + 1);
        }
        if (controller.defaultAction != null)
        {
            activeInController = active && controller.CurrentAction == controller.defaultAction;
            ListAction(controller.defaultAction, activeInController, layer + 1);
        }
    }
    public void ListAction(FSM controller, bool active, int layer)
    {
        if (EditorGUILayout.Foldout(true, Spacing(layer) + controller.ToString(), CorrectStyle(active)) == false) return;

        for (int i = 0; i < controller.allStates.Count; i++)
        {
            bool activeInController = active && controller.currentState == controller.allStates[i];
            ListAction(controller.allStates[i], activeInController, layer + 1);
        }
    }

    string Spacing(int numberOfLayers)
    {
        string s = "";

        // Adds a number of spacings equal to the number of layers minus one, so the last one can be an arrow indicator
        for (int i = 1; i < numberOfLayers; i++)
        {
            s += "    ";
        }
        // Adds the arrow indicator
        if (numberOfLayers > 0)
        {
            s += " L> ";
        }

        return s;
    }
}