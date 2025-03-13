using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidDyingObjective : Objective
{
    public CheckpointManager checkpointManager;
    public string deathMessageFormatting = "Deaths: {0}";

    int deaths => checkpointManager.deathCount;

    public override Vector3? location => null;


    public override string formattedProgress
    {
        get
        {
            if (deaths == 0) return base.formattedProgress;

            return string.Format(deathMessageFormatting, deaths);
        }
    }

    protected override bool DetermineFailure() => deaths > 0;
    protected override bool DetermineSuccess() => deaths <= 0;

    protected override string GetSerializedProgress()
    {
        return "";
        //throw new System.NotImplementedException();
    }

    protected override void Setup(string progress)
    {
        //throw new System.NotImplementedException();
    }
}
