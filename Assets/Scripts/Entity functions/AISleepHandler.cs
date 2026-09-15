using CptnFabulous.MiscUtility;
using RootMotion.Dynamics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISleepHandler : MonoBehaviour
{
    public AI rootAI;
    public PuppetmasterRagdollHandler puppetmasterHandler;

    public StateFunction[] idleStates;

    private void Update()
    {
        /*
        // Check if the entity is engaging in any real activity

        // If the entity is not performing a complex action
        bool isIdle = MiscFunctions.DoAnyMeetCriteria(idleStates, (s) => rootAI.stateController.currentStateInHierarchy == s);

        // If it's not in the same area as any player
        LevelArea currentArea = LevelArea.FindCurrentArea(rootAI);
        bool outsideArea = !MiscFunctions.DoAnyMeetCriteria(Player.activePlayers, (p) => LevelArea.FindCurrentArea(p) == currentArea);

        // If the enemy is not being seen by any camera
        bool notBeingRendered = !MiscFunctions.DoAnyMeetCriteria(rootAI.renderers, (r) => r.isVisible);

        // If so, then its more processor intensive functions should be disabled.
        bool irrelevantToPlayer = isIdle && outsideArea && notBeingRendered;

        rootAI.DebugLog($"{isIdle} + {outsideArea} + {notBeingRendered} = {irrelevantToPlayer}");
        */


        // Check if the entity is engaging in any real activity
        bool irrelevantToPlayer = IsAINotRelevantToPlayer();

        // Switch puppetmaster between enabled and kinematic
        // Enable/disable IK
        // Enable/disable animations
        puppetmasterHandler.puppetmaster.mode = irrelevantToPlayer ? PuppetMaster.Mode.Disabled : PuppetMaster.Mode.Active;
        //puppetmasterHandler.animator.enabled = !irrelevantToPlayer;
    }

    bool IsAINotRelevantToPlayer()
    {
        // If the entity is not performing a complex action
        bool isIdle = MiscFunctions.DoAnyMeetCriteria(idleStates, (s) => rootAI.stateController.currentStateInHierarchy == s);
        if (!isIdle) return false;

        // If it's not in the same area as any player
        LevelArea currentArea = LevelArea.FindCurrentArea(rootAI);
        bool outsideArea = !MiscFunctions.DoAnyMeetCriteria(Player.activePlayers, (p) => LevelArea.FindCurrentArea(p) == currentArea);
        if (!outsideArea) return false;

        // If the enemy is not being seen by any camera
        //bool notBeingRendered = !MiscFunctions.DoAnyMeetCriteria(rootAI.renderers, (r) => r.isVisible);
        bool notBeingRendered = !MiscFunctions.DoAnyMeetCriteria(rootAI.renderers, (r) =>
        {
            if (!r.isVisible) return false;

            // Check if line of sight is established to any camera (that can render it)
            return MiscFunctions.DoAnyMeetCriteria(Camera.allCameras, (c) =>
            {
                // Check if this camera can render this renderer
                if (PhysicsUtility.IsLayerInLayerMask(c.cullingMask, r.gameObject.layer) == false) return false;
                // Check if line of sight is established
                if (!AIAction.LineOfSight(c.transform.position, r.transform.position, null, rootAI, c.cullingMask)) return false;
                return true;
            });
        });
        if (!notBeingRendered) return false;


        //rootAI.DebugLog($"{isIdle} + {outsideArea} + {notBeingRendered} = {irrelevantToPlayer}");

        return true;
    }
}
