using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class DiegeticAudioListener : MonoBehaviour
{
    public float minVolumeToHear = 10;
    public UnityEvent<DiegeticSound, Entity, Vector3, float> onSoundHeard;

    private void OnEnable() => _active.Add(this);
    private void OnDisable() => _active.Remove(this);

    public static IReadOnlyCollection<DiegeticAudioListener> active => _active;
    static List<DiegeticAudioListener> _active;
    static int soundLayerMask = ~0; // I need to change this at some point so that the sound can travel through smoke
    static int soundNavMeshMask = ~0;
    static NavMeshPath reverbPath;

    // TO DO: make the following curve logarithmic, and figure out where the sound should max out
    public static AnimationCurve volumeCurve = AnimationCurve.Linear(0, 0, 100, 1);

    public static void PlaySound(DiegeticSound sound, Entity source, Vector3 position, float diegeticVolume)
    {
        float playerVolume = volumeCurve.Evaluate(diegeticVolume);
        sound.PlayWithoutSource(position, playerVolume);

        // Check all active listeners
        //Vector3 origin = transform.position;
        foreach (DiegeticAudioListener listener in active)
        {
            CheckIfListenerCanHearSound(listener, sound, source, diegeticVolume);
        }
    }
    static void CheckIfListenerCanHearSound(DiegeticAudioListener listener, DiegeticSound sound, Entity source, float volume)
    {
        Vector3 origin = source.transform.position; // For the sake of simplifying the AI work, the sound 'plays' at the exact position as the entity that caused it.
        Vector3 destination = listener.transform.position;

        // Calculate a distance/clear path check
        float travelDistance = Vector3.Distance(origin, destination);
        if (Physics.Raycast(origin, destination, out _, travelDistance, soundLayerMask))
        {
            // If not, check if the sound bounces around corners.
            // Reverb is cheated with a NavMesh path to simulate bouncing.
            // If the path is incomplete, the sound cannot reach the target
            NavMesh.CalculatePath(origin, destination, soundNavMeshMask, reverbPath);
            if (reverbPath.status != NavMeshPathStatus.PathComplete) return;
            // Update distance to reflect path corners
            travelDistance = AIAction.NavMeshPathDistance(reverbPath);
        }

        // Check if the sound is loud enough for the listener to hear it. If not, cancel
        float volumeAtLocation = MiscFunctions.InverseSquareValue(volume, travelDistance);
        if (volumeAtLocation < listener.minVolumeToHear) return;
        // Invoke the event
        listener.onSoundHeard.Invoke(sound, source, origin, volumeAtLocation);
    }
}
