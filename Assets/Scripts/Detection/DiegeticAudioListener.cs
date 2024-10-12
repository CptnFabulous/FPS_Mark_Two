using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] float minVolumeToHear = 1;
    [SerializeField] LayerMask soundLayerMask = ~0;
    [SerializeField] int soundNavMeshMask = NavMesh.AllAreas;
    public UnityEvent<HeardSound> onSoundHeard;

    Entity _root;
    static NavMeshPath rp = null;

    public Entity rootEntity => _root ??= GetComponentInParent<Entity>();
    static NavMeshPath reverbPath => rp ??= new NavMeshPath(); // A singleton NavMeshPath re-used to save memory when calculating reverb


    private void OnEnable() => active.Add(this);
    private void OnDisable() => active.Remove(this);

    public static void DiegeticCheck(DiegeticSound sound, float decibels, Entity source)
    {
        // For the sake of simplifying the AI work, the sound 'plays' at the exact position as the entity that caused it.
        foreach (DiegeticAudioListener listener in active)
        {
            // Check if the listener can hear the sound
            if (listener.CheckIfListenerCanHearSound(source, decibels, out float heardDecibels) == false) continue;

            // TO DO POSSIBLY: check if the sound is not being drowned out by other sounds the entity is currently hearing
            // Add each heard sound to a list, and remove all entries whose times are long enough ago for the sound to expire.
            // Then only count a sound if the incoming volume is greater than or equal to the loudest sound.

            // If it's above the threshold for being heard, play its onHeard event.
            //Debug.Log($"{listener.rootEntity} heard {sound}, volume = {heardDecibels}dB, on frame {Time.frameCount}");
            listener.onSoundHeard.Invoke(new HeardSound(sound, heardDecibels, source, source.CentreOfMass, Time.time));
        }
    }

    bool CheckIfListenerCanHearSound(Entity source, float decibels, out float heardDecibels)
    {
        heardDecibels = 0;

        // Ignore own sounds
        if (source == rootEntity) return false;
        // Ignore sounds coming from friendly characters
        if (rootEntity.IsHostileTowards(source) == false) return false;

        // Check how far it takes the sound to travel, based on how the sound would reach the target.
        // First, check if it's a clear line of sight to the target.
        float travelDistance;
        Vector3 origin = source.CentreOfMass;
        Vector3 destination = transform.position;
        if (AIAction.LineOfSight(origin, destination, soundLayerMask, source.colliders, rootEntity.colliders))
        {
            // Clear shot between the source and destination!
            travelDistance = Vector3.Distance(origin, destination);
        }
        else
        {
            // Not a straight shot, check if the sound bounces around corners.
            // Reverb is cheated with a NavMesh path to simulate bouncing.
            // If the path is incomplete, the sound cannot reach the target

            // TO DO POSSIBLY IF I FEEL IT'S NECESSARY: under whatever criteria, get the hit objects from the line of sight check and reduce the sound volume based on the types of objects hit

            // Sample start and end positions on NavMesh, and cancel if start and end points can't be found
            float maxDistance = 10;
            bool originSampleCheck = NavMesh.SamplePosition(origin, out NavMeshHit pathStart, maxDistance, soundNavMeshMask);
            bool destinationSampleCheck = NavMesh.SamplePosition(destination, out NavMeshHit pathEnd, maxDistance, soundNavMeshMask);
            if (originSampleCheck == false || destinationSampleCheck == false) return false;
            // Calculate path, cancel if it can't be completed
            NavMesh.CalculatePath(pathStart.position, pathEnd.position, soundNavMeshMask, reverbPath);
            if (reverbPath.status != NavMeshPathStatus.PathComplete) return false;

            // Update distance to reflect path corners (plus distance from real to sampled ends)
            float originToStart = Vector3.Distance(origin, pathStart.position);
            float endToEars = Vector3.Distance(pathEnd.position, destination);
            travelDistance = originToStart + AIAction.NavMeshPathDistance(reverbPath) + endToEars;
        }

        // Check if the sound is loud enough for the listener to hear it. If not, cancel
        heardDecibels = decibels * MiscFunctions.InverseSquareValueMultiplier(travelDistance);
        return heardDecibels >= minVolumeToHear;
    }
}
