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
        string path = binding.effectivePath;

        int index = inputStrings.IndexOf(path);
        if (index >= 0 && index < iconArray.Length && iconArray[index] != null) // If index is valid, and the display name has a matching name in the list AND there's a corresponding sprite for the index
        {
            graphic.sprite = iconArray[index];
            keyName.text = "";
        }
        else
        {
            // Default to key graphic and display name of binding
            // Cut off everything before the binding name
            int lastSlashBeforeName = path.LastIndexOf("/");
            string displayName = path.Remove(0, lastSlashBeforeName + 1);
            // Capitalise binding name

            //Debug.Log(displayName);
            keyName.text = displayName;
            graphic.sprite = keyboardKey;
        }

        gameObject.SetActive(true);
    }


    /*
    void SetMappingGraphic(KeyCode key)
    {
        int i;
        for (i = 0; i < slightlyWider.Length; i++)
        {
            if (key == slightlyWider[i])
            {
                singleKeyDimensions.x = Mathf.RoundToInt(singleKeyDimensions.x * 1.5f);
                break;
            }
        }
        for (i = 0; i < wide.Length; i++)
        {
            if (key == wide[i])
            {
                singleKeyDimensions.x = Mathf.RoundToInt(singleKeyDimensions.x * 2.5f);
                break;
            }
        }
        for (i = 0; i < tall.Length; i++)
        {
            if (key == tall[i])
            {
                singleKeyDimensions.y *= 2;
                break;
            }
        }
        if (key == KeyCode.Space)
        {
            singleKeyDimensions *= 6;
        }
        Rect r = graphic.rectTransform.rect;
        r.width = singleKeyDimensions.x;
        r.height = singleKeyDimensions.y;
        graphic.SetClipRect(r, true);
        graphic.sprite = keyboardKey;
        keyName.text = key.ToString();//System.Enum.GetName(typeof(KeyCode), key);
    }
    static readonly KeyCode[] slightlyWider = new KeyCode[]
        {
            KeyCode.LeftControl,
            KeyCode.RightControl,
            KeyCode.LeftWindows,
            KeyCode.RightWindows,
            KeyCode.LeftAlt,
            KeyCode.RightAlt,
            KeyCode.AltGr,
            KeyCode.Tab,
            KeyCode.LeftApple,
            KeyCode.RightApple,
        };
    static readonly KeyCode[] wide = new KeyCode[]
    {
            KeyCode.LeftShift,
            KeyCode.RightShift,
            KeyCode.CapsLock,
            KeyCode.Backspace,
            KeyCode.Return,
    };
    static readonly KeyCode[] tall = new KeyCode[]
    {
        KeyCode.KeypadPlus,
        KeyCode.KeypadEnter,
    };
    */
}
