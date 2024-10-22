using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractWithObject : Objective
{
    public Interactable toInteractWith;

    bool completed;

    protected override bool DetermineSuccess() => completed;

    protected override string GetSerializedProgress() => completed.ToString();

    protected override void Setup(string progress)
    {
        toInteractWith.onInteract.AddListener((_) => completed = true);
    }

    public override Vector3 location => toInteractWith.collider.bounds.center;
}
