using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GUIButtonPrompt : MonoBehaviour
{
    public Image graphic;
    public Text keyName;
    Vector2Int singleKeyDimensions;

    [Header("Sprites - Keyboard + mouse")]
    public Sprite keyboardKey;
    public Sprite mouseLeft;
    public Sprite mouseRight;
    public Sprite mouseMiddle;
    public Sprite mouseMove;
    public Sprite mouseScrollWheel;
    [Header("Sprites - Gamepad")]
    public Sprite leftStickMove;
    public Sprite rightStickMove;
    public Sprite leftStickClick;
    public Sprite rightStickClick;
    public Sprite faceButtonNorth;
    public Sprite faceButtonSouth;
    public Sprite faceButtonEast;
    public Sprite faceButtonWest;
    public Sprite dpadUp;
    public Sprite dpadDown;
    public Sprite dpadLeft;
    public Sprite dpadRight;
    public Sprite leftBumper;
    public Sprite rightBumper;
    public Sprite leftTrigger;
    public Sprite rightTrigger;
    public Sprite start;
    public Sprite select;

    

    InputAction assignedInput;
    InputBinding assignedBinding;
    PlayerInput player;

    public Sprite[] iconArray
    {
        get
        {
            return new Sprite[]
            {
                mouseLeft,
                mouseRight,
                mouseMiddle,
                mouseMove,
                mouseScrollWheel,
                leftStickMove,
                rightStickMove,
                leftStickClick,
                rightStickClick,
                faceButtonNorth,
                faceButtonSouth,
                faceButtonEast,
                faceButtonWest,
                dpadUp,
                dpadDown,
                dpadLeft,
                dpadRight,
                leftBumper,
                rightBumper,
                leftTrigger,
                rightTrigger,
                start,
                select,
            };
        }
    }
    public static readonly List<string> inputStrings = new List<string>(new string[]
    {
        "<Mouse>/leftButton",
        "<Mouse>/rightButton",
        "<Mouse>/middleButton",
        "<Pointer>/delta",
        "<Mouse>/scroll/y",
        "<Gamepad>/leftStick",
        "<Gamepad>/rightStick",
        "<Gamepad>/leftStickPress",
        "<Gamepad>/rightStickPress",
        "<Gamepad>/buttonNorth",
        "<Gamepad>/buttonSouth",
        "<Gamepad>/buttonEast",
        "<Gamepad>/buttonWest",
        "<Gamepad>/dpad/up",
        "<Gamepad>/dpad/down",
        "<Gamepad>/dpad/left",
        "<Gamepad>/dpad/right",
        "<Gamepad>/leftShoulder",
        "<Gamepad>/rightShoulder",
        "<Gamepad>/leftTrigger",
        "<Gamepad>/rightTrigger",
        "<Gamepad>/start",
        "<Gamepad>/select",
    });

    public void AssignAction(InputAction newInput, PlayerInput newPlayer)
    {
        assignedInput = newInput;
        player = newPlayer;
        gameObject.SetActive(true);
        Update();
    }

    private void Update()
    {
        if (assignedInput == null || player == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Set up some kind of code so this code updates automatically when the input scheme changes

        // Figure out the player's current input scheme
        string currentPlayerInputScheme = player.currentControlScheme;
        for (int i = 0; i < assignedInput.bindings.Count; i++)
        {
            // Check if an available binding matches the current control scheme
            InputBinding b = assignedInput.bindings[i];
            //Debug.Log(b.groups + ", " + b.path + ", " + currentPlayerInputScheme);
            if (assignedBinding != b && b.groups.Contains(currentPlayerInputScheme))
            {
                assignedBinding = b;
                Refresh(assignedBinding);
                return;
            }
        }
        // Reaching this point means a binding matching the current scheme was not found
        // Disable button prompt
        gameObject.SetActive(false);
    }

    public void Refresh(InputBinding binding)
    {
        Refresh(binding.effectivePath);
    }
    public void Refresh(string path)
    {
        int index = inputStrings.IndexOf(path);
        if (index >= 0 && index < iconArray.Length && iconArray[index] != null) // If index is valid, and the display name has a matching name in the list AND there's a corresponding sprite for the index
        {
            graphic.sprite = iconArray[index];
            keyName.text = "";
        }
        else
        {
            // Default to key graphic
            graphic.sprite = keyboardKey;
            keyName.text = MiscFunctions.FormatNameForPresentation(path);
        }
    }
}
