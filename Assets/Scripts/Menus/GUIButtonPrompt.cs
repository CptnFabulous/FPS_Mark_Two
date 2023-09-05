using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GUIButtonPrompt : MonoBehaviour
{
    [SerializeField] Image graphic;
    [SerializeField] Text keyName;

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

    InputAction assignedInput;
    InputBinding assignedBinding;
    PlayerInput player;
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

    private void Update() => DetermineCurrentBinding(player);

    public void AssignAction(InputAction newInput, PlayerInput newPlayer)
    {
        assignedInput = newInput;
        player = newPlayer;

        bool active = assignedInput != null && player != null;
        gameObject.SetActive(active);
        if (active == false) return;

        DetermineCurrentBinding(player);
    }

    void DetermineCurrentBinding(PlayerInput player)
    {
        string currentPlayerInputScheme = player.currentControlScheme;
        foreach (InputBinding b in assignedInput.bindings)
        {
            // Check if an available binding matches the current control scheme
            if (assignedBinding != b && b.groups.Contains(currentPlayerInputScheme))
            {
                assignedBinding = b;
                Refresh(assignedBinding);
                return;
            }
        }
        // Disable button prompt because a binding was not found
        gameObject.SetActive(false);
    }
    public void Refresh(InputBinding binding) => Refresh(binding.effectivePath);
    public void Refresh(string path)
    {
        if (iconDictionary.TryGetValue(path, out Sprite sprite))
        {
            graphic.sprite = sprite;
            keyName.text = ""; // Don't show text because the icon explains it
        }
        else // Default to key graphic
        {
            graphic.sprite = keyboardKey;
            keyName.text = MiscFunctions.FormatNameForPresentation(path);
            // I might be able to use InputAction.GetBindingDisplayString(), but I don't fully understand how it works.
        }
    }
}