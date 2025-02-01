using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyState : StateFunction
{
    public Behaviour[] toMatchActiveState;

    private void OnEnable() => SetActive(true);
    private void OnDisable() => SetActive(false);
    void SetActive(bool enabled)
    {
        foreach (Behaviour c in toMatchActiveState) c.enabled = enabled;
    }
}
