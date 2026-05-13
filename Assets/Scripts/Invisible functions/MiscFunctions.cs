using CptnFabulous.MiscUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct MiscFunctions
{
    #region IEnumerators
    public delegate void LerpLoop(ref float t);
    public static IEnumerator WaitOnLerp(float secondsToWait, LerpLoop frameAction)
    {
        float t = 0;
        do
        {
            t += Time.deltaTime / secondsToWait;
            t = Mathf.Clamp01(t);
            frameAction.Invoke(ref t);
            if (t > 1) yield break;
            yield return t;
        }
        while (t < 1);
    }
    #endregion

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

    public static bool GetToggleableInput(bool currentState, bool buttonPressed, bool isToggled)
    {
        if (isToggled == false) currentState = buttonPressed;
        else if (buttonPressed) currentState = !currentState;

        return currentState;
    }

}
