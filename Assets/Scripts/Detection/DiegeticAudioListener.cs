using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class DiegeticAudioListener : MonoBehaviour
{
    static List<DiegeticAudioListener> active = new List<DiegeticAudioListener>();

    [SerializeField] float minVolumeToHear = 10;
    public UnityEvent<DiegeticSound, Entity, Vector3, float> onSoundHeard;

    /*
    static int soundLayerMask = ~0; // I need to change this at some point so that the sound can travel through smoke
    static int soundNavMeshMask = ~0;
    static NavMeshPath reverbPath;
    */

    private void OnEnable() => active.Add(this);
    private void OnDisable() => active.Remove(this);

    public static void DiegeticCheck(DiegeticSound sound, float decibels, Entity source)
    {
        // For the sake of simplifying the AI work, the sound 'plays' at the exact position as the entity that caused it.
        Vector3 origin = source.transform.position;
        foreach (DiegeticAudioListener listener in DiegeticAudioListener.active)
        {
            // Check if the listener can hear the sound
            if (listener.CheckIfListenerCanHearSound(origin, decibels, out float heardDecibels) == false) continue;

            // TO DO: check if the sound is not being drowned out by other sounds the entity is currently hearing

            // If it's above the threshold for being heard, play its onHeard event.
            Debug.Log($"{listener} heard {sound} from {source}, at {heardDecibels}dB, on frame {Time.frameCount}");
            listener.onSoundHeard.Invoke(sound, source, origin, heardDecibels);
        }
    }

    bool CheckIfListenerCanHearSound(Vector3 origin, float decibels, out float heardDecibels)
    {
        // For each active listener, calculate the volume at that distance.

        Vector3 destination = transform.position;

        // Check how far it takes the sound to travel
        float travelDistance = Vector3.Distance(origin, destination);
        /*
        // TO DO AT SOME POINT: add more accurate checks that simulate bouncing and hitting walls
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
        */

        // Check if the sound is loud enough for the listener to hear it. If not, cancel
        heardDecibels = decibels * MiscFunctions.InverseSquareValueMultiplier(travelDistance);
        return heardDecibels >= minVolumeToHear;
    }

}
