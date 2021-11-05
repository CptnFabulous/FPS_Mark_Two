using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct MiscFunctions
{



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
