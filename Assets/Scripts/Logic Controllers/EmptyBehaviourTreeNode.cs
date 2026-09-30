using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyBehaviourTreeNode : BehaviourTreeNode
{
    public override IEnumerator OnCancelTask()
    {
        yield break;
    }

    public override bool Result()
    {
        return true;
    }

    protected override IEnumerator PerformTask()
    {
        yield return new WaitForSeconds(1);
    }
}
