using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct MiscFunctions
{
    public static Quaternion FromToRotation(Quaternion a, Quaternion b)
    {
        return a * Quaternion.Inverse(b);
    }

    public static Vector3 ScreenToAnchoredPosition(Vector3 screenSpace, RectTransform rt, RectTransform parent)
    {
        Vector3 canvasSpace = screenSpace;
        // Multiplies screen space values to convert them to the canvas' dimensions.
        canvasSpace.x = canvasSpace.x / Screen.width;
        canvasSpace.y = canvasSpace.y / Screen.height;
        canvasSpace *= parent.rect.size;
        // Calculates the rectTransform's anchor reference point
        Vector2 anchorDimensions = rt.anchorMin + ((rt.anchorMax - rt.anchorMin) / 2);
        // Multiplies by canvas rect dimensions to get an offset
        Vector3 anchorOffset = anchorDimensions * parent.rect.size;
        return canvasSpace - anchorOffset; // Adds offset to canvas space to produce an anchored position
    }




    



    public static float InverseClamp(float value, float min, float max)
    {
        if (value > max)
        {
            return min;
        }
        else if (value < min)
        {
            return max;
        }
        else
        {
            return value;
        }
    }
    public static int InverseClamp(int value, int min, int max)
    {
        if (value > max)
        {
            return min;
        }
        else if (value < min)
        {
            return max;
        }
        else
        {
            return value;
        }
    }



    /// <summary>
    /// Checks if a number key is pressed and produces an array index. If startWithOne is true, values are shifted down to represent number positions on a num row, otherwise the int accurately represents the number pressed.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="startWithOne"></param>
    /// <returns></returns>
    public static bool NumKeyPressed(out int key, bool startWithOne)
    {
        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (Input.GetKeyDown(numberKeys[i]))
            {
                key = i;
                if (startWithOne)
                {
                    key -= 1;
                    key = InverseClamp(key, 0, 9);
                }
                return true;
            }
        }

        key = -1;
        return false;
    }
    public static readonly KeyCode[] numberKeys = new KeyCode[10]
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

}
