using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public struct HeardSound
{
    public DiegeticSound sound { get; private set; }
    public float decibels { get; private set; }
    public Entity source { get; private set; }
    public Vector3 originPoint { get; private set; }
    public float timeHeard { get; private set; }

    public HeardSound(DiegeticSound sound, float decibels, Entity source, Vector3 originPoint, float timeHeard)
    {
        this.sound = sound;
        this.decibels = decibels;
        this.source = source;
        this.originPoint = originPoint;
        this.timeHeard = timeHeard;
    }
}
public class DiegeticAudioListener : MonoBehaviour
{
    static List<DiegeticAudioListener> active = new List<DiegeticAudioListener>();

    static NavMeshPath reverbPath; // A singleton NavMeshPath re-used to save memory when calculating reverb
    static int _agentID = -1;

    static int agentID
    {
        get
        {
            if (_agentID == -1)
            {
                // TO DO: only using humanoid agent because sound propagation agent is messed up.
                // Later on I'll need to make a proper 3D pathfinding system, not just for flying enemies but for sound propagation too.
                _agentID = AIAction.GetNavMeshAgentIDByName("Humanoid");
            }
            return _agentID;
        }
    }

    [SerializeField] float sensitivityMultiplier = 1;
    [SerializeField] float minVolumeToHear = 1;
    [SerializeField] float maxDistance = 10;
    [SerializeField] LayerMask soundLayerMask = ~0;
    [SerializeField] int soundNavMeshMask = NavMesh.AllAreas;
    public UnityEvent<HeardSound> onSoundHeard;

    Entity _root;

    public Entity rootEntity => _root ??= GetComponentInParent<Entity>();

    private void OnEnable() => active.Add(this);
    private void OnDisable() => active.Remove(this);

    public static void DiegeticCheck(DiegeticSound sound, float multiplier, Entity source)
    {
        // For the sake of simplifying the AI work, the sound 'plays' at the exact position as the entity that caused it.
        foreach (DiegeticAudioListener listener in active)
        {
            // Check if the listener can hear the sound
            if (listener.CheckIfListenerCanHearSound(sound, source, multiplier, out float heardDecibels) == false) continue;

            // TO DO POSSIBLY: check if the sound is not being drowned out by other sounds the entity is currently hearing
            // Add each heard sound to a list, and remove all entries whose times are long enough ago for the sound to expire.
            // Then only count a sound if the incoming volume is greater than or equal to the loudest sound.

            // If it's above the threshold for being heard, play its onHeard event.
            //Debug.Log($"{listener.rootEntity} heard {sound}, volume = {heardDecibels}dB, on frame {Time.frameCount}");
            listener.onSoundHeard.Invoke(new HeardSound(sound, heardDecibels, source, source.CentreOfMass, Time.time));
        }
    }

    bool CheckIfListenerCanHearSound(DiegeticSound sound, Entity source, float multiplier, out float heardVolume)
    {
        float debugDuration = 1;
        Color successColor = Color.green;
        Color failColor = Color.red;
        bool allowDebug = source.ShouldDebug() && rootEntity.ShouldDebug();


        heardVolume = 0;

        // Ignore own sounds
        if (source == rootEntity) return false;
        // Ignore sounds coming from friendly characters
        if (rootEntity.IsHostileTowards(source) == false) return false;



        // TO DO: should I just use distance, or keep relying on the inverse square stuff?

        float maxVolume = sound.decibels * multiplier;
        float maxDistance = sound.range * multiplier * sensitivityMultiplier;

        Vector3 origin = source.CentreOfMass;
        Vector3 destination = transform.position;

        // Check how far it takes the sound to travel, based on how the sound would reach the target.
        float travelDistance;
        if (AIAction.LineOfSight(origin, destination, out _, source, rootEntity, soundLayerMask))
        {
            // If sound can directly travel to target, just get that distance
            travelDistance = Vector3.Distance(origin, destination);
            if (allowDebug) Debug.DrawLine(origin, destination, successColor, debugDuration);
        }
        else
        {
            // If direct line fails, do a check for sound bouncing around corners
            bool validPath = ReverbPath(origin, destination, soundNavMeshMask, out travelDistance, allowDebug);
            if (allowDebug) AIAction.DebugDrawNavMeshPath(reverbPath, validPath ? successColor : failColor, debugDuration);

            if (!validPath)
            {
                if (allowDebug) rootEntity.DebugLog($"Checking {sound.name} from {source.name}, sound completely blocked");
                return false;
            }
        }

        // Calculate if sound could travel to enemy ears, and approximate heard volume
        // This math is probably not that accurate and will need to be improved.
        heardVolume = maxVolume * (travelDistance / maxDistance);
        bool canHear = travelDistance < maxDistance;

        if (allowDebug) rootEntity.DebugLog($"Checking {sound.name} from {source.name}, result = {canHear}\n{travelDistance}/{maxDistance}\n{heardVolume}/{maxVolume}");
        return canHear;
    }

    /// <summary>
    /// Reverb is cheated with a NavMesh path to simulate bouncing. If the path is incomplete, the sound cannot reach the target.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="travelDistance"></param>
    /// <returns></returns>
    public static bool ReverbPath(Vector3 origin, Vector3 destination, int navMeshMask, out float travelDistance, bool debug = false)
    {
        // Ensure a path asset exists
        if (reverbPath == null) reverbPath = new NavMeshPath();

        // Generate filter
        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        filter.agentTypeID = agentID;
        filter.areaMask = navMeshMask;

        // Try to calculate a path
        bool pathCalculated = NavMesh.CalculatePath(origin, destination, filter, reverbPath);
        //bool pathCalculated = NavMesh.CalculatePath(start, end, navMeshMask, reverbPath);

        // Update distance to reflect path length
        travelDistance = AIAction.NavMeshPathDistance(reverbPath);

        if (debug) Debug.Log($"{pathCalculated}, {reverbPath.status}. Length = {travelDistance}, point count = {reverbPath.corners.Length}");
        if (!pathCalculated) return false;
        if (reverbPath.status != NavMeshPathStatus.PathComplete) return false;

        return true;
    }
}
