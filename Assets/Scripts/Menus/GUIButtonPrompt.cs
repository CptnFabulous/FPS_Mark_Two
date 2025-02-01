using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;



public abstract class GUIButtonPromptBase : MonoBehaviour
{
    [SerializeField] bool _inputDisabled;

    protected InputAction assignedInput;
    protected PlayerInput player;

    public bool inputDisabled
    {
        get => _inputDisabled;
        set
        {
            _inputDisabled = value;
            //DetermineCurrentBinding(player);
        }
    }


    //private void OnEnable() => player.onControlsChanged += DetermineCurrentBinding;
    //private void OnDisable() => player.onControlsChanged -= DetermineCurrentBinding;
    private void LateUpdate() => DetermineCurrentBinding();

    public void AssignAction(InputAction newInput, PlayerInput newPlayer)
    {
        assignedInput = newInput;
        player = newPlayer;

        bool active = assignedInput != null && player != null;
        gameObject.SetActive(active);
        if (active == false) return;

        DetermineCurrentBinding();
    }
    protected abstract void DetermineCurrentBinding();
}

public class GUIButtonPrompt : GUIButtonPromptBase
{
    //[SerializeField] bool _inputDisabled;

    [Header("GUI elements")]
    [SerializeField] Image graphic;
    [SerializeField] TMPro.TextMeshProUGUI keyNameText;

    [Header("Sprites - Keyboard + mouse")]
    [SerializeField] Sprite keyboardKey;
    [SerializeField] Sprite mouseLeft;
    [SerializeField] Sprite mouseRight;
    [SerializeField] Sprite mouseMiddle;
    [SerializeField] Sprite mouseMove;
    [SerializeField] Sprite mouseScrollWheel;

    [Header("Sprites - Gamepad")]
    [SerializeField] Sprite leftStickMove;
    [SerializeField] Sprite rightStickMove;
    [SerializeField] Sprite leftStickClick;
    [SerializeField] Sprite rightStickClick;
    [SerializeField] Sprite faceButtonNorth;
    [SerializeField] Sprite faceButtonSouth;
    [SerializeField] Sprite faceButtonEast;
    [SerializeField] Sprite faceButtonWest;
    [SerializeField] Sprite dpadUp;
    [SerializeField] Sprite dpadDown;
    [SerializeField] Sprite dpadLeft;
    [SerializeField] Sprite dpadRight;
    [SerializeField] Sprite leftBumper;
    [SerializeField] Sprite rightBumper;
    [SerializeField] Sprite leftTrigger;
    [SerializeField] Sprite rightTrigger;
    [SerializeField] Sprite start;
    [SerializeField] Sprite select;

    [Header("Sprites - Other")]
    [SerializeField] Sprite undefined;
    [SerializeField] Sprite disabled;

    Dictionary<string, Sprite> _dictionary;

    public Dictionary<string, Sprite> iconDictionary => _dictionary ??= new Dictionary<string, Sprite>()
    {
        {"<Mouse>/leftButton", mouseLeft },
        {"<Mouse>/rightButton", mouseRight },
        {"<Mouse>/middleButton", mouseMiddle },
        {"<Pointer>/delta", mouseMove },
        {"<Mouse>/scroll/y", mouseScrollWheel },

        {"<Gamepad>/leftStick", leftStickMove },
        {"<Gamepad>/rightStick", rightStickMove },
        {"<Gamepad>/leftStickPress", leftStickClick },
        {"<Gamepad>/rightStickPress", rightStickClick },

        {"<Gamepad>/buttonNorth", faceButtonNorth },
        {"<Gamepad>/buttonSouth", faceButtonSouth },
        {"<Gamepad>/buttonEast", faceButtonEast },
        {"<Gamepad>/buttonWest", faceButtonWest },

        {"<Gamepad>/dpad/up", dpadUp },
        {"<Gamepad>/dpad/down", dpadDown },
        {"<Gamepad>/dpad/left", dpadLeft },
        {"<Gamepad>/dpad/right", dpadRight },

        {"<Gamepad>/leftShoulder", leftBumper },
        {"<Gamepad>/rightShoulder", rightBumper },
        {"<Gamepad>/leftTrigger", leftTrigger },
        {"<Gamepad>/rightTrigger", rightTrigger },

        {"<Gamepad>/start", start },
        {"<Gamepad>/select", select },
    };

    protected override void DetermineCurrentBinding()
    {
        graphic.enabled = player != null;
        if (player == null)
        {
            graphic.sprite = null;
            keyNameText.text = "";
            return;
        }

        if (inputDisabled)
        {
            graphic.sprite = disabled;
            keyNameText.text = "";
            return;
        }

        // Check if an available binding matches the current control scheme
        foreach (InputBinding b in assignedInput.bindings)
        {
            bool match = b.groups.Contains(player.currentControlScheme);
            if (match == false) continue;
            Refresh(b);
            return;
        }

        // Show that a binding was not found
        Debug.Log("Undefined");
        Refresh(null);
    }
    public void Refresh(InputBinding binding) => Refresh(binding.effectivePath);
    public void Refresh(string path)
    {
        // If a binding wasn't found, display the appropriate text
        if (path == null)
        {
            Debug.Log("Undefined path");
            Debug.Log(assignedInput);
            graphic.sprite = undefined;
            keyNameText.text = "";
            return;
        }
        
        // Check for a valid icon
        if (iconDictionary.TryGetValue(path, out Sprite sprite))
        {
            graphic.sprite = sprite;
            keyNameText.text = ""; // Don't show text because the icon explains it
        }
        else // Otherwise default to key graphic
        {
            graphic.sprite = keyboardKey;
            keyNameText.text = MiscFunctions.FormatNameForPresentation(path);
            // I might be able to use InputAction.GetBindingDisplayString(), but I don't fully understand how it works.
        }
    }
}