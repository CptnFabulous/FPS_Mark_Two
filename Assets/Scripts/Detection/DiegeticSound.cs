using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Diegetic Sound", menuName = "ScriptableObjects/Diegetic Sound", order = 1)]
public class DiegeticSound : ScriptableObject
{
    /// <summary>
    /// A private struct used just to check how recently a sound played in the same location.
    /// </summary>
    struct PlayedSound
    {
        public Vector3 checkOriginPoint;
        public float time;

        public PlayedSound(Vector3 point, float time)
        {
            this.checkOriginPoint = point;
            this.time = time;
        }
    }

    [SerializeField] AudioClip[] sounds;
    //[SerializeField] AudioMixerGroup mixerGroup;
    [SerializeField] float decibels = 80;
    [SerializeField, Range(-3, 3)] float minPitchVariance = 1;
    [SerializeField, Range(-3, 3)] float maxPitchVariance = 1;
    /*
    [SerializeField, Range(0, 1)] float minVolumeVariance = 1;
    [SerializeField, Range(0, 1)] float maxVolumeVariance = 1;
    [SerializeField] float delay;
    */

    HashSet<PlayedSound> recentSoundChecks = new HashSet<PlayedSound>();
    static readonly float distanceThreshold = 1f;
    static readonly float ageThreshold = 0.2f;

    public void Play(Entity entity) => Play(entity.bounds.center, entity);
    public void Play(AudioSource source) => Play(source.transform.position, source.GetComponentInParent<Entity>(), source);
    public void Play(Vector3 point, Entity sourceEntity, float multiplier = 1, bool playAudioAtMaxVolume = false)
    {
        AudioSource source = null;
        if (sourceEntity != null)
        {
            source = sourceEntity.audioSource;
        }
        Play(point, sourceEntity, source, multiplier, playAudioAtMaxVolume);
    }
    public void Play(Vector3 point, Entity sourceEntity, AudioSource source, float multiplier = 1, bool playAudioAtMaxVolume = false)
    {
        AudioClip clip = sounds[Random.Range(0, sounds.Length)];
        float volumeForPlayer = /*Random.Range(minVolumeVariance, maxVolumeVariance) * */multiplier;
        volumeForPlayer = playAudioAtMaxVolume ? 1 : volumeForPlayer;
        if (source != null)
        {
            source.pitch = Random.Range(minPitchVariance, maxPitchVariance);
            source.volume = volumeForPlayer;
            source.PlayOneShot(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, point, volumeForPlayer);
        }

        // TO DO: play text in subtitles, if close enough for the player to hear

        // Play diegetic code (assuming there's an entity to check against)
        if (sourceEntity == null) return;

        #region Check if an AI check was done for this sound recently near this point. If so, don't do an AI check until that one has expired.

        // Remove all sounds played before the age threshold.
        // If a sound is still in, then that always means it's new enough that other checks shouldn't be run near that position.
        // This acts as an age check and keeps the hashset length small, at the same time.
        float oldestAcceptableTime = Time.unscaledTime - ageThreshold;
        recentSoundChecks.RemoveWhere((ps) => ps.time < oldestAcceptableTime);

        // If a recent instance of this sound was played near this point, don't do an AI hearing check.
        Vector3 checkOriginPoint = sourceEntity.CentreOfMass;
        foreach (PlayedSound sound in recentSoundChecks)
        {
            float distance = Vector3.Distance(sound.checkOriginPoint, checkOriginPoint);
            if (distance < distanceThreshold) return;
        }

        // Ensure this sound check's position is logged in the hashset.
        recentSoundChecks.Add(new PlayedSound(checkOriginPoint, Time.unscaledTime));

        #endregion

        // Do the actual check
        DiegeticAudioListener.DiegeticCheck(this, decibels * multiplier, sourceEntity);
    }
}
