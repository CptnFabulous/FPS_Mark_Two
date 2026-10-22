using CptnFabulous.MiscUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public readonly struct MiscFunctions
{
    #region Finding components

    /// <summary>
    /// Gets all components of type <typeparamref name="Child"/>, whose closest parent is <paramref name="parent"/> and not any other instance.
    /// </summary>
    /// <typeparam name="Child"></typeparam>
    /// <typeparam name="Parent"></typeparam>
    /// <param name="parent"></param>
    /// <param name="cachedData"></param>
    /// <returns></returns>
    public static Child[] GetImmediateComponentsInChildren<Child, Parent>(Parent parent, ref Child[] cachedData) where Child : Component where Parent : Component
    {
        return CptnFabulous.MiscUtility.ComponentUtility.GetImmediateComponentsInChildren(parent, ref cachedData, (i) => ComponentCache<Parent>.GetInParent(i.gameObject));
    }

    #endregion

    public static bool CurrentCameraNotMain() => Camera.current.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay;
    
    public static bool GetToggleableInput(bool currentState, bool buttonPressed, bool isToggled)
    {
        if (isToggled == false) currentState = buttonPressed;
        else if (buttonPressed) currentState = !currentState;

        return currentState;
    }

    public static void SetMaterialBool(Material material, string propertyName, bool value)
    {
        material.SetInt(propertyName, value ? 1 : 0);
    }


    public static bool DoAnyMeetCriteria<T>(IEnumerable<T> collection, System.Func<T, bool> criteria)
    {
        foreach (T t in collection)
        {
            if (criteria.Invoke(t)) return true;
        }
        return false;
    }









    public static int IndexOfInCollection<T>(IEnumerable<T> array, T data)
    {
        return IndexOfInCollection(array, data, 0, array.Count());
    }
    public static int IndexOfInCollection<T>(IEnumerable<T> array, T data, int start, int length)
    {
        for (int i = start; i < start + length; i++)
        {
            if (array.ElementAt(i).Equals(data)) return i;
        }
        return -1;
    }

    public static bool TryParseCharToInt(char c, out int value)
    {
        value = (int)char.GetNumericValue(c);
        //int.TryParse(c, out index);
        return value >= 0;
    }
    public static void Shuffle<T>(IList<T> list, int start, int length)
    {
        for (int i = start; i < length; i++)
        {
            int r = UnityEngine.Random.Range(start, length);
            if (r == i) continue;
            T swapping = list[i];
            list[i] = list[r];
            list[r] = swapping;
        }
    }
}

public class SortWithOnePredicate<T> : IComparer<T>
{
    public System.Func<T, IComparable> obtainValue;
    public bool reverse = false;

    public int Compare(T x, T y)
    {
        IComparable _a = obtainValue.Invoke(x);
        IComparable _b = obtainValue.Invoke(y);
        return reverse ? _b.CompareTo(_a) : _a.CompareTo(_b);
    }
}
