using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NumberKeySelector : MonoBehaviour
{
    #region MonoBehaviour stuff
    public UnityEvent<int> onSelectionMade;
    [Tooltip("If true, values are shifted down to represent number positions on a num row, otherwise the int accurately represents the number pressed.")]
    public bool startWithOne;

    private void Update()
    {
        if (NumKeyPressed(out int keyIndex, startWithOne))
        {
            onSelectionMade.Invoke(keyIndex);
        }
    }
    #endregion

    #region Static code
    public static readonly KeyCode[] alphaKeys = new KeyCode[10]
    {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
    };
    public static readonly KeyCode[] numpadKeys = new KeyCode[10]
    {
        KeyCode.Keypad0,
        KeyCode.Keypad1,
        KeyCode.Keypad2,
        KeyCode.Keypad3,
        KeyCode.Keypad4,
        KeyCode.Keypad5,
        KeyCode.Keypad6,
        KeyCode.Keypad7,
        KeyCode.Keypad8,
        KeyCode.Keypad9,
    };
    public static bool NumKeyPressed(out int keyIndex, bool startWithOne)
    {
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(alphaKeys[i]) || Input.GetKeyDown(numpadKeys[i]))
            {
                keyIndex = i;
                if (startWithOne)
                {
                    keyIndex -= 1;
                    keyIndex = MiscFunctions.InverseClamp(keyIndex, 0, 9);
                }
                return true;
            }
        }

        keyIndex = -1;
        return false;
    }
    #endregion
}
