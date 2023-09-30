using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct MiscFunctions
{
    #region Position and rotation
    public static Quaternion FromToRotation(Quaternion a, Quaternion b) => a * Quaternion.Inverse(b);
    public static Quaternion WorldToLocalRotation(Quaternion worldRotation, Transform target) => Quaternion.Inverse(target.rotation) * worldRotation;
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
    public static Vector3 ClampDirection(Vector3 direction, Vector3 reference, float maxAngle)
    {
        float angle = Vector3.Angle(direction, reference);
        if (angle > maxAngle)
        {
            direction = Vector3.RotateTowards(direction, reference, angle - maxAngle, 0);
        }
        return direction;
    }
    #endregion

    #region IEnumerables
    public static bool ArrayContains<T>(IEnumerable<T> array, T data)
    {
        foreach(T t in array)
        {
            if (t.Equals(data)) return true;
        }
        return false;
    }
    public static void ShuffleList<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(0, list.Count - 1);
            if (r == i) continue;
            T value = list[i];
            list[i] = list[r];
            list[r] = value;
        }
    }
    #endregion

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
    public static LayerMask GetPhysicsLayerMask(int currentLayer)
    {
        int finalMask = 0;
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(currentLayer, i)) finalMask = finalMask | (1 << i);
        }
        return finalMask;
    }

    #region Math
    public static float Min(params float[] values)
    {
        float final = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            final = Mathf.Min(final, values[i]);
        }
        return final;
    }
    public static float Max(params float[] values)
    {
        float final = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            final = Mathf.Max(final, values[i]);
        }
        return final;
    }
    /// <summary>
    /// If the value exceeds the specified range, loop back around to the start.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float Loop(float value, float min, float max)
    {
        if (value > max)
        {
            // Includes difference
            return min + (value - max);
        }
        else if (value < min)
        {
            // Includes difference
            return max - (min - value);
        }
        else
        {
            return value;
        }
    }
    public static int Loop(int value, int min, int max) => (int)Loop((float)value, (float)min, (float)max);
    public static int LoopIndex(int value, int length)
    {
        if (value < 0)
        {
            value = length - value;
        }
        else if (value >= length)
        {
            value = value - length;
        }
        return value;
    }
    public static bool WithinRange(float value, float min, float max) => value >= min && value <= max;
    public static bool WithinArray(int index, int arrayLength) => WithinRange(index, 0, arrayLength - 1);
    public static float InverseSquareValue(float startingValue, float distance) => startingValue / (distance * distance);
    public static float RoundToDecimalPlaces(float value, int decimalPlaces)
    {
        decimalPlaces = Mathf.Max(decimalPlaces, 0); // Ensure it's not less than zero
        float multiplier = Mathf.Pow(10, decimalPlaces);
        value *= multiplier;
        value = Mathf.RoundToInt(value);
        value /= multiplier;
        return value;
    }
    public static float LengthOfDiagonal(float width, float height) => Mathf.Sqrt((width * width) + (height * height));
    #endregion

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
    public static Bounds CombinedBounds(IList<Collider> colliders)
    {
        Bounds b = colliders[0].bounds;
        for (int i = 1; i < colliders.Count; i++)
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
    public static bool GetToggleableInput(bool currentState, bool buttonPressed, bool isToggled)
    {
        if (isToggled == false) currentState = buttonPressed;
        else if (buttonPressed) currentState = !currentState;

        return currentState;
    }
}
