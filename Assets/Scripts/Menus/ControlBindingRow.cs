using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlBindingRow : MonoBehaviour
{
    public Text title;

    public GUIButtonPrompt keyboardPrompt;
    public Button rebindKeyboard;
    int keyboardBindingIndex;
    string newKeyboardPath;

    public GUIButtonPrompt gamepadPrompt;
    public Button rebindGamepad;
    int gamepadBindingIndex;
    string newGamepadPath;


    public UnityEvent onBindingChanged;

    public RectTransform rectTransform { get; private set; }



    public PlayerInput playerToUpdateControlsOf { get; private set; }
    public InputAction assignedAction { get; private set; }


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(InputAction newAction, PlayerInput newPlayer)
    {
        assignedAction = newAction;
        playerToUpdateControlsOf = newPlayer;
        title.text = newAction.name;
    }

    public void Refresh()
    {
        // Obtain binding values for the action
        for (int b = 0; b < assignedAction.bindings.Count; b++)
        {
            InputBinding binding = assignedAction.bindings[b];
            // Get groups value to determine if the binding is for KB+M or gamepad

            if (binding.groups.Contains("Keyboard&Mouse") && binding.isComposite == false && binding.isPartOfComposite == false)
            {
                keyboardPrompt.enabled = false;
                keyboardPrompt.Refresh(binding);
                keyboardBindingIndex = b;
                continue;
            }
            if (binding.groups.Contains("Gamepad") && binding.isComposite == false && binding.isPartOfComposite == false)
            {
                gamepadPrompt.enabled = false;
                gamepadPrompt.Refresh(binding);
                gamepadBindingIndex = b;
                continue;
            }
        }
    }

    void SetPending()
    {
        // Set player binding index to match this
    }


    public void CheckToAssignNewBinding(InputAction.CallbackContext context)
    {

        string newPath = context.control.path;

        for (int i = 0; i < kbmVariants.Length; i++)
        {
            if (newPath.Contains(kbmVariants[i]))
            {
                newKeyboardPath = newPath;
                break;
            }
        }

        for (int i = 0; i < gamepadVariants.Length; i++)
        {
            if (newPath.Contains(gamepadVariants[i]))
            {
                newGamepadPath = newPath;
                break;
            }
        }
    }

    public static readonly string[] kbmVariants = new string[]
    {
        "Keyboard",
        "Mouse",
    };

    public static readonly string[] gamepadVariants = new string[]
    {
        "Gamepad",
        "XInputController",
        "DualShockGamepad",
        "SwitchProControllerHID",
        "WebGLGamepad",
        "iOSGameController",
        "AndroidGamepad",
    };


    public void Apply()
    {
        //assignedAction.ChangeBindingWithPath()
    }
}