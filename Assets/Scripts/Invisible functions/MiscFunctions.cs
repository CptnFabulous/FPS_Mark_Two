using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct MiscFunctions
{
    public static Quaternion FromToRotation(Quaternion a, Quaternion b)
    {
        return a * Quaternion.Inverse(b);
    }
    public static Quaternion WorldToLocalRotation(Quaternion worldRotation, Transform target)
    {
        return Quaternion.Inverse(target.rotation) * worldRotation;
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

    public static float[] Vector3Array(Vector3 vector3)
    {
        return new float[3]
        {
            vector3.x,
            vector3.y,
            vector3.z,
        };
    }


    
    public static List<RaycastHit> RaycastAllWithExceptions(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, Collider[] exceptions)
    {
        List<RaycastHit> list = new List<RaycastHit>(Physics.RaycastAll(origin, direction, distance, layerMask));
        for (int i = 0; i < exceptions.Length; i++)
        {
            // Remove all returned values where the collider is part of the exceptions list
            list.RemoveAll((rh) => rh.collider == exceptions[i]);
        }
        return list;
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
        // Check alpha keys
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            if (Input.GetKeyDown(alphaKeys[i]))
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

        // Check numpad keys
        for (int i = 0; i < numpadKeys.Length; i++)
        {
            if (Input.GetKeyDown(numpadKeys[i]))
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

    #region Formatting text
    public static bool CharMatches(char c, List<char> array)
    {
        int index = array.IndexOf(c);
        //Debug.Log(c + ", " + array + ", " + WithinArray(index, array.Count));
        return WithinArray(index, array.Count);
    }
    public static bool IsUppercase(char c)
    {
        return CharMatches(c, uppercaseLetters);
    }
    public static bool IsLowercase(char c)
    {
        return CharMatches(c, lowercaseLetters);
    }
    public static char Capitalise(char c)
    {
        int lowercaseIndex = lowercaseLetters.IndexOf(c);
        if (WithinArray(lowercaseIndex, lowercaseLetters.Count))
        {
            c = uppercaseLetters[lowercaseIndex];
        }
        return c;
    }
    
    public static readonly List<char> uppercaseLetters = new List<char>(new char[26]
    {
        'A',
        'B',
        'C',
        'D',
        'E',
        'F',
        'G',
        'H',
        'I',
        'J',
        'K',
        'L',
        'M',
        'N',
        'O',
        'P',
        'Q',
        'R',
        'S',
        'T',
        'U',
        'V',
        'W',
        'X',
        'Y',
        'Z',
    });
    public static readonly List<char> lowercaseLetters = new List<char>(new char[26]
    {
        'a',
        'b',
        'c',
        'd',
        'e',
        'f',
        'g',
        'h',
        'i',
        'j',
        'k',
        'l',
        'm',
        'n',
        'o',
        'p',
        'q',
        'r',
        's',
        't',
        'u',
        'v',
        'w',
        'x',
        'y',
        'z',
    });

    /// <summary>
    /// Formats a name string to start with a capital letter and have spaces.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string FormatNameForPresentation(string text)
    {
        // Display name of binding
        // Cut off everything before the binding name
        int lastSlashBeforeName = text.LastIndexOf("/");
        string displayName = text.Remove(0, lastSlashBeforeName + 1);
        // Capitalise and format binding name


        displayName = Capitalise(displayName[0]) + displayName.Remove(0, 1);

        int displayNameIndex = 1;
        while (displayNameIndex < displayName.Length)
        {
            if (IsUppercase(displayName[displayNameIndex]))
            {
                displayName = displayName.Insert(displayNameIndex, " ");
                displayNameIndex++; // Increase the index a second time to account for the string length being increased by one
            }
            displayNameIndex++;

        }
        //Debug.Log(displayName);

        return displayName;
    }
    
    /// <summary>
    /// Formats a name string to start with a capital letter and have spaces.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string FormatNameForPresentation(string text, List<char> addSpaces, List<char> replaceWithSpaces)
    {
        int index = 0;
        while (index < text.Length)
        {
            char current = text[index];
            if ((CharMatches(current, addSpaces) || IsUppercase(current)) && index > 0)
            {
                Debug.Log(text + ": " + current + " needs a space added");
                text = text.Insert(index, " ");
                index += 2; // Increase the index a second time to account for the string length being increased by one
                break;
            }
            else if (CharMatches(current, replaceWithSpaces))
            {
                text = text.Remove(index, 1);
                if (index > 0)
                {
                    text = text.Insert(index, " ");
                    index++;
                }
                break;
            }
            else if (index == 0)
            {
                text = Capitalise(text[0]) + text.Remove(0, 1);
                index++;
                break;
            }
            else
            {
                index++;
            }
        }
        //Debug.Log(text);

        return text;
    }
    #endregion

    public static bool WithinArray(int index, int arrayLength)
    {
        return index >= 0 && index < arrayLength;
    }

    #region Combined bounds
    public static Bounds CombinedBounds(Vector3[] positions)
    {
        Bounds b = new Bounds(positions[0], Vector3.zero);
        for (int i = 1; i < positions.Length; i++)
        {
            b.Encapsulate(positions[i]);
        }
        return b;
    }
    public static Bounds CombinedBounds(Bounds[] bounds)
    {
        Bounds b = bounds[0];
        for (int i = 1; i < bounds.Length; i++)
        {
            b.Encapsulate(bounds[i]);
        }
        return b;
    }
    public static Bounds CombinedBounds(Collider[] colliders)
    {
        Bounds b = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            b.Encapsulate(colliders[i].bounds);
        }
        return b;
    }
    public static Bounds CombinedBounds(Hitbox[] hitboxes)
    {
        Bounds b = hitboxes[0].collider.bounds;
        for (int i = 1; i < hitboxes.Length; i++)
        {
            b.Encapsulate(hitboxes[i].collider.bounds);
        }
        return b;
    }
    #endregion
}
