using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GUIButtonPrompt : MonoBehaviour
{
    public bool inputDisabled;

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
        {"<Mouse>/scroll", mouseScrollWheel },
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

    public void Refresh(InputBinding binding) => Refresh(binding.effectivePath);
    public void Refresh(string path)
    {
        if (inputDisabled)
        {
            graphic.sprite = disabled;
            keyNameText.text = "";
            return;
        }

        // If a binding wasn't found, display the appropriate text
        if (path == null)
        {
            Debug.Log("Undefined path");
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